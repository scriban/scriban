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
    public class ScriptArray : IList<object>, IList, IScriptObject
    {
        internal static readonly IScriptCustomType CustomOperator = new ListCustomOperator();

        private readonly List<object> values;

        // Attached ScriptObject is only created if needed
        private ScriptObject script;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        public ScriptArray()
        {
            values = new List<object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ScriptArray(int capacity)
        {
            values = new List<object>(capacity);
        }

        public ScriptObject ScriptObject => script ?? (script = new ScriptObject());

        public int Count => values.Count;

        bool ICollection.IsSynchronized => ((ICollection)values).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)values).SyncRoot;

        bool IList.IsReadOnly => ((IList)values).IsReadOnly;

        public object this[int index]
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

        public void Add(object item)
        {
            values.Add(item);
        }

        int IList.Add(object value)
        {
            return ((IList) values).Add(value);
        }

        public void Clear()
        {
            values.Clear();
        }

        public bool Contains(object item)
        {
            return values.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        public int IndexOf(object item)
        {
            return values.IndexOf(item);
        }

        public void Insert(int index, object item)
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
            ((IList) values).Remove(value);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= values.Count)
            {
                return;
            }
            values.RemoveAt(index);
        }

        bool IList.IsFixedSize => ((IList)values).IsFixedSize;

        public bool Remove(object item)
        {
            return values.Remove(item);
        }

        public List<object>.Enumerator GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        bool ICollection<object>.IsReadOnly => false;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)values).CopyTo(array, index);
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

        public bool TrySetValue(string member, object value, bool readOnly)
        {
            return ScriptObject.TrySetValue(member, value, readOnly);
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
    }
}