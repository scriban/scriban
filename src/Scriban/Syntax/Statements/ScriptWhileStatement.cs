// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("while statement", "while <expression> ... end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptWhileStatement : ScriptLoopStatementBase
    {
        private ScriptKeyword _whileKeyword;
        private ScriptExpression? _condition;
        private ScriptBlockStatement? _body;

        public ScriptWhileStatement()
        {
            _whileKeyword = ScriptKeyword.While();
            _whileKeyword.Parent = this;
        }

        public ScriptKeyword WhileKeyword
        {
            get => _whileKeyword;
            set => ParentToThis(ref _whileKeyword, value);
        }

        public ScriptExpression? Condition
        {
            get => _condition;
            set => ParentToThisNullable(ref _condition, value);
        }

        public ScriptBlockStatement? Body
        {
            get => _body;
            set => ParentToThisNullable(ref _body, value);
        }

        protected override object? LoopItem(TemplateContext context, LoopState state)
        {
            return Body is null ? null : context.Evaluate(Body);
        }

        protected override object? EvaluateImpl(TemplateContext context)
        {
            var index = 0;
            object? result = null;
            BeforeLoop(context);

            var loopState = CreateLoopState();
            context.SetLoopVariable(ScriptVariable.WhileObject, loopState);

            while (context.StepLoop(this))
            {
                if (Condition is null)
                {
                    break;
                }

                var conditionResult = context.ToBool(Condition.Span, context.Evaluate(Condition));
                if (!conditionResult)
                {
                    break;
                }

                loopState.Index = index++;
                result = LoopItem(context, loopState);

                if (!ContinueLoop(context))
                {
                    break;
                }
            };
            AfterLoop(context);
            return result;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(WhileKeyword).ExpectSpace();
            printer.Write(Condition);
            printer.ExpectEos();
            printer.Write(Body).ExpectEos();
        }
    }
}
