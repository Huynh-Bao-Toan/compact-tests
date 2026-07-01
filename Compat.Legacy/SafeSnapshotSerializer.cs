using System;
using System.Reflection;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Compat.Legacy
{
    /// <summary>
    /// Test-only serializer for capturing the legacy MVC5 response shape safely.
    ///
    /// Two problems with using the stock Newtonsoft serializer on MVC5 JsonResult.Data:
    ///   1. Computed getters (e.g. CodeStep.SuccessStep) may call AppConfig.Instance, which requires
    ///      HttpRuntime.AppDomainAppPath — null in a host-less test process — causing crashes.
    ///   2. [ScriptIgnore] is honoured by MVC5's JavaScriptSerializer on the real wire but is
    ///      invisible to Newtonsoft, so the captured shape would include properties the frontend
    ///      never receives, producing false diffs.
    ///
    /// This serializer fixes both:
    ///   • ScriptIgnoreAwareContractResolver skips [ScriptIgnore] members (mirrors real wire shape).
    ///   • SafeValueProvider wraps every getter: if a getter throws, the property value is replaced
    ///     by the sentinel "<<unavailable:getter-threw>>" rather than crashing serialization.
    ///
    /// No production classes are modified. This is entirely test infrastructure.
    /// </summary>
    public static class SafeSnapshotSerializer
    {
        /// <summary>Sentinel value written for any getter that throws during snapshot capture.</summary>
        public const string UnavailableSentinel = "<<unavailable:getter-threw>>";

        public static readonly JsonSerializer Instance = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateFormatHandling    = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling  = DateTimeZoneHandling.Utc,
            NullValueHandling     = NullValueHandling.Include,
            Formatting            = Formatting.Indented,
            ContractResolver      = new ScriptIgnoreAwareContractResolver(),
            // Belt-and-suspenders: if SafeValueProvider somehow lets an exception through,
            // mark it handled so the whole snapshot is not discarded.
            Error = (_, args) => args.ErrorContext.Handled = true
        });

        /// <summary>
        /// Serializes <paramref name="obj"/> to a JToken using the safe contract resolver.
        /// If the top-level serialization fails despite error handling, returns an error object
        /// rather than propagating the exception — forwarded calls are preserved regardless.
        /// </summary>
        public static JToken TrySerialize(object obj)
        {
            if (obj == null) return JValue.CreateNull();
            try
            {
                return JToken.FromObject(obj, Instance);
            }
            catch (Exception ex)
            {
                return new JObject
                {
                    ["__serializationError"] = ex.GetType().Name,
                    ["__message"]            = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// Contract resolver that:
    ///   (a) Skips properties decorated with [ScriptIgnore], mirroring MVC5's JavaScriptSerializer.
    ///   (b) Wraps every property getter in a safe try/catch, replacing thrown exceptions with
    ///       <see cref="SafeSnapshotSerializer.UnavailableSentinel"/>.
    /// Generic — works for any controller response type, not just CodeStep.
    /// </summary>
    internal sealed class ScriptIgnoreAwareContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            // Honor [ScriptIgnore] so the snapshot shape matches the real MVC5 wire format.
            if (member.GetCustomAttribute<ScriptIgnoreAttribute>() != null)
            {
                prop.ShouldSerialize = _ => false;
                return prop;
            }

            // Wrap the value getter so hosting-dependent getters degrade to a sentinel
            // instead of propagating ArgumentNullException / other infrastructure exceptions.
            if (prop.Readable && prop.ValueProvider != null)
                prop.ValueProvider = new SafeValueProvider(prop.ValueProvider);

            return prop;
        }
    }

    /// <summary>
    /// Delegates to the real value provider; catches any exception from the getter and
    /// returns <see cref="SafeSnapshotSerializer.UnavailableSentinel"/> instead.
    /// </summary>
    internal sealed class SafeValueProvider : IValueProvider
    {
        private readonly IValueProvider _inner;

        public SafeValueProvider(IValueProvider inner) => _inner = inner;

        public object GetValue(object target)
        {
            try   { return _inner.GetValue(target); }
            catch { return SafeSnapshotSerializer.UnavailableSentinel; }
        }

        public void SetValue(object target, object value) => _inner.SetValue(target, value);
    }
}
