// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("nested expression", "(<expression>)")]
    public class ScriptNestedExpression : ScriptExpression
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.Evaluate(Expression);
        }

        protected override void WriteImpl(RenderContext context)
        {
            context.Write("(");
            Expression?.Write(context);
            context.Write(")");
        }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }
}