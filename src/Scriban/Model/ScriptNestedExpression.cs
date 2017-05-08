// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("nested expression", "(<expression>)")]
    public class ScriptNestedExpression : ScriptExpression
    {
        public ScriptExpression Expression { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            Expression?.Evaluate(context);
        }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }
}