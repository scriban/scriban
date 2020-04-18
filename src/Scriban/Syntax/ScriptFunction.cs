// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Threading.Tasks;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("function statement", "func <variable> ... end")]
    public partial class ScriptFunction : ScriptStatement, IScriptCustomFunction
    {
        public ScriptVariable Name { get; set; }

        public ScriptStatement Body { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Name != null)
            {
                context.SetValue(Name, this);
            }
            return null;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return Name != null;
        }

        public override string ToString()
        {
            return $"func {Name} ... end";
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Name != null)
            {
                context.Write("func").ExpectSpace();
                context.Write(Name);
            }
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            context.SetValue(ScriptVariable.Arguments, arguments, true);

            // Set the block delegate
            if (blockStatement != null)
            {
                context.SetValue(ScriptVariable.BlockDelegate, blockStatement, true);
            }
            return context.Evaluate(Body);
        }

        public int RequiredParameterCount => 0;

        public bool IsExpressionParameter(int index) => false;

        public int GetParameterIndex(string name) => -1;
    }
}