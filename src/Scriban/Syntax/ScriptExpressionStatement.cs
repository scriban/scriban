// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("expression statement", "<expression>")]
    public class ScriptExpressionStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var result = context.Evaluate(Expression);
            var codeDelegate = result as ScriptNode;
            if (codeDelegate != null)
            {
                return context.Evaluate(codeDelegate);
            }
            return result;
        }

        public override string ToString()
        {
            return Expression?.ToString();
        }
    }
}