// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Can apply a transform to each element (e.g ScriptArray.Transform(...))
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptTransformable
    {
        Type ElementType { get; }

        bool CanTransform(Type transformType);

        bool Visit(TemplateContext context, SourceSpan span, Func<object, bool> visit);

        object Transform(TemplateContext context, SourceSpan span, Func<object, object> apply, Type destType);
    }
}