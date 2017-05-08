// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Functions
{
    /// <summary>
    /// Object functions available through the object 'object' in scriban.
    /// </summary>
    public class ObjectFunctions : ScriptObject
    {
        public new static IEnumerable<object> Keys(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                yield break;
            }

            foreach (var entry in dictionary)
            {
                yield return entry.Key;
            }
        }

        public new static IEnumerable<object> Values(IDictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                yield break;
            }

            foreach (var entry in dictionary)
            {
                yield return entry.Value;
            }
        }

        public static string Typeof(object value)
        {
            if (value == null)
            {
                return null;
            }
            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            if (type == typeof (string))
            {
                return "string";
            }

            if (type == typeof (bool))
            {
                return "boolean";
            }

            // We assume that we are only using int/double/long for integers and shortcut to IsPrimitive
            if (typeInfo.IsPrimitive)
            {
                return "number";
            }

            // Test first IList, then IEnumerable
            if (typeof (IList).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return "array";
            }

            if ((!typeof(ScriptObject).GetTypeInfo().IsAssignableFrom(typeInfo) && !typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeInfo)) && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return "iterator";
            }

            return "object";
        }
    }
}