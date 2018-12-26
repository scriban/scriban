// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.IO;
using System.Threading;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif

namespace Scriban.Runtime
{
    /// <summary>
    /// Output to a <see cref="TextWriter"/>
    /// </summary>
    public class TextWriterOutput : IScriptOutput
    {
        /// <summary>
        /// Initialize a new instance of <see cref="TextWriterOutput"/> with a writer default to <see cref="StringWriter"/>
        /// </summary>
        public TextWriterOutput() : this(new StringWriter())
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="TextWriterOutput"/> with the specified <see cref="TextWriter"/>
        /// </summary>
        /// <param name="writer">An existing <see cref="TextWriter"/></param>
        public TextWriterOutput(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <summary>
        /// The underlying <see cref="TextWriter"/>
        /// </summary>
        public TextWriter Writer { get; }

        public IScriptOutput Write(string text, int offset, int count)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Writer.Write(text.Substring(offset, count));
            return this;
        }
#if SCRIBAN_ASYNC
        public async ValueTask<IScriptOutput> WriteAsync(string text, int offset, int count, CancellationToken cancellationToken)
        {
            // TextWriter doesn't support to pass CancellationToken oO
            await Writer.WriteAsync(text.Substring(offset, count));
            return this;
        }
#endif

        public override string ToString()
        {
            return Writer.ToString();
        }
    }
}