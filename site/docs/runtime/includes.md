---
title: "Include and ITemplateLoader"
---

## Include and `ITemplateLoader`

The `include` directives requires that a template loader is setup on the `TemplateContext.TemplateLoader` property

A template loader is responsible for providing the text template from an include directive. The interface of a [`ITemplateLoader`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/ITemplateLoader.cs) is defined like this:

```csharp
/// <summary>
/// Interface used for loading a template.
/// </summary>
public interface ITemplateLoader
{
    /// <summary>
    /// Gets an absolute path for the specified include template name. Note that it is not necessarily a path on a disk,
    /// but an absolute path that can be used as a dictionary key for caching). If the loader maps template names to
    /// files, it is responsible for validating and normalizing names against its allowed template roots.
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

- The `GetPath` method translates a `templateName` (the argument passed to the `include <templateName>` directive) to a logical/physical path that the `ITemplateLoader.Load` method will understand.
- The `Load` method to actually load the the text template code from the specified `templatePath` (previously returned by `GetPath` method)


The 2 step methods, `GetPath` and then `Load` allows to cache intermediate results. If a template loader returns the same `template path` for a `template name` any existing cached templates will be returned instead. Cached templates are stored in the `TemplateContext.CachedTemplates` property.

A template name is application-defined: Scriban passes the value from `include` to the loader, but it does not normalize or restrict it because a loader might use logical names, database keys, embedded resources, or another non-file scheme. If a loader maps template names to files, the loader should normalize the candidate path and verify that it stays under the intended template root before reading from disk:

```csharp
/// <summary>
/// A simple ITemplateLoader loading templates from a configured directory.
/// </summary>
public class MyIncludeFromDisk : ITemplateLoader
{
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private readonly string _templateRoot;

    public MyIncludeFromDisk(string templateRoot)
    {
        _templateRoot = Path.GetFullPath(templateRoot);

        if (!_templateRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            _templateRoot += Path.DirectorySeparatorChar;
        }
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        var templatePath = Path.GetFullPath(Path.Combine(_templateRoot, templateName));

        if (!templatePath.StartsWith(_templateRoot, PathComparison))
        {
            throw new ScriptRuntimeException(callerSpan, $"Include `{templateName}` is outside the template root.");
        }

        return templatePath;
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        // Template path was produced by the `GetPath` method above in case the Template has 
        // not been loaded yet
        return File.ReadAllText(templatePath);
    }
}
```
