// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("raw statement", "<raw_text>")]
    public partial class ScriptRawStatement : ScriptStatement
    {
        public string Text { get; set; }

        public int SliceIndex { get; set; }

        public int SliceLength { get; set; }

        public bool IsEscape { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Text == null) return null;

            if (SliceLength > 0)
            {
                // If we are in the context of output, output directly to TemplateContext.Output
                if (context.EnableOutput)
                {
                    context.Write(Text, SliceIndex, SliceLength);
                }
                else
                {
                    return Text.Substring(SliceIndex, SliceLength);
                }
            }
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Text != null && SliceLength > 0)
            {
                context.Write(Text.Substring(SliceIndex, SliceLength));
            }
        }
    }
}