// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract partial class ScriptVisitor
    {
        public virtual void Visit(ScriptNode node)
        {
            if (node == null)
                return;

            node.Accept(this);
        }

        public virtual void Visit(ScriptList list)
        {
            if (list == null) return;
            var count = list.ChildrenCount;
            for (int i = 0; i < count; i++)
            {
                var child = list[i];
                Visit(child);
            }
        }

        protected virtual void DefaultVisit(ScriptNode node)
        {
            if (node == null)
                return;

            var childrenCount = node.ChildrenCount;
            for(int i = 0; i < childrenCount; i++)
            {
                var child = node.GetChildren(i);
                Visit(child);
            }
        }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract partial class ScriptVisitor<TResult>
    {
        public virtual TResult Visit(ScriptNode node)
        {
            if (node == null)
                return default;

            return node.Accept(this);
        }

        protected virtual TResult DefaultVisit(ScriptNode node)
        {
            if (node == null)
                return default;

            var childrenCount = node.ChildrenCount;
            for (int i = 0; i < childrenCount; i++)
            {
                var child = node.GetChildren(i);
                Visit(child);
            }

            return default;
        }
    }
}
