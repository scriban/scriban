// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scriban.Runtime
{
    public interface IMemberAccessor
    {
        bool HasMember(object target, string member);

        bool TryGetValue(object target, string member, out object value);

        bool HasReadonly { get; }

        bool TrySetValue(object target, string member, object value);

        void SetReadOnly(object target, string member, bool isReadOnly);
    }
}