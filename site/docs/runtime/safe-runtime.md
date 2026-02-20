---
title: "Safe runtime"
---

# Evaluating an expression

It is sometimes convenient to evaluate a script expression without rendering it to a string.

First, there is an option in `TemplateContext.EnableOutput` that can be set to disable the output to the `TemplateContext.Output` StringBuilder.

Also, as in the [Abstract Syntax Tree](ast#abstract-syntax-tree) section, all AST `ScriptNode` have an `Evaluate` method that returns the result of an evaluation.

Lastly, you can use the convenient static method `Template.Evaluate` to quickly evaluate an expression relative to a `TemplateContext`:

```C#
var scriptObject1 = new ScriptObject();
scriptObject1.Add("var1", 5);

var context = new TemplateContext();
context.PushGlobal(scriptObject1);

var result = Template.Evaluate("var1 * 5 + 2", context);
// Prints `27`
Console.WriteLine(result);
```
When using `Template.Evaluate`, the underlying code will use the `ScriptMode.ScriptOnly` when compiling the expression and will disable the output on the `TemplateContext`.


# Changing the Culture

The default culture when running a template is `CultureInfo.InvariantCulture`

You can change the culture that is used when rendering numbers/date/time and parsing date/time by pushing a new Culture to a `TemplateContext`

```C#
var context = new TemplateContext();
context.PushCulture(CultureInfo.CurrentCulture);
// ...
context.PopCulture();
```

> Notice that the parsing of numbers in the language is not culture dependent but is baked into the language specs instead.



# Safe Runtime

The `TemplateContext` provides a few properties to control the runtime and make it safer. You can tweak the following properties:

- `LoopLimit` (default is `1000`): If a script performs a loop over 1000 iteration, the runtime will throw a `ScriptRuntimeException`.  Set to 0 to disable loop limits.
- `RecursiveLimit` (default is `100`): If a script performs a recursive call over 100 depth, the runtime will throw a `ScriptRuntimeException`.  Set to 0 to disable recursion limits.
- `StrictVariables` (default is `false`): If set to `true`, any variables that were not found during variable resolution will throw a `ScriptRuntimeException`
- `RegexTimeOut` (default is `10s`): If a builtin function is using a regular expression that is taking more than 10s to complete, the runtime will throw an exception.  Set to `System.Text.RegularExpressions.Regex.InfiniteMatchTimeout` to disable regular expression timeouts.
