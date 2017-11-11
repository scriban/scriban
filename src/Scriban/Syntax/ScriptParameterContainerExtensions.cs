// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    public static class ScriptParameterContainerExtensions
    {
        public static void AddParameter(this IScriptNamedArgumentContainer container, ScriptNamedArgument argument)
        {
            if (container.NamedArguments == null)
            {
                container.NamedArguments = new List<ScriptNamedArgument>();
            }
            container.NamedArguments.Add(argument);
        }

        public static void Write(this TemplateRewriterContext context, List<ScriptNamedArgument> parameters)
        {
            if (parameters == null)
            {
                return;
            }
            for (var i = 0; i < parameters.Count; i++)
            {
                var option = parameters[i];
                context.ExpectSpace();
                context.Write(option);
            }
        }
    }
}