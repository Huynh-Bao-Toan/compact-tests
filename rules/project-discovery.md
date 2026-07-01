# Project discovery

Before generating any compatibility case, establish where things live in the **target repository**
(the repo this `compat-tests` folder has been copied into). Never assume paths from any other project
you've seen before - discover them fresh, every time.

## Steps

1. **Check for an existing local config first.**
   Look for `compat-tests/compat-project.config.json`. If it exists, read it and use its values
   instead of re-discovering everything from scratch - but spot-check 1-2 fields against the repo
   (e.g. does `controllerRoot` actually contain `.cs` controller files?) before trusting it blindly;
   repos drift.

2. **Locate the legacy project file.**
   Search for a `.csproj` targeting an old .NET Framework TFM (e.g. `net48`, `net472`) that contains
   MVC5 controllers. Record its name and relative path
   (`legacyProjectName` / `legacyProjectPath`).

3. **Locate the migrated project file, if it exists yet.**
   Search for a `.csproj` targeting `net8.0`/`net9.0`+ that looks like the ASP.NET Core successor
   (similar controller/route names, or explicitly referenced from migration docs/commits). It's normal
   for this not to exist yet - record `migratedProjectPath` as empty/absent and note it as a blocker,
   don't invent a path.

4. **Locate controllers.**
   Find the directory holding MVC5 controllers (`*Controller.cs` deriving from `Controller` or
   `ApiController`). Record it as `controllerRoot`. Note if controllers are split across multiple
   areas/folders - `controllerRoot` may need to be the common ancestor, or you may need one config
   entry per area.

5. **Locate AngularJS (or other frontend) scripts.**
   Find the directory holding the frontend JS for the feature(s) in scope - look for
   `angular.module`, `.controller(`, `.service(`, or `.factory(` calls, and for API-call patterns
   (see `controller-js-mapping.md`). Record it as `scriptRoot`.

6. **Locate or infer the forwarding abstraction.**
   Find the interface/class the controllers use to call the downstream API (commonly named something
   like `IApiHelper`, `IApiClient`, `IApiService`). Identify:
   - the interface name and its implementation's name,
   - its method names for GET/POST/PUT/DELETE (they rarely match those verbs exactly - record what's
     actually there).
   Record these as `forwardingSeam.interfaceName` / `implementationName` / `knownMethods`.
   If a response wrapper type is used consistently across controllers (e.g. a `Status`/`Message`/`Data`
   envelope), record its property names under `responseWrapper` - leave fields empty if there's no
   consistent wrapper.

7. **Identify test project names/paths.**
   Confirm (or create, following this template) `Compat.Core` / `Compat.Legacy` / `Compat.Migrated`
   project locations relative to the target repo root, and how `dotnet test` should be invoked from
   the repo root.

8. **Fill a local `compat-project.config.json`.**
   Copy `compat-project.config.template.json` to `compat-tests/compat-project.config.json` and fill
   in every field you discovered. Leave a field's placeholder in place (don't guess) if you couldn't
   confirm it with evidence - flag it in your coverage report instead (see `coverage-reporting.md`).

## What NOT to do

* **Never commit project-specific values back into `compat-project.config.template.json` or
  `compat-project.config.example.json`.** Those two files are the reusable template's shared,
  project-independent artifacts - only `compat-project.config.json` (the local, per-repo copy) may
  contain real project names/paths.
* Don't invent a migrated project path if none exists - report it as a blocker instead.
* Don't assume the forwarding seam's method names match another project's (e.g. don't assume
  `DeleteMethod` is spelled that way here - check the actual interface).
* Whether `compat-project.config.json` itself gets committed to the target repo's git history is that
  project's decision, not this template's - see the root `README.md`'s authoring-kit section and the
  `.gitignore` comment near it.
