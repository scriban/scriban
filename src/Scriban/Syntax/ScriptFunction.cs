// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

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

        public override string ToString()
        {
            return $"func {Name} ... end";
        }

        public override void Write(RenderContext context)
        {
            if (Name != null)
            {
                context.Write("func").WithSpace();
                context.Write(Name);
            }
            context.WithEos();
            context.Write(Body);
            context.WithEnd();
        }
    }
}