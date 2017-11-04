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
            BeforeLoop(context);
            while (context.StepLoop(this))
            {
                var conditionResult = context.ToBool(context.Evaluate(Condition));
                if (!conditionResult)
                {
                    break;
                }

                if (!Loop(context, index++, index, false))
                {
                    break;
                }
            };
            AfterLoop(context);
        }

        public override void Write(RenderContext context)
        {
            context.Write("while").ExpectSpace();
            context.Write(Condition);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
        
        public override string ToString()
        {
            return $"while {Condition} ... end";
        }
    }
}