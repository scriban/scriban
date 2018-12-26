// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("nested expression", "(<expression>)")]
    public partial class ScriptNestedExpression : ScriptExpression, IScriptVariablePath
    {
        public ScriptExpression Expression { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            // A nested expression will reset the pipe arguments for the group
            context.PushPipeArguments();
            try
            {
                return context.GetValue(this);
            }
            finally
            {
                context.PopPipeArguments();
            }
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("(");
            context.Write(Expression);
            context.Write(")");
        }

        public override string ToString()
        {
            return $"({Expression})";
        }

        public object GetValue(TemplateContext context)
        {
            return context.Evaluate(Expression);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            context.SetValue(Expression, valueToSet);
        }

        public string GetFirstPath()
        {
            return (Expression as IScriptVariablePath)?.GetFirstPath();
        }
    }
}