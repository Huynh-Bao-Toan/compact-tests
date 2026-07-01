# Prompt: generate compatibility scenarios for one feature/module

Use this prompt when asked to produce `CompatibilityCase`s for one controller/feature, given some
combination of: controller file(s), AngularJS/frontend JS file(s), optional DTO/model files, and
optional existing snapshots. It assumes you're working inside a target repo that has this
`compat-tests` folder copied into it.

## Inputs you should expect to receive

* One or more MVC controller files (required).
* One or more frontend JS files for the same feature/module (required - without these you cannot
  satisfy the evidence rule in `rules/scenario-authoring.md`; if truly none exist, say so and stop
  rather than inventing cases from the controller signature alone).
* Optional: DTO/model files referenced by the controller's action parameters.
* Optional: an existing `snapshots/<Feature>.approved.json` (if this feature already has cases -
  you're extending coverage, not starting fresh; read it first so you don't duplicate a `Key`).

## Required steps, in order

1. **Read `../rules/project-discovery.md` and check for `compat-tests/compat-project.config.json`.**
   If present, use it. If missing or incomplete for what you need, infer the missing paths yourself
   from the repo and **propose** a filled config (don't silently skip this - state what you inferred
   and why, per `project-discovery.md`'s "what NOT to do").

2. **Read `../rules/controller-js-mapping.md` and map every JS call site to an MVC action.**
   For each mapped action, extract: URL, HTTP verb, content type, and the literal payload shape sent.

3. **Identify risky binding patterns per call site**, comparing the JS-sent shape against the
   controller action's parameter/DTO types (see `controller-js-mapping.md` step 4). Cross-reference
   against `Compat.Core/Scenarios/CompatibilityScenarios.cs` for the matching global scenario
   constant; only reach for a feature-specific scenario when `rules/naming-conventions.md`'s
   "Global vs. feature-specific scenarios" table says to.

4. **Read `../rules/scenario-authoring.md` and generate cases from evidence.**
   Each `CompatibilityCase` must trace to a specific JS call site (or be explicitly marked synthetic
   per that file's rule 4). Use `CompatibilityCaseKey.For(action, CompatibilityScenarios.X)` for the
   `Key` - never a raw string literal.

5. **Place cases under the correct file**, per `../rules/naming-conventions.md`:
   `Compat.Legacy/Cases/<Feature>Cases.cs` (start from `Compat.Legacy/Cases/ExampleCases.template.cs`
   if the file doesn't exist yet). Set `Feature` explicitly on every case.

6. **Wire the cases file into test discovery**, in `Compat.Legacy/LegacySnapshotGenerationTests.cs`:
   * add the controller registration to `BuildHost()`,
   * add the `foreach` aggregation line to `Cases()`,
   * remove the `Skip` from `Generate_legacy_snapshot` if it's still present.

7. **Generate or update the grouped snapshot** by running legacy snapshot generation (see the root
   `README.md`, "How to run — legacy snapshot generation"). Do not hand-write
   `snapshots/<Feature>.approved.json`.

8. **Mirror into `Compat.Migrated`, only if that project is unblocked** (see
   `Compat.Migrated/Cases/README.md`). If it's still a scaffold, skip this step and say so - don't
   force a mirror against a host that doesn't exist yet.

9. **Report coverage and uncertainty** using the exact structure in `../rules/coverage-reporting.md`.
   This is not optional even when coverage is complete - report "none" explicitly for empty sections.

10. **Do not modify production code.** If satisfying a case seems to require changing the controller,
    a DTO, or the frontend JS, stop and flag it in "unresolved ambiguities" instead - per the root
    `README.md`'s "Keeping production code unchanged", this framework is characterization-only.

## Guardrails (repeat of the most-violated rules)

* No case without evidence (JS call site or explicit synthetic label).
* No raw string scenario keys - always `CompatibilityCaseKey.For(...)` + a `CompatibilityScenarios`
  constant (or a feature-local scenario constant if genuinely feature-specific).
* No project-specific names written into `Compat.Core/Scenarios/CompatibilityScenarios.cs`,
  `compat-project.config.template.json`, or `compat-project.config.example.json` - those are shared,
  reusable-template files. Project-specific values belong only in the local
  `compat-tests/compat-project.config.json` and in the `Cases/<Feature>Cases.cs` file you're adding.
* No hand-edited snapshot JSON - always generated, then reviewed via git diff.

## Output format

End your response with:

1. A short summary of the feature/module and how many actions/cases were produced.
2. The coverage report (per `rules/coverage-reporting.md`).
3. The list of files added/changed.
4. Anything you propose for a human to review before this is committed (especially any synthetic
   cases, or any inferred config values from step 1).
