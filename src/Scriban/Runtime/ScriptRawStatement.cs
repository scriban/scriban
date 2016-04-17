// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
namespace Scriban.Runtime
{
    [ScriptSyntax("raw statement", "<raw_text>")]
    public class ScriptRawStatement : ScriptStatement
    {
        public string Text { get; set; }

        public override void Evaluate(TemplateContext context)
        {           
            var text = Text?.Substring(this.Span.Start.Offset, this.Span.End.Offset - this.Span.Start.Offset + 1);
            if (text != null)
            {
                context.Write(text);
            }
        }

        public override string ToString()
        {
            return Text?.Substring(this.Span.Start.Offset, this.Span.End.Offset - this.Span.Start.Offset + 1);
        }
    }
}