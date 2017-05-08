// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Runtime.Accessors
{
    public class NullAccessor : IObjectAccessor
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
       
        public bool TrySetValue(object target, string member, object value)
        {
            return false;
        }
    }
}