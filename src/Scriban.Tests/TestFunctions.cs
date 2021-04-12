// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
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
            public void TestSortNoError()
            {
                TestParser.AssertTemplate("true", "{{ [1,2] || array.sort }}");
            }
        }

        public class Strings
        {
            [Test]
            public void TestSliceError()
            {
                TestParser.AssertTemplate("text(1,16) : error : Invalid number of arguments `0` passed to `string.slice` while expecting `2` arguments", "{{ string.slice }}");
            }
            [Test]
            public void TestSliceAtError()
            {
                TestParser.AssertTemplate("text(1,17) : error : Invalid number of arguments `0` passed to `string.slice1` while expecting `2` arguments", "{{ string.slice1 }}");
            }

            [Test]
            [TestCaseSource(nameof(TestReplaceFirstArguments))]
            public void TestReplaceFirst(string source, string match, string replace, bool fromend, string expected)
            {
                var script = @"{{source | string.replace_first  match replace fromend}}";
                var template = Template.Parse(script);
                var result = template.Render(new
                {
                    Source = source,
                    Match = match,
                    Replace = replace,
                    Fromend = fromend,
                });
                Assert.AreEqual(result, expected);
            }

            static readonly object[] TestReplaceFirstArguments =
            {
                // Replace from start
                new object [] {
                    "Hello, world. Goodbye, world.",    // source
                    "world",                            // match
                    "buddy",                            // replace
                    false,                              // fromEnd
                    "Hello, buddy. Goodbye, world.",    // expected
                },
                // Replace from end
                new object [] {
                    "Hello, world. Goodbye, world.",    // source
                    "world",                            // match
                    "buddy",                            // replace
                    true,                               // fromEnd
                    "Hello, world. Goodbye, buddy.",    // expected
                },
                // notghing to replace
                new object [] {
                    "Hello, world. Goodbye, world.",    // source
                    "xxxx",                             // match
                    "buddy",                            // replace
                    false,                              // fromEnd
                    "Hello, world. Goodbye, world.",    // expected
                },
            };
        }

        public class Math
        {
            [Test]
            public void TestMathUuid()
            {
                var script = @"{{math.uuid}}";
                var template = Template.Parse(script);
                var result = template.Render();
                Assert.IsTrue(Guid.TryParse(result, out var _));
            }

            [Test]
            public void TestMathRandom()
            {
                var script = @"{{math.random 1 10}}";
                var template = Template.Parse(script);
                var result = template.Render();
                Assert.IsTrue(int.TryParse(result, out var number));
                Assert.IsTrue(number < 10 && number >= 1);
            }

            [Test]
            public void TestMathRandomError()
            {
                TestParser.AssertTemplate("text(1,4) : error : minValue must be greater than maxValue", "{{ math.random 11 10 }}");
            }
        }
    }
}