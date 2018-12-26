// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif
using Scriban.Syntax;

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

#if SCRIBAN_ASYNC
        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(_customFunction(context, callerContext, arguments));
        }
#endif
    }
}