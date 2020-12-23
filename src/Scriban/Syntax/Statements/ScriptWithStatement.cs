// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("with statement", "with <variable> ... end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptWithStatement : ScriptStatement
    {
        private ScriptKeyword _withKeyword;
        private ScriptExpression _name;
        private ScriptBlockStatement _body;

        public ScriptWithStatement()
        {
            WithKeyword = ScriptKeyword.With();
        }

        public ScriptKeyword WithKeyword
        {
            get => _withKeyword;
            set => ParentToThis(ref _withKeyword, value);
        }

        public ScriptExpression Name
        {
            get => _name;
            set => ParentToThis(ref _name, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var target = context.GetValue(Name);
            if (!(target is IScriptObject))
            {
                var targetName = target?.GetType().Name ?? "null";
                throw new ScriptRuntimeException(Name.Span, $"Invalid target property `{Name}` used for [with] statement. Must be a ScriptObject instead of `{targetName}`");
            }

            context.PushGlobal((IScriptObject)target);
            try
            {
                var result = context.Evaluate(Body);
                return result;
            }
            finally
            {
                context.PopGlobal();
            }
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(WithKeyword).ExpectSpace();
            printer.Write(Name);
            printer.ExpectEos();
            printer.Write(Body).ExpectEos();
        }
    }
}