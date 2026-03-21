# Issue Follow-up Plan

This cleanup pass closed 55 old issues that were stale, already answered, resolved in current Scriban, or outside the scope of core Scriban.

Issues `#625`, `#453`, `#529`, `#209`, and `#368` are fixed locally by the changes in this pass.

Issue `#553` is being closed without a code change because `{{~ ... }}` shows the same indentation-trimming behavior at top level, so this does not appear to be a caller-indent regression specific to function rendering.

The 5 issues below remain open because they need an explicit design decision before any code change is worth doing, or are low-priority cleanup items better closed later.

Default stance for the next pass:

- Fix the clearly incorrect docs and reproducible bugs first.
- Do not implement broad feature requests until the desired behavior is agreed.
- Prefer narrowing or closing vague backlog items instead of keeping them open indefinitely.

## 1. Concrete docs and bug fixes

## 2. Docs cleanup that should be narrowed

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

1. #188 and #405 together
2. #513
3. #284
4. #446
