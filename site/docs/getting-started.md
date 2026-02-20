---
title: "Getting started"
---

# Getting started

This guide walks you through installing Scriban, writing your first template, and rendering output from C#.

## Installation

Scriban is available as a [NuGet package](https://www.nuget.org/packages/Scriban/). Add it to your project with:

```shell-session
dotnet add package Scriban
```

Scriban targets **.NET Standard 2.0+**, so it works with .NET 6, .NET 7, .NET 8, .NET 9, .NET 10, .NET Framework 4.7.2+, and more.

> **Tip:** A signed variant is also available as [`Scriban.Signed`](https://www.nuget.org/packages/Scriban.Signed/).

## Your first template

Scriban templates mix plain text with **code blocks** enclosed in `{{ "{{" }}` and `{{ "}}" }}`:

```csharp
using Scriban;

var template = Template.Parse("Hello {{ "{{" }} name {{ "}}" }}!");
var result = template.Render(new { Name = "World" });
Console.WriteLine(result);
// Output: Hello World!
```

That's it — three lines to parse, render, and output a template.

### How it works

1. **`Template.Parse`** compiles the template string into an internal AST (Abstract Syntax Tree).
2. **`template.Render`** evaluates the AST with the given model and produces a string.
3. Properties on the model (like `Name`) are automatically exposed as lowercase/snake_case variables (`name`). See [Member renamer](runtime/readme.md#member-renamer) to customize this.

## Passing data to templates

### Anonymous objects

The simplest way to pass data:

```csharp
var template = Template.Parse("{{ "{{" }} product.name {{ "}}" }} costs {{ "{{" }} product.price {{ "}}" }}€");
var result = template.Render(new { 
    Product = new { Name = "Widget", Price = 9.99 } 
});
// Output: Widget costs 9.99€
```

### Dictionaries via ScriptObject

For dynamic data, use `ScriptObject`:

```csharp
using Scriban;
using Scriban.Runtime;

var template = Template.Parse("Hello {{ "{{" }} name {{ "}}" }}, you are {{ "{{" }} age {{ "}}" }} years old.");

var scriptObject = new ScriptObject();
scriptObject["name"] = "Alice";
scriptObject["age"] = 30;

var context = new TemplateContext();
context.PushGlobal(scriptObject);

var result = template.Render(context);
// Output: Hello Alice, you are 30 years old.
```

## Using loops and conditions

Scriban supports full control flow:

```scriban-html
<ul>
{{ "{{" }} for product in products {{ "}}" }}
  <li>{{ "{{" }} product.name {{ "}}" }} — {{ "{{" }} product.price {{ "}}" }}€</li>
{{ "{{" }} end {{ "}}" }}
</ul>
```

```csharp
var template = Template.Parse(@"<ul>
{{ "{{" }} for product in products {{ "}}" }}
  <li>{{ "{{" }} product.name {{ "}}" }} — {{ "{{" }} product.price {{ "}}" }}€</li>
{{ "{{" }} end {{ "}}" }}
</ul>");

var result = template.Render(new {
    Products = new[] {
        new { Name = "Apple", Price = 1.20 },
        new { Name = "Banana", Price = 0.80 },
        new { Name = "Cherry", Price = 2.50 }
    }
});
```

Output:

```html
<ul>
  <li>Apple — 1.2€</li>
  <li>Banana — 0.8€</li>
  <li>Cherry — 2.5€</li>
</ul>
```

## Using built-in functions

Scriban comes with a rich set of [built-in functions](builtins/readme.md) for strings, arrays, dates, math, and more. Use the **pipe** operator `|` to apply them:

```scriban-html
{{ "{{" }} "hello world" | string.capitalize {{ "}}" }}
```
Output: `Hello world`

```scriban-html
{{ "{{" }} [3, 1, 4, 1, 5] | array.sort | array.join ", " {{ "}}" }}
```
Output: `1, 1, 3, 4, 5`

```scriban-html
{{ "{{" }} date.now | date.to_string '%Y-%m-%d' {{ "}}" }}
```
Output: `2026-02-20` (current date)

## Pipes and chaining

You can chain multiple functions with pipes, just like Unix shell commands:

```scriban-html
{{ "{{" }} "hello beautiful world" | string.split ' ' | array.reverse | array.join ' ' {{ "}}" }}
```
Output: `world beautiful hello`

## Conditionals

```scriban-html
{{ "{{" }} if user.is_admin {{ "}}" }}
  <p>Welcome, admin!</p>
{{ "{{" }} else {{ "}}" }}
  <p>Welcome, {{ "{{" }} user.name {{ "}}" }}!</p>
{{ "{{" }} end {{ "}}" }}
```

## Liquid compatibility

If you are coming from Liquid, Scriban can parse Liquid templates directly:

```csharp
var template = Template.ParseLiquid("Hello {{ "{{" }} name {{ "}}" }}!");
var result = template.Render(new { Name = "World" });
// Output: Hello World!
```

See the [Liquid support](liquid-support.md) guide for the full mapping between Liquid and Scriban syntax.

## Async rendering

Scriban fully supports `async`/`await`:

```csharp
var template = Template.Parse("Hello {{ "{{" }} name {{ "}}" }}!");
var result = await template.RenderAsync(new { Name = "World" });
```

This is useful in web applications where you want non-blocking template rendering.

## Error handling

Always check for parsing errors before rendering:

```csharp
var template = Template.Parse("Hello {{ "{{" }} name }");

if (template.HasErrors)
{
    foreach (var message in template.Messages)
    {
        Console.WriteLine(message);
    }
}
else
{
    var result = template.Render(new { Name = "World" });
    Console.WriteLine(result);
}
```

Scriban provides precise error messages with line and column numbers.

## Source embedding

Starting with Scriban 3.2.1+, you can embed Scriban sources directly into your project instead of referencing the NuGet package as a library. This is useful in contexts where NuGet references are not convenient (e.g., Roslyn Source Generators).

To enable source embedding:

```xml
<PropertyGroup>
  <PackageScribanIncludeSource>true</PackageScribanIncludeSource>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Scriban" Version="6.5.0" IncludeAssets="Build"/>
</ItemGroup>
```

> **Note:** In source-embedding mode, all Scriban types become `internal`.

## What's next?

- **[Language reference](language/readme.md)** — Learn all the syntax: variables, expressions, loops, functions, and more.
- **[Built-in functions](builtins/readme.md)** — Explore the full list of string, array, date, math, and other functions.
- **[Runtime API](runtime/readme.md)** — Dive deeper into `TemplateContext`, `ScriptObject`, custom functions, and advanced scenarios.
- **[Liquid support](liquid-support.md)** — Migrate from Liquid or use Liquid templates with Scriban.