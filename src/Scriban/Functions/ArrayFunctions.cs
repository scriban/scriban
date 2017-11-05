// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// Array functions available through the object 'array' in scriban.
    /// </summary>
    public class ArrayFunctions : ScriptObject
    {
        public ArrayFunctions()
        {
            SetValue("cycle", new DelegateCustomFunction(Cycle), true);
        }

        public static string Join(TemplateContext context, SourceSpan span, IEnumerable enumerable, string delimiter)
        {
            var text = new StringBuilder();
            bool afterFirst = false;
            foreach (var obj in enumerable)
            {
                if (afterFirst)
                {
                    text.Append(delimiter);
                }
                // TODO: We need to convert to a string but we don't have a span!
                text.Append(context.ToString(new SourceSpan("unknown", new TextPosition(), new TextPosition()), obj));
                afterFirst = true;
            }
            return text.ToString();
        }

        public static IEnumerable Uniq(IEnumerable iterator)
        {
            return iterator?.Cast<object>().Distinct();
        }

        /// <summary>
        /// Returns only count elments from the input list
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="count">The number of elements to return from the input list</param>
        public static ScriptArray Limit(IEnumerable list, int count)
        {
            if (list == null)
            {
                return null;
            }

            var result = new ScriptArray();
            foreach (var item in list)
            {
                count--;
                if (count < 0)
                {
                    break;
                }
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Returns the remaining of the list after the specified offset
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="index">The index of a list to return elements</param>
        public static ScriptArray Offset(IEnumerable list, int index)
        {
            if (list == null)
            {
                return null;
            }

            var result = new ScriptArray();
            foreach (var item in list)
            {
                if (index <= 0)
                {
                    result.Add(item);
                }
                else
                {
                    index--;
                }
            }
            return result;
        }


        /// <summary>
        /// Removes any non-null values from the input list
        /// </summary>
        /// <param name="list">An input list</param>
        /// <returns>Returns a list with null value removed</returns>
        public static ScriptArray Compact(IEnumerable list)
        {
            if (list == null)
            {
                return null;
            }

            var result = new ScriptArray();
            foreach (var item in list)
            {
                if (item != null)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        /// <summary>
        /// Concats the two arrays into another array
        /// </summary>
        /// <param name="left">An input list</param>
        /// <param name="right">An input list</param>
        /// <returns>The concatenation of the </returns>
        public static object Concat(IEnumerable left, IEnumerable right)
        {
            if (right == null && left == null)
            {
                return null;
            }
            if (right == null)
            {
                return left;
            }

            if (left == null)
            {
                return right;
            }

            var result = new ScriptArray();
            foreach (var item in left) result.Add(item);
            foreach (var item in right) result.Add(item);
            return result;
        }

        public static int Size(IEnumerable list)
        {
            if (list == null)
            {
                return 0;
            }
            var collection = list as ICollection;
            if (collection != null)
            {
                return collection.Count;
            }

            // Slow path, go through the whole list
            return list.Cast<object>().Count();
        }

        public static object First(IEnumerable iterator)
        {
            if (iterator == null)
            {
                return null;
            }

            var list = iterator as IList;
            if (list != null)
            {
                return list.Count > 0 ? list[0] : null;
            }

            foreach (var item in iterator)
            {
                return item;
            }

            return null;
        }

        public static object Last(IEnumerable iterator)
        {
            if (iterator == null)
            {
                return null;
            }

            var list = iterator as IList;
            if (list != null)
            {
                return list.Count > 0 ? list[list.Count - 1] : null;
            }

            // Slow path, go through the whole list
            return iterator.Cast<object>().LastOrDefault();
        }

        public static IEnumerable Reverse(IEnumerable iterator)
        {
            if (iterator == null)
            {
                return Enumerable.Empty<object>();
            }

            // TODO: provide a special path for IList
            //var list = iterator as IList;
            //if (list != null)
            //{
            //}
            return iterator.Cast<object>().Reverse();
        }

        public static IList RemoveAt(IList list, int index)
        {
            if (list == null)
            {
                return new ScriptArray();
            }

            list = new ScriptArray(list);

            // If index is negative, start from the end
            if (index < 0)
            {
                index = list.Count + index;
            }

            if (index >= 0 && index < list.Count)
            {
                list.RemoveAt(index);
            }
            return list;
        }

        public static IList Add(IList list, object value)
        {
            if (list == null)
            {
                return new ScriptArray { value };
            }

            list = new ScriptArray(list) {value};
            return list;
        }

        public static IList AddRange(IList list, IEnumerable iterator)
        {
            if (iterator == null)
            {
                return list;
            }

            list = list == null ? new ScriptArray() : new ScriptArray(list);
            foreach (var value in iterator)
            {
                list.Add(value);
            }
            return list;
        }


        public static IList InsertAt(IList list, int index, object value)
        {
            if (index < 0)
            {
                index = 0;
            }

            list = list == null ? new ScriptArray() : new ScriptArray(list);
            // Make sure that the list has already inserted elements before the index
            for (int i = list.Count; i < index; i++)
            {
                list.Add(null);
            }

            list.Insert(index, value);

            return list;
        }

        public static IEnumerable Sort(TemplateContext context, SourceSpan span, object input, string member = null)
        {
            if (input == null)
            {
                return Enumerable.Empty<object>();
            }

            var enumerable = input as IEnumerable;
            if (enumerable == null)
            {
                return new ScriptArray(1) {input};
            }

            var list = enumerable.Cast<object>().ToList();
            if (list.Count == 0)
                return list;

            if (string.IsNullOrEmpty(member))
            {
                list.Sort();
            }
            else
            {
                list.Sort((a, b) =>
                {
                    var leftAccessor = context.GetMemberAccessor(a);
                    var rightAccessor = context.GetMemberAccessor(b);

                    object leftValue = null;
                    object rightValue = null;
                    if (!leftAccessor.TryGetValue(context, span, a, member, out leftValue))
                    {
                        context.TryGetMember?.Invoke(context, span, a, member, out leftValue);
                    }

                    if (!rightAccessor.TryGetValue(context, span, b, member, out rightValue))
                    {
                        context.TryGetMember?.Invoke(context, span, b, member, out rightValue);
                    }

                    return Comparer<object>.Default.Compare(leftValue, rightValue);
                });
            }

            return list;
        }

        public static IEnumerable Map(TemplateContext context, SourceSpan span, object input, string member = null)
        {
            if (input == null)
            {
                yield break;
            }

            var enumerable = input as IEnumerable;
            var list = enumerable?.Cast<object>().ToList() ?? new List<object>(1) {input};
            if (list.Count == 0)
            {
                yield break;
            }

            foreach (var item in list)
            {
                var itemAccessor = context.GetMemberAccessor(item);
                if (itemAccessor.HasMember(context, span, item, member))
                {
                    object value = null;
                    itemAccessor.TryGetValue(context, span, item, member, out value);

                    yield return value;
                }
            }
        }

        private static object Cycle(TemplateContext context, ScriptNode callerContext, ScriptArray parameters)
        {
            string group = null;
            IList values = null;
            if (parameters.Count == 1)
            {
                values = context.ToList(callerContext.Span, parameters[0]);
                group = Join(context, callerContext.Span, values, ",");
            }
            else if (parameters.Count == 2)
            {
                group = context.ToString(callerContext.Span, parameters[0]);
                values = context.ToList(callerContext.Span, parameters[1]);
            }
            else
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected number of arguments `{parameters.Count}` for cycle. Expecting at least 1 parameter: cycle [val1, val2..] or cycle <group> [val1, val2...]");
            }

            // We create a cycle variable that is dependent on the exact AST context.
            // So we allow to have multiple cycle running in the same loop
            var cycleKey = new CycleKey(group);

            object cycleValue;
            var currentTags = context.Tags;
            if (!currentTags.TryGetValue(cycleKey, out cycleValue) || !(cycleValue is int))
            {
                cycleValue = 0;
            }

            var cycleIndex = (int) cycleValue;
            cycleIndex = values.Count == 0 ? 0 : cycleIndex % values.Count;
            object result = null;
            if (values.Count > 0)
            {
                result = values[cycleIndex];
                cycleIndex++;
            }
            currentTags[cycleKey] = cycleIndex;

            return result;
        }

        private class CycleKey : IEquatable<CycleKey>
        {
            public CycleKey(string @group)
            {
                Group = @group;
            }

            public readonly string Group;

            public bool Equals(CycleKey other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Group, other.Group);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((CycleKey) obj);
            }

            public override int GetHashCode()
            {
                return (Group != null ? Group.GetHashCode() : 0);
            }

            public override string ToString()
            {
                return $"cycle {Group}";
            }
        }
    }
}