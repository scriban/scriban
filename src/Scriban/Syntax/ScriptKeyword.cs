// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    /// <summary>
    /// A verbatim node (use for custom parsing).
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptKeyword : ScriptVerbatim
    {
        public static ScriptKeyword This() => new ScriptKeyword("this");

        public static ScriptKeyword Func() => new ScriptKeyword("func");

        public static ScriptKeyword Do() => new ScriptKeyword("do");

        public static ScriptKeyword Break() => new ScriptKeyword("break");

        public static ScriptKeyword Capture() => new ScriptKeyword("capture");

        public static ScriptKeyword Case() => new ScriptKeyword("case");

        public static ScriptKeyword Continue() => new ScriptKeyword("continue");

        public static ScriptKeyword Else() => new ScriptKeyword("else");

        public static ScriptKeyword End() => new ScriptKeyword("end");

        public static ScriptKeyword If() => new ScriptKeyword("if");

        public static ScriptKeyword In() => new ScriptKeyword("in");

        public static ScriptKeyword For() => new ScriptKeyword("for");

        public static ScriptKeyword Import() => new ScriptKeyword("import");

        public static ScriptKeyword ReadOnly() => new ScriptKeyword("readonly");

        public static ScriptKeyword Ret() => new ScriptKeyword("ret");

        public static ScriptKeyword TableRow() => new ScriptKeyword("tablerow");

        public static ScriptKeyword When() => new ScriptKeyword("when");

        public static ScriptKeyword While() => new ScriptKeyword("while");

        public static ScriptKeyword With() => new ScriptKeyword("with");

        public static ScriptKeyword Wrap() => new ScriptKeyword("wrap");

        public ScriptKeyword()
        {
        }

        public ScriptKeyword(string value) : base(value)
        {
            Value = value;
        }
    }
}