// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    /// <summary>
    /// Used to rewrite a function call expression at evaluation time based
    /// on the arguments required by a function. Used by scientific mode scripting.
    /// </summary>
    internal partial class ScientificFunctionCallRewriter
    {
        /// <summary>
        /// The precedence of an implicit function call is between Add (+) and Multiply (*) operator
        /// </summary>
        private const int ImplicitFunctionCallPrecedence = Parser.PrecedenceOfAdd + 1;

        public static ScriptExpression Rewrite(TemplateContext context, ScriptBinaryExpression binaryExpression)
        {
            Debug.Assert(ImplicitFunctionCallPrecedence < Parser.PrecedenceOfMultiply);

            if (!HasImplicitBinaryExpression(binaryExpression))
            {
                return binaryExpression;
            }

            // TODO: use a TLS cache
            var iterator = new BinaryExpressionIterator();
            // a b c / d + e
            // stack will contain:
            // [0] a
            // [1] implicit *
            // [2] b
            // [3] implicit *
            // [4] c
            // [5] /
            // [6] d
            // [7] +
            // [8] e
            FlattenBinaryExpressions(context, binaryExpression, iterator);

            return ParseBinaryExpressionTree(iterator, 0, false);
        }

        private static bool HasImplicitBinaryExpression(ScriptExpression expression)
        {
            if (expression is ScriptBinaryExpression binaryExpression)
            {
                if (binaryExpression.OperatorToken == null && binaryExpression.Operator == ScriptBinaryOperator.Multiply)
                {
                    return true;
                }

                return HasImplicitBinaryExpression(binaryExpression.Left) || HasImplicitBinaryExpression(binaryExpression.Right);
            }
            return false;
        }
        
        private static void FlattenBinaryExpressions(TemplateContext context, ScriptExpression expression, List<BinaryExpressionOrOperator> expressions)
        {
            while (true)
            {
                if (!(expression is ScriptBinaryExpression binaryExpression))
                {
                    expressions.Add(new BinaryExpressionOrOperator(expression, GetFunctionCallKind(context, expression)));
                    return;
                }

                var left = (ScriptExpression)binaryExpression.Left.Clone();
                var right = (ScriptExpression)binaryExpression.Right.Clone();
                var token = (ScriptToken)binaryExpression.OperatorToken?.Clone();
                FlattenBinaryExpressions(context, left, expressions);

                expressions.Add(new BinaryExpressionOrOperator(binaryExpression.Operator, token));

                // Iterate on right (equivalent to tail recursive call)
                expression = right;
            }
        }
        
        private static ScriptExpression ParseBinaryExpressionTree(BinaryExpressionIterator it, int precedence, bool isExpectingExpression)
        {
            ScriptExpression leftOperand = null;
            while (it.HasCurrent)
            {
                var op = it.Current;
                var nextOperand = op.Expression;

                if (nextOperand == null)
                {
                    var newPrecedence = Parser.GetDefaultBinaryOperatorPrecedence(op.Operator);

                    // Probe if the next argument is a function call
                    if (!isExpectingExpression && it.HasNext && it.PeekNext().CallKind != FunctionCallKind.None)
                    {
                        // If it is a function call, use its precedence
                        newPrecedence = newPrecedence < ImplicitFunctionCallPrecedence ? newPrecedence : ImplicitFunctionCallPrecedence;
                    }
                    
                    if (newPrecedence <= precedence)
                    {
                        break;
                    }

                    it.MoveNext(); // Skip operator

                    var binary = new ScriptBinaryExpression
                    {
                        Left = leftOperand,
                        Operator = op.Operator,
                        OperatorToken = op.OperatorToken ?? ScriptToken.Star(), // Force an explicit token so that we don't enter an infinite loop when formatting an expression
                        Span =
                        {
                            Start = leftOperand.Span.Start
                        },
                    };

                    binary.Right = ParseBinaryExpressionTree(it, newPrecedence, isExpectingExpression);
                    binary.Span.End = binary.Right.Span.End;
                    leftOperand = binary;
                }
                else
                {
                    if (!isExpectingExpression && op.CallKind != FunctionCallKind.None)
                    {
                        var functionCall = new ScriptFunctionCall
                        {
                            Target = nextOperand,
                            ExplicitCall = true,
                            Span =
                            {
                                Start = nextOperand.Span.Start
                            }
                        };

                        // Skip the function and move to the operator
                        if (!it.MoveNext())
                        {
                            throw new ScriptRuntimeException(nextOperand.Span, $"The function is expecting at least one argument");
                        }

                        // We are expecting only an implicit multiply. Anything else is invalid.
                        if (it.Current.Expression == null && (it.Current.Operator != ScriptBinaryOperator.Multiply || it.Current.OperatorToken != null))
                        {
                            throw new ScriptRuntimeException(nextOperand.Span, $"The function expecting one argument cannot be followed by the operator {it.Current.OperatorToken?.ToString() ?? it.Current.Operator.ToText()}");
                        }

                        // Skip the operator
                        if (!it.MoveNext())
                        {
                            throw new ScriptRuntimeException(nextOperand.Span, $"The function is expecting at least one argument");
                        }

                        var argExpression = ParseBinaryExpressionTree(it, ImplicitFunctionCallPrecedence, op.CallKind == FunctionCallKind.Expression);
                        functionCall.Arguments.Add(argExpression);
                        functionCall.Span.End = argExpression.Span.End;

                        leftOperand = functionCall;
                        continue;
                    }

                    leftOperand = nextOperand;
                    it.MoveNext();
                }
            }

            return leftOperand;
        }

        private static FunctionCallKind GetFunctionCallKind(TemplateContext context, ScriptExpression expression)
        {
            var restoreStrictVariables = context.StrictVariables;

            // Don't fail on trying to lookup for a variable
            context.StrictVariables = false;
            object result = null;
            try
            {
                result = context.Evaluate(expression, true);
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
                // We match all functions with at least one argument.
                // If we are expecting more than one argument, let the error happen later with the function call.
                if (maxArg > 0)
                {
                    var isExpectingExpression = function.IsParameterType<ScriptExpression>(0);
                    return isExpectingExpression ? FunctionCallKind.Expression : FunctionCallKind.Regular;
                }
            }

            return FunctionCallKind.None;
        }

        /// <summary>
        /// Stores a list of binary expressions to rewrite.
        /// </summary>
        [DebuggerDisplay("Count = {Count}, Current = {Index} : {Current}")]
        private class BinaryExpressionIterator : List<BinaryExpressionOrOperator>
        {
            public int Index { get; set; }

            public BinaryExpressionOrOperator Current => Index < Count ? this[Index] : default;

            public bool HasCurrent => Index < Count;

            public bool HasNext => Index + 1 < Count;

            public bool MoveNext()
            {
                Index++;
                return HasCurrent;
            }

            public BinaryExpressionOrOperator PeekNext()
            {
                return this[Index + 1];
            }

        }

        /// <summary>
        /// A binary expression part to rewrite. It is either:
        /// - An "terminal" expression
        /// - An operator (`*`, `+`...)
        /// </summary>
        [DebuggerDisplay("{" + nameof(ToDebuggerDisplay) + "(),nq}")]
        private readonly struct BinaryExpressionOrOperator
        {
            public BinaryExpressionOrOperator(ScriptExpression expression, FunctionCallKind kind)
            {
                Expression = expression;
                Operator = 0;
                OperatorToken = null;
                CallKind = kind;
            }

            public BinaryExpressionOrOperator(ScriptBinaryOperator @operator, ScriptToken operatorToken)
            {
                Expression = null;
                Operator = @operator;
                OperatorToken = operatorToken;
                CallKind = FunctionCallKind.None;
            }

            public readonly ScriptExpression Expression;

            public readonly ScriptBinaryOperator Operator;

            public readonly ScriptToken OperatorToken;

            public readonly FunctionCallKind CallKind;

            private string ToDebuggerDisplay()
            {
                return Expression != null ? Expression.ToString() : OperatorToken?.ToString() ?? $"`{Operator.ToText()}` - CallKind = {CallKind}";
            }
        }

        private enum FunctionCallKind
        {
            None,
            Regular,
            Expression
        }
    }
}