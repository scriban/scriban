// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
namespace Scriban.Runtime
{
    public class ScriptAnonymousFunction : ScriptExpression
    {
        public ScriptFunction Function { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            context.Result = Function;
        }
    }
}