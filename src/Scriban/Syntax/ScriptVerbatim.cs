// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
        }

        protected ScriptVerbatim(string value)
        {
            Value = value;
        }


        public ScriptTrivias Trivias { get; set; }

        public string Value { get; set; }

        public override object Evaluate(TemplateContext context)
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