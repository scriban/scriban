// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    /// <summary>
    /// Used to rewrite a function call expression at evaluation time based
    /// on the arguments required by a function. Used by scientific mode scripting.
    /// </summary>
    internal partial class ScientificFunctionCallRewriter : List<ScriptExpression>
    {
        private int _index;

        public ScientificFunctionCallRewriter()
        {
        }

        public ScientificFunctionCallRewriter(int capacity) : base(capacity)
        {
        }

        public ScriptExpression Rewrite(TemplateContext context)
        {
            _index = 0;
            return Rewrite(context, PrecedenceTopLevel);
        }

        private static readonly int PrecedenceOfMultiply = Parser.GetDefaultBinaryOperatorPrecedence(ScriptBinaryOperator.Multiply);

        private static readonly int PrecedenceTopLevel = PrecedenceOfMultiply - 1;

        private ScriptExpression Rewrite(TemplateContext context, int precedence)
        {
            ScriptExpression leftValue = null;

            while (_index < Count)
            {
                ScriptExpression nextExpression = this[_index];

                // leftValue (*/^%)
                if (nextExpression is ScriptArgumentBinary bin)
                {
                    if (leftValue == null)
                    {
                        throw new ScriptRuntimeException(nextExpression.Span, precedence == 0 ? "This operator cannot be after a function call." : "This operator cannot be applied here.");
                    }
                    
                    // Power has higher precedence than */%
                    var newPrecedence = Parser.GetDefaultBinaryOperatorPrecedence(bin.Operator);

                    if (newPrecedence <= precedence) // if new operator has lower precedence than previous, exit
                    {
                        break;
                    }

                    var rightArgExpr = this[_index + 1];
                    if (rightArgExpr is IScriptVariablePath)
                    {
                        var rightArg = context.Evaluate(rightArgExpr, true);
                        if (rightArg is IScriptCustomFunction function)
                        {
                            var maxArg = function.RequiredParameterCount;
                            if (maxArg >= 1 && PrecedenceTopLevel != precedence)
                            {
                                break;
                            }
                        }
                    }

                    // Skip the binary argument
                    _index++;
                    var rightValue = Rewrite(context, newPrecedence);

                    leftValue = new ScriptBinaryExpression()
                    {
                        Left = leftValue,
                        Operator = bin.Operator,
                        Right = rightValue,
                    };
                    continue;
                }

                if (nextExpression is IScriptVariablePath)
                {
                    var restoreStrictVariables = context.StrictVariables;

                    // Don't fail on trying to lookup for a variable
                    context.StrictVariables = false;
                    object result = null;
                    try
                    {
                        result = context.Evaluate(nextExpression, true);
                    }
                    finally
                    {
                        context.StrictVariables = restoreStrictVariables;
                    }

                    // If one argument is a function, the remaining arguments 
                    if (result is IScriptCustomFunction function)
                    {
                        var maxArg = function.RequiredParameterCount;
                        if (maxArg > 1)
                        {
                            throw new ScriptRuntimeException(nextExpression.Span, $"Cannot use a function with more than 1 argument ({maxArg}) in a sequence of implicit multiplications.");
                        }

                        if (maxArg == 1 || function.IsExpressionParameter(0))
                        {
                            if (PrecedenceTopLevel == precedence || leftValue == null)
                            {
                                var functionCall = new ScriptFunctionCall { Target = nextExpression, ExplicitCall = true };
                                _index++;

                                var arg = Rewrite(context, 0);

                                functionCall.Arguments.Add(arg);

                                if (leftValue == null)
                                {
                                    leftValue = functionCall;
                                }
                                else
                                {
                                    leftValue = new ScriptBinaryExpression()
                                    {
                                        Left = leftValue,
                                        Operator = ScriptBinaryOperator.Multiply,
                                        Right = functionCall,
                                    };
                                }
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                {
                    if (leftValue == null)
                    {
                        leftValue = nextExpression;
                        _index++;
                    }
                    else
                    {
                        int precedenceOfImplicitMultiply = PrecedenceOfMultiply + 1;

                        if (precedenceOfImplicitMultiply <= precedence)
                        {
                            break;
                        }
                        // Implicit  the binary argument
                        var rightValue = Rewrite(context, precedenceOfImplicitMultiply);

                        if (rightValue is ScriptLiteral)
                        {
                            throw new ScriptRuntimeException(rightValue.Span, "Cannot use a literal on the right side of an implicit multiplication.");
                        }

                        leftValue = new ScriptBinaryExpression()
                        {
                            Left = leftValue,
                            Operator = ScriptBinaryOperator.Multiply,
                            Right = rightValue,
                        };
                    }
                }
            }

            return leftValue;
        }
    }
}