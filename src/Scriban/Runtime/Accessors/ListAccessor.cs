// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ListAccessor : IListAccessor, IObjectAccessor
    {
        public static ListAccessor Default = new ListAccessor();

        private ListAccessor()
        {
        }

        public int GetLength(TemplateContext context, SourceSpan span, object target)
        {
            return ((IList) target).Count;
        }

        public object GetValue(TemplateContext context, SourceSpan span, object target, int index)
        {
            var list = ((IList)target);
            if (index < 0 || index >= list.Count)
            {
                return null;
            }

            return list[index];
        }

        public void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value)
        {
            var list = ((IList) target);
            if (index < 0)
            {
                return;
            }
            // Auto-expand the array in case of accessing a range outside the current value
            for (int i = list.Count; i <= index; i++)
            {
                // TODO: If the array doesn't support null value, we shoud add a default value or throw an error?
                list.Add(null);
            }

            list[index] = value;
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            // size
            return 1;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            yield return "size";
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return member == "size";
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            if (member == "size")
            {
                value = GetLength(context, span, target);
                return true;
            }
            if (target is IScriptObject)
            {
                return (((IScriptObject) target)).TryGetValue(context, span, member, out value);
            }

            value = null;
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            if (member == "size")
            {
                return false;
            }
            if (target is IScriptObject)
            {
                return (((IScriptObject)target)).TrySetValue(context, span, member, value, false);
            }
            return false;
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
        {
            throw new System.NotImplementedException();
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
        {
            throw new System.NotImplementedException();
        }

        public bool HasIndexer => false;

        public Type IndexType => null;
    }
}