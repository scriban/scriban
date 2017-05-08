// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("expression statement", "<expression>")]
    public class ScriptExpressionStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            Expression?.Evaluate(context);

            var codeDelegate = context.Result as ScriptNode;
            if (codeDelegate != null)
            {
                context.Result = null;
                codeDelegate.Evaluate(context);
            }
        }

        public override string ToString()
        {
            return Expression?.ToString();
        }
    }
}