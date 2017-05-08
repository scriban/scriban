// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("with statement", "with <variable> ... end")]
    public class ScriptWithStatement : ScriptStatement
    {
        public ScriptExpression Name { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            var target = context.GetValue(Name);
            if (!(target is IScriptObject))
            {
                var targetName = target?.GetType().Name ?? "null";
                throw new ScriptRuntimeException(Name.Span, $"Invalid target property [{Name}] used for [with] statement. Must be a ScriptObject instead of [{targetName}]");
            }

            context.PushGlobal((IScriptObject)target);
            try
            {
                context.Evaluate(Body);
            }
            finally
            {
                context.PopGlobal();
            }
        }

        public override string ToString()
        {
            return $"with {Name} <...> end";
        }
    }
}