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

        protected override object LoopItem(TemplateContext context, LoopState state)
        {
            return context.Evaluate(Body);
        }

        protected virtual ScriptVariable GetLoopVariable(TemplateContext context)
        {
            return ScriptVariable.ForObject;
        }

        protected override object EvaluateImpl(TemplateContext context)
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
                object loopResult = null;
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

                var loopState = CreateLoopState();
                context.SetValue(GetLoopVariable(context), loopState);
                loopState.Length = list.Count;

                while (!reversed && index <= endIndex || reversed && index >= startIndex)
                {
                    if (!context.StepLoop(this))
                    {
                        return null;
                    }

                    // We update on next run on previous value (in order to handle last)
                    var value = list[index];
                    bool isLast = reversed ? index == startIndex : index == endIndex;
                    loopState.Index = index;
                    loopState.LocalIndex = i;
                    loopState.IsLast = isLast;
                    loopState.ValueChanged = isFirst || !Equals(previousValue, value);
                    context.SetValue(Variable, value);

                    loopResult = LoopItem(context, loopState);
                    if (!ContinueLoop(context))
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
                return loopResult;
            }

            if (loopIterator != null)
            {
                throw new ScriptRuntimeException(Iterator.Span, $"Unexpected type `{loopIterator.GetType()}` for iterator");
            }

            return null;
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
        protected virtual ValueTask ProcessArgumentAsync(TemplateContext context, ScriptNamedArgument argument)
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