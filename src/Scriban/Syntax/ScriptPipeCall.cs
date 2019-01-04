// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections;
using System.IO;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("pipe expression", "<expression> | <expression>")]
    public partial class ScriptPipeCall : ScriptExpression
    {
        public ScriptExpression From { get; set; }

        public ScriptExpression To { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            // We don't evaluate the From but we let the pipe evaluate it later
            var leftResult = context.Evaluate(From);

            // Push a new pipe arguments
            context.PushPipeArguments();
            try
            {
                // Support for Parameters expansion
                var unaryExpression = From as ScriptUnaryExpression;
                if (unaryExpression != null && unaryExpression.Operator == ScriptUnaryOperator.FunctionParametersExpand)
                {
                    // TODO: Pipe calls will not work correctly in case of (a | b) | ( c | d)
                    var valueEnumerator = leftResult as IEnumerable;
                    if (valueEnumerator != null)
                    {
                        var pipeArguments = context.PipeArguments;
                        foreach (var subValue in valueEnumerator)
                        {
                            pipeArguments.Add(subValue);
                        }
                    }
                    else
                    {
                        context.PipeArguments.Add(leftResult);
                    }
                }
                else
                {
                    context.PipeArguments.Add(leftResult);
                }

                var result = context.Evaluate(To);

                // If we have still remaining arguments, it is likely that the destination expression is not a function
                // so pipe arguments were not picked up and this is an error
                if (context.PipeArguments.Count > 0)
                {
                    throw new ScriptRuntimeException(To.Span, $"Pipe expression destination `{To}` is not a valid function ");
                }
                return result;
            }
            finally
            {
                context.PopPipeArguments();
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(From);
            context.Write("|");
            context.Write(To);
        }

        public override string ToString()
        {
            return $"{From} | {To}";
        }
    }
}