// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;

namespace Scriban.Syntax
{
    [ScriptSyntax("{{ or }}", "{{ or }}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptEscapeStatement : ScriptStatement, IScriptTerminal
    {
        public ScriptEscapeStatement()
        {
            CanSkipEvaluation = true;
        }

        public ScriptTrivias Trivias { get; set; }

        public ScriptWhitespaceMode WhitespaceMode { get; set; }

        public string Indent { get; set; }

        public bool IsEntering { get; set; }


        public bool IsClosing => !IsEntering;

        public int EscapeCount { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (IsEntering)
            {
                printer.WriteEnterCode(EscapeCount);
                WriteWhitespaceMode(printer);
            }
            else
            {
                WriteWhitespaceMode(printer);
                printer.WriteExitCode(EscapeCount);
            }
        }

        private void WriteWhitespaceMode(ScriptPrinter printer)
        {
            switch (WhitespaceMode)
            {
                case ScriptWhitespaceMode.None:
                    break;
                case ScriptWhitespaceMode.Greedy:
                    printer.Write("-");
                    break;
                case ScriptWhitespaceMode.NonGreedy:
                    printer.Write("~");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}