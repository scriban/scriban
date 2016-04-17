// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
namespace Scriban.Runtime
{
    [ScriptSyntax("error statement", "error <message>")]
    public class ScriptErrorStatement : ScriptExpressionStatement
    {
        public override void Evaluate(TemplateContext context)
        {
            base.Evaluate(context);
            var errorMessage = ScriptValueConverter.ToString(this.Span, context.Result);
            throw new ScriptRuntimeException(this.Span, errorMessage);
        }
    }
}