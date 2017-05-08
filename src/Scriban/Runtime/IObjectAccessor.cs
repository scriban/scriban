// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Runtime
{
    public interface IObjectAccessor
    {
        bool HasMember(object target, string member);

        bool TryGetValue(object target, string member, out object value);

        bool TrySetValue(object target, string member, object value);
    }
}