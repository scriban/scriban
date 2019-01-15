// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("return statement", "return <expression>?")]
    public partial class ScriptReturnStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var result = context.Evaluate(Expression);
            context.FlowState = ScriptFlowState.Return;
            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("ret").ExpectSpace();
            context.Write(Expression);
            context.ExpectEos();
        }
    }
}