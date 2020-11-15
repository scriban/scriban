// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NUnit.Framework;
using Scriban.Syntax;

namespace Scriban.Tests
{
    public class TestStringSlice
    {
        private static readonly string[] StringsToCompare = new string[]
        {
            null,
            "",
            "A",
            "ABCDEF",
            "BC",
            "Ad",
            "ABCDEFGHJIKL",
        };

        [TestCase("A")]
        [TestCase("ABCDEF")]
        public void TestMixed(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                var subText = text?.Substring(i);
                var slice = text.Slice(i);
                CompareString(subText, slice);
            }
        }

        [TestCase("     A    \n   B       ", 0, 6)]
        [TestCase("\n\n\n\n\n", 0, 3)]
        [TestCase("\n \n \n \n \n", 0, 3)]
        public void TestTrim(string text, int index, int length)
        {
            for (int i = 0; i < text.Length - 1; i++)
            {
                for (int j = 1; j < length && i + j < text.Length; j++)
                {
                    var subText = text.Substring(index, length);
                    var slice = text.Slice(index, length);

                    var newSlice = slice.TrimEnd();
                    var newSubText = subText.TrimEnd();

                    Assert.AreEqual(newSubText, newSlice.ToString());
                }
            }
        }

        [Test]
        public void TestEmpty()
        {
            CompareString(string.Empty, ScriptStringSlice.Empty);
        }

        private static void CompareString(string subText, ScriptStringSlice slice)
        {
            Assert.AreEqual(subText, slice.ToString());

            Assert.AreEqual(subText.Length, slice.Length);

            if (slice.Length > 0)
            {
                Assert.NotZero(slice.GetHashCode());
            }

            Assert.True(subText == slice, "String not comparing correctly: Expecting: {subText}, Result: {slice}");

            for (int j = 0; j < slice.Length; j++)
            {
                Assert.AreEqual(subText[j], slice[j]);
            }

            foreach (var compare in StringsToCompare)
            {
                {
                    var result = subText.CompareTo(compare);
                    var sliceResult = slice.CompareTo(compare);
                    Assert.AreEqual(result, sliceResult);
                }

                Assert.AreEqual(subText == compare, slice == compare);
                Assert.AreEqual(subText == compare, compare == slice);
            }
        }
    }
}