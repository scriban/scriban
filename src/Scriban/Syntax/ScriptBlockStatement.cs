// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("block statement", "<statement>...end")]
    public sealed partial class ScriptBlockStatement : ScriptStatement
    {
        public ScriptBlockStatement()
        {
            Statements = new List<ScriptStatement>();
        }

        public List<ScriptStatement> Statements { get; private set; }

        public override object Evaluate(TemplateContext context)
        {
            object result = null;
            for (int i = 0; i < Statements.Count; i++)
            {
                var statement = Statements[i];

                var expressionStatement = statement as ScriptExpressionStatement;
                var isAssign = expressionStatement?.Expression is ScriptAssignExpression;

#if SCRIBAN_ASYNC
                // Throw if cancellation is requested
                if (context.CancellationToken.IsCancellationRequested)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                }
#endif
                result = context.Evaluate(statement);

                // Top-level assignment expression don't output anything
                if (isAssign)
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
            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            foreach (var scriptStatement in Statements)
            {
                context.Write(scriptStatement);
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override string ToString()
        {
            return $"<statements[{Statements.Count}]>";
        }
    }
}