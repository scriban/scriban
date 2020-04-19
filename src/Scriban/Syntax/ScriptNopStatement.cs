// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections.Generic;
using System.Linq;

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

        public override void Accept(ScriptVisitor visitor) => visitor.Visit(this);

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => visitor.Visit(this);
    }
}