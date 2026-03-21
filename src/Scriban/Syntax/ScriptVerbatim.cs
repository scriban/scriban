// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract class ScriptVerbatim : ScriptNode, IScriptTerminal
    {
        protected ScriptVerbatim()
        {
            Trivias = new ScriptTrivias();
            Value = string.Empty;
        }

        protected ScriptVerbatim(string value)
        {
            Trivias = new ScriptTrivias();
            Value = value;
        }


        public ScriptTrivias Trivias { get; set; }

        public string Value { get; set; }

        public override object? Evaluate(TemplateContext context)
        {
            // Nothing to evaluate
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Value);
        }

    }
}
