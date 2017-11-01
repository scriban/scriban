// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("nested expression", "(<expression>)")]
    public class ScriptNestedExpression : ScriptExpression
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            // A nested expression will reset the pipe arguments for the group
            context.PushPipeArguments();
            try
            {
                return context.Evaluate(Expression);
            }
            finally
            {
                context.PopPipeArguments();
            }
        }

        public override void Write(RenderContext context)
        {
            context.Write("(");
            context.Write(Expression);
            context.Write(")");
        }

        public override string ToString()
        {
            return $"({Expression})";
        }
    }
}