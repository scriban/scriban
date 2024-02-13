// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// Base runtime object for arrays.
    /// </summary>
    /// <seealso cref="object" />
    /// <seealso cref="System.Collections.IList" />
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ScriptArray<>.DebugListView))]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptArray<T> : IList<T>, IList, IScriptObject, IScriptCustomBinaryOperation, IScriptTransformable
    {
        private List<T> _values;
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

        public ScriptArray(T[] array)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            _values = new List<T>(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                _values.Add(array[i]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ScriptArray(IEnumerable<T> values)
        {
            this._values = new List<T>(values);
        }

        public ScriptArray(IEnumerable values)
        {
            this._values = new List<T>();
            foreach (var value in values)
            {
                _values.Add((T)value);
            }
        }

        public int Capacity
        {
            get => _values.Capacity;
            set => _values.Capacity = value;
        }

        public virtual bool IsReadOnly
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

        public virtual IScriptObject Clone(bool deep)
        {
            var array = (ScriptArray<T>) MemberwiseClone();
            array._values = new List<T>(_values.Count);
            array._script = null;
            if (deep)
            {
                foreach (var value in _values)
                {
                    var fromValue = value;
                    if (value is IScriptObject)
                    {
                        var fromObject = (IScriptObject)value;
                        fromValue = (T)fromObject.Clone(true);
                    }
                    array._values.Add(fromValue);
                }

                if (_script != null)
                {
                    array._script = (ScriptObject)_script.Clone(true);
                }
            }
            else
            {
                foreach (var value in _values)
                {
                    array._values.Add(value);
                }

                if (_script != null)
                {
                    array._script = (ScriptObject)_script.Clone(false);
                }
            }
            return array;
        }

        public ScriptObject ScriptObject => _script ?? (_script = new ScriptObject() { IsReadOnly = IsReadOnly});

        public int Count => _values.Count;

        public virtual T this[int index]
        {
            get => index < 0 || index >= _values.Count ? default : _values[index];
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
                    _values.Add(default);
                }

                _values[index] = value;
            }
        }

        public virtual void Add(T item)
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
            Add((T) value);
            return 0;
        }

        bool IList.Contains(object value)
        {
            return ((IList) _values).Contains(value);
        }

        public virtual void Clear()
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
            Insert(index, (T)value);
        }

        public virtual bool Contains(T item)
        {
            return _values.Contains(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            _values.CopyTo(index, array, arrayIndex, count);
        }

        public virtual int IndexOf(T item)
        {
            return _values.IndexOf(item);
        }

        public virtual void Insert(int index, T item)
        {
            this.AssertNotReadOnly();
            // Auto-expand the array in case of accessing a range outside the current value
            for (int i = _values.Count; i < index; i++)
            {
                _values.Add(default);
            }

            _values.Insert(index, item);
        }

        void IList.Remove(object value)
        {
            Remove((T) value);
        }

        public virtual void RemoveAt(int index)
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
            get => this[index];
            set
            {
                if (typeof(T) == typeof(object))
                {
                    this[index] = (T)value;
                }
                else
                {
                    if (value is T tValue)
                    {
                        this[index] = tValue;
                    }
                    else
                    {
                        this[index] = (T)Convert.ChangeType(value, typeof(T));
                    }
                }
            }
        }

        public virtual bool Remove(T item)
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

        bool IList.IsReadOnly => IsReadOnly;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        bool ICollection<T>.IsReadOnly => IsReadOnly;

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_values).CopyTo(array, index);
        }

        public IEnumerable<string> GetMembers()
        {
            yield return "size";
            if (_script != null)
            {
                foreach (var member in _script.GetMembers())
                {
                    yield return member;
                }
            }
        }

        public virtual bool Contains(string member)
        {
            return ScriptObject.Contains(member);
        }

        public virtual bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            if (member == "size")
            {
                value = Count;
                return true;
            }

            return ScriptObject.TryGetValue(context, span, member, out value);
        }

        public virtual bool CanWrite(string member)
        {
            if (member == "size")
            {
                return false;
            }

            return ScriptObject.CanWrite(member);
        }

        public virtual bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            return ScriptObject.TrySetValue(context, span, member, value, readOnly);
        }

        public virtual bool Remove(string member)
        {
            return ScriptObject.Remove(member);
        }

        public virtual void SetReadOnly(string member, bool readOnly)
        {
            ScriptObject.SetReadOnly(member, readOnly);
        }

        public bool TryEvaluate(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object leftValue, SourceSpan rightSpan, object rightValue, out object result)
        {
            result = null;
            var leftArray = TryGetArray(leftValue);
            var rightArray = TryGetArray(rightValue);
            int intModifier = 0;
            var intSpan = leftSpan;

            var errorSpan = span;
            string reason = null;
            switch (op)
            {
                case ScriptBinaryOperator.BinaryOr:
                case ScriptBinaryOperator.BinaryAnd:
                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.Add:
                    if (leftArray == null)
                    {
                        errorSpan = leftSpan;
                        reason = " Expecting an array for the left argument.";
                    }
                    if (rightArray == null)
                    {
                        errorSpan = rightSpan;
                        reason = " Expecting an array for the right argument.";
                    }
                    break;
                case ScriptBinaryOperator.Multiply:
                    if (leftArray == null && rightArray == null || leftArray != null && rightArray != null)
                    {
                        reason = " Expecting only one array for the left or right argument.";
                    }
                    else
                    {
                        intModifier = context.ToInt(span, leftArray == null ? leftValue : rightValue);
                        if (rightArray == null) intSpan = rightSpan;
                    }
                    break;
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                    if (leftArray == null)
                    {
                        errorSpan = leftSpan;
                        reason = " Expecting an array for the left argument.";
                    }
                    else
                    {
                        intModifier = context.ToInt(span, rightValue);
                        intSpan = rightSpan;
                    }
                    break;
                case ScriptBinaryOperator.ShiftLeft:
                    if (leftArray == null)
                    {
                        errorSpan = leftSpan;
                        reason = " Expecting an array for the left argument.";
                    }
                    break;
                case ScriptBinaryOperator.ShiftRight:
                    if (rightArray == null)
                    {
                        errorSpan = rightSpan;
                        reason = " Expecting an array for the right argument.";
                    }
                    break;
                default:
                    reason = string.Empty;
                    break;
            }

            if (intModifier < 0)
            {
                errorSpan = intSpan;
                reason = $" Integer {intModifier} cannot be negative when multiplying";
            }

            if (reason != null)
            {
                throw new ScriptRuntimeException(errorSpan, $"The operator `{op.ToText()}` is not supported between {context.GetTypeName(leftValue)} and {context.GetTypeName(rightValue)}.{reason}");
            }

            switch (op)
            {
                case ScriptBinaryOperator.BinaryOr:
                    result = new ScriptArray<T>(leftArray.Union(rightArray));
                    return true;

                case ScriptBinaryOperator.BinaryAnd:
                    result = new ScriptArray<T>(leftArray.Intersect(rightArray));
                    return true;

                case ScriptBinaryOperator.Add:
                    result = ArrayFunctions.Concat(leftArray, rightArray);
                    return true;

                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareGreater:
                    result = CompareTo(context, span, op, leftArray, rightArray);
                    return true;

                case ScriptBinaryOperator.Multiply:
                {
                    // array with integer
                    var array = leftArray ?? rightArray;
                    if (intModifier == 0)
                    {
                        result = new ScriptArray<T>();
                        return true;
                    }

                    var newArray = new ScriptArray<T>(intModifier * array.Count);
                    for (int i = 0; i < intModifier; i++)
                    {
                        newArray.AddRange(array);
                    }

                    result = newArray;
                    return true;
                }

                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                {
                    // array with integer
                    var array = leftArray ?? rightArray;
                    if (intModifier == 0) throw new ScriptRuntimeException(intSpan, "Cannot divide by 0");

                    var newLength = array.Count / intModifier;
                    var newArray = new ScriptArray<T>(newLength);
                    for (int i = 0; i < newLength; i++)
                    {
                        newArray.Add(array[i]);
                    }

                    result = newArray;
                    return true;
                }

                case ScriptBinaryOperator.Modulus:
                {
                    // array with integer
                    var array = leftArray ?? rightArray;
                    if (intModifier == 0) throw new ScriptRuntimeException(intSpan, "Cannot divide by 0");

                    var newArray = new ScriptArray<T>(array.Count);
                    for (int i = 0; i < array.Count; i++)
                    {
                        if ((i % intModifier) == 0)
                        {
                            newArray.Add(array[i]);
                        }
                    }

                    result = newArray;
                    return true;
                }

                case ScriptBinaryOperator.ShiftLeft:
                    var newLeft = new ScriptArray<T>(leftArray);
                    newLeft.Add(typeof(T) == typeof(object) ? (T)rightValue : context.ToObject<T>(rightSpan, rightValue));
                    result = newLeft;
                    return true;

                case ScriptBinaryOperator.ShiftRight:
                    var newRight = new ScriptArray<T>(rightArray);
                    newRight.Insert(0, typeof(T) == typeof(object) ? (T)leftValue : context.ToObject<T>(leftSpan, leftValue));
                    result = newRight;
                    return true;
            }

            return false;
        }

        private static ScriptArray<T> TryGetArray(object rightValue)
        {
            var rightArray = rightValue as ScriptArray<T>;
            if (rightArray == null)
            {
                var list = rightValue as IList;
                if (list != null)
                {
                    rightArray = new ScriptArray<T>(list);
                }
                else if (rightValue is IEnumerable enumerable && !(rightValue is string))
                {
                    rightArray = new ScriptArray<T>(enumerable);
                }
            }
            return rightArray;
        }

        private static bool CompareTo(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, ScriptArray<T> left, ScriptArray<T> right)
        {
            // Compare the length first
            var compare = left.Count.CompareTo(right.Count);
            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                    if (compare != 0) return false;
                    break;
                case ScriptBinaryOperator.CompareNotEqual:
                    if (compare != 0) return true;
                    if (left.Count == 0) return false;
                    break;
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareLess:
                    if (compare < 0) return true;
                    if (compare > 0) return false;
                    if (left.Count == 0 && op == ScriptBinaryOperator.CompareLess) return false;
                    break;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareGreater:
                    if (compare < 0) return false;
                    if (compare > 0) return true;
                    if (left.Count == 0 && op == ScriptBinaryOperator.CompareGreater) return false;
                    break;
                default:
                    throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not supported between {context.GetTypeName(left)} and {context.GetTypeName(right)}.");
            }

            // Otherwise we need to compare each element
            for (int i = 0; i < left.Count; i++)
            {
                var result = (bool) ScriptBinaryExpression.Evaluate(context, span, op, left[i], right[i]);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }

        public Type ElementType => typeof(object);

        public virtual bool CanTransform(Type transformType)
        {
            return true;
        }

        public virtual bool Visit(TemplateContext context, SourceSpan span, Func<object, bool> visit)
        {
            foreach (var item in this)
            {
                if (!visit(item)) return false;
            }
            return true;
        }

        public virtual object Transform(TemplateContext context, SourceSpan span, Func<object, object> apply, Type destType)
        {
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            var clone = (ScriptArray<T>)Clone(true);
            var values = clone._values;
            if (typeof(T) == typeof(object))
            {
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = (T)apply(values[i]);
                }
            }
            else
            {
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = context.ToObject<T>(span, apply(values[i]));
                }
            }

            return clone;
        }

        internal class DebugListView
        {
            private readonly ScriptArray<T> _collection;

            public DebugListView(ScriptArray<T> collection)
            {
                this._collection = collection;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object[] Items => _collection._values.Cast<object>().ToArray();
        }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptArray : ScriptArray<object>
    {
        public ScriptArray()
        {
        }

        public ScriptArray(int capacity) : base(capacity)
        {
        }

        public ScriptArray(IEnumerable<object> values) : base(values)
        {
        }

        public ScriptArray(IEnumerable values) : base(values)
        {
        }
    }
}