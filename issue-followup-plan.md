# Issue Follow-up Plan

This cleanup pass closed 55 old issues that were stale, already answered, resolved in current Scriban, or outside the scope of core Scriban.

Issues `#625`, `#453`, `#529`, `#209`, `#368`, `#405`, and `#188` are fixed locally by the changes in this pass.

Issue `#553` is being closed without a code change because `{{~ ... }}` shows the same indentation-trimming behavior at top level, so this does not appear to be a caller-indent regression specific to function rendering.

Issues `#284` and `#446` are also closed now as low-priority maintenance backlog items that are better handled outside the issue tracker.

Issue `#513` is also being closed without a code change because its requested `DateOnly`/`TimeOnly` support is still too broad and undefined, while the maintainer guidance explicitly prefers not to expand the public date API without a concrete design.

This follow-up plan is now complete: all currently open issues from this pass have been fixed or closed.

Default stance for the next pass:

- Fix the clearly incorrect docs and reproducible bugs first.
- Do not implement broad feature requests until the desired behavior is agreed.
- Prefer narrowing or closing vague backlog items instead of keeping them open indefinitely.

### #513 Support For DateOnly and TimeOnly

Status: closed without implementation.

Evidence:

- Tests exist for JSON serialization involving `DateOnly`, but there is no obvious broader runtime/date builtin support for `DateOnly` and `TimeOnly`.
- `src/Scriban/Functions/DateTimeFunctions.cs` is still heavily `DateTime`-shaped, so "support" would require a broader API/behavior design than a narrow bug fix.

Reason for closing:

- The desired behavior is still not defined clearly enough to implement safely.
- The maintainer guidance on the issue already says not to broaden the date API without a concrete plan.
