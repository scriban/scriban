// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.Linq;

namespace Scriban.Syntax
{
    /// <summary>
    /// this expression returns the current <see cref="TemplateContext.CurrentGlobal"/> script object.
    /// </summary>
    [ScriptSyntax("this expression", "this")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptThisExpression : ScriptExpression, IScriptVariablePath
    {
        private ScriptKeyword _thisKeyword;

        public ScriptThisExpression()
        {
            ThisKeyword = ScriptKeyword.This();
        }

        public ScriptKeyword ThisKeyword
        {
            get => _thisKeyword;
            set => ParentToThis(ref _thisKeyword, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ThisKeyword);
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
    }
}