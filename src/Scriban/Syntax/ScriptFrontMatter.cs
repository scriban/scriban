// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptToken _startMarker = new ScriptToken(TokenType.FrontMatterMarker);
        private ScriptToken _endMarker = new ScriptToken(TokenType.FrontMatterMarker);
        private ScriptBlockStatement _statements = new ScriptBlockStatement();
        public ScriptFrontMatter()
        {
            _startMarker.Parent = this;
            _endMarker.Parent = this;
            _statements.Parent = this;
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

        public override object? Evaluate(TemplateContext context)
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
