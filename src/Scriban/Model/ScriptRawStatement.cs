// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("raw statement", "<raw_text>")]
    public class ScriptRawStatement : ScriptStatement
    {
        public string Text { get; set; }

        public override void Evaluate(TemplateContext context)
        {           
            if (Text != null)
            {
                context.Output.Append(Text, Span.Start.Offset, Span.End.Offset - Span.Start.Offset + 1);
            }
        }

        public override string ToString()
        {
            return Text?.Substring(Span.Start.Offset, Span.End.Offset - Span.Start.Offset + 1);
        }
    }
}