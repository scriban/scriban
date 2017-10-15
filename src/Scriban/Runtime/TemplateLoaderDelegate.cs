// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Template loading delegate to setup on a <see cref="TemplateContext.TemplateLoader"/>
    /// </summary>
    /// <param name="context">The current context called from</param>
    /// <param name="callerSpan">The current span called from</param>
    /// <param name="templateName">The name of the template to load</param>
    /// <param name="templateFilePath">The name of the file path (template dependent) used for error reporting</param>
    /// <returns>The string loaded from the specified template name</returns>
    public delegate string TemplateLoaderDelegate(TemplateContext context, SourceSpan callerSpan, string templateName, out string templateFilePath);
}