// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptBlockStatement? _body;
        private ScriptConditionStatement? _next;

        public ScriptWhenStatement()
        {
            _whenKeyword = ScriptKeyword.When();
            _whenKeyword.Parent = this;
            _values = new ScriptList<ScriptExpression>();
            _values.Parent = this;
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

        public ScriptBlockStatement? Body
        {
            get => _body;
            set => ParentToThisNullable(ref _body, value);
        }

        public ScriptConditionStatement? Next
        {
            get => _next;
            set => ParentToThisNullable(ref _next, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            var caseValue = context.PeekCase();
            foreach (var value in Values)
            {
                var whenValue = context.Evaluate(value);
                var result = ScriptBinaryExpression.Evaluate(context, Span, ScriptBinaryOperator.CompareEqual, caseValue, whenValue);
                if (result is bool booleanResult && booleanResult)
                {
                    return Body is null ? null : context.Evaluate(Body);
                }

            }
            return Next is null ? null : context.Evaluate(Next);
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
