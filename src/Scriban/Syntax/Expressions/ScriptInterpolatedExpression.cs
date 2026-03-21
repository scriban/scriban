// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptExpression? _expression;
        private ScriptToken _openBrace;
        private ScriptToken _closeBrace;

        public ScriptInterpolatedExpression()
        {
            _openBrace = ScriptToken.OpenInterpBrace();
            _openBrace.Parent = this;
            _closeBrace = ScriptToken.CloseInterpBrace();
            _closeBrace.Parent = this;
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

        public ScriptExpression? Expression
        {
            get => _expression;
            set => ParentToThisNullable(ref _expression, value);
        }

        public ScriptToken CloseBrace
        {
            get => _closeBrace;
            set => ParentToThis(ref _closeBrace, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Expression is null)
            {
                return null;
            }

            // A nested expression will reset the pipe arguments for the group
            context.PushPipeArguments();
            try
            {
                return context.Evaluate(Expression);
            }
            finally
            {
                if (context.CurrentPipeArguments is not null)
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
