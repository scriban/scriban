// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static partial class ScriptParameterContainerExtensions
    {
        public static void AddParameter(this IScriptNamedArgumentContainer container, ScriptNamedArgument argument)
        {
            if (container.NamedArguments == null)
            {
                container.NamedArguments = new ScriptList<ScriptNamedArgument>();
            }
            container.NamedArguments.Add(argument);
        }

        public static void Write(this ScriptPrinter printer, List<ScriptNamedArgument> parameters)
        {
            if (parameters == null)
            {
                return;
            }
            for (var i = 0; i < parameters.Count; i++)
            {
                var option = parameters[i];
                printer.ExpectSpace();
                printer.Write(option);
            }
        }
    }
}