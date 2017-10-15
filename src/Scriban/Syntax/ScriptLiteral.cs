// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("literal", "<value>")]
    public class ScriptLiteral : ScriptExpression
    {
        public object Value { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return Value;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}