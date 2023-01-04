// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

            [Test]
            public void TestContains()
            {
                var mixed = new object[] { "hi", 1, TestEnum.First };
                Assert.True(ArrayFunctions.Contains(mixed, "hi"));
                Assert.True(ArrayFunctions.Contains(mixed, 1));
                Assert.True(ArrayFunctions.Contains(mixed, TestEnum.First));
                Assert.True(ArrayFunctions.Contains(mixed, "First"));
                Assert.True(ArrayFunctions.Contains(mixed, 100));
                Assert.False(ArrayFunctions.Contains(mixed, TestEnum.Second));
                Assert.False(ArrayFunctions.Contains(mixed, 101));
                Assert.False(ArrayFunctions.Contains(mixed, "Third"));
                Assert.False(ArrayFunctions.Contains(null, 1));
                TestParser.AssertTemplate("true", "{{ value | array.contains 'First' }}", model: new ObjectModel { Value = mixed });
                TestParser.AssertTemplate("true", "{{ value | array.contains 100 }}", model: new ObjectModel { Value = mixed });
                TestParser.AssertTemplate("false", "{{ value | array.contains 'Second' }}", model: new ObjectModel { Value = mixed });
                TestParser.AssertTemplate("false", "{{ value | array.contains 101 }}", model: new ObjectModel { Value = mixed });
                TestParser.AssertTemplate("false", "{{ value | array.contains 'Third' }}", model: new ObjectModel { Value = mixed });
                TestParser.AssertTemplate("false", "{{ null | array.contains 100 }}");
            }
            class ObjectModel
            {
                public object[] Value { get; set; }
            }

            enum TestEnum : int
            {
                First = 100,
                Second = 101
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

            public record IndexOfTestCase(
                string Text,
                string Search,
                int Expected,
                int? StartIndex = null,
                int? Count = null,
                StringComparison? StringComparison = null
            );

            public static readonly IReadOnlyList<IndexOfTestCase> IndexOfTestCases = new IndexOfTestCase[]
            {
                new ("The quick brown fox", "quick", 4),
                new ("The the the the", "the", 0, null, null, StringComparison.OrdinalIgnoreCase),
                new ("The quick brown fox", "quick", -1, null, 2),
                new ("The the the the", "the", 8, 6),
            };

            [Test]
            [TestCaseSource(nameof(IndexOfTestCases))]
            public void TestIndexOf(IndexOfTestCase testCase)
            {
                testCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
                var args = new[]
                {
                    (Name: "text", Value: MakeString(testCase.Text)),
                    (Name: "search", Value: MakeString(testCase.Search)),
                    (Name: "start_index", Value: MakeInt(testCase.StartIndex)),
                    (Name: "count", Value: MakeInt(testCase.Count)),
                    (Name: "string_comparison", Value: MakeString(testCase.StringComparison?.ToString())),
                }.Select(x => $"{x.Name}: {x.Value}");
                var script = $@"{{{{ string.index_of {string.Join(" ", args)} }}}}";
                Template template = null;
                Assert.DoesNotThrow(() => template = Template.Parse(script));
                Assert.That(template, Is.Not.Null);
                Assert.That(template.HasErrors, Is.False);
                Assert.That(template.Messages, Is.Not.Null);
                Assert.That(template.Messages.Count, Is.EqualTo(0));
                var result = template.Render();
                Assert.That(result, Is.EqualTo(testCase.Expected.ToString()));

                static string MakeString(string value) => value is null ? "null" : $"'{value}'";
                static string MakeInt(int? value) => value is null ? "null" : value.ToString();
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
                // nothing to replace
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