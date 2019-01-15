// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("assign expression", "<target_expression> = <value_expression>")]
    public partial class ScriptAssignExpression : ScriptExpression
    {
        public ScriptExpression Target { get; set; }

        public ScriptExpression Value { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var valueObject = context.Evaluate(Value);
            context.SetValue(Target, valueObject);
            return null;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            context.Write("=");
            context.Write(Value);
        }

        public override string ToString()
        {
            return $"{Target} = {Value}";
        }
    }
}