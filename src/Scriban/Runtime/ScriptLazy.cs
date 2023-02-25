// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Threading.Tasks;
using System;
using Scriban.Syntax;

namespace Scriban.Runtime
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptLazy<T> : IScriptCustomFunction
    {
        private readonly Lazy<T> _lazy;

        public ScriptLazy(Func<T> valueFactory)
        {
            _lazy = new Lazy<T>(valueFactory);
        }

        public int RequiredParameterCount => 0;

        public int ParameterCount => 0;

        public ScriptVarParamKind VarParamKind => ScriptVarParamKind.None;

        public Type ReturnType => typeof(T);

        public ScriptParameterInfo GetParameterInfo(int index) => default;

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return _lazy.Value;
        }

#if !SCRIBAN_NO_ASYNC
        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(_lazy.Value);
        }
#endif
    }
}