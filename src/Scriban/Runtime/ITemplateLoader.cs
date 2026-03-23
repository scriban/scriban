// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using Scriban.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace Scriban.Runtime
{
    /// <summary>
    /// Interface used for loading a template.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface ITemplateLoader
    {
        /// <summary>
        /// Gets an absolute path for the specified include template name. Note that it is not necessarely a path on a disk,
        /// but an absolute path that can be used as a dictionary key for caching)
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templateName">The name of the template to load</param>
        /// <returns>An absolute path or unique key for the specified template name, or <see langword="null"/> if no template could be resolved.</returns>
        string? GetPath(TemplateContext context, SourceSpan callerSpan, string templateName);

        /// <summary>
        /// Loads a template using the specified template path/key.
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templatePath">The path/key previously returned by <see cref="GetPath"/></param>
        /// <returns>The content string loaded from the specified template path/key, or <see langword="null"/> if no template content could be loaded.</returns>
        string? Load(TemplateContext context, SourceSpan callerSpan, string templatePath);

#if !SCRIBAN_NO_ASYNC
        /// <summary>
        /// Loads a template using the specified template path/key.
        /// </summary>
        /// <param name="context">The current context called from</param>
        /// <param name="callerSpan">The current span called from</param>
        /// <param name="templatePath">The path/key previously returned by <see cref="GetPath"/></param>
        /// <returns>The content string loaded from the specified template path/key, or <see langword="null"/> if no template content could be loaded.</returns>
        ValueTask<string?> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath);
#endif
    }
}
