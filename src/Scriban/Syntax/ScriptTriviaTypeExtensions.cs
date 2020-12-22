// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptTriviaTypeExtensions
    {
        public static bool IsSpace(this ScriptTriviaType triviaType)
        {
            return triviaType switch
            {
                ScriptTriviaType.Whitespace => true,
                ScriptTriviaType.WhitespaceFull => true,
                _ => false
            };
        }

        public static bool IsNewLine(this ScriptTriviaType triviaType)
        {
            return triviaType == ScriptTriviaType.NewLine;
        }

        public static bool IsSpaceOrNewLine(this ScriptTriviaType triviaType)
        {
            return triviaType switch
            {
                ScriptTriviaType.Whitespace => true,
                ScriptTriviaType.WhitespaceFull => true,
                ScriptTriviaType.NewLine => true,
                _ => false
            };
        }
    }
}