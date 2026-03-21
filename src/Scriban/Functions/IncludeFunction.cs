// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// The include function available through the function 'include' in scriban.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed partial class IncludeFunction : IScriptCustomFunction
    {
        public IncludeFunction()
        {
        }

        public object? Invoke(TemplateContext context, ScriptNode? callerContext, ScriptArray arguments, ScriptBlockStatement? blockStatement)
        {
            var callerSpan = callerContext?.Span ?? context.CurrentSpan;
            var resolvedCallerContext = callerContext ?? context.CurrentNode;
            if (arguments.Count == 0)
            {
                throw new ScriptRuntimeException(callerSpan, "Expecting at least the name of the template to include for the <include> function");
            }

            var templateName = context.ObjectToString(arguments[0]);
            if (resolvedCallerContext is null)
            {
                throw new ScriptRuntimeException(callerSpan, "Unable to resolve the include caller context.");
            }

            var templatePath = context.GetTemplatePathFromName(templateName, resolvedCallerContext);
            // liquid compatibility
            if (templatePath is null) return null;

            Template template = context.GetOrCreateTemplate(templatePath, resolvedCallerContext);

            return context.RenderTemplate(template, arguments, resolvedCallerContext);
        }

        public int RequiredParameterCount => 1;

        public int ParameterCount => 1;

        public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;

        public Type ReturnType => typeof(object);

        public ScriptParameterInfo GetParameterInfo(int index)
        {
            if (index == 0) return new ScriptParameterInfo(typeof(string), "template_name");
            return new ScriptParameterInfo(typeof(object), "value");
        }

        public int GetParameterIndexByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
