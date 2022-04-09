// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Scriban.Functions;
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
        private int _allowNewLineLevel = 0;
        private int _expressionLevel = 0;

        public int ExpressionLevel => _expressionLevel;

        internal const int PrecedenceOfAdd = 100;
        internal const int PrecedenceOfMultiply = 110;

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
                case TokenType.Minus: binaryOperator = ScriptBinaryOperator.Subtract; break;
                case TokenType.Percent: binaryOperator = ScriptBinaryOperator.Modulus; break;
                case TokenType.DoubleLessThan: binaryOperator = ScriptBinaryOperator.ShiftLeft; break;
                case TokenType.DoubleGreaterThan: binaryOperator = ScriptBinaryOperator.ShiftRight; break;
                case TokenType.DoubleQuestion: binaryOperator = ScriptBinaryOperator.EmptyCoalescing; break;
                case TokenType.QuestionExclamation: binaryOperator = ScriptBinaryOperator.NotEmptyCoalescing; break;
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

        private ScriptExpression ParseExpressionAsVariableOrStringOrExpression(ScriptNode parentNode)
        {
            switch (Current.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                    return ParseVariable();
                case TokenType.String:
                    return ParseString();
                case TokenType.VerbatimString:
                    return ParseVerbatimString();
                default:
                    return ParseExpression(parentNode);
            }
        }

        private ScriptExpression ParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int precedence = 0, ParseExpressionMode mode = ParseExpressionMode.Default, bool allowAssignment = true)
        {
            bool hasAnonymousFunction = false;
            int expressionCount = 0;
            _expressionLevel++;
            var expressionDepthBeforeEntering = _expressionDepth;

            var enteringPrecedence = precedence;

            // Override the mode
            var originalMode = mode;
            mode = mode == ParseExpressionMode.WhenExpression ? ParseExpressionMode.Default : mode;

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
                        leftOperand = ParseParenthesis();
                        break;
                    case TokenType.OpenBrace:
                        leftOperand = ParseObjectInitializer();
                        break;
                    case TokenType.OpenBracket:
                        leftOperand = ParseArrayInitializer();
                        break;
                    case TokenType.DoublePlus:
                    case TokenType.DoubleMinus:
                        leftOperand = ParseIncrementDecrementExpression();
                        break;
                    default:
                        if (IsStartingAsUnaryExpression())
                        {
                            leftOperand = ParseUnaryExpression();
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
                    if (Current.Type == TokenType.Dot || (!_isLiquid && Current.Type == TokenType.QuestionDot))
                    {
                        var nextToken = PeekToken();
                        if (nextToken.Type == TokenType.Identifier)
                        {
                            var dotToken = ParseToken(Current.Type);

                            var currentAsText = GetAsText(Current);
                            if ((currentAsText == "empty" || currentAsText == "blank") && PeekToken().Type == TokenType.Question)
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

                                // We parse left associated, so we need to propagate the ?. if it is nested
                                if (leftOperand is ScriptMemberExpression nestedMemberExpression && nestedMemberExpression.DotToken.TokenType == TokenType.QuestionDot)
                                {
                                    dotToken.TokenType = TokenType.QuestionDot;
                                }

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
                            return leftOperand;
                        }
                        continue;
                    }

                    if (Current.Type == TokenType.DoublePlus || Current.Type == TokenType.DoubleMinus)
                    {
                        var op = Current;
                        if (!(leftOperand is IScriptVariablePath))
                        {
                            LogError($"The operand of an increment or decrement operator must be a variable, property or indexer");
                        }
                        var unaryExpression = new ScriptIncrementDecrementExpression
                        {
                            Right = leftOperand,
                            Span = leftOperand.Span,
                            OperatorToken = this.ParseToken(op.Type),
                            Operator = op.Type == TokenType.DoublePlus ? ScriptUnaryOperator.Increment : ScriptUnaryOperator.Decrement,
                            Post = true
                        };
                        leftOperand = unaryExpression;
                    }

                    // If we have a bracket but left operand is a (variable || member || indexer), then we consider next as an indexer
                    // unit test: 130-indexer-accessor-accept1.txt
                    if (Current.Type == TokenType.OpenBracket && (leftOperand is IScriptVariablePath || leftOperand is ScriptLiteral || leftOperand is ScriptFunctionCall) && !IsPreviousCharWhitespace())
                    {
                        var indexerExpression = Open<ScriptIndexerExpression>();
                        indexerExpression.Span = leftOperand.Span;
                        indexerExpression.Target = leftOperand;

                        ExpectAndParseTokenTo(indexerExpression.OpenBracket, TokenType.OpenBracket); // parse [

                        // unit test: 130-indexer-accessor-error5.txt
                        indexerExpression.Index = ExpectAndParseExpression(indexerExpression, functionCall, 0, $"Expecting <index_expression> instead of `{GetAsText(Current)}`", mode);

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

                    // Named argument
                    if (mode != ParseExpressionMode.DefaultNoNamedArgument && Current.Type == TokenType.Colon)
                    {
                        if (!(leftOperand is ScriptVariable))
                        {
                            LogError(leftOperand.Span, $"Expecting a simple global or local variable before `:` in order to create a named argument");
                            break;
                        }

                        var namedArgument = Open<ScriptNamedArgument>();
                        namedArgument.Name = (ScriptVariable) leftOperand;
                        namedArgument.ColonToken = ParseToken(TokenType.Colon);
                        namedArgument.Value = ExpectAndParseExpression(parentNode);
                        Close(namedArgument);
                        leftOperand = namedArgument;
                        break;
                    }

                    if (Current.Type == TokenType.Equal && leftOperand is ScriptFunctionCall call && call.TryGetFunctionDeclaration(out ScriptFunction declaration))
                    {
                        if (_expressionLevel > 1 || !allowAssignment)
                        {
                            LogError(leftOperand, $"Creating a function is only allowed for a top level assignment");
                        }

                        declaration.EqualToken = ParseToken(TokenType.Equal); // eat equal token
                        declaration.Body = ParseExpressionStatement();
                        declaration.Span.End = declaration.Body.Span.End;
                        leftOperand = new ScriptExpressionAsStatement(declaration) {Span = declaration.Span};
                        break;
                    }
                    if(TryGetCompoundAssignmentOperator(out var scriptToken, out var tokenType) && !(scriptToken is null))
                    {
                        var assignExpression = Open<ScriptAssignExpression>();
                        assignExpression.EqualToken = scriptToken;

                        if (leftOperand != null)
                        {
                            assignExpression.Span.Start = leftOperand.Span.Start;
                        }

                        if (leftOperand != null && !(leftOperand is IScriptVariablePath) || functionCall != null || _expressionLevel > 1 || !allowAssignment)
                        {
                            // unit test: 101-assign-complex-error1.txt
                            LogError(assignExpression, $"Expression is only allowed for a top level assignment");
                        }

                        ExpectAndParseTokenTo(assignExpression.EqualToken, tokenType);

                        assignExpression.Target = TransformKeyword(leftOperand);

                        // unit test: 105-assign-error3.txt
                        assignExpression.Value = ExpectAndParseExpression(assignExpression, parentExpression);

                        leftOperand = Close(assignExpression);
                        break;
                    }

                    // Handle binary operators here
                    ScriptBinaryOperator binaryOperatorType;
                    int newPrecedence;
                    if (TryBinaryOperator(out binaryOperatorType, out newPrecedence) || (_isLiquid && TryLiquidBinaryOperator(out binaryOperatorType, out newPrecedence)))
                    {
                        if (originalMode == ParseExpressionMode.WhenExpression && binaryOperatorType == ScriptBinaryOperator.Or)
                        {
                            break;
                        }

                        // Check precedence to see if we should "take" this operator here (Thanks TimJones for the tip code! ;)
                        if (newPrecedence <= precedence)
                        {
                            if (enteringPrecedence == 0)
                            {
                                precedence = enteringPrecedence;
                                continue;
                            }
                            break;
                        }

                        // We fake entering an expression here to limit the number of expression
                        EnterExpression();
                        var binaryExpression = Open<ScriptBinaryExpression>();
                        binaryExpression.Span = leftOperand.Span;
                        binaryExpression.Left = leftOperand;
                        binaryExpression.Operator = binaryOperatorType;

                        // Parse the operator
                        binaryExpression.OperatorToken = ParseToken(Current.Type);

                        // Special case for liquid, we revert the verbatim to the original scriban operator
                        if (_isLiquid && binaryOperatorType != ScriptBinaryOperator.Custom)
                        {
                            binaryExpression.OperatorToken.Value = binaryOperatorType.ToText();
                        }

                        // unit test: 110-binary-simple-error1.txt
                        binaryExpression.Right = ExpectAndParseExpression(binaryExpression,
                            functionCall ?? parentExpression, newPrecedence,
                            $"Expecting an <expression> to the right of the operator instead of `{GetAsText(Current)}`",
                            mode); // propagate the mode in case we are in DefaultNoNamedArgument (so that the colon in 1 + 2 + 3 : will be correctly skipped)
                        leftOperand = Close(binaryExpression);

                        continue;
                    }

                    // Parse conditional expression
                    if (!_isLiquid && Current.Type == TokenType.Question)
                    {
                        if (precedence > 0)
                        {
                            break;
                        }

                        // If we have any pending function call, we close it
                        if (functionCall != null)
                        {
                            functionCall.Arguments.Add(leftOperand);
                            Close(functionCall);
                            leftOperand = functionCall;
                            functionCall = null;
                        }

                        var conditionalExpression = Open<ScriptConditionalExpression>();
                        conditionalExpression.Span = leftOperand.Span;

                        conditionalExpression.Condition = leftOperand;

                        // Parse ?
                        ExpectAndParseTokenTo(conditionalExpression.QuestionToken, TokenType.Question);

                        conditionalExpression.ThenValue = ExpectAndParseExpression(conditionalExpression, mode: ParseExpressionMode.DefaultNoNamedArgument);

                        // Parse :
                        ExpectAndParseTokenTo(conditionalExpression.ColonToken, TokenType.Colon);

                        conditionalExpression.ElseValue = ExpectAndParseExpression(conditionalExpression, mode: ParseExpressionMode.DefaultNoNamedArgument);

                        Close(conditionalExpression);
                        leftOperand = conditionalExpression;
                        break;
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
                                // Don't parse boolean parameters as named parameters for function calls
                                if (Current.Type != TokenType.Identifier || (paramContainer == null && PeekToken().Type != TokenType.Colon))
                                {
                                    break;
                                }

                                var parameter = Open<ScriptNamedArgument>();

                                // Parse the name
                                var variable = ParseVariable();
                                if (!(variable is ScriptVariable))
                                {
                                    LogError(variable.Span, $"Invalid identifier passed as a named argument. Expecting a simple variable name");
                                    break;
                                }

                                parameter.Name = (ScriptVariable)variable;

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
                                    // Certain patterns of malformed input (see issue-295) can cause the expression parser to return null
                                    // at this point.  In that case, leave the span empty
                                    if (parameter.Value != null)
                                        parameter.Span.End = parameter.Value.Span.End;

                                }

                                if (functionCall != null)
                                {
                                    functionCall.Span.End = parameter.Span.End;
                                    if (parameter.Value is ScriptAnonymousFunction)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (paramContainer != null || !IsStartOfExpression())
                            {
                                if (functionCall != null)
                                {
                                    leftOperand = functionCall;
                                    functionCall = null;
                                }

                                // We don't allow anything after named parameters for IScriptNamedArgumentContainer
                                break;
                            }

                            // Otherwise we allow to mix normal parameters within named parameters
                            goto parseExpression;
                        }

                        bool isLikelyExplicitFunctionCall = Current.Type == TokenType.OpenParen && !IsPreviousCharWhitespace();
                        if (functionCall == null || isLikelyExplicitFunctionCall)
                        {
                            if (_isScientific && !isLikelyExplicitFunctionCall)
                            {
                                newPrecedence = PrecedenceOfMultiply;

                                if (newPrecedence <= precedence)
                                {
                                    if (enteringPrecedence == 0)
                                    {
                                        precedence = enteringPrecedence;
                                        continue;
                                    }

                                    break;
                                }

                                // We fake entering an expression here to limit the number of expression
                                EnterExpression();
                                var binaryExpression = Open<ScriptBinaryExpression>();
                                binaryExpression.Span = leftOperand.Span;
                                binaryExpression.Left = leftOperand;
                                // OperatorToken = null // implicit multiply operator
                                binaryExpression.Operator = ScriptBinaryOperator.Multiply;
                                binaryExpression.Right = ExpectAndParseExpression(binaryExpression, functionCall ?? parentExpression, newPrecedence,
                                    $"Expecting an <expression> to the right of the operator instead of `{GetAsText(Current)}`");
                                leftOperand = Close(binaryExpression);

                                continue;
                            }

                            var pendingFunctionCall = functionCall;

                            functionCall = Open<ScriptFunctionCall>();
                            functionCall.Target = leftOperand;

                            // If we need to convert liquid to scriban functions:
                            if (_isLiquid && Options.LiquidFunctionsToScriban)
                            {
                                TransformLiquidFunctionCallToScriban(functionCall);
                            }

                            functionCall.Span.Start = leftOperand.Span.Start;

                            // Regular function call target(arg0, arg1, arg3, arg4...)
                            if (Current.Type == TokenType.OpenParen && !IsPreviousCharWhitespace())
                            {
                                // This is an explicit call
                                functionCall.ExplicitCall = true;
                                functionCall.OpenParent = ParseToken(TokenType.OpenParen);

                                bool isFirst = true;
                                while (true)
                                {
                                    // Parse any required comma (before each new non-first argument)
                                    // Or closing parent (and we exit the loop)
                                    if (Current.Type == TokenType.CloseParen)
                                    {
                                        functionCall.CloseParen = ParseToken(TokenType.CloseParen);
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

                                functionCall = pendingFunctionCall;
                                continue;
                            }
                        }
                        else
                        {
                            functionCall.AddArgument(leftOperand);
                            functionCall.Span.End = leftOperand.Span.End;

                            if (leftOperand is ScriptAnonymousFunction)
                            {
                                break;
                            }
                        }
                        goto parseExpression;
                    }

                    if (enteringPrecedence > 0)
                    {
                        break;
                    }

                    if ((!_isScientific) && (parentNode is ScriptPipeCall) // after a pipe call we expect to see a function call
                                         && (functionCall == null)         // but when function is not followed by any parameter e.g. '1 | math.abs', above code does not create function call,
                                                                           // here we fix that by creating function call, but only when leftOperand is e.g. '1 | abs' or '1 | math.abs'
                                         && (leftOperand is IScriptVariablePath) // we need that restriction since leftOperand can be of other type e.g. binary expression '"123" | string.to_int + 1'
                        )
                    {
                        var funcCall = Open<ScriptFunctionCall>();
                        funcCall.Target = leftOperand;
                        funcCall.Span = leftOperand.Span;
                        leftOperand = funcCall;
                    }

                    if (
                            (_isScientific &&
                              Current.Type == TokenType.PipeGreater)
                            ||
                             (!_isScientific &&
                                    Current.Type == TokenType.VerticalBar) ||
                                    Current.Type == TokenType.PipeGreater
                       )
                    {
                        if (functionCall != null)
                        {
                            functionCall.Arguments.Add(leftOperand);
                            leftOperand = functionCall;
                            functionCall = null;
                        }

                        var pipeCall = Open<ScriptPipeCall>();
                        if (leftOperand != null)
                        {
                            pipeCall.Span.Start = leftOperand.Span.Start;
                        }
                        pipeCall.From = leftOperand;
						  //Allow pipes to span lines
                        _allowNewLineLevel++;
                        pipeCall.PipeToken = ParseToken(Current.Type); // skip | or |>
                        _allowNewLineLevel--;
                        // unit test: 310-func-pipe-error1.txt
                        pipeCall.To = ExpectAndParseExpression(pipeCall);
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

                    var variableOrLiteral = ParseExpressionAsVariableOrStringOrExpression(scriptObject);
                    var variable = variableOrLiteral as ScriptVariable;
                    var literal = variableOrLiteral as ScriptLiteral;

                    if (variable == null && literal == null)
                    {
                        hasErrors = true;
                        LogError(positionBefore, $"Unexpected member type `{variableOrLiteral}/{ScriptSyntaxAttribute.Get(variableOrLiteral).TypeName}` found for object initializer member name");
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

        private ScriptExpression ParseParenthesis()
        {
            // unit test: 106-parenthesis.txt
            var expression = Open<ScriptNestedExpression>();
            ExpectAndParseTokenTo(expression.OpenParen, TokenType.OpenParen); // Parse (
            expression.Expression = ExpectAndParseExpression(expression);

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

        private ScriptToken ParseToken(TokenType tokenType)
        {
            var verbatim = Open<ScriptToken>();
            if (Current.Type != tokenType)
            {
                LogError(CurrentSpan, $"Unexpected token found `{GetAsText(Current)}` while expecting `{tokenType.ToText()}`.");
            }
            verbatim.TokenType = Current.Type;
            verbatim.Value = tokenType.ToText();
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

        private ScriptExpression ParseIncrementDecrementExpression()
        {
            // Parse the operator as verbatim text
            var unaryTokenType = Current.Type;
            var expression = Open<ScriptIncrementDecrementExpression>();
            expression.OperatorToken = ParseToken(unaryTokenType);
            switch (unaryTokenType)
            {
                case TokenType.DoublePlus:
                    expression.Operator = ScriptUnaryOperator.Increment;
                    break;
                case TokenType.DoubleMinus:
                    expression.Operator = ScriptUnaryOperator.Decrement;
                    break;
                default:
                    LogError($"Unexpected token `{unaryTokenType}` for unary expression");
                    break;
            }
            var newPrecedence = GetDefaultUnaryOperatorPrecedence(expression.Operator);
            expression.Right = ExpectAndParseExpression(expression, null, newPrecedence);
            if (!(expression.Right is IScriptVariablePath))
            {
                LogError($"The operand of an increment or decrement operator must be a variable, property or indexer");
            }
            return Close(expression);
        }

        private ScriptExpression ParseUnaryExpression()
        {
            // unit test: 113-unary.txt
            var unaryExpression = Open<ScriptUnaryExpression>();
            int newPrecedence;

            // Parse the operator as verbatim text
            var unaryTokenType = Current.Type;

            unaryExpression.OperatorToken = ParseToken(Current.Type);
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
            unaryExpression.Right = ExpectAndParseExpression(unaryExpression, null, newPrecedence) ;
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

        private ScriptExpression ExpectAndParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int newPrecedence = 0, string message = null,  ParseExpressionMode mode = ParseExpressionMode.Default, bool allowAssignment = true)
        {
            if (IsStartOfExpression())
            {
                return ParseExpression(parentNode, parentExpression, newPrecedence, mode, allowAssignment);
            }
            LogError(parentNode, CurrentSpan, message ?? $"Expecting <expression> instead of `{GetAsText(Current)}`");
            return null;
        }

        private ScriptExpression ExpectAndParseExpressionAndAnonymous(ScriptNode parentNode, ParseExpressionMode mode = ParseExpressionMode.Default)
        {
            if (IsStartOfExpression())
            {
                return ParseExpression(parentNode, null, 0, mode);
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

        private bool TryGetCompoundAssignmentOperator(out ScriptToken scriptToken, out TokenType tokenType)
        {
            tokenType = this.Current.Type;
            scriptToken = tokenType switch
            {
                TokenType.Equal => ScriptToken.Equal(),
                TokenType.PlusEqual => ScriptToken.PlusEqual(),
                TokenType.MinusEqual => ScriptToken.MinusEqual(),
                TokenType.AsteriskEqual => ScriptToken.StarEqual(),
                TokenType.DivideEqual => ScriptToken.DivideEqual(),
                TokenType.DoubleDivideEqual => ScriptToken.DoubleDivideEqual(),
                TokenType.PercentEqual => ScriptToken.ModulusEqual(),
                _ => default
            };
            return !(scriptToken is null);
        }
        private bool IsStartingAsUnaryExpression()
        {
            switch (Current.Type)
            {
                case TokenType.Exclamation:
                case TokenType.Minus:
                case TokenType.Plus:
                case TokenType.Arroba:
                case TokenType.DoublePlus:
                case TokenType.DoubleMinus:
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
                case ScriptBinaryOperator.NotEmptyCoalescing:
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
                case ScriptBinaryOperator.Subtract:
                    return PrecedenceOfAdd;
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                    return PrecedenceOfMultiply;
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
                case ScriptUnaryOperator.Decrement:
                case ScriptUnaryOperator.Increment:
                    // Increment and decrement are "primary expressions" in C#, higher precedence than unary operators
                    return 210;
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
            /// All expressions (e.g literals, function calls, function pipes...etc.)
            /// </summary>
            DefaultNoNamedArgument,

            /// <summary>
            /// Only literal, unary, nested, array/object initializer, dot access, array access
            /// </summary>
            BasicExpression,

            /// <summary>
            /// A when expression cannot use `||`, ',' or 'or' at the top-level as they are used to separate expressions.
            /// </summary>
            WhenExpression,
        }
    }
}
