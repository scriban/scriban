// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using NUnit.Framework;
using Scriban.Functions;

namespace Scriban.Tests
{
    public class TestFunctions
    {
        public class Arrays
        {
            [Test]
            public void TestOffset()
            {
                Assert.Null(ArrayFunctions.Offset(null, 0));
            }

            [Test]
            public void TestLimit()
            {
                Assert.Null(ArrayFunctions.Limit(null, 0));
            }

            [Test]
            public void TestSortError()
            {
                TestParser.AssertTemplate("text(1,19) : error : Unexpected number of arguments `0` for sort. Expecting at least 1 parameter <property>? <array>", "{{ [1,2] || array.sort }}");
            }
        }

        public class Strings
        {
            [Test]
            public void TestSliceError()
            {
                TestParser.AssertTemplate("text(1,11) : error : Unexpected number of arguments `0` for slice. Expecting at least 2 parameters <start> <length>? <text>", "{{ string.slice }}");
            }
            [Test]
            public void TestSliceAtError()
            {
                TestParser.AssertTemplate("text(1,11) : error : Unexpected number of arguments `0` for slice1. Expecting at least 2 parameters <start> <length>? <text>", "{{ string.slice1 }}");
            }
        }
    }
}