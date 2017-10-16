// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Parsing
{
    /// <summary>
    /// Defines the options for the lexer.
    /// </summary>
    public struct LexerOptions
    {
        public const string DefaultFrontMatterMarker = "+++";

        /// <summary>
        /// Default <see cref="LexerOptions"/>
        /// </summary>
        public static readonly LexerOptions Default = new LexerOptions()
        {
            FrontMatterMarker = DefaultFrontMatterMarker
        };

        /// <summary>
        /// Gets or sets the template mode (text and script, script only, script with frontmatter...etc.). Default is <see cref="ScriptMode.Default"/> text and script mixed.
        /// </summary>
        public ScriptMode Mode { get; set; }

        /// <summary>
        /// If selected mode is <see cref="ScriptMode.FrontMatterOnly"/> or <see cref="ScriptMode.FrontMatterAndContent"/>, this marker will be used
        /// </summary>
        public string FrontMatterMarker { get; set; }

        /// <summary>
        /// Defines the position to start the lexer parsing relative to the input text passed to <see cref="Lexer"/> constructor
        /// </summary>
        public TextPosition StartPosition { get; set; }
    }
}