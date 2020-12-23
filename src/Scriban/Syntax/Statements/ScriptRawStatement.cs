// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

namespace Scriban.Syntax
{
    [ScriptSyntax("raw statement", "<raw_text>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptRawStatement : ScriptStatement
    {
        public ScriptStringSlice Text { get; set; }

        public bool IsEscape { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Text == null) return null;

            if (Text.Length > 0)
            {
                // If we are in the context of output, output directly to TemplateContext.Output
                if (context.EnableOutput)
                {
                    context.Write(Text);
                }
                else
                {
                    return Text.ToString();
                }
            }
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Text != null && Text.Length > 0)
            {
                printer.Write(Text);
            }
        }
    }
}