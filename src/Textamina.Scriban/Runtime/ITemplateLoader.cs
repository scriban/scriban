// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using Textamina.Scriban.Parsing;

namespace Textamina.Scriban.Runtime
{
    public interface ITemplateLoader
    {
        string Load(TemplateContext context, SourceSpan callerSpan, string templateName, out string templateFilePath);
    }
}