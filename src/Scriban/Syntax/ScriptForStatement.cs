// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif
using Scriban.Runtime;

namespace Scriban.Syntax
{
    /// <summary>
    /// A for in loop statement.
    /// </summary>
    [ScriptSyntax("for statement", "for <variable> in <expression> ... end")]
    public partial class ScriptForStatement : ScriptLoopStatementBase, IScriptNamedArgumentContainer
    {
        public ScriptExpression Variable { get; set; }

        public ScriptExpression Iterator { get; set; }

        public List<ScriptNamedArgument> NamedArguments { get; set; }

        internal ScriptNode IteratorOrLastParameter => NamedArguments != null && NamedArguments.Count > 0
            ? NamedArguments[NamedArguments.Count - 1]
            : Iterator;

        protected override void EvaluateImpl(TemplateContext context)
        {
            var loopIterator = context.Evaluate(Iterator);
            var list = loopIterator as IList;
            if (list == null)
            {
                var iterator = loopIterator as IEnumerable;
                if (iterator != null)
                {
                    list = new ScriptArray(iterator);
                }
            }

            if (list != null)
            {
                context.SetValue(ScriptVariable.LoopLength, list.Count);
                object previousValue = null;

                bool reversed = false;
                int startIndex = 0;
                int limit = list.Count;
                if (NamedArguments != null)
                {
                    foreach (var option in NamedArguments)
                    {
                        switch (option.Name)
                        {
                            case "offset":
                                startIndex = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                                break;
                            case "reversed":
                                reversed = true;
                                break;
                            case "limit":
                                limit = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                                break;
                            default:
                                ProcessArgument(context, option);
                                break;
                        }
                    }
                }
                var endIndex = Math.Min(limit + startIndex, list.Count) - 1;

                var index = reversed ? endIndex : startIndex;
                var dir = reversed ? -1 : 1;
                bool isFirst = true;
                int i = 0;
                BeforeLoop(context);
                while (!reversed && index <= endIndex || reversed && index >= startIndex)
                {
                    if (!context.StepLoop(this))
                    {
                        return;
                    }

                    // We update on next run on previous value (in order to handle last)
                    var value = list[index];
                    bool isLast = reversed ? index == startIndex : index == endIndex;
                    context.SetValue(ScriptVariable.LoopLast, isLast);
                    context.SetValue(ScriptVariable.LoopChanged, isFirst || !Equals(previousValue, value));
                    context.SetValue(ScriptVariable.LoopRIndex, list.Count - index - 1);
                    context.SetValue(Variable, value);

                    if (!Loop(context, index, i, isLast))
                    {
                        break;
                    }

                    previousValue = value;
                    isFirst = false;
                    index += dir;
                    i++;
                }
                AfterLoop(context);

                context.SetValue(ScriptVariable.Continue, index);
            }
            else if (loopIterator != null)
            {
                throw new ScriptRuntimeException(Iterator.Span, $"Unexpected type `{loopIterator.GetType()}` for iterator");
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("for").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            if (!context.PreviousHasSpace)
            {
                context.Write(" ");
            }
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            context.Write(NamedArguments);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        protected virtual void ProcessArgument(TemplateContext context, ScriptNamedArgument argument)
        {
            throw new ScriptRuntimeException(argument.Span, $"Unsupported argument `{argument.Name}` for statement: `{this}`");
        }

#if SCRIBAN_ASYNC
        protected virtual Task ProcessArgumentAsync(TemplateContext context, ScriptNamedArgument argument)
        {
            throw new ScriptRuntimeException(argument.Span, $"Unsupported argument `{argument.Name}` for statement: `{this}`");
        }
#endif

        public override string ToString()
        {
            return $"for {Variable} in {Iterator} ... end";
        }
    }
}