// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    public interface IScriptConvertible
    {
        object ConvertTo(SourceSpan span, Type type);
    }
}