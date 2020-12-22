// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    /// <summary>
    /// Whitespace mode handling for code/escape enter/exit.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptWhitespaceMode
    {
        /// <summary>
        /// No change in whitespaces.
        /// </summary>
        None,

        /// <summary>
        /// The greedy mode using the character - (e.g {{- or -}}), removes any whitespace, including newlines
        /// </summary>
        Greedy,

        /// <summary>
        /// The non greedy mode using the character ~.
        ///
        /// - Using a {{~ will remove any whitespace before but will stop on the first newline without including it
        /// - Using a ~}} will remove any whitespace after including the first newline but will stop after
        /// </summary>
        NonGreedy,
    }
}