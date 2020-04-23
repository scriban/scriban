// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    public abstract class ScriptVerbatim : ScriptNode
    {
        protected ScriptVerbatim()
        {
        }

        protected ScriptVerbatim(string value)
        {
            Value = value;
        }


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