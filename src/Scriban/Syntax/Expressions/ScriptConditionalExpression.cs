// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("conditional expression", "<condition> ? <then_value> : <else_value>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptConditionalExpression : ScriptExpression
    {
        private ScriptExpression? _condition;
        private ScriptToken _questionToken;
        private ScriptExpression? _thenValue;
        private ScriptToken _colonToken;
        private ScriptExpression? _elseValue;

        public ScriptConditionalExpression()
        {
            _questionToken = ScriptToken.Question();
            _questionToken.Parent = this;
            _colonToken = ScriptToken.Colon();
            _colonToken.Parent = this;
        }

        public ScriptExpression? Condition
        {
            get => _condition;
            set => ParentToThisNullable(ref _condition, value);
        }

        public ScriptToken QuestionToken
        {
            get => _questionToken;
            set => ParentToThis(ref _questionToken, value);
        }

        public ScriptExpression? ThenValue
        {
            get => _thenValue;
            set => ParentToThisNullable(ref _thenValue, value);
        }

        public ScriptToken ColonToken
        {
            get => _colonToken;
            set => ParentToThis(ref _colonToken, value);
        }

        public ScriptExpression? ElseValue
        {
            get => _elseValue;
            set => ParentToThisNullable(ref _elseValue, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Condition is null || ThenValue is null || ElseValue is null)
            {
                return null;
            }

            var condValue = context.Evaluate(Condition);
            var result = context.ToBool(Condition.Span, condValue);
            return context.Evaluate(result ? ThenValue : ElseValue);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Condition);
            printer.Write(QuestionToken);
            printer.Write(ThenValue);
            printer.Write(ColonToken);
            printer.Write(ElseValue);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }
    }
}
