// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.CompilerServices;
using Scriban.Helpers;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Parsing
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class Parser
    {
        private ScriptBlockStatement _currentBlockStatement;

        private ScriptBlockStatement ParseBlockStatement(ScriptNode parentStatement, bool parseEndOfStatementAfterEnd = true)
        {
            Debug.Assert(!(parentStatement is ScriptBlockStatement));

            Blocks.Push(parentStatement);

            _blockLevel++;
            EnterExpression();

            var blockStatement = Open<ScriptBlockStatement>();
            var previousBlockStatement = _currentBlockStatement;
            _currentBlockStatement = blockStatement;
            try
            {
                ScriptStatement statement;
                bool hasEnd;
                while (TryParseStatement(parentStatement, parseEndOfStatementAfterEnd, out statement, out hasEnd))
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

                // Don't emit an EOS for an end statement that doesn't expect it
                if (!parseEndOfStatementAfterEnd && statement is ScriptEndStatement endStatement)
                {
                    endStatement.ExpectEos = false;
                }

                if (!hasEnd)
                {
                    // If there are any end block not matching, we have an error
                    if (_blockLevel > 1)
                    {
                        if (_isLiquid)
                        {
                            var syntax = ScriptSyntaxAttribute.Get(parentStatement);
                            LogError(parentStatement, parentStatement?.Span ?? CurrentSpan, $"The `end{syntax.TypeName}` was not found");
                        }
                        else
                        {
                            // unit test: 201-if-else-error2.txt
                            LogError(parentStatement, GetSpanForToken(Previous), $"The <end> statement was not found");
                        }
                    }
                }
            }
            finally
            {
                _currentBlockStatement = previousBlockStatement;
                LeaveExpression();
                _blockLevel--;
            }


            Blocks.Pop();
            return Close(blockStatement);
        }

        private bool TryParseStatement(ScriptNode parent, bool parseEndOfStatementAfterEnd, out ScriptStatement statement, out bool hasEnd)
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
                    if (parent is ScriptCaseStatement)
                    {
                        // In case we have a raw statement within directly a case
                        // we don't keep it
                        statement = null;
                        goto continueParsing;
                    }
                    break;

                case TokenType.CodeEnter:
                case TokenType.LiquidTagEnter:
                case TokenType.CodeExit:
                case TokenType.LiquidTagExit:
                case TokenType.EscapeEnter:
                case TokenType.EscapeExit:
                    statement = ParseEscapeStatement();
                    break;

                case TokenType.FrontMatterMarker:
                    if (_inFrontMatter)
                    {
                        _inFrontMatter = false;
                        _inCodeSection = false;

                        // Parse the frontmatter end-marker
                        ExpectAndParseTokenTo(_frontmatter.EndMarker, TokenType.FrontMatterMarker);

                        Close(_frontmatter);

                        _frontmatter.TextPositionAfterEndMarker = Current.Start;

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
                                    ParseLiquidStatement(identifier, parent, ref statement, ref hasEnd, ref nextStatement);
                                }
                                else
                                {
                                    ParseScribanStatement(identifier, parent, parseEndOfStatementAfterEnd, ref statement, ref hasEnd, ref nextStatement);
                                }
                                break;
                            default:
                                if (IsStartOfExpression())
                                {
                                    statement = ParseExpressionStatement();
                                }
                                else
                                {
                                    nextStatement = false;
                                    LogError($"Unexpected token {GetAsText(Current)}");
                                }
                                break;
                        }
                    }
                    else
                    {
                        nextStatement = false;
                        LogError($"Unexpected token {GetAsText(Current)} while not in a code block {{ ... }}");
                        // LOG an ERROR. Don't expect any other tokens outside a code section
                    }
                    break;
            }

            return nextStatement;
        }

        private ScriptCaptureStatement ParseCaptureStatement()
        {
            var captureStatement = Open<ScriptCaptureStatement>();
            ExpectAndParseKeywordTo(captureStatement.CaptureKeyword); // Parse capture keyword
            // unit test: 231-capture-error1.txt
            captureStatement.Target = ExpectAndParseExpression(captureStatement);
            ExpectEndOfStatement();
            captureStatement.Body = ParseBlockStatement(captureStatement);

            return Close(captureStatement);
        }

        private ScriptEscapeStatement ParseEscapeStatement()
        {
            bool isCodeEnter = Current.Type == TokenType.CodeEnter || Current.Type == TokenType.LiquidTagEnter;
            bool isEscape = Current.Type == TokenType.EscapeEnter || Current.Type == TokenType.EscapeExit;
            bool isEnter = isCodeEnter || Current.Type == TokenType.EscapeEnter;
            bool isLiquid = Current.Type == TokenType.LiquidTagEnter || Current.Type == TokenType.LiquidTagExit;
            Debug.Assert(isEnter || Current.Type == TokenType.CodeExit || Current.Type == TokenType.LiquidTagExit || isEscape);

            // Log errors depending on if we are already in a code section or not
            if (isCodeEnter)
            {
                if (_inCodeSection)
                {
                    LogError("Unexpected token while already in a code block");
                }
            }
            else if (!isEscape)
            {
                if (!_inCodeSection)
                {
                    LogError("Unexpected code block exit '}}' while no code block enter '{{' has been found");
                }
                else if (CurrentParsingMode == ScriptMode.ScriptOnly)
                {
                    LogError("Unexpected code clock exit '}}' while parsing in script only mode. '}}' is not allowed.");
                }
            }

            _isLiquidTagSection = isCodeEnter && isLiquid;
            _inCodeSection = isCodeEnter;

            var scriptEscapeStatement = Open<ScriptEscapeStatement>();
            scriptEscapeStatement.IsEntering = isEnter;

            var tokenText = GetAsText(Current);
            var whitespaceChar = tokenText[isEnter ? tokenText.Length - 1 : 0];
            scriptEscapeStatement.WhitespaceMode = whitespaceChar switch
            {
                '-' => ScriptWhitespaceMode.Greedy,
                '~' => ScriptWhitespaceMode.NonGreedy,
                _ => ScriptWhitespaceMode.None,
            };

            if (isEscape)
            {
                if (_isLiquid)
                {
                    scriptEscapeStatement.EscapeCount = 6;
                }
                else
                {
                    int escapeCount = tokenText.Length - 2; // minus opening and closing {%{
                    if (whitespaceChar != (isEnter ? '{' : '}')) escapeCount--;
                    scriptEscapeStatement.EscapeCount = escapeCount;
                }
            }
            NextToken(); // Skip enter/exit token

            // Check indent
            if (isCodeEnter && _currentBlockStatement.Statements.Count > 0)
            {
                var previousStatement = _currentBlockStatement.Statements[_currentBlockStatement.Statements.Count - 1];
                if (previousStatement is ScriptRawStatement rawStatement)
                {
                    string indent = null;

                    // Iterate on the original string to detect \n
                    var slice = rawStatement.Text;
                    var text = slice.FullText;
                    var end = slice.Index + slice.Length - 1;
                    for (int j = end; j >= 0; j--)
                    {
                        var c = text[j];
                        if (c == '\n')
                        {
                            indent = text.Substring(j + 1, end - j);
                            break;
                        }

                        if (!char.IsWhiteSpace(c))
                        {
                            break;
                        }

                        if (j == 0)
                        {
                            // We have a raw statement that has only white spaces
                            // It could be the first raw statement of the document
                            // so we continue but we handle it later
                            indent = text.Substring(0, end + 1);
                        }
                    }
                    scriptEscapeStatement.Indent = indent;
                }
            }

            return Close(scriptEscapeStatement);
        }

        private ScriptCaseStatement ParseCaseStatement()
        {
            var caseStatement = Open<ScriptCaseStatement>();
            ExpectAndParseKeywordTo(caseStatement.CaseKeyword); // Parse case keyword
            caseStatement.Value = ExpectAndParseExpression(caseStatement, allowAssignment: false);

            if (ExpectEndOfStatement())
            {
                caseStatement.Body = ParseBlockStatement(caseStatement);
            }

            return Close(caseStatement);
        }

        private ScriptConditionStatement ParseElseStatement(bool isElseIf)
        {
            // Case of elsif
            if (_isLiquid && isElseIf)
            {
                return ParseIfStatement(false, ScriptKeyword.Else());
            }

            // unit test: 200-if-else-statement.txt
            var nextToken = PeekToken();
            if (!_isLiquid && nextToken.Type == TokenType.Identifier && GetAsText(nextToken) == "if")
            {
                var elseKeyword = ScriptKeyword.Else();
                ExpectAndParseKeywordTo(elseKeyword);
                return ParseIfStatement(false, elseKeyword);
            }

            var elseStatement = Open<ScriptElseStatement>();
            ExpectAndParseKeywordTo(elseStatement.ElseKeyword); // Parse else statement

            // unit test: 201-if-else-error4.txt
            if (ExpectEndOfStatement())
            {
                elseStatement.Body = ParseBlockStatement(elseStatement);
            }
            return Close(elseStatement);
        }

        private ScriptStatement ParseExpressionStatement()
        {
            var expressionStatement = Open<ScriptExpressionStatement>();

            var expression = TransformKeyword(ExpectAndParseExpressionAndAnonymous(expressionStatement));

            // Special case, if the expression return should be converted back to a statement
            if (expression is ScriptExpressionAsStatement expressionAsStatement)
            {
                return expressionAsStatement.Statement;
            }
            expressionStatement.Expression = expression;

            ExpectEndOfStatement();
            return Close(expressionStatement);
        }

        private T ParseForStatement<T>() where T : ScriptForStatement, new()
        {
            var forStatement = Open<T>();
            ExpectAndParseKeywordTo(forStatement.ForOrTableRowKeyword); // Parse for or tablerow keyword

            // unit test: 211-for-error1.txt
            forStatement.Variable = ExpectAndParseExpression(forStatement, mode: ParseExpressionMode.BasicExpression);

            if (forStatement.Variable != null)
            {
                if (!(forStatement.Variable is IScriptVariablePath))
                {
                    LogError(forStatement, $"Expecting a variable instead of `{forStatement.Variable}`");
                }

                // in
                if (Current.Type != TokenType.Identifier || GetAsText(Current) != "in")
                {
                    // unit test: 211-for-error2.txt
                    LogError(forStatement, $"Expecting 'in' word instead of `{GetAsText(Current)}`");
                }
                else
                {
                    ExpectAndParseKeywordTo(forStatement.InKeyword); // Parse in keyword
                }

                // unit test: 211-for-error3.txt
                forStatement.Iterator = ExpectAndParseExpression(forStatement);

                if (ExpectEndOfStatement())
                {
                    forStatement.Body = ParseBlockStatement(forStatement);
                }
            }

            return Close(forStatement);
        }

        private ScriptIfStatement ParseIfStatement(bool invert, ScriptKeyword elseKeyword = null)
        {
            // unit test: 200-if-else-statement.txt
            var ifStatement = Open<ScriptIfStatement>();
            ifStatement.ElseKeyword = elseKeyword;

            if (_isLiquid && elseKeyword != null)
            {
                // Parse elseif
                Open(ifStatement.IfKeyword);
                NextToken();
                Close(ifStatement.IfKeyword);
            }
            else
            {
                if (_isLiquid && invert) // we have an unless
                {
                    Open(ifStatement.IfKeyword); // still transfer trivias to IfKeyword
                    NextToken();
                    Close(ifStatement.IfKeyword);
                }
                else
                {
                    ExpectAndParseKeywordTo(ifStatement.IfKeyword); // Parse if keyword
                }
            }

            var condition =  ExpectAndParseExpression(ifStatement, allowAssignment: false);

            // Transform a `if condition` to `if !(condition)`
            if (invert)
            {
                var invertCondition = ScriptUnaryExpression.Wrap(ScriptUnaryOperator.Not, ScriptToken.Exclamation(), ScriptNestedExpression.Wrap(condition, _isKeepTrivia), _isKeepTrivia);
                condition = invertCondition;
            }

            ifStatement.Condition = condition;

            if (ExpectEndOfStatement())
            {
                ifStatement.Then = ParseBlockStatement(ifStatement);
            }

            return Close(ifStatement);
        }

        private ScriptRawStatement ParseRawStatement()
        {
            bool isEscape = Current.Type == TokenType.Escape;

            var scriptStatement = Open<ScriptRawStatement>();

            scriptStatement.IsEscape = isEscape;

            // We keep span End here to update it with the raw span
            var spanStart = Current.Start;
            var spanEnd = Current.End;


            NextToken(); // Skip raw or escape count
            Close(scriptStatement);
            // Because the previous will update the ScriptStatement with the wrong Span End for escape (escapecount1+)
            // We make sure that we use the span end of the Raw token
            scriptStatement.Span.End = spanEnd;

            // Update the index of the slice/length
            scriptStatement.Text = new ScriptStringSlice(_lexer.Text, spanStart.Offset, spanEnd.Offset - spanStart.Offset + 1);

            return scriptStatement;
        }

        private ScriptWhenStatement ParseWhenStatement()
        {
            var whenStatement = Open<ScriptWhenStatement>();
            ExpectAndParseKeywordTo(whenStatement.WhenKeyword); // Parse when keyword

            // Parse the when values
            // - a, b, c
            // - a || b || c (scriban)
            // - a or b or c (liquid)
            while (true)
            {
                if (IsStartOfExpression())
                {
                    var constantExpression = ParseExpression(whenStatement, mode:ParseExpressionMode.WhenExpression, allowAssignment:false);
                    whenStatement.Values.Add(constantExpression);
                }
                else
                {
                    break;
                }

                if (Current.Type == TokenType.Comma || (!_isLiquid && Current.Type == TokenType.DoubleVerticalBar) || (_isLiquid && GetAsText(Current) == "or"))
                {
                    NextToken();
                }
            }

            if (whenStatement.Values.Count == 0)
            {
                LogError(Current, "When is expecting at least one value.");
            }

            if (ExpectEndOfStatement())
            {
                whenStatement.Body = ParseBlockStatement(whenStatement);
            }

            return Close(whenStatement);
        }

        private void CheckNotInCase(ScriptNode parent, Token token)
        {
            if (parent is ScriptCaseStatement)
            {
                // 205-case-when-statement-error1.txt
                LogError(token, $"Unexpected statement/expression `{GetAsText(token)}` in the body of a `case` statement. Only `when`/`else` are expected.");
            }
        }

        private ScriptVariable ExpectAndParseVariable(ScriptNode parentNode)
        {
            if (parentNode == null) throw new ArgumentNullException(nameof(parentNode));
            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.IdentifierSpecial)
            {
                var variableOrLiteral = ParseVariable();
                var variable = variableOrLiteral as ScriptVariable;
                if (variable != null)
                {
                    return variable;
                }
                LogError(parentNode, $"Unexpected variable `{variableOrLiteral}`");
            }
            else
            {
                LogError(parentNode, $"Expecting a variable instead of `{GetAsText(Current)}`");
            }
            return null;
        }

        private bool ExpectEndOfStatement()
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
                    FlushTriviasToLastTerminal();
                    NextToken();
                }
                return true;
            }
            // If we are not finding an end of statement, log a fatal error
            LogError(CurrentSpan, $"Invalid token found `{GetAsText(Current)}`. Expecting <EOL>/end of line.", true);
            return false;
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
            return (scriptNode is ScriptIfStatement && !((ScriptIfStatement)scriptNode).IsElseIf)
                   || scriptNode is ScriptForStatement
                   || scriptNode is ScriptCaptureStatement
                   || scriptNode is ScriptWithStatement
                   || scriptNode is ScriptWhileStatement
                   || scriptNode is ScriptWrapStatement
                   || scriptNode is ScriptCaseStatement
                   || scriptNode is ScriptFunction
                   || scriptNode is ScriptAnonymousFunction;
        }

        /// <summary>
        /// Used internally to transform an expression into a statement
        /// </summary>
        private partial class ScriptExpressionAsStatement : ScriptExpression
        {
            public ScriptExpressionAsStatement(ScriptStatement statement)
            {
                Statement = statement;
            }

            public ScriptStatement Statement { get; }

            public override object Evaluate(TemplateContext context)
            {
                throw new NotSupportedException();
            }

            public override void PrintTo(ScriptPrinter printer)
            {
                throw new NotSupportedException();
            }

            public override void Accept(ScriptVisitor visitor)
            {
                throw new NotSupportedException();
            }

            public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor)
            {
                throw new NotSupportedException();
            }
        }
    }
}