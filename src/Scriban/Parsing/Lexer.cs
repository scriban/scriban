// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Scriban.Helpers;

namespace Scriban.Parsing
{
    /// <summary>
    /// Lexer enumerator that generates <see cref="Token"/>, to use in a foreach.
    /// </summary>
    public class Lexer : IEnumerable<Token>
    {
        private TextPosition position;
        private Token token;
        private char c;
        private readonly bool scriptOnly;
        private BlockType blockType;
        private List<LogMessage> errors;
        private int openBraceCount;
        private int escapeRawCharCount;

        private const char StripWhiteSpaceSpecialChar = '~';
        private const char RawEscapeSpecialChar = '%';

        /// <summary>
        /// Initialize a new instance of this <see cref="Lexer" />.
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <param name="sourcePath">The file path used for error reporting only.</param>
        /// <param name="scriptOnly">if set to <c>true</c> the text is directly a script without raw statements.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentNullException">If text is null</exception>
        public Lexer(string text, string sourcePath = null, bool scriptOnly = false)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Text = text;
            SourcePath = sourcePath ?? "<input>";
            blockType = scriptOnly ? BlockType.Code : BlockType.Raw;
            this.scriptOnly = scriptOnly;
        }

        /// <summary>
        /// Gets the text being parsed by this lexer
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// Gets a boolean indicating whether this lexer has errors.
        /// </summary>
        public bool HasErrors => errors != null && errors.Count > 0;

        /// <summary>
        /// Gets error messages.
        /// </summary>
        public IEnumerable<LogMessage> Errors => errors ?? Enumerable.Empty<LogMessage>();

        /// <summary>
        /// Enumerator. Use simply <code>foreach</code> on this instance to automatically trigger an enumeration of the tokens.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private bool MoveNext()
        {
            var previousPosition = new TextPosition();
            bool isFirstLoop = true;

            while (true)
            {
                // If we have errors or we are already at the end of the file, we don't continue
                if (HasErrors || token.Type == TokenType.Eof)
                {
                    return false;
                }

                if (position.Offset == Text.Length)
                {
                    token = Token.Eof;
                    return true;
                }

                // Safe guard in any case where the lexer has an error and loop forever (in case we forget to eat a token)
                if (!isFirstLoop && previousPosition == position)
                {
                    throw new InvalidOperationException("Invalid internal state of the lexer in a forever loop");
                }

                isFirstLoop = false;
                previousPosition = position;

                if (!scriptOnly)
                {
                    bool skipPreviousSpaces;
                    if (blockType == BlockType.Raw && IsCodeEnterOrEscape(out skipPreviousSpaces))
                    {
                        ReadCodeEnterOrEscape();
                        if (blockType == BlockType.Code)
                        {
                            return true;
                        }

                        // Else we have a BlockType.EscapeRaw, so we need to parse the raw block
                    }

                    if (blockType != BlockType.Raw && IsCodeExit())
                    {
                        var wasInBlock = blockType == BlockType.Code;
                        ReadCodeExitOrEscape();
                        if (wasInBlock)
                        {
                            return true;
                        }
                        // We are exiting from a BlockType.EscapeRaw, so we are back to a raw block or code, so we loop again
                        continue;
                    }
                }

                // We may me directly at the end of the EOF without reading anykind of block
                // So we need to exit here
                if (position.Offset == Text.Length)
                {
                    token = Token.Eof;
                    return true;
                }

                if (blockType == BlockType.Code)
                {
                    if (ReadCode())
                    {
                        break;
                    }
                }
                else
                {
                    if (ReadRaw())
                    {
                        break;
                    }
                }
            }

            return true;
        }

        private enum BlockType
        {
            Code,

            RawEscape,

            Raw,
        }

        private bool IsCodeEnterOrEscape(out bool removePreviousSpaces)
        {
            removePreviousSpaces = false;
            if (c == '{')
            {
                int i = 1;
                var nc = PeekChar(i);
                while (nc == RawEscapeSpecialChar)
                {
                    i++;
                    nc = PeekChar(i);
                }
                if (nc == '{')
                {
                    if (PeekChar(i + 1) == StripWhiteSpaceSpecialChar)
                    {
                        removePreviousSpaces = true;
                    }
                    return true;
                }
            }
            return false;
        }

        private void ReadCodeEnterOrEscape()
        {
            var start = position;
            var end = position;

            NextChar(); // Skip {

            while (c == RawEscapeSpecialChar)
            {
                escapeRawCharCount++;
                end = end.NextColumn();
                NextChar();
            }

            end = end.NextColumn();
            NextChar(); // Skip {

            if (c == StripWhiteSpaceSpecialChar)
            {
                end = end.NextColumn();
                NextChar();
            }

            if (escapeRawCharCount > 0)
            {
                blockType = BlockType.RawEscape;
            }
            else
            {
                blockType = BlockType.Code;
                token = new Token(TokenType.CodeEnter, start, end);
            }
        }

        private bool IsCodeExit()
        {
            // Do we have any brace still opened? If yes, let ReadCode handle them
            if (openBraceCount > 0)
            {
                return false;
            }

            // Do we have a ~}} or ~}%}
            int start = 0;
            if (c == StripWhiteSpaceSpecialChar)
            {
                start = 1;
            }

            // Do we have a regular 
            if (PeekChar(start) != '}')
            {
                return false;
            }

            start++;
            for (int i = 0; i < escapeRawCharCount; i++)
            {
                if (PeekChar(i + start) != RawEscapeSpecialChar)
                {
                    return false;
                }
            }
            
            return PeekChar(escapeRawCharCount + start) == '}';
        }

        private void ReadCodeExitOrEscape()
        {
            var start = position;

            var shouldSkipSpacesAfterExit = false;
            if (c == StripWhiteSpaceSpecialChar)
            {
                shouldSkipSpacesAfterExit = true;
                NextChar();
            }

            NextChar();  // skip }
            for (int i = 0; i < escapeRawCharCount; i++)
            {
                NextChar(); // skip !
            }
            var end = position;
            NextChar(); // skip }

            if (escapeRawCharCount > 0)
            {
                escapeRawCharCount = 0;
            }
            else
            {
                token = new Token(TokenType.CodeExit, start, end);
            }

            // Eat spaces after an exit
            if (shouldSkipSpacesAfterExit)
            {
                ConsumeWhitespace(false);
            }

            blockType = BlockType.Raw;
        }

        private bool ReadRaw()
        {
            var start = position;
            var end = start;
            bool nextCodeEnterOrEscapeExit = false;
            bool removePreviousSpaces = false;

            while (true)
            {
                if (PeekChar() != '\0')
                {
                    if (blockType == BlockType.Raw && IsCodeEnterOrEscape(out removePreviousSpaces) || blockType == BlockType.RawEscape && IsCodeExit())
                    {
                        nextCodeEnterOrEscapeExit = true;
                        break;
                    }

                    end = position;
                    NextChar();
                }
                else
                {
                    break;
                }
            }


            if (removePreviousSpaces)
            {
                int i = -1;
                while (true)
                {
                    var testc = PeekChar(i);
                    if (char.IsWhiteSpace(testc) || testc == '\r')
                    {
                        if (testc == '\r')
                        {
                            end.Offset--;
                        }
                        else
                        {
                            end = testc == '\n' ? end.NextLine(-1) : end.NextColumn(-1);
                        }
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
                if (end.Offset < start.Offset)
                {
                    return false;
                }
            }

            token = new Token(TokenType.Raw, start, nextCodeEnterOrEscapeExit ? end : position);

            // Go to eof
            if (!nextCodeEnterOrEscapeExit)
            {
                NextChar();
            }

            return true;
        }

        private bool ReadCode()
        {
            bool hasTokens = true;
            var start = position;
            switch (c)
            {
                case '\n':
                    token = new Token(TokenType.NewLine, start, position);
                    NextChar();
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false);
                    break;
                case ';':
                    token = new Token(TokenType.SemiColon, start, position);
                    NextChar();
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false);
                    break;
                case '\r':
                    NextChar();
                    // case of: \r\n
                    if (c == '\n')
                    {
                        token = new Token(TokenType.NewLine, start, position);
                        NextChar();
                        // consume all remaining space including new lines
                        ConsumeWhitespace(false);
                        break;
                    }
                    // case of \r
                    token = new Token(TokenType.NewLine, start, start);
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false);
                    break;
                case ':':
                    token = new Token(TokenType.Colon, start, start);
                    NextChar();
                    break;
                case '@':
                    token = new Token(TokenType.Arroba, start, start);
                    NextChar();
                    break;
                case '^':
                    token = new Token(TokenType.Caret, start, start);
                    NextChar();
                    break;
                case '*':
                    token = new Token(TokenType.Multiply, start, start);
                    NextChar();
                    break;
                case '/':
                    NextChar();
                    if (c == '/')
                    {
                        token = new Token(TokenType.DoubleDivide, start, position);
                        NextChar();
                        break;
                    }
                    token = new Token(TokenType.Divide, start, start);
                    break;
                case '+':
                    token = new Token(TokenType.Plus, start, start);
                    NextChar();
                    break;
                case '-':
                    token = new Token(TokenType.Minus, start, start);
                    NextChar();
                    break;
                case '%':
                    token = new Token(TokenType.Modulus, start, start);
                    NextChar();
                    break;
                case ',':
                    token = new Token(TokenType.Comma, start, start);
                    NextChar();
                    break;
                case '&':
                    NextChar();
                    if (c == '&')
                    {
                        token = new Token(TokenType.And, start, position);
                        NextChar();
                        break;
                    }

                    // & is an invalid char alone
                    token = new Token(TokenType.Invalid, start, start);
                    break;
                case '?':
                    NextChar();
                    if (c == '?')
                    {
                        token = new Token(TokenType.EmptyCoalescing, start, position);
                        NextChar();
                        break;
                    }

                    // ? is an invalid char alone
                    token = new Token(TokenType.Invalid, start, start);
                    break;
                case '|':
                    NextChar();
                    if (c == '|')
                    {
                        token = new Token(TokenType.Or, start, position);
                        NextChar();
                        break;
                    }
                    token = new Token(TokenType.Pipe, start, start);
                    break;
                case '.':
                    NextChar();
                    if (c == '.')
                    {
                        var index = position;
                        NextChar();
                        if (c == '<')
                        {
                            token = new Token(TokenType.DoubleDotLess, start, position);
                            NextChar();
                            break;
                        }

                        token = new Token(TokenType.DoubleDot, start, index);
                        break;
                    }
                    token = new Token(TokenType.Dot, start, start);
                    break;

                case '!':
                    NextChar();
                    if (c == '=')
                    {
                        token = new Token(TokenType.CompareNotEqual, start, position);
                        NextChar();
                        break;
                    }
                    token = new Token(TokenType.Not, start, start);
                    break;

                case '=':
                    NextChar();
                    if (c == '=')
                    {
                        token = new Token(TokenType.CompareEqual, start, position);
                        NextChar();
                        break;
                    }
                    token = new Token(TokenType.Equal, start, start);
                    break;
                case '<':
                    NextChar();
                    if (c == '=')
                    {
                        token = new Token(TokenType.CompareLessOrEqual, start, position);
                        NextChar();
                        break;
                    }
                    if (c == '<')
                    {
                        token = new Token(TokenType.ShiftLeft, start, position);
                        NextChar();
                        break;
                    }
                    token = new Token(TokenType.CompareLess, start, start);
                    break;
                case '>':
                    NextChar();
                    if (c == '=')
                    {
                        token = new Token(TokenType.CompareGreaterOrEqual, start, position);
                        NextChar();
                        break;
                    }
                    if (c == '>')
                    {
                        token = new Token(TokenType.ShiftRight, start, position);
                        NextChar();
                        break;
                    }
                    token = new Token(TokenType.CompareGreater, start, start);
                    break;
                case '(':
                    token = new Token(TokenType.OpenParent, position, position);
                    NextChar();
                    break;
                case ')':
                    token = new Token(TokenType.CloseParent, position, position);
                    NextChar();
                    break;
                case '[':
                    token = new Token(TokenType.OpenBracket, position, position);
                    NextChar();
                    break;
                case ']':
                    token = new Token(TokenType.CloseBracket, position, position);
                    NextChar();
                    break;
                case '{':
                    // We count brace open to match then correctly later and avoid confusing with code exit
                    openBraceCount++;
                    token = new Token(TokenType.OpenBrace, position, position);
                    NextChar();
                    break;
                case '}':
                    if (openBraceCount > 0)
                    {
                        // We match first brace open/close
                        openBraceCount--;
                        token = new Token(TokenType.CloseBrace, position, position);
                        NextChar();
                    }
                    else
                    {
                        if (!scriptOnly && IsCodeExit())
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
                        else
                        {
                            // Else we have a close brace but it is invalid
                            token = new Token(TokenType.CloseBrace, position, position);
                            AddError("Unexpected } while no matching {", position, position);
                            NextChar();
                        }
                    }
                    break;
                case '#':
                    ReadComment();
                    break;
                case '"':
                case '\'':
                    ReadString();
                    break;
                case '\0':
                    token = Token.Eof;
                    break;
                default:
                    // Eat any whitespace
                    if (ConsumeWhitespace(true))
                    {
                        // We have no tokens for this ReadCode
                        hasTokens = false;
                        break;
                    }

                    bool specialIdentifier = c == '$';
                    if (IsFirstIdentifierLetter(c) || specialIdentifier)
                    {
                        ReadIdentifier(specialIdentifier);
                        break;
                    }

                    if (char.IsDigit(c))
                    {
                        ReadNumber();
                        break;
                    }

                    // invalid char
                    token = new Token(TokenType.Invalid, position, position);
                    NextChar();
                    break;
            }

            return hasTokens;
        }

        private bool ConsumeWhitespace(bool stopAtNewLine)
        {
            var start = position;
            while (char.IsWhiteSpace(c) && (!stopAtNewLine || !IsNewLine(c)))
            {
                NextChar();
            }
            return start != position;
        }

        private static bool IsNewLine(char c)
        {
            return c == '\n';
        }

        private void ReadIdentifier(bool special)
        {
            var start = position;

            TextPosition beforePosition;
            bool first = true;
            do
            {
                beforePosition = position;
                NextChar();

                // Special $$ variable allowed only here
                if (first && special && c == '$')
                {
                    token = new Token(TokenType.IdentifierSpecial, start, position);
                    NextChar();
                    return;
                }

                first = false;
            } while (IsIdentifierLetter(c));

            token = new Token(special ? TokenType.IdentifierSpecial : TokenType.Identifier, start, beforePosition);
        }

        [MethodImpl(MethodImplOptionsPortable.AggressiveInlining)]
        private static bool IsFirstIdentifierLetter(char c)
        {
            return c == '_' || char.IsLetter(c);
        }

        [MethodImpl(MethodImplOptionsPortable.AggressiveInlining)]
        private static bool IsIdentifierLetter(char c)
        {
            return IsFirstIdentifierLetter(c) || char.IsDigit(c);
        }

        private void ReadNumber()
        {
            var start = position;
            var end = position;
            var hasDot = false;

            // Read first part
            do
            {
                end = position;
                NextChar();
            } while (char.IsDigit(c));


            // Read any number following
            if (c == '.')
            {
                // If the next char is a '.' it means that we have a range iterator, so we don't touch it
                if (PeekChar() != '.')
                {
                    hasDot = true;
                    end = position;
                    NextChar();
                    while (char.IsDigit(c))
                    {
                        end = position;
                        NextChar();
                    }
                }
            }

            if (c == 'e' || c == 'E')
            {
                end = position;
                NextChar();
                if (c == '+' || c == '-')
                {
                    end = position;
                    NextChar();
                }

                if (!char.IsDigit(c))
                {
                    AddError("Expecting at least one digit after the exponent", position, position);
                    return;
                }

                while (char.IsDigit(c))
                {
                    end = position;
                    NextChar();
                }
            }

            token = new Token(hasDot ? TokenType.Float : TokenType.Integer, start, end);
        }

        private void ReadString()
        {
            var start = position;
            var end = position;
            char startChar = c;
            NextChar(); // Skip "
            while (true)
            {
                if (c == '\\')
                {
                    end = position;
                    NextChar();
                    // 0 ' " \ b f n r t v u0000-uFFFF x00-xFF
                    switch (c)
                    {
                        case '\n':
                            end = position;
                            NextChar();
                            continue;
                        case '\r':
                            end = position;
                            NextChar();
                            if (c == '\n')
                            {
                                end = position;
                                NextChar();
                            }
                            continue;
                        case '0':
                        case '\'':
                        case '"':
                        case '\\':
                        case 'b':
                        case 'f':
                        case 'n':
                        case 'r':
                        case 't':
                        case 'v':
                            end = position;
                            NextChar();
                            continue;
                        case 'u':
                            end = position;
                            NextChar();
                            // Must be followed 4 hex numbers (0000-FFFF)
                            if (c.IsHex()) // 1
                            {
                                end = position;
                                NextChar();
                                if (c.IsHex()) // 2
                                {
                                    end = position;
                                    NextChar();
                                    if (c.IsHex()) // 3
                                    {
                                        end = position;
                                        NextChar();
                                        if (c.IsHex()) // 4
                                        {
                                            end = position;
                                            NextChar();
                                            continue;
                                        }
                                    }
                                }
                            }
                            break;
                        case 'x':
                            end = position;
                            NextChar();
                            // Must be followed 2 hex numbers (00-FF)
                            if (c.IsHex())
                            {
                                end = position;
                                NextChar();
                                if (c.IsHex())
                                {
                                    end = position;
                                    NextChar();
                                    continue;
                                }
                            }
                            break;

                    }
                    AddError($"Unexpected escape character [{c}] in string. Only 0 ' \\ \" b f n r t v u0000-uFFFF x00-xFF are allowed", position, position);
                }
                else if (c == '\0')
                {
                    AddError($"Unexpected end of file while parsing a string not terminated by a {startChar}", end, end);
                    return;
                }
                else if (c == startChar)
                {
                    end = position;
                    NextChar();
                    break;
                }
                else
                {
                    end = position;
                    NextChar();
                }
            }

            token = new Token(TokenType.String, start, end);
        }

        private void ReadComment()
        {
            var start = position;
            var end = position;

            NextChar();

            // Is Multiline?
            bool isMulti = false;
            if (c == '#')
            {
                isMulti = true;

                end = position;
                NextChar();

                while (!IsCodeExit())
                {
                    if (c == '\0')
                    {
                        break;
                    }

                    var mayBeEndOfComment = c == '#';

                    end = position;
                    NextChar();

                    if (mayBeEndOfComment && c == '#')
                    {
                        end = position;
                        NextChar();
                        break;
                    }
                }

            }
            else
            {
                while (!IsCodeExit())
                {
                    if (c == '\0' || c == '\r' || c == '\n')
                    {
                        break;
                    }
                    end = position;
                    NextChar();
                }
            }

            token = new Token(isMulti ? TokenType.CommentMulti : TokenType.Comment, start, end);
        }

        [MethodImpl(MethodImplOptionsPortable.AggressiveInlining)]
        private char PeekChar(int count = 1)
        {
            var offset = position.Offset + count;

            return offset >= 0 && offset < Text.Length ? Text[offset] : '\0';
        }

        [MethodImpl(MethodImplOptionsPortable.AggressiveInlining)]
        private void NextChar()
        {
            position.Offset++;
            if (position.Offset < Text.Length)
            {
                if (c == '\n')
                {
                    position.Column = 0;
                    position.Line += 1;
                }
                else
                {
                    position.Column++;
                }
                c = Text[position.Offset];
            }
            else
            {
                position.Offset = Text.Length;
                c = '\0';
            }
        }

        IEnumerator<Token> IEnumerable<Token>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void AddError(string message, TextPosition start, TextPosition end)
        {
            token = new Token(TokenType.Invalid, start, end);
            if (errors == null)
            {
                errors = new List<LogMessage>();
            }
            errors.Add(new LogMessage(ParserMessageType.Error, new SourceSpan(SourcePath, start, end), message));
        }


        private void Reset()
        {
            c = Text.Length > 0 ? Text[0] : '\0';
            position = new TextPosition();
            errors = null;
        }

        /// <summary>
        /// Custom enumerator on <see cref="Token"/>
        /// </summary>
        public struct Enumerator : IEnumerator<Token>
        {
            private readonly Lexer lexer;

            public Enumerator(Lexer lexer)
            {
                this.lexer = lexer;
                lexer.Reset();
            }

            public bool MoveNext()
            {
                return lexer.MoveNext();
            }

            public void Reset()
            {
                lexer.Reset();
            }

            public Token Current => lexer.token;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
