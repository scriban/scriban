// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
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
    public sealed partial class IncludeFunction : IScriptCustomFunction
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

            var templateName = context.ToString(callerContext.Span, arguments[0]);

            // If template name is empty, throw an exception
            if (string.IsNullOrEmpty(templateName))
            {
                // In a liquid template context, we let an include to continue without failing
                if (context is LiquidTemplateContext)
                {
                    return null;
                }
                throw new ScriptRuntimeException(callerContext.Span, $"Include template name cannot be null or empty");
            }

            var templateLoader = context.TemplateLoader;
            if (templateLoader == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unable to include <{templateName}>. No TemplateLoader registered in TemplateContext.TemplateLoader");
            }

            string templatePath;

            try
            {
                templatePath = templateLoader.GetPath(context, callerContext.Span, templateName);
            }
            catch (Exception ex) when (!(ex is ScriptRuntimeException))
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception while getting the path for the include name `{templateName}`", ex);
            }
            // If template path is empty (probably because template doesn't exist), throw an exception
            if (templatePath == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Include template path is null for `{templateName}");
            }

            // Compute a new parameters for the include
            var newParameters = new ScriptArray(arguments.Count - 1);
            for (int i = 1; i < arguments.Count; i++)
            {
                newParameters[i] = arguments[i];
            }

            context.SetValue(ScriptVariable.Arguments, newParameters, true);

            Template template;

            if (!context.CachedTemplates.TryGetValue(templatePath, out template))
            {

                string templateText;
                try
                {
                    templateText = templateLoader.Load(context, callerContext.Span, templatePath);
                }
                catch (Exception ex) when (!(ex is ScriptRuntimeException))
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception while loading the include `{templateName}` from path `{templatePath}`", ex);
                }

                if (templateText == null)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"The result of including `{templateName}->{templatePath}` cannot be null");
                }

                // Clone parser options
                var parserOptions = context.TemplateLoaderParserOptions;
                var lexerOptions = context.TemplateLoaderLexerOptions;
                template = Template.Parse(templateText, templatePath, parserOptions, lexerOptions);

                // If the template has any errors, throw an exception
                if (template.HasErrors)
                {
                    throw new ScriptParserRuntimeException(callerContext.Span, $"Error while parsing template `{templateName}` from `{templatePath}`", template.Messages);
                }

                context.CachedTemplates.Add(templatePath, template);
            }

            // Make sure that we cannot recursively include a template

            context.PushOutput();
            object result = null;
            try
            {
                context.EnterRecursive(callerContext);
                result = template.Render(context);
                context.ExitRecursive(callerContext);
            }
            finally
            {
                context.PopOutput();
            }

            return result;
        }
    }
}