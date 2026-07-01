using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Compat.Core
{
    /// <summary>
    /// Orchestrates the Golden Master workflow. The LEGACY host is the source of truth.
    ///
    ///   * GenerateLegacySnapshotAsync  → run legacy, save approved snapshot.
    ///   * RunMigratedAndCompareAsync   → run migrated, compare to approved snapshot.
    ///   * RunBothAndCompareAsync       → run both live and diff directly (no file needed).
    ///
    /// Normalization (sort + ignore + normalize rules) is applied centrally here so the
    /// legacy and migrated sides are always canonicalized identically.
    /// </summary>
    public sealed class CompatibilityRunner
    {
        private readonly SnapshotStore _store;
        private readonly ICompatibilityHost _legacy;
        private readonly ICompatibilityHost _migrated;

        /// <summary>Set COMPAT_UPDATE=1 to overwrite approved snapshots during a migrated verify run.</summary>
        public bool UpdateApproved { get; set; } =
            Environment.GetEnvironmentVariable("COMPAT_UPDATE") == "1";

        public CompatibilityRunner(SnapshotStore store,
                                   ICompatibilityHost legacy = null,
                                   ICompatibilityHost migrated = null)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _legacy = legacy;
            _migrated = migrated;
        }

        /// <summary>Run the legacy app and write/refresh the approved snapshot. Returns the canonical text.</summary>
        public async Task<string> GenerateLegacySnapshotAsync(CompatibilityCase testCase)
        {
            Require(_legacy, "legacy");
            var raw = await _legacy.ExecuteAsync(testCase).ConfigureAwait(false);
            RunAssertRules(testCase, raw, "legacy");
            var canonical = SnapshotCanonicalizer.Canonicalize(raw, testCase);
            var text = SnapshotCanonicalizer.ToText(canonical);
            _store.WriteApproved(testCase.Feature, testCase.Key, canonical);
            return text;
        }

        /// <summary>Run the migrated app and compare against the approved (legacy) snapshot.</summary>
        public async Task<CompatibilityComparison> RunMigratedAndCompareAsync(CompatibilityCase testCase)
        {
            Require(_migrated, "migrated");
            var raw = await _migrated.ExecuteAsync(testCase).ConfigureAwait(false);
            RunAssertRules(testCase, raw, "migrated");
            var actual = SnapshotCanonicalizer.Canonicalize(raw, testCase);
            var actualText = SnapshotCanonicalizer.ToText(actual);

            var approvedToken = _store.ReadApproved(testCase.Feature, testCase.Key);
            if (approvedToken == null)
            {
                if (UpdateApproved)
                {
                    _store.WriteApproved(testCase.Feature, testCase.Key, actual);
                    return new CompatibilityComparison { IsMatch = true, Actual = actual, ActualText = actualText };
                }
                throw new InvalidOperationException(
                    $"No approved snapshot for feature '{testCase.Feature}', key '{testCase.Key}'. " +
                    "Generate it from the legacy app first (legacy mode).");
            }

            var expected = (JObject)approvedToken;
            var approvedText = SnapshotCanonicalizer.ToText(expected);
            var diff = JsonDiffer.Diff(expected, actual);
            var comparison = new CompatibilityComparison
            {
                IsMatch = diff.IsEmpty,
                Diff = diff,
                Expected = expected, Actual = actual,
                ExpectedText = approvedText, ActualText = actualText
            };

            if (!comparison.IsMatch)
            {
                _store.WriteReceived(testCase.Feature, testCase.Key, actual);
                if (UpdateApproved)
                {
                    _store.WriteApproved(testCase.Feature, testCase.Key, actual);
                    comparison.IsMatch = true; // explicitly re-baselined by operator
                }
            }
            return comparison;
        }

        /// <summary>Run legacy and migrated live and diff directly (both hosts must be available).</summary>
        public async Task<CompatibilityComparison> RunBothAndCompareAsync(CompatibilityCase testCase)
        {
            Require(_legacy, "legacy");
            Require(_migrated, "migrated");

            var legacyRaw = await _legacy.ExecuteAsync(testCase).ConfigureAwait(false);
            var migratedRaw = await _migrated.ExecuteAsync(testCase).ConfigureAwait(false);
            RunAssertRules(testCase, legacyRaw, "legacy");
            RunAssertRules(testCase, migratedRaw, "migrated");

            var expected = SnapshotCanonicalizer.Canonicalize(legacyRaw, testCase);
            var actual = SnapshotCanonicalizer.Canonicalize(migratedRaw, testCase);
            var diff = JsonDiffer.Diff(expected, actual);

            return new CompatibilityComparison
            {
                IsMatch = diff.IsEmpty,
                Diff = diff,
                Expected = expected, Actual = actual,
                ExpectedText = SnapshotCanonicalizer.ToText(expected),
                ActualText = SnapshotCanonicalizer.ToText(actual)
            };
        }

        private static void RunAssertRules(CompatibilityCase c, CompatibilitySnapshot s, string side)
        {
            if (c.AssertRules == null) return;
            foreach (var rule in c.AssertRules)
            {
                var failure = rule?.Invoke(s);
                if (!string.IsNullOrEmpty(failure))
                    throw new InvalidOperationException($"[{side}] AssertRule failed for '{c.Name}': {failure}");
            }
        }

        private static void Require(ICompatibilityHost host, string which)
        {
            if (host == null)
                throw new InvalidOperationException($"No {which} host was supplied to the CompatibilityRunner.");
        }
    }
}
