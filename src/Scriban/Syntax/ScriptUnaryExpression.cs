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
        public ScriptUnaryOperator Operator { get; set; }

        public ScriptToken OperatorToken { get; set; }

        public string OperatorAsText => OperatorToken?.Value ?? Operator.ToText();

        public ScriptExpression Right { get; set; }

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
                            throw new ScriptRuntimeException(Right.Span, $"Expecting a boolean instead of {value?.GetType().ScriptFriendlyName()} value: {value}");
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

        public override void Write(TemplateRewriterContext context)
        {
            if (OperatorToken != null)
            {
                context.Write(OperatorToken);
            }
            else
            {
                context.Write(Operator.ToText());
            }
            context.Write(Right);
        }

        public override string ToString()
        {
            return $"{OperatorAsText}{Right}";
        }

        public override void Accept(ScriptVisitor visitor) => visitor.Visit(this);

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => visitor.Visit(this);

        protected override IEnumerable<ScriptNode> GetChildren()
        {
            yield return Right;
        }
    }
}