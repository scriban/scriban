# Issue Follow-up Plan

This cleanup pass closed 55 old issues that were stale, already answered, resolved in current Scriban, or outside the scope of core Scriban.

Issue `#625` is fixed locally by the documentation change in this pass.

The 10 issues below remain open because they were either reproduced on current head, still point to incorrect documentation, or need an explicit design decision before any code change is worth doing.

Default stance for the next pass:

- Fix the clearly incorrect docs and reproducible bugs first.
- Do not implement broad feature requests until the desired behavior is agreed.
- Prefer narrowing or closing vague backlog items instead of keeping them open indefinitely.

## 1. Concrete docs and bug fixes

### #453 Default arguments from a delegate definition are ignored

Status: reproduced.

Evidence:

- An imported custom delegate with an optional parameter still fails when called without arguments.
- `src/Scriban/Runtime/DelegateCustomFunction.cs` builds metadata from `del.Method`, which loses defaults from the delegate type's `Invoke` signature in this scenario.

Suggested action:

- Preserve optional/default parameter metadata from the delegate type's `Invoke` signature when wrapping a delegate.
- Add a regression test for an imported custom delegate with an optional parameter.

### #529 Null-Conditional cannot be mixed with indexers

Status: reproduced.

Evidence:

- `{{ a?.b[0] }}` still throws `Object \`a?.b\` is null. Cannot access indexer: a?.b[0]`.

Suggested action:

- Decide whether null-conditional access should short-circuit a following indexer in the same chain.
- If yes, propagate a null result cleanly through the indexer access path and add coverage for mixed member/indexer chains.

### #553 Whitespace control inside of functions affects auto-indent of caller

Status: reproduced.

Evidence:

- `{{~ ... }}` inside a called function still perturbs the caller's auto-indent/output layout.

Suggested action:

- Isolate callee whitespace trimming from caller indentation state.
- Add regression tests for function calls with and without `{{~ ... }}` inside the callee body.

## 2. Docs cleanup that should be narrowed

### #209 Is the documentation out of date?

Status: still broadly true, but too broad.

Evidence:

- `site/docs/runtime/scriptobject.md` still says pipe calls pass the previous value as the last argument, which is outdated or at least misleading.
- #625 is another concrete example of doc drift.

Suggested action:

- Either narrow this issue to a short checklist of specific doc fixes or close it after fixing the concrete doc items already identified.
- Do not leave this as a permanent catch-all docs issue.

## 3. Feature and design decisions before implementation

### #188 array.sort only works with simple members (can't use nested members)

Status: still valid.

Evidence:

- `src/Scriban/Functions/ArrayFunctions.cs` passes the full `member` string to `TryGetValue`, so `"product.name"` is treated as a single member.

Suggested action:

- Decide between:
  - supporting dotted member paths,
  - supporting them only as a fallback when an exact key is missing,
  - or keeping current semantics because dictionary keys containing `.` are already valid.

Recommendation:

- Review together with #405 because both change `array.sort` semantics.

### #405 array.sort for multiple members and/or stable sort algorithm

Status: still valid.

Evidence:

- `ArrayFunctions.Sort` still uses `List<object>.Sort()` and only a single sort key.

Suggested action:

- Decide whether stable sorting and/or multi-key sorting are worth adding.
- If work is approved, combine the design with #188 so `array.sort` changes happen in one pass.

### #368 Possible to Support Async members in RenderAsync?

Status: still valid.

Evidence:

- `await Template.Parse("Hello {{ value }}").RenderAsync(new { value = Task.FromResult(42) })` still renders `Hello System.Threading.Tasks.Task\`1[System.Int32]`.

Suggested action:

- Decide whether `RenderAsync` should auto-await arbitrary model values, only imported async functions, or not at all.
- Only implement after agreeing on the exact behavior and compatibility impact.

### #513 Support For DateOnly and TimeOnly

Status: partially covered only.

Evidence:

- Tests exist for JSON serialization involving `DateOnly`, but there is no obvious broader runtime/date builtin support for `DateOnly` and `TimeOnly`.

Suggested action:

- Clarify the intended scope before touching code:
  - serialization/import only,
  - or full runtime/date builtin support.

Recommendation:

- If broader support is not actually desired, close this instead of expanding the surface area.

## 4. Internal maintenance and low-priority backlog

### #284 Cleanup codegen

Status: still reasonable internal debt, but not a user-facing bug.

Suggested action:

- Only pursue this if there is already active work in the generators.
- Otherwise it can be closed later and tracked outside the issue tracker.

### #446 Benchmarks are outdated

Status: low priority.

Suggested action:

- Revisit only after performance work or before a benchmark-focused refresh.
- Otherwise this can likely be closed as stale, especially now that #367 was closed as the older duplicate.

## Suggested review order

1. #453
2. #529
3. #553
4. #209
5. #188 and #405 together
6. #368
7. #513
8. #284
9. #446
