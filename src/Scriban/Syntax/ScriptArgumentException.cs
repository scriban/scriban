// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Syntax
{
    public class ScriptArgumentException : Exception
    {
        public ScriptArgumentException(int argumentIndex, string message) : base(message)
        {
            ArgumentIndex = argumentIndex;
        }
        public int ArgumentIndex { get; }
    }
}