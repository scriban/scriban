// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Runtime
{
    [Flags]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptMemberImportFlags
    {
        Field = 1,

        Property = 2,

        Method = 4,

        [Obsolete("Importing Method Instance is actually not supported - This flag will be removed in a future release")]
        MethodInstance = 8,

        All = Field | Property | Method
    }
}