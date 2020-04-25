// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

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
    public partial class Parser
    {

        private void ParseScribanStatement(string identifier, ScriptStatement parent, ref ScriptStatement statement, ref bool hasEnd, ref bool nextStatement)
        {
            var startToken = Current;
            switch (identifier)
            {
                case "end":
                    hasEnd = true;
                    statement = ParseEndStatement();
                    break;
                case "wrap":
                    CheckNotInCase(parent, startToken);
                    statement = ParseWrapStatement();
                    break;
                case "if":
                    CheckNotInCase(parent, startToken);
                    statement = ParseIfStatement(false, null);
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
                        statement = ParseForStatement<ScriptForStatement>();
                    }
                    break;
                case "tablerow":
                    if (_isScientific) goto default;
                    CheckNotInCase(parent, startToken);
                    if (PeekToken().Type == TokenType.Dot)
                    {
                        statement = ParseExpressionStatement();
                    }
                    else
                    {
                        statement = ParseForStatement<ScriptTableRowStatement>();
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
                    var breakStatement = Open<ScriptBreakStatement>();
                    statement = breakStatement;
                    ExpectAndParseKeywordTo(breakStatement.BreakKeyword); // Parse break
                    ExpectEndOfStatement();
                    Close(statement);

                    // This has to be done at execution time, because of the wrap statement
                    //if (!IsInLoop())
                    //{
                    //    LogError(statement, "Unexpected statement outside of a loop");
                    //}
                    break;
                case "continue":
                    CheckNotInCase(parent, startToken);
                    var continueStatement =  Open<ScriptContinueStatement>();
                    statement = continueStatement;
                    ExpectAndParseKeywordTo(continueStatement.ContinueKeyword); // Parse continue keyword
                    ExpectEndOfStatement();
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

        private ScriptEndStatement ParseEndStatement()
        {
            var endStatement = Open<ScriptEndStatement>();
            ExpectAndParseKeywordTo(endStatement.EndKeyword);
            ExpectEndOfStatement();
            return Close(endStatement);
        }

        private ScriptFunction ParseFunctionStatement(bool isAnonymous)
        {
            var scriptFunction = Open<ScriptFunction>();
            if (isAnonymous)
            {
                scriptFunction.NameOrDoToken = ExpectAndParseKeywordTo(ScriptKeyword.Do());
            }
            else
            {
                scriptFunction.FuncToken = ExpectAndParseKeywordTo(ScriptKeyword.Func());
                scriptFunction.NameOrDoToken = ExpectAndParseVariable(scriptFunction);
            }

            // If we have parenthesis, this is a function with explicit parameters
            if (Current.Type == TokenType.OpenParen)
            {
                scriptFunction.OpenParen = ParseToken();
                scriptFunction.Parameters = new ScriptList<ScriptVariable>();

                bool isFirst = true;
                while (true)
                {
                    // Parse any required comma (before each new non-first argument)
                    // Or closing parent (and we exit the loop)
                    if (Current.Type == TokenType.CloseParen)
                    {
                        scriptFunction.CloseParen = ParseToken();
                        break;
                    }

                    if (!isFirst)
                    {
                        if (Current.Type == TokenType.Comma)
                        {
                            PushTokenToTrivia();
                            NextToken();
                        }
                        else
                        {
                            LogError(Current, "Expecting a comma to separate arguments in a function call.");
                        }
                    }
                    isFirst = false;

                    // Else we expect an expression
                    if (IsStartOfExpression())
                    {
                        var arg = ExpectAndParseVariable(scriptFunction);
                        if (!(arg is ScriptVariableGlobal))
                        {
                            LogError(arg.Span, "Expecting only a simple name parameter for a function");
                        }
                        scriptFunction.Parameters.Add(arg);
                        scriptFunction.Span.End = arg.Span.End;
                    }
                    else
                    {
                        LogError(Current, "Expecting an expression for argument function calls instead of this token.");
                        break;
                    }
                }

                if (scriptFunction.CloseParen == null)
                {
                    LogError(Current, "Expecting a closing parenthesis for a function call.");
                }
            }

            ExpectEndOfStatement();
            scriptFunction.Body = ParseBlockStatement(scriptFunction);

            return Close(scriptFunction);
        }

        private ScriptImportStatement ParseImportStatement()
        {
            var importStatement = Open<ScriptImportStatement>();
            ExpectAndParseKeywordTo(importStatement.ImportKeyword); // Parse import keyword

            importStatement.Expression = ExpectAndParseExpression(importStatement);
            ExpectEndOfStatement();

            return Close(importStatement);
        }

        private ScriptReadOnlyStatement ParseReadOnlyStatement()
        {
            var readOnlyStatement = Open<ScriptReadOnlyStatement>();
            ExpectAndParseKeywordTo(readOnlyStatement.ReadOnlyKeyword); // Parse readonly keyword

            readOnlyStatement.Variable = ExpectAndParseVariable(readOnlyStatement);
            ExpectEndOfStatement();

            return Close(readOnlyStatement);
        }

        private ScriptReturnStatement ParseReturnStatement()
        {
            var ret = Open<ScriptReturnStatement>();
            ExpectAndParseKeywordTo(ret.RetKeyword); // Parse ret keyword

            if (IsStartOfExpression())
            {
                ret.Expression = ParseExpression(ret);
            }
            ExpectEndOfStatement();

            return Close(ret);
        }

        private ScriptWhileStatement ParseWhileStatement()
        {
            var whileStatement = Open<ScriptWhileStatement>();
            ExpectAndParseKeywordTo(whileStatement.WhileKeyword); // Parse while keyword

            // Parse the condition
            // unit test: 220-while-error1.txt
            whileStatement.Condition = ExpectAndParseExpression(whileStatement, allowAssignment: false);

            if (ExpectEndOfStatement())
            {
                whileStatement.Body = ParseBlockStatement(whileStatement);
            }

            return Close(whileStatement);
        }

        private ScriptWithStatement ParseWithStatement()
        {
            var withStatement = Open<ScriptWithStatement>();
            ExpectAndParseKeywordTo(withStatement.WithKeyword); // // Parse with keyword
            withStatement.Name = ExpectAndParseExpression(withStatement);

            if (ExpectEndOfStatement())
            {
                withStatement.Body = ParseBlockStatement(withStatement);
            }
            return Close(withStatement);
        }

        private ScriptWrapStatement ParseWrapStatement()
        {
            var wrapStatement = Open<ScriptWrapStatement>();
            ExpectAndParseKeywordTo(wrapStatement.WrapKeyword); // Parse wrap keyword

            wrapStatement.Target = ExpectAndParseExpression(wrapStatement);

            if (ExpectEndOfStatement())
            {
                wrapStatement.Body = ParseBlockStatement(wrapStatement);
            }

            return Close(wrapStatement);
        }

        private void FixRawStatementAfterFrontMatter(ScriptPage page)
        {
            // In case of parsing a front matter, we don't want to include any \r\n after the end of the front-matter
            // So we manipulate back the syntax tree for the expected raw statement (if any), otherwise we can early
            // exit.
            var rawStatement = page.Body.Statements.FirstOrDefault() as ScriptRawStatement;
            if (rawStatement == null)
            {
                return;
            }
            rawStatement.Text = rawStatement.Text.TrimStart();
        }

        private static bool IsScribanKeyword(string text)
        {
            switch (text)
            {
                case "if":
                case "else":
                case "end":
                case "for":
                case "case":
                case "when":
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
            return false;
        }
    }
}