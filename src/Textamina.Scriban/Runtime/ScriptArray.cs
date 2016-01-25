// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Textamina.Scriban.Runtime
{

    /// <summary>
    /// Base runtime object for arrays.
    /// </summary>
    /// <seealso cref="object" />
    /// <seealso cref="System.Collections.IList" />
    public sealed class ScriptArray : IList<object>, IList
    {
        private readonly List<object> values;

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
    }
}