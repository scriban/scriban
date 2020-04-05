// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("function statement", "func <variable> ... end")]
    public class ScriptFunction : ScriptStatement
    {
        public ScriptVariable Name { get; set; }

        public ScriptStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Name != null)
            {
                context.SetValue(Name, this);
            }
            return null;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return Name != null;
        }

        public override string ToString()
        {
            return $"func {Name} ... end";
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Name != null)
            {
                context.Write("func").ExpectSpace();
                context.Write(Name);
            }
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        public override void Accept(ScriptVisitor visitor) => visitor.Visit(this);

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => visitor.Visit(this);

        protected override IEnumerable<ScriptNode> GetChildren()
        {
            yield return Name;
            yield return Body;
        }
    }
}