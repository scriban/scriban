// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Parsing
{
    /// <summary>
    /// Defines the language the parser should use.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptLang
    {
        /// <summary>
        /// Default scriban language.
        /// </summary>
        Default,

        /// <summary>
        /// Liquid language.
        /// </summary>
        Liquid,

        /// <summary>
        /// Scientific language (similar to <see cref="Default"/>, but with different parsing rules).
        /// </summary>
        Scientific,
    }
}