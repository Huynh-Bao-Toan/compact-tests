using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using CoC.Web.SOM;

namespace Compat.Legacy
{
    /// <summary>
    /// PROJECT-SPECIFIC ADAPTER - not part of the reusable Compat.Core/Compat.Legacy framework.
    /// This class reflects into THIS project's concrete <see cref="CoC.Web.SOM.AppConfig"/> type
    /// (its exact private field names: "_dicConfigs", "lazy"). If you copy this compat-tests folder
    /// into another migration project, delete or rewrite this file against your own project's
    /// AppConfig-equivalent singleton - only do so if you hit the same "static field init needs
    /// HttpRuntime.AppDomainAppPath in a host-less test process" problem described below.
    ///
    /// xUnit class fixture that pre-seeds AppConfig.Instance before any legacy test runs.
    ///
    /// Problem: AppConfig.Instance uses a Lazy&lt;AppConfig&gt;. The constructor calls
    ///   XElement.Load(HttpRuntime.AppDomainAppPath + "\\appSetting.config")
    /// which throws because HttpRuntime.AppDomainAppPath is null in a host-less test process.
    ///
    /// Effect without this fixture: ApiEndPoint's static field initializers
    ///   (e.g. SOMAPIBaseUrl = AppConfig.Instance.ApiUrl) throw TypeInitializationException
    ///   the first time they run. The controller's catch(Exception) swallows the exception
    ///   and returns an error response without ever calling IApiHelper.PostMethod — so
    ///   ForwardedCalls is always empty and snapshots have no forwarded call data.
    ///
    /// Fix: replace the lazy singleton with a pre-seeded, uninitialized AppConfig instance:
    ///   • All instance fields default to null / false / 0 (no production code changed).
    ///   • _APIDebug = false → CodeStep.SuccessStep/ErrorStep/ErrorMessage/DataSend return
    ///     "" or null (same as a non-debug production environment) — no sentinel needed.
    ///   • _ApiUrl = null → ApiEndPoint.SOMAPIBaseUrl = null → ApiEndPoint.SupplierResource =
    ///     "api/Suppliers/" (null + string = string) — normalizes correctly in snapshots.
    ///
    /// This is entirely test infrastructure. No production classes are modified.
    /// Register with: [Collection("LegacyCompat")] on every test class that invokes controllers.
    /// </summary>
    public sealed class AppConfigFixture : IDisposable
    {
        public AppConfigFixture() => SeedAppConfig();

        public void Dispose() { }

        private static void SeedAppConfig()
        {
            var type = typeof(AppConfig);

            // (A) Set the static _dicConfigs to an empty dictionary.
            //     Prevents NullReferenceException if any static AppConfig member (SeqURI etc.)
            //     is accessed. Static members that index into _dicConfigs will throw
            //     KeyNotFoundException — those are not called in the pilot controller actions.
            var dicConfigsProp = type.GetProperty(
                "_dicConfigs", BindingFlags.NonPublic | BindingFlags.Static);
            dicConfigsProp?.SetValue(null, new Dictionary<string, string>());

            // (B) Create an AppConfig instance without calling its constructor.
            //     All instance fields stay at CLR defaults (null / false / 0).
            //     The important consequence:
            //       _APIDebug  = false  → CodeStep debug getters return "" (no AppConfig call crash)
            //       _ApiUrl    = null   → ApiEndPoint.SOMAPIBaseUrl = null (harmless; see above)
            var instance = (AppConfig)FormatterServices.GetUninitializedObject(type);

            // (C) Replace the private readonly Lazy<AppConfig> singleton so that
            //     AppConfig.Instance returns our pre-seeded instance.
            //     .NET Framework 4.8 allows writing to readonly static fields via reflection
            //     (this restriction was introduced in .NET 5+; not applicable here on net48).
            var lazyField = type.GetField(
                "lazy", BindingFlags.NonPublic | BindingFlags.Static);
            lazyField?.SetValue(null, new Lazy<AppConfig>(() => instance));
        }
    }

    /// <summary>Marks tests that require AppConfigFixture.</summary>
    [Xunit.CollectionDefinition("LegacyCompat")]
    public sealed class LegacyCompatCollection : Xunit.ICollectionFixture<AppConfigFixture> { }
}
