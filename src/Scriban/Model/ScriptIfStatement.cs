// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{

    public abstract class ScriptConditionStatement : ScriptStatement
    {
        public ScriptConditionStatement Else { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            Else?.Evaluate(context);
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

        public override void Evaluate(TemplateContext context)
        {
            var conditionValue = ScriptValueConverter.ToBool(context.Evaluate(Condition));
            if (conditionValue)
            {
                Then?.Evaluate(context);
            }
            else
            {
                base.Evaluate(context);
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

        public override void Evaluate(TemplateContext context)
        {
            Body?.Evaluate(context);
            base.Evaluate(context);
        }
    }
}