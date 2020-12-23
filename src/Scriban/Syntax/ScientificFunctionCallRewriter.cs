// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
        private static readonly int PrecedenceOfMultiply = Parser.GetDefaultBinaryOperatorPrecedence(ScriptBinaryOperator.Multiply);
        private static readonly int PrecedenceTopLevel = PrecedenceOfMultiply - 1;
        private int _index;
        private ScriptNode _parent;

        public ScientificFunctionCallRewriter()
        {
        }

        public ScientificFunctionCallRewriter(int capacity) : base(capacity)
        {
        }

        public ScriptExpression Rewrite(TemplateContext context, ScriptNode parent)
        {
            _parent = parent;
            _index = 0;
            var node = Rewrite(context, PrecedenceTopLevel);
            if (node.Parent == null)
            {
                node.Parent = _parent?.Parent;
            }
            return node;
        }

        private ScriptExpression Rewrite(TemplateContext context, int precedence, bool expectingExpression = false)
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
                        OperatorToken = bin.Operator.ToToken(),
                        Right = rightValue,
                    };
                    continue;
                }

                object result = null;
                if (!expectingExpression && nextExpression is IScriptVariablePath)
                {
                    var restoreStrictVariables = context.StrictVariables;

                    // Don't fail on trying to lookup for a variable
                    context.StrictVariables = false;
                    try
                    {
                        result = context.Evaluate(nextExpression, true);
                    }
                    catch (ScriptRuntimeException) when (context.IgnoreExceptionsWhileRewritingScientific)
                    {
                        // ignore any exceptions during trial evaluating as we could try to evaluate
                        // variable that aren't setup
                    }
                    finally
                    {
                        context.StrictVariables = restoreStrictVariables;
                    }

                    // If one argument is a function, the remaining arguments
                    if (result is IScriptCustomFunction function)
                    {
                        var maxArg = function.RequiredParameterCount != 0 ? function.RequiredParameterCount : function.ParameterCount > 0 ? 1 : 0;

                        if (maxArg > 0)
                        {
                            if (PrecedenceTopLevel == precedence || leftValue == null)
                            {
                                var functionCall = new ScriptFunctionCall { Target = (ScriptExpression)nextExpression.Clone(), ExplicitCall = true };
                                _index++;

                                var isExpectingExpression = function.IsParameterType<ScriptExpression>(0);

                                if (_index == Count)
                                {
                                    throw new ScriptRuntimeException(nextExpression.Span, "The function is expecting a parameter");
                                }

                                var arg = Rewrite(context, 0, isExpectingExpression);

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
                                        OperatorToken = ScriptToken.Star(),
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
                        leftValue = (ScriptExpression)nextExpression.Clone();
                        _index++;
                    }
                    else
                    {
                        int precedenceOfImplicitMultiply = result is IScriptCustomImplicitMultiplyPrecedence ? PrecedenceOfMultiply : PrecedenceOfMultiply + 1;

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
                            OperatorToken = ScriptToken.Star(),
                            Right = rightValue,
                        };
                    }
                }
            }

            return leftValue;
        }
    }
}