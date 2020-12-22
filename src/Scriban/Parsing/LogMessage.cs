// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Text;

namespace Scriban.Parsing
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class LogMessage
    {
        public LogMessage(ParserMessageType type, SourceSpan span, string message)
        {
            Type = type;
            Span = span;
            Message = message;
        }

        public ParserMessageType Type { get; set; }

        public SourceSpan Span { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Span.ToStringSimple());
            builder.Append(" : ");
            builder.Append(Type.ToString().ToLowerInvariant());
            builder.Append(" : ");
            if (Message != null)
            {
                builder.Append(Message);
            }
            return builder.ToString();
        }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ParserMessageType
    {
        Error,

        Warning,
    }
}