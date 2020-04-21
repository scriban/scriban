// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("case statement", "case <expression> ... end|when|else")]
    public partial class ScriptCaseStatement : ScriptConditionStatement
    {
        private ScriptExpression _value;
        private ScriptBlockStatement _body;

        /// <summary>
        /// Get or sets the value used to check against When clause.
        /// </summary>
        public ScriptExpression Value
        {
            get => _value;
            set => ParentToThis(ref _value, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

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
    }
}