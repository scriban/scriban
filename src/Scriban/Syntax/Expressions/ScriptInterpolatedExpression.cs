// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("interpolated expression", "{<expression>}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptInterpolatedExpression : ScriptExpression, IScriptVariablePath
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

        public static ScriptInterpolatedExpression Wrap(ScriptExpression expression, bool transferTrivia = false)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var nested = new ScriptInterpolatedExpression()
                {
                    Span = expression.Span,
                    Expression = expression
                };

            if (!transferTrivia) return nested;

            var firstTerminal = expression.FindFirstTerminal();
            firstTerminal?.MoveLeadingTriviasTo(nested.OpenBrace);

            var lastTerminal = expression.FindLastTerminal();
            lastTerminal?.MoveTrailingTriviasTo(nested.CloseBrace, true);

            return nested;
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
                return context.GetValue(this);
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
        public object GetValue(TemplateContext context)
        {
            return context.Evaluate(Expression);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            context.SetValue(Expression, valueToSet);
        }

        public string GetFirstPath()
        {
            return (Expression as IScriptVariablePath)?.GetFirstPath();
        }
    }
}