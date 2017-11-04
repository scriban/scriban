// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("include expression", "include <name> [,param1: paramValue, param2:...]")]
    public class ScriptIncludeExpression : ScriptFunctionCall, IScriptNamedParameterContainer
    {
        public List<ScriptNamedParameter> NamedParameters { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            if (Arguments.Count == 0)
            {
                throw new ScriptRuntimeException(Span, "Expecting at least the name of the template to include for the <include> function");
            }

            string templateName = null;
            try
            {
                var name = context.Evaluate(Arguments[0]);
                templateName = context.ToString(Span, name);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(Span, $"Unexpected exception while converting first parameter for <include> function. Expecting a string", ex);
            }

            // If template name is empty, throw an exception
            if (templateName == null || string.IsNullOrEmpty(templateName = templateName.Trim()))
            {
                throw new ScriptRuntimeException(Span, $"Include template name cannot be null or empty");
            }

            var templateLoader = context.TemplateLoader;
            if (templateLoader == null)
            {
                throw new ScriptRuntimeException(Span, $"Unable to include <{templateName}>. No TemplateLoader registered in TemplateContext.TemplateLoader");
            }

            var templatePath = templateLoader.GetPath(context, Span, templateName);
            // If template name is empty, throw an exception
            if (templatePath == null || string.IsNullOrEmpty(templatePath = templatePath.Trim()))
            {
                throw new ScriptRuntimeException(Span, $"Include template path cannot be null or empty");
            }

            // Compute a new parameters for the include
            var newParameters = new ScriptArray(Arguments.Count - 1);
            for (int i = 1; i < Arguments.Count; i++)
            {
                newParameters[i] = Arguments[i];
            }

            context.SetValue(ScriptVariable.Arguments, newParameters, true);

            Template template;

            if (!context.CachedTemplates.TryGetValue(templatePath, out template))
            {

                var templateText = templateLoader.Load(context, Span, templatePath);

                if (templateText == null)
                {
                    throw new ScriptRuntimeException(Span, $"The result of including `{templateName}->{templatePath}` cannot be null");
                }

                // Clone parser options
                var parserOptions = context.TemplateLoaderParserOptions;

                var lexerOptions = context.TemplateLoaderLexerOptions;
                // Parse include in default modes (while top page can be using front matter)
                lexerOptions.Mode = lexerOptions.Mode == ScriptMode.ScriptOnly
                    ? ScriptMode.ScriptOnly
                    : ScriptMode.Default;

                template = Template.Parse(templateText, templatePath, parserOptions, lexerOptions);

                // If the template has any errors, throw an exception
                if (template.HasErrors)
                {
                    throw new ScriptParserRuntimeException(Span, $"Error while parsing template `{templateName}` from `{templatePath}`", template.Messages);
                }

                context.CachedTemplates.Add(templatePath, template);
            }

            // Query the pending includes stored in the context
            HashSet<string> pendingIncludes;
            object pendingIncludesObject;
            if (!context.Tags.TryGetValue(typeof(ScriptIncludeExpression), out pendingIncludesObject))
            {
                pendingIncludesObject = pendingIncludes = new HashSet<string>();
                context.Tags[typeof(ScriptIncludeExpression)] = pendingIncludesObject;
            }
            else
            {
                pendingIncludes = (HashSet<string>)pendingIncludesObject;
            }

            // Make sure that we cannot recursively include a template
            if (pendingIncludes.Contains(templateName))
            {
                throw new ScriptRuntimeException(Span, $"The include `{templateName}` cannot be used recursively");
            }
            pendingIncludes.Add(templateName);

            context.PushOutput();
            object result = null;
            try
            {
                result = template.Render(context);
            }
            finally
            {
                context.PopOutput();
                pendingIncludes.Remove(templateName);
            }

            return result;
        }

        public override void Write(RenderContext context)
        {
            
        }
    }
}