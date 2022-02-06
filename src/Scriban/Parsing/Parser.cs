// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Scriban.Helpers;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Parsing
{
    /// <summary>
    /// The parser.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class Parser
    {
        private readonly Lexer _lexer;
        private readonly bool _isLiquid;
        private Lexer.Enumerator _tokenIt;
        private readonly List<Token> _tokensPreview;
        private int _tokensPreviewStart;
        private Token _previousToken;
        private Token _token;
        private bool _inCodeSection;
        private bool _isLiquidTagSection;
        private int _blockLevel;
        private bool _inFrontMatter;
        private bool _isExpressionDepthLimitReached;
        private int _expressionDepth;
        private bool _hasFatalError;
        private readonly bool _isScientific;
        private readonly bool _isKeepTrivia;
        private readonly List<ScriptTrivia> _trivias;
        private readonly Queue<ScriptStatement> _pendingStatements;
        private IScriptTerminal _lastTerminalWithTrivias;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Parser(Lexer lexer, ParserOptions? options = null)
        {
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            _isLiquid = _lexer.Options.Lang == ScriptLang.Liquid;
            _isScientific = _lexer.Options.Lang == ScriptLang.Scientific;
            _tokensPreview = new List<Token>(4);
            Messages = new LogMessageBag();
            _trivias = new List<ScriptTrivia>();

            Options = options ?? new ParserOptions();
            CurrentParsingMode = lexer.Options.Mode;

            _isKeepTrivia = lexer.Options.KeepTrivia;

            _pendingStatements = new Queue<ScriptStatement>(2);
            Blocks = new Stack<ScriptNode>();

            // Initialize the iterator
            _tokenIt = lexer.GetEnumerator();
            NextToken();
        }

        public readonly ParserOptions Options;
        private ScriptFrontMatter _frontmatter;

        public LogMessageBag Messages { get; private set; }

        public bool HasErrors { get; private set; }

        private Stack<ScriptNode> Blocks { get; }

        private Token Current => _token;

        private Token Previous => _previousToken;

        public SourceSpan CurrentSpan => GetSpanForToken(Current);

        private ScriptMode CurrentParsingMode { get; set; }

        public ScriptPage Run()
        {
            Messages = new LogMessageBag();
            HasErrors = false;
            _blockLevel = 0;
            _isExpressionDepthLimitReached = false;
            Blocks.Clear();

            var page = Open<ScriptPage>();
            var parsingMode = CurrentParsingMode;
            switch (parsingMode)
            {
                case ScriptMode.FrontMatterAndContent:
                case ScriptMode.FrontMatterOnly:
                    if (Current.Type != TokenType.FrontMatterMarker)
                    {
                        LogError($"When `{CurrentParsingMode}` is enabled, expecting a `{_lexer.Options.FrontMatterMarker}` at the beginning of the text instead of `{Current.GetText(_lexer.Text)}`");
                        return null;
                    }

                    _inFrontMatter = true;
                    _inCodeSection = true;

                    _frontmatter = Open<ScriptFrontMatter>();

                    // Parse the frontmatter start=-marker
                    ExpectAndParseTokenTo(_frontmatter.StartMarker, TokenType.FrontMatterMarker);

                    // Parse front-marker statements
                    _frontmatter.Statements = ParseBlockStatement(_frontmatter);

                    // We should not be in a frontmatter after parsing the statements
                    if (_inFrontMatter)
                    {
                        LogError($"End of frontmatter `{_lexer.Options.FrontMatterMarker}` not found");
                    }

                    page.FrontMatter = _frontmatter;
                    page.Span = _frontmatter.Span;

                    if (parsingMode == ScriptMode.FrontMatterOnly)
                    {
                        return page;
                    }
                    break;
                case ScriptMode.ScriptOnly:
                    _inCodeSection = true;
                    break;
            }

            page.Body = ParseBlockStatement(page);
            if (page.Body != null)
            {
                page.Span = page.Body.Span;
            }

            // Flush any pending trivias
            if (_isKeepTrivia && _lastTerminalWithTrivias != null)
            {
                FlushTriviasToLastTerminal();
            }

            if (page.FrontMatter != null)
            {
                FixRawStatementAfterFrontMatter(page);
            }

            if (_lexer.HasErrors)
            {
                foreach (var lexerError in _lexer.Errors)
                {
                    Log(lexerError);
                }
            }

            return page;
        }

        private void PushTokenToTrivia()
        {
            if (_isKeepTrivia)
            {
                if (Current.Type == TokenType.NewLine)
                {
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.NewLine, CurrentStringSlice));
                }
                else if (Current.Type == TokenType.SemiColon)
                {
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.SemiColon, CurrentStringSlice));
                }
                else if (Current.Type == TokenType.Comma)
                {
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Comma, CurrentStringSlice));
                }
            }
        }

        private ScriptStringSlice CurrentStringSlice => GetAsStringSlice(Current);

        private ScriptStringSlice GetAsStringSlice(Token token)
        {
            return new ScriptStringSlice(_lexer.Text,  token.Start.Offset,  token.End.Offset -  token.Start.Offset + 1);
        }

        private T Open<T>(T element) where T : ScriptNode
        {
            element.Span = new SourceSpan() { FileName = _lexer.SourcePath, Start = Current.Start };
            if (_isKeepTrivia && element is IScriptTerminal terminal)
            {
                FlushTrivias(terminal, true);
            }
            return element;
        }

        private T Open<T>() where T : ScriptNode, new()
        {
            var element = new T() { Span = {FileName = _lexer.SourcePath, Start = Current.Start}};
            if (_isKeepTrivia && element is IScriptTerminal terminal)
            {
                FlushTrivias(terminal, true);
            }
            return element;
        }

        private void FlushTrivias(IScriptTerminal element, bool isBefore)
        {
            if (_isKeepTrivia && _trivias.Count > 0)
            {
                element.AddTrivias(_trivias, isBefore);
                _trivias.Clear();
            }
        }

        private T Close<T>(T node) where T : ScriptNode
        {
            node.Span.End = Previous.End;
            if (_isKeepTrivia && node is IScriptTerminal terminal)
            {
                _lastTerminalWithTrivias = terminal;
                FlushTrivias(terminal, false);
            }
            return node;
        }

        private void FlushTriviasToLastTerminal()
        {
            if (_isKeepTrivia && _lastTerminalWithTrivias != null)
            {
                FlushTrivias(_lastTerminalWithTrivias, false);
            }
        }

        private string GetAsText(Token localToken)
        {
            return localToken.GetText(_lexer.Text);
        }

        private bool MatchText(Token localToken, string text)
        {
            return localToken.Match(text, _lexer.Text);
        }

        private void NextToken()
        {
            _previousToken = _token;
            bool result;

            while (_tokensPreviewStart < _tokensPreview.Count)
            {
                _token = _tokensPreview[_tokensPreviewStart];
               _tokensPreviewStart++;

                // We can reset the tokens if we hit the upper limit of the preview
                if (_tokensPreviewStart == _tokensPreview.Count)
                {
                    _tokensPreviewStart = 0;
                    _tokensPreview.Clear();
                }

                if (IsHidden(_token.Type))
                {
                    if (_isKeepTrivia)
                    {
                        PushTrivia(_token);
                    }
                }
                else
                {
                    return;
                }

            }

            // Skip Comments
            while ((result = _tokenIt.MoveNext()))
            {
                if (IsHidden(_tokenIt.Current.Type))
                {
                    if (_isKeepTrivia)
                    {
                        PushTrivia(_tokenIt.Current);
                    }
                }
                else
                {
                    break;
                }
            }

            _token = result ? _tokenIt.Current : Token.Eof;
        }

        private void PushTrivia(Token token)
        {
            ScriptTriviaType type;
            switch (token.Type)
            {
                case TokenType.Comment:
                    type = ScriptTriviaType.Comment;
                    break;

                case TokenType.CommentMulti:
                    type = ScriptTriviaType.CommentMulti;
                    break;

                case TokenType.Whitespace:
                    type = ScriptTriviaType.Whitespace;
                    break;

                case TokenType.WhitespaceFull:
                    type = ScriptTriviaType.WhitespaceFull;
                    break;

                case TokenType.NewLine:
                    type = ScriptTriviaType.NewLine;
                    break;
                default:
                    throw new InvalidOperationException($"Token type `{token.Type}` not supported by trivia");
            }

            var trivia = new ScriptTrivia(GetSpanForToken(token), type,  GetAsStringSlice(token));
            _trivias.Add(trivia);
        }

        private Token PeekToken()
        {
            // Do we have preview token available?
            for (int i = _tokensPreviewStart; i < _tokensPreview.Count; i++)
            {
                var nextToken = _tokensPreview[i];
                if (!IsHidden(nextToken.Type))
                {
                    return nextToken;
                }
            }

            // Else try to find the first token not hidden
            while (_tokenIt.MoveNext())
            {
                var nextToken = _tokenIt.Current;
                _tokensPreview.Add(nextToken);
                if (!IsHidden(nextToken.Type))
                {
                    return nextToken;
                }
            }

            return Token.Eof;
        }

        private ScriptIdentifier ParseIdentifier()
        {
            var identifier = Open<ScriptIdentifier>();
            identifier.Value = GetAsText(Current);
            NextToken();
            return Close(identifier);
        }

        private bool IsHidden(TokenType tokenType)
        {
            return tokenType == TokenType.Comment || tokenType == TokenType.CommentMulti || tokenType == TokenType.Whitespace || tokenType == TokenType.WhitespaceFull || (tokenType == TokenType.NewLine && _allowNewLineLevel > 0);
        }

        private void LogError(string text, bool isFatal = false)
        {
            LogError(Current, text, isFatal);
        }

        private void LogError(Token tokenArg, string text, bool isFatal = false)
        {
            LogError(GetSpanForToken(tokenArg), text, isFatal);
        }

        private SourceSpan GetSpanForToken(Token tokenArg)
        {
            return new SourceSpan(_lexer.SourcePath, tokenArg.Start, tokenArg.End);
        }

        private void LogError(SourceSpan span, string text, bool isFatal = false)
        {
            Log(new LogMessage(ParserMessageType.Error, span, text), isFatal);
        }

        private void LogError(ScriptNode node, string message, bool isFatal = false)
        {
            LogError(node, node.Span, message, isFatal);
        }

        private void LogError(ScriptNode node, SourceSpan span, string message, bool isFatal = false)
        {
            var syntax = ScriptSyntaxAttribute.Get(node);
            string inMessage = " in";
            if (message.EndsWith("after"))
            {
                inMessage = string.Empty;
            }
            LogError(span, $"Error while parsing {syntax.TypeName}: {message}{inMessage}: {syntax.Example}", isFatal);
        }

        private void Log(LogMessage logMessage, bool isFatal = false)
        {
            if (logMessage == null) throw new ArgumentNullException(nameof(logMessage));
            Messages.Add(logMessage);
            if (logMessage.Type == ParserMessageType.Error)
            {
                HasErrors = true;
                if (isFatal)
                {
                    _hasFatalError = true;
                }
            }
        }
    }
}