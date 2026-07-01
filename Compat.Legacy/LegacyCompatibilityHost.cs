using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Compat.Core;
using Newtonsoft.Json.Linq;

namespace Compat.Legacy
{
    /// <summary>
    /// LEGACY (.NET Framework 4.8 / MVC5) compatibility host.
    ///
    /// Runs a request host-less but through the REAL MVC5 model-binding pipeline:
    ///   * JSON bodies bind via the real <see cref="JsonValueProviderFactory"/>.
    ///   * form-urlencoded + query bind via <see cref="NameValueCollectionValueProvider"/>
    ///     (the same providers the Form/QueryString factories build), with the same cultures.
    ///   * the action METHOD is invoked directly (auth/result filters are intentionally skipped —
    ///     we are characterizing binding + forwarding + response, not the auth pipeline).
    ///
    /// Controllers are registered with a one-line factory that injects the <see cref="RecordingApiHelper"/>.
    /// Adding a new controller = one registration line; no framework changes.
    /// </summary>
    public sealed class LegacyCompatibilityHost : ICompatibilityHost
    {
        public string Name => "legacy-net48";

        /// <summary>
        /// Reduces a forwarded endpoint to a base-URL-independent form. Migrations commonly remap
        /// downstream base URLs (e.g. LegacyApiBaseUrl -> ApiBaseUrl), so both hosts normalize to the
        /// relative "api/..." path and only the meaningful suffix is compared. Override if needed.
        /// </summary>
        public Func<string, string> NormalizeEndpoint { get; set; } = DefaultNormalizeEndpoint;

        public static string DefaultNormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint)) return endpoint;
            int i = endpoint.IndexOf("api/", StringComparison.OrdinalIgnoreCase);
            return i >= 0 ? endpoint.Substring(i) : endpoint;
        }

        private readonly Dictionary<string, Func<RecordingApiHelper, Controller>> _controllers =
            new Dictionary<string, Func<RecordingApiHelper, Controller>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Register a controller factory, e.g. Register("Widgets", api => new WidgetsController(api, ...)).</summary>
        public LegacyCompatibilityHost Register(string controllerName, Func<RecordingApiHelper, Controller> factory)
        {
            _controllers[controllerName] = factory;
            return this;
        }

        public Task<CompatibilitySnapshot> ExecuteAsync(CompatibilityCase testCase)
            => Task.FromResult(Execute(testCase));

        private CompatibilitySnapshot Execute(CompatibilityCase testCase)
        {
            var (controllerName, actionName, query) = ParseUrl(testCase);
            if (!_controllers.TryGetValue(controllerName, out var factory))
                throw new InvalidOperationException(
                    $"Controller '{controllerName}' is not registered with the LegacyCompatibilityHost.");

            var api = new RecordingApiHelper();
            var controller = factory(api);

            // Build fake request/context.
            var form = ParseUrlEncoded(testCase);
            byte[] body = string.IsNullOrEmpty(testCase.Body) ? new byte[0] : Encoding.UTF8.GetBytes(testCase.Body);
            var headers = new NameValueCollection();
            if (testCase.Headers != null)
                foreach (var kv in testCase.Headers) headers[kv.Key] = kv.Value;

            var request = new FakeHttpRequest(testCase.Method, testCase.ContentType, body, query, form, headers);
            var httpContext = new FakeHttpContext(request);
            var controllerContext = new ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);
            controller.ControllerContext = controllerContext;

            // Resolve action + bind parameters through the real MVC binder.
            var method = ResolveAction(controller.GetType(), actionName);
            var valueProvider = BuildValueProvider(controllerContext, testCase, query, form);
            var args = BindParameters(controllerContext, method, valueProvider);

            // Invoke directly (filters skipped on purpose) and capture the result.
            var snapshot = new CompatibilitySnapshot
            {
                RequestMethod = testCase.Method,
                RequestUrl = testCase.Url,
                RequestContentType = testCase.ContentType,
                StatusCode = 200,
                ResponseContentType = "application/json",
                ForwardedCalls = api.Forwarded
            };

            // --- invoke action -------------------------------------------------------------------
            // Two failure modes are intentionally separated:
            //   (a) TargetInvocationException — the action itself threw; this IS characterizable
            //       legacy behavior, record as a 500-shaped result.
            //   (b) CaptureResult failure — a framework/serialization issue in our test code;
            //       ForwardedCalls (the primary migration artifact) are already safe by this point
            //       so we record the capture error in the snapshot rather than losing the run.
            ActionResult actionResult = null;
            bool actionThrew = false;

            try
            {
                actionResult = method.Invoke(controller, args) as ActionResult;
            }
            // (endpoint normalization applied below regardless of outcome)
            catch (TargetInvocationException tie)
            {
                actionThrew = true;
                var ex = tie.InnerException ?? tie;
                snapshot.StatusCode = 500;
                snapshot.Response = new JObject
                {
                    ["__exception"] = ex.GetType().Name,
                    ["__message"]   = ex.Message
                };
            }

            if (!actionThrew)
            {
                try
                {
                    CaptureResult(actionResult, httpContext, snapshot);
                }
                catch (Exception captureEx)
                {
                    // SafeSnapshotSerializer should prevent this, but defend anyway.
                    // ForwardedCalls are already captured and must not be discarded.
                    snapshot.Response = new JObject
                    {
                        ["__captureError"]   = captureEx.GetType().Name,
                        ["__captureMessage"] = captureEx.Message
                    };
                }
            }

            if (NormalizeEndpoint != null)
                foreach (var f in snapshot.ForwardedCalls)
                    f.Endpoint = NormalizeEndpoint(f.Endpoint);

            return snapshot;
        }

        // ---- result capture -----------------------------------------------------------------

        private static void CaptureResult(ActionResult actionResult, HttpContextBase ctx, CompatibilitySnapshot snap)
        {
            switch (actionResult)
            {
                case JsonResult json:
                    snap.StatusCode = ctx.Response.StatusCode;
                    // Use SafeSnapshotSerializer (not the stock Newtonsoft serializer) so that:
                    //   • [ScriptIgnore] properties are excluded (mirrors the real MVC5 wire shape).
                    //   • Getters that throw (e.g. CodeStep.SuccessStep → AppConfig.Instance →
                    //     HttpRuntime.AppDomainAppPath == null in host-less tests) are replaced
                    //     with a sentinel rather than crashing the snapshot run.
                    snap.Response = SafeSnapshotSerializer.TrySerialize(json.Data);
                    break;
                case HttpStatusCodeResult status:
                    snap.StatusCode = status.StatusCode;
                    snap.Response = string.IsNullOrEmpty(status.StatusDescription)
                        ? JValue.CreateNull() : new JValue(status.StatusDescription);
                    break;
                case ContentResult content:
                    snap.StatusCode = ctx.Response.StatusCode;
                    snap.ResponseContentType = content.ContentType ?? "text/plain";
                    snap.ResponseRaw = content.Content;
                    break;
                case null:
                    snap.StatusCode = ctx.Response.StatusCode;
                    snap.Response = JValue.CreateNull();
                    break;
                default:
                    snap.StatusCode = ctx.Response.StatusCode;
                    snap.Response = new JObject { ["__resultType"] = actionResult.GetType().Name };
                    break;
            }
        }

        // ---- model binding ------------------------------------------------------------------

        private static IValueProvider BuildValueProvider(ControllerContext ctx, CompatibilityCase testCase,
                                                         NameValueCollection query, NameValueCollection form)
        {
            var collection = new ValueProviderCollection();
            var ct = testCase.ContentType ?? string.Empty;

            // Body provider (matches MVC factory ordering: body before query).
            if (ct.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            {
                var json = new JsonValueProviderFactory().GetValueProvider(ctx);
                if (json != null) collection.Add(json);
            }
            else if (ct.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
                  || ct.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                // FormValueProviderFactory uses CurrentCulture.
                collection.Add(new NameValueCollectionValueProvider(form, CultureInfo.CurrentCulture));
            }

            // QueryStringValueProviderFactory uses InvariantCulture.
            collection.Add(new NameValueCollectionValueProvider(query, CultureInfo.InvariantCulture));
            return collection;
        }

        private static object[] BindParameters(ControllerContext ctx, MethodInfo method, IValueProvider valueProvider)
        {
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                var metadata = ModelMetadataProviders.Current.GetMetadataForType(null, p.ParameterType);
                var bindingContext = new ModelBindingContext
                {
                    ModelName = p.Name,
                    ModelMetadata = metadata,
                    ValueProvider = valueProvider,
                    FallbackToEmptyPrefix = !valueProvider.ContainsPrefix(p.Name)
                };
                var binder = ModelBinders.Binders.GetBinder(p.ParameterType);
                var bound = binder.BindModel(ctx, bindingContext);

                if (bound == null)
                {
                    if (p.HasDefaultValue) bound = p.DefaultValue;
                    else if (p.ParameterType.IsValueType) bound = Activator.CreateInstance(p.ParameterType);
                }
                args[i] = bound;
            }
            return args;
        }

        private static MethodInfo ResolveAction(Type controllerType, string actionName)
        {
            // Prefer a public instance method matching the action name; ignore overloads' verb attrs here.
            var candidates = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase)
                            && typeof(ActionResult).IsAssignableFrom(m.ReturnType))
                .ToArray();

            if (candidates.Length == 0)
                throw new InvalidOperationException($"Action '{actionName}' not found on {controllerType.Name}.");
            if (candidates.Length > 1)
                throw new InvalidOperationException(
                    $"Action '{actionName}' is overloaded on {controllerType.Name}; register a disambiguating case.");
            return candidates[0];
        }

        // ---- url / body parsing -------------------------------------------------------------

        private static (string controller, string action, NameValueCollection query) ParseUrl(CompatibilityCase c)
        {
            var url = c.Url ?? "";
            string path = url, qs = "";
            int q = url.IndexOf('?');
            if (q >= 0) { path = url.Substring(0, q); qs = url.Substring(q + 1); }

            var segments = path.Trim('/').Split('/');
            if (segments.Length < 2)
                throw new InvalidOperationException($"Url '{url}' must be /Controller/Action.");
            var controller = segments[0];
            var action = segments[1];

            var query = HttpUtility.ParseQueryString(qs);
            if (c.Query != null)
                foreach (var kv in c.Query) query.Add(kv.Key, kv.Value);
            return (controller, action, query);
        }

        private static NameValueCollection ParseUrlEncoded(CompatibilityCase c)
        {
            var form = new NameValueCollection();
            var ct = c.ContentType ?? "";
            if (!ct.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)) return form;
            if (string.IsNullOrEmpty(c.Body)) return form;
            return HttpUtility.ParseQueryString(c.Body);
        }
    }
}
