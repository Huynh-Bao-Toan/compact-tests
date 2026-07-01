# Compatibility Testing Framework (Golden Master / Snapshot Regression)

A reusable **Golden Master / Snapshot** harness for characterizing an ASP.NET MVC5 / .NET Framework
4.8 web app's request-handling behavior and verifying that a migrated ASP.NET Core / .NET 8 app
behaves identically. **The legacy app is the source of truth.** It generates approved snapshots; the
migrated app is verified against them. This is characterization testing, not unit testing.

This folder ships as a **template with no project-specific feature cases in it** - see
"Copying this template into another project" below. `dotnet test` on a freshly-copied instance
reports Skipped, not failed or silently absent (see "What to expect with no cases registered").

## What is captured per case

For each `CompatibilityCase` (request data only), a `CompatibilitySnapshot` records:

- incoming request: method, url, content-type
- **frontend result**: HTTP status code + response JSON shape/body
- **downstream forwarded calls** (captured at the app's forwarding seam): method, endpoint, payload, query, headers

Both sides normalize forwarded endpoints to their base-URL-independent `api/...` form, because
migrations commonly remap downstream base URLs.

## Projects

| Project | TFM | Role |
|---|---|---|
| `Compat.Core` | netstandard2.0 | Framework: models, canonical JSON, normalization, diff, runner, scenario taxonomy. Fully generic - shared by both sides, no project-specific code. |
| `Compat.Legacy` | net48 | **Legacy snapshot generator.** Runs requests through the REAL MVC5 model binder host-less, records forwards via a recording seam. Mostly generic; two files are project-specific adapters (see below). |
| `Compat.Migrated` | net8.0 | **Verifier.** Runs requests against the migrated app (WebApplicationFactory) and diffs vs approved snapshots. **Currently a scaffold — blocked** until a migrated .NET 8 project exists. |

```
compat-tests/
  Compat.Core/
    CompatibilityModels, SnapshotEngine, JsonDiffer, CompatibilityRunner
    Scenarios/CompatibilityScenarios, Scenarios/CompatibilityCaseKey
  Compat.Legacy/
    LegacyFakes, LegacyCompatibilityHost, SafeSnapshotSerializer       (generic - reuse as-is)
    RecordingApiHelper, AppConfigFixture                              (project-specific adapters - rewrite per project)
    LegacySnapshotGenerationTests                                     (extension point - register your controllers/cases here)
    Cases/
      README.md                  how to add your first cases file
      ExampleCases.template.cs   sample only, excluded from compilation
  Compat.Migrated/
    MigratedCompatibilityHost (scaffold), MigratedVerificationTests (extension point, currently Skipped)
    Cases/
      README.md                  mirroring convention (Legacy is the source of truth)
  snapshots/
    .gitkeep                     no <Feature>.approved.json ships in the template
```

## Copying this template into another migration project

1. Copy the whole `compat-tests/` folder into the target repo.
2. In `Compat.Legacy/Compat.Legacy.csproj`, repoint `<Reference Include="..."><HintPath>` at the
   target's own legacy web app DLL (the bundled state points at this repo's own legacy web app DLL -
   treat that as a worked example, not a value to keep).
3. Rewrite the two project-specific adapter files against the target app's own types (each has a
   banner comment explaining exactly what to change):
   - `Compat.Legacy/RecordingApiHelper.cs` - implements the target's own forwarding-seam interface
     (whatever the real controllers take as their downstream-calling dependency), recording calls
     instead of making them.
   - `Compat.Legacy/AppConfigFixture.cs` - only needed if the target app has a similar
     "static singleton needs web-hosting infra to initialize" problem; otherwise delete it and the
     `[Collection("LegacyCompat")]` attribute in `LegacySnapshotGenerationTests.cs`.
4. Everything else (`Compat.Core`, `LegacyCompatibilityHost.cs`, `LegacyFakes.cs`,
   `SafeSnapshotSerializer.cs`, `MigratedCompatibilityHost.cs`) is generic MVC5/ASP.NET Core plumbing
   and needs no changes.
5. Add your first feature cases file - see `Compat.Legacy/Cases/README.md`, or use the
   Compatibility Scenario Authoring Kit below to generate it from your controller + frontend JS.

## Compatibility Scenario Authoring Kit

A standardized rule kit so an agent (or a human) can generate accurate compatibility cases
consistently when given only a controller file, its frontend JS, and this target repo - without
guessing project-specific facts and without leaking any project's real name/paths back into the
reusable template.

| Artifact | Purpose |
|---|---|
| `compat-project.config.template.json` | Placeholder-only schema for a local, per-repo config. Copy to `compat-project.config.json` and fill in. |
| `compat-project.config.example.json` | A fully-filled, fictional ("Contoso") example showing the shape of a real config - not real values. |
| `rules/project-discovery.md` | How to find the legacy/migrated projects, controllers, frontend JS, and forwarding seam in an unfamiliar target repo. |
| `rules/controller-js-mapping.md` | How to map AngularJS/JS call sites to MVC actions and extract real content-type/payload evidence. |
| `rules/scenario-authoring.md` | How evidence becomes `CompatibilityCase`s: no invented scenarios, synthetic variants must be labeled, verify what reaches the forwarding seam. |
| `rules/naming-conventions.md` | Feature/case-file/snapshot-file/case-key/scenario-key naming, and the global-vs-feature-specific scenario decision. |
| `rules/coverage-reporting.md` | The required shape for reporting actions found/covered/skipped, scenarios generated vs. recommended, ambiguities, and confidence. |
| `prompts/generate-feature-scenarios.prompt.md` | The master prompt tying all of the above together for a single feature/module generation pass. |

### How to configure it for a new project

1. Copy `compat-project.config.template.json` to `compat-project.config.json` in this folder.
2. Follow `rules/project-discovery.md` to fill in every field with real, evidence-based values.
3. Never write real project names/paths into `compat-project.config.template.json`,
   `compat-project.config.example.json`, or anything under `rules/`/`prompts/` - those are the shared,
   reusable-template files. Real values live only in `compat-project.config.json` and in the
   `Cases/<Feature>Cases.cs` files you add.

### How agents should use it

Follow `prompts/generate-feature-scenarios.prompt.md` when asked to generate cases for a
feature/module. It sequences the rule files above into required steps and mandates a coverage report
at the end - it will refuse (and tell you so) rather than invent cases when it lacks JS/controller
evidence.

### How humans should review generated scenarios

* Check the coverage report's "confidence level" and "unresolved ambiguities" first - Low confidence
  or open ambiguities mean the cases need a closer read before merging.
* Any case commented `// SYNTHETIC (no current JS evidence): ...` (see `rules/scenario-authoring.md`
  rule 4) should get explicit sign-off - it's testing a shape the frontend doesn't currently send.
* Review the generated `snapshots/<Feature>.approved.json` diff the same way you'd review any other
  generated-but-consequential artifact: read what changed, don't just accept because it's generated.

### Commands

```powershell
# Generate legacy snapshots (source of truth)
dotnet test compat-tests\Compat.Legacy --no-restore

# Verify migrated snapshots (once Compat.Migrated is unblocked)
dotnet test compat-tests\Compat.Migrated --no-restore
```

### Warning

Project-specific names, paths, and assembly references must live in `compat-project.config.json` and
in your own `Cases/<Feature>Cases.cs` files - never in `Compat.Core`, `rules/`, `prompts/`, or the two
`compat-project.config.*.json` template/example files. Those are what makes this folder copyable into
the next migration project without a find-and-replace pass.

## Where snapshots are stored

Approved snapshots are stored grouped by controller/module (feature) under:
`compat-tests/snapshots/<Feature>.approved.json` — check these into git.

On a mismatch the verifier writes `<Feature>.received.json` next to it for inspection (gitignore the `.received.json`).

Inside each snapshot file, multiple case snapshots are stored in a nested dictionary by case key:
```json
{
  "Feature": "Widgets",
  "Cases": {
    "AddProducts/number_as_string": { ... },
    "Search/form_urlencoded": { ... }
  }
}
```

### Trade-off
Grouping snapshots into a single file per controller/module results in **fewer files** to track in Git, making the workspace cleaner. However, it may result in **larger diffs** when multiple cases within the same controller/module change at once.

## How to add a new feature/module cases file

See `Compat.Legacy/Cases/README.md` for the full walkthrough. In short:

* One controller/module = one cases file: `Compat.Legacy/Cases/<Feature>Cases.cs`.
* One controller/module = one grouped snapshot file: `snapshots/<Feature>.approved.json` (generated,
  never hand-written).
* Set `Key` via `CompatibilityCaseKey.For("<Action>", CompatibilityScenarios.X)` - see "Scenario
  taxonomy" below for what `X` should be.
* Wire the new cases file into `Compat.Legacy/LegacySnapshotGenerationTests.cs` (`BuildHost()` +
  `Cases()`) and remove its `Skip` once at least one case is registered.
* Mirror the same cases into `Compat.Migrated/MigratedVerificationTests.cs` when you're ready to
  verify the migrated side - see `Compat.Migrated/Cases/README.md`.

Unstable values (timestamps, ids, session/trace) are auto-redacted; add case-specific ones via
`IgnorePaths` (JSONPath, e.g. `"$..Token"`) or `NormalizeRules` (redact / sort arrays).

## Scenario taxonomy

A **scenario** is the second segment of a case `Key` (`"<Action>/<scenario>"`) - it names the
specific binding/serialization risk a case exercises, independent of which controller/action it's
attached to (e.g. `number_as_string`, `form_urlencoded`).

Scenario names should not be plain string literals typed inline in each cases file - strings drift,
typos silently create a "new" scenario instead of reusing an existing one, and there is no single
place to see "which scenario risks do we cover, and where."
`Compat.Core/Scenarios/CompatibilityScenarios.cs` and `CompatibilityCaseKey.cs` fix this without
changing the `Key` format, snapshot files, or any runtime behavior - they only change how the string
is authored.

### Global vs. feature-specific scenarios

| Kind | Lives in | Represents |
|---|---|---|
| **Global migration scenario** | `Compat.Core/Scenarios/CompatibilityScenarios.cs` | A generic MVC5 -> ASP.NET Core / .NET 8 binding or serialization risk that can recur on **any** controller (type coercion, empty/missing values, request encoding, casing, etc). |
| **Feature-specific scenario** | `Compat.Legacy/Cases/<Feature>/<Feature>Scenarios.cs` (create only if needed) | A business rule specific to one controller/module that is not a generic binding risk. |

Use a global scenario whenever the case is really just "the frontend sent shape X, does the migrated
binder still accept it." Only add a feature-specific scenarios file once a controller actually has a
case that isn't a reusable binding risk; don't create the file speculatively.

### Naming convention

`snake_case`, describing the **input shape/condition**, not the expected outcome:
good: `number_as_string`; avoid: `number_as_string_should_coerce`.

### How to add a new global scenario

1. Add one `public const string` to `CompatibilityScenarios` (alphabetically grouped by category,
   matching the existing layout).
2. Reuse it from any cases file via `CompatibilityCaseKey.For(action, CompatibilityScenarios.X)`.
3. Do **not** add a new global scenario if an existing one already describes the same risk under a
   different label - search `CompatibilityScenarios` first. Do **not** add a global scenario for a
   one-off business rule that only one controller will ever hit - that belongs in a feature-specific
   scenarios file instead.

### Using scenarios in a cases file

```csharp
yield return new CompatibilityCase
{
    Feature = "Widgets",
    Key = CompatibilityCaseKey.For("AddProducts", CompatibilityScenarios.NumberAsString),
    Method = "POST",
    Url = "/Widgets/AddProducts",
    ContentType = "application/json",
    Body = @"{ ""WidgetID"": ""10"", ""listModel"": [] }"
};
```

## How to run — legacy snapshot generation (source of truth)

Prerequisites: build the legacy web app project first (so its own dependencies resolve) and make its
`AppConfig`-equivalent configuration available to the test process if you kept an `AppConfigFixture`-
style adapter. Then:

```powershell
dotnet test compat-tests\Compat.Legacy --no-restore
```

This writes/refreshes every `<Feature>.approved.json` for every case registered in
`LegacySnapshotGenerationTests.Cases()`. Review the diff in git before committing — that review is
the moment you accept "this is the legacy behavior we must preserve."

## How to run — .NET 8 verification (once unblocked)

```powershell
dotnet test compat-tests\Compat.Migrated --no-restore                 # verify; fails on any diff
$env:COMPAT_UPDATE=1; dotnet test compat-tests\Compat.Migrated --no-restore # deliberately re-baseline (rare)
```

A failing test prints an exact path-level diff (see below). A diff is a **migration incompatibility to
review** — fix it with the narrowest endpoint/DTO-specific change (e.g. a per-DTO converter), never a
broad global JSON leniency setting, unless a snapshot proves the broad change is required and safe.

## What to expect with no cases registered

In the template's shipped state, `Compat.Legacy/LegacySnapshotGenerationTests.Cases()` and
`Compat.Migrated/MigratedVerificationTests.Cases()` both yield nothing, and their theories carry a
`Skip` explaining why (`NoCasesRegisteredReason` / `SkipReason`). Running `dotnet test` on either
project in this state reports something like:

```
Skipped! - Failed: 0, Passed: 0, Skipped: 1, Total: 1
```

This is expected and not a failure - it means no project-specific cases have been registered yet.
Once you add at least one case (see "How to add a new feature/module cases file") and remove the
`Skip`, the theory runs for real against your registered cases.

## How to add project-specific fixtures if needed

`Compat.Legacy/AppConfigFixture.cs` is a worked example of the pattern: some legacy apps have a
static singleton (an `AppConfig`-style class) whose constructor throws outside of real IIS hosting
(commonly because it reads `HttpRuntime.AppDomainAppPath`). If your target app has the same problem,
write an equivalent fixture that pre-seeds the singleton via reflection into an uninitialized
instance, and register it the same way: `[CollectionDefinition("...")] : ICollectionFixture<...>` +
`[Collection("...")]` on your test class. If your app doesn't have this problem, delete
`AppConfigFixture.cs` and the `[Collection("LegacyCompat")]` attribute entirely - it is not required
by the rest of the framework.

## Keeping production code unchanged

Nothing in this framework should require changes to the app under test:

- `LegacyCompatibilityHost` invokes real controller actions directly via `MethodInfo.Invoke`,
  bypassing auth/result filters rather than requiring the app to expose a test-only code path.
- `RecordingApiHelper` (or your project's equivalent) is a test-only implementation of the app's
  existing forwarding-seam interface, swapped in only inside tests - the app's real implementation of
  that interface is never touched.
- `AppConfigFixture` (if you need one) replaces a `Lazy<T>` singleton via reflection instead of
  editing the singleton's source.

If a change you're making to unblock a case requires editing the app under test, stop and reconsider
the case - the framework is characterization-only.

## Example approved snapshot (illustrative)

```json
{
  "ForwardedCalls": [
    {
      "Endpoint": "api/Widgets/AddWidgetProducts",
      "Headers": null,
      "Method": "POST",
      "Payload": [ { "IsActived": true, "ProductID": 0, "WidgetID": 10, "WidgetOrderTypeID": 3 } ],
      "Query": null
    }
  ],
  "RequestContentType": "application/json",
  "RequestMethod": "POST",
  "RequestUrl": "/Widgets/AddProducts",
  "Response": { "objCodeStep": { "Message": "Added successfully.", "Status": 1 } },
  "StatusCode": 200
}
```

## Example diff output (migrated vs legacy)

```
Compatibility diff for 'Widgets/AddProducts_number_as_string':
3 difference(s):  (- legacy/approved   + migrated/received)
  ~ [$.StatusCode]
      - 200
      + 400
  - [$.ForwardedCalls[0]] missing in migrated (legacy had): {"Endpoint":"api/Widgets/AddWidgetProducts",...}
  ~ [$.Response.objCodeStep.Status]
      - 1
      + 3
```

Reading: legacy coerced `"3"` → forwarded the call and returned success; .NET 8 `[FromBody]`+STJ rejected
the string number with **400** and never forwarded. Fix: add `NumberHandling=AllowReadingFromString` to that
DTO/endpoint only, re-run, expect a clean diff.

## Tier 1 (current) vs Tier 2 (future)

### Tier 1 — host-less MVC5 with safe response capture (default)

All current tests run in Tier 1:

| What is covered | How |
|---|---|
| MVC5 model binding (JSON, form-urlencoded, query) | Real `JsonValueProviderFactory` + `NameValueCollectionValueProvider` |
| Controller action logic / DTO transformation | Direct `MethodInfo.Invoke` — no auth/result filters |
| Downstream forwarded calls (endpoint, method, payload, cache key) | `RecordingApiHelper` seam at the app's forwarding interface |
| Frontend response shape (Status, Message, Data, `[ScriptIgnore]` exclusions) | `SafeSnapshotSerializer` — see below |

**Why full IIS hosting is not required for most controller tests:** The compatibility risk is in _binding_ (how frontend-sent request data is coerced into C# parameters) and _forwarding_ (what the controller builds and sends downstream). Both are exercised host-lessly. Auth/session infrastructure is intentionally skipped — characterization, not end-to-end testing.

**Why response capture is safe-shape based:** MVC5 serializes `JsonResult.Data` using `JavaScriptSerializer`, which honors `[ScriptIgnore]`. Some response-model getters may also depend on app-config singletons that require real web hosting to initialize. `SafeSnapshotSerializer` solves both:
- `ScriptIgnoreAwareContractResolver` skips `[ScriptIgnore]` properties so the snapshot shape matches the real wire.
- `SafeValueProvider` wraps every getter — if it throws (e.g. hosting-dependent getters), the value is replaced with `"<<unavailable:getter-threw>>"` instead of crashing the run.

**Known Tier 1 limitation — headers below the forwarding seam are not captured.** Auth tokens, user/session identifiers, and similar values are typically attached below the recording seam (inside the real forwarding implementation's HTTP client setup). These headers are not captured at Tier 1. They are also authentication-infrastructure, not business-payload — their normalization during migration should be tracked separately.

### Tier 2 — real host integration tests (not yet implemented)

Use Tier 2 only when Tier 1 cannot characterize the behavior, e.g.:

- File downloads / streaming responses
- Redirect chains
- Cookie management
- Authentication pipeline behavior
- Full exact response serialization (including config-gated fields)
- Middleware-level behavior

Tier 2 would run the full ASP.NET Core `WebApplicationFactory<Program>` (migrated) or a full IIS Express / TestServer (legacy) and capture the raw HTTP response. No current template cases require Tier 2.

## Known limitations / blockers

- **Migrated `.NET 8` project not materialized.** `Compat.Migrated` is a scaffold; tests are
  `Skip`-ped. Unblock: reference the migrated project, implement `MigratedCompatibilityHost`
  (`WebApplicationFactory<Program>` + a recording seam over the migrated app's forwarding
  abstraction), remove the `Skip`.
- **No cases registered.** This is intentional in the template state (see "What to expect with no
  cases registered") - add your own via `Compat.Legacy/Cases/README.md`.
- **Legacy runtime config.** `Compat.Legacy` needs the legacy app's DLLs (+ `AppConfig`-equivalent
  settings at runtime, if you kept that fixture). Endpoint base URLs are normalized away, so missing
  base URLs are tolerated as long as static init does not throw.
