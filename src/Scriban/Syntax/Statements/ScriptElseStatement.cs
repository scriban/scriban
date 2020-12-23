// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("else statement", "else | else if <expression> ... end|else|else if")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptElseStatement : ScriptConditionStatement
    {
        private ScriptKeyword _elseKeyword;
        private ScriptBlockStatement _body;

        public ScriptElseStatement()
        {
            ElseKeyword = ScriptKeyword.Else();
        }

        public ScriptKeyword ElseKeyword
        {
            get => _elseKeyword;
            set => ParentToThis(ref  _elseKeyword, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.Evaluate(Body);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ElseKeyword).ExpectEos();
            printer.Write(Body).ExpectEos();
        }
    }
}