---
title: "Safe runtime"
---

## Safe runtime and `TemplateContext`

Scriban's safe runtime has two complementary parts:

- **Exposure control**: templates can only access the builtin functions plus the objects, members, and functions that your application explicitly exposes.
- **Execution control**: `TemplateContext` lets you put limits on loops, recursion, string/output growth, regex execution, and how permissive the runtime should be when values are missing or null.

This is not a process-level sandbox or a security boundary around arbitrary .NET objects. If you expose a .NET object that can access the file system, network, secrets, or other sensitive state, or configure an [`ITemplateLoader`](includes.md#include-and-itemplateloader) that reads from disk, templates can use those capabilities.

For untrusted templates, the host application is responsible for exposing only data and functions that the template is allowed to use. Prefer building the context from explicit, sanitized [`ScriptObject`](scriptobject.md) and `ScriptArray` values instead of passing rich .NET objects directly. This also keeps the data model compatible with Native AOT/trimming because Scriban does not need reflection to discover members. Do not rely on `MemberFilter`, `MemberRenamer`, relaxed access switches, or individual builtins as a complete sandbox for objects that contain sensitive members.

When a template accesses a reflected .NET object directly, it can assign to public fields and public non-`init` property or indexer setters. Properties with private, internal, protected, or `init` setters are read-only to templates. `MemberFilter` controls which reflected members are exposed, but it does not provide separate read and write filtering.

The practical exposure boundary is therefore:

- which globals and builtins you expose through [`ScriptObject`](scriptobject.md)
- which .NET objects and values you choose to put in the context; project or sanitize sensitive data before exposure
- which .NET members you allow through the [member renamer and filter](member-renamer.md), as a convenience exposure mechanism rather than a security guarantee
- whether you configure `TemplateContext.TemplateLoader` for `include`
- which `TemplateContext` execution limits and relaxed-access switches you enable

## Evaluating an expression

It is sometimes convenient to evaluate a script expression without rendering it to a string.

First, there is an option in `TemplateContext.EnableOutput` that can be set to disable the output to the `TemplateContext.Output` StringBuilder.

Also, as in the [Abstract Syntax Tree](ast.md#abstract-syntax-tree) section, all AST `ScriptNode` have an `Evaluate` method that returns the result of an evaluation.

Lastly, you can use the convenient static method `Template.Evaluate` to quickly evaluate an expression relative to a `TemplateContext`:

```csharp
var scriptObject1 = new ScriptObject();
scriptObject1.Add("var1", 5);

var context = new TemplateContext();
context.PushGlobal(scriptObject1);

var result = Template.Evaluate("var1 * 5 + 2", context);
// Prints `27`
Console.WriteLine(result);
```
When using `Template.Evaluate`, the underlying code will use the `ScriptMode.ScriptOnly` when compiling the expression and will disable the output on the `TemplateContext`.


## Changing the culture

The default culture when running a template is `CultureInfo.InvariantCulture`

You can change the culture that is used when rendering numbers/date/time and parsing date/time by pushing a new Culture to a `TemplateContext`

```csharp
var context = new TemplateContext();
context.PushCulture(CultureInfo.CurrentCulture);
// ...
context.PopCulture();
```

> [!NOTE]
> The parsing of numbers in the language is not culture dependent but is baked into the language specs instead.



## Important `TemplateContext` runtime properties

The following table lists the main `TemplateContext` properties that influence runtime safety, permissiveness, or rendering behavior. These are the defaults for a new `TemplateContext()`:

| Property | Default | Used for |
|----------|---------|----------|
| `StrictVariables` | `false` | Throws a `ScriptRuntimeException` when a variable lookup fails instead of returning `null`. This only affects unresolved variables; relaxed member/indexer settings still control member access behavior on resolved objects. |
| `LoopLimit` | `1000` | Caps cumulative iterations across each dynamically nested iteration tree, including language loops and internal array/range work. Nested operations share one budget; a separate top-level operation starts a new budget. Set to `0` to disable this limit. |
| `LoopLimitQueryable` | `null` | Optional separate loop limit for `IQueryable` enumerations. When `null`, Scriban uses `LoopLimit`. Set to `0` to disable the `IQueryable`-specific limit. |
| `RecursiveLimit` | `100` | Caps recursive function calls. Set to `0` to disable recursion-depth checks. |
| `LimitToString` | `1048576` | String conversion buffer limit in UTF-16 characters, also used by allocation guards in string-producing operations. By default this also caps cumulative rendered output, including the final output. Set to `0` to disable this limit. |
| `OutputLimit` | `null` | Cumulative rendered output limit in UTF-16 characters. `null` follows `LimitToString` for backward compatibility. Set a positive value to decouple it from `LimitToString`, or `0` to disable only the output limit. |
| `OnStringLimit` | `ScriptLimitBehavior.Truncate` | Controls `ObjectToString` conversions that reach `LimitToString`: append `...`, or throw `ScriptRuntimeException` with `ScriptLimitBehavior.Throw`. Allocation guards in builtins and string multiplication still throw regardless of this setting. |
| `OnOutputLimit` | `ScriptLimitBehavior.Truncate` | Controls writes that would exceed the effective `OutputLimit`: truncate and append `...` once, or throw `ScriptRuntimeException` with `ScriptLimitBehavior.Throw`. |
| `ObjectRecursionLimit` | `20` | Caps recursion depth when walking object graphs for string/JSON-style conversion, helping avoid very deep structures and reference loops. Set to `0` to disable the limit. |
| `RegexTimeOut` | `10s` | Maximum execution time for built-in regex operations. Set it to `System.Text.RegularExpressions.Regex.InfiniteMatchTimeout` to disable regex timeouts. |
| `CancellationToken` | `CancellationToken.None` | Allows rendering/evaluation to be cancelled. Scriban checks this token during evaluation and throws `ScriptAbortException` when cancellation is requested. |
| `EnableOutput` | `true` | Enables writes to `TemplateContext.Output`. `Template.Evaluate(...)` temporarily disables it so expressions can be evaluated without producing rendered output. |
| `EnableBreakAndContinueAsReturnOutsideLoop` | `false` | Makes `break` and `continue` behave like a return when they appear outside a loop. This is primarily used for Liquid compatibility. |
| `EnableRelaxedTargetAccess` | `false` | Lets member/indexer access on a `null` target return `null` instead of throwing. This is broader than using the `?.` operator at a single call site. |
| `EnableRelaxedMemberAccess` | `true` | Lets missing members or dictionary-style keys on an existing object return `null` instead of throwing. |
| `EnableRelaxedFunctionAccess` | `false` | Lets calls to missing functions return `null` instead of throwing. |
| `EnableRelaxedIndexerAccess` | `true` | Lets list/array indexers that are out of bounds return `null` instead of throwing. |
| `EnableNullIndexer` | `false` | Lets a `null` index value return `null` instead of throwing when using an indexer expression. |
| `AutoIndent` | `true` | Preserves the current indentation when rendering included templates and other auto-indented output blocks. |
| `IndentOnEmptyLines` | `true` | Keeps indentation on empty lines when auto-indent is active. |
| `ErrorForStatementFunctionAsExpression` | `false` | Produces an explicit error when a statement-style or `void` function is used where an expression value is expected. |
| `UseScientific` | `false` | Enables scientific-mode evaluation behavior. This is normally set automatically when rendering a template parsed with `ScriptLang.Scientific`. |

Two defaults change when using `LiquidTemplateContext` instead of `TemplateContext`:

- `EnableBreakAndContinueAsReturnOutsideLoop = true`
- `EnableRelaxedTargetAccess = true`

`LiquidTemplateContext` also switches the include parser and lexer options to Liquid-compatible defaults.

### Configuring string and output limits independently

Existing configurations keep their behavior: if `OutputLimit` is left at `null`, changing
`LimitToString` also changes the rendered output limit. To allow a larger document while keeping
individual string conversions bounded, set `OutputLimit` explicitly:

```csharp
var context = new TemplateContext
{
    LimitToString = 1_048_576,
    OutputLimit = 16_777_216,
    OnStringLimit = ScriptLimitBehavior.Throw,
    OnOutputLimit = ScriptLimitBehavior.Throw,
};
```

Set `OutputLimit = 0` to allow unbounded rendered output without disabling string conversion
and allocation guards. Conversely, `LimitToString = 0` with a positive `OutputLimit` disables
the string limit but keeps the output bounded. Disable limits only for trusted workloads.
Both limits treat negative values like `0`.

These limits count UTF-16 characters, not encoded bytes: the default of 1,048,576 characters
corresponds to 2 MiB of UTF-16 character data, not 1 MiB. Each truncation ellipsis is additional
to its own limit; an ellipsis from string conversion still counts toward the output budget
when written. The output budget counts writes across the entire render,
including indentation, nested includes, and temporary outputs such as captures, not just the
length of the final returned string. It resets for each top-level render and on `Reset()`.

For compatibility, string conversion retains its existing boundary behavior: it reports the
limit when the converted buffer reaches the limit (even an exactly-sized string gets `...`
in truncation mode). `OnStringLimit = ScriptLimitBehavior.Throw` reports that same condition
as an exception. The output limit, in contrast, allows an exactly-sized output and reports
only a write that would exceed it. In throw mode, the overflowing output chunk is not written;
earlier writes remain in the output, so streaming output is not transactional.
