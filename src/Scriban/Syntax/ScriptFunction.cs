// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Threading.Tasks;
using Scriban.Runtime;
using System.Collections.Generic;
using System.Text;


namespace Scriban.Syntax
{
    [ScriptSyntax("function statement", "func <variable> ... end")]
    public partial class ScriptFunction : ScriptStatement, IScriptCustomFunction
    {
        public ScriptVariable Name { get; set; }


        public ScriptToken OpenParen { get; set; }
        
        public List<ScriptVariableGlobal> Parameters { get; set; }
        
        public ScriptToken CloseParen { get; set; }
        
        public ScriptToken EqualToken { get; set; }
        
        public ScriptStatement Body { get; set; }

        public bool HasParameters => Parameters != null;

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
            var builder = new StringBuilder();
            if (HasParameters && !(Body is ScriptBlockStatement))
            {
                builder.Append(Name);
                builder.Append("(");
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var param = Parameters[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(param.Name);
                }
                builder.Append(") = ...");
            }
            else
            {
                builder.Append("func ");
                builder.Append(Name);

                if (HasParameters)
                {
                    builder.Append("(");
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var param = Parameters[i];
                        if (i > 0) builder.Append(", ");
                        builder.Append(param.Name);
                    }
                    builder.Append(")");
                }

                builder.Append(" ... end");
            }

            return builder.ToString();
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Body is ScriptBlockStatement)
            {
                context.Write("func").ExpectSpace();
            }
            context.Write(Name);

            if (OpenParen != null) context.Write(OpenParen);
            if (HasParameters)
            {
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var param = Parameters[i];
                    if (OpenParen == null || i > 0)
                    {
                        context.ExpectSpace();
                    }
                    context.Write(param);
                }
            }
            if (CloseParen != null) context.Write(CloseParen);

            if (Body is ScriptBlockStatement)
            {
                context.ExpectEos();
                context.Write(Body);
                context.ExpectEnd();
            }
            else
            {
                context.Write(EqualToken);
                context.Write(Body);
            }
        }

        public override void Accept(ScriptVisitor visitor) => visitor.Visit(this);

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => visitor.Visit(this);

        protected override IEnumerable<ScriptNode> GetChildren()
        {
            yield return Name;
            if (OpenParen != null) yield return OpenParen;
            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    yield return parameter;
                }
            }
            if (CloseParen != null) yield return CloseParen;
            if (EqualToken != null) yield return EqualToken;
            yield return Body;
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            if (HasParameters)
            {
                context.PushVariableScope(ScriptVariableScope.Global);
            }

            try
            {
                context.SetValue(ScriptVariable.Arguments, arguments, true);

                if (HasParameters)
                {
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var arg = Parameters[i];
                        context.SetValue(arg, arguments[i]);
                    }
                }
                
                // Set the block delegate
                if (blockStatement != null)
                {
                    context.SetValue(ScriptVariable.BlockDelegate, blockStatement, true);
                }

                return context.Evaluate(Body);
            }
            finally
            {
                if (HasParameters)
                {
                    context.PopVariableScope(ScriptVariableScope.Global);
                }
            }
        }

        public int RequiredParameterCount => Parameters?.Count ?? 0;

        public bool IsExpressionParameter(int index) => false;

        public int GetParameterIndex(string name)
        {
            if (Parameters == null) return -1;
            for (var i = 0; i < Parameters.Count; i++)
            {
                var p = Parameters[i];
                if (p.Name == name) return i;
            }

            return -1;
        }
    }
}