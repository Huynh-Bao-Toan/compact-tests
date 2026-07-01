using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Compat.Core
{
    /// <summary>
    /// Turns a <see cref="CompatibilitySnapshot"/> into deterministic, human-readable JSON:
    /// recursively sorts object keys (arrays preserved unless a SortArray rule applies) and
    /// applies ignore/normalize rules so unstable values never cause false diffs.
    /// </summary>
    public static class SnapshotCanonicalizer
    {
        private const string Placeholder = "<<normalized>>";

        // Default redactions applied to EVERY snapshot. These are framework-wide unstable values.
        // (Add project-wide ones here; per-case ones go on CompatibilityCase.IgnorePaths.)
        public static readonly string[] DefaultIgnorePaths =
        {
            "$..SessionID",
            "$..SessIonID",
            "$..ProcessID",
            "$..TraceId",
            "$..traceId",
            "$..RequestId",
            "$..ConcungContextID",
            "$..HTTPRequestLength"
        };

        public static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        });

        /// <summary>Project a snapshot to a canonical JObject (sorted + normalized).</summary>
        public static JObject Canonicalize(CompatibilitySnapshot snapshot, CompatibilityCase testCase)
        {
            var root = JObject.FromObject(snapshot, Serializer);

            ApplyIgnorePaths(root, DefaultIgnorePaths);
            if (testCase?.IgnorePaths != null)
                ApplyIgnorePaths(root, testCase.IgnorePaths);

            if (testCase?.NormalizeRules != null)
                foreach (var rule in testCase.NormalizeRules)
                    ApplyNormalizeRule(root, rule);

            return (JObject)Sort(root);
        }

        public static string ToText(JObject canonical) =>
            canonical.ToString(Formatting.Indented);

        private static void ApplyIgnorePaths(JObject root, IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                JToken[] hits;
                try { hits = root.SelectTokens(path, errorWhenNoMatch: false).ToArray(); }
                catch { continue; } // malformed path → skip, never throw during compare
                foreach (var hit in hits)
                    Replace(hit, new JValue(Placeholder));
            }
        }

        private static void ApplyNormalizeRule(JObject root, NormalizeRule rule)
        {
            var hits = root.SelectTokens(rule.Path, errorWhenNoMatch: false).ToArray();
            foreach (var hit in hits)
            {
                if (rule.Kind == NormalizeKind.Redact)
                {
                    Replace(hit, new JValue(Placeholder));
                }
                else if (rule.Kind == NormalizeKind.SortArray && hit is JArray arr)
                {
                    var sorted = new JArray(arr.OrderBy(t => Sort(t).ToString(Formatting.None), StringComparer.Ordinal));
                    Replace(hit, sorted);
                }
            }
        }

        private static void Replace(JToken target, JToken replacement)
        {
            if (target.Parent == null) return; // root — nothing sensible to replace
            target.Replace(replacement);
        }

        /// <summary>Recursively sort object property names. Array element order is preserved.</summary>
        public static JToken Sort(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = (JObject)token;
                    var sorted = new JObject();
                    foreach (var prop in obj.Properties().OrderBy(p => p.Name, StringComparer.Ordinal))
                        sorted.Add(prop.Name, Sort(prop.Value));
                    return sorted;
                case JTokenType.Array:
                    return new JArray(((JArray)token).Select(Sort));
                default:
                    return token.DeepClone();
            }
        }
    }

    /// <summary>Reads/writes approved snapshot files on disk.</summary>
    public sealed class SnapshotStore
    {
        private readonly string _dir;
        private static readonly object FileLock = new object();

        /// <param name="snapshotDirectory">Absolute path to the approved-snapshots folder.</param>
        public SnapshotStore(string snapshotDirectory)
        {
            _dir = snapshotDirectory;
            Directory.CreateDirectory(_dir);
        }

        public string ApprovedPath(string feature) => Path.Combine(_dir, feature + ".approved.json");
        public string ReceivedPath(string feature) => Path.Combine(_dir, feature + ".received.json");

        public bool HasApproved(string feature, string key)
        {
            lock (FileLock)
            {
                var path = ApprovedPath(feature);
                if (!File.Exists(path)) return false;

                try
                {
                    var json = File.ReadAllText(path);
                    var doc = JObject.Parse(json);
                    var cases = doc["Cases"] as JObject;
                    return cases != null && cases.ContainsKey(key);
                }
                catch
                {
                    return false;
                }
            }
        }

        public JToken ReadApproved(string feature, string key)
        {
            lock (FileLock)
            {
                var path = ApprovedPath(feature);
                if (!File.Exists(path)) return null;

                try
                {
                    var json = File.ReadAllText(path);
                    var doc = JObject.Parse(json);
                    var cases = doc["Cases"] as JObject;
                    return cases?[key];
                }
                catch
                {
                    return null;
                }
            }
        }

        public void WriteApproved(string feature, string key, JToken caseSnapshot)
        {
            lock (FileLock)
            {
                var path = ApprovedPath(feature);
                JObject doc;
                if (File.Exists(path))
                {
                    try
                    {
                        doc = JObject.Parse(File.ReadAllText(path));
                    }
                    catch
                    {
                        doc = CreateNewFeatureDoc(feature);
                    }
                }
                else
                {
                    doc = CreateNewFeatureDoc(feature);
                }

                var cases = doc["Cases"] as JObject;
                if (cases == null)
                {
                    cases = new JObject();
                    doc["Cases"] = cases;
                }

                cases[key] = caseSnapshot;

                // Sort the cases by key for determinism and cleaner diffs
                var sortedCases = new JObject();
                foreach (var prop in cases.Properties().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    sortedCases.Add(prop.Name, prop.Value);
                }
                doc["Cases"] = sortedCases;

                File.WriteAllText(path, doc.ToString(Formatting.Indented));

                // A fresh approval invalidates any stale .received file
                var rcv = ReceivedPath(feature);
                if (File.Exists(rcv)) File.Delete(rcv);
            }
        }

        public void WriteReceived(string feature, string key, JToken caseSnapshot)
        {
            lock (FileLock)
            {
                var approvedPath = ApprovedPath(feature);
                var receivedPath = ReceivedPath(feature);
                JObject doc;

                if (File.Exists(receivedPath))
                {
                    try
                    {
                        doc = JObject.Parse(File.ReadAllText(receivedPath));
                    }
                    catch
                    {
                        doc = LoadOrNewApproved(approvedPath, feature);
                    }
                }
                else
                {
                    doc = LoadOrNewApproved(approvedPath, feature);
                }

                var cases = doc["Cases"] as JObject;
                if (cases == null)
                {
                    cases = new JObject();
                    doc["Cases"] = cases;
                }

                cases[key] = caseSnapshot;

                // Sort cases by key
                var sortedCases = new JObject();
                foreach (var prop in cases.Properties().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    sortedCases.Add(prop.Name, prop.Value);
                }
                doc["Cases"] = sortedCases;

                File.WriteAllText(receivedPath, doc.ToString(Formatting.Indented));
            }
        }

        private JObject LoadOrNewApproved(string approvedPath, string feature)
        {
            if (File.Exists(approvedPath))
            {
                try
                {
                    return JObject.Parse(File.ReadAllText(approvedPath));
                }
                catch
                {
                    return CreateNewFeatureDoc(feature);
                }
            }
            return CreateNewFeatureDoc(feature);
        }

        private JObject CreateNewFeatureDoc(string feature)
        {
            return new JObject
            {
                ["Feature"] = feature,
                ["Cases"] = new JObject()
            };
        }

        // Backward compatibility support for flat snapshots
        public bool HasApproved(string name)
        {
            var c = new CompatibilityCase { Name = name };
            return HasApproved(c.Feature, c.Key);
        }

        public string ReadApproved(string name)
        {
            var c = new CompatibilityCase { Name = name };
            var token = ReadApproved(c.Feature, c.Key);
            return token != null ? SnapshotCanonicalizer.ToText((JObject)token) : null;
        }

        public void WriteApproved(string name, string text)
        {
            var c = new CompatibilityCase { Name = name };
            var token = JToken.Parse(text);
            WriteApproved(c.Feature, c.Key, token);
        }

        public void WriteReceived(string name, string text)
        {
            var c = new CompatibilityCase { Name = name };
            var token = JToken.Parse(text);
            WriteReceived(c.Feature, c.Key, token);
        }
    }
}
