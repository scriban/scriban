// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    public static class ScriptParameterContainerExtensions
    {
        public static void AddParameter(this IScriptNamedParameterContainer container, ScriptNamedParameter parameter)
        {
            if (container.NamedParameters == null)
            {
                container.NamedParameters = new List<ScriptNamedParameter>();
            }
            container.NamedParameters.Add(parameter);
        }


        public static void Write(this List<ScriptNamedParameter> parameters, RenderContext context)
        {
            if (parameters == null)
            {
                return;
            }
            for (var i = 0; i < parameters.Count; i++)
            {
                var option = parameters[i];
                context.Write(",");
                context.Write(option);
            }
        }
    }
}