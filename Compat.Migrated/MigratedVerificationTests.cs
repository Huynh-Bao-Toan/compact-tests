using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Compat.Core;
using Xunit;

namespace Compat.Migrated
{
    /// <summary>
    /// MIGRATED MODE: runs each registered case against the .NET 8 app and compares to the approved
    /// (legacy) golden snapshot. A non-empty diff = a behavior change that must be reviewed.
    ///
    /// Run (once unblocked):  dotnet test compat-tests/Compat.Migrated
    /// Re-baseline on purpose: COMPAT_UPDATE=1 dotnet test compat-tests/Compat.Migrated
    ///
    /// Cases here should mirror the same feature's Compat.Legacy/Cases/&lt;Feature&gt;Cases.cs 1:1
    /// (same Feature/Key/request data) so legacy and migrated verify IDENTICAL inputs against the
    /// same approved snapshot. See compat-tests/Compat.Migrated/Cases/README.md.
    /// </summary>
    public class MigratedVerificationTests
    {
        private const string SkipReason =
            "BLOCKED + no cases registered: (1) migrated som-web (.NET 8) project is not yet " +
            "materialized - implement MigratedCompatibilityHost first; (2) no project-specific cases " +
            "are mirrored into Cases() below yet. Once both are done, remove this Skip. " +
            "See compat-tests/Compat.Migrated/Cases/README.md.";

        private static string SnapshotDir =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "snapshots"));

        /// <summary>
        /// EXTENSION POINT: mirror each case from your Compat.Legacy/Cases/&lt;Feature&gt;Cases.cs here
        /// (same Feature/Key/request data), e.g.:
        ///   yield return Case("Widgets", CompatibilityCaseKey.For("AddProducts", CompatibilityScenarios.NumberAsString),
        ///       "POST", "/Widgets/AddProducts", "application/json", @"{ ""WidgetID"": ""10"" }");
        /// </summary>
        public static IEnumerable<object[]> Cases()
        {
            yield break;
        }

        private static object[] Case(string feature, string key, string method, string url, string ct, string body) =>
            new object[] { new CompatibilityCase { Feature = feature, Key = key, Method = method, Url = url, ContentType = ct, Body = body } };

        [Theory(Skip = SkipReason)]
        [MemberData(nameof(Cases))]
        public async Task Migrated_matches_legacy_snapshot(CompatibilityCase testCase)
        {
            var store = new SnapshotStore(SnapshotDir);
            var runner = new CompatibilityRunner(store, migrated: new MigratedCompatibilityHost());

            var comparison = await runner.RunMigratedAndCompareAsync(testCase);

            Assert.True(comparison.IsMatch,
                $"Compatibility diff for '{testCase.Feature}/{testCase.Key}':\n{comparison.Diff}");
        }
    }
}
