// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using Scriban.Runtime;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("capture statement", "capture <variable> ... end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptCaptureStatement : ScriptStatement
    {
        private ScriptExpression _target;
        private ScriptBlockStatement _body;
        private ScriptKeyword _captureKeyword;

        public ScriptCaptureStatement()
        {
            CaptureKeyword = ScriptKeyword.Capture();
        }

        public ScriptKeyword CaptureKeyword
        {
            get => _captureKeyword;
            set => ParentToThis(ref _captureKeyword, value);
        }

        public ScriptExpression Target
        {
            get => _target;
            set => ParentToThis(ref _target, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // unit test: 230-capture-statement.txt
            context.PushOutput();
            try
            {
                context.Evaluate(Body);
            }
            finally
            {
                var result = context.PopOutput();
                context.SetValue(Target, result);
            }
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(CaptureKeyword).ExpectSpace();
            printer.Write(Target).ExpectEos();
            printer.Write(Body).ExpectEos();
        }
    }
}