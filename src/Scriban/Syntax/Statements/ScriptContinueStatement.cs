// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.Linq;

namespace Scriban.Syntax
{
    [ScriptSyntax("continue statement", "continue")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptContinueStatement : ScriptStatement
    {
        private ScriptKeyword _continueKeyword;

        public ScriptContinueStatement()
        {
            ContinueKeyword = ScriptKeyword.Continue();
        }

        public ScriptKeyword ContinueKeyword
        {
            get => _continueKeyword;
            set => ParentToThis(ref _continueKeyword, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // Only valid when we are in a loop (this should not happen as this is detected by the parser)
            if (context.IsInLoop)
            {
                context.FlowState = ScriptFlowState.Continue;
            }
            else
            {
                if (context.EnableBreakAndContinueAsReturnOutsideLoop)
                {
                    context.FlowState = ScriptFlowState.Return;
                }
                else
                {
                    // unit test: 216-break-continue-error2.txt
                    throw new ScriptRuntimeException(Span, $"The <continue> statement can only be used inside for/while loops");
                }
            }
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ContinueKeyword).ExpectEos();
        }
    }
}