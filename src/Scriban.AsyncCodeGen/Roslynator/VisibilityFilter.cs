// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Roslynator
{
    [Flags]
    internal enum VisibilityFilter
    {
        None = 0,
        Public = 1,
        Internal = 2,
        Private = 4,
        All = Public | Internal | Private
    }
}