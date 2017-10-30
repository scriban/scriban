// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("return statement", "return <expression>?")]
    public class ScriptReturnStatement : ScriptExpressionStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            context.FlowState = ScriptFlowState.Return;
            return base.Evaluate(context);
        }

        protected override void WriteImpl(RenderContext context)
        {
            context.Write("ret").WithSpace();
            base.WriteImpl(context);
        }
    }
}