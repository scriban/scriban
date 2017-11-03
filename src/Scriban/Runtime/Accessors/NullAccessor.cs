// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
    public class NullAccessor : IObjectAccessor
    {
        public static readonly NullAccessor Default = new NullAccessor();

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return 0;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            yield break;
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return false;
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;
            return false;
        }
       
        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            return false;
        }
    }
}