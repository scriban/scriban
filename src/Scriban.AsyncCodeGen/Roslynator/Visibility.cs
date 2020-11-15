// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    /// <summary>
    /// Specifies visibility of a symbol.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// No visibility specified.
        /// </summary>
        NotApplicable = 0,

        /// <summary>
        /// Symbol is privately visible.
        /// </summary>
        Private = 1,

        /// <summary>
        /// Symbol is internally visible.
        /// </summary>
        Internal = 2,

        /// <summary>
        /// Symbol is publicly visible.
        /// </summary>
        Public = 3,
    }
}