// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptObjectAccessor : IObjectAccessor
    {
        public static readonly IObjectAccessor Default = new ScriptObjectAccessor();

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return (target as IScriptObject).Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            return (target as IScriptObject).GetMembers();
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return (target as IScriptObject).Contains(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            return (target as IScriptObject).TryGetValue(context, span, member, out value);
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            return (target as IScriptObject).TrySetValue(context, span, member, value, false);
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
        {
            throw new NotImplementedException();
        }

        public bool HasIndexer => false;

        public Type IndexType => null;
    }
}