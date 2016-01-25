// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
namespace Textamina.Scriban.Runtime
{
    [ScriptSyntax("with statement", "while <variable> ... end")]
    public class ScriptWithStatement : ScriptStatement
    {
        public ScriptVariable Name { get; set; }

        public ScriptBlockStatement Body { get; set; }

        public override void Evaluate(TemplateContext context)
        {
            var target = context.GetValue(Name);
            if (!(target is ScriptObject))
            {
                throw new ScriptRuntimeException(Name.Span, $"Invalid variable used for with. Must be a ScriptObject instead of [{target?.GetType().Name}");
            }

            context.PushGlobal((ScriptObject)target);
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