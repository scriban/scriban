// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("interpolated string expression", "$\"string{<expression>}string\"")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptInterpolatedExpression : ScriptExpression
    {
        private ScriptExpression _expression;
        private ScriptToken _openBrace;
        private ScriptToken _closeBrace;

        public ScriptInterpolatedExpression()
        {
            OpenBrace = ScriptToken.OpenInterpBrace();
            CloseBrace = ScriptToken.CloseInterpBrace();
        }

        public ScriptInterpolatedExpression(ScriptExpression expression) : this()
        {
            Expression = expression;
        }

        public ScriptToken OpenBrace
        {
            get => _openBrace;
            set => ParentToThis(ref _openBrace, value);
        }

        public ScriptExpression Expression
        {
            get => _expression;
            set => ParentToThis(ref _expression, value);
        }

        public ScriptToken CloseBrace
        {
            get => _closeBrace;
            set => ParentToThis(ref _closeBrace, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // A nested expression will reset the pipe arguments for the group
            context.PushPipeArguments();
            try
            {
                return context.Evaluate(Expression);
            }
            finally
            {
                if (context.CurrentPipeArguments != null)
                {
                    context.PopPipeArguments();
                }
            }
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(OpenBrace);
            printer.Write(Expression);
            printer.Write(CloseBrace);
        }
    }
}