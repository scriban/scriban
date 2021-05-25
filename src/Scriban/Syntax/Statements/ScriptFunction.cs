// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("function statement", "func <variable> ... end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptFunction : ScriptStatement, IScriptCustomFunction
    {
        private ScriptKeyword _funcToken;
        private ScriptNode _nameOrDoToken;
        private ScriptToken _openParen;
        private ScriptList<ScriptParameter> _parameters;
        private ScriptToken _closeParen;
        private ScriptToken _equalToken;
        private ScriptStatement _body;
        private bool _hasReturnType;
        private ScriptVarParamKind _varParamKind;
        private int _requiredParameterCount;

        public ScriptFunction()
        {
            _varParamKind = ScriptVarParamKind.Direct;
        }

        public ScriptKeyword FuncToken
        {
            get => _funcToken;
            set => ParentToThis(ref _funcToken, value);
        }

        public ScriptNode NameOrDoToken
        {
            get => _nameOrDoToken;
            set
            {
                if (value != null && (!(value is ScriptVariable || (value is ScriptKeyword token && token.Value == "do"))))
                {
                    throw new ArgumentException($"Must be a {nameof(ScriptVariable)} or `do` {nameof(ScriptKeyword)}");
                }

                ParentToThis(ref _nameOrDoToken, value);
            }
        }

        public ScriptToken OpenParen
        {
            get => _openParen;
            set => ParentToThis(ref _openParen, value);
        }

        public ScriptList<ScriptParameter> Parameters
        {
            get => _parameters;
            set
            {
                ParentToThis(ref _parameters, value);

                // Pre-calculate parameters
                _requiredParameterCount = _parameters?.Count ?? 0;
                _varParamKind = _parameters == null ? ScriptVarParamKind.Direct : ScriptVarParamKind.None;
                if (_parameters != null)
                {
                    for (int i = 0; i < _parameters.Count; i++)
                    {
                        var param = _parameters[i];
                        var token = param.EqualOrTripleDotToken;
                        if (token != null)
                        {
                            if (token.TokenType == TokenType.TripleDot)
                            {
                                _requiredParameterCount--;
                                _varParamKind = ScriptVarParamKind.LastParameter;
                            }

                            if (token.TokenType == TokenType.Equal)
                            {
                                _requiredParameterCount--;
                            }
                        }
                    }
                }
            }
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
            set
            {
                ParentToThis(ref _body, value);
                UpdateReturnType();
            }
        }

        public void UpdateReturnType()
        {
            _hasReturnType = Body is ScriptExpressionStatement || FindRetVisitor.HasRet(Body);
        }

        public bool IsAnonymous => !(NameOrDoToken is ScriptVariable);

        public bool HasParameters => Parameters != null;

        public override object Evaluate(TemplateContext context)
        {
            if (NameOrDoToken is ScriptVariable variable)
            {
                context.SetValue(variable, this);
                return null;
            }
            else
            {
                return this;
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return NameOrDoToken != null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (!IsAnonymous && Body is ScriptBlockStatement)
            {
                printer.Write(FuncToken).ExpectSpace();
            }
            printer.Write(NameOrDoToken);

            if (OpenParen != null) printer.Write(OpenParen);
            if (HasParameters)
            {
                if (OpenParen != null)
                {
                    printer.WriteListWithCommas(Parameters);
                }
                else
                {
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var param = Parameters[i];
                        if (i > 0)
                        {
                            printer.ExpectSpace();
                        }
                        printer.Write(param);
                    }
                }
            }
            if (CloseParen != null) printer.Write(CloseParen);

            if (Body is ScriptBlockStatement)
            {
                printer.ExpectEos();
                printer.Write(Body);
            }
            else
            {
                printer.Write(EqualToken);
                printer.Write(Body);
            }

            if (!IsAnonymous)
            {
                printer.ExpectEos();
            }
        }
        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            bool hasParams = HasParameters;
            if (hasParams)
            {
                context.PushGlobal(new ScriptObject());
            }
            else
            {
                context.PushLocal();
            }
            try
            {
                if (NameOrDoToken is ScriptVariableLocal localVariable)
                {
                    context.SetValue(localVariable, this);
                }

                context.SetValue(ScriptVariable.Arguments, arguments, true);

                if (hasParams)
                {
                    var glob = context.CurrentGlobal;
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        var param = Parameters[i];
                        glob.SetValue(param.Name.Name, arguments[i], false);
                    }
                }

                // Set the block delegate
                if (blockStatement != null)
                {
                    context.SetValue(ScriptVariable.BlockDelegate, blockStatement, true);
                }

                var result = context.Evaluate(Body);
                return result;
            }
            finally
            {
                if (hasParams)
                {
                    context.PopGlobal();
                }
                else
                {
                    context.PopLocal();
                }
            }
        }

        public int RequiredParameterCount => _requiredParameterCount;

        public int ParameterCount => Parameters?.Count ?? 0;

        public ScriptVarParamKind VarParamKind => _varParamKind;

        public Type ReturnType => _hasReturnType ? typeof(object) : typeof(void);

        public ScriptParameterInfo GetParameterInfo(int index)
        {
            if (Parameters == null) return new ScriptParameterInfo(typeof(object), string.Empty);
            var parameterCount = ParameterCount;
            if (index > parameterCount - 1)
            {
                index = parameterCount - 1;
            }
            var param = Parameters[index];
            var name = param.Name.Name;
            var defaultValue = param.DefaultValue?.Value;
            return defaultValue != null ? new ScriptParameterInfo(typeof(object), name, defaultValue) : new ScriptParameterInfo(typeof(object), name);
        }

        /// <summary>
        /// Finds a <see cref="ScriptReturnStatement"/> in a tree.
        /// TODO: could be provided as a generic version
        /// </summary>
        private class FindRetVisitor : ScriptVisitor
        {
            [ThreadStatic] private static FindRetVisitor _instance;

            private FindRetVisitor(){}

            public static bool HasRet(ScriptNode node)
            {
                if (node == null) return false;
                var local = _instance ??= new FindRetVisitor();
                local.Found = false;
                local.Visit(node);
                return local.Found;
            }

            public bool Found { get; private set; }

            public override void Visit(ScriptReturnStatement node)
            {
                Found = true;
            }

            protected override void DefaultVisit(ScriptNode node)
            {
                if (Found) return;

                if (node == null)
                    return;

                var childrenCount = node.ChildrenCount;
                for(int i = 0; i < childrenCount; i++)
                {
                    var child = node.GetChildren(i);
                    Visit(child);
                    if (Found) return; // early exit if found
                }
            }
        }
    }
}