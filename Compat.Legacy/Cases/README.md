# Cases (extension point)

This folder is intentionally empty of real cases in the reusable template. Each project that adopts
this framework adds its own feature cases here.

## Convention

* One controller/module = one cases file: `Cases/<Feature>Cases.cs`.
* Optionally, if a feature has business-specific (non-generic) scenarios: `Cases/<Feature>/<Feature>Scenarios.cs`.
* One controller/module = one grouped snapshot file: `snapshots/<Feature>.approved.json` (created
  automatically the first time you run legacy generation - don't hand-write it).

## How to add your first cases file

1. Copy `ExampleCases.template.cs` in this folder to `<Feature>Cases.cs` (e.g. `WidgetsCases.cs`) and
   rename `ExampleCases`/`ExampleScenarios` to match. It's excluded from compilation as a `.template.cs`
   file (see `Compat.Legacy.csproj`'s `<Compile Remove>`), so renaming it is what activates it.
2. Fill in one `CompatibilityCase` per risk, setting `Feature`, and `Key` via
   `CompatibilityCaseKey.For("<Action>", CompatibilityScenarios.X)` - reuse a constant from
   `Compat.Core/Scenarios/CompatibilityScenarios.cs` whenever the risk is a generic migration
   binding/serialization concern (number-as-string, missing field, form-urlencoded, ...).
3. Wire it into `../LegacySnapshotGenerationTests.cs`:
   * `BuildHost()` - add one `.Register("<Feature>", api => new <Feature>Controller(api, ...))` line.
   * `Cases()` - add `foreach (var c in Compat.Legacy.Cases.<Feature>Cases.All()) yield return new object[] { c };`
   * Remove the `Skip = NoCasesRegisteredReason` from `Generate_legacy_snapshot` once at least one
     case is registered.
4. Run legacy generation (see `compat-tests/README.md`) and review/commit the generated
   `snapshots/<Feature>.approved.json`.

## When NOT to add a new global scenario

If your risk is a generic MVC5 -> ASP.NET Core binding/serialization concern that could recur on any
controller, check `CompatibilityScenarios` first - don't invent a new string for something already
covered there. Only add a `<Feature>Scenarios.cs` for genuinely feature-specific business rules that
no other controller would ever hit.
