// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Diagnostics;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    /// <summary>
    /// Options used by <see cref="ScriptFormatter"/>
    /// </summary>
    [DebuggerDisplay("Lang: {Language} Flags: {Flags}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    readonly struct ScriptFormatterOptions
    {
        public ScriptFormatterOptions(ScriptFormatterFlags flags)
        {
            Language = ScriptLang.Default;
            Flags = flags;
            Context = null;
        }

        public ScriptFormatterOptions(TemplateContext context, ScriptLang language, ScriptFormatterFlags flags)
        {
            Language = language;
            Flags = flags;
            Context = context == null && language == ScriptLang.Scientific ? throw new ArgumentNullException(nameof(context), "Context cannot be null with scientific language.") : context;
        }

        /// <summary>
        /// Gets or sets the input/output language to format (Only <see cref="ScriptLang.Default"/> and <see cref="ScriptLang.Scientific"/> are supported)
        /// </summary>
        public readonly ScriptLang Language;

        /// <summary>
        /// Flags used for formatting.
        /// </summary>
        public readonly ScriptFormatterFlags Flags;

        /// <summary>
        /// Defines the context used for formatting scientific scripts.
        /// </summary>
        public readonly TemplateContext Context;
    }
}