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
    public abstract class ScriptList : ScriptNode
    {
        protected readonly List<ScriptNode> _children;

        internal ScriptList()
        {
            _children = new List<ScriptNode>();
        }

        public int Count => ChildrenCount;

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
    public sealed class ScriptList<TScriptNode> : ScriptList, IList<TScriptNode> where TScriptNode : ScriptNode
    {
        /// <summary>
        /// Creates an instance of <see cref="ScriptList{TScriptNode}"/>
        /// </summary>
        public ScriptList()
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

        public void AddRange(IEnumerable<TScriptNode> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            foreach (var node in nodes)
            {
                Add(node);
            }
        }

        public void Clear()
        {
            foreach (var item in _children)
            {
                if (item != null)
                {
                    item.Parent = null;
                }
            }
            _children.Clear();
        }

        public bool Contains(TScriptNode item)
        {
            return _children.Contains(item);
        }

        public void CopyTo(TScriptNode[] array, int arrayIndex)
        {
            _children.CopyTo((ScriptNode[])array, arrayIndex);
        }

        public bool Remove(TScriptNode item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (_children.Remove(item))
            {
                item.Parent = null;
                return true;
            }

            return false;
        }

        public bool IsReadOnly => false;

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
        /// Enumerator of a <see cref="ScriptList{TScriptNode}"/>
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

        public int IndexOf(TScriptNode item)
        {
            return _children.IndexOf(item);
        }

        public void Insert(int index, TScriptNode item)
        {
            AssertNoParent(item);
            _children.Insert(index, item);
            if (item != null)
            {
                item.Parent = this;
            }
        }

        public void RemoveAt(int index)
        {
            var previous = _children[index];
            _children.RemoveAt(index);
            if (previous != null) previous.Parent = null;
        }

        public TScriptNode this[int index]
        {
            get => (TScriptNode)_children[index];
            set
            {
                var previous = _children[index];
                if (previous == value) return;
                AssertNoParent(value);
                _children[index] = value;
                if (previous != null) previous.Parent = null;
            }
        }

        private void AssertNoParent(ScriptNode node)
        {
            if (node != null && node.Parent != null) throw new ArgumentException("Cannot add this node which is already attached to another list instance");
        }
    }
}