// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
        private ScriptExpression _condition;
        private ScriptBlockStatement _then;
        private ScriptConditionStatement _else;
        private ScriptKeyword _ifKeyword;
        private ScriptKeyword _elseKeyword;

        public ScriptIfStatement()
        {
            IfKeyword = ScriptKeyword.If();
        }

        /// <summary>
        /// Only valid for `else if`
        /// </summary>
        public ScriptKeyword ElseKeyword
        {
            get => _elseKeyword;
            set => ParentToThis(ref _elseKeyword, value);
        }

        public ScriptKeyword IfKeyword
        {
            get => _ifKeyword;
            set => ParentToThis(ref _ifKeyword, value);
        }

        /// <summary>
        /// Get or sets the condition of this if statement.
        /// </summary>
        public ScriptExpression Condition
        {
            get => _condition;
            set => ParentToThis(ref _condition, value);
        }

        public ScriptBlockStatement Then
        {
            get => _then;
            set => ParentToThis(ref _then, value);
        }

        public ScriptConditionStatement Else
        {
            get => _else;
            set => ParentToThis(ref _else, value);
        }

        public bool IsElseIf => ElseKeyword != null;

        public override object Evaluate(TemplateContext context)
        {
            var conditionValue = context.ToBool(Condition.Span, context.Evaluate(Condition));
            return conditionValue ? context.Evaluate(Then) : context.Evaluate(Else);
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