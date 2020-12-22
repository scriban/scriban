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
    interface IScriptVisitorContext
    {
        ScriptNode Current { get; }
        ScriptNode Parent { get; }
        IEnumerable<ScriptNode> Ancestors { get; }
    }
}