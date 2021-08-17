// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// The standard field value setter is invoking GetValue method on the FieldInfo object and return the function result.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed class StandardFieldValueSetter
    {
        public static readonly FieldValueSetterDelegate Default = Set;

        /// <summary>
        /// Get the value of the field for the object passed
        /// </summary>
        /// <param name="info">The FieldInfo</param>
        /// <param name="obj">The object</param>
        /// <returns>retult of GetValue method from FieldInfo using object as parameter</returns>
        public static object? Set(FieldInfo info, object obj) => info?.GetValue(obj);
    }
}