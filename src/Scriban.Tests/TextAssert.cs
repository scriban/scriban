// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace Scriban.Tests
{
    /// <summary>
    /// Pretty text assert from https://gist.github.com/Haacked/1610603
    /// Modified version to only print +-10 characters around the first diff
    /// </summary>
    public static class TextAssert
    {
        public enum DiffStyle
        {
            Full,
            Minimal
        }

        public static string Normalize(string text) => text.Replace("\r\n", "\n");

        public static void AreEqual(string expected, string actual)
        {
            AreEqual(expected, actual, DiffStyle.Full, Console.Out);
        }

        public static void AreEqual(string expected, string actual, DiffStyle diffStyle)
        {
            AreEqual(expected, actual, diffStyle, Console.Out);
        }

        public static void AreEqual(string expected, string actual, DiffStyle diffStyle, TextWriter output)
        {
            if (actual == null || expected == null)
            {
                Assert.AreEqual(expected, actual);
                return;
            }

            if (actual.Equals(expected, StringComparison.Ordinal))
            {
                return;
            }

            Console.WriteLine();
            output.WriteLine("Index    Expected     Actual");
            output.WriteLine("----------------------------");
            int maxLen = Math.Max(actual.Length, expected.Length);
            int minLen = Math.Min(actual.Length, expected.Length);

            if (diffStyle != DiffStyle.Minimal)
            {
                int startDifferAt = 0;
                for (int i = 0; i < maxLen; i++)
                {
                    if (i >= minLen || actual[i] != expected[i])
                    {
                        startDifferAt = i;
                        break;
                    }
                }

                var endDifferAt = Math.Min(startDifferAt + 10, maxLen);
                startDifferAt = Math.Max(startDifferAt - 10, 0);

                bool isFirstDiff = true;
                for (int i = startDifferAt; i < endDifferAt; i++)
                {
                    if (i >= minLen || actual[i] != expected[i])
                    {
                        output.WriteLine("{0,-3} {1,-3}    {2,-4} {3,-3}   {4,-4} {5,-3}",
                            i < minLen && actual[i] == expected[i] ? " " : isFirstDiff  ? ">>>": "***",
                            // put a mark beside a differing row
                            i, // the index
                            i < expected.Length ? ((int) expected[i]).ToString() : "",
                            // character decimal value
                            i < expected.Length ? expected[i].ToSafeString() : "", // character safe string
                            i < actual.Length ? ((int) actual[i]).ToString() : "", // character decimal value
                            i < actual.Length ? actual[i].ToSafeString() : "" // character safe string
                            );

                        isFirstDiff = false;
                    }
                }
                //output.WriteLine();
            }

            Console.WriteLine("Actual");
            Console.WriteLine("-----------");
            Console.WriteLine(actual);
            Console.WriteLine();
            Console.WriteLine("Expected");
            Console.WriteLine("-----------");
            Console.WriteLine(expected);
            Assert.AreEqual(expected, actual);
        }

        private static string ToSafeString(this char c)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
            {
                switch (c)
                {
                    case '\b':
                        return @"\b";
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\t':
                        return @"\t";
                    case '\a':
                        return @"\a";
                    case '\v':
                        return @"\v";
                    case '\f':
                        return @"\f";
                    default:
                        return $"\\u{(int) c:X};";
                }
            }
            return c.ToString(CultureInfo.InvariantCulture);
        }
    }
}