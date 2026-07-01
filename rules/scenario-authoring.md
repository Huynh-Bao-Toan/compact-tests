# Scenario authoring rules

These rules govern how evidence gathered via `project-discovery.md` and `controller-js-mapping.md`
turns into actual `CompatibilityCase`s. The single guiding principle: **a case must be traceable to
real evidence.** If you can't point to the JS call site or the controller code that justifies a case,
don't add it.

## 1. Generate cases from actual JS + controller evidence

Every case's `Body`/`ContentType`/`Method`/`Url` should come from a real call site you found in
`scriptRoot` (see `controller-js-mapping.md`), not from what you assume a "typical" AngularJS app
would send. Two different features can send the same logical data very differently (query string vs.
JSON body, camelCase vs PascalCase) - always check.

## 2. Do not invent scenarios without evidence

Don't add a case for a risk category just because `CompatibilityScenarios` has a constant for it. If
nothing in the JS ever sends a duplicate query key, don't manufacture a `DuplicateQueryKey` case for
that action - it would test a request shape the frontend never produces, adding noise without
characterizing real behavior.

## 3. Prefer request shapes AngularJS actually sends

When a field could plausibly be sent multiple ways (e.g. a numeric ID sometimes appears as a JS
number and sometimes as a string depending on which form control produced it), prefer the shape you
can point to in the actual code over a hypothetical "worst case." If you found *both* shapes at
different call sites, that's two legitimate cases, not one - record both.

## 4. Synthetic variants are allowed only when clearly labeled and risk-justified

Sometimes real evidence for an important risk doesn't exist in the current JS (e.g. the frontend
currently never sends an empty string for a numeric field, but a future frontend change plausibly
could, and that's a known ASP.NET Core `System.Text.Json` migration risk). You may add such a case,
but:

* label it explicitly in a comment as synthetic, e.g. `// SYNTHETIC (no current JS evidence): ...`,
* explain the migration risk it defends against,
* keep it clearly separate from evidence-backed cases in your coverage report (see
  `coverage-reporting.md`'s "scenarios recommended but not generated" / synthetic vs evidenced split).

Never mark a synthetic case as evidence-backed, and don't let synthetic cases outnumber
evidence-backed ones for a given action without calling that imbalance out in your report.

## 5. Inspect controller transformation before forwarding

Read the full action body between model binding and the forwarding call. Note any transformation
that happens first - default-value substitution, validation short-circuits (`if (!ModelState.IsValid)
return ...;`), DTO-to-DTO mapping, conditional branches that forward to *different* endpoints. A
binding risk that gets "fixed up" by the controller before forwarding may still change the **frontend
response** (e.g. a validation error) even if it never reaches the forwarding seam - both observations
matter and both belong in the snapshot (the framework captures both automatically; your job is to
make sure the case actually exercises the branch you think it does).

## 6. Verify whether a scenario reaches the forwarding seam

For each case, form an explicit expectation: does this input actually get forwarded downstream, or
does the controller short-circuit first (validation failure, early return, swallowed exception)? Say
so in a comment on the case. This expectation is what makes a later diff readable - if legacy forwards
and migrated doesn't (or vice versa), that's the exact signal the diff output highlights (see the
root `README.md`'s "Example diff output").

## 7. Document when a scenario intentionally results in no forwarded call

If a case is specifically about characterizing a *rejection* path (e.g. legacy model binding fails
validation and never calls the forwarding seam), say so explicitly in the case's comment, e.g.:

```csharp
// EXPECTED: no forwarded call - legacy ModelState is invalid for this shape and short-circuits.
```

Without this, a reviewer (or a future agent) can't tell an intentional "no forward" case apart from a
case that's broken/misconfigured.
