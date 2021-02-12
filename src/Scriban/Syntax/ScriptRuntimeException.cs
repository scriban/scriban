// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Threading;
using Scriban.Helpers;
using Scriban.Parsing;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptRuntimeException : Exception
    {
        public ScriptRuntimeException(SourceSpan span, string message) : base(message)
        {
            Span = span;
        }

        public ScriptRuntimeException(SourceSpan span, string message, Exception innerException) : base(message, innerException)
        {
            Span = span;
        }

        public SourceSpan Span { get; }

        public override string Message
        {
            get
            {
                return new LogMessage(ParserMessageType.Error, Span, base.Message).ToString();
            }
        }
        public static bool EnableDisplayInnerException
        {
            get;
            set;
        }

        /// <summary>
        /// Provides the exception message without the source span prefix.
        /// </summary>
        public string OriginalMessage
        {
            get
            {
                return base.Message;
            }
        }

        public override string ToString()
        { 
            if (ScriptRuntimeException.EnableDisplayInnerException && InnerException != null)
            {
                return base.ToString();
            }

            return Message;
        }
    }
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptAbortException : ScriptRuntimeException
    {
        public ScriptAbortException(SourceSpan span, CancellationToken cancellationToken) : this(span, "The operation was cancelled", cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public ScriptAbortException(SourceSpan span, string message, CancellationToken cancellationToken) : base(span, message)
        {
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptParserRuntimeException : ScriptRuntimeException
    {
        public ScriptParserRuntimeException(SourceSpan span, string message, LogMessageBag parserMessages) : this(span, message, parserMessages, null)
        {
        }

        public ScriptParserRuntimeException(SourceSpan span, string message, LogMessageBag parserMessages, Exception innerException) : base(span, message, innerException)
        {
            if (parserMessages == null) throw new ArgumentNullException(nameof(parserMessages));
            ParserMessages = parserMessages;
        }

        public LogMessageBag ParserMessages { get; }

        public override string Message
        {
            get
            {
                return $"{base.Message} Parser messages:\n {ParserMessages}";
            }
        }

    }
}