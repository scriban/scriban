// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("nested expression", "(<expression>)")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptNestedExpression : ScriptExpression, IScriptVariablePath
    {
        private ScriptExpression _expression;
        private ScriptToken _openParen;
        private ScriptToken _closeParen;

        public ScriptNestedExpression()
        {
            OpenParen = ScriptToken.OpenParen();
            CloseParen = ScriptToken.CloseParen();
        }

        public ScriptNestedExpression(ScriptExpression expression) : this()
        {
            Expression = expression;
        }

        public static ScriptNestedExpression Wrap(ScriptExpression expression, bool transferTrivia = false)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var nested = new ScriptNestedExpression()
                {
                    Span = expression.Span,
                    Expression = expression
                };

            if (!transferTrivia) return nested;

            var firstTerminal = expression.FindFirstTerminal();
            firstTerminal?.MoveLeadingTriviasTo(nested.OpenParen);

            var lastTerminal = expression.FindLastTerminal();
            lastTerminal?.MoveTrailingTriviasTo(nested.CloseParen, true);

            return nested;
        }

        public ScriptToken OpenParen
        {
            get => _openParen;
            set => ParentToThis(ref _openParen, value);
        }

        public ScriptExpression Expression
        {
            get => _expression;
            set => ParentToThis(ref _expression, value);
        }

        public ScriptToken CloseParen
        {
            get => _closeParen;
            set => ParentToThis(ref _closeParen, value);
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
            printer.Write(OpenParen);
            printer.Write(Expression);
            printer.Write(CloseParen);
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