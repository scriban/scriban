// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
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
    public partial class Parser
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
        private bool _isStatementDepthLimitReached;
        private bool _hasFatalError;
        private readonly bool _isKeepTrivia;
        private readonly List<ScriptTrivia> _trivias;
        private readonly Queue<ScriptStatement> _pendingStatements;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public Parser(Lexer lexer, ParserOptions? options = null)
        {
            _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
            _isLiquid = _lexer.Options.Mode == ScriptMode.Liquid;
            _tokensPreview = new List<Token>(4);
            Messages = new List<LogMessage>();
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

        public List<LogMessage> Messages { get; private set; }

        public bool HasErrors { get; private set; }

        private Stack<ScriptNode> Blocks { get; }

        private Token Current => _token;

        private Token Previous => _previousToken;

        public SourceSpan CurrentSpan => GetSpanForToken(Current);

        private ScriptMode CurrentParsingMode { get; set; }

        public ScriptPage Run()
        {
            Messages = new List<LogMessage>();
            HasErrors = false;
            _blockLevel = 0;
            _isStatementDepthLimitReached = false;
            Blocks.Clear();

            var page = Open<ScriptPage>();
            var parsingMode = CurrentParsingMode;
            switch (parsingMode)
            {
                case ScriptMode.FrontMatterAndContent:
                case ScriptMode.FrontMatterOnly:
                    if (Current.Type != TokenType.FrontMatterMarker)
                    {
                        LogError($"When [{CurrentParsingMode}] is enabled, expecting a `{_lexer.Options.FrontMatterMarker}` at the beginning of the text instead of `{Current.GetText(_lexer.Text)}`");
                        return null;
                    }

                    _inFrontMatter = true;
                    _inCodeSection = true;

                    // Skip the frontmatter marker
                    NextToken();

                    // Parse the front matter
                    page.FrontMatter = ParseBlockStatement(null);

                    // We should not be in a frontmatter after parsing the statements
                    if (_inFrontMatter)
                    {
                        LogError($"End of frontmatter `{_lexer.Options.FrontMatterMarker}` not found");
                    }

                    if (parsingMode == ScriptMode.FrontMatterOnly)
                    {
                        return page;
                    }
                    break;
                case ScriptMode.ScriptOnly:
                    _inCodeSection = true;
                    break;
            }

            ParseBlockStatement(page);

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

            return !HasErrors ? page : null;
        }

        private void FixRawStatementAfterFrontMatter(ScriptPage page)
        {
            // In case of parsing a front matter, we don't want to include any \r\n after the end of the front-matter
            // So we manipulate back the syntax tree for the expected raw statement (if any), otherwise we can early 
            // exit.
            var rawStatement = page.Statements.FirstOrDefault() as ScriptRawStatement;
            if (rawStatement == null)
            {
                return;
            }

            var startOffset = rawStatement.Span.Start.Offset;
            var endOffset = rawStatement.Span.End.Offset;
            for (int i = startOffset; i <= endOffset; i++)
            {
                var c = rawStatement.Text[i];
                if (c == ' ' || c == '\t')
                {
                    continue;
                }
                if (c == '\r')
                {
                    if (i + 1 <= endOffset && rawStatement.Text[i+1] == '\n')
                    {
                        rawStatement.Span.Start = new TextPosition(i + 2, rawStatement.Span.Start.Line + 1, 0);
                    }
                    break;
                }

                if (c == '\n')
                {
                    rawStatement.Span.Start = new TextPosition(i + 1, rawStatement.Span.Start.Line + 1, 0);
                }
                break;
            }
        }

        private bool TryParseStatement(ScriptStatement parent, out ScriptStatement statement, out bool hasEnd)
        {
            hasEnd = false;
            bool nextStatement = true;
            statement = null;

            continueParsing:

            if (_hasFatalError)
            {
                return false;
            }

            if (_pendingStatements.Count > 0)
            {
                statement = _pendingStatements.Dequeue();
                return true;
            }

            switch (Current.Type)
            {
                case TokenType.Eof:
                    // Early exit
                    nextStatement = false;
                    break;

                case TokenType.Raw:
                case TokenType.Escape:
                    statement = ParseRawStatement();
                    break;

                case TokenType.CodeEnter:
                case TokenType.LiquidTagEnter:
                    if (_inCodeSection)
                    {
                        LogError("Unexpected token while already in a code block");
                    }
                    _isLiquidTagSection = Current.Type == TokenType.LiquidTagEnter;
                    _inCodeSection = true;

                    // If we have any pending trivias before processing this code enter and we want to keep trivia
                    // we need to generate a RawStatement to store these trivias
                    if (_isKeepTrivia && (_trivias.Count > 0 || Previous.Type == TokenType.CodeEnter))
                    {
                        var rawStatement = Open<ScriptRawStatement>();
                        Close(rawStatement);
                        if (_trivias.Count > 0)
                        {
                            rawStatement.Trivias.After.AddRange(rawStatement.Trivias.Before);
                            rawStatement.Trivias.Before.Clear();
                            var firstTriviaSpan = rawStatement.Trivias.After[0].Span;
                            var lastTriviaSpan = rawStatement.Trivias.After[rawStatement.Trivias.After.Count - 1].Span;
                            rawStatement.Span = new SourceSpan(firstTriviaSpan.FileName, firstTriviaSpan.Start, lastTriviaSpan.End);
                        }
                        else
                        {
                            // Else Add an empty trivia
                            rawStatement.AddTrivia(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Empty, null), false);
                        }
                        statement = rawStatement;
                    }

                    NextToken();

                    if (Current.Type == TokenType.CodeExit)
                    {
                        var nopStatement = Open<ScriptNopStatement>();
                        Close(nopStatement);
                        if (statement == null)
                        {
                            statement = nopStatement;
                        }                        
                        else
                        {
                            _pendingStatements.Enqueue(nopStatement);
                        }
                    }

                    // If we have a ScriptRawStatement previously defined, we need to break out of the loop to record it
                    if (statement == null)
                    {
                        goto continueParsing;
                    }
                    break;
                case TokenType.FrontMatterMarker:
                    if (_inFrontMatter)
                    {
                        _inFrontMatter = false;
                        _inCodeSection = false;
                        // When we expect to parse only the front matter, don't try to tokenize the following text
                        // Keep the current token as the code exit of the front matter
                        if (CurrentParsingMode != ScriptMode.FrontMatterOnly)
                        {
                            NextToken();
                        }

                        if (CurrentParsingMode == ScriptMode.FrontMatterAndContent || CurrentParsingMode == ScriptMode.FrontMatterOnly)
                        {
                            // Once the FrontMatter has been parsed, we can switch to default parsing mode.

                            CurrentParsingMode = ScriptMode.Default;
                            nextStatement = false;
                        }
                    }
                    else
                    {
                        LogError($"Unexpected frontmatter marker `{_lexer.Options.FrontMatterMarker}` while not inside a frontmatter");
                        NextToken();
                    }
                    break;

                case TokenType.CodeExit:
                case TokenType.LiquidTagExit:
                    if (!_inCodeSection)
                    {
                        LogError("Unexpected code block exit '}}' while no code block enter '{{' has been found");
                    }
                    else if (CurrentParsingMode == ScriptMode.ScriptOnly)
                    {
                        LogError("Unexpected code clock exit '}}' while parsing in script only mode. '}}' is not allowed.");
                    }

                    _isLiquidTagSection = false;
                    _inCodeSection = false;

                    // We clear any trivia that might not have been takend by a node
                    // so that they don't end up after this token
                    if (_isKeepTrivia)
                    {
                        _trivias.Clear();
                    }

                    NextToken();

                    // If next token is directly a code enter or an eof but we want to keep trivia
                    // and with have trivias
                    // we need to generate a RawStatement to store these trivias
                    if (_isKeepTrivia && (Current.Type == TokenType.CodeEnter || Current.Type == TokenType.Eof))
                    {
                        var rawStatement = Open<ScriptRawStatement>();
                        Close(rawStatement);
                        if (_trivias.Count > 0)
                        {
                            var firstTriviaSpan = rawStatement.Trivias.Before[0].Span;
                            var lastTriviaSpan = rawStatement.Trivias.Before[rawStatement.Trivias.Before.Count - 1].Span;
                            rawStatement.Span = new SourceSpan(firstTriviaSpan.FileName, firstTriviaSpan.Start, lastTriviaSpan.End);
                        }
                        else
                        {
                            // Else Add an empty trivia
                            rawStatement.AddTrivia(new ScriptTrivia(CurrentSpan, ScriptTriviaType.Empty, null), false);
                        }
                        statement = rawStatement;
                    }
                    else
                    {
                        goto continueParsing;
                    }
                    break;

                default:
                    if (_inCodeSection)
                    {
                        switch (Current.Type)
                        {
                            case TokenType.NewLine:
                            case TokenType.SemiColon:
                                PushTokenToTrivia();
                                NextToken();
                                goto continueParsing;
                            case TokenType.Identifier:
                            case TokenType.IdentifierSpecial:
                                var identifier = GetAsText(Current);
                                if (_isLiquid)
                                {
                                    ReadLiquidStatement(identifier, parent, ref statement, ref hasEnd, ref nextStatement);
                                }
                                else
                                {
                                    ReadScribanStatement(identifier, parent, ref statement, ref hasEnd, ref nextStatement);
                                }
                                break;
                            default:
                                if (StartAsExpression())
                                {
                                    statement = ParseExpressionStatement();
                                }
                                else
                                {
                                    nextStatement = false;
                                    LogError($"Unexpected token {Current.Type}");
                                }
                                break;
                        }
                    }
                    else
                    {
                        nextStatement = false;
                        LogError($"Unexpected token {Current.Type} while not in a code block {{ ... }}");
                        // LOG an ERROR. Don't expect any other tokens outside a code section
                    }
                    break;
            }

            return nextStatement;
        }

        private void ReadScribanStatement(string identifier, ScriptStatement parent, ref ScriptStatement statement, ref bool hasEnd, ref bool nextStatement)
        {
            var startToken = Current;
            switch (identifier)
            {
                case "end":
                    hasEnd = true;
                    nextStatement = false;

                    if (_isKeepTrivia)
                    {
                        _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.End, _lexer.Text));
                    }
                    NextToken();

                    var matchingStatement = FindFirstStatementExpectingEnd();
                    ExpectEndOfStatement(matchingStatement);
                    if (_isKeepTrivia)
                    {
                        FlushTrivias(matchingStatement, false);
                    }
                    break;
                case "wrap":
                    CheckNotInCase(parent, startToken);
                    statement = ParseWrapStatement();
                    break;
                case "if":
                    CheckNotInCase(parent, startToken);
                    statement = ParseIfStatement(false, false);
                    break;
                case "case":
                    CheckNotInCase(parent, startToken);
                    statement = ParseCaseStatement();
                    break;
                case "when":
                    var whenStatement = ParseWhenStatement();
                    var whenParent = parent as ScriptConditionStatement;
                    if (parent is ScriptWhenStatement)
                    {
                        ((ScriptWhenStatement)whenParent).Next = whenStatement;
                    }
                    else if (parent is ScriptCaseStatement)
                    {
                        statement = whenStatement;
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: TODO
                        LogError(startToken, "A `when` condition must be preceded by another `when`/`else`/`case` condition");
                    }
                    hasEnd = true;
                    break;
                case "else":
                    var nextCondition = ParseElseStatement(false);
                    var parentCondition = parent as ScriptConditionStatement;
                    if (parent is ScriptIfStatement || parent is ScriptWhenStatement)
                    {
                        if (parent is ScriptIfStatement)
                        {
                            ((ScriptIfStatement)parentCondition).Else = nextCondition;
                        }
                        else
                        {
                            ((ScriptWhenStatement)parentCondition).Next = nextCondition;
                        }
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: 201-if-else-error3.txt
                        LogError(startToken, "A else condition must be preceded by another if/else/when condition");
                    }
                    hasEnd = true;
                    break;
                case "for":
                    CheckNotInCase(parent, startToken);
                    if (PeekToken().Type == TokenType.Dot)
                    {
                        statement = ParseExpressionStatement();
                    }
                    else
                    {
                        statement = ParseForStatement();
                    }
                    break;
                case "with":
                    CheckNotInCase(parent, startToken);
                    statement = ParseWithStatement();
                    break;
                case "import":
                    CheckNotInCase(parent, startToken);
                    statement = ParseImportStatement();
                    break;
                case "readonly":
                    CheckNotInCase(parent, startToken);
                    statement = ParseReadOnlyStatement();
                    break;
                case "while":
                    CheckNotInCase(parent, startToken);
                    if (PeekToken().Type == TokenType.Dot)
                    {
                        statement = ParseExpressionStatement();
                    }
                    else
                    {
                        statement = ParseWhileStatement();
                    }
                    break;
                case "break":
                    CheckNotInCase(parent, startToken);
                    statement = Open<ScriptBreakStatement>();
                    NextToken();
                    ExpectEndOfStatement(statement);
                    Close(statement);

                    // This has to be done at execution time, because of the wrap statement
                    //if (!IsInLoop())
                    //{
                    //    LogError(statement, "Unexpected statement outside of a loop");
                    //}
                    break;
                case "continue":
                    CheckNotInCase(parent, startToken);
                    statement = Open<ScriptContinueStatement>();
                    NextToken();
                    ExpectEndOfStatement(statement);
                    Close(statement);

                    // This has to be done at execution time, because of the wrap statement
                    //if (!IsInLoop())
                    //{
                    //    LogError(statement, "Unexpected statement outside of a loop");
                    //}
                    break;
                case "func":
                    CheckNotInCase(parent, startToken);
                    statement = ParseFunctionStatement(false);
                    break;
                case "ret":
                    CheckNotInCase(parent, startToken);
                    statement = ParseReturnStatement();
                    break;
                case "capture":
                    CheckNotInCase(parent, startToken);
                    statement = ParseCaptureStatement();
                    break;
                default:
                    CheckNotInCase(parent, startToken);
                    // Otherwise it is an expression statement
                    statement = ParseExpressionStatement();
                    break;
            }
        }

        private ScriptStatement FindFirstStatementExpectingEnd()
        {
            foreach (var scriptNode in Blocks)
            {
                if (ExpectStatementEnd(scriptNode))
                {
                    return (ScriptStatement)scriptNode;
                }
            }
            return null;
        }

        private static bool ExpectStatementEnd(ScriptNode scriptNode)
        {
            return (scriptNode is ScriptIfStatement && !((ScriptIfStatement) scriptNode).IsElseIf)
                   || scriptNode is ScriptForStatement
                   || scriptNode is ScriptCaptureStatement
                   || scriptNode is ScriptWithStatement
                   || scriptNode is ScriptWhileStatement
                   || scriptNode is ScriptWrapStatement
                   || scriptNode is ScriptCaseStatement
                   || scriptNode is ScriptFunction
                   || scriptNode is ScriptAnonymousFunction;
        }

        private void CheckInTagSection()
        {
            if (!_isLiquidTagSection)
            {
                LogError(Current, "Expecting the expression to be in a tag section `{% ... %}`");
            }
        }

        private void ReadLiquidStatement(string identifier, ScriptStatement parent, ref ScriptStatement statement, ref bool hasEnd, ref bool nextStatement)
        {
            bool isNotExpectedInTagSection = true;
            var startToken = Current;
            switch (identifier)
            {
                case "endif":
                    CheckInTagSection();
                    NextToken();

                    hasEnd = true;
                    nextStatement = false;

                    var matchingStatement = FindFirstStatementExpectingEnd() as ScriptIfStatement;
                    if (matchingStatement == null)
                    {
                        LogError(startToken, $"Unable to find a pending `if`/`else` for this `endif`");
                    }

                    ExpectEndOfStatement(matchingStatement);
                    if (_isKeepTrivia)
                    {
                        _trivias.Clear();
                    }
                    break;
                case "endunless":
                    CheckInTagSection();
                    NextToken();

                    hasEnd = true;
                    nextStatement = false;

                    var unless = FindFirstStatementExpectingEnd() as ScriptIfStatement;
                    if (unless == null || !unless.InvertCondition)
                    {
                        LogError(startToken, $"Unable to find a pending `unless` for this `endunless`");
                    }

                    ExpectEndOfStatement(unless);
                    if (_isKeepTrivia)
                    {
                        _trivias.Clear();
                    }
                    break;

                case "endfor":
                    CheckInTagSection();
                    NextToken();

                    hasEnd = true;
                    nextStatement = false;

                    var forStatement = FindFirstStatementExpectingEnd() as ScriptForStatement;
                    if (forStatement == null)
                    {
                        LogError(startToken, $"Unable to find a pending `for` for this `endfor`");
                    }

                    ExpectEndOfStatement(forStatement);
                    if (_isKeepTrivia)
                    {
                        _trivias.Clear();
                    }
                    break;

                case "endcase":
                    CheckInTagSection();
                    NextToken();

                    hasEnd = true;
                    nextStatement = false;

                    var caseStatement = FindFirstStatementExpectingEnd() as ScriptCaseStatement;
                    if (caseStatement == null)
                    {
                        LogError(startToken, $"Unable to find a pending `case` for this `endcase`");
                    }

                    ExpectEndOfStatement(caseStatement);
                    if (_isKeepTrivia)
                    {
                        _trivias.Clear();
                    }
                    break;

                case "endcapture":
                    CheckInTagSection();
                    NextToken();

                    hasEnd = true;
                    nextStatement = false;

                    var captureStatement = FindFirstStatementExpectingEnd() as ScriptCaptureStatement;
                    if (captureStatement == null)
                    {
                        LogError(startToken, $"Unable to find a pending `capture` for this `endcapture`");
                    }

                    ExpectEndOfStatement(captureStatement);
                    if (_isKeepTrivia)
                    {
                        _trivias.Clear();
                    }
                    break;

                case "case":
                    CheckInTagSection();
                    statement = ParseCaseStatement();
                    break;

                case "when":
                    var whenStatement = ParseWhenStatement();
                    CheckInTagSection();
                    var whenParent = parent as ScriptConditionStatement;
                    if (parent is ScriptWhenStatement)
                    {
                        ((ScriptWhenStatement)whenParent).Next = whenStatement;
                    }
                    else if (parent is ScriptCaseStatement)
                    {
                        statement = whenStatement;
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: TODO
                        LogError(startToken, "A `when` condition must be preceded by another `when`/`else`/`case` condition");
                    }
                    hasEnd = true;
                    break;

                case "if":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = ParseIfStatement(false, false);
                    break;

                case "unless":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = ParseIfStatement(true, false);
                    break;

                case "else":
                case "elsif":
                    CheckInTagSection();

                    var nextCondition = ParseElseStatement(identifier == "elsif");
                    var parentCondition = parent as ScriptConditionStatement;
                    if (parent is ScriptIfStatement || parent is ScriptWhenStatement)
                    {
                        if (parent is ScriptIfStatement)
                        {
                            ((ScriptIfStatement)parentCondition).Else = nextCondition;
                        }
                        else
                        {
                            if (identifier == "elseif")
                            {
                                LogError(startToken, "A elsif condition is not allowed within a when/case condition");
                            }
                            ((ScriptWhenStatement)parentCondition).Next = nextCondition;
                        }
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: 201-if-else-error3.txt
                        LogError(startToken, "A else condition must be preceded by another if/else/when condition");
                    }
                    hasEnd = true;
                    break;
                case "for":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = ParseForStatement();
                    break;
                case "break":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = Open<ScriptBreakStatement>();
                    NextToken();
                    Close(statement);

                    ExpectEndOfStatement(statement);
                    break;
                case "continue":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = Open<ScriptContinueStatement>();
                    NextToken();
                    Close(statement);
                    ExpectEndOfStatement(statement);
                    break;
                case "assign":
                {
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    NextToken(); // skip assign
                    
                    var token = _token;
                    // Try to parse an expression
                    var expressionStatement = ParseExpressionStatement();
                    // If we don't have an assign expression, this is not a valid assign
                    if (!(expressionStatement.Expression is ScriptAssignExpression))
                    {
                        LogError(token, "Expecting an assign expression: <variable> = <expression>");
                    }
                    statement = expressionStatement;
                }
                    break;

                case "capture":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = ParseCaptureStatement();
                    break;

                case "increment":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = ParseIncDecStatement(false);
                    break;

                case "decrement":
                    CheckNotInCase(parent, startToken);
                    CheckInTagSection();
                    statement = ParseIncDecStatement(true);
                    break;

                default:
                {
                    CheckNotInCase(parent, startToken);
                    var expressionStatement = ParseExpressionStatement();

                    var functionCall = expressionStatement.Expression as ScriptFunctionCall;
                    bool isInclude = functionCall != null && functionCall.Target is ScriptVariable &&
                                     ((ScriptVariable) functionCall.Target).Name == "include";

                    // Otherwise it is an expression statement
                    if (isInclude)
                    {
                        if (!_isLiquidTagSection)
                        {
                            LogError(startToken, $"The `include` statement must be in a tag section `{{% ... %}}`");
                        }
                    }
                    else if (_isLiquidTagSection)
                    {
                        LogError(startToken, $"Expecting the expression `{GetAsText(startToken)}` to be in an object section `{{{{ ... }}}}`");
                    }
                    statement = expressionStatement;
                    if (!isInclude && expressionStatement.Expression is ScriptAssignExpression)
                    {
                        LogError(statement, $"Assignment expression is not allowed");
                    }
                }
                    break;
            }
        }

        private void CheckIsAssignment(Token token, ScriptExpression expression)
        {
            while (expression != null)
            {
                if (expression is ScriptPipeCall)
                {
                    expression = ((ScriptPipeCall) expression).From;
                }
                else if (expression is ScriptAssignExpression)
                {
                    return;
                }
            }
            LogError(token, "Expecting an assign expression: <variable> = <expression>");
        }

        private T PeekCurrentBlock<T>() where T : ScriptNode
        {
            return Blocks.Count == 0 ? null : Blocks.Peek() as T;
        }

        private void CheckNotInCase(ScriptStatement parent, Token token)
        {
            if (parent is ScriptCaseStatement)
            {
                // 205-case-when-statement-error1.txt
                LogError(token, $"Unexpected statement/expression `{GetAsText(token)}` in the body of a `case` statement. Only `when`/`else` are expected.");
            }
        }

        private ScriptStatement ParseIncDecStatement(bool isDec)
        {
            var incdecStatement = Open<ScriptExpressionStatement>();
            NextToken(); // skip increment/decrement keyword

            var binaryExpression = Open<ScriptBinaryExpression>();
            binaryExpression.Left = ExpectAndParseVariable(incdecStatement);
            binaryExpression.Right = new ScriptLiteral() {Span = binaryExpression.Span, Value = 1};
            binaryExpression.Operator = isDec ? ScriptBinaryOperator.Substract : ScriptBinaryOperator.Add;
            ExpectEndOfStatement(incdecStatement);

            incdecStatement.Expression = binaryExpression;

            Close(binaryExpression);
            return Close(incdecStatement);
        }

        private ScriptReadOnlyStatement ParseReadOnlyStatement()
        {
            var readOnlyStatement = Open<ScriptReadOnlyStatement>();
            NextToken(); // Skip readonly keyword

            readOnlyStatement.Variable = ExpectAndParseVariable(readOnlyStatement);
            ExpectEndOfStatement(readOnlyStatement);

            return Close(readOnlyStatement);
        }

        private ScriptCaptureStatement ParseCaptureStatement()
        {
            var captureStatement = Open<ScriptCaptureStatement>();
            NextToken(); // Skip capture keyword

            // unit test: 231-capture-error1.txt
            captureStatement.Target = ExpectAndParseExpression(captureStatement);
            ExpectEndOfStatement(captureStatement);
            captureStatement.Body = ParseBlockStatement(captureStatement);

            return Close(captureStatement);
        }

        private ScriptImportStatement ParseImportStatement()
        {
            var importStatement = Open<ScriptImportStatement>();
            NextToken(); // skip import

            importStatement.Expression = ExpectAndParseExpression(importStatement);
            ExpectEndOfStatement(importStatement);

            return Close(importStatement);
        }

        private ScriptWithStatement ParseWithStatement()
        {
            var withStatement = Open<ScriptWithStatement>();
            NextToken();
            withStatement.Name = ExpectAndParseExpression(withStatement);

            if (ExpectEndOfStatement(withStatement))
            {
                withStatement.Body = ParseBlockStatement(withStatement);
            }
            return Close(withStatement);
        }

        private ScriptVariable ExpectAndParseVariable(ScriptNode parentNode)
        {
            if (parentNode == null) throw new ArgumentNullException(nameof(parentNode));
            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.IdentifierSpecial)
            {
                var variableOrLiteral = ParseVariableOrLiteral();
                var variable = variableOrLiteral as ScriptVariable;
                if (variable != null && variable.Scope != ScriptVariableScope.Loop)
                {
                    return (ScriptVariable)variableOrLiteral;
                }
                LogError(parentNode, $"Unexpected variable [{variableOrLiteral}]");
            }
            else
            {
                LogError(parentNode, $"Expecting a variable instead of [{Current.Type}]");
            }
            return null;
        }

        private bool ExpectEndOfStatement(ScriptStatement statement)
        {
            if (_isLiquid)
            {
                if (Current.Type == TokenType.CodeExit || (_isLiquidTagSection && Current.Type == TokenType.LiquidTagExit))
                {
                    return true;
                }
            }
            else if (Current.Type == TokenType.NewLine || Current.Type == TokenType.CodeExit || Current.Type == TokenType.SemiColon || Current.Type == TokenType.Eof)
            {
                if (Current.Type == TokenType.NewLine || Current.Type == TokenType.SemiColon)
                {
                    PushTokenToTrivia();
                    NextToken();
                }
                return true;
            }
            // If we are not finding an end of statement, log a fatal error
            LogError(statement, $"Invalid token found `{GetAsText(Current)}`. Expecting <EOL>/end of line after", true);
            return false;
        }

        private void PushTokenToTrivia()
        {
            if (_isKeepTrivia)
            {
                if (Current.Type == TokenType.NewLine)
                {
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.NewLine, _lexer.Text));
                }
                else if (Current.Type == TokenType.SemiColon)
                {
                    _trivias.Add(new ScriptTrivia(CurrentSpan, ScriptTriviaType.SemiColon, _lexer.Text));
                }
            }
        }

        private ScriptFunction ParseFunctionStatement(bool isAnonymous)
        {
            var scriptFunction = Open<ScriptFunction>();
            NextToken(); // skip func or do

            if (!isAnonymous)
            {
                scriptFunction.Name = ExpectAndParseVariable(scriptFunction);
            }
            ExpectEndOfStatement(scriptFunction);

            scriptFunction.Body = ParseBlockStatement(scriptFunction);
            return Close(scriptFunction);
        }

        private ScriptExpressionStatement ParseExpressionStatement()
        {
            var expressionStatement = Open<ScriptExpressionStatement>();
            bool hasAnonymous;
            expressionStatement.Expression = ExpectAndParseExpressionAndAnonymous(expressionStatement, out hasAnonymous);

            // In case of an anonymous, there was already an ExpectEndOfStatement issued for the function
            // so we don't have to verify this here again
            if (!hasAnonymous)
            {
                ExpectEndOfStatement(expressionStatement);
            }
            return Close(expressionStatement);
        }

        private ScriptReturnStatement ParseReturnStatement()
        {
            var ret = Open<ScriptReturnStatement>();
            NextToken(); // skip ret

            if (StartAsExpression())
            {
                ret.Expression = ParseExpression(ret);
            }
            ExpectEndOfStatement(ret);

            return Close(ret);
        }

        private ScriptRawStatement ParseRawStatement()
        {
            var scriptStatement = Open<ScriptRawStatement>();

            // We keep span End here to update it with the raw span
            var spanEnd = Current.End;

            // If we have an escape, we can fetch the escape count
            if (Current.Type == TokenType.Escape)
            {
                NextToken(); // Skip escape
                if (Current.Type < TokenType.EscapeCount1 && Current.Type > TokenType.EscapeCount9)
                {
                    LogError(Current, $"Unexpected token `{GetAsText(Current)}` found. Expecting EscapeCount1-9.");
                }
                else
                {
                    scriptStatement.EscapeCount = (Current.Type - TokenType.EscapeCount1) + 1;
                }
            }

            scriptStatement.Text = _lexer.Text;
            NextToken(); // Skip raw or escape count
            Close(scriptStatement);
            // Because the previous will update the ScriptStatement with the wrong Span End for escape (escapecount1+)
            // We make sure that we use the span end of the Raw token
            scriptStatement.Span.End = spanEnd;
            return scriptStatement;
        }

        private ScriptCaseStatement ParseCaseStatement()
        {
            var caseStatement = Open<ScriptCaseStatement>();
            NextToken(); // skip case

            caseStatement.Value = ExpectAndParseExpression(caseStatement);

            if (ExpectEndOfStatement(caseStatement))
            {
                FlushTrivias(caseStatement.Value, false);
                caseStatement.Body = ParseBlockStatement(caseStatement);
            }

            return Close(caseStatement);
        }

        private ScriptWhenStatement ParseWhenStatement()
        {
            var whenStatement = Open<ScriptWhenStatement>();
            NextToken(); // skip when

            whenStatement.Value = ExpectAndParseExpression(whenStatement);

            if (ExpectEndOfStatement(whenStatement))
            {
                FlushTrivias(whenStatement.Value, false);
                whenStatement.Body = ParseBlockStatement(whenStatement);
            }

            return Close(whenStatement);
        }

        private ScriptIfStatement ParseIfStatement(bool invert, bool isElseIf)
        {
            // unit test: 200-if-else-statement.txt
            var condition = Open<ScriptIfStatement>();
            condition.IsElseIf = isElseIf;
            condition.InvertCondition = invert;
            NextToken(); // skip if

            condition.Condition = ExpectAndParseExpression(condition);

            if (ExpectEndOfStatement(condition))
            {
                FlushTrivias(condition.Condition, false);
                condition.Then = ParseBlockStatement(condition);
            }

            return Close(condition);
        }
        private ScriptWrapStatement ParseWrapStatement()
        {
            var wrapStatement = Open<ScriptWrapStatement>();
            NextToken(); // skip wrap

            wrapStatement.Target = ExpectAndParseExpression(wrapStatement);

            if (ExpectEndOfStatement(wrapStatement))
            {
                FlushTrivias(wrapStatement.Target, false);
                wrapStatement.Body = ParseBlockStatement(wrapStatement);
            }

            return Close(wrapStatement);
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null)
        {
            if (StartAsExpression())
            {
                return ParseExpression(parentNode, parentExpression, newPrecedence);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of [{Current.Type}]" );
            return null;
        }

        private ScriptExpression ExpectAndParseExpressionAndAnonymous(ScriptNode parentNode, out bool hasAnonymousFunction)
        {
            hasAnonymousFunction = false;
            if (StartAsExpression())
            {
                return ParseExpression(parentNode, ref hasAnonymousFunction);
            }
            LogError(parentNode, CurrentSpan, $"Expecting <expression> instead of [{Current.Type}]");
            return null;
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ref bool hasAnonymousExpression, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null)
        {
            if (StartAsExpression())
            {
                return ParseExpression(parentNode, ref hasAnonymousExpression, parentExpression, newPrecedence);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of [{Current.Type}]");
            return null;
        }

        private ScriptConditionStatement ParseElseStatement(bool isElseIf)
        {
            // Case of elsif
            if (_isLiquid && isElseIf)
            {
                return ParseIfStatement(false, true);
            }

            // unit test: 200-if-else-statement.txt
            var nextToken = PeekToken();
            if (!_isLiquid && nextToken.Type == TokenType.Identifier && GetAsText(nextToken) == "if")
            {
                NextToken();

                if (_isKeepTrivia)
                {
                    // We don't store the trivias here
                    _trivias.Clear();
                }
                return ParseIfStatement(false, true);
            }

            var elseStatement = Open<ScriptElseStatement>();
            NextToken(); // skip else

            // unit test: 201-if-else-error4.txt
            if (ExpectEndOfStatement(elseStatement))
            {
                elseStatement.Body = ParseBlockStatement(elseStatement);
            }
            return Close(elseStatement);
        }

        private ScriptForStatement ParseForStatement()
        {
            var forStatement = Open<ScriptForStatement>();
            NextToken(); // skip for

            // unit test: 211-for-error1.txt
            forStatement.Variable = ExpectAndParseVariable(forStatement);

            if (forStatement.Variable != null)
            {
                // in 
                if (Current.Type != TokenType.Identifier || GetAsText(Current) != "in")
                {
                    // unit test: 211-for-error2.txt
                    LogError(forStatement, $"Expecting 'in' word instead of [{Current.Type} {GetAsText(Current)}]");
                }
                else
                {
                    NextToken(); // skip in
                }

                // unit test: 211-for-error3.txt
                forStatement.Iterator = ExpectAndParseExpression(forStatement);

                if (ExpectEndOfStatement(forStatement))
                {
                    FlushTrivias(forStatement.Iterator, false);
                    forStatement.Body = ParseBlockStatement(forStatement);
                }
            }

            return Close(forStatement);
        }

        private ScriptWhileStatement ParseWhileStatement()
        {
            var whileStatement = Open<ScriptWhileStatement>();
            NextToken(); // Skip while

            // Parse the condition
            // unit test: 220-while-error1.txt
            whileStatement.Condition = ExpectAndParseExpression(whileStatement);

            if (ExpectEndOfStatement(whileStatement))
            {
                FlushTrivias(whileStatement.Condition, false);
                whileStatement.Body = ParseBlockStatement(whileStatement);
            }

            return Close(whileStatement);
        }

        private ScriptBlockStatement ParseBlockStatement(ScriptStatement parentStatement)
        {
            Blocks.Push(parentStatement);

            _blockLevel++;
            if (Options.StatementDepthLimit.HasValue && !_isStatementDepthLimitReached && _blockLevel > Options.StatementDepthLimit.Value)
            {
                LogError(parentStatement, GetSpanForToken(Previous), $"The statement depth limit `{Options.StatementDepthLimit.Value}` was reached when parsing this statement");
                _isStatementDepthLimitReached = true;
            }

            var blockStatement = parentStatement is ScriptBlockStatement
                ? (ScriptBlockStatement) parentStatement
                : Open<ScriptBlockStatement>();

            ScriptStatement statement;
            bool hasEnd;
            while (TryParseStatement(parentStatement, out statement, out hasEnd))
            {
                // statement may be null if we have parsed an else continuation of a previous block
                if (statement != null)
                {
                    blockStatement.Statements.Add(statement);
                }
                if (hasEnd)
                {
                    break;
                }
            }

            if (!hasEnd)
            {
                // If there are any end block not matching, we have an error
                if (_blockLevel > 1)
                {
                    if (_isLiquid)
                    {
                        var syntax = ScriptSyntaxAttribute.Get(parentStatement);
                        LogError(parentStatement, parentStatement.Span, $"The `end{syntax.Name}` was not found");
                    }
                    else
                    {
                        // unit test: 201-if-else-error2.txt
                        LogError(parentStatement, GetSpanForToken(Previous), $"The <end> statement was not found");
                    }
                }
            }

            _blockLevel--;

            Blocks.Pop();
            return Close(blockStatement);
        }

        private bool IsInLoop()
        {
            foreach (var block in Blocks)
            {
                if (block is ScriptLoopStatementBase)
                {
                    return true;
                }
            }
            return false;
        }

        private T Open<T>() where T : ScriptNode, new()
        {
            var element = new T() { Span = {FileName = _lexer.SourcePath, Start = Current.Start}};
            FlushTrivias(element, true);
            return element;
        }

        private void FlushTrivias(ScriptNode element, bool isBefore)
        {
            if (_isKeepTrivia && _trivias.Count > 0 && !(element is ScriptBlockStatement))
            {
                element.AddTrivias(_trivias, isBefore);
                _trivias.Clear();
            }
        }

        private T Close<T>(T statement) where T : ScriptNode
        {
            statement.Span.End = Previous.End;
            FlushTrivias(statement, false);
            return statement;
        }

        private string GetAsText(Token localToken)
        {
            return localToken.GetText(_lexer.Text);
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

                case TokenType.NewLine:
                    type = ScriptTriviaType.Whitespace;
                    break;
                default:
                    throw new InvalidOperationException($"Token type `{token.Type}` not supported by trivia");
            }

            var trivia = new ScriptTrivia(GetSpanForToken(token), type,  _lexer.Text);
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

        private bool IsHidden(TokenType tokenType)
        {
            return tokenType == TokenType.Comment || tokenType == TokenType.CommentMulti || tokenType == TokenType.Whitespace || (tokenType == TokenType.NewLine && allowNewLineLevel > 0);
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
            LogError(span, $"Error while parsing {syntax.Name}: {message}{inMessage}: {syntax.Example}", isFatal);
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

        private bool IsKeyword(string text)
        {
            if (_isLiquid)
            {
                switch (text)
                {
                    case "assign":
                    case "if":
                    case "else":
                    case "elsif":
                    case "endif":
                    case "for":
                    case "endfor":
                    case "case":
                    case "when":
                    case "endcase":
                    case "break":
                    case "continue":
                    case "unless":
                    case "endunless":
                    case "capture":
                    case "endcapture":
                    case "increment":
                    case "decrement":
                        return true;
                }
            }
            else
            {
                switch (text)
                {
                    case "if":
                    case "else":
                    case "end":
                    case "for":
                    case "while":
                    case "break":
                    case "continue":
                    case "func":
                    case "import":
                    case "readonly":
                    case "with":
                    case "capture":
                    case "ret":
                    case "wrap":
                    case "do":
                        return true;
                }
            }
            return false;
        }
    }
}