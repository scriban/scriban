#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Scriban.Helpers
{
    [DebuggerTypeProxy(typeof(InlineList<>.DebugListView)), DebuggerDisplay("Count = {Count}")]
    internal struct InlineList<T> : IEnumerable<T>
    {
        private const int DefaultCapacity = 4;

        public int Count;

        public T[] Items;

        public InlineList(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            Count = 0;
            Items = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        public int Capacity
        {
            get => Items?.Length ?? 0;
            set
            {
                Ensure();
                if (value <= Items.Length) return;
                EnsureCapacity(value);
            }
        }

        public static InlineList<T> Create()
        {
            return new InlineList<T>(0);
        }

        public static InlineList<T> Create(int capacity)
        {
            return new InlineList<T>(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure()
        {
            if (Items == null) Items = Array.Empty<T>();
        }

        public bool IsReadOnly => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (Count > 0)
            {
                Array.Clear(Items, 0, Count);
                Count = 0;
            }
        }

        public InlineList<T> Clone()
        {
            var items = (T[])Items?.Clone();
            return new InlineList<T>() { Count = Count, Items = items };
        }

        public bool Contains(T item)
        {
            return Count > 0 && IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (Count > 0)
            {
                System.Array.Copy(Items, 0, array, arrayIndex, Count);
            }
        }

        public T[] ToArray()
        {
            var array = new T[Count];
            CopyTo(array, 0);
            return array;
        }

        public void Reset()
        {
            Clear();
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T child)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            Items[Count++] = child;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddByRef(in T child)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            Items[Count++] = child;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(int index, T item)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            if (index < Count)
            {
                Array.Copy(Items, index, Items, index + 1, Count - index);
            }
            Items[index] = item;
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T InsertReturnRef(int index, T item)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            if (index < Count)
            {
                Array.Copy(Items, index, Items, index + 1, Count - index);
            }

            ref var refItem = ref Items[index];
            refItem = item;
            Count++;
            return ref refItem;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void InsertByRef(int index, in T item)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            if (index < Count)
            {
                Array.Copy(Items, index, Items, index + 1, Count - index);
            }
            Items[index] = item;
            Count++;
        }

        public bool Remove(T element)
        {
            var index = IndexOf(element);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public int IndexOf(T element)
        {
            return Array.IndexOf(Items, element, 0, Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T RemoveAt(int index)
        {
            if (index < 0 || index >= Count) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
            Count--;
            // previous children
            var item = Items[index];
            if (index < Count)
            {
                Array.Copy(Items, index + 1, Items, index, Count - index);
            }
            Items[Count] = default(T);
            return item;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Count) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
                return Items[index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                if ((uint)index >= (uint)Count) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
                Items[index] = value;
            }
        }

        private void EnsureCapacity(int min)
        {
            if (Items.Length < min)
            {
                int num = (Items.Length == 0) ? DefaultCapacity : (Items.Length * 2);
                if (num < min)
                {
                    num = min;
                }
                Array.Resize(ref Items, num);
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly InlineList<T> list;
            private int index;
            private T current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(InlineList<T> list)
            {
                this.list = list;
                index = 0;
                current = default(T);
            }

            public T Current => current;

            object IEnumerator.Current => Current;


            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < list.Count)
                {
                    current = list[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.Count + 1;
                current = default(T);
                return false;
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }

        internal class DebugListView
        {
            private InlineList<T> _collection;

            public DebugListView(InlineList<T> collection)
            {
                this._collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items
            {
                get
                {
                    var array = new T[this._collection.Count];
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = _collection[i];
                    }
                    return array;
                }
            }
        }
    }
}