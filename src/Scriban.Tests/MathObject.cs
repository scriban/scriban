// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;

namespace Scriban.Tests
{
    public static class MathObject
    {
        public const double PI = Math.PI;

        public static double Cos(double value)
        {
            return Math.Cos(value);
        }

        public static double Sin(double value)
        {
            return Math.Sin(value);
        }

        public static double Round(double value)
        {
            return Math.Round(value);
        }
    }
}