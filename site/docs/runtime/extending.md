---
title: "Extending and custom functions"
---

# Extending `TemplateContext`

You may need to extend a `TemplateContext` to overrides some methods there, typically in cases you want:

- To hook into whenever a `ScriptNode` AST node is evaluated
- To catch if a property/member is accessed and should not be null
- Provides a `IObjectAccessor` for non .NET, non `Dictionary<string, object>` in case you are looking to expose a specific object to the runtime that requires a specific access pattern. By overriding the method `GetMemberAccessorImpl` you can override this aspect.
- To override `ToString(span, object)` method to provide custom `ToString` for specifics .NET objects.
- ...etc.


# `ScriptObject` advanced usages

It is sometimes required for a custom function to have access to the current `TemplateContext` or to the access to original location of the text code, where a particular expression is occurring (via a `SourceSpan` that gives a `line`, `column` and `sourcefile` )

## Advanced custom functions

In the [`ScriptObject`](scriptobject) section we described how to easily import a custom function either by using a delegate or a pre-defined .NET static functions/properties.

In some cases, you also need to have access to the current `TemplateContext` and also, the current `SourceSpan` (original location position in the text template code).
By simply adding as a first parameter `TemplateContext`, and optionally as a second parameter, a `SourceSpan` a custom function can have access to the current evaluation context:

```csharp
var scriptObject1 = new ScriptObject();
// Here, we can have access to the `TemplateContext`
scriptObject1.Import("contextAccess", new Func<TemplateContext, string>(templateContext => "Yes"));
```


## Hyper custom functions — `IScriptCustomFunction` 

Some custom functions can require deeper access to the internals for exposing a function. Scriban provides the interface [`IScriptCustomFunction`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/IScriptCustomFunction.cs) for this matter. If an object inherits from this interface and is accessed another `ScriptObject`, it will call the method `IScriptCustomFunction.Invoke`.

```csharp
namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to create a custom function object.
    /// </summary>
    public interface IScriptCustomFunction
    {
        /// <summary>
        /// Calls the custom function object.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="callerContext">The script node originating this call</param>
        /// <param name="parameters">The parameters of the call</param>
        /// <param name="blockStatement">The current block statement this call is made</param>
        /// <returns>The result of the call</returns>
        object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement);
    }
}
```

As you can see, the `IScriptCustomFunction` gives you access to:

- The current `TemplateContext` evaluating the current `Template`
- The AST node context from the `Template` that is calling this custom functions, so you can precisely get information about the location of the parameters in the original source code...etc.
- The parameters already evaluated
- The block statement (not yet used for custom functions - but used by the `wrap` statement)

The `include` expression is typically implemented via a `IScriptCustomFunction`. You can have a look at the details [here](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Functions/IncludeFunction.cs)
