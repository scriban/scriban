// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    [ScriptSyntax("else statement", "else | else if <expression> ... end|else|else if")]
    public partial class ScriptElseStatement : ScriptConditionStatement
    {
        public ScriptBlockStatement Body { get; set; }

        public ScriptConditionStatement Else { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.Evaluate(Body);
            return context.Evaluate(Else);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("else").ExpectEos();
            context.Write(Body);
            context.Write(Else);
        }
    }
}