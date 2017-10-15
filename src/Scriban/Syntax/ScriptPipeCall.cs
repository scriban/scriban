// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("pipe expression", "<expression> | <expression>")]
    public class ScriptPipeCall : ScriptExpression
    {
        public ScriptExpression From { get; set; }

        public ScriptExpression To { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            int beforePipeArgumentCount = context.PipeArguments.Count + 1;

            // We don't evaluate the From but we let the pipe evalute it later
            context.PipeArguments.Push(From);
            var result = context.Evaluate(To);

            int afterPipeArgumentCount = context.PipeArguments.Count;
            if (afterPipeArgumentCount >= beforePipeArgumentCount)
            {
                throw new ScriptRuntimeException(To.Span, $"Pipe expression destination [{To}] is not a valid function ");
            }
            return result;
        }

        public override string ToString()
        {
            return $"{From} | {To}";
        }
    }
}