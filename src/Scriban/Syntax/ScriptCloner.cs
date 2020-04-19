// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    internal sealed class ScriptCloner : ScriptRewriter
    {
        public static readonly ScriptCloner Instance = new ScriptCloner() { CopyTrivias = false };

        public static readonly ScriptCloner WithTrivias = new ScriptCloner() { CopyTrivias = true };
    }
}