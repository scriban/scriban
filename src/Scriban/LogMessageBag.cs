// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Scriban.Parsing;

namespace Scriban
{
    /// <summary>
    /// Contains log messages.
    /// </summary>
    [DebuggerDisplay("Count: {Count}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class LogMessageBag : IReadOnlyList<LogMessage>
    {
        private readonly List<LogMessage> _messages;

        public LogMessageBag()
        {
            _messages = new List<LogMessage>();
        }

        public int Count => _messages.Count;

        public LogMessage this[int index] => _messages[index];

        public bool HasErrors { get; private set; }

        public void Add(LogMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (message.Type == ParserMessageType.Error)
            {
                HasErrors = true;
            }

            _messages.Add(message);
        }

        public void AddRange(IEnumerable<LogMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            foreach (var message in messages)
            {
                Add(message);
            }
        }

        public IEnumerator<LogMessage> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _messages).GetEnumerator();
        }


        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var message in _messages)
            {
                builder.AppendLine(message.ToString());
            }
            return builder.ToString();
        }
    }
}