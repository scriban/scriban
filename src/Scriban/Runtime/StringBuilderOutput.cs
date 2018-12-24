// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Text;
using System.Threading;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif

namespace Scriban.Runtime
{
    /// <summary>
    /// Output to a <see cref="StringBuilder"/>
    /// </summary>
    public class StringBuilderOutput : IScriptOutput
    {
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

        public IScriptOutput Write(string text, int offset, int count)
        {
            Builder.Append(text, offset, count);
            return this;
        }

#if SCRIBAN_ASYNC
        public async Task<IScriptOutput> WriteAsync(string text, int offset, int count, CancellationToken cancellationToken)
        {
            Builder.Append(text, offset, count);
            return this;
        }
#endif
        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}