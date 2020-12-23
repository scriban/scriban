// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("readonly statement", "readonly <variable>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptReadOnlyStatement : ScriptStatement
    {
        private ScriptVariable _variable;
        private ScriptKeyword _readOnlyKeyword;

        public ScriptReadOnlyStatement()
        {
            ReadOnlyKeyword = ScriptKeyword.ReadOnly();
        }

        public ScriptKeyword ReadOnlyKeyword
        {
            get => _readOnlyKeyword;
            set => ParentToThis(ref _readOnlyKeyword, value);
        }

        public ScriptVariable Variable
        {
            get => _variable;
            set => ParentToThis(ref _variable, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            context.SetReadOnly(Variable);
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ReadOnlyKeyword).ExpectSpace();
            printer.Write(Variable);
            printer.ExpectEos();
        }
    }
}