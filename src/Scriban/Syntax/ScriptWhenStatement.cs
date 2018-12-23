// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Text;
using Scriban.Functions;

namespace Scriban.Syntax
{
    [ScriptSyntax("when statement", "when <expression> ... end|when|else")]
    public partial class ScriptWhenStatement : ScriptConditionStatement
    {
        public ScriptWhenStatement()
        {
            Values = new List<ScriptExpression>();
        }

        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public List<ScriptExpression> Values { get; }

        public ScriptBlockStatement Body { get; set; }

        public ScriptConditionStatement Next { get; set; }

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

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("when").ExpectSpace();
            context.WriteListWithCommas(Values);
            context.ExpectEos();
            context.Write(Body);
            context.Write(Next);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("when ");
            for (var i = 0; i < Values.Count; i++)
            {
                var value = Values[i];
                if (i > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(value);
            }

            return builder.ToString();
        }
    }
}