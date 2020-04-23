// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("nested expression", "(<expression>)")]
    public partial class ScriptNestedExpression : ScriptExpression, IScriptVariablePath
    {
        private ScriptExpression _expression;

        public ScriptNestedExpression()
        {
        }

        public ScriptNestedExpression(ScriptExpression expression)
        {
            Expression = expression;
        }

        public ScriptExpression Expression
        {
            get => _expression;
            set => ParentToThis(ref _expression, value);
        }

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

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write("(");
            printer.Write(Expression);
            printer.Write(")");
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