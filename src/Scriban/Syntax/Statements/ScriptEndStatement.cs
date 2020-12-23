// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

namespace Scriban.Syntax
{
    [ScriptSyntax("end statement", "end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptEndStatement : ScriptStatement
    {
        private ScriptKeyword _endKeyword;

        public ScriptEndStatement()
        {
            EndKeyword = ScriptKeyword.End();
            CanSkipEvaluation = true;
            ExpectEos = true;
        }

        public ScriptKeyword EndKeyword
        {
            get => _endKeyword;
            set => ParentToThis(ref _endKeyword, value);
        }

        public bool ExpectEos { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(EndKeyword);
            if (ExpectEos)
            {
                printer.ExpectEos();
            }
        }
    }
}