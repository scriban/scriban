// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Runtime.Accessors
{
    public class ScriptObjectAccessor : IObjectAccessor
    {
        public static readonly IObjectAccessor Default = new ScriptObjectAccessor();

        public bool HasMember(object target, string member)
        {
            return ((IScriptObject)target).Contains(member);
        }

        public bool TryGetValue(object target, string member, out object value)
        {
            return ((IScriptObject)target).TryGetValue(member, out value);
        }

        public bool TrySetValue(object target, string member, object value)
        {
            return ((IScriptObject)target).TrySetValue(member, value, false);
        }
    }
}