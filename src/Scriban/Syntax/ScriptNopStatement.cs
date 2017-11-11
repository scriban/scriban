// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    /// <summary>
    /// Empty instruction for an empty code block
    /// </summary>
    public class ScriptNopStatement : ScriptStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
        }
    }
}