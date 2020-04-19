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
        private ScriptVariable _name;
        private ScriptToken _openParen;
        private ScriptList<ScriptVariable> _parameters;
        private ScriptToken _closeParen;
        private ScriptToken _equalToken;
        private ScriptStatement _body;

        public ScriptVariable Name
        {
            get => _name;
            set => ParentToThis(ref _name, value);
        }

        public ScriptToken OpenParen
        {
            get => _openParen;
            set => ParentToThis(ref _openParen, value);
        }

        public ScriptList<ScriptVariable> Parameters
        {
            get => _parameters;
            set => ParentToThis(ref _parameters, value);
        }

        public ScriptToken CloseParen
        {
            get => _closeParen;
            set => ParentToThis(ref _closeParen, value);
        }

        public ScriptToken EqualToken
        {
            get => _equalToken;
            set => ParentToThis(ref _equalToken, value);
        }

        public ScriptStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public bool IsAnonymous { get; set; }

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
            if (!IsAnonymous)
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
            }

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