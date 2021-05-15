// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Scriban.Helpers;

namespace Scriban.Parsing
{
    /// <summary>
    /// Lexer enumerator that generates <see cref="Token"/>, to use in a foreach.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class Lexer : IEnumerable<Token>
    {
        private TextPosition _position;
        private readonly int _textLength;
        private Token _token;
        private char c;
        private BlockType _blockType;
        private bool _isLiquidTagBlock;
        private List<LogMessage> _errors;
        private int _openBraceCount;
        private int _escapeRawCharCount;
        private bool _isExpectingFrontMatter;
        private readonly bool _isLiquid;

        private readonly char _stripWhiteSpaceFullSpecialChar;
        private readonly char _stripWhiteSpaceRestrictedSpecialChar;
        private const char RawEscapeSpecialChar = '%';
        private readonly Queue<Token> _pendingTokens;
        private readonly TryMatchCustomTokenDelegate _tryMatchCustomToken;

        /// <summary>
        /// Lexer options.
        /// </summary>
        public readonly LexerOptions Options;

        /// <summary>
        /// Initialize a new instance of this <see cref="Lexer" />.
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <param name="sourcePath">The sourcePath</param>
        /// <param name="options">The options for the lexer</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Lexer(string text, string sourcePath = null, LexerOptions? options = null)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));

            // Setup options
            var localOptions = options ?? LexerOptions.Default;
            if (localOptions.FrontMatterMarker == null)
            {
                localOptions.FrontMatterMarker = LexerOptions.DefaultFrontMatterMarker;
            }
            Options = localOptions;

            _tryMatchCustomToken = Options.TryMatchCustomToken;

            _position = Options.StartPosition;

            if (_position.Offset > text.Length)
            {
                throw new ArgumentOutOfRangeException($"The starting position `{_position.Offset}` of range [0, {text.Length - 1}]");
            }

            _textLength = text.Length;

            SourcePath = sourcePath ?? "<input>";
            _blockType = Options.Mode == ScriptMode.ScriptOnly ? BlockType.Code : BlockType.Raw;
            _pendingTokens = new Queue<Token>();

            _isExpectingFrontMatter = Options.Mode == ScriptMode.FrontMatterOnly ||
                                     Options.Mode == ScriptMode.FrontMatterAndContent;
            _isLiquid = Options.Lang == ScriptLang.Liquid;
            _stripWhiteSpaceFullSpecialChar = '-';
            _stripWhiteSpaceRestrictedSpecialChar = '~';
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
        public bool HasErrors => _errors != null && _errors.Count > 0;

        /// <summary>
        /// Gets error messages.
        /// </summary>
        public IEnumerable<LogMessage> Errors => _errors ?? Enumerable.Empty<LogMessage>();

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
                if (_pendingTokens.Count > 0)
                {
                    _token = _pendingTokens.Dequeue();
                    return true;
                }

                // If we have errors or we are already at the end of the file, we don't continue
                if (_token.Type == TokenType.Eof)
                {
                    return false;
                }

                if (_position.Offset == _textLength)
                {
                    _token = Token.Eof;
                    return true;
                }

                // Safe guard in any case where the lexer has an error and loop forever (in case we forget to eat a token)
                if (!isFirstLoop && previousPosition == _position)
                {
                    throw new InvalidOperationException("Invalid internal state of the lexer in a forever loop");
                }

                isFirstLoop = false;
                previousPosition = _position;

                if (Options.Mode != ScriptMode.ScriptOnly)
                {
                    if (_blockType == BlockType.Raw)
                    {
                        TokenType whiteSpaceMode;
                        if (IsCodeEnterOrEscape(out whiteSpaceMode))
                        {
                            ReadCodeEnterOrEscape();
                            return true;
                        }
                        else if (_isExpectingFrontMatter && TryParseFrontMatterMarker())
                        {
                            _blockType = BlockType.Code;
                            return true;
                        }
                        Debug.Assert(_blockType == BlockType.Raw || _blockType == BlockType.Escape);
                        // Else we have a BlockType.EscapeRaw, so we need to parse the raw block
                    }

                    if (_blockType != BlockType.Raw && IsCodeExit())
                    {
                        var wasInBlock = _blockType == BlockType.Code;
                        ReadCodeExitOrEscape();
                        if (wasInBlock)
                        {
                            return true;
                        }
                        // We are exiting from a BlockType.EscapeRaw, so we are back to a raw block or code, so we loop again
                        continue;
                    }

                    if (_blockType == BlockType.Code && _isExpectingFrontMatter && TryParseFrontMatterMarker())
                    {
                        // Once we have parsed a front matter, we don't expect them any longer
                        _blockType = BlockType.Raw;
                        _isExpectingFrontMatter = false;
                        return true;
                    }
                }

                // We may me directly at the end of the EOF without reading anykind of block
                // So we need to exit here
                if (_position.Offset == _textLength)
                {
                    _token = Token.Eof;
                    return true;
                }

                if (_blockType == BlockType.Code)
                {
                    if (_isLiquid)
                    {
                        if (ReadCodeLiquid())
                        {
                            break;
                        }
                    }
                    else if (ReadCode())
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

            Escape,

            Raw,
        }

        private bool TryParseFrontMatterMarker()
        {
            var start = _position;
            var end = _position;

            var marker = Options.FrontMatterMarker;
            int i = 0;
            for (; i < marker.Length; i++)
            {
                if (PeekChar(i) != marker[i])
                {
                    return false;
                }
            }
            var pc = PeekChar(i);
            while (pc == ' ' || pc == '\t')
            {
                i++;
                pc = PeekChar(i);
            }

            bool valid = false;

            if (pc == '\n')
            {
                valid = true;
            }
            else if (pc == '\r')
            {
                valid = true;
                if (PeekChar(i + 1) == '\n')
                {
                    i++;
                }
            }

            if (valid)
            {
                while (i-- >= 0)
                {
                    end = _position;
                    NextChar();
                }

                _token = new Token(TokenType.FrontMatterMarker, start, end);
                return true;
            }

            return false;
        }

        private bool IsCodeEnterOrEscape(out TokenType whitespaceMode)
        {
            whitespaceMode = TokenType.Invalid;
            if (c == '{')
            {
                int i = 1;
                var nc = PeekChar(i);
                if (!_isLiquid)
                {
                    while (nc == RawEscapeSpecialChar)
                    {
                        i++;
                        nc = PeekChar(i);
                    }
                }
                if (nc == '{' || (_isLiquid && nc == '%'))
                {
                    var charSpace = PeekChar(i + 1);
                    if (charSpace == _stripWhiteSpaceFullSpecialChar)
                    {
                        whitespaceMode = TokenType.WhitespaceFull;
                    }
                    else if (!_isLiquid && charSpace == _stripWhiteSpaceRestrictedSpecialChar)
                    {
                        whitespaceMode = TokenType.Whitespace;
                    }
                    return true;
                }
            }
            return false;
        }

        private void ReadCodeEnterOrEscape()
        {
            var start = _position;
            var end = _position;

            NextChar(); // Skip {

            if (!_isLiquid)
            {
                while (c == RawEscapeSpecialChar)
                {
                    _escapeRawCharCount++;
                    end = end.NextColumn();
                    NextChar();
                }
            }

            end = end.NextColumn();
            if (_isLiquid && c == '%')
            {
                _isLiquidTagBlock = true;
            }
            NextChar(); // Skip { or %

            if (c == _stripWhiteSpaceFullSpecialChar || (!_isLiquid && c == _stripWhiteSpaceRestrictedSpecialChar))
            {
                end = end.NextColumn();
                NextChar();
            }

            if (_escapeRawCharCount > 0)
            {
                _blockType = BlockType.Escape;
                _token = new Token(TokenType.EscapeEnter, start, end);
            }
            else
            {
                if (_isLiquid && _isLiquidTagBlock)
                {
                    if (TryReadLiquidCommentOrRaw(start, end))
                    {
                        return;
                    }
                }
                _blockType = BlockType.Code;
                _token = new Token(_isLiquidTagBlock ? TokenType.LiquidTagEnter : TokenType.CodeEnter, start, end);
            }
        }

        private bool TryReadLiquidCommentOrRaw(TextPosition codeEnterStart, TextPosition codeEnterEnd)
        {
            var start = _position;
            int offset = 0;
            PeekSkipSpaces(ref offset);
            bool isComment;
            if ((isComment = TryMatchPeek("comment", offset, out offset)) || TryMatchPeek("raw", offset, out offset))
            {
                PeekSkipSpaces(ref offset);
                if (TryMatchPeek("%}", offset, out offset))
                {
                    codeEnterEnd = new TextPosition(start.Offset + offset - 1, start.Line, start.Column + offset - 1);
                    start = new TextPosition(start.Offset + offset, start.Line, start.Column + offset);
                    // Reinitialize the position to the prior character
                    _position = new TextPosition(start.Offset - 1, start.Line, start.Column - 1);
                    c = '}';
                    while (true)
                    {
                        var end = _position;
                        NextChar();
                        var codeExitStart = _position;
                        if (c == '{')
                        {
                            NextChar();
                            if (c == '%')
                            {
                                NextChar();
                                if (c == '-')
                                {
                                    NextChar();
                                }
                                SkipSpaces();

                                if (TryMatch(isComment ? "endcomment" : "endraw"))
                                {
                                    SkipSpaces();
                                    if (c == '-')
                                    {
                                        NextChar();
                                    }
                                    if (c == '%')
                                    {
                                        NextChar();
                                        if (c == '}')
                                        {
                                            var codeExitEnd = _position;
                                            NextChar(); // Skip }
                                            _blockType = BlockType.Raw;
                                            if (isComment)
                                            {
                                                // Convert a liquid comment into a Scriban multi-line {{ ## comment ## }}
                                                _token = new Token(TokenType.CodeEnter, codeEnterStart, codeEnterEnd);
                                                _pendingTokens.Enqueue(new Token(TokenType.CommentMulti, start, end));
                                                _pendingTokens.Enqueue(new Token(TokenType.CodeExit, codeExitStart, codeExitEnd));
                                            }
                                            else
                                            {
                                                // Convert a liquid comment into a Scriban multi-line {{ ## comment ## }}
                                                _token = new Token(TokenType.EscapeEnter, codeEnterStart, codeEnterEnd);
                                                _pendingTokens.Enqueue(new Token(TokenType.Escape, start, end));
                                                _pendingTokens.Enqueue(new Token(TokenType.EscapeExit, codeExitStart, codeExitEnd));
                                            }

                                            _isLiquidTagBlock = false;
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (c == 0)
                        {
                            break;
                        }
                    }
                }
            }
            return false;
        }

        private void SkipSpaces()
        {
            while (IsWhitespace(c))
            {
                NextChar();
            }
        }

        private void PeekSkipSpaces(ref int i)
        {
            while (true)
            {
                var nc = PeekChar(i);
                if (nc == ' ' || nc == '\t')
                {
                    i++;
                }
                else
                {
                    break;
                }
            }
        }


        private bool TryMatchPeek(string text, int offset, out int offsetOut)
        {
            offsetOut = offset;
            for (int index = 0; index < text.Length; offset++, index++)
            {
                if (PeekChar(offset) != text[index])
                {
                    return false;
                }
            }
            offsetOut = offset;
            return true;
        }

        private bool TryMatch(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (c != text[i])
                {
                    return false;
                }
                NextChar();
            }
            return true;
        }

        private bool IsCodeExit()
        {
            // Do we have any brace still opened? If yes, let ReadCode handle them
            if (_openBraceCount > 0)
            {
                return false;
            }

            // Do we have a ~}} or ~}%}
            int start = 0;
            if (c == _stripWhiteSpaceFullSpecialChar || (!_isLiquid && c == _stripWhiteSpaceRestrictedSpecialChar))
            {
                start = 1;
            }

            // Check for either }} or ( %} if liquid active)
            if (PeekChar(start) != (_isLiquidTagBlock? '%' : '}'))
            {
                return false;
            }

            start++;
            if (!_isLiquid)
            {
                for (int i = 0; i < _escapeRawCharCount; i++)
                {
                    if (PeekChar(i + start) != RawEscapeSpecialChar)
                    {
                        return false;
                    }
                }
            }

            return PeekChar(_escapeRawCharCount + start) == '}';
        }

        private void ReadCodeExitOrEscape()
        {
            var start = _position;

            var whitespaceMode = TokenType.Invalid;
            if (c == _stripWhiteSpaceFullSpecialChar)
            {
                whitespaceMode = TokenType.WhitespaceFull;
                NextChar();
            }
            else if (!_isLiquid && c == _stripWhiteSpaceRestrictedSpecialChar)
            {
                whitespaceMode = TokenType.Whitespace;
                NextChar();
            }

            NextChar();  // skip } or %
            if (!_isLiquid)
            {
                for (int i = 0; i < _escapeRawCharCount; i++)
                {
                    NextChar(); // skip !
                }
            }
            var end = _position;
            NextChar(); // skip }

            if (_escapeRawCharCount > 0)
            {
                // We limit the escape count to 9 levels (only for roundtrip mode)
                _pendingTokens.Enqueue(new Token(TokenType.EscapeExit, start, end));
                _escapeRawCharCount = 0;
            }
            else
            {
                _token = new Token(_isLiquidTagBlock ? TokenType.LiquidTagExit : TokenType.CodeExit, start, end);
            }

            // Eat spaces after an exit
            if (whitespaceMode != TokenType.Invalid)
            {
                var startSpace = _position;
                var endSpace = new TextPosition();
                if (ConsumeWhitespace(whitespaceMode == TokenType.Whitespace, ref endSpace, whitespaceMode == TokenType.Whitespace))
                {
                    _pendingTokens.Enqueue(new Token(whitespaceMode, startSpace, endSpace));
                }
            }

            _isLiquidTagBlock = false;
            _blockType = BlockType.Raw;
        }

        private bool ReadRaw()
        {
            var start = _position;
            var end = new TextPosition(-1, 0, 0);
            bool nextCodeEnterOrEscapeExit = false;
            var whitespaceMode = TokenType.Invalid;

            bool isEmptyRaw = false;

            var beforeSpaceFull = TextPosition.Eof;
            var beforeSpaceRestricted = TextPosition.Eof;
            var lastSpaceFull = TextPosition.Eof;
            var lastSpaceRestricted = TextPosition.Eof;
            while (c != '\0')
            {
                if (_blockType == BlockType.Raw && IsCodeEnterOrEscape(out whitespaceMode) || _blockType == BlockType.Escape && IsCodeExit())
                {
                    isEmptyRaw = end.Offset < 0;
                    nextCodeEnterOrEscapeExit = true;
                    break;
                }

                if (char.IsWhiteSpace(c))
                {
                    if (lastSpaceFull.Offset < 0)
                    {
                        lastSpaceFull = _position;
                        beforeSpaceFull = end;
                    }

                    if (!(c == '\n' || (c == '\r' && PeekChar() != '\n')))
                    {
                        if (lastSpaceRestricted.Offset < 0)
                        {
                            lastSpaceRestricted = _position;
                            beforeSpaceRestricted = end;
                        }
                    }
                    else
                    {
                        lastSpaceRestricted.Offset = -1;
                        beforeSpaceRestricted.Offset = -1;
                    }
                }
                else
                {
                    // Reset white space if any
                    lastSpaceFull.Offset = -1;
                    beforeSpaceFull.Offset = -1;
                    lastSpaceRestricted.Offset = -1;
                    beforeSpaceRestricted.Offset = -1;
                }

                end = _position;
                NextChar();
            }

            if (end.Offset < 0)
            {
                end = start;
            }

            var lastSpace = lastSpaceFull;
            var beforeSpace = beforeSpaceFull;
            if (whitespaceMode == TokenType.Whitespace)
            {
                lastSpace = lastSpaceRestricted;
                beforeSpace = beforeSpaceRestricted;
            }

            if (whitespaceMode != TokenType.Invalid && lastSpace.Offset >= 0)
            {
                _pendingTokens.Enqueue(new Token(whitespaceMode, lastSpace, end));

                if (beforeSpace.Offset < 0)
                {
                    return false;
                }
                end = beforeSpace;
            }

            Debug.Assert(_blockType == BlockType.Raw || _blockType == BlockType.Escape);

            if (nextCodeEnterOrEscapeExit)
            {
                if (isEmptyRaw)
                {
                    end = new TextPosition(start.Offset - 1, start.Line, start.Column - 1);
                }
            }

            _token = new Token(_blockType == BlockType.Escape ? TokenType.Escape : TokenType.Raw, start, end);

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
            var start = _position;

            // Try match a custom token
            if (TryMatchCustomToken(start))
            {
                return true;
            }

            switch (c)
            {
                case '\n':
                    _token = new Token(TokenType.NewLine, start, _position);
                    NextChar();
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false, ref _token.End);
                    break;
                case ';':
                    _token = new Token(TokenType.SemiColon, start, _position);
                    NextChar();
                    break;
                case '\r':
                    NextChar();
                    // case of: \r\n
                    if (c == '\n')
                    {
                        _token = new Token(TokenType.NewLine, start, _position);
                        NextChar();
                        // consume all remaining space including new lines
                        ConsumeWhitespace(false, ref _token.End);
                        break;
                    }
                    // case of \r
                    _token = new Token(TokenType.NewLine, start, start);
                    // consume all remaining space including new lines
                    ConsumeWhitespace(false, ref _token.End);
                    break;
                case ':':
                    _token = new Token(TokenType.Colon, start, start);
                    NextChar();
                    break;
                case '@':
                    _token = new Token(TokenType.Arroba, start, start);
                    NextChar();
                    break;
                case '^':
                    NextChar();
                    if (c == '^')
                    {
                        _token = new Token(TokenType.DoubleCaret, start, _position);
                        NextChar();
                        break;
                    }

                    _token = new Token(TokenType.Caret, start, start);
                    break;
                case '*':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.AsteriskEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Asterisk, start, start);
                    break;
                case '/':
                    NextChar();
                    if (c == '/')
                    {
                        _token = new Token(TokenType.DoubleDivide, start, _position);
                        NextChar();
                        if (c == '=')
                        {
                            _token = new Token(TokenType.DoubleDivideEqual, start, _position);
                            NextChar();
                            break;
                        }
                        break;
                    }
                    if (c == '=')
                    {
                        _token = new Token(TokenType.DivideEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Divide, start, start);
                    break;
                case '+':
                    NextChar();
                    if (c == '+')
                    {
                        _token = new Token(TokenType.DoublePlus, start, _position);
                        NextChar();
                        break;
                    }
                    if (c == '=')
                    {
                        _token = new Token(TokenType.PlusEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Plus, start, start);
                    break;
                case '-':
                    NextChar();
                    if (c == '-')
                    {
                        _token = new Token(TokenType.DoubleMinus, start, _position);
                        NextChar();
                        break;
                    }
                    if (c == '=')
                    {
                        _token = new Token(TokenType.MinusEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Minus, start, start);
                    break;
                case '%':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.PercentEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Percent, start, start);
                    break;
                case ',':
                    _token = new Token(TokenType.Comma, start, start);
                    NextChar();
                    break;
                case '&':
                    NextChar();
                    if (c == '&')
                    {
                        _token = new Token(TokenType.DoubleAmp, start, _position);
                        NextChar();
                        break;
                    }

                    // & is an invalid char alone
                    _token = new Token(TokenType.Amp, start, start);
                    break;
                case '?':
                    NextChar();
                    if (c == '?')
                    {
                        _token = new Token(TokenType.DoubleQuestion, start, _position);
                        NextChar();
                        break;
                    }

                    if (c == '.')
                    {
                        _token = new Token(TokenType.QuestionDot, start, _position);
                        NextChar();
                        break;
                    }

                    if (c == '!')
                    {
                        _token = new Token(TokenType.QuestionExclamation, start, _position);
                        NextChar();
                        break;
                    }

                    _token = new Token(TokenType.Question, start, start);
                    break;
                case '|':
                    NextChar();
                    if (c == '|')
                    {
                        _token = new Token(TokenType.DoubleVerticalBar, start, _position);
                        NextChar();
                        break;
                    }
                    else if (c == '>')
                    {
                        _token = new Token(TokenType.PipeGreater, start, _position);
                        NextChar();
                        break;
                    }

                    _token = new Token(TokenType.VerticalBar, start, start);
                    break;
                case '.':
                    NextChar();
                    if (c == '.')
                    {
                        var index = _position;
                        NextChar();
                        if (c == '<')
                        {
                            _token = new Token(TokenType.DoubleDotLess, start, _position);
                            NextChar();
                            break;
                        }

                        if (c == '.')
                        {
                            _token = new Token(TokenType.TripleDot, start, _position);
                            NextChar();
                            break;
                        }

                        _token = new Token(TokenType.DoubleDot, start, index);
                        break;
                    }
                    _token = new Token(TokenType.Dot, start, start);
                    break;

                case '!':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.ExclamationEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Exclamation, start, start);
                    break;

                case '=':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.DoubleEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Equal, start, start);
                    break;
                case '<':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.LessEqual, start, _position);
                        NextChar();
                        break;
                    }
                    if (c == '<')
                    {
                        _token = new Token(TokenType.DoubleLessThan, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Less, start, start);
                    break;
                case '>':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.GreaterEqual, start, _position);
                        NextChar();
                        break;
                    }
                    if (c == '>')
                    {
                        _token = new Token(TokenType.DoubleGreaterThan, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Greater, start, start);
                    break;
                case '(':
                    _token = new Token(TokenType.OpenParen, _position, _position);
                    NextChar();
                    break;
                case ')':
                    _token = new Token(TokenType.CloseParen, _position, _position);
                    NextChar();
                    break;
                case '[':
                    _token = new Token(TokenType.OpenBracket, _position, _position);
                    NextChar();
                    break;
                case ']':
                    _token = new Token(TokenType.CloseBracket, _position, _position);
                    NextChar();
                    break;
                case '{':
                    // We count brace open to match then correctly later and avoid confusing with code exit
                    _openBraceCount++;
                    _token = new Token(TokenType.OpenBrace, _position, _position);
                    NextChar();
                    break;
                case '}':
                    if (_openBraceCount > 0)
                    {
                        // We match first brace open/close
                        _openBraceCount--;
                        _token = new Token(TokenType.CloseBrace, _position, _position);
                        NextChar();
                    }
                    else
                    {
                        if (Options.Mode != ScriptMode.ScriptOnly && IsCodeExit())
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
                        else
                        {
                            // Else we have a close brace but it is invalid
                            AddError("Unexpected } while no matching {", _position, _position);
                            // Remove the previous error token to still output a valid token
                            _token = new Token(TokenType.CloseBrace, _position, _position);
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
                case '`':
                    ReadVerbatimString();
                    break;
                case '\0':
                    _token = Token.Eof;
                    break;
                default:
                    // Eat any whitespace
                    var lastSpace = new TextPosition();
                    if (ConsumeWhitespace(true, ref lastSpace))
                    {
                        if (Options.KeepTrivia)
                        {
                            _token = new Token(TokenType.Whitespace, start, lastSpace);
                        }
                        else
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
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
                    _token = new Token(TokenType.Invalid, _position, _position);
                    NextChar();
                    break;
            }

            return hasTokens;
        }

        private bool TryMatchCustomToken(TextPosition start)
        {
            if (_tryMatchCustomToken != null)
            {
                if (_tryMatchCustomToken(Text, _position, out var matchLength, out var matchTokenType))
                {
                    if (matchLength <= 0) throw new InvalidOperationException($"Invalid match length ({matchLength}) for custom token must be > 0");
                    if (_position.Offset + matchLength > Text.Length) throw new InvalidOperationException($"Invalid match length ({matchLength}) out of range of the input text.");
                    if (matchTokenType < TokenType.Custom && matchTokenType > TokenType.Custom9)
                        throw new InvalidOperationException($"Invalid token type {matchTokenType}. Expecting between {nameof(TokenType)}.{TokenType.Custom} ... {nameof(TokenType)}.{TokenType.Custom9}.");

                    TextPosition matchEnd = _position;
                    while (matchLength > 0)
                    {
                        NextChar();

                        if (_position.Line != start.Line)
                        {
                            throw new InvalidOperationException($"Invalid match, cannot match between new lines at {_position}");
                        }

                        matchEnd = _position;

                        matchLength--;
                    }

                    _token = new Token(matchTokenType, start, matchEnd);

                    return true;
                }
            }

            return false;
        }

        private bool ReadCodeLiquid()
        {
            bool hasTokens = true;
            var start = _position;
            switch (c)
            {
                case ':':
                    _token = new Token(TokenType.Colon, start, start);
                    NextChar();
                    break;
                case ',':
                    _token = new Token(TokenType.Comma, start, start);
                    NextChar();
                    break;
                case '|':
                    _token = new Token(TokenType.VerticalBar, start, start);
                    NextChar();
                    break;
                case '?':
                    NextChar();
                    _token = new Token(TokenType.Question, start, start);
                    break;
                case '-':
                    _token = new Token(TokenType.Minus, start, start);
                    NextChar();
                    break;
                case '.':
                    NextChar();
                    if (c == '.')
                    {
                        _token = new Token(TokenType.DoubleDot, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Dot, start, start);
                    break;

                case '!':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.ExclamationEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Invalid, start, start);
                    break;

                case '=':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.DoubleEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Equal, start, start);
                    break;
                case '<':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.LessEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Less, start, start);
                    break;
                case '>':
                    NextChar();
                    if (c == '=')
                    {
                        _token = new Token(TokenType.GreaterEqual, start, _position);
                        NextChar();
                        break;
                    }
                    _token = new Token(TokenType.Greater, start, start);
                    break;
                case '(':
                    _token = new Token(TokenType.OpenParen, _position, _position);
                    NextChar();
                    break;
                case ')':
                    _token = new Token(TokenType.CloseParen, _position, _position);
                    NextChar();
                    break;
                case '[':
                    _token = new Token(TokenType.OpenBracket, _position, _position);
                    NextChar();
                    break;
                case ']':
                    _token = new Token(TokenType.CloseBracket, _position, _position);
                    NextChar();
                    break;
                case '"':
                case '\'':
                    ReadString();
                    break;
                case '\0':
                    _token = Token.Eof;
                    break;
                default:
                    // Eat any whitespace
                    var lastSpace = new TextPosition();
                    if (ConsumeWhitespace(true, ref lastSpace))
                    {
                        if (Options.KeepTrivia)
                        {
                            _token = new Token(TokenType.Whitespace, start, lastSpace);
                        }
                        else
                        {
                            // We have no tokens for this ReadCode
                            hasTokens = false;
                        }
                        break;
                    }

                    if (IsFirstIdentifierLetter(c))
                    {
                        ReadIdentifier(false);
                        break;
                    }

                    if (char.IsDigit(c))
                    {
                        ReadNumber();
                        break;
                    }

                    // invalid char
                    _token = new Token(TokenType.Invalid, _position, _position);
                    NextChar();
                    break;
            }

            return hasTokens;
        }


        private bool ConsumeWhitespace(bool stopAtNewLine, ref TextPosition lastSpace, bool keepNewLine = false)
        {
            var start = _position;
            while (char.IsWhiteSpace(c))
            {
                if (stopAtNewLine && IsNewLine(c))
                {
                    if (keepNewLine)
                    {
                        lastSpace = _position;
                        NextChar();
                    }
                    break;
                }
                lastSpace = _position;
                NextChar();
            }
            return start != _position;
        }

        private static bool IsNewLine(char c)
        {
            return c == '\n';
        }

        private void ReadIdentifier(bool special)
        {
            var start = _position;

            TextPosition beforePosition;
            bool first = true;
            do
            {
                beforePosition = _position;
                NextChar();

                // Special $$ variable allowed only here
                if (first && special && c == '$')
                {
                    _token = new Token(TokenType.IdentifierSpecial, start, _position);
                    NextChar();
                    return;
                }

                first = false;
            } while (IsIdentifierLetter(c));

            _token = new Token(special ? TokenType.IdentifierSpecial : TokenType.Identifier, start, beforePosition);

            // If we have an include token, we are going to parse spaces and non_white_spaces
            // in order to support the tag "include"
            if (_isLiquid && Options.EnableIncludeImplicitString && _token.Match("include", Text) && char.IsWhiteSpace(c))
            {
                var startSpace = _position;
                var endSpace = startSpace;
                ConsumeWhitespace(false, ref startSpace);
                _pendingTokens.Enqueue(new Token(TokenType.Whitespace, startSpace, endSpace));

                var startPath = _position;
                var endPath = startPath;
                while (!char.IsWhiteSpace(c) && c != 0 && c != '%' && PeekChar() != '}')
                {
                    endPath = _position;
                    NextChar();
                }
                _pendingTokens.Enqueue(new Token(TokenType.ImplicitString, startPath, endPath));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsFirstIdentifierLetter(char c)
        {
            return c == '_' || char.IsLetter(c);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsIdentifierLetter(char c)
        {
            return IsFirstIdentifierLetter(c) || char.IsDigit(c) || (_isLiquid && c ==  '-');
        }

        private void ReadNumber()
        {
            var start = _position;
            var end = _position;
            var isFloat = false;

            var isZero = c == '0';
            NextChar();

            if (isZero && c == 'x')
            {
                ReadHexa(start);
                return;
            }

            if (isZero && c == 'b')
            {
                ReadBinary(start);
                return;
            }

            // Read first part
            if (char.IsDigit(c) || c == '_')
            {
                do
                {
                    end = _position;
                    NextChar();
                } while (char.IsDigit(c) || c == '_');
            }


            // Read any number following
            if (c == '.')
            {
                // If the next char is a '.' it means that we have a range iterator, so we don't touch it
                var nc = PeekChar();
                // Only support . followed
                // - by the postfix: 1.f 2.f
                // - by a digit: 1.0
                // - by an exponent 1.e10
                if (nc != '.' && (IsNumberPostFix(nc) || char.IsDigit(nc) || !char.IsLetter(nc) || nc == 'e' || nc == 'E'))
                {
                    isFloat = true;
                    end = _position;
                    NextChar();
                    while (char.IsDigit(c))
                    {
                        end = _position;
                        NextChar();
                    }
                }
            }

            if (c == 'e' || c == 'E')
            {
                end = _position;
                NextChar();
                if (c == '+' || c == '-')
                {
                    end = _position;
                    NextChar();
                }

                if (!char.IsDigit(c))
                {
                    AddError("Expecting at least one digit after the exponent", _position, _position);
                    return;
                }

                while (char.IsDigit(c))
                {
                    end = _position;
                    NextChar();
                }
            }

            if (!IsIdentifierLetter(PeekChar()) && IsNumberPostFix(c))
            {
                isFloat = true;
                end = _position;
                NextChar();
            }

            _token = new Token(isFloat ? TokenType.Float : TokenType.Integer, start, end);
        }

        private static bool IsNumberPostFix(char c)
        {
            return c == 'f' || c == 'F' || c == 'd' || c == 'D' || c == 'm' || c == 'M';
        }

        private void ReadHexa(TextPosition start)
        {
            var end = _position;
            NextChar(); // skip x

            bool hasHexa = false;
            while (true)
            {
                if (CharHelper.IsHexa(c)) hasHexa = true;
                else if (c != '_') break;
                end = _position;
                NextChar();
            }

            if (!IsIdentifierLetter(PeekChar()) && (c == 'u' || c == 'U'))
            {
                end = _position;
                NextChar();
            }

            if (!hasHexa)
            {
                AddError($"Invalid hex number, expecting at least a hex digit [0-9a-fA-F] after 0x", start, end);
            }
            else
            {
                _token = new Token(TokenType.HexaInteger, start, end);
            }
        }

        private void ReadBinary(TextPosition start)
        {
            var end = _position;
            NextChar(); // skip b

            // Read first part
            bool hasBinary = false;
            bool hasDotAlready = false;
            while (true)
            {
                if (CharHelper.IsBinary(c)) hasBinary = true;
                else if (c != '_') break;
                end = _position;
                NextChar();
                if (c == '.')
                {
                    if (hasDotAlready) break;
                    var nc = PeekChar();
                    if (nc != '0' && nc != '1') break;
                    hasDotAlready = true;
                    NextChar();
                }
            }

            if (!IsIdentifierLetter(PeekChar()) && (c == 'u' || c == 'U' || hasDotAlready && (c == 'f' || c == 'F' || c == 'd' || c == 'D')))
            {
                end = _position;
                NextChar();
            }

            if (!hasBinary)
            {
                AddError($"Invalid binary number, expecting at least a binary digit 0 or 1 after 0b", start, end);
            }
            else
            {
                _token = new Token(TokenType.BinaryInteger, start, end);
            }
        }

        private void ReadString()
        {
            var start = _position;
            var end = _position;
            char startChar = c;
            NextChar(); // Skip "
            while (true)
            {
                if (c == '\\')
                {
                    end = _position;
                    NextChar();
                    // 0 ' " \ b f n r t v u0000-uFFFF x00-xFF
                    switch (c)
                    {
                        case '\n':
                            end = _position;
                            NextChar();
                            continue;
                        case '\r':
                            end = _position;
                            NextChar();
                            if (c == '\n')
                            {
                                end = _position;
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
                            end = _position;
                            NextChar();
                            continue;
                        case 'u':
                            end = _position;
                            NextChar();
                            // Must be followed 4 hex numbers (0000-FFFF)
                            if (c.IsHex()) // 1
                            {
                                end = _position;
                                NextChar();
                                if (c.IsHex()) // 2
                                {
                                    end = _position;
                                    NextChar();
                                    if (c.IsHex()) // 3
                                    {
                                        end = _position;
                                        NextChar();
                                        if (c.IsHex()) // 4
                                        {
                                            end = _position;
                                            NextChar();
                                            continue;
                                        }
                                    }
                                }
                            }
                            AddError($"Unexpected hex number `{c}` following `\\u`. Expecting `\\u0000` to `\\uffff`.", _position, _position);
                            break;
                        case 'x':
                            end = _position;
                            NextChar();
                            // Must be followed 2 hex numbers (00-FF)
                            if (c.IsHex())
                            {
                                end = _position;
                                NextChar();
                                if (c.IsHex())
                                {
                                    end = _position;
                                    NextChar();
                                    continue;
                                }
                            }
                            AddError($"Unexpected hex number `{c}` following `\\x`. Expecting `\\x00` to `\\xff`", _position, _position);
                            break;

                    }
                    AddError($"Unexpected escape character `{c}` in string. Only 0 ' \\ \" b f n r t v u0000-uFFFF x00-xFF are allowed", _position, _position);
                }
                else if (c == '\0')
                {
                    AddError($"Unexpected end of file while parsing a string not terminated by a {startChar}", end, end);
                    return;
                }
                else if (c == startChar)
                {
                    end = _position;
                    NextChar();
                    break;
                }
                else
                {
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(TokenType.String, start, end);
        }

        private void ReadVerbatimString()
        {
            var start = _position;
            var end = _position;
            char startChar = c;
            NextChar(); // Skip `
            while (true)
            {
                if (c == '\0')
                {
                    AddError($"Unexpected end of file while parsing a verbatim string not terminated by a {startChar}", end, end);
                    return;
                }
                else if (c == startChar)
                {
                    end = _position;
                    NextChar(); // Do we have an escape?
                    if (c != startChar)
                    {
                        break;
                    }
                    end = _position;
                    NextChar();
                }
                else
                {
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(TokenType.VerbatimString, start, end);
        }

        private void ReadComment()
        {
            var start = _position;
            var end = _position;

            NextChar();

            // Is Multiline?
            bool isMulti = false;
            if (c == '#')
            {
                isMulti = true;

                end = _position;
                NextChar();

                while (!IsCodeExit())
                {
                    if (c == '\0')
                    {
                        break;
                    }

                    var mayBeEndOfComment = c == '#';

                    end = _position;
                    NextChar();

                    if (mayBeEndOfComment && c == '#')
                    {
                        end = _position;
                        NextChar();
                        break;
                    }
                }

            }
            else
            {
                while (Options.Mode == ScriptMode.ScriptOnly || !IsCodeExit())
                {
                    if (c == '\0' || c == '\r' || c == '\n')
                    {
                        break;
                    }
                    end = _position;
                    NextChar();
                }
            }

            _token = new Token(isMulti ? TokenType.CommentMulti : TokenType.Comment, start, end);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char PeekChar(int count = 1)
        {
            var offset = _position.Offset + count;

            return offset >= 0 && offset < _textLength ? Text[offset] : '\0';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void NextChar()
        {
            _position.Offset++;
            if (_position.Offset < _textLength)
            {
                var nc = Text[_position.Offset];
                if (c == '\n' || (c == '\r' && nc != '\n'))
                {
                    _position.Column = 0;
                    _position.Line += 1;
                }
                else
                {
                    _position.Column++;
                }
                c = nc;
            }
            else
            {
                _position.Offset = _textLength;
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
            _token = new Token(TokenType.Invalid, start, end);
            if (_errors == null)
            {
                _errors = new List<LogMessage>();
            }
            _errors.Add(new LogMessage(ParserMessageType.Error, new SourceSpan(SourcePath, start, end), message));
        }


        private void Reset()
        {
            c = Text.Length > 0 ? Text[Options.StartPosition.Offset] : '\0';
            _position = Options.StartPosition;
            _errors = null;
        }

        private static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t';
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

            public Token Current => lexer._token;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
