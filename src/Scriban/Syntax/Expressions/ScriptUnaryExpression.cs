// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("unary expression", "<operator> <expression>")]
    public partial class ScriptUnaryExpression : ScriptExpression
    {
        private ScriptToken _operatorToken;
        private ScriptExpression _right;
        public ScriptUnaryOperator Operator { get; set; }

        public ScriptToken OperatorToken
        {
            get => _operatorToken;
            set => ParentToThis(ref _operatorToken, value);
        }

        public string OperatorAsText => OperatorToken?.Value ?? Operator.ToText();

        public ScriptExpression Right
        {
            get => _right;
            set => ParentToThis(ref _right, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            if (Operator == ScriptUnaryOperator.FunctionAlias)
            {
                return context.Evaluate(Right, true);
            }

            var value = context.Evaluate(Right);

            if (value is IScriptCustomUnaryOperation customUnary)
            {
                return customUnary.Evaluate(context, Right.Span, Operator, value);
            }

            switch (Operator)
            {
                case ScriptUnaryOperator.Not:
                {
                    if (context.UseScientific)
                    {
                        if (!(value is bool))
                        {
                            throw new ScriptRuntimeException(Right.Span, $"Expecting a boolean instead of {value?.GetType().ScriptPrettyName()} value: {value}");
                        }
                        return !(bool) value;
                    }
                    else
                    {
                        return !context.ToBool(Right.Span, value);
                    }
                }
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                {
                    bool negate = Operator == ScriptUnaryOperator.Negate;

                    if (value != null)
                    {
                        if (value is int)
                        {
                            return negate ? -((int) value) : value;
                        }
                        else if (value is double)
                        {
                            return negate ? -((double) value) : value;
                        }
                        else if (value is float)
                        {
                            return negate ? -((float) value) : value;
                        }
                        else if (value is long)
                        {
                            return negate ? -((long) value) : value;
                        }
                        else if (value is decimal)
                        {
                            return negate ? -((decimal)value) : value;
                        }
                        else if (value is BigInteger)
                        {
                            return negate ? -((BigInteger)value) : value;
                        }
                        else
                        {
                            throw new ScriptRuntimeException(this.Span, $"Unexpected value `{value} / Type: {value?.GetType()}`. Cannot negate(-)/positive(+) a non-numeric value");
                        }
                    }
                }
                    break;
                case ScriptUnaryOperator.FunctionAlias:
                    return context.Evaluate(Right, true);

                case ScriptUnaryOperator.FunctionParametersExpand:
                    // Function parameters expand is done at the function level, so here, we simply return the actual list
                    return context.Evaluate(Right);
            }

            throw new ScriptRuntimeException(Span, $"Operator `{OperatorAsText}` is not supported");
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (OperatorToken != null)
            {
                printer.Write(OperatorToken);
            }
            else
            {
                printer.Write(Operator.ToText());
            }
            printer.Write(Right);
        }


        public static ScriptUnaryExpression Wrap(ScriptUnaryOperator unaryOperator, ScriptToken unaryToken, ScriptExpression expression, bool transferTrivia)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var unary = new ScriptUnaryExpression()
            {
                Span = expression.Span,
                Operator = unaryOperator,
                OperatorToken = unaryToken,
                Right = expression,
            };

            if (!transferTrivia) return unary;

            var firstTerminal = expression.FindFirstTerminal();
            firstTerminal?.MoveLeadingTriviasTo(unary.OperatorToken);

            return unary;
        }
    }
}