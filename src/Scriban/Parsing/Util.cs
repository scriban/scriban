// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Parsing
{
    internal static class Util
    {
        public static bool IsHex(this char c)
        {
            return c is
                   >= '0' and <= '9' or
                   >= 'a' and <= 'f' or
                   >= 'A' and <= 'F';
        }

        public static int HexToInt(this char c)
        {
            switch (c)
            {
                case >= '0' and <= '9':
                    return c - '0';
                case >= 'a' and <= 'f':
                    return c - 'a' + 10;
                case >= 'A' and <= 'F':
                    return c - 'A' + 10;
                default:
                    // Don't throw an exception as we are checking and logging an error if IsHex is false already
                    return 0;
            }
        }
    }
}