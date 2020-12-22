// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    /// <summary>
    /// Interfaces used by statements/expressions that have special trailing parameters (for, tablerow, include...)
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptNamedArgumentContainer
    {
        ScriptList<ScriptNamedArgument> NamedArguments { get; set; }
    }
}