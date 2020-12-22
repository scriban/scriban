// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace Scriban.Parsing
{
    /// <summary>
    /// Defines the options for the lexer.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    struct LexerOptions
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
        /// Gets or sets the scripting language used (e.g default, liquid, scientific...).
        /// </summary>
        public ScriptLang Lang { get; set; }

        /// <summary>
        /// If selected mode is <see cref="ScriptMode.FrontMatterOnly"/> or <see cref="ScriptMode.FrontMatterAndContent"/>, this marker will be used
        /// </summary>
        public string FrontMatterMarker { get; set; }

        /// <summary>
        /// <c>true</c> to parse the include target as an implicit string (to support Jekyll passing raw path /a/b/c.txt as an include target).
        /// Only valid if Lang == <see cref="ScriptLang.Liquid"/> as well.
        /// </summary>
        public bool EnableIncludeImplicitString { get; set; }

        /// <summary>
        /// Defines the position to start the lexer parsing relative to the input text passed to <see cref="Lexer"/> constructor
        /// </summary>
        public TextPosition StartPosition { get; set; }

        /// <summary>
        /// The lexer will return whitespaces tokens
        /// </summary>
        public bool KeepTrivia { get; set; }

        /// <summary>
        /// Gets or sets a delegate to allow to match a custom token.
        /// </summary>
        public TryMatchCustomTokenDelegate TryMatchCustomToken { get; set; }

    }

    /// <summary>
    /// A delegate used for matching a custom token. NOTE: A custom token should not parse new lines (`\n` or `\r`)
    /// </summary>
    /// <param name="text">Text being parsed</param>
    /// <param name="position">Current position within the string (to increment if the token is parsed, to keep it as it is if not)</param>
    /// <param name="length">Output the number of character successfully matched at <paramref name="position"/>.</param>
    /// <param name="tokenType">The custom token type within the range (<see cref="TokenType.Custom"/> to <see cref="TokenType.Custom9"/></param>
    /// <returns><c>true</c> if the text at position <paramref name="position"/> is a custom token.</returns>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    delegate bool TryMatchCustomTokenDelegate(string text, TextPosition position, out int length, out TokenType tokenType);
}