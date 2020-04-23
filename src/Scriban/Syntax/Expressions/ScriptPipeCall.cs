// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("pipe expression", "<expression> | <expression>")]
    public partial class ScriptPipeCall : ScriptExpression
    {
        private ScriptExpression _from;
        private ScriptToken _pipeToken;
        private ScriptExpression _to;

        public ScriptExpression From
        {
            get => _from;
            set => ParentToThis(ref _from, value);
        }

        public ScriptToken PipeToken
        {
            get => _pipeToken;
            set => ParentToThis(ref _pipeToken, value);
        }

        public ScriptExpression To
        {
            get => _to;
            set => ParentToThis(ref _to, value);
        }

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

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(From);
            printer.Write(PipeToken);
            printer.Write(To);
        }
    }
}