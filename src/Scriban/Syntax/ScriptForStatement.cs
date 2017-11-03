// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    public interface IScriptOptionsContainer
    {
        List<ScriptStatementOption> Options { get; set; }
    }

    public class ScriptStatementOption : ScriptExpression
    {
        public ScriptStatementOption()
        {
        }

        public ScriptStatementOption(string name)
        {
            Name = name;
        }

        public ScriptStatementOption(string name, ScriptExpression value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public ScriptExpression Value { get; set; }


        public override object Evaluate(TemplateContext context)
        {
            if (Value != null) return context.Evaluate(Value);
            return true;
        }

        public override void Write(RenderContext context)
        {
            if (Name == null)
            {
                return;
            }
            context.Write(Name);
            if (Value is ScriptExpression)
            {
                context.Write(":");
                context.Write(Value);
            }
        }
    }

    public static class ScriptOptionsContainerExtensions
    {
        public static void AddOption(this IScriptOptionsContainer options, ScriptStatementOption option)
        {
            if (options.Options == null)
            {
                options.Options = new List<ScriptStatementOption>();
            }
            options.Options.Add(option);
        }


        public static void Write(this List<ScriptStatementOption> options, RenderContext context)
        {
            if (options == null)
            {
                return;
            }
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                context.Write(",");
                context.Write(option);
            }
        }
    }

    /// <summary>
    /// A for in loop statement.
    /// </summary>
    [ScriptSyntax("for statement", "for <variable> in <expression> ... end")]
    public class ScriptForStatement : ScriptLoopStatementBase, IScriptOptionsContainer
    {
        public ScriptVariable Variable { get; set; }

        public ScriptExpression Iterator { get; set; }


        public List<ScriptStatementOption> Options { get; set; }

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
                if (Options != null)
                {
                    foreach (var option in Options)
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
                        }
                    }
                }
                var endIndex = Math.Min(limit + startIndex, list.Count) - 1;

                var index = reversed ? endIndex : startIndex;
                var dir = reversed ? -1 : 1;
                bool isFirst = true;
                while (!reversed && index <= endIndex || reversed && index >= startIndex)
                {
                    if (!context.StepLoop(this))
                    {
                        return;
                    }

                    // We update on next run on previous value (in order to handle last)
                    var value = list[index];
                    context.SetValue(ScriptVariable.LoopLast, reversed ? index == startIndex: index == endIndex );
                    context.SetValue(ScriptVariable.LoopChanged, isFirst || !Equals(previousValue, value));
                    context.SetValue(ScriptVariable.LoopRIndex, list.Count - index - 1);
                    context.SetValue(Variable, value);

                    if (!Loop(context, index))
                    {
                        break;
                    }

                    previousValue = value;
                    isFirst = false;
                    index += dir;
                }

                context.SetValue(ScriptVariable.Continue, index);
            }
            else if (loopIterator != null)
            {
                throw new ScriptRuntimeException(Iterator.Span, $"Unexpected type [{loopIterator.GetType()}] for iterator");
            }
        }

        public override void Write(RenderContext context)
        {
            context.Write("for").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            Options?.Write(context);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        public override string ToString()
        {
            return $"for {Variable} in {Iterator} ... end";
        }
    }
}