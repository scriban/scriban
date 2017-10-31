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
        private bool _isInCode;
        private bool _expectSpace;
        private bool _expectEnd;
        private bool _expectEndOfStatement;
        // Gets a boolean indicating whether the last character written has a whitespace.
        private bool _previousHasSpace;
        private bool _nextLStrip;
        private bool _nextRStrip;
        private bool _hasEndOfStatement;

        public RenderContext(TextWriter writer, RenderOptions options = default(RenderOptions))
        {
            Options = options;
            _writer = writer;
        }

        /// <summary>
        /// Gets the options for rendering
        /// </summary>
        public readonly RenderOptions Options;

        internal bool IsNextStatementRaw { get; set; }

        internal ScriptRawStatement PreviousRawStatement { get; set; }

        public RenderContext Write(ScriptNode node)
        {
            if (node != null)
            {
                WriteBegin(node);
                node.Write(this);
                WriteEnd(node);
            }
            return this;
        }

        public RenderContext Write(string text)
        {
            _previousHasSpace = text.Length > 0 && char.IsWhiteSpace(text[text.Length - 1]);
            _writer.Write(text);
            return this;
        }

        public RenderContext WithEos()
        {
            _expectEndOfStatement = true;
            return this;
        }

        public RenderContext WithSpace()
        {
            _expectSpace = true;
            return this;
        }

        public RenderContext WithEnd()
        {
            _expectEnd = true;
            WithEos();
            return this;
        }

        public RenderContext WriteEnterCode(int escape = 0)
        {
            Write("{");
            for (int i = 0; i < escape; i++)
            {
                Write("%");
            }
            Write("{");
            if (_nextLStrip)
            {
                Write("~");
                _nextLStrip = false;
            }
            _isInCode = true;
            return this;
        }

        public RenderContext WriteExitCode(int escape = 0)
        {
            if (_nextRStrip)
            {
                Write("~");
                _nextRStrip = false;
            }
            Write("}");
            for (int i = 0; i < escape; i++)
            {
                Write("%");
            }
            Write("}");
            _isInCode = false;
            return this;
        }

        private void WriteBegin(ScriptNode node)
        {
            var rawStatement = node as ScriptRawStatement;
            if (!(node is ScriptBlockStatement))
            {
                if (_isInCode)
                {
                    if (rawStatement != null)
                    {
                        _nextRStrip = rawStatement.HasTrivia(ScriptTriviaType.Whitespace, true);
                        WriteExitCode();
                    }
                }
                else if (rawStatement == null)
                {
                    if (PreviousRawStatement != null)
                    {
                        _nextLStrip = PreviousRawStatement.HasTrivia(ScriptTriviaType.Whitespace, false);
                    }
                    WriteEnterCode();
                }
            }

            WriteTrivias(node, true);

            // Add a space if this is required and no trivia are providing it
            if (node.CanHaveLeadingTrivia() && _expectSpace && !_previousHasSpace)
            {
                Write(" ");
            }
            _expectSpace = false;

            if (node is ScriptPage)
            {
                IsNextStatementRaw = true;
            }
        }

        private void WriteEnd(ScriptNode node)
        {
            if (_expectEnd)
            {
                var triviasHasEnd = node.HasTrivia(ScriptTriviaType.End, false);

                if (PreviousRawStatement != null)
                {
                    _nextLStrip = PreviousRawStatement.HasTrivia(ScriptTriviaType.Whitespace, false);
                }

                if (!_isInCode)
                {
                    WriteEnterCode();
                }

                if (triviasHasEnd)
                {
                    WriteTrivias(node, false);
                }
                else
                {
                    Write(_isInCode ? "end" : " end ");
                }

                if (!_isInCode)
                {
                    WriteExitCode();
                }
                _expectEnd = false;
            }
            else
            {
                WriteTrivias(node, false);
            }

            if (node is ScriptStatement && _isInCode && _expectEndOfStatement)
            {
                if (!_hasEndOfStatement)
                {
                    if (!IsNextStatementRaw)
                    {
                        Write("; ");
                    }
                }
                _expectEndOfStatement = false;
                _hasEndOfStatement = false;
            }

            if (node is ScriptPage)
            {
                if (_isInCode)
                {
                    WriteExitCode();
                }
            }
        }

        private void WriteTrivias(ScriptNode node, bool before)
        {
            _hasEndOfStatement = false;
            if (node.Trivias != null)
            {
                foreach (var trivia in (before ? node.Trivias.Before : node.Trivias.After))
                {
                    trivia.Write(this);
                    if (trivia.Type == ScriptTriviaType.NewLine || trivia.Type == ScriptTriviaType.SemiColon)
                    {
                        _hasEndOfStatement = true;
                    }
                }
            }
        }
    }
}