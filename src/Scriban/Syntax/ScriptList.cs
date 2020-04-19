// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using Scriban.Helpers;

namespace Scriban.Syntax
{
    /// <summary>
    /// Abstract list of <see cref="ScriptNode"/>
    /// </summary>
    public abstract class SyntaxList : ScriptNode
    {
        protected readonly List<ScriptNode> _children;

        internal SyntaxList()
        {
            _children = new List<ScriptNode>();
        }

        public sealed override int ChildrenCount => _children.Count;

        protected override ScriptNode GetChildrenImpl(int index)
        {
            return _children[index];
        }
    }

    /// <summary>
    /// Abstract list of <see cref="ScriptNode"/>
    /// </summary>
    /// <typeparam name="TScriptNode">Type of the node</typeparam>
    public sealed class SyntaxList<TScriptNode> : SyntaxList, IEnumerable<TScriptNode> where TScriptNode : ScriptNode
    {
        /// <summary>
        /// Creates an instance of <see cref="SyntaxList{TScriptNode}"/>
        /// </summary>
        public SyntaxList()
        {
        }

        /// <summary>
        /// Adds the specified node to this list.
        /// </summary>
        /// <param name="node">Node to add to this list</param>
        public void Add(TScriptNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Parent != null) throw ThrowHelper.GetExpectingNoParentException();
            _children.Add(node);
            node.Parent = this;
        }

        public override object Evaluate(TemplateContext context)
        {
            throw new InvalidOperationException("A list cannot be evaluated.");
        }

        public new TScriptNode GetChildren(int index)
        {
            return (TScriptNode)base.GetChildren(index);
        }

        protected override ScriptNode GetChildrenImpl(int index)
        {
            return _children[index];
        }

        public override void Write(TemplateRewriterContext context)
        {
            throw new NotImplementedException();
        }

        public override void Accept(ScriptVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    
        /// <summary>
        /// Removes a node at the specified index.
        /// </summary>
        /// <param name="index">Index of the node to remove</param>
        public void RemoveChildrenAt(int index)
        {
            var node = _children[index];
            _children.RemoveAt(index);
            node.Parent = null;
        }

        /// <summary>
        /// Removes the specified node instance.
        /// </summary>
        /// <param name="node">Node instance to remove</param>
        public void RemoveChildren(TScriptNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Parent != this) throw new InvalidOperationException("The node is not part of this list");
            _children.Remove(node);
            node.Parent = null;
        }

        /// <summary>
        /// Gets the default enumerator.
        /// </summary>
        /// <returns>The enumerator of this list</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_children);
        }

        IEnumerator<TScriptNode> IEnumerable<TScriptNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Enumerator of a <see cref="SyntaxList{TScriptNode}"/>
        /// </summary>
        public struct Enumerator : IEnumerator<TScriptNode>
        {
            private readonly List<ScriptNode> _nodes;
            private int _index;

            /// <summary>
            /// Initialize an enumerator with a list of <see cref="ScriptNode"/>
            /// </summary>
            /// <param name="nodes"></param>
            public Enumerator(List<ScriptNode> nodes)
            {
                _nodes = nodes;
                _index = -1;
            }

            public bool MoveNext()
            {
                if (_index + 1 == _nodes.Count) return false;
                _index++;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }

            public TScriptNode Current
            {
                get
                {
                    if (_index < 0) throw new InvalidOperationException("MoveNext must be called before accessing Current");
                    return (TScriptNode)_nodes[_index];
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}