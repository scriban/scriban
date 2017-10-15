// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{

    public abstract class ScriptConditionStatement : ScriptStatement
    {
        public ScriptConditionStatement Else { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.Evaluate(Else);
        }
    }

    [ScriptSyntax("if statement", "if <expression> ... end|else|else if")]
    public class ScriptIfStatement : ScriptConditionStatement
    {
        /// <summary>
        /// Get or sets the condition of this if statement.
        /// </summary>
        public ScriptExpression Condition { get; set; }

        public ScriptBlockStatement Then { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var conditionValue = context.ToBool(context.Evaluate(Condition));
            if (conditionValue)
            {
                return context.Evaluate(Then);
            }
            else
            {
                return base.Evaluate(context);
            }
        }

        public override string ToString()
        {
            return $"if {Condition}";
        }
    }

    [ScriptSyntax("else statement", "else | else if <expression> ... end|else|else if")]
    public class ScriptElseStatement : ScriptConditionStatement
    {
        public ScriptBlockStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            context.Evaluate(Body);
            return base.Evaluate(context);
        }
    }
}