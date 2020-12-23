// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

namespace Scriban.Syntax
{
    [ScriptSyntax("break statement", "break")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptBreakStatement : ScriptStatement
    {
        private ScriptKeyword _breakKeyword;

        public ScriptBreakStatement()
        {
            BreakKeyword = ScriptKeyword.Break();
        }

        public ScriptKeyword BreakKeyword
        {
            get => _breakKeyword;
            set => ParentToThis(ref _breakKeyword, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // Only valid when we are in a loop (this should not happen as this is detected by the parser)
            if (context.IsInLoop)
            {
                context.FlowState = ScriptFlowState.Break;
            }
            else
            {
                if (context.EnableBreakAndContinueAsReturnOutsideLoop)
                {
                    context.FlowState = ScriptFlowState.Return;
                }
                else
                {
                    // unit test: 216-break-continue-error1.txt
                    throw new ScriptRuntimeException(Span, $"The <break> statement can only be used inside for/while loops");
                }
            }
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(BreakKeyword).ExpectEos();
        }
    }
}