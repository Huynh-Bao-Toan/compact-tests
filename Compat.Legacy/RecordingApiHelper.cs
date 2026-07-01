using System;
using System.Collections.Generic;
using Compat.Core;
using CoC.Web.SOM.Helpers;
using CoC.Web.SOM.ViewModels.ShareViewModels;
using Newtonsoft.Json.Linq;

namespace Compat.Legacy
{
    /// <summary>
    /// PROJECT-SPECIFIC ADAPTER - not part of the reusable Compat.Core/Compat.Legacy framework.
    /// Implements THIS project's concrete <see cref="IApiHelper"/> and <see cref="CodeStep"/> types
    /// from CoC.Web.SOM. If you copy this compat-tests folder into another migration project,
    /// rewrite this class against your own project's forwarding-seam interface (whatever the real
    /// controllers take as their downstream-calling dependency) - the pattern below (record instead
    /// of call, return a canned response so the controller has something non-null to read) carries
    /// over unchanged; only the interface/DTO types are project-specific.
    ///
    /// Test seam for the LEGACY app. Implements the real <see cref="IApiHelper"/> but instead of
    /// calling downstream it RECORDS each forwarded call (method/endpoint/payload) and returns a
    /// deterministic canned response. This is the narrowest possible seam — it changes no
    /// production code; the legacy app is wired to use this implementation only inside tests.
    ///
    /// The migrated .NET 8 app gets an equivalent recording seam over its own forwarding abstraction.
    /// </summary>
    public sealed class RecordingApiHelper : IApiHelper
    {
        private CodeStep _codeStep = new CodeStep();
        public readonly List<ForwardedCallSnapshot> Forwarded = new List<ForwardedCallSnapshot>();

        /// <summary>Optional per-endpoint canned downstream responses. Key = endpoint suffix match.</summary>
        public Func<string, Type, object> CannedResponse { get; set; }

        // ---- IApiHelper surface ---------------------------------------------------------------

        public CodeStep objCodeStep { get => _codeStep; set => _codeStep = value; }
        public string CacheKeyDelete { set { /* no-op for capture */ } }
        public string MediaType { set { /* no-op for capture */ } }

        public bool RemoveCache(string CacheKey) => true;
        public void LogError(Exception objEx) { /* swallow; characterized via response */ }

        public T GetMethod<T>(string ApiEndPoint, Dictionary<string, object> objPara = null)
        {
            Record("GET", ApiEndPoint, objPara);
            return Create<T>(ApiEndPoint);
        }

        public T ReadByID<T>(string ApiEndPoint, string Para) where T : new()
        {
            Record("GET", ApiEndPoint + Para, Para);
            return Create<T>(ApiEndPoint);
        }

        public T ReadAll<T>(string ApiEndPoint, string CacheKey,
                            Dictionary<string, object> param = null, bool IsReadOnlyCache = false) where T : new()
        {
            Record("GET", ApiEndPoint, param);
            return Create<T>(ApiEndPoint);
        }

        public void ReadCacheBacground<T>(string ApiEndPoint, string CacheKey, Dictionary<string, object> param = null)
            => Record("GET", ApiEndPoint, param);

        public T1 PostMethod<T1, T2>(string ApiEndPoint, T2 objPara, string CacheKey = "")
        {
            Record("POST", ApiEndPoint, objPara, CacheKey);
            return Create<T1>(ApiEndPoint);
        }

        public T1 PutMethod<T1, T2>(string ApiEndPoint, T2 reqObj, string CacheKey = "") where T2 : new()
        {
            Record("PUT", ApiEndPoint, reqObj, CacheKey);
            return Create<T1>(ApiEndPoint);
        }

        public T1 DeteleMethod<T1>(string ApiEndPoint, string Para, string CacheKey = "")
        {
            Record("DELETE", ApiEndPoint + Para, Para, CacheKey);
            return Create<T1>(ApiEndPoint);
        }

        public void Dispose() { }

        // ---- helpers --------------------------------------------------------------------------

        private void Record(string method, string endpoint, object payload, string cacheKey = null)
        {
            Forwarded.Add(new ForwardedCallSnapshot
            {
                Method   = method,
                Endpoint = endpoint,
                Payload  = payload == null ? null : JToken.FromObject(payload, SnapshotCanonicalizer.Serializer),
                CacheKey = string.IsNullOrEmpty(cacheKey) ? null : cacheKey
            });
        }

        private T Create<T>(string endpoint)
        {
            var canned = CannedResponse?.Invoke(endpoint, typeof(T));
            if (canned is T typed) return typed;
            try { return Activator.CreateInstance<T>(); }   // gives wrapper a non-null object to read
            catch { return default; }
        }
    }
}
