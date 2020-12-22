// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptFormatterExtensions
    {
        public static ScriptNode Format(this ScriptNode node, ScriptFormatterOptions options)
        {
            var formatter = new ScriptFormatter(options);
            var newNode = formatter.Format(node);
            return newNode;
        }

        public static bool HasFlags(this ScriptFormatterFlags input, ScriptFormatterFlags flags) => (input & flags) == flags;
    }
}