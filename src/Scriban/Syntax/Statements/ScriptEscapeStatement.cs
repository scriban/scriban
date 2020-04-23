// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace Scriban.Syntax
{
    [ScriptSyntax("{{ or }}", "{{ or }}")]
    public partial class ScriptEscapeStatement : ScriptStatement
    {
        public ScriptEscapeStatement()
        {
        }

        public ScriptWhitespaceMode WhitespaceMode { get; set; }

        public bool IsEntering { get; set; }


        public bool IsClosing => !IsEntering;

        public int EscapeCount { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (IsEntering)
            {
                context.WriteEnterCode(EscapeCount);
                WriteWhitespaceMode(context);
            }
            else
            {
                WriteWhitespaceMode(context);
                context.WriteExitCode(EscapeCount);
            }
        }

        private void WriteWhitespaceMode(TemplateRewriterContext context)
        {
            switch (WhitespaceMode)
            {
                case ScriptWhitespaceMode.None:
                    break;
                case ScriptWhitespaceMode.Greedy:
                    context.Write("-");
                    break;
                case ScriptWhitespaceMode.NonGreedy:
                    context.Write("~");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ScriptWhitespaceMode
    {
        /// <summary>
        /// No change in whitespace
        /// </summary>
        None,

        /// <summary>
        /// The greedy mode using the character - (e.g {{- or -}}), removes any whitespace, including newlines
        /// </summary>
        Greedy,

        /// <summary>
        /// he non greedy mode using the character ~.
        ///
        /// - Using a {{~ will remove any whitespace before but will stop on the first newline without including it
        /// - Using a ~}} will remove any whitespace after including the first newline but will stop after
        /// </summary>
        NonGreedy,
    }
}