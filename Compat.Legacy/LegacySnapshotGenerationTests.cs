using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Compat.Core;
using Xunit;

namespace Compat.Legacy
{
    /// <summary>
    /// LEGACY MODE: runs each registered case against the .NET Framework 4.8 app and writes the
    /// approved golden snapshot under compat-tests/snapshots/. Re-running refreshes the approved
    /// snapshots.
    ///
    /// Run:  dotnet test compat-tests/Compat.Legacy
    ///
    /// AppConfigFixture pre-seeds AppConfig.Instance so that ApiEndPoint static field initializers
    /// succeed (they read AppConfig.Instance.*Url which would otherwise throw due to
    /// HttpRuntime.AppDomainAppPath being null in a host-less test process).
    ///
    /// THIS FILE IS THE PROJECT'S EXTENSION POINT (see compat-tests/Compat.Legacy/Cases/README.md):
    /// out of the box there are no registered controllers/cases, so the theory below is Skip-annotated
    /// and `dotnet test` reports it as Skipped (not failed, not silently absent). Wire up your own
    /// controllers/cases and delete the Skip to activate real snapshot generation.
    /// </summary>
    [Collection("LegacyCompat")]
    public class LegacySnapshotGenerationTests
    {
        private const string NoCasesRegisteredReason =
            "No project-specific cases are registered yet. Add a Cases/<Feature>Cases.cs file, " +
            "register its controller in BuildHost() and its cases in Cases() below, then remove " +
            "this Skip. See compat-tests/Compat.Legacy/Cases/README.md.";

        private static string SnapshotDir =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "snapshots"));

        /// <summary>
        /// EXTENSION POINT: register one controller factory per line, e.g.:
        ///   .Register("Widgets", api => new WidgetsController(api, /* other deps: null unless the action uses them */))
        /// Non-IApiHelper deps can usually be null unless the exercised action actually uses them.
        /// </summary>
        private static LegacyCompatibilityHost BuildHost()
        {
            return new LegacyCompatibilityHost();
        }

        /// <summary>
        /// EXTENSION POINT: aggregate every project cases file's All() here, e.g.:
        ///   foreach (var c in Compat.Legacy.Cases.WidgetsCases.All()) yield return new object[] { c };
        /// </summary>
        public static IEnumerable<object[]> Cases()
        {
            yield break;
        }

        [Theory(Skip = NoCasesRegisteredReason)]
        [MemberData(nameof(Cases))]
        public async Task Generate_legacy_snapshot(CompatibilityCase testCase)
        {
            var store = new SnapshotStore(SnapshotDir);
            var runner = new CompatibilityRunner(store, legacy: BuildHost());

            var text = await runner.GenerateLegacySnapshotAsync(testCase);

            Assert.False(string.IsNullOrWhiteSpace(text));
            Assert.True(store.HasApproved(testCase.Feature, testCase.Key),
                $"Approved snapshot not written for feature '{testCase.Feature}', key '{testCase.Key}'");
        }
    }
}
