// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
    public class ScriptObjectAccessor : IObjectAccessor
    {
        public static readonly IObjectAccessor Default = new ScriptObjectAccessor();

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return ((IScriptObject)target).Contains(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            return ((IScriptObject)target).TryGetValue(context, span, member, out value);
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            return ((IScriptObject)target).TrySetValue(member, value, false);
        }
    }
}