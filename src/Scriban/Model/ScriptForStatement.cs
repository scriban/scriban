// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections;
using Scriban.Runtime;

namespace Scriban.Model
{
    /// <summary>
    /// A for in loop statement.
    /// </summary>
    [ScriptSyntax("for statement", "for <variable> in <expression> ... end")]
    public class ScriptForStatement : ScriptLoopStatementBase
    {
        public ScriptVariable Variable { get; set; }

        public ScriptExpression Iterator { get; set; }

        protected override void EvaluateImpl(TemplateContext context)
        {
            var loopIterator = context.Evaluate(Iterator);
            var iterator = loopIterator as IEnumerable;
            if (iterator != null)
            {
                var index = -1;
                object previousValue = null;
                bool escape = false;

                foreach (var value in iterator)
                {
                    if (!context.StepLoop())
                    {
                        return;
                    }

                    // We update on next run on previous value (in order to handle last)
                    if (index >= 0)
                    {
                        context.SetValue(ScriptVariable.LoopLast, false);
                        context.SetValue(Variable, previousValue);

                        if (!Loop(context, index))
                        {
                            escape = true;
                            break;
                        }
                    }

                    previousValue = value;
                    index++;
                }

                if (!escape && index >= 0 && context.StepLoop())
                {
                    context.SetValue(ScriptVariable.LoopLast, true);
                    context.SetValue(Variable, previousValue);
                    Loop(context, index);
                }
            }
            else if (loopIterator != null)
            {
                throw new ScriptRuntimeException(Iterator.Span, $"Unexpected type [{loopIterator.GetType()}] for iterator");
            }
        }

        public override string ToString()
        {
            return $"for {Variable} in {Iterator} ... end";
        }
    }
}