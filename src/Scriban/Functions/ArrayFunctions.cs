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
        public static string Join(TemplateContext context, SourceSpan span, IEnumerable list, string delimiter)
        {
            if (list == null)
            {
                return string.Empty;
            }

            var text = new StringBuilder();
            bool afterFirst = false;
            foreach (var obj in list)
            {
                if (afterFirst)
                {
                    text.Append(delimiter);
                }
                text.Append(context.ToString(span, obj));
                afterFirst = true;
            }
            return text.ToString();
        }

        public static IEnumerable Uniq(IEnumerable list)
        {
            return list?.Cast<object>().Distinct();
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
        /// <param name="list1">An input list</param>
        /// <param name="list2">An input list</param>
        /// <returns>The concatenation of the </returns>
        public static object Concat(IEnumerable list1, IEnumerable list2)
        {
            if (list2 == null && list1 == null)
            {
                return null;
            }
            if (list2 == null)
            {
                return list1;
            }

            if (list1 == null)
            {
                return list2;
            }

            var result = new ScriptArray();
            foreach (var item in list1) result.Add(item);
            foreach (var item in list2) result.Add(item);
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

        public static object First(IEnumerable list)
        {
            if (list == null)
            {
                return null;
            }

            var realList = list as IList;
            if (realList != null)
            {
                return realList.Count > 0 ? realList[0] : null;
            }

            foreach (var item in list)
            {
                return item;
            }

            return null;
        }

        public static object Last(IEnumerable list)
        {
            if (list == null)
            {
                return null;
            }

            var readList = list as IList;
            if (readList != null)
            {
                return readList.Count > 0 ? readList[readList.Count - 1] : null;
            }

            // Slow path, go through the whole list
            return list.Cast<object>().LastOrDefault();
        }

        public static IEnumerable Reverse(IEnumerable list)
        {
            if (list == null)
            {
                return Enumerable.Empty<object>();
            }

            // TODO: provide a special path for IList
            //var list = list as IList;
            //if (list != null)
            //{
            //}
            return list.Cast<object>().Reverse();
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

        public static IList AddRange(IList list1, IEnumerable list2)
        {
            if (list2 == null)
            {
                return list1;
            }

            list1 = list1 == null ? new ScriptArray() : new ScriptArray(list1);
            foreach (var value in list2)
            {
                list1.Add(value);
            }
            return list1;
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

        public static IEnumerable Sort(TemplateContext context, SourceSpan span, object list, string member = null)
        {
            if (list == null)
            {
                return Enumerable.Empty<object>();
            }

            var enumerable = list as IEnumerable;
            if (enumerable == null)
            {
                return new ScriptArray(1) {list};
            }

            var realList = enumerable.Cast<object>().ToList();
            if (realList.Count == 0)
                return realList;

            if (string.IsNullOrEmpty(member))
            {
                realList.Sort();
            }
            else
            {
                realList.Sort((a, b) =>
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

            return realList;
        }

        public static IEnumerable Map(TemplateContext context, SourceSpan span, object list, string member = null)
        {
            if (list == null)
            {
                yield break;
            }

            var enumerable = list as IEnumerable;
            var realList = enumerable?.Cast<object>().ToList() ?? new List<object>(1) {list};
            if (realList.Count == 0)
            {
                yield break;
            }

            foreach (var item in realList)
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

        public static object Cycle(TemplateContext context, SourceSpan span, object listOrGroup, object list = null)
        {
            string group = null;
            IList valueList = null;
            if (list == null)
            {
                valueList = context.ToList(span, listOrGroup);
                group = Join(context, span, valueList, ",");
            }
            else 
            {
                group = context.ToString(span, listOrGroup);
                valueList = context.ToList(span, list);
            }

            if (!(valueList is IList))
            {
                return null;
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

            var cycleIndex = (int)cycleValue;
            cycleIndex = valueList.Count == 0 ? 0 : cycleIndex % valueList.Count;
            object result = null;
            if (valueList.Count > 0)
            {
                result = valueList[cycleIndex];
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