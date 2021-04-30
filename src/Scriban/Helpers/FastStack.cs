// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Scriban.Helpers
{
    /// <summary>
    /// Lightweight stack object.
    /// </summary>
    /// <typeparam name="T">Type of the object</typeparam>
    internal struct FastStack<T>
    {
        private T[] _array; // Storage for stack elements.
        private int _size; // Number of items in the stack.
        private const int DefaultCapacity = 4;

        // Create a stack with a specific initial capacity.  The initial capacity
        // must be a non-negative number.
        public FastStack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be > 0");
            _array = new T[capacity];
            _size = 0;
        }

        public int Count => _size;

        public T[] Items => _array;

        // Removes all Objects from the Stack.
        public void Clear()
        {
            // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
            Array.Clear(_array, 0, _size);
            _size = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            if (_size == 0)
            {
                ThrowForEmptyStack();
            }

            return _array[_size - 1];
        }

        // Pops an item from the top of the stack. If the stack is empty, Pop
        // throws an InvalidOperationException.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            if (_size == 0)
            {
                ThrowForEmptyStack();
            }

            T item = _array[--_size];
            _array[_size] = default(T);     // Free memory quicker.
            return item;
        }

        // Pushes an item to the top of the stack.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item)
        {
            if (_size == _array.Length)
            {
                Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
            }
            _array[_size++] = item;
        }

        private void ThrowForEmptyStack()
        {
            Debug.Assert(_size == 0);
            throw new InvalidOperationException("Stack is empty");
        }
    }
}