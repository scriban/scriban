// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("while statement", "while <expression> ... end")]
    public class ScriptWhileStatement : ScriptLoopStatementBase
    {
        public ScriptExpression Condition { get; set; }

        protected override void EvaluateImpl(TemplateContext context)
        {
            var index = 0;
            while(context.StepLoop(this))
            {
                var conditionResult = context.ToBool(context.Evaluate(Condition));
                if (!conditionResult)
                {
                    break;
                }

                if (!Loop(context, index++))
                {
                    break;
                }
            };
        }

        protected override void WriteImpl(RenderContext context)
        {
            context.Write("while").WithSpace();
            Condition?.Write(context);
            context.WithEos();
            Body?.Write(context);
            WriteEnd(context);
        }
        
        public override string ToString()
        {
            return $"while {Condition} ... end";
        }
    }
}