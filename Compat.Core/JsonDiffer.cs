using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Compat.Core
{
    public enum DiffKind { Added, Removed, Changed }

    public sealed class DiffEntry
    {
        public string Path { get; set; }
        public DiffKind Kind { get; set; }
        public string Expected { get; set; }   // legacy / approved
        public string Actual { get; set; }     // migrated / received
    }

    public sealed class DiffResult
    {
        public List<DiffEntry> Entries { get; } = new List<DiffEntry>();
        public bool IsEmpty => Entries.Count == 0;

        public override string ToString()
        {
            if (IsEmpty) return "(no differences)";
            var sb = new StringBuilder();
            sb.AppendLine($"{Entries.Count} difference(s):  (- legacy/approved   + migrated/received)");
            foreach (var e in Entries)
            {
                switch (e.Kind)
                {
                    case DiffKind.Added:
                        sb.AppendLine($"  + [{e.Path}] only in migrated: {e.Actual}");
                        break;
                    case DiffKind.Removed:
                        sb.AppendLine($"  - [{e.Path}] missing in migrated (legacy had): {e.Expected}");
                        break;
                    case DiffKind.Changed:
                        sb.AppendLine($"  ~ [{e.Path}]");
                        sb.AppendLine($"      - {e.Expected}");
                        sb.AppendLine($"      + {e.Actual}");
                        break;
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>Deep, path-aware diff over two canonical JTokens. Order-stable (objects are pre-sorted).</summary>
    public static class JsonDiffer
    {
        public static DiffResult Diff(JToken expected, JToken actual)
        {
            var result = new DiffResult();
            Walk("$", expected, actual, result);
            return result;
        }

        private static void Walk(string path, JToken expected, JToken actual, DiffResult result)
        {
            if (expected == null && actual == null) return;

            if (expected == null)
            {
                result.Entries.Add(new DiffEntry { Path = path, Kind = DiffKind.Added, Actual = Render(actual) });
                return;
            }
            if (actual == null)
            {
                result.Entries.Add(new DiffEntry { Path = path, Kind = DiffKind.Removed, Expected = Render(expected) });
                return;
            }

            if (expected.Type != actual.Type)
            {
                result.Entries.Add(new DiffEntry
                {
                    Path = path, Kind = DiffKind.Changed,
                    Expected = Render(expected), Actual = Render(actual)
                });
                return;
            }

            switch (expected.Type)
            {
                case JTokenType.Object:
                    var eo = (JObject)expected;
                    var ao = (JObject)actual;
                    var names = eo.Properties().Select(p => p.Name)
                        .Union(ao.Properties().Select(p => p.Name))
                        .OrderBy(n => n, System.StringComparer.Ordinal);
                    foreach (var n in names)
                        Walk(path + "." + n, eo[n], ao[n], result);
                    break;

                case JTokenType.Array:
                    var ea = (JArray)expected;
                    var aa = (JArray)actual;
                    int max = ea.Count > aa.Count ? ea.Count : aa.Count;
                    for (int i = 0; i < max; i++)
                        Walk($"{path}[{i}]",
                            i < ea.Count ? ea[i] : null,
                            i < aa.Count ? aa[i] : null,
                            result);
                    break;

                default:
                    if (!JToken.DeepEquals(expected, actual))
                        result.Entries.Add(new DiffEntry
                        {
                            Path = path, Kind = DiffKind.Changed,
                            Expected = Render(expected), Actual = Render(actual)
                        });
                    break;
            }
        }

        private static string Render(JToken t)
        {
            var s = t.ToString(Newtonsoft.Json.Formatting.None);
            return s.Length > 400 ? s.Substring(0, 400) + "…" : s;
        }
    }
}
