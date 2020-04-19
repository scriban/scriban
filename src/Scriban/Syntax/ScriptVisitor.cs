// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Syntax
{
    public abstract partial class ScriptVisitor
    {
        public virtual void Visit(ScriptNode node)
        {
            if (node == null)
                return;

            node.Accept(this);
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

    public abstract partial class ScriptVisitor<TResult>
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
