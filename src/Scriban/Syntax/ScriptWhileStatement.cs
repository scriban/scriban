// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("while statement", "while <expression> ... end")]
    public partial class ScriptWhileStatement : ScriptLoopStatementBase
    {
        public ScriptExpression Condition { get; set; }

        protected override object LoopItem(TemplateContext context, LoopState state)
        {
            return context.Evaluate(Body);
        }

        protected override object EvaluateImpl(TemplateContext context)
        {
            var index = 0;
            object result = null;
            BeforeLoop(context);

            var loopState = CreateLoopState();
            context.SetValue(ScriptVariable.WhileObject, loopState);

            while (context.StepLoop(this))
            {
                var conditionResult = context.ToBool(Condition.Span, context.Evaluate(Condition));
                if (!conditionResult)
                {
                    break;
                }

                loopState.Index = index++;
                loopState.LocalIndex = index;
                loopState.IsLast = false;
                
                result = LoopItem(context, loopState);

                if (!ContinueLoop(context))
                {
                    break;
                }
            };
            AfterLoop(context);
            return result;
        }

        public override void Write(TemplateRewriterContext context)
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