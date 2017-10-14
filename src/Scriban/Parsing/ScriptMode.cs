// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Parsing
{
    /// <summary>
    /// Defines how the parser should parse a scriban text.
    /// </summary>
    public enum ScriptMode
    {
        /// <summary>
        /// The template contains a regular content.
        /// </summary>
        Default,

        /// <summary>
        /// The template contains a scriban frontmatter and the parser will parse only this part.
        /// </summary>
        FrontMatterOnly,

        /// <summary>
        /// The template contains a scriban frontmatter and a content and will parse both.
        /// </summary>
        FrontMatterAndContent,

        /// <summary>
        /// The template is directly scriban code (so no necessary {{ }} for entering a code block)
        /// </summary>
        ScriptOnly,
    }
}