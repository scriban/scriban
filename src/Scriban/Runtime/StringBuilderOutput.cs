// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scriban.Runtime
{
    /// <summary>
    /// Output to a <see cref="StringBuilder"/>
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class StringBuilderOutput : IScriptOutput
    {
        [ThreadStatic] private static StringBuilder TlsBuilder;

        /// <summary>
        /// Initialize a new instance of <see cref="StringBuilderOutput"/>
        /// </summary>
        public StringBuilderOutput() : this(new StringBuilder())
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="StringBuilderOutput"/>
        /// </summary>
        /// <param name="builder">An existing <see cref="StringBuilder"/></param>
        public StringBuilderOutput(StringBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// The underlying <see cref="StringBuilder"/>
        /// </summary>
        public StringBuilder Builder { get; }

        public void Write(string text, int offset, int count)
        {
            Builder.Append(text, offset, count);
        }

        /// <summary>
        /// Returns a thread local instance
        /// </summary>
        /// <returns></returns>
        public static StringBuilderOutput GetThreadInstance()
        {
            if (TlsBuilder == null)
            {
                TlsBuilder = new StringBuilder();
            }
            TlsBuilder.Length = 0;
            return new StringBuilderOutput(TlsBuilder);
        }


#if !SCRIBAN_NO_ASYNC
        public ValueTask WriteAsync(string text, int offset, int count, CancellationToken cancellationToken)
        {
            Builder.Append(text, offset, count);
            return default;
        }
#endif
        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}