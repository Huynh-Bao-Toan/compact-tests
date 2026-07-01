# Coverage reporting

After generating cases for a feature/module, report back in this shape so a human reviewer (and any
agent picking up the work later) can see exactly what was and wasn't covered, and why. Keep it
concise - a table plus short notes, not prose per action.

## Required sections

### 1. Actions found

Every action method discovered on the controller(s) in scope, regardless of whether a case was
generated for it.

### 2. Actions covered

Which of those actions got at least one `CompatibilityCase`. Reference the case key(s).

### 3. Actions skipped, with reason

Every found-but-uncovered action, each with an explicit reason - pick from (or state your own):

* no frontend call site found in `scriptRoot` (dead/unused action, or JS lives elsewhere - say which
  you suspect),
* action requires auth/session state this framework's Tier 1 host doesn't provide (see the root
  `README.md`'s Tier 1/Tier 2 section) - candidate for Tier 2, not skipped forever,
  * action has no interesting binding risk (e.g. a no-parameter GET) - covered implicitly, not worth a
  dedicated case,
* out of scope for this pass - explicitly deferred.

### 4. Scenarios generated

List each case's `Feature/Key`, one line each, with a one-phrase justification pointing back to the
evidence (e.g. "JS sends `OrderType` as a template-bound string - see `WidgetsCases.cs`").

### 5. Scenarios recommended but not generated

Risks you identified as plausible (from `CompatibilityScenarios`, or feature-specific) but didn't
turn into a case - split into:

* **evidence exists, not yet authored** (e.g. ran out of scope for this pass),
* **synthetic/no current evidence** (per `scenario-authoring.md` rule 4) - state the risk and why it's
  still worth tracking even without current JS evidence.

### 6. Unresolved ambiguities

Anything you couldn't confirm with evidence and didn't want to guess at, e.g.:

* a field's actual runtime type is unclear because it flows through several JS variables before the
  call site,
* two call sites disagree about content type for what looks like the same action,
* `compat-project.config.json` is missing/incomplete for a field you needed (see
  `project-discovery.md`).

### 7. Confidence level

One of **High / Medium / Low** for the feature/module as a whole, plus a one-line reason:

* **High** - every generated case traces to a clear JS call site and controller code path you read in
  full.
* **Medium** - most cases are evidence-backed, but some rely on partial evidence (e.g. a wrapper
  service's default behavior wasn't fully traced) or a synthetic variant was added.
* **Low** - config was incomplete, controller/JS mapping was ambiguous in multiple places, or you had
  to guess at more than one non-trivial detail.

## What NOT to do

* Don't omit the "skipped" and "unresolved ambiguities" sections just because they're empty - state
  "none" explicitly so a reviewer knows you checked.
* Don't report confidence as High if any case in the batch is synthetic (per rule 4 in
  `scenario-authoring.md`) - cap it at Medium and say why.
