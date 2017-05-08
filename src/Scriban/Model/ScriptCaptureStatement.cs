// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("capture statement", "capture <variable> ... end")]
    public class ScriptCaptureStatement : ScriptStatement
    {
        public ScriptExpression Target { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            // unit test: 230-capture-statement.txt
            context.PushOutput();
            {
                context.Evaluate(Body);
            }
            var result = context.PopOutput();

            context.SetValue(Target, result);
        }
    }
}