---
title: "Safe runtime"
---

# Safe runtime and `TemplateContext`

Scriban's safe runtime has two complementary parts:

- **Exposure control**: templates can only access the builtin functions plus the objects, members, and functions that your application explicitly exposes.
- **Execution control**: `TemplateContext` lets you put limits on loops, recursion, string/output growth, regex execution, and how permissive the runtime should be when values are missing or null.

This is not a process-level sandbox. If you expose a .NET object that can access the file system or network, or configure an [`ITemplateLoader`](includes.md#include-and-itemplateloader) that reads from disk, templates can use those capabilities.

The practical sandbox boundary is therefore:

- which globals and builtins you expose through [`ScriptObject`](scriptobject.md)
- which .NET members you allow through the [member renamer and filter](member-renamer.md)
- whether you configure `TemplateContext.TemplateLoader` for `include`
- which `TemplateContext` execution limits and relaxed-access switches you enable

# Evaluating an expression

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


# Changing the culture

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



# Important `TemplateContext` runtime properties

The following table lists the main `TemplateContext` properties that influence runtime safety, permissiveness, or rendering behavior. These are the defaults for a new `TemplateContext()`:

| Property | Default | Used for |
|----------|---------|----------|
| `StrictVariables` | `false` | Throws a `ScriptRuntimeException` when a variable lookup fails instead of returning `null`. This only affects unresolved variables; relaxed member/indexer settings still control member access behavior on resolved objects. |
| `LoopLimit` | `1000` | Caps loop iterations and also limits range expressions such as `1..100000`. Set to `0` to disable this limit. |
| `LoopLimitQueryable` | `null` | Optional separate loop limit for `IQueryable` enumerations. When `null`, Scriban uses `LoopLimit`. Set to `0` to disable the `IQueryable`-specific limit. |
| `RecursiveLimit` | `100` | Caps recursive function calls. Set to `0` to disable recursion-depth checks. |
| `LimitToString` | `1048576` | Caps string materialization and rendered output growth. Scriban truncates output with `...` when the limit is reached, and some builtins throw if an operation would create a string larger than this limit. Set to `0` to disable the limit. |
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
