# Cases (extension point)

Compat.Migrated does not have its own case-authoring files. Its `MigratedVerificationTests.Cases()`
mirrors the same feature's `Compat.Legacy/Cases/<Feature>Cases.cs` 1:1 (same `Feature`, `Key`, and
request data) so the legacy and migrated hosts verify IDENTICAL inputs against the same approved
snapshot. This folder exists to document that convention and give it a stable place to live once the
two sides can share a single source file (see the TODO in `MigratedVerificationTests.cs`).

## How to add a case here

1. Add/confirm the case exists in `Compat.Legacy/Cases/<Feature>Cases.cs` first - legacy is always
   the source of truth and generates the approved snapshot.
2. Mirror it into `../MigratedVerificationTests.cs`'s `Cases()`, using the exact same
   `CompatibilityCaseKey.For(...)` / `CompatibilityScenarios.X` combination as the legacy case, so the
   `Feature`/`Key` match and the comparison resolves to the same `snapshots/<Feature>.approved.json`
   entry.
3. Once `MigratedCompatibilityHost` is implemented (see its own doc comment - blocked until the
   migrated .NET 8 project is referenced) and at least one case is mirrored, remove the `Skip` from
   `Migrated_matches_legacy_snapshot`.

## Why not a shared file?

`Compat.Legacy` targets net48 and `Compat.Migrated` targets net8.0; a single case-provider class
can't cross-reference `CoC.Web.SOM.Controllers` (net48-only) from the net8 side. When the migrated
project is unblocked, prefer linking the request-data-only parts of a shared `*.cs` file via a
`<Compile Include>` in both `.csproj`s so cases live in exactly one place - do that refactor then,
not before, since the mirroring shape may need to change once the migrated host's shape is known.
