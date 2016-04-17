// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;

namespace Scriban.Runtime
{
    public interface IScriptCustomFunction
    {
        object Evaluate(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement);
    }

    public class DelegateCustomFunction : IScriptCustomFunction
    {
        private readonly Func<TemplateContext, ScriptNode, ScriptArray, object> customFunction;

        public DelegateCustomFunction(Func<TemplateContext, ScriptNode, ScriptArray, object> customFunction)
        {
            this.customFunction = customFunction;
        }

        public object Evaluate(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement)
        {
            return customFunction(context, callerContext, parameters);
        }
    }
}