// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("import statement", "import <expression>")]
    public class ScriptImportStatement : ScriptExpressionStatement
    {
        public override object Evaluate(TemplateContext context)
        {
            var value = base.Evaluate(context);
            if (value == null)
            {
                return null;
            }
            var scriptObject = value as ScriptObject;
            if (scriptObject == null)
            {
                throw new ScriptRuntimeException(Expression.Span, $"Unexpected value [{value.GetType()}] for import. Expecting an plain script object {{}}");
            }

            context.CurrentGlobal.Import(scriptObject);
            return null;
        }
    }
}