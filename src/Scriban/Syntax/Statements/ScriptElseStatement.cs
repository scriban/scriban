// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("else statement", "else | else if <expression> ... end|else|else if")]
    public partial class ScriptElseStatement : ScriptConditionStatement
    {
        private ScriptKeyword _elseKeyword;
        private ScriptBlockStatement _body;
        private ScriptConditionStatement _else;

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

        public ScriptConditionStatement Else
        {
            get => _else;
            set => ParentToThis(ref _else, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            context.Evaluate(Body);
            return context.Evaluate(Else);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ElseKeyword).ExpectEos();
            printer.Write(Body);
            printer.Write(Else);
        }
    }
}