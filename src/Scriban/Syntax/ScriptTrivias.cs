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
    class ScriptTrivias
    {
        public ScriptTrivias()
        {
            Before = new List<ScriptTrivia>();
            After = new List<ScriptTrivia>();
        }

        public List<ScriptTrivia> Before { get; }

        public List<ScriptTrivia> After { get; }
    }
}