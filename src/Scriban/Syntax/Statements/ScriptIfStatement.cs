// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("if statement", "if <expression> ... end|else|else if")]
    public partial class ScriptIfStatement : ScriptConditionStatement
    {
        private ScriptExpression _condition;
        private ScriptBlockStatement _then;
        private ScriptConditionStatement _else;

        /// <summary>
        /// Get or sets the condition of this if statement.
        /// </summary>
        public ScriptExpression Condition
        {
            get => _condition;
            set => ParentToThis(ref _condition, value);
        }

        /// <summary>
        /// Gets or sets a boolean indicating that the result of the condition is inverted
        /// </summary>
        public bool InvertCondition { get; set; }

        public ScriptBlockStatement Then
        {
            get => _then;
            set => ParentToThis(ref _then, value);
        }

        public ScriptConditionStatement Else
        {
            get => _else;
            set => ParentToThis(ref _else, value);
        }
        
        public bool IsElseIf { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var conditionValue = context.ToBool(Condition.Span, context.Evaluate(Condition));
            if (InvertCondition)
            {
                conditionValue = !conditionValue;
            }
            return conditionValue ? context.Evaluate(Then) : context.Evaluate(Else);
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (IsElseIf)
            {
                context.Write("else ");
            }
            context.Write("if").ExpectSpace();
            if (InvertCondition)
            {
                context.Write("!(");
            }
            context.Write(Condition);
            if (InvertCondition)
            {
                context.Write(")");
            }
            context.ExpectEos();

            context.Write(Then);

            context.Write(Else);

            if (!IsElseIf)
            {
                context.ExpectEnd();
            }
        }
    }
}