// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("return statement", "return <expression>?")]
    public class ScriptReturnStatement : ScriptStatement
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.FlowState = ScriptFlowState.Return;
            return context.Evaluate(Expression);
        }

        public override void Write(RenderContext context)
        {
            context.Write("ret").WithSpace();
            context.Write(Expression);
            context.WithEos();
        }
    }
}