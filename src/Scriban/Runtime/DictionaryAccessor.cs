// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    class NullAccessor : IMemberAccessor
    {
        public static readonly NullAccessor Default = new NullAccessor();

        public bool HasMember(object target, string member)
        {
            return false;
        }

        public bool TryGetValue(object target, string member, out object value)
        {
            value = null;
            return false;
        }

        public bool HasReadonly => false;

        public bool TrySetValue(object target, string member, object value)
        {
            return false;
        }

        public void SetReadOnly(object target, string member, bool isReadOnly)
        {
        }
    }

    class DictionaryAccessor : IMemberAccessor
    {
        public static readonly DictionaryAccessor Default = new DictionaryAccessor();

        private DictionaryAccessor()
        {
        }


        public static bool TryGet(Type type, out IMemberAccessor accessor)
        {
            if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                accessor = Default;
                return true;
            }

            var dictionaryType = type.GetBaseOrInterface(typeof(IDictionary<,>));
            accessor = null;
            if (dictionaryType == null) return false;
            var keyType = dictionaryType.GetTypeInfo().GetGenericArguments()[0];
            var valueType = dictionaryType.GetTypeInfo().GetGenericArguments()[1];

            var accessorType = typeof(GenericDictionaryAccessor<,>).GetTypeInfo().MakeGenericType(keyType, valueType);
            accessor = (IMemberAccessor)Activator.CreateInstance(accessorType);
            return true;
        }

        public bool HasMember(object target, string member)
        {
            return ((IDictionary) target).Contains(member);
        }

        public bool TryGetValue(object target, string member, out object value)
        {
            value = null;
            if (((IDictionary) target).Contains(member))
            {
                value = ((IDictionary)target)[member];
                return true;
            }
            return false;
        }

        public bool HasReadonly => false;

        public void SetReadOnly(object target, string member, bool isReadOnly)
        {
        }

        public bool TrySetValue(object target, string member, object value)
        {
            ((IDictionary) target)[member] = value;
            return true;
        }
    }

    class GenericDictionaryAccessor<TKey, TValue> : IMemberAccessor
    {
        public GenericDictionaryAccessor()
        {
        }

        public bool HasMember(object value, string member)
        {
            return ((IDictionary<TKey, TValue>) value).ContainsKey(TransformToKey(member));
        }

        public bool TryGetValue(object target, string member, out object value)
        {
            TValue tvalue;
            var result = ((IDictionary<TKey, TValue>) target).TryGetValue(TransformToKey(member), out tvalue);
            value = tvalue;
            return result;
        }

        public bool HasReadonly => false;

        public void SetReadOnly(object target, string member, bool isReadOnly)
        {
        }

        public bool TrySetValue(object target, string member, object value)
        {
            ((IDictionary<TKey, TValue>) value)[TransformToKey(member)] = (TValue)value;
            return true;
        }

        private TKey TransformToKey(string member)
        {
            return (TKey) ScriptValueConverter.ToObject(new SourceSpan(), member, typeof(TKey));
        }
    }
}