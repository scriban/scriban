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
        /// <summary>
        /// Adds a value to the input list.
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="value">The value to add at the end of the list</param>
        /// <returns>A new list with the value added</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | array.add 4 }}
        /// ```
        /// ```html
        /// [1, 2, 3, 4]
        /// ```
        /// </remarks>
        public static IList Add(IList list, object value)
        {
            if (list == null)
            {
                return new ScriptArray {value};
            }

            list = new ScriptArray(list) {value};
            return list;
        }


        /// <summary>
        /// Concatenates two lists.
        /// </summary>
        /// <param name="list1">The 1st input list</param>
        /// <param name="list2">The 2nd input list</param>
        /// <returns>The concatenation of the two input lists</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | array.concat [4, 5] }}
        /// ```
        /// ```html
        /// [1, 2, 3, 4, 5]
        /// ```
        /// </remarks>
        public static IEnumerable AddRange(IEnumerable list1, IEnumerable list2)
        {
            return Concat(list1, list2);
        }


        /// <summary>
        /// Removes any non-null values from the input list.
        /// </summary>
        /// <param name="list">An input list</param>
        /// <returns>Returns a list with null value removed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, null, 3] | array.compact }}
        /// ```
        /// ```html
        /// [1, 3]
        /// ```
        /// </remarks>
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
        /// Concatenates two lists.
        /// </summary>
        /// <param name="list1">The 1st input list</param>
        /// <param name="list2">The 2nd input list</param>
        /// <returns>The concatenation of the two input lists</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | array.concat [4, 5] }}
        /// ```
        /// ```html
        /// [1, 2, 3, 4, 5]
        /// ```
        /// </remarks>
        public static IEnumerable Concat(IEnumerable list1, IEnumerable list2)
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

            var result = new ScriptArray(list1);
            foreach (var item in list2) result.Add(item);
            return result;
        }

        /// <summary>
        /// Loops through a group of strings and outputs them in the order that they were passed as parameters. Each time cycle is called, the next string that was passed as a parameter is output.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">An input list</param>
        /// <param name="group">The group used. Default is `null`</param>
        /// <returns>Returns a list with null value removed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// {{ array.cycle ['one', 'two', 'three'] }}
        /// ```
        /// ```html
        /// one
        /// two
        /// three
        /// one
        /// ```
        /// `cycle` accepts a parameter called cycle group in cases where you need multiple cycle blocks in one template. 
        /// If no name is supplied for the cycle group, then it is assumed that multiple calls with the same parameters are one group.
        /// </remarks>
        public static object Cycle(TemplateContext context, SourceSpan span, IList list, object group = null)
        {
            if (list == null)
            {
                return null;
            }
            var strGroup = group == null ? Join(context, span, list, ",") : context.ToString(span, group);

            // We create a cycle variable that is dependent on the exact AST context.
            // So we allow to have multiple cycle running in the same loop
            var cycleKey = new CycleKey(strGroup);

            object cycleValue;
            var currentTags = context.Tags;
            if (!currentTags.TryGetValue(cycleKey, out cycleValue) || !(cycleValue is int))
            {
                cycleValue = 0;
            }

            var cycleIndex = (int) cycleValue;
            cycleIndex = list.Count == 0 ? 0 : cycleIndex % list.Count;
            object result = null;
            if (list.Count > 0)
            {
                result = list[cycleIndex];
                cycleIndex++;
            }
            currentTags[cycleKey] = cycleIndex;

            return result;
        }

        /// <summary>
        /// Returns the first element of the input `list`.
        /// </summary>
        /// <param name="list">The input list</param>
        /// <returns>The first element of the input `list`.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6] | array.first }}
        /// ```
        /// ```html
        /// 4
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Inserts a `value` at the specified index in the input `list`.
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="index">The index in the list where to insert the element</param>
        /// <param name="value">The value to insert</param>
        /// <returns>A new list with the element inserted.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ ["a", "b", "c"] | array.insert_at 2 "Yo" }}
        /// ```
        /// ```html
        /// [a, b, Yo, c]
        /// ```
        /// </remarks>
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


        /// <summary>
        /// Joins the element of a list separated by a delimiter string and return the concatenated string.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">The input list</param>
        /// <param name="delimiter">The delimiter string to use to separate elements in the output string</param>
        /// <returns>A new list with the element inserted.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | array.join "|" }}
        /// ```
        /// ```html
        /// 1|2|3
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Returns the last element of the input `list`.
        /// </summary>
        /// <param name="list">The input list</param>
        /// <returns>The last element of the input `list`.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6] | array.last }}
        /// ```
        /// ```html
        /// 6
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Returns a limited number of elments from the input list
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="count">The number of elements to return from the input list</param>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6] | array.limit 2 }}
        /// ```
        /// ```html
        /// [4, 5]
        /// ```
        /// </remarks>
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
        /// Accepts an array element's attribute as a parameter and creates an array out of each array element's value.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">The input list</param>
        /// <param name="member">The member to extract the value from</param>
        /// <remarks>
        /// ```scriban-html
        /// {{ 
        /// products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
        /// products | array.map "type" | array.uniq | array.sort }}
        /// ```
        /// ```html
        /// [electronics, fruit, furniture]
        /// ```
        /// </remarks>
        public static IEnumerable Map(TemplateContext context, SourceSpan span, object list, string member)
        {
            if (list == null || member == null)
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
                    itemAccessor.TryGetValue(context, span, item, member, out object value);
                    yield return value;
                }
            }
        }

        /// <summary>
        /// Returns the remaining of the list after the specified offset
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="index">The index of a list to return elements</param>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6, 7, 8] | array.offset 2 }}
        /// ```
        /// ```html
        /// [6, 7, 8]
        /// ```
        /// </remarks>
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
        /// Removes an element at the specified `index` from the input `list`
        /// </summary>
        /// <param name="list">The input list</param>
        /// <param name="index">The index of a list to return elements</param>
        /// <returns>A new list with the element removed. If index is negative, remove at the end of the list.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6, 7, 8] | array.remove_at 2 }}
        /// ```
        /// ```html
        /// [4, 5, 7, 8]
        /// ```
        /// If the `index` is negative, removes at the end of the list (notice that we need to put -1 in parenthesis to avoid confusing the parser with a binary `-` operation):
        /// ```scriban-html
        /// {{ [4, 5, 6, 7, 8] | array.remove_at (-1) }}
        /// ```
        /// ```html
        /// [4, 5, 6, 7]
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Reverses the input `list`
        /// </summary>
        /// <param name="list">The input list</param>
        /// <returns>A new list in reversed order.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6, 7] | array.reverse }}
        /// ```
        /// ```html
        /// [7, 6, 5, 4]
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Returns the number of elements in the input `list`
        /// </summary>
        /// <param name="list">The input list</param>
        /// <returns>A number of elements in the input `list`.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [4, 5, 6] | array.size }}
        /// ```
        /// ```html
        /// 3
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Sorts the elements of the input `list` according to the value of each element or the value of the specified `member` of each element
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">The input list</param>
        /// <param name="member">The member name to sort according to its value. Null by default, meaning that the element's value are used instead.</param>
        /// <returns>A list sorted according to the value of each element or the value of the specified `member` of each element.</returns>
        /// <remarks>
        /// Sorts by element's value: 
        /// ```scriban-html
        /// {{ [10, 2, 6] | array.sort }}
        /// ```
        /// ```html
        /// [2, 6, 10]
        /// ```
        /// Sorts by elements member's value: 
        /// ```scriban-html
        /// {{
        /// products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
        /// products | array.sort "title" | array.map "title"
        /// }}
        /// ```
        /// ```html
        /// [computer, orange, sofa]
        /// ```
        /// </remarks>
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

        /// <summary>
        /// Returns the unique elements of the input `list`.
        /// </summary>
        /// <param name="list">The input list</param>
        /// <returns>A list of unique elements of the input `list`.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 1, 4, 5, 8, 8] | array.uniq }}
        /// ```
        /// ```html
        /// [1, 4, 5, 8]
        /// ```
        /// </remarks>
        public static IEnumerable Uniq(IEnumerable list)
        {
            return list?.Cast<object>().Distinct();
        }

        /// <summary>
        /// Returns if an `list` contains an specifique element 
        /// </summary>
        /// <param name="list">the input list</param>
        /// <param name="item">the input item</param>
        /// <returns>**true** if element is in `list`; otherwise **false**</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3, 4] | array.contains 4 }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool Contains(IEnumerable list, object item)
        {
            foreach (var element in list)
                if (element == item || (element != null && element.Equals(item))) return true;
            return false;
        }

        private class CycleKey : IEquatable<CycleKey>
        {
            public readonly string Group;

            public CycleKey(string @group)
            {
                Group = @group;
            }

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