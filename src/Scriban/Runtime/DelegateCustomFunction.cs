// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Threading.Tasks;
using Scriban.Syntax;
using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// An implementation of <see cref="IScriptCustomFunction"/> using a function delegate.
    /// </summary>
    public class DelegateCustomFunction : IScriptCustomFunction
    {
        private readonly Func<TemplateContext, ScriptNode, ScriptArray, object> _customFunction;

        public DelegateCustomFunction(Func<TemplateContext, ScriptNode, ScriptArray, object> customFunction)
        {
            _customFunction = customFunction ?? throw new ArgumentNullException(nameof(customFunction));
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return _customFunction(context, callerContext, arguments);
        }

        public int RequiredParameterCount => 0;

        public bool IsExpressionParameter(int index) => false;

        public int GetParameterIndex(string name) => -1;

#if !SCRIBAN_NO_ASYNC
        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(_customFunction(context, callerContext, arguments));
        }
#endif
    }
}