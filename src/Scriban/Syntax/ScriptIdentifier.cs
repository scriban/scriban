// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    /// <summary>
    /// An identifier (which is not a <see cref="ScriptVariable"/>)
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptIdentifier : ScriptVerbatim
    {
        public ScriptIdentifier()
        {
        }

        public ScriptIdentifier(string value) : base(value)
        {
        }
    }
}