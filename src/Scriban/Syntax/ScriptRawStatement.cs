// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("raw statement", "<raw_text>")]
    public partial class ScriptRawStatement : ScriptStatement
    {
        public string Text { get; set; }

        public int EscapeCount { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Text == null) return null;

            var length = Span.End.Offset - Span.Start.Offset + 1;
            if (length > 0)
            {
                // If we are in the context of output, output directly to TemplateContext.Output
                if (context.EnableOutput)
                {
                    context.Write(Text, Span.Start.Offset, length);
                }
                else
                {
                    return Text.Substring(Span.Start.Offset, length);
                }
            }
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Text == null)
            {
                return;
            }

            if (EscapeCount > 0)
            {
                context.WriteEnterCode(EscapeCount);
            }

            // TODO: handle escape
            var length = Span.End.Offset - Span.Start.Offset + 1;
            if (length > 0)
            {
                context.Write(Text.Substring(Span.Start.Offset, length));
            }

            if (EscapeCount > 0)
            {
                context.WriteExitCode(EscapeCount);
            }
        }

        public override string ToString()
        {
            var length = Span.End.Offset - Span.Start.Offset + 1;
            return Text?.Substring(Span.Start.Offset, length) ?? string.Empty;
        }
    }
}