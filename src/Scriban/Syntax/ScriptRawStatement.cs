// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("raw statement", "<raw_text>")]
    public class ScriptRawStatement : ScriptStatement
    {
        public string Text { get; set; }

        public bool IsEscape { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Text == null) return null;

            var length = Span.End.Offset - Span.Start.Offset + 1;
            // If we are in the context of output, output directly to TemplateContext.Output
            if (context.EnableOutput)
            {
                context.Write(Text, Span.Start.Offset, length);
            }
            else
            {
                return Text.Substring(Span.Start.Offset, length);
            }
            return null;
        }

        public override string ToString()
        {
            return Text?.Substring(Span.Start.Offset, Span.End.Offset - Span.Start.Offset + 1) ?? string.Empty;
        }
    }
}