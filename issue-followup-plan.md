# Issue Follow-up Plan

This cleanup pass closed 55 old issues that were stale, already answered, resolved in current Scriban, or outside the scope of core Scriban.

Issues `#625`, `#453`, `#529`, `#209`, `#368`, `#405`, and `#188` are fixed locally by the changes in this pass.

Issue `#553` is being closed without a code change because `{{~ ... }}` shows the same indentation-trimming behavior at top level, so this does not appear to be a caller-indent regression specific to function rendering.

Issues `#284` and `#446` are also closed now as low-priority maintenance backlog items that are better handled outside the issue tracker.

The 1 issue below remains open because it still needs an explicit design decision before any code change is worth doing.

Default stance for the next pass:

- Fix the clearly incorrect docs and reproducible bugs first.
- Do not implement broad feature requests until the desired behavior is agreed.
- Prefer narrowing or closing vague backlog items instead of keeping them open indefinitely.

## 1. Concrete docs and bug fixes

## 2. Docs cleanup that should be narrowed

## 3. Feature and design decisions before implementation

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

## Suggested review order

1. #513
