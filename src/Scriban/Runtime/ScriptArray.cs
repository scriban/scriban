// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

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

        private readonly List<T> values;

        // Attached ScriptObject is only created if needed
        private ScriptObject script;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        public ScriptArray()
        {
            values = new List<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ScriptArray(int capacity)
        {
            values = new List<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray{T}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ScriptArray(IEnumerable<T> values)
        {
            this.values = new List<T>(values);
        }

        public ScriptObject ScriptObject => script ?? (script = new ScriptObject());

        public int Count => values.Count;

        public T this[int index]
        {
            get { return index < 0 || index >= values.Count ? null : values[index]; }
            set
            {
                if (index < 0)
                {
                    return;
                }

                // Auto-expand the array in case of accessing a range outside the current value
                for (int i = values.Count; i <= index; i++)
                {
                    values.Add(null);
                }

                values[index] = value;
            }
        }

        public void Add(T item)
        {
            values.Add(item);
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
            return ((IList)values).Add(value);
        }

        bool IList.Contains(object value)
        {
            return ((IList) values).Contains(value);
        }

        public void Clear()
        {
            values.Clear();
        }

        int IList.IndexOf(object value)
        {
            return ((IList)values).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)values).Insert(index, value);
        }

        public bool Contains(T item)
        {
            return values.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return values.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            // Auto-expand the array in case of accessing a range outside the current value
            for (int i = values.Count; i < index; i++)
            {
                values.Add(null);
            }

            values.Insert(index, item);
        }

        void IList.Remove(object value)
        {
            ((IList)values).Remove(value);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= values.Count)
            {
                return;
            }
            values.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return ((IList) values)[index]; }
            set { ((IList) values)[index] = value; }
        }

        public bool Remove(T item)
        {
            return values.Remove(item);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return values.GetEnumerator();
        }

        bool IList.IsFixedSize => ((IList)values).IsFixedSize;

        bool ICollection.IsSynchronized => ((ICollection)values).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)values).SyncRoot;

        bool IList.IsReadOnly => ((IList)values).IsReadOnly;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)values).CopyTo(array, index);
        }

        public bool Contains(string member)
        {
            return ScriptObject.Contains(member);
        }

        public bool TryGetValue(string member, out object value)
        {
            return ScriptObject.TryGetValue(member, out value);
        }

        object IScriptObject.this[string key]
        {
            get { return ScriptObject[key]; }
            set { ScriptObject[key] = value; }
        }

        public bool IsReadOnly(string member)
        {
            return ScriptObject.IsReadOnly(member);
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