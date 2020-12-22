// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Scriban.Parsing;

namespace Scriban.Syntax
{
    /// <summary>
    /// Identifies a script terminal (token, identifier, variable, literal)
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptTerminal
    {
        /// <summary>
        /// Trivias, can be null if <see cref="LexerOptions.KeepTrivia"/> is <c>false</c>
        /// </summary>
        ScriptTrivias Trivias { get; set; }
    }
}