// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("block statement", "<statement>...end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed partial class ScriptBlockStatement : ScriptStatement
    {
        private ScriptList<ScriptStatement> _statements;

        public ScriptBlockStatement()
        {
            Statements = new ScriptList<ScriptStatement>();
        }

        public ScriptList<ScriptStatement> Statements
        {
            get => _statements;
            set => ParentToThis(ref _statements, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var autoIndent = context.AutoIndent;
            object result = null;
            var statements = Statements;
            string previousIndent = context.CurrentIndent;
            string currentIndent = previousIndent;
            try
            {
                for (int i = 0; i < statements.Count; i++)
                {
                    var statement = statements[i];

                    // Throws a cancellation
                    context.CheckAbort();

                    if (autoIndent && statement is ScriptEscapeStatement escape)
                    {
                        if (escape.IsEntering)
                        {
                            currentIndent = escape.Indent;
                        }
                        else if (escape.IsClosing)
                        {
                            currentIndent = previousIndent;
                        }
                    }
                    context.CurrentIndent = currentIndent;

                    if (statement.CanSkipEvaluation)
                    {
                        continue;
                    }

                    result = context.Evaluate(statement);

                    // Top-level assignment expression don't output anything
                    if (!statement.CanOutput)
                    {
                        result = null;
                    }
                    else if (result != null && context.FlowState != ScriptFlowState.Return && context.EnableOutput)
                    {
                        context.Write(Span, result);
                        result = null;
                    }

                    // If flow state is different, we need to exit this loop
                    if (context.FlowState != ScriptFlowState.None)
                    {
                        break;
                    }
                }
            }
            finally
            {
                context.CurrentIndent = previousIndent;
            }

            return result;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            foreach (var scriptStatement in Statements)
            {
                printer.Write(scriptStatement);
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }
    }
}