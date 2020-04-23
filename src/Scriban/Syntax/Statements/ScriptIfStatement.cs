// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("if statement", "if <expression> ... end|else|else if")]
    public partial class ScriptIfStatement : ScriptConditionStatement
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

        public override void Write(TemplateRewriterContext context)
        {
            if (IsElseIf)
            {
                context.Write(ElseKeyword).ExpectSpace();
            }
            context.Write(IfKeyword).ExpectSpace();
            context.Write(Condition);
            context.ExpectEos();
            context.Write(Then);
            context.Write(Else);
        }
    }
}