// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("case statement", "case <expression> ... end|when|else")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptCaseStatement : ScriptConditionStatement
    {
        private ScriptKeyword _caseKeyword;
        private ScriptExpression? _value;
        private ScriptBlockStatement? _body;

        public ScriptCaseStatement()
        {
            _caseKeyword = ScriptKeyword.Case();
            _caseKeyword.Parent = this;
        }

        public ScriptKeyword CaseKeyword
        {
            get => _caseKeyword;
            set => ParentToThis(ref _caseKeyword, value);
        }

        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public ScriptExpression? Value
        {
            get => _value;
            set => ParentToThisNullable(ref _value, value);
        }

        public ScriptBlockStatement? Body
        {
            get => _body;
            set => ParentToThisNullable(ref _body, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Value is null || Body is null)
            {
                return null;
            }

            var caseValue = context.Evaluate(Value);
            context.PushCase(caseValue);
            try
            {
                return context.Evaluate(Body);
            }
            finally
            {
                context.PopCase();
            }
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(CaseKeyword).ExpectSpace();
            printer.Write(Value).ExpectEos();
            printer.Write(Body).ExpectEos();
        }
    }
}
