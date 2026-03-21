// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptExpression? _variable;
        private ScriptKeyword _inKeyword;
        private ScriptExpression? _iterator;
        private ScriptList<ScriptNamedArgument> _namedArguments = new ScriptList<ScriptNamedArgument>();
        private ScriptBlockStatement? _body;
        private ScriptElseStatement? _else;
        public ScriptForStatement()
        {
            _forOrTableRowKeyword = this is ScriptTableRowStatement ? ScriptKeyword.TableRow() : ScriptKeyword.For();
            _forOrTableRowKeyword.Parent = this;
            _inKeyword = ScriptKeyword.In();
            _inKeyword.Parent = this;
            _namedArguments.Parent = this;
        }

        public ScriptKeyword ForOrTableRowKeyword
        {
            get => _forOrTableRowKeyword;
            set => ParentToThis(ref _forOrTableRowKeyword, value);
        }

        public ScriptExpression? Variable
        {
            get => _variable;
            set => ParentToThisNullable(ref _variable, value);
        }

        public ScriptKeyword InKeyword
        {
            get => _inKeyword;
            set => ParentToThis(ref _inKeyword, value);
        }

        public ScriptExpression? Iterator
        {
            get => _iterator;
            set => ParentToThisNullable(ref _iterator, value);
        }

        public ScriptList<ScriptNamedArgument> NamedArguments
        {
            get => _namedArguments;
            set => ParentToThis(ref _namedArguments, value);
        }

        public ScriptBlockStatement? Body
        {
            get => _body;
            set => ParentToThisNullable(ref _body, value);
        }

        public ScriptElseStatement? Else
        {
            get => _else;
            set => ParentToThisNullable(ref _else, value);
        }

        /// <summary>
        /// <c>true</c> to set the global variable `continue` after the loop (used by liquid)
        /// </summary>
        public bool SetContinue { get; set; }

        internal ScriptNode? IteratorOrLastParameter => NamedArguments is not null && NamedArguments.Count > 0
            ? NamedArguments[NamedArguments.Count - 1]
            : Iterator;

        protected override object? LoopItem(TemplateContext context, LoopState state)
        {
            if (Body is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid for statement. Body is required.");
            }
            return context.Evaluate(Body);
        }

        protected virtual ScriptVariable GetLoopVariable(TemplateContext context)
        {
            return ScriptVariable.ForObject;
        }

        protected override object? EvaluateImpl(TemplateContext context)
        {
            var iterator = Iterator;
            var variable = Variable;
            if (iterator is null || variable is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid for statement. Variable and iterator are required.");
            }

            var loopIterator = context.Evaluate(iterator);
            var list = loopIterator as IEnumerable;

            if (list is not null)
            {
                object? loopResult = null;
                object? previousValue = null;
                var loopType = loopIterator is System.Linq.IQueryable ? TemplateContext.LoopType.Queryable : TemplateContext.LoopType.Default;

                //int startIndex = 0;
                int limit = -1;
                int continueIndex = 0;
                if (NamedArguments is not null)
                {
                    bool reversed = false;
                    int offset = 0;

                    var listTyped = System.Linq.Enumerable.Cast<object>(list);
                    foreach (var option in NamedArguments)
                    {
                        var optionName = option.Name?.Name;
                        switch (optionName)
                        {
                            case "offset":

                            if (option.Value is null) throw new ScriptRuntimeException(option.Span, "Invalid `offset` argument. Value is required.");
                            offset = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                            continueIndex = offset;
                                break;
                            case "reversed":
                                reversed = true;
                                break;
                            case "limit":
                                if (option.Value is null) throw new ScriptRuntimeException(option.Span, "Invalid `limit` argument. Value is required.");
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

                        if (variable is ScriptVariable loopVariable)
                        {
                            context.SetLoopVariable(loopVariable, value);
                        }
                        else
                        {
                            context.SetValue(variable, value);
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

                if (!enteredLoop && Else is not null)
                {
                    loopResult = context.Evaluate(Else);
                }

                return loopResult;
            }

            if (loopIterator is not null)
            {
                    throw new ScriptRuntimeException(iterator.Span, $"Unexpected type `{loopIterator.GetType()}` for iterator");
                }

            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ForOrTableRowKeyword).ExpectSpace();
            if (Variable is not null)
            {
                printer.Write(Variable).ExpectSpace();
            }
            if (!printer.PreviousHasSpace)
            {
                printer.Write(" ");
            }
            printer.Write(InKeyword).ExpectSpace();
            if (Iterator is not null)
            {
                printer.Write(Iterator);
            }
            if (NamedArguments is not null)
            {
                foreach (var arg in NamedArguments)
                {
                    printer.ExpectSpace();
                    printer.Write(arg);
                }
            }
            printer.ExpectEos();
            if (Body is not null)
            {
                printer.Write(Body).ExpectEos();
            }
            if (Else is not null)
            {
                printer.Write(Else);
            }
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
