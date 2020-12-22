// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Scriban.Parsing;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    readonly partial struct ScriptTrivia
    {
        public static readonly ScriptTrivia Space = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.Whitespace, (ScriptStringSlice)" ");

        public static readonly ScriptTrivia Comma = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.Comma, (ScriptStringSlice)",");

        public static readonly ScriptTrivia SemiColon = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.SemiColon, (ScriptStringSlice)";");

        public ScriptTrivia(SourceSpan span, ScriptTriviaType type, ScriptStringSlice text)
        {
            Span = span;
            Type = type;
            Text = text;
        }

        public readonly SourceSpan Span;

        public readonly ScriptTriviaType Type;

        public readonly ScriptStringSlice Text;

        public ScriptTrivia WithText(ScriptStringSlice text)
        {
            return new ScriptTrivia(Span, Type, text);
        }

        public void Write(ScriptPrinter printer)
        {
            var rawText = ToString();

            bool isRawComment = Type == ScriptTriviaType.CommentMulti && !rawText.StartsWith("##");
            if (isRawComment)
            {
                // Escape any # by \#
                rawText = rawText.Replace("#", "\\#");
                // Escape any }}
                rawText = rawText.Replace("}", "\\}");
                printer.Write("## ");
            }

            printer.Write(rawText);

            if (isRawComment)
            {
                printer.Write(" ##");
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ScriptTriviaType.Empty:
                    return string.Empty;
                case ScriptTriviaType.Comma:
                    return ",";
                case ScriptTriviaType.SemiColon:
                    return ";";
            }
            return (string)Text;
        }
    }
}