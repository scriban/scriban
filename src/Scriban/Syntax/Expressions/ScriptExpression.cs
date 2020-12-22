// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    /// <summary>
    /// Base class for all expressions.
    /// </summary>
    /// <seealso cref="ScriptNode" />
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract partial class ScriptExpression : ScriptNode
    {
    }
}