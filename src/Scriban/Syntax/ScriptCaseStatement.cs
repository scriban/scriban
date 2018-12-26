// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    [ScriptSyntax("case statement", "case <expression> ... end|when|else")]
    public partial class ScriptCaseStatement : ScriptConditionStatement
    {
        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public ScriptExpression Value { get; set; }

        public ScriptBlockStatement Body { get; set; }
        
        public override object Evaluate(TemplateContext context)
        {
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

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("case").ExpectSpace();
            context.Write(Value).ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        public override string ToString()
        {
            return $"case {Value}";
        }
    }
}