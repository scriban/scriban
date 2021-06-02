// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed partial class DictionaryAccessor : IObjectAccessor
    {
        public static readonly DictionaryAccessor Default = new DictionaryAccessor();

        private DictionaryAccessor()
        {
        }


        public static bool TryGet(object target, out IObjectAccessor accessor)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target is IDictionary<string, object>)
            {
                accessor = DictionaryStringObjectAccessor.Default;
                return true;
            }

            var type = target.GetType();
            var dictionaryType = type.GetBaseOrInterface(typeof(IDictionary<,>));
            if (dictionaryType == null)
            {
                if (target is IDictionary)
                {
                    accessor = Default;
                    return true;
                }

                accessor = null;
                return false;
            }
            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];

            var accessorType = typeof(GenericDictionaryAccessor<,>).MakeGenericType(keyType, valueType);
            accessor = (IObjectAccessor)Activator.CreateInstance(accessorType);
            return true;
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return ((IDictionary) target).Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            foreach (var key in ((IDictionary) target).Keys)
            {
                yield return context.ObjectToString(key);
            }
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return ((IDictionary) target).Contains(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;
            if (((IDictionary) target).Contains(member))
            {
                value = ((IDictionary)target)[member];
                return true;
            }
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            ((IDictionary) target)[member] = value;
            return true;
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
        {
            value = null;
            if (((IDictionary) target).Contains(index))
            {
                value = ((IDictionary)target)[index];
                return true;
            }
            return false;
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
        {
            ((IDictionary) target)[index] = value;
            return true;
        }

        public bool HasIndexer => true;

        public Type IndexType => typeof(object);
    }

    class DictionaryStringObjectAccessor : GenericDictionaryAccessor<string, object>
    {
        public readonly static DictionaryStringObjectAccessor Default = new DictionaryStringObjectAccessor();
    }

    class GenericDictionaryAccessor<TKey, TValue> : IObjectAccessor
    {
        public GenericDictionaryAccessor()
        {
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return ((IDictionary<TKey, TValue>)target).Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            foreach (var key in ((IDictionary<TKey, TValue>)target).Keys)
            {
                yield return context.ObjectToString(key);
            }
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object value, string member)
        {
            return ((IDictionary<TKey, TValue>) value).ContainsKey(TransformToKey(context, member));
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            TValue tvalue;
            var result = ((IDictionary<TKey, TValue>) target).TryGetValue(TransformToKey(context, member), out tvalue);
            value = tvalue;
            return result;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            ((IDictionary<TKey, TValue>) value)[TransformToKey(context, member)] = (TValue)value;
            return true;
        }

        private TKey TransformToKey(TemplateContext context, string member)
        {
            return (TKey)context.ToObject(new SourceSpan(), member, typeof(TKey));
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
        {
            if (((IDictionary<TKey, TValue>) target).TryGetValue((TKey) index, out var typedValue))
            {
                value = typedValue;
                return true;
            }
            value = null;
            return false;
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
        {
            ((IDictionary<TKey, TValue>) value)[(TKey)index] = (TValue)value;
            return true;
        }

        public bool HasIndexer => true;

        public Type IndexType => typeof(TKey);
    }
}