// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// Array functions available through the object 'array' in scriban.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ArrayFunctions : ScriptObject
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
        public static IEnumerable Add(IEnumerable list, object value)
        {
            if (list == null)
            {
                return new ScriptRange { value };
            }

            return list is IList ? (IEnumerable)new ScriptArray(list) { value } : new ScriptRange(list) { value };
        }


        /// <summary>
        /// Concatenates two lists.
        /// </summary>
        /// <param name="list1">The 1st input list</param>
        /// <param name="list2">The 2nd input list</param>
        /// <returns>The concatenation of the two input lists</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | array.add_range [4, 5] }}
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
        public static IEnumerable Compact(IEnumerable list)
        {
            return ScriptRange.Compact(list);
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
            return ScriptRange.Concat(list1, list2);
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
            var strGroup = group == null ? Join(context, span, list, ",") : context.ObjectToString(@group);

            // We create a cycle variable that is dependent on the exact AST context.
            // So we allow to have multiple cycle running in the same loop
            var cycleKey = new CycleKey(strGroup);

            object cycleValue;
            var currentTags = context.Tags;
            if (!currentTags.TryGetValue(cycleKey, out cycleValue) || !(cycleValue is int))
            {
                cycleValue = 0;
            }

            var cycleIndex = (int)cycleValue;
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
        /// Applies the specified function to each element of the input.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">An input list</param>
        /// <param name="function">The function to apply to each item in the list</param>
        /// <returns>Returns a list with each item being transformed by the function.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [" a", " 5", "6 "] | array.each @string.strip }}
        /// ```
        /// ```html
        /// ["a", "5", "6"]
        /// ```
        /// </remarks>
        public static ScriptRange Each(TemplateContext context, SourceSpan span, IEnumerable list, object function)
        {
            return ApplyFunction(context, span, list, function, EachProcessor);
        }
        private static IEnumerable EachInternal(TemplateContext context, ScriptNode callerContext, SourceSpan span, IEnumerable list, IScriptCustomFunction function, Type destType)
        {
            var arg = new ScriptArray(1);
            foreach (var item in list)
            {
                var itemToTransform = context.ToObject(span, item, destType);
                arg[0] = itemToTransform;
                var itemTransformed = ScriptFunctionCall.Call(context, callerContext, function, arg);
                yield return itemTransformed;
            }
        }

        private static readonly ListProcessor EachProcessor = EachInternal;

        /// <summary>
        /// Filters the input list according the supplied filter function.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">An input list</param>
        /// <param name="function">The function used to test each elemement of the list</param>
        /// <returns>Returns a new list which contains only those elements which match the filter function.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{["", "200", "","400"] | array.filter @string.empty}}
        /// ```
        /// ```html
        /// ["", ""]
        /// ```
        /// </remarks>
        public static ScriptRange Filter(TemplateContext context, SourceSpan span, IEnumerable list, object function)
        {
            return ApplyFunction(context, span, list, function, FilterProcessor);
        }


        static IEnumerable FilterInternal(TemplateContext context, ScriptNode callerContext, SourceSpan span, IEnumerable list, IScriptCustomFunction function, Type destType)
        {
            var arg = new ScriptArray(1);
            foreach (var item in list)
            {
                var itemToTransform = context.ToObject(span, item, destType);
                arg[0] = itemToTransform;
                var itemTransformed = ScriptFunctionCall.Call(context, callerContext, function, arg);
                if (context.ToBool(span, itemTransformed))
                    yield return itemToTransform;
            }
        }

        private static readonly ListProcessor FilterProcessor = FilterInternal;

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
        /// ["a", "b", "Yo", "c"]
        /// ```
        /// </remarks>
        public static IEnumerable InsertAt(IEnumerable list, int index, object value)
        {
            if (index < 0)
            {
                index = 0;
            }

            var array = list == null ? new ScriptArray() : new ScriptArray(list);
            // Make sure that the list has already inserted elements before the index
            for (int i = array.Count; i < index; i++)
            {
                array.Add(null);
            }

            array.Insert(index, value);

            return array;
        }


        /// <summary>
        /// Joins the element of a list separated by a delimiter string and return the concatenated string.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="list">The input list</param>
        /// <param name="delimiter">The delimiter string to use to separate elements in the output string</param>
        /// <param name="function">An optional function that will receive the string representation of the item to join and can transform the text before joining.</param>
        /// <returns>A new list with the element inserted.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | array.join "|" }}
        /// ```
        /// ```html
        /// 1|2|3
        /// ```
        /// </remarks>
        public static string Join(TemplateContext context, SourceSpan span, IEnumerable list, string delimiter, object function = null)
        {
            if (list == null)
            {
                return string.Empty;
            }

            var scriptingFunction = function as IScriptCustomFunction;
            if (function != null && scriptingFunction == null)
            {
                throw new ArgumentException($"The parameter `{function}` is not a function. Maybe prefix it with @?", nameof(function));
            }

            var text = new StringBuilder();
            bool afterFirst = false;
            var arg = new ScriptArray(1);
            foreach (var obj in list)
            {
                if (afterFirst)
                {
                    text.Append(delimiter);
                }

                var item = context.ObjectToString(obj);
                if (scriptingFunction != null)
                {
                    arg[0] = item;
                    var result = ScriptFunctionCall.Call(context, context.CurrentNode, scriptingFunction, arg);
                    item = context.ObjectToString(result);
                }

                text.Append(item);
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
        public static IEnumerable Limit(IEnumerable list, int count)
        {
            return ScriptRange.Limit(list, count);
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
        /// ["electronics", "fruit", "furniture"]
        /// ```
        /// </remarks>
        public static IEnumerable Map(TemplateContext context, SourceSpan span, object list, string member)
        {
            return new ScriptRange(MapImpl(context, span, list, member));
        }

        private static IEnumerable MapImpl(TemplateContext context, SourceSpan span, object list, string member)
        {
            if (list == null || member == null)
            {
                yield break;
            }

            var enumerable = list as IEnumerable;
            var realList = enumerable?.Cast<object>().ToList() ?? new List<object>(1) { list };
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
        public static IEnumerable Offset(IEnumerable list, int index)
        {
            return ScriptRange.Offset(list, index);
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
            return ScriptRange.Reverse(list);
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
        /// ["computer", "orange", "sofa"]
        /// ```
        /// </remarks>
        public static IEnumerable Sort(TemplateContext context, SourceSpan span, object list, string member = null)
        {
            if (list == null)
            {
                return new ScriptRange();
            }

            var enumerable = list as IEnumerable;
            if (enumerable == null)
            {
                return new ScriptArray(1) { list };
            }

            var realList = enumerable.Cast<object>().ToList();
            if (realList.Count == 0)
                return new ScriptArray();

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

            return new ScriptArray(realList);
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
            return ScriptRange.Uniq(list);
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
            {
                if (element == item || (element != null && element.Equals(item))) return true;
                if (element is Enum e && CompareEnum(e, item)) return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CompareEnum(Enum e, object item)
        {
            try
            {
                if (item is string s) return Enum.Parse(e.GetType(), s)?.Equals(e) ?? false;
                return Enum.ToObject(e.GetType(), item)?.Equals(e) ?? false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Delegate type for function used to process a list
        /// </summary>
        private delegate IEnumerable ListProcessor(TemplateContext context, ScriptNode callerContext, SourceSpan span, IEnumerable list, IScriptCustomFunction function, Type destType);


        /// <summary>
        /// Attempts to apply a Scriban function to a list and returns the results as a ScriptRange
        /// </summary>
        /// <remarks>
        /// Encapsulates a common approach to parameter checking for any method that will take a Scriban function and apply it to a list
        /// </remarks>
        private static ScriptRange ApplyFunction(TemplateContext context, SourceSpan span, IEnumerable list, object function,
            ListProcessor impl)
        {
            if (list == null) return null;
            if (function == null) return new ScriptRange(list);

            var scriptingFunction = function as IScriptCustomFunction;
            if (scriptingFunction == null)
            {
                throw new ArgumentException($"The parameter `{function}` is not a function. Maybe prefix it with @?", nameof(function));
            }

            var callerContext = context.CurrentNode;

            return new ScriptRange(impl(context, callerContext, span, list, scriptingFunction, scriptingFunction.GetParameterInfo(0).ParameterType));
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
                return Equals((CycleKey)obj);
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