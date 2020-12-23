// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using Scriban.Parsing;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptFrontMatter : ScriptStatement
    {
        private ScriptToken _startMarker;
        private ScriptToken _endMarker;
        private ScriptBlockStatement _statements;

        public ScriptFrontMatter()
        {
            StartMarker = new ScriptToken(TokenType.FrontMatterMarker);
            EndMarker = new ScriptToken(TokenType.FrontMatterMarker);
        }


        public ScriptToken StartMarker
        {
            get => _startMarker;
            set => ParentToThis(ref _startMarker, value);
        }

        public ScriptBlockStatement Statements
        {
            get => _statements;
            set => ParentToThis(ref _statements, value);
        }

        public ScriptToken EndMarker
        {
            get => _endMarker;
            set => ParentToThis(ref _endMarker, value);
        }

        public TextPosition TextPositionAfterEndMarker;

        public override object Evaluate(TemplateContext context)
        {
            return context.Evaluate(Statements);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.ExpectEos();
            printer.Write(StartMarker);
            printer.ExpectEos();
            printer.Write(Statements);
            printer.ExpectEos();
            printer.Write(EndMarker);
            printer.ExpectEos();
        }
    }
}