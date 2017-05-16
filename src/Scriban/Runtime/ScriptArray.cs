// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using Scriban.Model;

namespace Scriban.Runtime
{
    /// <summary>
    /// Base runtime object for arrays.
    /// </summary>
    /// <seealso cref="object" />
    /// <seealso cref="System.Collections.IList" />
    public class ScriptArray<T> : IList<T>, IList, IScriptObject where T : class
    {
        internal static readonly IScriptCustomType CustomOperator = new ListCustomOperator();

        private readonly List<T> _values;
        private bool _isReadOnly;

        // Attached ScriptObject is only created if needed
        private ScriptObject _script;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        public ScriptArray()
        {
            _values = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ScriptArray(int capacity)
        {
            _values = new List<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ScriptArray(IEnumerable<T> values)
        {
            this._values = new List<T>(values);
        }

        public ScriptObject ScriptObject => _script ?? (_script = new ScriptObject() { IsReadOnly = IsReadOnly});

        public int Count => _values.Count;

        public T this[int index]
        {
            get => index < 0 || index >= _values.Count ? null : _values[index];
            set
            {
                if (index < 0)
                {
                    return;
                }

                this.AssertNotReadOnly();

                // Auto-expand the array in case of accessing a range outside the current value
                for (int i = _values.Count; i <= index; i++)
                {
                    _values.Add(null);
                }

                _values[index] = value;
            }
        }

        public void Add(T item)
        {
            this.AssertNotReadOnly();
            _values.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
            {
                Add(item);
            }
        }

        int IList.Add(object value)
        {
            this.AssertNotReadOnly();
            return ((IList)_values).Add(value);
        }

        bool IList.Contains(object value)
        {
            return ((IList) _values).Contains(value);
        }

        public void Clear()
        {
            this.AssertNotReadOnly();
            _values.Clear();
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_values).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            this.AssertNotReadOnly();
            ((IList)_values).Insert(index, value);
        }

        public bool Contains(T item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return _values.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.AssertNotReadOnly();
            // Auto-expand the array in case of accessing a range outside the current value
            for (int i = _values.Count; i < index; i++)
            {
                _values.Add(null);
            }

            _values.Insert(index, item);
        }

        void IList.Remove(object value)
        {
            this.AssertNotReadOnly();
            ((IList)_values).Remove(value);
        }

        public void RemoveAt(int index)
        {
            this.AssertNotReadOnly();
            if (index < 0 || index >= _values.Count)
            {
                return;
            }
            _values.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get => ((IList) _values)[index];
            set
            {
                this.AssertNotReadOnly();
                ((IList) _values)[index] = value;
            }
        }

        public bool Remove(T item)
        {
            this.AssertNotReadOnly();
            return _values.Remove(item);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        bool IList.IsFixedSize => ((IList)_values).IsFixedSize;

        bool ICollection.IsSynchronized => ((ICollection)_values).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)_values).SyncRoot;

        bool IList.IsReadOnly => ((IList)_values).IsReadOnly;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_values).CopyTo(array, index);
        }

        public bool Contains(string member)
        {
            return ScriptObject.Contains(member);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                if (_script != null)
                {
                    _script.IsReadOnly = value;
                }
                _isReadOnly = value;
            }
        }

        public bool TryGetValue(string member, out object value)
        {
            return ScriptObject.TryGetValue(member, out value);
        }

        object IScriptObject.this[string key]
        {
            get => ScriptObject[key];
            set => ScriptObject[key] = value;
        }

        public bool CanWrite(string member)
        {
            return ScriptObject.CanWrite(member);
        }

        public void SetValue(string member, object value, bool readOnly)
        {
            ScriptObject.SetValue(member, value, readOnly);
        }

        public bool Remove(string member)
        {
            return ScriptObject.Remove(member);
        }

        public void SetReadOnly(string member, bool readOnly)
        {
            ScriptObject.SetReadOnly(member, readOnly);
        }

        private class ListCustomOperator : IScriptCustomType
        {
            bool IScriptCustomType.TryConvertTo(Type destinationType, out object value)
            {
                value = null;
                return false;
            }

            object IScriptCustomType.EvaluateUnaryExpression(ScriptUnaryExpression expression)
            {
                throw new ScriptRuntimeException(expression.Span,
                    $"Operator [{expression.Operator}] is not supported for an array");
            }

            object IScriptCustomType.EvaluateBinaryExpression(ScriptBinaryExpression expression, object left,
                object right)
            {
                var listLeft = left as IList;
                var listRight = right as IList;
                if (expression.Operator == ScriptBinaryOperator.ShiftLeft && listLeft != null)
                {
                    listLeft.Add(right);
                }
                else if (expression.Operator == ScriptBinaryOperator.ShiftRight && listRight != null)
                {
                    listRight.Insert(0, left);
                }
                else
                {
                    throw new ScriptRuntimeException(expression.Span,
                        $"Operator [{expression.Operator}] is not supported for an array");
                }

                return listLeft ?? listRight;
            }
        }
    }

    /// <summary>
    /// Base runtime object for arrays.
    /// </summary>
    /// <seealso cref="object" />
    /// <seealso cref="System.Collections.IList" />
    public class ScriptArray : ScriptArray<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        public ScriptArray()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ScriptArray(int capacity) : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ScriptArray(IEnumerable<object> values) : base(values)
        {
        }
    }
}