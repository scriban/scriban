// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Scriban.Functions;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Parsing
{
    public partial class Parser
    {
        private int _allowNewLineLevel = 0;
        private int _expressionLevel = 0;

        public int ExpressionLevel => _expressionLevel;

        private static readonly int PrecedenceOfMultiply = GetDefaultBinaryOperatorPrecedence(ScriptBinaryOperator.Multiply);

        private ScriptExpression ParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int precedence = 0, ParseExpressionMode mode = ParseExpressionMode.Default, bool allowAssignment = true)
        {
            bool hasAnonymousFunction = false;
            return ParseExpression(parentNode, ref hasAnonymousFunction, parentExpression, precedence, mode, allowAssignment);
        }

        private bool TryBinaryOperator(out ScriptBinaryOperator binaryOperator, out int precedence)
        {
            var tokenType = Current.Type;

            precedence = 0;

            binaryOperator = ScriptBinaryOperator.None;
            switch (tokenType)
            {
                case TokenType.Asterisk: binaryOperator = ScriptBinaryOperator.Multiply; break;
                case TokenType.Divide: binaryOperator = ScriptBinaryOperator.Divide; break;
                case TokenType.DoubleDivide: binaryOperator = ScriptBinaryOperator.DivideRound; break;
                case TokenType.Plus: binaryOperator = ScriptBinaryOperator.Add; break;
                case TokenType.Minus: binaryOperator = ScriptBinaryOperator.Substract; break;
                case TokenType.Percent: binaryOperator = ScriptBinaryOperator.Modulus; break;
                case TokenType.DoubleLessThan: binaryOperator = ScriptBinaryOperator.ShiftLeft; break;
                case TokenType.DoubleGreaterThan: binaryOperator = ScriptBinaryOperator.ShiftRight; break;
                case TokenType.DoubleQuestion: binaryOperator = ScriptBinaryOperator.EmptyCoalescing; break;
                case TokenType.DoubleAmp: binaryOperator = ScriptBinaryOperator.And; break;
                case TokenType.DoubleVerticalBar: binaryOperator = ScriptBinaryOperator.Or; break;
                case TokenType.DoubleEqual: binaryOperator = ScriptBinaryOperator.CompareEqual; break;
                case TokenType.ExclamationEqual: binaryOperator = ScriptBinaryOperator.CompareNotEqual; break;
                case TokenType.Greater: binaryOperator = ScriptBinaryOperator.CompareGreater; break;
                case TokenType.GreaterEqual: binaryOperator = ScriptBinaryOperator.CompareGreaterOrEqual; break;
                case TokenType.Less: binaryOperator = ScriptBinaryOperator.CompareLess; break;
                case TokenType.LessEqual: binaryOperator = ScriptBinaryOperator.CompareLessOrEqual; break;
                case TokenType.DoubleDot: binaryOperator = ScriptBinaryOperator.RangeInclude; break;
                case TokenType.DoubleDotLess: binaryOperator = ScriptBinaryOperator.RangeExclude; break;
                default:
                    if (_isScientific)
                    {
                        switch (tokenType)
                        {
                            case TokenType.Caret: binaryOperator = ScriptBinaryOperator.Power; break;
                            case TokenType.Amp: binaryOperator = ScriptBinaryOperator.BinaryAnd; break;
                            case TokenType.VerticalBar: binaryOperator = ScriptBinaryOperator.BinaryOr; break;
                        }
                    }
                    break;
            }

            if (binaryOperator != ScriptBinaryOperator.None)
            {
                precedence = GetDefaultBinaryOperatorPrecedence(binaryOperator);
            }

            return binaryOperator != ScriptBinaryOperator.None;
        }

        private ScriptExpression ParseExpression(ScriptNode parentNode, ref bool hasAnonymousFunction, ScriptExpression parentExpression = null, int precedence = 0,
            ParseExpressionMode mode = ParseExpressionMode.Default, bool allowAssignment = true)
        {
            int expressionCount = 0;
            _expressionLevel++;
            var expressionDepthBeforeEntering = _expressionDepth;
            EnterExpression();
            try
            {
                ScriptFunctionCall functionCall = null;
                parseExpression:
                expressionCount++;

                // Allow custom parsing for a first pre-expression
                ScriptExpression leftOperand = null;
                bool isLeftOperandClosed = false;

                switch (Current.Type)
                {
                    case TokenType.Identifier:
                    case TokenType.IdentifierSpecial:
                        leftOperand = ParseVariable();

                        // In case of liquid template, we accept the syntax colon after a tag
                        if (_isLiquid && parentNode is ScriptPipeCall && Current.Type == TokenType.Colon)
                        {
                            NextToken();
                        }

                        // Special handle of the $$ block delegate variable
                        if (ScriptVariable.BlockDelegate.Equals(leftOperand))
                        {
                            if (expressionCount != 1 || _expressionLevel > 1)
                            {
                                LogError("Cannot use block delegate $$ in a nested expression");
                            }

                            if (!(parentNode is ScriptExpressionStatement))
                            {
                                LogError(parentNode, "Cannot use block delegate $$ outside an expression statement");
                            }

                            return leftOperand;
                        }
                        break;
                    case TokenType.Integer:
                        leftOperand = ParseInteger();
                        break;
                    case TokenType.HexaInteger:
                        leftOperand = ParseHexaInteger();
                        break;
                    case TokenType.BinaryInteger:
                        leftOperand = ParseBinaryInteger();
                        break;
                    case TokenType.Float:
                        leftOperand = ParseFloat();
                        break;
                    case TokenType.String:
                        leftOperand = ParseString();
                        break;
                    case TokenType.ImplicitString:
                        leftOperand = ParseImplicitString();
                        break;
                    case TokenType.VerbatimString:
                        leftOperand = ParseVerbatimString();
                        break;
                    case TokenType.OpenParen:
                        leftOperand = ParseParenthesis(ref hasAnonymousFunction);
                        break;
                    case TokenType.OpenBrace:
                        leftOperand = ParseObjectInitializer();
                        break;
                    case TokenType.OpenBracket:
                        leftOperand = ParseArrayInitializer();
                        break;
                    default:
                        if (IsStartingAsUnaryExpression())
                        {
                            leftOperand = ParseUnaryExpression(ref hasAnonymousFunction);
                        }
                        break;
                }

                // Should not happen but in case
                if (leftOperand == null)
                {
                    if (functionCall != null)
                    {
                        LogError($"Unexpected token `{GetAsText(Current)}` while parsing function call `{functionCall}`");
                    }
                    else
                    {
                        LogError($"Unexpected token `{GetAsText(Current)}` while parsing expression");
                    }
                    return null;
                }

                if (leftOperand is ScriptAnonymousFunction)
                {
                    hasAnonymousFunction = true;
                }

                while (!hasAnonymousFunction)
                {
                    if (_isLiquid && Current.Type == TokenType.Comma && functionCall != null)
                    {
                        NextToken(); // Skip the comma for arguments in a function call
                    }

                    // Parse Member expression are expected to be followed only by an identifier
                    if (Current.Type == TokenType.Dot)
                    {
                        var nextToken = PeekToken();
                        if (nextToken.Type == TokenType.Identifier)
                        {
                            var dotToken = ScriptToken.Dot();
                            Open(dotToken);
                            NextToken(); // Skip .
                            Close(dotToken);

                            if (GetAsText(Current) == "empty" && PeekToken().Type == TokenType.Question)
                            {
                                var memberExpression = Open<ScriptIsEmptyExpression>();
                                memberExpression.Span = leftOperand.Span;
                                memberExpression.DotToken = dotToken;
                                memberExpression.Member = (ScriptVariable)ParseVariable();
                                ExpectAndParseTokenTo(memberExpression.QuestionToken, TokenType.Question);
                                memberExpression.Target = leftOperand;
                                leftOperand = Close(memberExpression);
                            }
                            else
                            {
                                var memberExpression = Open<ScriptMemberExpression>();
                                memberExpression.Span = leftOperand.Span;

                                memberExpression.DotToken = dotToken;
                                memberExpression.Target = leftOperand;

                                var member = ParseVariable();
                                if (!(member is ScriptVariable))
                                {
                                    LogError("Unexpected literal member `{member}`");
                                    return null;
                                }
                                memberExpression.Member = (ScriptVariable)member;
                                leftOperand = Close(memberExpression);
                            }
                        }
                        else
                        {
                            LogError(nextToken, $"Invalid token `{nextToken.Type}`. The dot operator is expected to be followed by a plain identifier");
                            return null;
                        }
                        continue;
                    }

                    // If we have a bracket but left operand is a (variable || member || indexer), then we consider next as an indexer
                    // unit test: 130-indexer-accessor-accept1.txt
                    if (Current.Type == TokenType.OpenBracket && (leftOperand is IScriptVariablePath || leftOperand is ScriptLiteral)  && !IsPreviousCharWhitespace())
                    {
                        var indexerExpression = Open<ScriptIndexerExpression>();
                        indexerExpression.Span = leftOperand.Span;
                        indexerExpression.Target = leftOperand;

                        ExpectAndParseTokenTo(indexerExpression.OpenBracket, TokenType.OpenBracket); // parse [

                        // unit test: 130-indexer-accessor-error5.txt
                        indexerExpression.Index = ExpectAndParseExpression(indexerExpression, ref hasAnonymousFunction, functionCall, 0, $"Expecting <index_expression> instead of `{GetAsText(Current)}`");

                        if (Current.Type != TokenType.CloseBracket)
                        {
                            LogError($"Unexpected `{GetAsText(Current)}`. Expecting ']'");
                        }
                        else
                        {
                            ExpectAndParseTokenTo(indexerExpression.CloseBracket, TokenType.CloseBracket); // parse ]
                        }

                        leftOperand = Close(indexerExpression);
                        continue;
                    }

                    if (mode == ParseExpressionMode.BasicExpression)
                    {
                        break;
                    }

                    if (Current.Type == TokenType.Equal)
                    {
                        if (leftOperand is ScriptFunctionCall call && call.TryGetFunctionDeclaration(out ScriptFunction declaration))
                        {
                            if (_expressionLevel > 1 || !allowAssignment)
                            {
                                LogError(leftOperand, $"Creating a function is only allowed for a top level assignment");
                            }

                            declaration.EqualToken = ParseToken(); // eat equal token
                            declaration.Body = ParseExpressionStatement();
                            declaration.Span.End = declaration.Body.Span.End;
                            leftOperand = new ScriptExpressionAsStatement(declaration) {Span = declaration.Span};
                        }
                        else
                        {
                            var assignExpression = Open<ScriptAssignExpression>();

                            if (leftOperand != null && !(leftOperand is IScriptVariablePath) || functionCall != null || _expressionLevel > 1 || !allowAssignment)
                            {
                                // unit test: 101-assign-complex-error1.txt
                                LogError(assignExpression, $"Expression is only allowed for a top level assignment");
                            }

                            ExpectAndParseTokenTo(assignExpression.EqualToken, TokenType.Equal);

                            assignExpression.Target = TransformKeyword(leftOperand);

                            // unit test: 105-assign-error3.txt
                            assignExpression.Value = ExpectAndParseExpression(assignExpression, ref hasAnonymousFunction, parentExpression);

                            leftOperand = Close(assignExpression);
                        }

                        break;
                    }

                    // Parse unary -1 if a minus is not followed by a space
                    bool isUnaryMinus = _isScientific && Current.Type == TokenType.Minus && !IsNextCharWhitespace();

                    // Handle binary operators here
                    ScriptBinaryOperator binaryOperatorType;
                    int newPrecedence;
                    if (!isUnaryMinus && TryBinaryOperator(out binaryOperatorType, out newPrecedence) || (_isLiquid && TryLiquidBinaryOperator(out binaryOperatorType, out newPrecedence)))
                    {
                        // Check precedence to see if we should "take" this operator here (Thanks TimJones for the tip code! ;)
                        if (newPrecedence <= precedence)
                        {
                            if (_isScientific)
                            {
                                if (functionCall != null)
                                {
                                    // if we were in the middle of a function call and the new operator
                                    // doesn't have the same associativity (/*%^) we will transform
                                    // the pending call into a left operand and let the current operator (eg +)
                                    // to work on it. Example: cos 2x + 1
                                    // functionCall: cos 2
                                    // leftOperand: x
                                    // binaryOperatorType: Add
                                    if (newPrecedence < precedence)
                                    {
                                        functionCall.AddArgument(leftOperand);
                                        leftOperand = functionCall;
                                        functionCall = null;
                                    }
                                    precedence = newPrecedence;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        // In scientific mode, with a pending function call, we can't detect how
                        // operations of similar precedence (*/%^) are going to behave until
                        // we resolve the functions (to detect if they take a parameter or not)
                        // In fact case, we need create a ScriptArgumentBinary for the operator
                        // and push it as an argument of the function call:
                        // expression to parse: cos 2x * cos 2x
                        // function call will be => cos(2, x, *, cos, 2, x)
                        // which is incorrect so we will rewrite it to cos(2 * x) * cos(2 * x)
                        if (_isScientific && (functionCall != null || newPrecedence >= PrecedenceOfMultiply))
                        {
                            // Store %*/^ in a pseudo function call
                            if (functionCall == null)
                            {
                                functionCall = Open<ScriptFunctionCall>();
                                functionCall.Target = leftOperand;
                                functionCall.Span = leftOperand.Span;
                            }
                            else
                            {
                                functionCall.AddArgument(leftOperand);
                            }

                            var binaryArgument = Open<ScriptArgumentBinary>();
                            binaryArgument.Operator = binaryOperatorType;
                            binaryArgument.OperatorToken = ParseToken();
                            Close(binaryArgument);

                            functionCall.AddArgument(binaryArgument);

                            precedence = newPrecedence;

                            goto parseExpression;
                        }

                        // We fake entering an expression here to limit the number of expression
                        EnterExpression();
                        var binaryExpression = Open<ScriptBinaryExpression>();
                        binaryExpression.Left = leftOperand;
                        binaryExpression.Operator = binaryOperatorType;

                        // Parse the operator
                        binaryExpression.OperatorToken = ParseToken();

                        // Special case for liquid, we revert the verbatim to the original scriban operator
                        if (_isLiquid && binaryOperatorType != ScriptBinaryOperator.Custom)
                        {
                            binaryExpression.OperatorToken.Value = binaryOperatorType.ToText();
                        }

                        // unit test: 110-binary-simple-error1.txt
                        binaryExpression.Right = ExpectAndParseExpression(binaryExpression, ref hasAnonymousFunction,
                            functionCall ?? parentExpression, newPrecedence,
                            $"Expecting an <expression> to the right of the operator instead of `{GetAsText(Current)}`");
                        leftOperand = Close(binaryExpression);

                        continue;
                    }

                    if (IsStartOfExpression())
                    {
                        // If we can parse a statement, we have a method call
                        if (parentExpression != null)
                        {
                            break;
                        }

                        // Parse named parameters
                        var paramContainer = parentNode as IScriptNamedArgumentContainer;
                        if (!_isScientific && Current.Type == TokenType.Identifier && (parentNode is IScriptNamedArgumentContainer || !_isLiquid && PeekToken().Type == TokenType.Colon))
                        {
                            if (paramContainer == null)
                            {
                                if (functionCall == null)
                                {
                                    functionCall = Open<ScriptFunctionCall>();
                                    functionCall.Target = leftOperand;
                                    functionCall.Span.Start = leftOperand.Span.Start;
                                }
                                else
                                {
                                    functionCall.Arguments.Add(leftOperand);
                                }
                            }
                            Close(leftOperand);
                            isLeftOperandClosed = true;

                            while (true)
                            {
                                if (Current.Type != TokenType.Identifier)
                                {
                                    break;
                                }

                                var parameter = Open<ScriptNamedArgument>();

                                // Parse the name
                                parameter.Name = ParseIdentifier();

                                if (paramContainer != null)
                                {
                                    paramContainer.AddParameter(Close(parameter));
                                }
                                else
                                {
                                    functionCall.Arguments.Add(parameter);
                                }

                                // If we have a colon, we have a value
                                // otherwise it is a boolean argument name
                                if (Current.Type == TokenType.Colon)
                                {
                                    // Parse : token
                                    parameter.ColonToken = ScriptToken.Colon();
                                    ExpectAndParseTokenTo(parameter.ColonToken, TokenType.Colon);
                                    parameter.Value = ExpectAndParseExpression(parentNode, mode: ParseExpressionMode.BasicExpression);
                                    parameter.Span.End = parameter.Value.Span.End;
                                }

                                if (functionCall != null)
                                {
                                    functionCall.Span.End = parameter.Span.End;
                                }
                            }

                            // As we have handled leftOperand here, we don't let the function out of this while to pick up the leftOperand
                            if (functionCall != null)
                            {
                                leftOperand = functionCall;
                                functionCall = null;
                            }

                            // We don't allow anything after named parameters
                            break;
                        }


                        if (functionCall == null)
                        {
                            if (_isScientific && Current.Type != TokenType.OpenParen)
                            {
                                newPrecedence = PrecedenceOfMultiply;

                                if (newPrecedence <= precedence)
                                {
                                    break;
                                }

                                precedence = newPrecedence;
                            }


                            functionCall = Open<ScriptFunctionCall>();
                            functionCall.Target = leftOperand;

                            // If we need to convert liquid to scriban functions:
                            if (_isLiquid && Options.LiquidFunctionsToScriban)
                            {
                                TransformLiquidFunctionCallToScriban(functionCall);
                            }

                            functionCall.Span.Start = leftOperand.Span.Start;

                            if (_isScientific)
                            {
                                // Regular function call target(arg0, arg1, arg3, arg4...)
                                if (Current.Type == TokenType.OpenParen && !IsPreviousCharWhitespace())
                                {
                                    // This is an explicit call
                                    functionCall.ExplicitCall = true;
                                    functionCall.OpenParent = ParseToken();

                                    bool isFirst = true;
                                    while (true)
                                    {
                                        // Parse any required comma (before each new non-first argument)
                                        // Or closing parent (and we exit the loop)
                                        if (Current.Type == TokenType.CloseParen)
                                        {
                                            functionCall.CloseParen = ParseToken();
                                            break;
                                        }

                                        if (!isFirst)
                                        {
                                            if (Current.Type == TokenType.Comma)
                                            {
                                                PushTokenToTrivia();
                                                NextToken();
                                                FlushTriviasToLastTerminal();
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
                                            var arg = ParseExpression(functionCall);
                                            functionCall.Arguments.Add(arg);
                                            functionCall.Span.End = arg.Span.End;
                                        }
                                        else
                                        {
                                            LogError(Current, "Expecting an expression for argument function calls instead of this token.");
                                            break;
                                        }
                                    }

                                    if (functionCall.CloseParen == null)
                                    {
                                        LogError(Current, "Expecting a closing parenthesis for a function call.");
                                    }

                                    leftOperand = functionCall;
                                    functionCall = null;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            functionCall.AddArgument(leftOperand);
                        }
                        goto parseExpression;
                    }

                    if (precedence > 0)
                    {
                        break;
                    }

                    if (_isScientific && Current.Type == TokenType.PipeGreater || !_isScientific && (Current.Type == TokenType.VerticalBar || Current.Type == TokenType.PipeGreater))
                    {
                        if (functionCall != null)
                        {
                            functionCall.Arguments.Add(leftOperand);
                            leftOperand = functionCall;
                            functionCall = null;
                        }

                        var pipeCall = Open<ScriptPipeCall>();
                        pipeCall.From = leftOperand;

                        pipeCall.PipeToken = ParseToken(); // skip | or |>

                        // unit test: 310-func-pipe-error1.txt
                        pipeCall.To = ExpectAndParseExpression(pipeCall, ref hasAnonymousFunction);
                        return Close(pipeCall);
                    }

                    break;
                }

                if (functionCall != null)
                {
                    functionCall.Arguments.Add(leftOperand);
                    functionCall.Span.End = leftOperand.Span.End;
                    return functionCall;
                }

                return isLeftOperandClosed ? leftOperand : Close(leftOperand);
            }
            finally
            {
                LeaveExpression();
                // Force to restore back to a level
                _expressionDepth = expressionDepthBeforeEntering;
                _expressionLevel--;
            }
        }

        private ScriptExpression ParseArrayInitializer()
        {
            var scriptArray = Open<ScriptArrayInitializerExpression>();

            // Should happen before the NextToken to consume any EOL after
            _allowNewLineLevel++;

            // Parse [
            ExpectAndParseTokenTo(scriptArray.OpenBracketToken, TokenType.OpenBracket);

            bool expectingEndOfInitializer = false;

            // unit test: 120-array-initializer-accessor.txt
            while (true)
            {
                if (Current.Type == TokenType.CloseBracket)
                {
                    break;
                }

                if (!expectingEndOfInitializer)
                {
                    // unit test: 120-array-initializer-error2.txt
                    var expression = ExpectAndParseExpression(scriptArray);
                    if (expression == null)
                    {
                        break;
                    }
                    scriptArray.Values.Add(expression);

                    if (Current.Type == TokenType.Comma)
                    {
                        // Record trailing Commas
                        if (_isKeepTrivia)
                        {
                            PushTokenToTrivia();
                        }

                        NextToken();

                        if (_isKeepTrivia)
                        {
                            FlushTriviasToLastTerminal();
                        }
                    }
                    else
                    {
                        expectingEndOfInitializer = true;
                    }
                }
                else
                {
                    break;
                }
            }

            // Should happen before NextToken() to stop on the next EOF
            _allowNewLineLevel--;

            // Parse ]
            // unit test: 120-array-initializer-error1.txt
            ExpectAndParseTokenTo(scriptArray.CloseBracketToken, TokenType.CloseBracket);
            return Close(scriptArray);
        }

        private ScriptExpression ParseObjectInitializer()
        {
            var scriptObject = Open<ScriptObjectInitializerExpression>();

            // Should happen before the NextToken to consume any EOL after
            _allowNewLineLevel++;
            ExpectAndParseTokenTo(scriptObject.OpenBrace, TokenType.OpenBrace); // Parse {

            // unit test: 140-object-initializer-accessor.txt
            bool expectingEndOfInitializer = false;
            bool hasErrors = false;
            while (true)
            {
                if (Current.Type == TokenType.CloseBrace)
                {
                    break;
                }

                if (!expectingEndOfInitializer && (Current.Type == TokenType.Identifier || Current.Type == TokenType.String))
                {
                    var positionBefore = Current;

                    var objectMember = Open<ScriptObjectMember>();

                    var variableOrLiteral = ParseExpression(scriptObject);
                    var variable = variableOrLiteral as ScriptVariable;
                    var literal = variableOrLiteral as ScriptLiteral;

                    if (variable == null && literal == null)
                    {
                        hasErrors = true;
                        LogError(positionBefore, $"Unexpected member type `{variableOrLiteral}/{ScriptSyntaxAttribute.Get(variableOrLiteral).Name}` found for object initializer member name");
                        break;
                    }

                    if (literal != null && !(literal.Value is string))
                    {
                        hasErrors = true;
                        LogError(positionBefore,
                            $"Invalid literal member `{literal.Value}/{literal.Value?.GetType()}` found for object initializer member name. Only literal string or identifier name are allowed");
                        break;
                    }

                    if (variable != null)
                    {
                        if (variable.Scope != ScriptVariableScope.Global)
                        {
                            // unit test: 140-object-initializer-error3.txt
                            hasErrors = true;
                            LogError("Expecting a simple identifier for member names");
                            break;
                        }
                    }

                    if (Current.Type != TokenType.Colon)
                    {
                        // unit test: 140-object-initializer-error4.txt
                        hasErrors = true;
                        LogError($"Unexpected token `{GetAsText(Current)}` Expecting a colon : after identifier `{variable?.Name}` for object initializer member name");
                        break;
                    }

                    ExpectAndParseTokenTo(objectMember.ColonToken, TokenType.Colon); // Parse :
                    objectMember.Name = variableOrLiteral;

                    if (!IsStartOfExpression())
                    {
                        // unit test: 140-object-initializer-error5.txt
                        hasErrors = true;
                        LogError($"Unexpected token `{GetAsText(Current)}`. Expecting an expression for the value of the member.");
                        break;
                    }

                    var expression = ParseExpression(scriptObject);
                    objectMember.Value = expression;
                    objectMember.Span.End = expression.Span.End;
                    Close(objectMember);

                    // Erase any previous declaration of this member
                    scriptObject.Members.Add(objectMember);

                    if (Current.Type == TokenType.Comma)
                    {
                        // Record trailing Commas
                        if (_isKeepTrivia)
                        {
                            PushTokenToTrivia();
                            FlushTriviasToLastTerminal();
                        }
                        NextToken();
                    }
                    else
                    {
                        expectingEndOfInitializer = true;
                    }
                }
                else
                {
                    // unit test: 140-object-initializer-error1.txt
                    hasErrors = true;
                    LogError($"Unexpected token `{GetAsText(Current)}` while parsing object initializer. Expecting a simple identifier for the member name.");
                    break;
                }
            }

            // Should happen before NextToken() to stop on the next EOF
            _allowNewLineLevel--;

            if (!hasErrors)
            {
                ExpectAndParseTokenTo(scriptObject.CloseBrace, TokenType.CloseBrace); // Parse }
            }

            return Close(scriptObject);
        }

        private ScriptExpression ParseParenthesis(ref bool hasAnonymousFunction)
        {
            // unit test: 106-parenthesis.txt
            var expression = Open<ScriptNestedExpression>();
            ExpectAndParseTokenTo(expression.OpenParen, TokenType.OpenParen); // Parse (
            expression.Expression = ExpectAndParseExpression(expression, ref hasAnonymousFunction);

            if (Current.Type == TokenType.CloseParen)
            {
                ExpectAndParseTokenTo(expression.CloseParen, TokenType.CloseParen); // Parse )
            }
            else
            {
                // unit test: 106-parenthesis-error1.txt
                LogError(Current, $"Invalid token `{GetAsText(Current)}`. Expecting a closing `)`.");
            }
            return Close(expression);
        }

        private ScriptToken ParseToken()
        {
            var verbatim = Open<ScriptToken>();
            verbatim.Value = GetAsText(Current);
            NextToken();
            return Close(verbatim);
        }

        private void ExpectAndParseTokenTo(ScriptToken existingToken, TokenType expectedTokenType)
        {
            var verbatim = Open(existingToken);
            if (Current.Type != expectedTokenType)
            {
                LogError(CurrentSpan, $"Unexpected token found `{GetAsText(Current)}` while expecting `{expectedTokenType.ToText()}`.");
            }
            NextToken();
            Close(verbatim);
        }

        private ScriptKeyword ExpectAndParseKeywordTo(ScriptKeyword existingKeyword)
        {
            if (existingKeyword == null) throw new ArgumentNullException(nameof(existingKeyword));
            if (existingKeyword.Value == null) throw new InvalidOperationException($"{nameof(ScriptKeyword)}.{nameof(ScriptKeyword.Value)} cannot be null");

            var verbatim = Open(existingKeyword);
            if (!MatchText(Current, existingKeyword.Value))
            {
                LogError(CurrentSpan, $"Unexpected keyword found `{GetAsText(Current)}` while expecting `{existingKeyword.Value}`.");
            }
            NextToken();
            Close(verbatim);
            return existingKeyword;
        }

        private ScriptExpression ParseUnaryExpression(ref bool hasAnonymousFunction)
        {
            // unit test: 113-unary.txt
            var unaryExpression = Open<ScriptUnaryExpression>();
            int newPrecedence;

            // Parse the operator as verbatim text
            var unaryTokenType = Current.Type;

            unaryExpression.OperatorToken = ParseToken();
            // Else we parse standard unary operators
            switch (unaryTokenType)
            {
                case TokenType.Exclamation:
                    unaryExpression.Operator = ScriptUnaryOperator.Not;
                    break;
                case TokenType.Minus:
                    unaryExpression.Operator = ScriptUnaryOperator.Negate;
                    break;
                case TokenType.Plus:
                    unaryExpression.Operator = ScriptUnaryOperator.Plus;
                    break;
                case TokenType.Arroba:
                    unaryExpression.Operator = ScriptUnaryOperator.FunctionAlias;
                    break;
                default:
                    if (_isScientific && unaryTokenType == TokenType.DoubleCaret || !_isScientific && unaryTokenType == TokenType.Caret)
                    {
                        unaryExpression.Operator = ScriptUnaryOperator.FunctionParametersExpand;
                    }

                    if (unaryExpression.Operator == ScriptUnaryOperator.None)
                    {
                        LogError($"Unexpected token `{unaryTokenType}` for unary expression");
                    }
                    break;
            }
            newPrecedence = GetDefaultUnaryOperatorPrecedence(unaryExpression.Operator);

            // unit test: 115-unary-error1.txt
            unaryExpression.Right = ExpectAndParseExpression(unaryExpression, ref hasAnonymousFunction, null, newPrecedence);
            return Close(unaryExpression);
        }

        private ScriptExpression TransformKeyword(ScriptExpression leftOperand)
        {
            // In case we are in liquid and we are assigning to a scriban keyword, we escape the variable with a nested expression
            if (_isLiquid && leftOperand is IScriptVariablePath && IsScribanKeyword(((IScriptVariablePath) leftOperand).GetFirstPath()) && !(leftOperand is ScriptNestedExpression))
            {
                var nestedExpression = ScriptNestedExpression.Wrap(leftOperand, _isKeepTrivia);
                return nestedExpression;
            }

            return leftOperand;
        }

        private void TransformLiquidFunctionCallToScriban(ScriptFunctionCall functionCall)
        {
            var liquidTarget = functionCall.Target as ScriptVariable;
            string targetName;
            string memberName;
            // In case of cycle we transform it to array.cycle at runtime
            if (liquidTarget != null && LiquidBuiltinsFunctions.TryLiquidToScriban(liquidTarget.Name, out targetName, out memberName))
            {
                var targetVariable = new ScriptVariableGlobal(targetName) {Span = liquidTarget.Span};
                var memberVariable = new ScriptVariableGlobal(memberName) {Span = liquidTarget.Span};

                var arrayCycle = new ScriptMemberExpression
                {
                    Span = liquidTarget.Span,
                    Target = targetVariable,
                    Member = memberVariable,
                };

                // Transfer trivias accordingly to target (trivias before) and member (trivias after)
                if (_isKeepTrivia && liquidTarget.Trivias != null)
                {
                    targetVariable.AddTrivias(liquidTarget.Trivias.Before, true);
                    memberVariable.AddTrivias(liquidTarget.Trivias.After, false);
                }
                functionCall.Target = arrayCycle;
            }
        }

        private void EnterExpression()
        {
            _expressionDepth++;
            if (Options.ExpressionDepthLimit.HasValue && !_isExpressionDepthLimitReached && _expressionDepth > Options.ExpressionDepthLimit.Value)
            {
                LogError(GetSpanForToken(Previous), $"The statement depth limit `{Options.ExpressionDepthLimit.Value}` was reached when parsing this statement");
                _isExpressionDepthLimitReached = true;
            }
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null,
            ParseExpressionMode mode = ParseExpressionMode.Default, bool allowAssignment = true)
        {
            if (IsStartOfExpression())
            {
                return ParseExpression(parentNode, parentExpression, newPrecedence, mode, allowAssignment);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of `{GetAsText(Current)}`");
            return null;
        }

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ref bool hasAnonymousExpression, ScriptExpression parentExpression = null, int newPrecedence = 0,
            string message = null, ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            if (IsStartOfExpression())
            {
                return ParseExpression(parentNode, ref hasAnonymousExpression, parentExpression, newPrecedence, mode);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of `{GetAsText(Current)}`");
            return null;
        }

        private ScriptExpression ExpectAndParseExpressionAndAnonymous(ScriptNode parentNode, out bool hasAnonymousFunction, ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            hasAnonymousFunction = false;
            if (IsStartOfExpression())
            {
                return ParseExpression(parentNode, ref hasAnonymousFunction, null, 0, mode);
            }
            LogError(parentNode, CurrentSpan, $"Expecting <expression> instead of `{GetAsText(Current)}`");
            return null;
        }

        public bool IsStartOfExpression()
        {
            if (IsStartingAsUnaryExpression()) return true;

            switch (Current.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                case TokenType.Integer:
                case TokenType.HexaInteger:
                case TokenType.BinaryInteger:
                case TokenType.Float:
                case TokenType.String:
                case TokenType.ImplicitString:
                case TokenType.VerbatimString:
                case TokenType.OpenParen:
                case TokenType.OpenBrace:
                case TokenType.OpenBracket:
                    return true;
            }

            return false;
        }

        private bool IsStartingAsUnaryExpression()
        {
            switch (Current.Type)
            {
                case TokenType.Exclamation:
                case TokenType.Minus:
                case TokenType.Plus:
                case TokenType.Arroba:
                    return true;

                case TokenType.Caret:
                case TokenType.DoubleCaret:
                    // In scientific Caret is used for exponent (so it is a binary operator)
                    if (_isScientific && Current.Type == TokenType.DoubleCaret || !_isScientific && Current.Type == TokenType.Caret)
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        private bool TryLiquidBinaryOperator(out ScriptBinaryOperator binaryOperator, out int precedence)
        {
            binaryOperator = ScriptBinaryOperator.None;
            precedence = 0;

            var text = GetAsText(Current);

            if (Current.Type != TokenType.Identifier)
            {
                return false;
            }

            switch (text)
            {
                case "or":
                    binaryOperator = ScriptBinaryOperator.Or;
                    break;
                case "and":
                    binaryOperator = ScriptBinaryOperator.And;
                    break;
                case "contains":
                    binaryOperator = ScriptBinaryOperator.LiquidContains;
                    break;
                case "startsWith":
                    binaryOperator = ScriptBinaryOperator.LiquidStartsWith;
                    break;
                case "endsWith":
                    binaryOperator = ScriptBinaryOperator.LiquidEndsWith;
                    break;
                case "hasKey":
                    binaryOperator = ScriptBinaryOperator.LiquidHasKey;
                    break;
                case "hasValue":
                    binaryOperator = ScriptBinaryOperator.LiquidHasValue;
                    break;
            }

            if (binaryOperator != ScriptBinaryOperator.None)
            {
                precedence = GetDefaultBinaryOperatorPrecedence(binaryOperator);
            }

            return binaryOperator != ScriptBinaryOperator.None;
        }

        internal static int GetDefaultBinaryOperatorPrecedence(ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.EmptyCoalescing:
                    return 20;
                case ScriptBinaryOperator.Or:
                    return 30;
                case ScriptBinaryOperator.And:
                    return 40;

                case ScriptBinaryOperator.BinaryOr:
                    return 50;

                case ScriptBinaryOperator.BinaryAnd:
                    return 60;

                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                    return 70;

                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return 80;

                case ScriptBinaryOperator.LiquidContains:
                case ScriptBinaryOperator.LiquidStartsWith:
                case ScriptBinaryOperator.LiquidEndsWith:
                case ScriptBinaryOperator.LiquidHasKey:
                case ScriptBinaryOperator.LiquidHasValue:
                    return 90;

                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Substract:
                    return 100;
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                    return 110;
                case ScriptBinaryOperator.Power:
                    return 120;
                case ScriptBinaryOperator.RangeInclude:
                case ScriptBinaryOperator.RangeExclude:
                    return 130;
                default:
                    return 0;
            }
        }

        private static int GetDefaultUnaryOperatorPrecedence(ScriptUnaryOperator op)
        {
            switch (op)
            {
                case ScriptUnaryOperator.Not:
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                case ScriptUnaryOperator.FunctionAlias:
                case ScriptUnaryOperator.FunctionParametersExpand:
                    return 200;
                default:
                    return 0;
            }
        }

        bool IsPreviousCharWhitespace()
        {
            int position = Current.Start.Offset - 1;
            if (position >= 0)
            {
                return char.IsWhiteSpace(_lexer.Text[position]);
            }
            return false;
        }

        bool IsNextCharWhitespace()
        {
            int position = Current.End.Offset + 1;
            if (position >= 0 && position < _lexer.Text.Length)
            {
                return char.IsWhiteSpace(_lexer.Text[position]);
            }
            return false;
        }

        private void LeaveExpression()
        {
            _expressionDepth--;
        }

        private enum ParseExpressionMode
        {
            /// <summary>
            /// All expressions (e.g literals, function calls, function pipes...etc.)
            /// </summary>
            Default,

            /// <summary>
            /// Only literal, unary, nested, array/object initializer, dot access, array access
            /// </summary>
            BasicExpression,
        }
    }
}