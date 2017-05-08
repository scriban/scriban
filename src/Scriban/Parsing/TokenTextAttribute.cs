// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Parsing
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class TokenTextAttribute : Attribute
    {
        public TokenTextAttribute(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}