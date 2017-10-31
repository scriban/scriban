// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    [ScriptSyntax("when statement", "when <expression> ... end|when|else")]
    public class ScriptWhenStatement : ScriptConditionStatement
    {
        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public ScriptExpression Value { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public ScriptConditionStatement Next { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var whenValue = context.Evaluate(Value);
            var caseValue = context.PeekCase();
            var result = (bool) ScriptBinaryExpression.Calculate(context, this.Span, ScriptBinaryOperator.CompareEqual, caseValue, caseValue?.GetType(), whenValue, whenValue?.GetType());
            return result ? context.Evaluate(Body) : context.Evaluate(Next);
        }

        public override void Write(RenderContext context)
        {
            context.Write("when").WithSpace();
            context.Write(Value).WithEos();
            context.Write(Body);
            context.Write(Next);
        }

        public override string ToString()
        {
            return $"when {Value}";
        }
    }
}