// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.IO;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("unary expression", "<operator> <expression>")]
    public partial class ScriptUnaryExpression : ScriptExpression
    {
        public ScriptUnaryOperator Operator { get; set; }

        public ScriptExpression Right { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            switch (Operator)
            {
                case ScriptUnaryOperator.Not:
                {
                    var value = context.Evaluate(Right);
                    return !context.ToBool(Right.Span, value);
                }
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                {
                    var value = context.Evaluate(Right);

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

            throw new ScriptRuntimeException(Span, $"Operator `{Operator}` is not supported");
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Operator.ToText());
            context.Write(Right);
        }

        public override string ToString()
        {
            return $"{Operator}{Right}";
        }
    }
}