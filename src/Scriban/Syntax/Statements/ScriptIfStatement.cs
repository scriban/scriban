// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("if statement", "if <expression> ... end|else|else if")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptIfStatement : ScriptConditionStatement
    {
        private ScriptExpression? _condition;
        private ScriptBlockStatement? _then;
        private ScriptConditionStatement? _else;
        private ScriptKeyword _ifKeyword;
        private ScriptKeyword? _elseKeyword;

        public ScriptIfStatement()
        {
            _ifKeyword = ScriptKeyword.If();
            _ifKeyword.Parent = this;
        }

        /// <summary>
        /// Only valid for `else if`
        /// </summary>
        public ScriptKeyword? ElseKeyword
        {
            get => _elseKeyword;
            set => ParentToThisNullable(ref _elseKeyword, value);
        }

        public ScriptKeyword IfKeyword
        {
            get => _ifKeyword;
            set => ParentToThis(ref _ifKeyword, value);
        }

        /// <summary>
        /// Get or sets the condition of this if statement.
        /// </summary>
        public ScriptExpression? Condition
        {
            get => _condition;
            set => ParentToThisNullable(ref _condition, value);
        }

        public ScriptBlockStatement? Then
        {
            get => _then;
            set => ParentToThisNullable(ref _then, value);
        }

        public ScriptConditionStatement? Else
        {
            get => _else;
            set => ParentToThisNullable(ref _else, value);
        }

        public bool IsElseIf => ElseKeyword is not null;

        public override object? Evaluate(TemplateContext context)
        {
            if (Condition is null)
            {
                return null;
            }

            var conditionValue = context.ToBool(Condition.Span, context.Evaluate(Condition));
            return conditionValue
                ? Then is null ? null : context.Evaluate(Then)
                : Else is null ? null : context.Evaluate(Else);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (IsElseIf)
            {
                printer.Write(ElseKeyword).ExpectSpace();
            }
            printer.Write(IfKeyword).ExpectSpace();
            printer.Write(Condition);
            printer.ExpectEos();
            printer.Write(Then);
            printer.Write(Else);
        }
    }
}
