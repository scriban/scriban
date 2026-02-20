---
title: "Include and ITemplateLoader"
---

# Include and `ITemplateLoader`

The `include` directives requires that a template loader is setup on the `TemplateContext.TemplateLoader` property

A template loader is responsible for providing the text template from an include directive. The interface of a [`ITemplateLoader`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/ITemplateLoader.cs) is defined like this:

```csharp
/// <summary>
/// Interface used for loading a template.
/// </summary>
public interface ITemplateLoader
{
    /// <summary>
    /// Gets an absolute path for the specified include template name. Note that it is not necessarely a path on a disk, 
    /// but an absolute path that can be used as a dictionary key for caching)
    /// </summary>
    /// <param name="context">The current context called from</param>
    /// <param name="callerSpan">The current span called from</param>
    /// <param name="templateName">The name of the template to load</param>
    /// <returns>An absolute path or unique key for the specified template name</returns>
    string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName);

    /// <summary>
    /// Loads a template using the specified template path/key.
    /// </summary>
    /// <param name="context">The current context called from</param>
    /// <param name="callerSpan">The current span called from</param>
    /// <param name="templatePath">The path/key previously returned by <see cref="GetPath"/></param>
    /// <returns>The content string loaded from the specified template path/key</returns>
    string Load(TemplateContext context, SourceSpan callerSpan, string templatePath);
}
```

In order to use the `include` directive, the template loader should provide:

- The `GetPath` method translates a `templateName` (the argument passed to the `include <templateName>` directive) to a logical/phyisical path that the `ITemplateLoader.Load` method will understand. 
- The `Load` method to actually load the the text template code from the specified `templatePath` (previously returned by `GetPath` method)


The 2 step methods, `GetPath` and then `Load` allows to cache intermediate results. If a template loader returns the same `template path` for a `template name` any existing cached templates will be returned instead. Cached templates are stored in the `TemplateContext.CachedTemplates` property.

A typical implementation of `ITemplateLoader` could read data from the disk:

```csharp
/// <summary>
/// A very simple ITemplateLoader loading directly from the disk, without any checks...etc.
/// </summary>
public class MyIncludeFromDisk : ITemplateLoader
{
    string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return Path.Combine(Environment.CurrentDirectory, templateName);
    }

    string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        // Template path was produced by the `GetPath` method above in case the Template has 
        // not been loaded yet
        return File.ReadAllText(templatePath);
    }
}
```
