// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
#if SCRIBAN_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Scriban.Runtime
{
    /// <summary>
    /// Interface used to text output when evaluating a template used by <see cref="TemplateContext.Output"/> and <see cref="TemplateContext.PushOutput()"/>
    /// </summary>
    public interface IScriptOutput
    {
        IScriptOutput Write(string text, int offset, int count);

#if SCRIBAN_ASYNC
        ValueTask<IScriptOutput> WriteAsync(string text, int offset, int count, CancellationToken cancellationToken);
#endif
    }

    /// <summary>
    /// Extensions for <see cref="IScriptOutput"/>
    /// </summary>
    public static partial class ScriptOutputExtensions
    {
        public static IScriptOutput Write(this IScriptOutput scriptOutput, string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return scriptOutput.Write(text, 0, text.Length);
        }
    }
}