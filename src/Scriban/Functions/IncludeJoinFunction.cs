// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// The include join function available through the function 'include_join' in scriban.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed partial class IncludeJoinFunction : IScriptCustomFunction
    {
        public IncludeJoinFunction()
        {
        }

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            if (arguments.Count < 2)
            {
                throw new ScriptRuntimeException(callerContext.Span, "Expecting at least the separator and components to include for the <include_join> function.");
            }

            var templateNames = 
                (arguments[0] as ScriptArray)?.Select(x => context.ObjectToString(x)).ToArray() 
                ?? (arguments[0] as IEnumerable<string>)?.ToArray();

            if (templateNames == null)
            {
                return string.Empty;
            }

            var separator = RenderComponent(context, callerContext, arguments, context.ObjectToString(arguments[1]) ?? string.Empty);
            var start = RenderComponent(context, callerContext, arguments, arguments.Count >= 2 ? context.ObjectToString(arguments[2]) : string.Empty);
            var end = RenderComponent(context, callerContext, arguments, arguments.Count >= 3 ? context.ObjectToString(arguments[3]) : string.Empty);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(start))
            {
                sb.Append(start);
            }
            for (int i = 0; i < templateNames.Length; ++i)
            {
                var templateName = templateNames[i];
                var templatePath = context.GetTemplatePathFromName(templateName, callerContext);

                // liquid compatibility
                if (templatePath == null) continue;

                Template template = context.GetOrCreateTemplate(templatePath, callerContext);

                sb.Append(context.RenderTemplate(template, arguments, callerContext));

                if (!string.IsNullOrEmpty(separator) && i < templateNames.Length - 1)
                {
                    sb.Append(separator);
                }
            }

            if (!string.IsNullOrEmpty(end))
            {
                sb.Append(end);
            }

            return sb.ToString();
        }

        public int RequiredParameterCount => 2;

        public int ParameterCount => 4;

        public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;

        public Type ReturnType => typeof(object);

        public ScriptParameterInfo GetParameterInfo(int index)
        {
            switch (index)
            {
                case 0:
                    return new ScriptParameterInfo(typeof(IList), "template_names");
                case 1:
                    return new ScriptParameterInfo(typeof(string), "separator");
                case 2:
                    return new ScriptParameterInfo(typeof(string), "prefix");
                case 3:
                    return new ScriptParameterInfo(typeof(string), "suffix");
                
            }
            throw new IndexOutOfRangeException();
        }

        private string RenderComponent(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, string component)
        {
            if (!component.StartsWith("tpl:"))
                return component;

            var path = context.GetTemplatePathFromName(component.Substring(4), callerContext);
            var template = context.GetOrCreateTemplate(path, callerContext);
            return context.RenderTemplate(template, arguments, callerContext); 
        }
    }
}