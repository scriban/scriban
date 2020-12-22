// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    /// <summary>
    /// Implicit multiplication 1/2x are interpreted by 1/(2*x), but if
    /// x implements this interface, it will be interpreted as 1/2 * x
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptCustomImplicitMultiplyPrecedence
    {
    }
}