// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// The standard rename make a camel/pascalcase name changed by `_` and lowercase. e.g `ThisIsAnExample` becomes `this_is_an_example`.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed class StandardPropertyValueSetter
    {
        public static readonly PropertyValueSetterDelegate Default = Set;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object? Set(PropertyInfo info, object obj) => info?.GetValue(obj);
    }
}