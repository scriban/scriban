// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
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
    public class ScriptArray : IList<object>, IList, IScriptObject, IScriptCustomBinaryOperation
    {
        private List<object> _values;
        private bool _isReadOnly;

        // Attached ScriptObject is only created if needed
        private ScriptObject _script;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        public ScriptArray()
        {
            _values = new List<object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray"/> class.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public ScriptArray(int capacity)
        {
            _values = new List<object>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptArray{object}"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ScriptArray(IEnumerable<object> values)
        {
            this._values = new List<object>(values);
        }

        public ScriptArray(IEnumerable values)
        {
            this._values = new List<object>();
            foreach (var value in values)
            {
                _values.Add((object)value);
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
            var array = (ScriptArray) MemberwiseClone();
            array._values = new List<object>(_values.Count);
            array._script = null;
            if (deep)
            {
                foreach (var value in array._values)
                {
                    var fromValue = value;
                    if (value is IScriptObject)
                    {
                        var fromObject = (IScriptObject)value;
                        fromValue = fromObject.Clone(true);
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
                foreach (var value in array._values)
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

        public virtual object this[int index]
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

        public virtual void Add(object item)
        {
            this.AssertNotReadOnly();
            _values.Add(item);
        }

        public void AddRange(IEnumerable<object> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
            {
                Add(item);
            }
        }

        int IList.Add(object value)
        {
            Add((object) value);
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
            Insert(index, (object)value);
        }

        public virtual bool Contains(object item)
        {
            return _values.Contains(item);
        }

        public virtual void CopyTo(object[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(int index, object[] array, int arrayIndex, int count)
        {
            _values.CopyTo(index, array, arrayIndex, count);
        }

        public virtual int IndexOf(object item)
        {
            return _values.IndexOf(item);
        }

        public virtual void Insert(int index, object item)
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
            Remove((object) value);
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
                this[index] = (object)value;
            }
        }

        public virtual bool Remove(object item)
        {
            this.AssertNotReadOnly();
            return _values.Remove(item);
        }

        public List<object>.Enumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        bool IList.IsFixedSize => ((IList)_values).IsFixedSize;

        bool ICollection.IsSynchronized => ((ICollection)_values).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)_values).SyncRoot;

        bool IList.IsReadOnly => IsReadOnly;

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        bool ICollection<object>.IsReadOnly => IsReadOnly;

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

        public virtual void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            ScriptObject.SetValue(context, span, member, value, readOnly);
        }

        public virtual bool Remove(string member)
        {
            return ScriptObject.Remove(member);
        }

        public virtual void SetReadOnly(string member, bool readOnly)
        {
            ScriptObject.SetReadOnly(member, readOnly);
        }

        public object Evaluate(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object leftValue, SourceSpan rightSpan, object rightValue)
        {
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
                throw new ScriptRuntimeException(errorSpan, $"The operator `{op.ToText()}` is not supported between {leftValue?.GetType().ScriptPrettyName()} and {rightValue?.GetType().ScriptPrettyName()}.{reason}");
            }

            switch (op)
            {
                case ScriptBinaryOperator.BinaryOr:
                    return new ScriptArray(leftArray.Union(rightArray));

                case ScriptBinaryOperator.BinaryAnd:
                    return new ScriptArray(leftArray.Intersect(rightArray));

                case ScriptBinaryOperator.Add:
                    return ArrayFunctions.Concat(leftArray, rightArray);

                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareGreater:
                    return CompareTo(context, span, op, leftArray, rightArray);

                case ScriptBinaryOperator.Multiply:
                {
                    // array with integer
                    var array = leftArray ?? rightArray;
                    if (intModifier == 0) return new ScriptArray();

                    var newArray = new ScriptArray(array);
                    for (int i = 0; i < intModifier; i++)
                    {
                        newArray.AddRange(array);
                    }

                    return newArray;
                }

                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                {
                    // array with integer
                    var array = leftArray ?? rightArray;
                    if (intModifier == 0) throw new ScriptRuntimeException(intSpan, "Cannot divide by 0");

                    var newLength = array.Count / intModifier;
                    var newArray = new ScriptArray(newLength);
                    for (int i = 0; i < newLength; i++)
                    {
                        newArray.Add(array[i]);
                    }

                    return newArray;
                }

                case ScriptBinaryOperator.Modulus:
                {
                    // array with integer
                    var array = leftArray ?? rightArray;
                    if (intModifier == 0) throw new ScriptRuntimeException(intSpan, "Cannot divide by 0");

                    var newArray = new ScriptArray(array.Count);
                    for (int i = 0; i < array.Count; i++)
                    {
                        if ((i % intModifier) == 0)
                        {
                            newArray.Add(array[i]);
                        }
                    }

                    return newArray;
                }

                case ScriptBinaryOperator.ShiftLeft:
                    var newLeft = new ScriptArray(leftArray);
                    newLeft.Add(rightValue);
                    return newLeft;

                case ScriptBinaryOperator.ShiftRight:
                    var newRight = new ScriptArray(rightArray);
                    newRight.Insert(0, leftValue);
                    return newRight;

                default:
                    throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not supported between {leftValue?.GetType().ScriptPrettyName()} and {rightValue?.GetType().ScriptPrettyName()}.");
            }
        }

        private static ScriptArray TryGetArray(object rightValue)
        {
            var rightArray = rightValue as ScriptArray;
            if (rightArray == null)
            {
                var list = rightValue as IList;
                if (list != null)
                {
                    rightArray = new ScriptArray(list);
                }
                else if (rightValue is IEnumerable enumerable && !(rightValue is string))
                {
                    rightArray = new ScriptArray(enumerable);
                }
            }
            return rightArray;
        }


        private static bool CompareTo(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, ScriptArray left, ScriptArray right)
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
                    throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not supported between {left?.GetType().ScriptPrettyName()} and {right?.GetType().ScriptPrettyName()}.");
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
    }

    internal class ScriptPipeArguments : Stack<ScriptExpression>
    {
        public ScriptPipeArguments()
        {
        }

        public ScriptPipeArguments(int capacity) : base(capacity)
        {
        }
    }
}