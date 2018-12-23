// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("expression statement", "<expression>")]
    public partial class ScriptExpressionStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var result = context.Evaluate(Expression);
            // This code is necessary for wrap to work
            var codeDelegate = result as ScriptNode;
            if (codeDelegate != null)
            {
                return context.Evaluate(codeDelegate);
            }
            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Expression);
            context.ExpectEos();
        }

        public override string ToString()
        {
            return Expression?.ToString();
        }
    }
}