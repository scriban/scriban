// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("block statement", "<statement>...end")]
    public class ScriptBlockStatement : ScriptStatement
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

        protected override void WriteImpl(RenderContext context)
        {
            var isNextStatementRaw = context.IsNextStatementRaw;
            for (var i = 0; i < Statements.Count; i++)
            {
                var scriptStatement = Statements[i];

                var rawStatement = scriptStatement as ScriptRawStatement;

                context.IsNextStatementRaw = i + 1 < Statements.Count ? Statements[i + 1] is ScriptRawStatement : isNextStatementRaw;

                scriptStatement.Write(context);

                context.PreviousRawStatement = rawStatement;
            }
            context.IsNextStatementRaw = isNextStatementRaw;
        }

        public override string ToString()
        {
            return $"<statements[{Statements.Count}]>";
        }
    }
}