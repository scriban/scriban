// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using Scriban.Runtime;

namespace Scriban.Parsing
{
    public partial class Parser
    {
		private static readonly Dictionary<TokenType, ScriptBinaryOperator> BinaryOperators = new Dictionary<TokenType, ScriptBinaryOperator>();

        private int allowNewLineLevel = 0;
        private int expressionLevel = 0;

        static Parser()
        {
            BinaryOperators.Add(TokenType.Multiply, ScriptBinaryOperator.Multiply);
            BinaryOperators.Add(TokenType.Divide, ScriptBinaryOperator.Divide);
            BinaryOperators.Add(TokenType.DoubleDivide, ScriptBinaryOperator.DivideRound);
            BinaryOperators.Add(TokenType.Plus, ScriptBinaryOperator.Add);
            BinaryOperators.Add(TokenType.Minus, ScriptBinaryOperator.Substract);
            BinaryOperators.Add(TokenType.Modulus, ScriptBinaryOperator.Modulus);
            BinaryOperators.Add(TokenType.ShiftLeft, ScriptBinaryOperator.ShiftLeft);
            BinaryOperators.Add(TokenType.ShiftRight, ScriptBinaryOperator.ShiftRight);
            BinaryOperators.Add(TokenType.EmptyCoalescing, ScriptBinaryOperator.EmptyCoalescing);
            BinaryOperators.Add(TokenType.And, ScriptBinaryOperator.And);
            BinaryOperators.Add(TokenType.Or, ScriptBinaryOperator.Or);
            BinaryOperators.Add(TokenType.CompareEqual, ScriptBinaryOperator.CompareEqual);
            BinaryOperators.Add(TokenType.CompareNotEqual, ScriptBinaryOperator.CompareNotEqual);
            BinaryOperators.Add(TokenType.CompareGreater, ScriptBinaryOperator.CompareGreater);
            BinaryOperators.Add(TokenType.CompareGreaterOrEqual, ScriptBinaryOperator.CompareGreaterOrEqual);
            BinaryOperators.Add(TokenType.CompareLess, ScriptBinaryOperator.CompareLess);
            BinaryOperators.Add(TokenType.CompareLessOrEqual, ScriptBinaryOperator.CompareLessOrEqual);
            BinaryOperators.Add(TokenType.DoubleDot, ScriptBinaryOperator.RangeInclude);
            BinaryOperators.Add(TokenType.DoubleDotLess, ScriptBinaryOperator.RangeExclude);
        }

        private bool StartAsExpression()
        {
            switch (Current.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                case TokenType.Integer:
                case TokenType.Float:
                case TokenType.String:
                case TokenType.VerbatimString:
                case TokenType.OpenParent:
                case TokenType.OpenBrace:
                case TokenType.OpenBracket:
                case TokenType.Not:
                case TokenType.Minus:
                case TokenType.Plus:
                case TokenType.Arroba:
                case TokenType.Caret:
                    return true;
            }

            return false;
        }

        private ScriptExpression ParseUnaryExpression(ref bool hasAnonymousFunction)
        {
            // unit test: 113-unary.txt
            var unaryExpression = Open<ScriptUnaryExpression>();
            switch (Current.Type)
            {
                case TokenType.Not:
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
                case TokenType.Caret:
                    unaryExpression.Operator = ScriptUnaryOperator.FunctionParametersExpand;
                    break;
                default:
                    LogError($"Unexpected token [{Current.Type}] for unary expression");
                    break;
            }
            var newPrecedence = GetOperatorPrecedence(unaryExpression.Operator);
            NextToken();
            // unit test: 115-unary-error1.txt
            unaryExpression.Right = ExpectAndParseExpression(unaryExpression, ref hasAnonymousFunction, null, newPrecedence);
            return Close(unaryExpression);
        }

        private ScriptExpression ParseExpression(ScriptNode parentNode, ScriptExpression parentExpression = null, int precedence = 0)
        {
            bool hasAnonymousFunction = false;
            return ParseExpression(parentNode, ref hasAnonymousFunction, parentExpression, precedence);
        }

        private ScriptExpression ParseExpression(ScriptNode parentNode, ref bool hasAnonymousFunction, ScriptExpression parentExpression = null, int precedence = 0)
        {
            int expressionCount = 0;
            expressionLevel++;
            try
            {
                ScriptFunctionCall functionCall = null;
                parseExpression:
                expressionCount++;
                ScriptExpression leftOperand = null;
                switch (Current.Type)
                {
                    case TokenType.Identifier:
                    case TokenType.IdentifierSpecial:
                        leftOperand = ParseVariableOrLiteral();

                        // Special handle of the $$ block delegate variable
                        if (ScriptVariable.BlockDelegate.Equals(leftOperand))
                        {
                            if (expressionCount != 1 || expressionLevel > 1)
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
                    case TokenType.Float:
                        leftOperand = ParseFloat();
                        break;
                    case TokenType.String:
                        leftOperand = ParseString();
                        break;
                    case TokenType.VerbatimString:
                        leftOperand = ParseVerbatimString();
                        break;
                    case TokenType.OpenParent:
                        leftOperand = ParseParenthesis(ref hasAnonymousFunction);
                        break;
                    case TokenType.OpenBrace:
                        leftOperand = ParseObjectInitializer();
                        break;
                    case TokenType.OpenBracket:
                        leftOperand = ParseArrayInitializer();
                        break;
                    case TokenType.Not:
                    case TokenType.Minus:
                    case TokenType.Arroba:
                    case TokenType.Plus:
                    case TokenType.Caret:
                        leftOperand = ParseUnaryExpression(ref hasAnonymousFunction);
                        break;
                }

                // Should not happen but in case
                if (leftOperand == null)
                {
                    LogError($"Unexpected token [{Current.Type}] for expression");
                    return null;
                }

                if (leftOperand is ScriptAnonymousFunction)
                {
                    hasAnonymousFunction = true;
                }

                while (!hasAnonymousFunction)
                {
                    // Parse Member expression are expected to be followed only by an identifier
                    if (Current.Type == TokenType.Dot)
                    {
                        var nextToken = PeekToken();
                        if (nextToken.Type == TokenType.Identifier)
                        {
                            NextToken();
                            var memberExpression = Open<ScriptMemberExpression>();
                            memberExpression.Target = leftOperand;
                            var member = ParseVariableOrLiteral();
                            if (!(member is ScriptVariable))
                            {
                                LogError("Unexpected literal member [{member}]");
                                return null;
                            }
                            memberExpression.Member = (ScriptVariable) member;
                            leftOperand = Close(memberExpression);
                        }
                        else
                        {
                            LogError(nextToken,
                                $"Invalid token [{nextToken.Type}]. The dot operator is expected to be followed by a plain identifier");
                            return null;
                        }
                        continue;
                    }

                    // If we have a bracket but left operand is a (variable || member || indexer), then we consider next as an indexer
                    // unit test: 130-indexer-accessor-accept1.txt
                    if (Current.Type == TokenType.OpenBracket && leftOperand is ScriptVariablePath && !IsPreviousCharWhitespace())
                    {
                        NextToken();
                        var indexerExpression = Open<ScriptIndexerExpression>();
                        indexerExpression.Target = leftOperand;
                        // unit test: 130-indexer-accessor-error5.txt
                        indexerExpression.Index = ExpectAndParseExpression(indexerExpression, ref hasAnonymousFunction, functionCall, 0, $"Expecting <index_expression> instead of [{Current.Type}]");

                        if (Current.Type != TokenType.CloseBracket)
                        {
                            LogError($"Unexpected [{Current.Type}]. Expecting ']'");
                        }
                        else
                        {
                            NextToken();
                        }

                        leftOperand = Close(indexerExpression);
                        continue;
                    }

                    if (Current.Type == TokenType.Equal)
                    {
                        var assignExpression = Open<ScriptAssignExpression>();

                        if (expressionLevel > 1)
                        {
                            // unit test: 101-assign-complex-error1.txt
                            LogError(assignExpression, $"Expression is only allowed for a top level assignment");
                        }

                        NextToken();
                        assignExpression.Target = leftOperand;

                        // unit test: 105-assign-error3.txt
                        assignExpression.Value = ExpectAndParseExpression(assignExpression, ref hasAnonymousFunction, parentExpression);

                        leftOperand = Close(assignExpression);
                        continue;
                    }

                    // Handle binary operators here
                    ScriptBinaryOperator binaryOperatorType;
                    if (BinaryOperators.TryGetValue(Current.Type, out binaryOperatorType))
                    {
                        var newPrecedence = GetOperatorPrecedence(binaryOperatorType);

                        // Check precedence to see if we should "take" this operator here (Thanks TimJones for the tip code! ;)
                        if (newPrecedence < precedence)
                            break;

                        var binaryExpression = Open<ScriptBinaryExpression>();
                        binaryExpression.Left = leftOperand;
                        binaryExpression.Operator = binaryOperatorType;

                        NextToken(); // skip the operator

                        // unit test: 110-binary-simple-error1.txt
                        binaryExpression.Right = ExpectAndParseExpression(binaryExpression, ref hasAnonymousFunction,
                            functionCall ?? parentExpression, newPrecedence,
                            $"Expecting an <expression> to the right of the operator instead of [{Current.Type}]");
                        leftOperand = Close(binaryExpression);

                        continue;
                    }

                    if (precedence > 0)
                    {
                        break;
                    }

                    // If we can parse a statement, we have a method call
                    if (StartAsExpression())
                    {
                        if (parentExpression != null)
                        {
                            break;
                        }

                        if (functionCall == null)
                        {
                            functionCall = Open<ScriptFunctionCall>();
                            functionCall.Target = leftOperand;
                        }
                        else
                        {
                            functionCall.Arguments.Add(leftOperand);
                        }
                        goto parseExpression;
                    }

                    if (Current.Type == TokenType.Pipe)
                    {
                        if (functionCall != null)
                        {
                            functionCall.Arguments.Add(leftOperand);
                            leftOperand = functionCall;
                        }

                        var pipeCall = Open<ScriptPipeCall>();
                        pipeCall.From = leftOperand;
                        NextToken(); // skip |

                        // unit test: 310-func-pipe-error1.txt
                        pipeCall.To = ExpectAndParseExpression(pipeCall, ref hasAnonymousFunction);
                        return Close(pipeCall);
                    }

                    break;
                }

                if (functionCall != null)
                {
                    functionCall.Arguments.Add(leftOperand);
                    return functionCall;
                }
                return Close(leftOperand);
            }
            finally
            {
                expressionLevel--;
            }
        }

        private ScriptExpression ParseArrayInitializer()
        {
            var scriptArray = Open<ScriptArrayInitializerExpression>();

            // Should happen before the NextToken to consume any EOL after
            allowNewLineLevel++;
            NextToken(); // Skip [

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
                        NextToken();
                    }
                    else
                    {
                        expectingEndOfInitializer = true;
                    }
                }
                else
                {
                    // unit test: 120-array-initializer-error1.txt
                    LogError($"Unexpected token [{Current.Type}]. Expecting a closing ] for the array initializer");
                    break;
                }
            }

            // Should happen before NextToken() to stop on the next EOF
            allowNewLineLevel--;
            NextToken(); // Skip ]

            return Close(scriptArray);
        }

        private ScriptExpression ParseObjectInitializer()
        {
            var scriptObject = Open<ScriptObjectInitializerExpression>();

            // Should happen before the NextToken to consume any EOL after
            allowNewLineLevel++;
            NextToken(); // Skip {

            // unit test: 140-object-initializer-accessor.txt
            bool expectingEndOfInitializer = false;
            while (true)
            {
                if (Current.Type == TokenType.CloseBrace)
                {
                    break;
                }

                if (!expectingEndOfInitializer && (Current.Type == TokenType.Identifier || Current.Type == TokenType.String))
                {
                    var positionBefore = Current;
                    var variableOrLiteral = ParseExpression(scriptObject);
                    var variable = variableOrLiteral as ScriptVariable;
                    var literal = variableOrLiteral as ScriptLiteral;
                    
                    if (variable == null && literal == null)
                    {
                        LogError(positionBefore, $"Unexpected member type [{variableOrLiteral}/{ScriptSyntaxAttribute.Get(variableOrLiteral).Name}] found for object initializer member name");
                        break;
                    }

                    if (literal != null && !(literal.Value is string))
                    {
                        LogError(positionBefore, $"Invalid literal member [{literal.Value}/{literal.Value?.GetType()}] found for object initializer member name. Only literal string or identifier name are allowed");
                        break;
                    }

                    if (variable != null)
                    {
                        if (IsKeyword(variable.Name))
                        {
                            // unit test: 140-object-initializer-error2.txt
                            LogError(positionBefore,
                                $"Invalid Keyword [{variable}] found for object initializer member name");
                            break;
                        }

                        if (variable.Scope != ScriptVariableScope.Global)
                        {
                            // unit test: 140-object-initializer-error3.txt
                            LogError("Expecting a simple identifier for member names");
                            break;
                        }
                    }

                    if (Current.Type != TokenType.Colon)
                    {
                        // unit test: 140-object-initializer-error4.txt
                        LogError($"Unexpected token [{Current.Type}] Expecting a colon : after identifier [{variable.Name}] for object initializer member name");
                        break;
                    }

                    NextToken(); // Skip :

                    if (!StartAsExpression())
                    {
                        // unit test: 140-object-initializer-error5.txt
                        LogError($"Unexpected token [{Current.Type}]. Expecting an expression for the value of the member instead of [{GetAsText(Current)}]");
                        break;
                    }

                    var expression = ParseExpression(scriptObject);

                    // Erase any previous declaration of this member
                    scriptObject.Members[variableOrLiteral] = expression;

                    if (Current.Type == TokenType.Comma)
                    {
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
                    LogError($"Unexpected token [{Current.Type}] while parsing object initializer. Expecting a simple identifier for the member name instead of [{GetAsText(Current)}]");
                    break;
                }
            }

            // Should happen before NextToken() to stop on the next EOF
            allowNewLineLevel--;
            NextToken(); // Skip }

            return Close(scriptObject);
        }

        private ScriptExpression ParseParenthesis(ref bool hasAnonymousFunction)
        {
            // unit test: 106-parenthesis.txt
            var expression = Open<ScriptNestedExpression>();
            NextToken(); // Skip (
            expression.Expression = ExpectAndParseExpression(expression, ref hasAnonymousFunction);

            if (Current.Type == TokenType.CloseParent)
            {
                NextToken();
            }
            else
            {
                // unit test: 106-parenthesis-error1.txt
                LogError(Current, $"Invalid token [{Current.Type}]. Expecting closing ) for opening [{expression.Span.Start}]");
            }
            return Close(expression);
        }

        bool IsPreviousCharWhitespace()
        {
            int position = Current.Start.Offset - 1;
            if (position >= 0)
            {
                return char.IsWhiteSpace(lexer.Text[position]);
            }
            return false;
        }

        private static int GetOperatorPrecedence(ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.EmptyCoalescing:
                    return 20;
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                    return 25;
                case ScriptBinaryOperator.Or:
                    return 30;
                case ScriptBinaryOperator.And:
                    return 40;
                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                    return 50;
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return 60;
                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Substract:
                    return 70;
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                    return 80;
                case ScriptBinaryOperator.RangeInclude:
                case ScriptBinaryOperator.RangeExclude:
                    return 90;
                default:
                    return 0;
            }
        }

        private static int GetOperatorPrecedence(ScriptUnaryOperator op)
        {
            switch (op)
            {
                case ScriptUnaryOperator.Not:
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                case ScriptUnaryOperator.FunctionAlias:
                case ScriptUnaryOperator.FunctionParametersExpand:
                    return 10;
                default:
                    return 0;
            }
        }
    }
}