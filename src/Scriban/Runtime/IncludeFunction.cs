// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// The include function available through the function 'include' in scriban.
    /// </summary>
    public sealed class IncludeFunction : IScriptCustomFunction
    {
        public IncludeFunction()
        {
        }

        public object Evaluate(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement)
        {
            if (parameters.Count == 0)
            {
                throw new ScriptRuntimeException(callerContext.Span, "Expecting at least the name of the template to include for the <include> function");
            }

            string templateName = null;
            try
            {
                templateName = ScriptValueConverter.ToString(callerContext.Span, parameters[0]);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception while converting first parameter for <include> function. Expecting a string", ex);
            }

            // If template name is empty, throw an exception
            if (templateName == null || string.IsNullOrEmpty(templateName = templateName.Trim()))
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Include template name cannot be null or empty");
            }

            // Compute a new parameters for the include
            var newParameters = new ScriptArray(parameters.Count - 1);
            for (int i = 1; i < parameters.Count; i++)
            {
                newParameters[i] = parameters[i];
            }

            context.SetValue(ScriptVariable.Arguments, newParameters, true);

            Template template;

            if (!context.CachedTemplates.TryGetValue(templateName, out template))
            {
                if (context.TemplateLoader == null)
                {
                    throw new ScriptRuntimeException(callerContext.Span,
                        $"Unable to include <{templateName}>. No TemplateLoader registered in TemplateContext.Options.TemplateLoader");
                }

                string templateFilePath;

                var templateText = context.TemplateLoader.Load(context, callerContext.Span, templateName, out templateFilePath);

                if (templateText == null)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"The result of including <{templateName}> cannot be null");
                }

                // IF template file path is not defined, we use the template name instead
                templateFilePath = templateFilePath ?? templateName;

                // Clone parser options
                var parserOptions = context.TemplateLoaderParserOptions.Clone();

                var lexerOptions = context.TemplateLoaderLexerOptions;
                // Parse include in default modes (while top page can be using front matter)
                lexerOptions.Mode = lexerOptions.Mode == ScriptMode.ScriptOnly
                    ? ScriptMode.ScriptOnly
                    : ScriptMode.Default;

                template = Template.Parse(templateText, templateFilePath, parserOptions, lexerOptions);

                // If the template has any errors, throw an exception
                if (template.HasErrors)
                {
                    throw new ScriptParserRuntimeException(callerContext.Span, $"Error while parsing template <{templateName}> from [{templateFilePath}]", template.Messages);
                }

                context.CachedTemplates.Add(templateName, template);
            }

            // Query the pending includes stored in the context
            HashSet<string> pendingIncludes;
            object pendingIncludesObject;
            if (!context.Tags.TryGetValue(typeof(IncludeFunction), out pendingIncludesObject))
            {
                pendingIncludesObject = pendingIncludes = new HashSet<string>();
                context.Tags[typeof (IncludeFunction)] = pendingIncludesObject;
            }
            else
            {
                pendingIncludes = (HashSet<string>) pendingIncludesObject;
            }

            // Make sure that we cannot recursively include a template
            if (pendingIncludes.Contains(templateName))
            {
                throw new ScriptRuntimeException(callerContext.Span, $"The include [{templateName}] cannot be used recursively");
            }
            pendingIncludes.Add(templateName);

            context.PushOutput();
            object result = null;
            try
            {
                template.Render(context);
            }
            finally
            {
                result = context.PopOutput();
                pendingIncludes.Remove(templateName);
            }

            return result;
        }
    }
}