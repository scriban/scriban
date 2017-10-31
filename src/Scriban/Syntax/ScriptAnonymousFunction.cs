// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    public class ScriptAnonymousFunction : ScriptExpression
    {
        public ScriptFunction Function { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return Function;
        }

        public override void Write(RenderContext context)
        {
            context.Write("do").WithSpace();
            context.Write(Function);
        }
    }
}