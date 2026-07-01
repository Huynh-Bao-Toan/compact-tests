using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compat.Core
{
    /// <summary>
    /// A single, data-driven compatibility test case. Future cases should only need
    /// to set request data (Name/Method/Url/ContentType/Body/...) — never new infrastructure.
    /// The legacy host generates the expected snapshot; no manual expected payload required.
    /// </summary>
    public sealed class CompatibilityCase
    {
        private string _feature;
        public string Feature
        {
            get
            {
                if (string.IsNullOrEmpty(_feature))
                {
                    DeriveFeatureAndKey();
                }
                return _feature;
            }
            set => _feature = value;
        }

        private string _key;
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_key))
                {
                    DeriveFeatureAndKey();
                }
                return _key;
            }
            set => _key = value;
        }

        private string _name;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_feature) && !string.IsNullOrEmpty(_key))
                {
                    return _feature + "_" + _key.Replace('/', '_');
                }
                return _name;
            }
            set => _name = value;
        }

        public string Method { get; set; } = "GET";

        /// <summary>Route + optional query string, e.g. "/Widgets/Search?page=1".</summary>
        public string Url { get; set; }

        /// <summary>e.g. "application/json", "application/x-www-form-urlencoded", "multipart/form-data".</summary>
        public string ContentType { get; set; }

        /// <summary>Raw request body exactly as AngularJS/jQuery would send it.</summary>
        public string Body { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>Extra query parameters merged into <see cref="Url"/> (optional convenience).</summary>
        public IDictionary<string, string> Query { get; set; } = new Dictionary<string, string>();

        /// <summary>Snapshot file name (without extension). Defaults to <see cref="Name"/>.</summary>
        public string SnapshotName { get; set; }

        /// <summary>JSONPath expressions (Newtonsoft SelectTokens syntax) to redact before compare,
        /// e.g. "$..SessionID", "$.Response.objCodeStep.Data.Token". Unstable values live here.</summary>
        public List<string> IgnorePaths { get; set; } = new List<string>();

        /// <summary>Per-case normalization (redact / sort arrays whose order is not business-meaningful).</summary>
        public List<NormalizeRule> NormalizeRules { get; set; } = new List<NormalizeRule>();

        /// <summary>Optional extra assertions over the produced snapshot (code, not data). Usually empty.</summary>
        public List<Func<CompatibilitySnapshot, string>> AssertRules { get; set; }
            = new List<Func<CompatibilitySnapshot, string>>();

        public string ResolvedSnapshotName => string.IsNullOrEmpty(SnapshotName) ? Name : SnapshotName;

        private void DeriveFeatureAndKey()
        {
            var caseName = string.IsNullOrEmpty(_name) ? SnapshotName : _name;
            if (string.IsNullOrEmpty(caseName))
            {
                _feature = "Unknown";
                _key = Guid.NewGuid().ToString();
                return;
            }

            // NOTE: derivation from Name/SnapshotName is a convenience fallback for callers that don't
            // set Feature/Key explicitly. It cannot know your controller's plural/irregular naming
            // (e.g. "Company" controller -> "Companies" feature) - if your naming needs that, set
            // Feature/Key explicitly on the CompatibilityCase instead of relying on this split.
            var parts = caseName.Split('_');
            if (parts.Length >= 3)
            {
                _feature = parts[0];
                _key = parts[1] + "/" + string.Join("_", parts.Skip(2));
            }
            else if (parts.Length == 2)
            {
                _feature = parts[0];
                _key = parts[1];
            }
            else
            {
                _feature = "General";
                _key = caseName;
            }
        }
    }

    public enum NormalizeKind
    {
        /// <summary>Replace the value(s) at Path with a stable placeholder.</summary>
        Redact,
        /// <summary>Sort the array at Path so ordering differences are ignored.</summary>
        SortArray
    }

    public sealed class NormalizeRule
    {
        public string Path { get; set; }
        public NormalizeKind Kind { get; set; }
        public NormalizeRule() { }
        public NormalizeRule(string path, NormalizeKind kind) { Path = path; Kind = kind; }

        public static NormalizeRule Redact(string path) => new NormalizeRule(path, NormalizeKind.Redact);
        public static NormalizeRule Sort(string path) => new NormalizeRule(path, NormalizeKind.SortArray);
    }

    /// <summary>One captured downstream call made by the wrapper to a downstream API.</summary>
    public sealed class ForwardedCallSnapshot
    {
        public string Method { get; set; }      // GET / POST / PUT / DELETE
        public string Endpoint { get; set; }    // resource url as the wrapper computed it
        public JToken Payload { get; set; }     // request body/object forwarded (deserialized shape)
        public JObject Query { get; set; }      // query params, if any
        public JObject Headers { get; set; }    // important headers, if captured
        public string CacheKey { get; set; }    // cache invalidation key passed to Post/Put/Delete (null for GET)
    }

    /// <summary>The full captured result of running one case against one host.</summary>
    public sealed class CompatibilitySnapshot
    {
        // Echo of the incoming request (helps reviewers read the snapshot file).
        public string RequestMethod { get; set; }
        public string RequestUrl { get; set; }
        public string RequestContentType { get; set; }

        // Frontend-facing result.
        public int StatusCode { get; set; }
        public string ResponseContentType { get; set; }
        public JToken Response { get; set; }          // parsed FE JSON body (shape + values)
        public string ResponseRaw { get; set; }       // non-JSON bodies (kept verbatim)

        // Downstream calls the wrapper forwarded.
        public List<ForwardedCallSnapshot> ForwardedCalls { get; set; } = new List<ForwardedCallSnapshot>();
    }

    /// <summary>Abstraction implemented by both the legacy and migrated hosts.</summary>
    public interface ICompatibilityHost
    {
        /// <summary>Human label, e.g. "legacy-net48" / "migrated-net8".</summary>
        string Name { get; }

        /// <summary>Run the request and capture a RAW (un-normalized) snapshot.</summary>
        Task<CompatibilitySnapshot> ExecuteAsync(CompatibilityCase testCase);
    }

    /// <summary>Outcome of comparing an actual snapshot against an approved/expected one.</summary>
    public sealed class CompatibilityComparison
    {
        public bool IsMatch { get; set; }
        public DiffResult Diff { get; set; }
        public JObject Expected { get; set; }   // canonical (normalized + sorted)
        public JObject Actual { get; set; }     // canonical (normalized + sorted)
        public string ExpectedText { get; set; }
        public string ActualText { get; set; }

        public override string ToString() =>
            IsMatch ? "MATCH" : ("MISMATCH\n" + (Diff?.ToString() ?? "(no diff)"));
    }
}
