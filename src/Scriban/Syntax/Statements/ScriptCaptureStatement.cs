// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptExpression? _target;
        private ScriptBlockStatement? _body;
        private ScriptKeyword _captureKeyword = ScriptKeyword.Capture();
        public ScriptCaptureStatement()
        {
            _captureKeyword.Parent = this;
        }

        public ScriptKeyword CaptureKeyword
        {
            get => _captureKeyword;
            set => ParentToThis(ref _captureKeyword, value);
        }

        public ScriptExpression? Target
        {
            get => _target;
            set => ParentToThisNullable(ref _target, value);
        }

        public ScriptBlockStatement? Body
        {
            get => _body;
            set => ParentToThisNullable(ref _body, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            var body = Body;
            var target = Target;
            if (body is null || target is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid capture statement. Target and body are required.");
            }

            // unit test: 230-capture-statement.txt
            context.PushOutput();
            try
            {
                context.Evaluate(body);
            }
            finally
            {
                var result = context.PopOutput();
                context.SetValue(target, result);
            }
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(CaptureKeyword).ExpectSpace();
            if (Target is not null)
            {
                printer.Write(Target).ExpectEos();
            }
            if (Body is not null)
            {
                printer.Write(Body).ExpectEos();
            }
        }
    }
}
