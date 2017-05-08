// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("assign expression", "<target_expression> = <value_expression>")]
    public class ScriptAssignExpression : ScriptExpression
    {
        public ScriptExpression Target { get; set; }

        public ScriptExpression Value { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            var valueObject = context.Evaluate(Value);
            context.SetValue(Target, valueObject);
            context.Result = valueObject;
        }

        public override string ToString()
        {
            return $"{Target} = {Value}";
        }
    }
}