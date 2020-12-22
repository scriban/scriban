// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace Scriban.Syntax
{
    [Flags]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptFormatterFlags
    {
        /// <summary>
        /// No flags defined.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use parenthesis to disambiguate and make more explicit expressions (e.g 1/5+2 => (1/5)+2)
        /// </summary>
        ExplicitParenthesis = 1 << 0,

        /// <summary>
        /// Add a space between both sides of the operators (e.g 1+5 => 1 + 5)
        /// </summary>
        AddSpaceBetweenOperators = 1 << 1,

        /// <summary>
        /// Remove the existing trivias (comments, newlines, spaces...).
        /// </summary>
        RemoveExistingTrivias = 1 << 2,

        /// <summary>
        /// Compress consecutive spaces to 1 space. Remove leading and trailing spaces.
        /// </summary>
        CompressSpaces = 1 << 3,

        /// <summary>
        /// Minimize parenthesis nesting (e.g ((1*5) + 2) => (1*5) + 2)
        /// </summary>
        MinimizeParenthesisNesting = 1 << 4,

        /// <summary>
        /// Add spaces for binary, compress spaces and minimize parenthesis nesting.
        /// </summary>
        Clean = AddSpaceBetweenOperators | CompressSpaces | MinimizeParenthesisNesting,

        /// <summary>
        /// Clean mode + add explicit parenthesis to disambiguate operators/function calls.
        /// </summary>
        ExplicitClean = ExplicitParenthesis | Clean,
    }
}