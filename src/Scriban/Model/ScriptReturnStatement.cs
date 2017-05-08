// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("return statement", "return <expression>?")]
    public class ScriptReturnStatement : ScriptExpressionStatement
    {
        public override void Evaluate(TemplateContext context)
        {
            context.FlowState = ScriptFlowState.Return;
            base.Evaluate(context);
        }
    }
}