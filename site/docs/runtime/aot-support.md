---
title: "AOT Support"
---

## AOT & Trimming Support

Starting with Scriban 7.x, the library is marked as **AOT-compatible** (`IsAotCompatible`) on .NET 8+. This means that Scriban can be used in applications published with [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) or [trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained) without producing linker warnings - provided you use the AOT-safe API surface.

- [AOT-safe APIs (no warnings)](#aot-safe-apis-no-warnings)
- [APIs that require reflection](#apis-that-require-reflection)
- [Quick example](#quick-example)
  - [❌ Reflection-based (produces AOT warning)](#-reflection-based-produces-aot-warning)
  - [✅ AOT-safe (no warning)](#-aot-safe-no-warning)
  - [✅ Using built-in functions (AOT-safe)](#-using-built-in-functions-aot-safe)
  - [✅ Nested objects (AOT-safe)](#-nested-objects-aot-safe)
- [How to migrate existing code](#how-to-migrate-existing-code)
- [Annotations reference](#annotations-reference)

## AOT-safe APIs (no warnings)

The following scenarios work fully with Native AOT and trimming **without any warnings**:

| Scenario | Example |
|---|---|
| Parse templates | `Template.Parse("Hello {{ "{{" }} name {{ "}}" }}!")` |
| Render with `TemplateContext` | `template.Render(context)` |
| Populate data via `ScriptObject` | `scriptObject["name"] = "World"` or `scriptObject.Add("key", value)` |
| Use all built-in functions | `string.upcase`, `array.sort`, `math.ceil`, `date.now`, etc. |
| Use `ScriptArray` for lists | `new ScriptArray { "a", "b", "c" }` |
| Nested `ScriptObject` | `parent["child"] = childScriptObject` |
| Async rendering | `await template.RenderAsync(context)` |
| Liquid templates | `Template.ParseLiquid(...)` with `TemplateContext` rendering |

The key principle is: **if you pass data through `ScriptObject` / `ScriptArray` (dictionary-style) rather than arbitrary .NET objects, everything is AOT-safe.**

## APIs that require reflection

Some Scriban APIs use .NET reflection to discover members on arbitrary .NET types at runtime. These APIs are annotated with `[RequiresUnreferencedCode]` and will produce **build-time warnings** if called from AOT/trimmed code:

| API | Why it needs reflection |
|---|---|
| `Template.Render(object model, ...)` | Reflects on the model to import its properties |
| `Template.Evaluate(string, object model, ...)` | Same as above |
| `scriptObject.Import(object)` | Discovers and imports fields/properties/methods from the object |
| `scriptObject.Import(typeof(T), ...)` | Discovers static members from the type |
| `ScriptObject.From(object)` | Creates a `ScriptObject` by reflecting on the source object |

These APIs still work on non-AOT runtimes (e.g. standard .NET 8/9/10 without `PublishAot`). They are **not removed** - they are simply annotated so the compiler can warn you when AOT safety is required.

> [!NOTE]
> 
> Passing `.NET objects` directly as the model (e.g. `template.Render(new { Name = "World" })`) relies on reflection to discover the `Name` property. In an AOT context, use `ScriptObject` to pass data explicitly.

## Quick example

### ❌ Reflection-based (produces AOT warning)

```csharp
var template = Template.Parse("Hello {{ "{{" }} name {{ "}}" }}!");
// This calls the overload that takes `object model` - it uses reflection
// to discover properties on the anonymous type.
var result = template.Render(new { Name = "World" });
```

### ✅ AOT-safe (no warning)

```csharp
var template = Template.Parse("Hello {{ "{{" }} name {{ "}}" }}!");

var scriptObject = new ScriptObject();
scriptObject["name"] = "World";

var context = new TemplateContext();
context.PushGlobal(scriptObject);

var result = template.Render(context);
// result == "Hello World!"
```

### ✅ Using built-in functions (AOT-safe)

```csharp
var template = Template.Parse("{{ "{{" }} name | string.upcase {{ "}}" }}");

var model = new ScriptObject {{ "{{" }} ["name"] = "world" {{ "}}" }};
var context = new TemplateContext();
context.PushGlobal(model);

Console.WriteLine(template.Render(context)); // "WORLD"
```

### ✅ Nested objects (AOT-safe)

```csharp
var product = new ScriptObject
{
    ["name"] = "Widget",
    ["price"] = 9.99
};

var model = new ScriptObject {{ "{{" }} ["product"] = product {{ "}}" }};
var context = new TemplateContext();
context.PushGlobal(model);

var template = Template.Parse("{{ "{{" }} product.name {{ "}}" }} costs {{ "{{" }} product.price {{ "}}" }}");
Console.WriteLine(template.Render(context)); // "Widget costs 9.99"
```

## How to migrate existing code

If you have existing code that uses `Template.Render(model)` with plain .NET objects and you want to make it AOT-compatible, follow these steps:

**1. Replace anonymous objects / POCOs with `ScriptObject`:**

Before:
```csharp
var result = template.Render(new { FirstName = "John", LastName = "Doe" });
```

After:
```csharp
var model = new ScriptObject
{
    ["first_name"] = "John",
    ["last_name"] = "Doe"
};
var context = new TemplateContext();
context.PushGlobal(model);
var result = template.Render(context);
```

> [!NOTE]
> 
> By default, Scriban converts PascalCase property names to snake_case (e.g. `FirstName` → `first_name`). When using `ScriptObject`, you set the keys directly, so use the snake_case form that your templates expect. Alternatively, configure a [member renamer](member-renamer.md) on the `TemplateContext`.

**2. Replace `ScriptObject.From(obj)` with explicit construction:**

Before:
```csharp
var model = ScriptObject.From(myObject); // reflection-based
```

After:
```csharp
var model = new ScriptObject
{
    ["property1"] = myObject.Property1,
    ["property2"] = myObject.Property2
};
```

**3. Replace `scriptObject.Import(obj)` calls:**

Before:
```csharp
var scriptObject = new ScriptObject();
scriptObject.Import(myObject); // reflection-based
```

After:
```csharp
var scriptObject = new ScriptObject();
scriptObject["my_property"] = myObject.MyProperty;
scriptObject["other_value"] = myObject.OtherValue;
```

## Annotations reference

Scriban uses the following .NET trimming/AOT attributes to communicate compatibility:

| Attribute | Meaning |
|---|---|
| `[RequiresUnreferencedCode]` | The method uses reflection that may not work after trimming. The linker cannot guarantee all referenced types are preserved. |
| `[RequiresDynamicCode]` | The method generates code at runtime (e.g. `MakeGenericType`), which is not supported in Native AOT. |
| `[DynamicallyAccessedMembers]` | Tells the trimmer which members of a type must be preserved. Used on parameters and fields to ensure reflected members are kept. |

When you call an API annotated with `[RequiresUnreferencedCode]` from code that is being trimmed or AOT compiled, the compiler emits a warning. This is **by design** - it tells you the call may not work correctly at runtime.

If you know the call is safe in your specific scenario (e.g. you are reflecting on types you control), you can suppress the warning with `[UnconditionalSuppressMessage]`:

```csharp
[UnconditionalSuppressMessage("Trimming", "IL2026",
    Justification = "MyModel's properties are always preserved.")]
static string RenderWithModel(Template template, MyModel model)
{
    return template.Render(model);
}
```

> [!WARNING]
> 
> Only suppress warnings when you are certain the reflected types will not be trimmed. Incorrect suppression can lead to runtime failures in AOT-published applications.
