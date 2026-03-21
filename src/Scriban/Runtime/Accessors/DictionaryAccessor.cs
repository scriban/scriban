// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

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


        [RequiresDynamicCode("Creating generic dictionary accessors requires MakeGenericType.")]
        [RequiresUnreferencedCode("Discovering generic dictionary interfaces requires reflection.")]
        public static bool TryGet(object target, out IObjectAccessor? accessor)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (target is IDictionary<string, object?>)
            {
                accessor = DictionaryStringObjectAccessor.Default;
                return true;
            }

            var type = target.GetType();
            var dictionaryType = type.GetBaseOrInterface(typeof(IDictionary<,>));
            if (dictionaryType is null)
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
            accessor = (IObjectAccessor?)Activator.CreateInstance(accessorType);
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
                yield return context.ObjectToString(key) ?? string.Empty;
            }
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return ((IDictionary) target).Contains(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object? value)
        {
            value = null;
            if (((IDictionary) target).Contains(member))
            {
                value = ((IDictionary)target)[member];
                return true;
            }
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object? value)
        {
            ((IDictionary) target)[member] = value;
            return true;
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object? value)
        {
            value = null;
            if (((IDictionary) target).Contains(index))
            {
                value = ((IDictionary)target)[index];
                return true;
            }
            return false;
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object? value)
        {
            ((IDictionary) target)[index] = value;
            return true;
        }

        public bool HasIndexer => true;

        public Type IndexType => typeof(object);
    }

    class DictionaryStringObjectAccessor : GenericDictionaryAccessor<string, object?>
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
                yield return context.ObjectToString(key) ?? string.Empty;
            }
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object value, string member)
        {
            return ((IDictionary<TKey, TValue>) value).ContainsKey(TransformToKey(context, member));
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object? value)
        {
            var result = ((IDictionary<TKey, TValue>) target).TryGetValue(TransformToKey(context, member), out var typedValue);
            value = typedValue;
            return result;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object? value)
        {
            ((IDictionary)target)[TransformToKey(context, member)!] = ConvertValue(context, span, value);
            return true;
        }

        private TKey TransformToKey(TemplateContext context, string member)
        {
            var convertedKey = context.ToObject(new SourceSpan(), member, typeof(TKey));
            if (convertedKey is TKey typedKey)
            {
                return typedKey;
            }

            throw new InvalidOperationException($"Unable to convert member `{member}` to dictionary key type `{typeof(TKey)}`.");
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object? value)
        {
            if (((IDictionary<TKey, TValue>) target).TryGetValue((TKey) index, out var typedValue))
            {
                value = typedValue;
                return true;
            }
            value = null;
            return false;
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object? value)
        {
            ((IDictionary)target)[index] = ConvertValue(context, span, value);
            return true;
        }

        public bool HasIndexer => true;

        public Type IndexType => typeof(TKey);

        [return: MaybeNull]
        private static TValue ConvertValue(TemplateContext context, SourceSpan span, object? value)
        {
            var convertedValue = context.ToObject(span, value, typeof(TValue));
            if (convertedValue is TValue typedValue)
            {
                return typedValue;
            }

            if (convertedValue is null && default(TValue) is null)
            {
                return default;
            }

            throw new ScriptRuntimeException(span, $"Unable to convert dictionary value to `{typeof(TValue)}`.");
        }
    }
}
