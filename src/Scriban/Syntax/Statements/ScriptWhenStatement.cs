// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.Text;
using Scriban.Functions;

namespace Scriban.Syntax
{
    [ScriptSyntax("when statement", "when <expression> ... end|when|else")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptWhenStatement : ScriptConditionStatement
    {
        private ScriptKeyword _whenKeyword;
        private ScriptList<ScriptExpression> _values;
        private ScriptBlockStatement _body;
        private ScriptConditionStatement _next;

        public ScriptWhenStatement()
        {
            WhenKeyword = ScriptKeyword.When();
            Values = new ScriptList<ScriptExpression>();
        }

        public ScriptKeyword WhenKeyword
        {
            get => _whenKeyword;
            set => ParentToThis(ref _whenKeyword, value);
        }

        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public ScriptList<ScriptExpression> Values
        {
            get => _values;
            set => ParentToThis(ref _values, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public ScriptConditionStatement Next
        {
            get => _next;
            set => ParentToThis(ref _next, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var caseValue = context.PeekCase();
            foreach (var value in Values)
            {
                var whenValue = context.Evaluate(value);
                var result = ScriptBinaryExpression.Evaluate(context, Span, ScriptBinaryOperator.CompareEqual, caseValue, whenValue);
                if (result is bool && (bool) result)
                {
                    return context.Evaluate(Body);
                }

            }
            return context.Evaluate(Next);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(WhenKeyword).ExpectSpace();
            printer.WriteListWithCommas(Values);
            printer.ExpectEos();
            printer.Write(Body).ExpectEos();
            printer.Write(Next);
        }
    }
}