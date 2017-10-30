// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    /// <summary>
    /// Render context used to write an AST/<see cref="ScriptNode"/> tree back to a text.
    /// </summary>
    public class RenderContext
    {
        private readonly TextWriter _writer;

        public RenderContext(TextWriter writer, RenderOptions options = default(RenderOptions))
        {
            Options = options;
            _writer = writer;
        }

        /// <summary>
        /// Gets the options for rendering
        /// </summary>
        public readonly RenderOptions Options;

        /// <summary>
        /// A boolean indicating whether the context is in a code section
        /// </summary>
        public bool IsInCode { get; set; }

        public bool ExpectSpace { get; set; }

        public bool ExpectEndOfStatement { get; set; }

        /// <summary>
        /// Gets a boolean indicating whether the last character written has a whitespace.
        /// </summary>
        public bool PreviousHasSpace { get; private set; }

        public bool NextLStrip { get; set; }

        public bool NextRStrip { get; set; }

        public bool IsNextStatementRaw { get; internal set; }

        public bool HasEndOfStatement { get; set; }

        public ScriptRawStatement PreviousRawStatement { get; internal set; }

        public RenderContext Write(string text)
        {
            PreviousHasSpace = text.Length > 0 && char.IsWhiteSpace(text[text.Length - 1]);
            _writer.Write(text);
            return this;
        }

        public RenderContext WithEos()
        {
            ExpectEndOfStatement = true;
            return this;
        }

        public RenderContext WithSpace()
        {
            ExpectSpace = true;
            return this;
        }

        public void WriteTrivias(ScriptNode node, bool before)
        {
            HasEndOfStatement = false;
            if (node.Trivias != null)
            {
                foreach (var trivia in (before ? node.Trivias.Before : node.Trivias.After))
                {
                    trivia.Write(this);
                    if (trivia.Type == ScriptTriviaType.NewLine || trivia.Type == ScriptTriviaType.SemiColon)
                    {
                        HasEndOfStatement = true;
                    }
                }
            }
        }
    }
}