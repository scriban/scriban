// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Runtime
{
    [AttributeUsage(AttributeTargets.Field| AttributeTargets.Property|AttributeTargets.Method)]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ScriptMemberIgnoreAttribute : Attribute
    {

    }
}