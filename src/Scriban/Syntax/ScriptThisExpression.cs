// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    /// <summary>
    /// this expression returns the current <see cref="TemplateContext.CurrentGlobal"/> script object.
    /// </summary>
    [ScriptSyntax("this expression", "this")]
    public partial class ScriptThisExpression : ScriptExpression, IScriptVariablePath
    {
        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("this");
        }

        public object GetValue(TemplateContext context)
        {
            return context.CurrentGlobal;
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            throw new ScriptRuntimeException(Span, "Cannot set this variable");
        }

        public string GetFirstPath()
        {
            return "this";
        }

        public override string ToString()
        {
            return $"this";
        }
    }
}