// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scriban.Runtime;

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

            if (loopIterator is System.Linq.IQueryable queryable)
            {
                // Value is a queryable, use Linq and deferred execution
                return LoopQueryable(context, queryable); 
            }

            var list = loopIterator as IList;
            if (list == null)
            {
                if (loopIterator is IEnumerable iterator)
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
                        switch (option.Name.Name)
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
                context.SetLoopVariable(GetLoopVariable(context), loopState);
                loopState.Length = list.Count;

                bool enteredLoop = false;
                while (!reversed && index <= endIndex || reversed && index >= startIndex)
                {
                    enteredLoop = true;
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

                    if (Variable is ScriptVariable loopVariable)
                    {
                        context.SetLoopVariable(loopVariable, value);
                    }
                    else
                    {
                        context.SetValue(Variable, value);
                    }

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

                if (SetContinue)
                {
                    context.SetValue(ScriptVariable.Continue, index);
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



        /// <summary>
        /// Loop over an IQueryable value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        private object LoopQueryable(TemplateContext context, System.Linq.IQueryable queryable)
        {
            var loopResult = default(object);

            queryable = HandleQueryableArguments(context, queryable);

            BeforeLoop(context);

            var loopState = CreateLoopState();
            context.SetValue(GetLoopVariable(context), loopState);

            var previousValue = default(object);
            var index = 0;
            var enteredLoop = false;

            foreach (var value in queryable)
            {
                if (!context.StepLoop(this, TemplateContext.LoopType.Queryable))
                    return default;

                loopState.Index = index;
                loopState.LocalIndex = index;
                loopState.ValueChanged = index == 0 || !Equals(previousValue, value);

                if (Variable is ScriptVariable loopVariable)
                    context.SetLoopVariable(loopVariable, value);
                else
                    context.SetValue(Variable, value);

                loopResult = LoopItem(context, default);

                if (!ContinueLoop(context))
                    break;

                previousValue = value;
                index++;
            }

            AfterLoop(context);

            if (SetContinue)
                context.SetValue(ScriptVariable.Continue, index);

            if (!enteredLoop && Else != default)
                loopResult = context.Evaluate(Else);

            return loopResult;
        }

        /// <summary>
        /// Use Linq IQueryable extension methods
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        private System.Linq.IQueryable HandleQueryableArguments(TemplateContext context, System.Linq.IQueryable queryable)
        {
            if (NamedArguments == null || NamedArguments.Count == 0)
                return queryable;

            var typeOfT = queryable.GetType().GetGenericArguments()[0];
            System.Linq.IQueryable InvokeQueryableMethod(string methodName, params object[] parameters)
            {
                var methodInfo = typeof(System.Linq.Queryable).GetMethod(methodName);
                methodInfo = methodInfo.MakeGenericMethod(typeOfT);

                return (System.Linq.IQueryable)methodInfo.Invoke(null, parameters);
            }

            foreach (var option in NamedArguments)
            {
                switch (option.Name.Name)
                {
                    case "offset": // call IQueryable<T>.Skip(count) extension method
                        {
                            var startIndex = context.ToInt(option.Value.Span, context.Evaluate(option.Value));

                            queryable = InvokeQueryableMethod(nameof(System.Linq.Queryable.Skip), queryable, startIndex);
                            break;
                        }
                    case "reversed": // call IQueryable<T>.Reverse() extension method
                        {
                            queryable = InvokeQueryableMethod(nameof(System.Linq.Queryable.Reverse), queryable);
                            break;
                        }
                    case "limit": // call IQueryable<T>.Take(count) extension method
                        {
                            var limit = context.ToInt(option.Value.Span, context.Evaluate(option.Value));

#if NET6_0_OR_GREATER
                            var methodInfo = Array.Find(
                                typeof(System.Linq.Queryable).GetMethods(),
                                x => x.Name == nameof(System.Linq.Queryable.Take) && x.GetParameters()[1].ParameterType == typeof(int));

                            methodInfo = methodInfo.MakeGenericMethod(typeOfT);

                            queryable = (System.Linq.IQueryable)methodInfo.Invoke(null, new object[] { queryable, limit });
#else
                            queryable = InvokeQueryableMethod(nameof(System.Linq.Queryable.Take), queryable, limit);
#endif
                            break;
                        }
                    default:
                        {
                            ProcessArgument(context, option);
                            break;
                        }
                }
            }

            return queryable;
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