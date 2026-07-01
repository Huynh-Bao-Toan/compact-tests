using System;
using System.Threading.Tasks;
using Compat.Core;

namespace Compat.Migrated
{
    /// <summary>
    /// MIGRATED (.NET 8 / ASP.NET Core) compatibility host. SCAFFOLD — blocked until som-web exists.
    ///
    /// Target implementation (fill in when the migrated project is referenced):
    ///
    ///   public sealed class MigratedCompatibilityHost
    ///       : WebApplicationFactory&lt;Program&gt;, ICompatibilityHost
    ///   {
    ///       public string Name => "migrated-net8";
    ///       public readonly RecordingApiHelperCore Recorder = new();
    ///
    ///       protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    ///           builder.ConfigureTestServices(s =>
    ///           {
    ///               s.RemoveAll&lt;som.web.Helpers.IApiHelper&gt;();
    ///               s.AddScoped&lt;som.web.Helpers.IApiHelper&gt;(_ => Recorder); // narrow test seam
    ///           });
    ///
    ///       public async Task&lt;CompatibilitySnapshot&gt; ExecuteAsync(CompatibilityCase c)
    ///       {
    ///           var client = CreateClient();
    ///           var req = new HttpRequestMessage(new HttpMethod(c.Method), c.Url);
    ///           if (c.Body is not null)
    ///               req.Content = new StringContent(c.Body, Encoding.UTF8, c.ContentType.Split(';')[0]);
    ///           foreach (var (k, v) in c.Headers) req.Headers.TryAddWithoutValidation(k, v);
    ///
    ///           Recorder.Reset();
    ///           var resp = await client.SendAsync(req);
    ///           var bodyText = await resp.Content.ReadAsStringAsync();
    ///
    ///           return new CompatibilitySnapshot
    ///           {
    ///               RequestMethod = c.Method, RequestUrl = c.Url, RequestContentType = c.ContentType,
    ///               StatusCode = (int)resp.StatusCode,
    ///               ResponseContentType = resp.Content.Headers.ContentType?.ToString(),
    ///               Response = TryParse(bodyText, out var jt) ? jt : null,
    ///               ResponseRaw = jt is null ? bodyText : null,
    ///               ForwardedCalls = Recorder.Forwarded, // endpoints normalized the SAME way as legacy
    ///           };
    ///       }
    ///   }
    ///
    /// IMPORTANT: apply the SAME endpoint normalization the legacy host uses
    /// (LegacyCompatibilityHost.DefaultNormalizeEndpoint) so base-URL remapping is not a false diff.
    /// </summary>
    public sealed class MigratedCompatibilityHost : ICompatibilityHost
    {
        public string Name => "migrated-net8";

        public Task<CompatibilitySnapshot> ExecuteAsync(CompatibilityCase testCase) =>
            throw new NotImplementedException(
                "BLOCKED: migrated som-web (.NET 8) project is not available yet. " +
                "Reference it from Compat.Migrated.csproj and implement this host (see XML doc above).");
    }
}
