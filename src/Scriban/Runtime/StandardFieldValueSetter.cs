// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Reflection;

namespace Scriban.Runtime
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed class StandardFieldValueSetter
    {
        public static readonly FieldValueSetterDelegate Default = Set;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object? Set(FieldInfo info, object obj) => info?.GetValue(obj);
    }
}