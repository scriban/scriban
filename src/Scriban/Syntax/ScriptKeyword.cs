// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    /// <summary>
    /// A verbatim node (use for custom parsing).
    /// </summary>
    public partial class ScriptKeyword : ScriptVerbatim
    {
        public static ScriptKeyword Func() => new ScriptKeyword("func");

        public static ScriptKeyword Do() => new ScriptKeyword("do");

        public static ScriptKeyword Break() => new ScriptKeyword("break");

        public ScriptKeyword()
        {
        }

        public ScriptKeyword(string value) : base(value)
        {
            Value = value;
        }
    }
}