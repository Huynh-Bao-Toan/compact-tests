using System.Collections.Generic;
using Compat.Core;

namespace Compat.Legacy.Cases
{
    /// <summary>
    /// SAMPLE ONLY - excluded from compilation (see Compat.Legacy.csproj's &lt;Compile Remove&gt;
    /// for "Cases/**/*.template.cs"). To activate: copy this file to "&lt;Feature&gt;Cases.cs" (e.g.
    /// "WidgetsCases.cs") in this folder, rename ExampleCases below to match, replace
    /// "Widgets"/"WidgetsController"/"/Widgets/..." with your real controller, then wire it into
    /// ../LegacySnapshotGenerationTests.cs (see Cases/README.md).
    /// </summary>
    public static class ExampleCases
    {
        public static IEnumerable<CompatibilityCase> All()
        {
            // RISK: number sent as string ("WidgetID":"10")
            // Legacy MVC coerces "10"->10. .NET 8 [FromBody]+STJ rejects unless AllowReadingFromString.
            yield return new CompatibilityCase
            {
                Feature = "Widgets",
                Key = CompatibilityCaseKey.For("AddProducts", CompatibilityScenarios.NumberAsString),
                Method = "POST",
                Url = "/Widgets/AddProducts",
                ContentType = "application/json",
                Body = @"{ ""WidgetID"": ""10"", ""listModel"": [] }"
            };

            // RISK: form-urlencoded request (NOT json)
            yield return new CompatibilityCase
            {
                Feature = "Widgets",
                Key = CompatibilityCaseKey.For("Search", CompatibilityScenarios.FormUrlEncoded),
                Method = "POST",
                Url = "/Widgets/Search",
                ContentType = "application/x-www-form-urlencoded",
                Body = "page=1&rows=20&keyword=widget"
            };
        }
    }
}
