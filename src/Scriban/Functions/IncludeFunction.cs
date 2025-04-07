// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            if (arguments.Count == 0)
            {
                throw new ScriptRuntimeException(callerContext.Span, "Expecting at least the name of the template to include for the <include> function");
            }

            var templateName = context.ObjectToString(arguments[0]);
            var templatePath = context.GetTemplatePathFromName(templateName, callerContext);
            // liquid compatibility
            if (templatePath == null) return null;

            Template template = context.GetOrCreateTemplate(templatePath, callerContext);

            return context.RenderTemplate(template, arguments, callerContext);
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