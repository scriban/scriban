---
title: "Member renamer and filter"
---

## Member renamer

By default, reflection-backed .NET objects accessed through a `ScriptObject` are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of `liquid` templates.

A renamer is simply a delegate that takes an input MemberInfo and return a new member name:

```csharp
namespace Scriban.Runtime
{
    public delegate string MemberRenamerDelegate(MemberInfo member);
}
```

The [`StandardMemberRenamer`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/StandardMemberRenamer.cs) is used to convert string camel/pascal case strings to "ruby" like strings.

If you want to import a .NET object without changing the cases, you can use the simple nop member renamer `member => member.Name`.

Note that renaming can be changed at two levels:

- When importing a .NET object into a `ScriptObject` by passing a renamer delegate, before passing an object to a `TemplateContext`:

  ```csharp
  var scriptObject1 = new ScriptObject();
  // Here the renamer will just return a same member name as the original
  // hence importing .NET member name as-is
  scriptObject1.Import(new MyObject(), renamer: member => member.Name);
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{ "{{" }}Hello{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```
- By setting the default member renamer on the `TemplateContext`

  ```csharp
  // Setup a default renamer at the `TemplateContext` level
  var context = new TemplateContext {MemberRenamer = member => member.Name};
  ```

  It is important to setup this on the `TemplateContext` for any reflection-backed .NET objects that might be accessed indirectly through another `ScriptObject` so that when a .NET object is exposed, it is exposed with the correct naming convention.

Member renamers apply to reflected .NET members. They do not rename dictionary keys or `ScriptObject` entries; those keys are template data and are exposed with their existing names.

The method `Template.Render(object, renamer)` takes also a member renamer, imports the object model with the renamer and setup correctly the renamer on the underlying `TemplateContext`.

So you can rewrite the previous example with the shorter version:

```csharp
var template = Template.Parse("This is Hello: `{{ "{{" }}Hello{{ "}}" }}`");
template.Render(new MyObject(), member => member.Name);
```


## Member filter

Similar to the member renamer, by default, reflection-backed .NET objects accessed through a `ScriptObject` are automatically exposing all public instance fields and properties of .NET objects.

> [!IMPORTANT]
> `MemberFilter` is an exposure convenience for reflected .NET members; it is not a security sandbox. If a template must not access a value, do not put that value (or an object graph that can reveal it) in the Scriban context. For security-sensitive or untrusted templates, prefer constructing a sanitized `ScriptObject` / `ScriptArray` model containing only approved values and functions, and push that into the `TemplateContext` instead of exposing .NET objects directly. This explicit model is also the Native AOT/trimming-friendly path.

A filter is simply a delegate that takes an input MemberInfo and return a boolean to indicate whether to expose the member (`true`) or discard the member (`false`)

```csharp
namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to filter a member while importing a .NET object into a ScriptObject 
    /// or while exposing a .NET instance through a ScriptObject, 
    /// by returning <c>true</c> to keep the member; or false to discard it.
    /// </summary>
    /// <param name="member">A member info</param>
    /// <returns><c>true</c> to keep the member; otherwise <c>false</c> to remove the member</returns>
    public delegate bool MemberFilterDelegate(MemberInfo member);
}
```

- You can use a MemberFilter when importing a an instance:

  ```csharp
  var scriptObject1 = new ScriptObject();
  // Imports only properties that contains the word "Yo"
  scriptObject1.Import(new MyObject(), filter: member => member is PropertyInfo && member.Name.Contains("Yo"));
  ```
- By setting the default member filter on the `TemplateContext`, so that reflection-backed .NET objects automatically exposed via a `ScriptObject` will follow the global filtering rules defined on the context:

  ```csharp
  // Setup a default filter at the `TemplateContext` level
  var context = new TemplateContext {MemberFilter = member => member is PropertyInfo && member.Name.Contains("Yo") };
  ```

Member filters apply to reflected .NET members only. They do not filter dictionary keys or `ScriptObject` entries because these are data entries, not `MemberInfo` reflection members. They also do not affect code paths that deliberately serialize or format a value outside Scriban's member-accessor layer; for example, `object.to_json` uses `System.Text.Json` directly for primitive/scalar values and values implementing `IFormattable`. If a template should not see some data, project or sanitize that data before exposing it to Scriban.

As for the member renamer, it is important to setup this on the `TemplateContext` for any reflection-backed .NET objects that might be accessed indirectly through another `ScriptObject` so that when a .NET object is exposed, it is exposed with the same filtering convention.
