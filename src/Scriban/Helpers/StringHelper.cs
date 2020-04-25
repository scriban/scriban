// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace Scriban.Helpers
{
    internal static class StringExtensions
    {
        public static string TrimEndKeepNewLine(this string text)
        {
            for (int i = text.Length - 1; i >= 0; i--)
            {
                var c = text[i];
                if (!char.IsWhiteSpace(c) || c == '\n')
                {
                    return text.Substring(0, i + 1);
                }
            }
            return text;
        }
    }
}