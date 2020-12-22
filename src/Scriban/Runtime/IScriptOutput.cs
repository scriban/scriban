// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// Interface used to text output when evaluating a template used by <see cref="TemplateContext.Output"/> and <see cref="TemplateContext.PushOutput()"/>
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptOutput
    {
        void Write(string text, int offset, int count);

#if !SCRIBAN_NO_ASYNC
        ValueTask WriteAsync(string text, int offset, int count, CancellationToken cancellationToken);
#endif
    }

    /// <summary>
    /// Extensions for <see cref="IScriptOutput"/>
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static partial class ScriptOutputExtensions
    {
        public static void Write(this IScriptOutput scriptOutput, string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            scriptOutput.Write(text, 0, text.Length);
        }

        public static void Write(this IScriptOutput scriptOutput, ScriptStringSlice text)
        {
            if (text.FullText == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) return;
            scriptOutput.Write(text.FullText, text.Index, text.Length);
        }
    }
}