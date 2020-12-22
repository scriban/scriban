// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Reflection;

namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to filter a member while importing a .NET object into a ScriptObject or while exposing a .NET instance through a ScriptObject, by returning <c>true</c> to keep the member; or false to discard it.
    /// </summary>
    /// <param name="member">A member info</param>
    /// <returns><c>true</c> to keep the member; otherwise <c>false</c> to remove the member</returns>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    delegate bool MemberFilterDelegate(MemberInfo member);
}