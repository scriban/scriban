// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Runtime.CompilerServices;

namespace Scriban.Helpers
{
    /// <summary>
    /// Internal helper to allow to declare a method using AggressiveInlining without being .NET 4.0+
    /// </summary>
    internal static class MethodImplOptionsPortable
    {
        public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;
    }
}