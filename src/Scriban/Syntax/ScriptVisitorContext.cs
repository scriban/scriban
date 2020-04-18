// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    internal class ScriptVisitorContext : IScriptVisitorContext
    {
        private readonly Stack<ScriptNode> _ancestors = new Stack<ScriptNode>();

        public ScriptNode Parent => _ancestors.Count > 0 ? _ancestors.Peek() : null;

        public IEnumerable<ScriptNode> Ancestors => _ancestors;

        public ScriptNode Current { get; private set; }

        public IDisposable Push(ScriptNode node)
        {
            if (Current != null)
                _ancestors.Push(Current);
            Current = node;
            return new Popper(this);
        }

        private void Pop()
        {
            Current = _ancestors.Count > 0 ? _ancestors.Pop() : null;
        }

        class Popper : IDisposable
        {
            private readonly ScriptVisitorContext _context;

            public Popper(ScriptVisitorContext context)
            {
                _context = context;
            }

            public void Dispose()
            {
                _context.Pop();
            }
        }
    }
}
