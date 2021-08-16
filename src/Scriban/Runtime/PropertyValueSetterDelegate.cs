// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to rename a member.
    /// </summary>
    /// <param name="info">A member info</param>
    /// <param name="obj">A member info</param>
    /// <returns>The new name name of member</returns>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    delegate object? PropertyValueSetterDelegate(PropertyInfo info, object obj);
}