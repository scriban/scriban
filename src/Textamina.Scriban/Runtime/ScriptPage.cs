// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using Textamina.Scriban.Parsing;

namespace Textamina.Scriban.Runtime
{
    public class ScriptPage : ScriptBlockStatement
    {
        /// <summary>
        /// Gets or sets the front matter. May be <c>null</c> if script is not parsed using <see cref=ParsingModeParsingMode.FrontMatter"/>. See remarks.
        /// </summary>
        /// <remarks>
        /// Note that this code block is not executed when evaluating this page. It has to be evaluated separately (usually before evaluating the page).
        /// </remarks>
        public ScriptBlockStatement FrontMatter { get; set; }
    }
}