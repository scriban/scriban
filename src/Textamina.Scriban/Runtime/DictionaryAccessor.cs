// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System.Collections;

namespace Textamina.Scriban.Runtime
{
    class NullAccessor : IMemberAccessor
    {
        public static readonly NullAccessor Default = new NullAccessor();

        public bool HasMember(object target, string member)
        {
            return false;
        }

        public object GetValue(object target, string member)
        {
            return null;
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

        public bool HasMember(object value, string member)
        {
            return ((IDictionary) value).Contains(member);
        }

        public object GetValue(object target, string member)
        {
            return ((IDictionary) target)[member];
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
}