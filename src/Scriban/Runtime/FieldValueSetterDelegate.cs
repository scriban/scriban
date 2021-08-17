// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to override the field value that will be added to script object during import.
    /// </summary>
    /// <param name="info">A field info</param>
    /// <param name="obj">The object that the value should be retrieved from</param>
    /// <returns>The retult of getting field value from the object</returns>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    delegate object? FieldValueSetterDelegate(FieldInfo info, object obj);
}