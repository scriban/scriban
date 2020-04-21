// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Scriban.Runtime;
using System.Collections.Generic;
using System.Text;


namespace Scriban.Syntax
{
    [ScriptSyntax("function statement", "func <variable> ... end")]
    public partial class ScriptFunction : ScriptStatement, IScriptCustomFunction
    {
        private ScriptNode _nameOrDoToken;
        private ScriptToken _openParen;
        private ScriptList<ScriptVariable> _parameters;
        private ScriptToken _closeParen;
        private ScriptToken _equalToken;
        private ScriptStatement _body;

        public ScriptNode NameOrDoToken
        {
            get => _nameOrDoToken;
            set
            {
                if (value != null && (!(value is ScriptVariable || (value is ScriptToken token && token.Value == "do"))))
                {
                    throw new ArgumentException($"Must be a {nameof(ScriptVariable)} or `do` {nameof(ScriptToken)}");
                }

                ParentToThis(ref _nameOrDoToken, value);
            }
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

        public bool IsAnonymous => !(NameOrDoToken is ScriptVariable);

        public bool HasParameters => Parameters != null;

        public override object Evaluate(TemplateContext context)
        {
            if (NameOrDoToken is ScriptVariable variable)
            {
                context.SetValue(variable, this);
            }
            return null;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return NameOrDoToken != null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (!IsAnonymous && Body is ScriptBlockStatement)
            {
                context.Write("func").ExpectSpace();
            }
            context.Write(NameOrDoToken);

            if (OpenParen != null) context.Write(OpenParen);
            if (HasParameters)
            {
                if (OpenParen != null)
                {
                    context.WriteListWithCommas(Parameters);
                }
                else
                {
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var param = Parameters[i];
                        if (i > 0)
                        {
                            context.ExpectSpace();
                        }
                        context.Write(param);
                    }
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