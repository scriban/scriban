// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections;
using System.Threading.Tasks;
using System;

namespace Scriban.Syntax
{
    /// <summary>
    /// A for in loop statement.
    /// </summary>
    [ScriptSyntax("for statement", "for <variable> in <expression> ... end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptForStatement : ScriptLoopStatementBase, IScriptNamedArgumentContainer
    {
        private ScriptKeyword _forOrTableRowKeyword;
        private ScriptExpression _variable;
        private ScriptKeyword _inKeyword;
        private ScriptExpression _iterator;
        private ScriptList<ScriptNamedArgument> _namedArguments;
        private ScriptBlockStatement _body;
        private ScriptElseStatement _else;

        public ScriptForStatement()
        {
            ForOrTableRowKeyword = this is ScriptTableRowStatement ?  ScriptKeyword.TableRow() : ScriptKeyword.For();
            InKeyword = ScriptKeyword.In();
        }

        public ScriptKeyword ForOrTableRowKeyword
        {
            get => _forOrTableRowKeyword;
            set => ParentToThis(ref _forOrTableRowKeyword, value);
        }

        public ScriptExpression Variable
        {
            get => _variable;
            set => ParentToThis(ref _variable, value);
        }

        public ScriptKeyword InKeyword
        {
            get => _inKeyword;
            set => ParentToThis(ref _inKeyword, value);
        }

        public ScriptExpression Iterator
        {
            get => _iterator;
            set => ParentToThis(ref _iterator, value);
        }

        public ScriptList<ScriptNamedArgument> NamedArguments
        {
            get => _namedArguments;
            set => ParentToThis(ref _namedArguments, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public ScriptElseStatement Else
        {
            get => _else;
            set => ParentToThis(ref _else, value);
        }

        /// <summary>
        /// <c>true</c> to set the global variable `continue` after the loop (used by liquid)
        /// </summary>
        public bool SetContinue { get; set; }

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
            var list = loopIterator as IEnumerable;

            if (list != null)
            {
                object loopResult = null;
                object previousValue = null;
                var loopType = loopIterator is System.Linq.IQueryable ? TemplateContext.LoopType.Queryable : TemplateContext.LoopType.Default;

                //int startIndex = 0;
                int limit = -1;
                int continueIndex = 0;
                if (NamedArguments != null)
                {
                    bool reversed = false;
                    int offset = 0;

                    var listTyped = System.Linq.Enumerable.Cast<object>(list);
                    foreach (var option in NamedArguments)
                    {
                        switch (option.Name.Name)
                        {
                            case "offset":

                                offset = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                                continueIndex = offset;
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

                    if (offset > 0)
                    {
                        listTyped = System.Linq.Enumerable.Skip(listTyped, offset);
                    }

                    if (reversed)
                    {
                        listTyped = System.Linq.Enumerable.Reverse(listTyped);
                    }

                    if (limit > 0)
                    {
                        listTyped = System.Linq.Enumerable.Take(listTyped, limit);
                    }

                    list = listTyped;
                }

                bool isFirst = true;
                int index = 0;
                BeforeLoop(context);

                var loopState = CreateLoopState();
                context.SetLoopVariable(GetLoopVariable(context), loopState);
                var it = list.GetEnumerator();
                loopState.SetEnumerable(list, it);

                bool enteredLoop = false;
                if (it.MoveNext())
                {
                    enteredLoop = true;
                    while (true)
                    {
                        if (!context.StepLoop(this, loopType))
                        {
                            return null;
                        }

                        loopState.ResetLast();

                        // We update on next run on previous value (in order to handle last)
                        var value = it.Current;
                        loopState.Index = index;
                        loopState.ValueChanged = isFirst || !Equals(previousValue, value);

                        if (Variable is ScriptVariable loopVariable)
                        {
                            context.SetLoopVariable(loopVariable, value);
                        }
                        else
                        {
                            context.SetValue(Variable, value);
                        }

                        loopResult = LoopItem(context, loopState);

                        var isLast = loopState.MoveNextAndIsLast();
                        if (!ContinueLoop(context) || isLast)
                        {
                            break;
                        }

                        previousValue = value;
                        isFirst = false;
                        index++;
                        continueIndex++;
                    }
                }
                AfterLoop(context);

                if (SetContinue)
                {
                    context.SetValue(ScriptVariable.Continue, continueIndex + 1);
                }

                if (!enteredLoop && Else != null)
                {
                    loopResult = context.Evaluate(Else);
                }

                return loopResult;
            }

            if (loopIterator != null)
            {
                throw new ScriptRuntimeException(Iterator.Span, $"Unexpected type `{loopIterator.GetType()}` for iterator");
            }

            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ForOrTableRowKeyword).ExpectSpace();
            printer.Write(Variable).ExpectSpace();
            if (!printer.PreviousHasSpace)
            {
                printer.Write(" ");
            }
            printer.Write(InKeyword).ExpectSpace();
            printer.Write(Iterator);
            if (NamedArguments != null)
            {
                foreach (var arg in NamedArguments)
                {
                    printer.ExpectSpace();
                    printer.Write(arg);
                }
            }
            printer.ExpectEos();
            printer.Write(Body).ExpectEos();
            printer.Write(Else);
        }

        protected virtual void ProcessArgument(TemplateContext context, ScriptNamedArgument argument)
        {
            throw new ScriptRuntimeException(argument.Span, $"Unsupported argument `{argument.Name}` for statement: `{this}`");
        }

#if !SCRIBAN_NO_ASYNC
        protected virtual ValueTask ProcessArgumentAsync(TemplateContext context, ScriptNamedArgument argument)
        {
            throw new ScriptRuntimeException(argument.Span, $"Unsupported argument `{argument.Name}` for statement: `{this}`");
        }
#endif
    }
}