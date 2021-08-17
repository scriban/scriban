// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// The standard property value setter is invoking GetValue method on the PropertyInfo object and return the function result.
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
        /// Get the value of the property for the object passed
        /// </summary>
        /// <param name="info">The PropertyInfo</param>
        /// <param name="obj">The object</param>
        /// <returns>retult of GetValue method from PropertyInfo using object as parameter</returns>
        public static object? Set(PropertyInfo info, object obj) => info?.GetValue(obj);
    }
}