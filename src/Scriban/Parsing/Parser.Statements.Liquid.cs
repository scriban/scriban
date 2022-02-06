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
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class Parser
    {
        private void ParseLiquidStatement(string identifier, ScriptNode parent, ref ScriptStatement statement, ref bool hasEnd, ref bool nextStatement)
        {
            var startToken = Current;
            if (!_isLiquidTagSection)
            {
                statement = ParseLiquidExpressionStatement(parent);
                return;
            }

            if (identifier != "when" && identifier != "case" && !identifier.StartsWith("end") && parent is ScriptCaseStatement)
            {
                // 205-case-when-statement-error1.txt
                LogError(startToken, $"Unexpected statement/expression `{GetAsText(startToken)}` in the body of a `case` statement. Only `when`/`else` are expected.");
            }

            ScriptStatement startStatement = null;
            string pendingStart = null;
            switch (identifier)
            {
                case "endif":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptIfStatement;
                    pendingStart = "`if`/`else`";
                    // Handle after the switch
                    break;

                case "endifchanged":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptIfStatement;
                    pendingStart = "`ifchanged`";
                    // Handle after the switch
                    break;

                case "endunless":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptIfStatement;
                    pendingStart = "`unless`";
                    break;

                case "endfor":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptForStatement;
                    pendingStart = "`for`";
                    break;

                case "endcase":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptCaseStatement;
                    pendingStart = "`case`";
                    break;

                case "endcapture":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptCaptureStatement;
                    pendingStart = "`capture`";
                    break;

                case "endtablerow":
                    startStatement = FindFirstStatementExpectingEnd() as ScriptTableRowStatement;
                    pendingStart = "`tablerow`";
                    break;

                case "case":
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

                case "if":
                    statement = ParseIfStatement(false);
                    break;

                case "ifchanged":
                    statement = ParseLiquidIfChanged();
                    break;

                case "unless":
                    CheckNotInCase(parent, startToken);
                    statement = ParseIfStatement(true);
                    break;

                case "else":
                case "elsif":
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
                    else if (identifier == "else" && parent is ScriptForStatement forStatement)
                    {
                        forStatement.Else = (ScriptElseStatement)nextCondition;
                    }
                    else
                    {
                        nextStatement = false;

                        // unit test: 201-if-else-error3.txt
                        LogError(startToken,
                            identifier == "else"
                                ? "A else condition must be preceded by another if/else/unless condition or a for loop."
                                : "A else condition must be preceded by another if/else/unless.");
                    }
                    hasEnd = true;
                    break;
                case "for":
                    var localForStatement = ParseForStatement<ScriptForStatement>();
                    localForStatement.SetContinue = true;
                    statement = localForStatement;
                    break;
                case "tablerow":
                    statement = ParseForStatement<ScriptTableRowStatement>();
                    break;
                case "cycle":
                    statement = ParseLiquidCycleStatement();
                    break;
                case "break":
                    var breakStatement = Open<ScriptBreakStatement>();
                    statement = breakStatement;
                    ExpectAndParseKeywordTo(breakStatement.BreakKeyword);
                    ExpectEndOfStatement();
                    Close(statement);
                    break;
                case "continue":
                    var continueStatement =  Open<ScriptContinueStatement>();
                    statement = continueStatement;
                    ExpectAndParseKeywordTo(continueStatement.ContinueKeyword); // Parse continue keyword
                    ExpectEndOfStatement();
                    FlushTriviasToLastTerminal();
                    Close(statement);
                    break;
                case "assign":
                    {
                        if (_isKeepTrivia)
                        {
                            _trivias.Clear();
                        }
                        NextToken(); // skip assign

                        var token = _token;
                        // Try to parse an expression
                        var expressionStatement = ParseExpressionStatement();
                        // If we don't have an assign expression, this is not a valid assign
                        if (!(expressionStatement is ScriptExpressionStatement exprStatementFinal && exprStatementFinal.Expression is ScriptAssignExpression))
                        {
                            LogError(token, "Expecting an assign expression: <variable> = <expression>");
                        }
                        statement = expressionStatement;
                    }
                    break;

                case "capture":
                    statement = ParseCaptureStatement();
                    break;

                case "increment":
                    statement = ParseLiquidIncDecStatement(false);
                    break;

                case "decrement":
                    statement = ParseLiquidIncDecStatement(true);
                    break;

                case "include":
                    statement = ParseLiquidIncludeStatement();
                    break;

                default:
                    statement = ParseLiquidExpressionStatement(parent);
                    break;
            }

            if (pendingStart != null)
            {
                var endStatement = Open<ScriptEndStatement>();
                NextToken();
                statement =  Close(endStatement);

                hasEnd = true;
                nextStatement = true;

                if (startStatement == null)
                {
                    LogError(startToken, $"Unable to find a pending {pendingStart} for this `{identifier}`");
                }
            }
        }

        private ScriptExpressionStatement ParseLiquidCycleStatement()
        {
            var statement = Open<ScriptExpressionStatement>();
            var functionCall = Open<ScriptFunctionCall>();
            statement.Expression = functionCall;
            functionCall.Target = ParseVariable();

            if (Options.LiquidFunctionsToScriban)
            {
                TransformLiquidFunctionCallToScriban(functionCall);
            }

            ScriptArrayInitializerExpression arrayInit = null;

            // Parse cycle without group: cycle "a", "b", "c" => transform to scriban: array.cycle ["a", "b", "c"]
            // Parse cycle with group: cycle "group1": "a", "b", "c" => transform to scriban: array.cycle ["a", "b", "c"] "group1"

            bool isFirst = true;
            while (IsVariableOrLiteral(Current))
            {
                var value = ParseVariableOrLiteral();

                if (isFirst && Current.Type == TokenType.Colon)
                {
                    var namedArg = Open<ScriptNamedArgument>();
                    namedArg.Name = new ScriptVariableGlobal("group");

                    namedArg.ColonToken = ScriptToken.Colon();
                    ExpectAndParseTokenTo(namedArg.ColonToken, TokenType.Colon); // Parse :

                    namedArg.Value = value;
                    Close(namedArg);
                    namedArg.Span = value.Span;

                    isFirst = false;
                    functionCall.Arguments.Add(namedArg);
                    continue;
                }

                if (arrayInit == null)
                {
                    arrayInit = Open<ScriptArrayInitializerExpression>();
                    functionCall.Arguments.Insert(0, arrayInit);
                    arrayInit.Span.Start = value.Span.Start;
                }

                arrayInit.Values.Add(value);
                arrayInit.Span.End = value.Span.End;

                if (Current.Type == TokenType.Comma)
                {
                    NextToken();
                }
                else if (Current.Type == TokenType.LiquidTagExit)
                {
                    break;
                }
                else
                {
                    LogError(Current, $"Unexpected token `{GetAsText(Current)}` after cycle value `{value}`. Expecting a `,`");
                    NextToken();
                    break;
                }
            }

            Close(functionCall);

            ExpectEndOfStatement();
            return Close(statement);
        }

        private ScriptStatement ParseLiquidExpressionStatement(ScriptNode parent)
        {
            var startToken = Current;
            CheckNotInCase(parent, startToken);
            var expressionStatement = ParseExpressionStatement();

            var statement = expressionStatement;

            // NOTE: We were previously performing the following checks
            // but as liquid doesn't have a strict syntax, we are instead not enforcing anykind of rules
            // so that the parser can still read custom liquid tags/object expressions, assuming that
            // they are not using fancy argument syntaxes (which are unfortunately allowed in liquid)

            //var functionCall = expressionStatement.Expression as ScriptFunctionCall;
            //// Otherwise it is an expression statement
            //if (functionCall != null)
            //{
            //    if (!_isLiquidTagSection)
            //    {
            //        LogError(startToken, $"The `{functionCall}` statement must be in a tag section `{{% ... %}}`");
            //    }
            //}
            //else if (_isLiquidTagSection)
            //{
            //    LogError(startToken, $"Expecting the expression `{GetAsText(startToken)}` to be in an object section `{{{{ ... }}}}`");
            //}
            //else if (!(expressionStatement.Expression is IScriptVariablePath || expressionStatement.Expression is ScriptPipeCall))
            //{
            //    LogError(statement, $"The <{expressionStatement.Expression}> is not allowed in this context");
            //}
            return statement;
        }

        private ScriptStatement ParseLiquidIfChanged()
        {
            var statement = Open<ScriptIfStatement>();
            statement.IfKeyword.Span = CurrentSpan;
            NextToken(); // skip ifchanged token
            statement.Condition = new ScriptMemberExpression() { Target = ScriptVariable.Create(ScriptVariable.ForObject.BaseName, ScriptVariableScope.Global), Member = ScriptVariable.Create("changed", ScriptVariableScope.Global) };
            statement.Then = ParseBlockStatement(statement);
            Close(statement);
            statement.Condition.Span = statement.Span;
            return statement;
        }

        private ScriptStatement ParseLiquidIncDecStatement(bool isDec)
        {
            var incdecStatement = Open<ScriptExpressionStatement>();
            NextToken(); // skip increment/decrement keyword

            var binaryExpression = Open<ScriptBinaryExpression>();
            binaryExpression.Left = ExpectAndParseVariable(incdecStatement);
            binaryExpression.Right = new ScriptLiteral() {Span = binaryExpression.Span, Value = 1};
            binaryExpression.Operator = isDec ? ScriptBinaryOperator.Subtract : ScriptBinaryOperator.Add;
            ExpectEndOfStatement();

            incdecStatement.Expression = binaryExpression;

            Close(binaryExpression);
            return Close(incdecStatement);
        }

        private ScriptStatement ParseLiquidIncludeStatement()
        {
            var include = Open<ScriptFunctionCall>();
            include.Target = ParseVariable();

            var templateNameToken = Current;
            var templateName = ExpectAndParseExpression(include, mode: ParseExpressionMode.BasicExpression);
            if (templateName != null)
            {
                var literal = templateName as ScriptLiteral;
                if (!(literal?.Value is string || templateName is IScriptVariablePath))
                {
                    LogError(templateNameToken, $"Unexpected include template name `{templateName}` expecting a string or a variable path");
                }

                include.Arguments.Add(templateName);
            }
            Close(include);

            var includeStatement = new ScriptExpressionStatement() {Span = include.Span, Expression = include};

            ScriptForStatement forStatement = null;
            ScriptBlockStatement block = null;

            if (Current.Type == TokenType.Identifier)
            {
                var next = GetAsText(Current);
                // Parse with <value>
                // Create a block statement equivalent:
                // this[target] = value;
                // include target
                if (next == "with")
                {
                    NextToken(); // skip with

                    var assignExpression = Open<ScriptAssignExpression>();
                    assignExpression.Target = new ScriptIndexerExpression()
                    {
                        Target = new ScriptThisExpression {Span = CurrentSpan},
                        Index = (ScriptExpression)templateName?.Clone(),
                    };

                    assignExpression.Value = ExpectAndParseExpression(include, mode: ParseExpressionMode.BasicExpression);
                    Close(assignExpression);

                    block = new ScriptBlockStatement {Span = include.Span};
                    block.Statements.Add(new ScriptExpressionStatement()
                    {
                        Span = assignExpression.Span,
                        Expression = assignExpression
                    });
                    block.Statements.Add(includeStatement);
                    Close(block);
                }
                else if (next == "for")
                {
                    // Create a block statement equivalent:
                    // for this[target] in value
                    //  include target
                    // end

                    NextToken(); // skip for

                    forStatement = Open<ScriptForStatement>();
                    forStatement.Variable = new ScriptIndexerExpression()
                    {
                        Target = new ScriptThisExpression {Span = CurrentSpan},
                        Index = (ScriptExpression)templateName?.Clone(),
                    };

                    forStatement.Iterator = ExpectAndParseExpression(include, mode: ParseExpressionMode.BasicExpression);

                    forStatement.Body = new ScriptBlockStatement() {Span = include.Span};
                    forStatement.Body.Statements.Add(includeStatement);
                    forStatement.Body.Statements.Add(new ScriptEndStatement());
                    Close(forStatement);
                }

                // For following variable separated by colon, add them as assignment before the include
                while (Current.Type == TokenType.Identifier)
                {
                    var variableToken = Current;
                    var variableObject = ParseVariable();

                    var variable = variableObject as ScriptVariable;
                    if (variable == null)
                    {
                        LogError(variableToken, $"Unexpected variable name `{GetAsText(variableToken)}` found in include parameter");
                    }

                    if (Current.Type == TokenType.Colon)
                    {
                        NextToken(); // skip :
                    }
                    else
                    {
                        LogError(Current, $"Unexpected token `{GetAsText(Current)}` after variable `{variable}`. Expecting a `:`");
                    }

                    if (block == null)
                    {
                        block = new ScriptBlockStatement {Span = include.Span};
                        block.Statements.Add(includeStatement);
                    }

                    var assignExpression = Open<ScriptAssignExpression>();
                    assignExpression.Target = variable;
                    assignExpression.Value = ExpectAndParseExpression(include, mode: ParseExpressionMode.BasicExpression);

                    block.Statements.Insert(0, new ScriptExpressionStatement()
                    {
                        Span = assignExpression.Span,
                        Expression = assignExpression
                    });

                    if (Current.Type == TokenType.Comma)
                    {
                        NextToken();
                    }
                }

                ExpectEndOfStatement();

                // If we only have an include for, return it directly
                if (forStatement != null)
                {
                    if (block == null)
                    {
                        return Close(forStatement);
                    }
                    block.Statements.Add(forStatement);
                }

                // Else we have a block
                if (block != null)
                {
                    Close(block);
                    return block;
                }
            }

            ExpectEndOfStatement();
            return Close(includeStatement);
        }
   }
}