// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections;
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("unary expression", "<operator> <expression>")]
    public class ScriptUnaryExpression : ScriptExpression
    {
        public ScriptUnaryOperator Operator { get; set; }

        public ScriptExpression Right { get; set; }

        public override string ToString()
        {
            return $"{Operator}{Right}";
        }

        public bool ExpandParameters(object value, ScriptArray expandedParameters)
        {
            // Handle parameters expansion for a function call when the operator ~ is used
            if (Operator == ScriptUnaryOperator.FunctionParametersExpand)
            {
                var valueEnumerator = value as IEnumerable;
                if (valueEnumerator != null)
                {
                    foreach (var subValue in valueEnumerator)
                    {
                        expandedParameters.Add(subValue);
                    }
                    return true;
                }
            }
            return false;
        }

        public override void Evaluate(TemplateContext context)
        {
            switch (Operator)
            {
                case ScriptUnaryOperator.Not:
                {
                    var value = context.Evaluate(Right);
                    context.Result = !ScriptValueConverter.ToBool(value);
                }
                    break;
                case ScriptUnaryOperator.Negate:
                case ScriptUnaryOperator.Plus:
                {
                    var value = context.Evaluate(Right);

                    bool negate = Operator == ScriptUnaryOperator.Negate;

                    var customType = value as IScriptCustomType;
                    if (customType != null)
                    {
                        context.Result = customType.EvaluateUnaryExpression(this);
                    }
                    else if (value != null)
                    {
                        if (value is int)
                        {
                            context.Result = negate ? -((int) value) : value;
                        }
                        else if (value is double)
                        {
                            context.Result = negate ? -((double) value) : value;
                        }
                        else if (value is float)
                        {
                            context.Result = negate ? -((float) value) : value;
                        }
                        else if (value is long)
                        {
                            context.Result = negate ? -((long) value) : value;
                        }
                        else
                        {
                            throw new ScriptRuntimeException(this.Span,
                                $"Unexpected value [{value} / Type: {value?.GetType()}]. Cannot negate(-)/positive(+) a non-numeric value");
                        }
                    }
                }
                    break;
                case ScriptUnaryOperator.FunctionAlias:
                    context.Result = context.Evaluate(Right, true);
                    break;

                case ScriptUnaryOperator.FunctionParametersExpand:
                    // Function parameters expand is done at the function level, so here, we simply return the actual list
                    Right?.Evaluate(context);
                    break;
                default:
                    throw new ScriptRuntimeException(Span, $"Operator [{Operator}] is not supported");
            }
        }
    }
}