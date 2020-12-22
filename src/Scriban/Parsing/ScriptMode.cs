// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace Scriban.Parsing
{
    /// <summary>
    /// Defines how the parser should parse a script text.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptMode
    {
        /// <summary>
        /// The template contains a regular script content (text and script mixed).
        /// </summary>
        Default,

        /// <summary>
        /// The template contains a script frontmatter (script only) and the parser will parse only this part.
        /// </summary>
        FrontMatterOnly,

        /// <summary>
        /// The template contains a script frontmatter (script only) and a content (text and script mixed) and will parse both.
        /// </summary>
        FrontMatterAndContent,

        /// <summary>
        /// The template is directly scriban code (script only) so no necessary {{ }} for entering a code block
        /// </summary>
        ScriptOnly,
    }
}