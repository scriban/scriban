// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    /// <summary>
    /// A verbatim node (use for custom parsing).
    /// </summary>
    public partial class ScriptToken : ScriptVerbatim
    {
        public static ScriptToken Equal() => new ScriptToken("=");

        public static ScriptToken Pipe() => new ScriptToken("|");

        public static ScriptToken Star() => new ScriptToken("*");

        public static ScriptToken PipeGreater() => new ScriptToken("|>");

        public static ScriptToken OpenParen() => new ScriptToken("(");

        public static ScriptToken CloseParen() => new ScriptToken(")");

        public static ScriptToken OpenBracket() => new ScriptToken("[");

        public static ScriptToken CloseBracket() => new ScriptToken("]");

        public ScriptToken()
        {
        }

        public ScriptToken(string value)
        {
            Value = value;
        }
    }
}