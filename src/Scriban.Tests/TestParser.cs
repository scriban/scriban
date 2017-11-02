// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    [TestFixture]
    public class TestParser
    {
        private const string RelativeBasePath = @"..\..\TestFiles";
        private const string InputFilePattern = "*.txt";
        private const string OutputEndFileExtension = ".out.txt";

        [Test]
        public void TestRoundtrip()
        {
            var text = "This is a text {{ code # With some comment }} and a text";
            AssertRoundtrip(text);
        }

        [Test]
        public void TestRoundtrip1()
        {
            var text = "This is a text {{ code | pipe a b c | a + b }} and a text";
            AssertRoundtrip(text);
        }

        [Test]
        public void RoundtripFunction()
        {
            var text = @"{{ func inc
    ret $0 + 1
end }}";
            AssertRoundtrip(text);
        }

        [Test]
        public void RoundtripFunction2()
        {
            var text = @"{{
func   inc
    ret $0 + 1
end
xxx 1
}}";
            AssertRoundtrip(text);
        }


        [Test]
        public void RoundtripIf()
        {
            var text = @"{{
if true
    ""yes""
end
}}
raw
";
            AssertRoundtrip(text);
        }

        [Test]
        public void RoundtripIfElse()
        {
            var text = @"{{
if true
    ""yes""
else
    ""no""
end
}}
raw
";
            AssertRoundtrip(text);
        }

        [Test]
        public void RoundtripIfElseIf()
        {
            var text = @"{{
if true
    ""yes""
else if yo
    ""no""
end
y
}}
raw
";
            AssertRoundtrip(text);
        }

        [Test]
        public void RoundtripCapture()
        {
            var text = @" {{ capture variable ~}}
    This is a capture
{{~ end ~}}
{{ variable }}";
            AssertRoundtrip(text);
        }


        [Test]
        public void RoundtripRaw()
        {
            var text = @"This is a raw     {{~ x ~}}     end";
            AssertRoundtrip(text);
        }

        [Test]
        public void TestDateNow()
        {
            // default is dd MM yyyy
            var dateNow = DateTime.Now.ToString("dd MMM yyyy", CultureInfo.CurrentCulture);
            var template = ParseTemplate(@"{{ date.now }}");
            var result = template.Render();
            Assert.AreEqual(dateNow, result);

            template = ParseTemplate(@"{{ date.format = '%Y'; date.now }}");
            result = template.Render();
            Assert.AreEqual(DateTime.Now.ToString("yyyy", CultureInfo.CurrentCulture), result);

            template = ParseTemplate(@"{{ date.format = '%Y'; date.now | date.add_years 1 }}");
            result = template.Render();
            Assert.AreEqual(DateTime.Now.AddYears(1).ToString("yyyy", CultureInfo.CurrentCulture), result);
        }

        [Test]
        public void TestHelloWorld()
        {
            var template = ParseTemplate(@"This is a {{ text }} World from scriban!");
            var result = template.Render(new { text = "Hello" });
            Assert.AreEqual("This is a Hello World from scriban!", result);
        }

        [Test]
        public void TestFrontMatter()
        {
            var options = new LexerOptions() {Mode = ScriptMode.FrontMatterAndContent};
            var input = @"+++
variable = 1
name = 'yes'
+++
This is after the frontmatter: {{ name }}
{{
variable + 1
}}";
            input = input.Replace("\r\n", "\n");
            var template = ParseTemplate(input, options);

            // Make sure that we have a front matter
            Assert.NotNull(template.Page.FrontMatter);

            var context = new TemplateContext();

            // Evaluate front-matter
            var frontResult = context.Evaluate(template.Page.FrontMatter);
            Assert.Null(frontResult);

            // Evaluate page-content
            context.Evaluate(template.Page);
            var pageResult = context.Output.ToString();
            TextAssert.AreEqual("This is after the frontmatter: yes\n2", pageResult);
        }


        [Test]
        public void TestFrontMatterOnly()
        {
            var options = new ParserOptions();

            var input = @"+++
variable = 1
name = 'yes'
+++
This is after the frontmatter: {{ name }}
{{
variable + 1
}}";
            input = input.Replace("\r\n", "\n");

            var lexer = new Lexer(input, null, new LexerOptions() { Mode = ScriptMode.FrontMatterOnly });
            var parser = new Parser(lexer, options);

            var page = parser.Run();
            foreach (var message in parser.Messages)
            {
                Console.WriteLine(message);
            }
            Assert.False(parser.HasErrors);

            // Check that the parser finished parsing on the first code exit }}
            // and hasn't tried to run the lexer on the remaining text
            Assert.AreEqual(new TextPosition(30, 3, 0), parser.CurrentSpan.Start);
            Assert.AreEqual(new TextPosition(33, 3, 3), parser.CurrentSpan.End);

            var startPositionAfterFrontMatter = parser.CurrentSpan.End.NextLine();

            // Make sure that we have a front matter
            Assert.NotNull(page.FrontMatter);
            Assert.AreEqual(0, page.Statements.Count);

            var context = new TemplateContext();

            // Evaluate front-matter
            var frontResult = context.Evaluate(page.FrontMatter);
            Assert.Null(frontResult);

            lexer = new Lexer(input, null, new LexerOptions() { StartPosition =  startPositionAfterFrontMatter });
            parser = new Parser(lexer);
            page = parser.Run();
            foreach (var message in parser.Messages)
            {
                Console.WriteLine(message);
            }
            Assert.False(parser.HasErrors);
            context.Evaluate(page);
            var pageResult = context.Output.ToString();
            TextAssert.AreEqual("This is after the frontmatter: yes\n2", pageResult);
        }

        [Test]
        public void TestScriptOnly()
        {
            var options = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
            var template = ParseTemplate(@"
variable = 1
name = 'yes'
", options);

            var context = new TemplateContext();

            template.Render(context);

            var outputStr = context.Output.ToString();
            Assert.AreEqual(string.Empty, outputStr);

            var global = context.CurrentGlobal;
            object value;
            Assert.True(global.TryGetValue("name", out value));
            Assert.AreEqual("yes", value);

            Assert.True(global.TryGetValue("variable", out value));
            Assert.AreEqual(1, value);
        }

        private static Template ParseTemplate(string text, LexerOptions? lexerOptions = null, ParserOptions? parserOptions = null)
        {
            var template = Template.Parse(text, "text", parserOptions, lexerOptions);
            foreach (var message in template.Messages)
            {
                Console.WriteLine(message);
            }
            Assert.False(template.HasErrors);
            return template;
        }

        [Test]
        public void TestFunctionCallInExpression()
        {
            var lexer = new Lexer(@"{{
with math
    round pi
end
}}");
            var parser = new Parser(lexer);

            var scriptPage = parser.Run();

            foreach (var message in parser.Messages)
            {
                Console.WriteLine(message);
            }
            Assert.False(parser.HasErrors);
            Assert.NotNull(scriptPage);

            var rootObject = new ScriptObject();
            rootObject.SetValue("math", ScriptObject.From(typeof(MathObject)), true);

            var context = new TemplateContext();
            context.PushGlobal(rootObject);
            context.Evaluate(scriptPage);
            context.PopGlobal();

            // Result
            var result = context.Output.ToString();

            Console.WriteLine(result);
        }

        [TestCaseSource("TestFiles")]
        public void Test(TestFilePath testFilePath)
        {
            var inputName = testFilePath.FilePath;

            var isSupportingExactRoundTrip = !NotSupportingExactRoundtrip.Contains(Path.GetFileName(inputName));

            var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));

            var inputFile = Path.Combine(baseDir, inputName);
            var inputText = File.ReadAllText(inputFile);

            var expectedOutputFile = Path.ChangeExtension(inputFile, OutputEndFileExtension);
            Assert.True(File.Exists(expectedOutputFile), $"Expecting output result file [{expectedOutputFile}] for input file [{inputName}]");
            var expectedOutputText = File.ReadAllText(expectedOutputFile, Encoding.UTF8);

            var isLiquid = inputName.Contains("liquid");

            AssertTemplate(inputText, isLiquid, expectedOutputText, false, isSupportingExactRoundTrip);
        }

        private void AssertRoundtrip(string inputText, bool isLiquid = false)
        {
            inputText = inputText.Replace("\r\n", "\n");
            AssertTemplate(inputText, isLiquid, inputText, true);
        }


        /// <summary>
        /// Lists of the tests that don't support exact byte-to-byte roundtrip (due to reformatting...etc.)
        /// </summary>
        private static readonly HashSet<string> NotSupportingExactRoundtrip = new HashSet<string>()
        {
            "003-whitespaces.txt",
            "010-literals.txt",
            "205-case-when-statement2.txt",
            "230-capture-statement2.txt",
            "470-html.txt"
        };

        private void AssertTemplate(string inputText, bool isLiquid, string expectedOutputText, bool isRoundTripTest = false, bool supportExactRoundtrip = true)
        {
            var parserOptions = new ParserOptions()
            {
                LiquidFunctionsToScriban = isLiquid
            };
            var lexerOptions = new LexerOptions()
            {
                Mode = isLiquid ? ScriptMode.Liquid : ScriptMode.Default
            };
            
            if (isRoundTripTest)
            {
                lexerOptions.KeepTrivia = true;
            }

            Console.WriteLine("Tokens");
            Console.WriteLine("======================================");
            var lexer = new Lexer(inputText, options: lexerOptions);
            foreach (var token in lexer)
            {
                Console.WriteLine($"{token.Type}: {token.GetText(inputText)}");
            }
            Console.WriteLine();

            string roundtripText = null;

            // We loop first on input text, then on rountrip
            while (true)
            {
                bool isRoundtrip = roundtripText != null;
                bool hasErrors = false;
                if (isRoundtrip)
                {
                    Console.WriteLine("Rountrip");
                    Console.WriteLine("======================================");
                    Console.WriteLine(roundtripText);
                    lexerOptions.Mode = ScriptMode.Default;

                    if (lexerOptions.Mode == ScriptMode.Default && !isLiquid && supportExactRoundtrip)
                    {
                        Console.WriteLine("Checking Exact Roundtrip - Input");
                        Console.WriteLine("======================================");
                        TextAssert.AreEqual(inputText, roundtripText);
                    }
                    inputText = roundtripText;
                }
                else
                {
                    Console.WriteLine("Input");
                    Console.WriteLine("======================================");
                    Console.WriteLine(inputText);
                }

                var template = Template.Parse(inputText, "text", parserOptions, lexerOptions);

                var result = string.Empty;
                if (template.HasErrors)
                {
                    hasErrors = true;
                    for (int i = 0; i < template.Messages.Count; i++)
                    {
                        var message = template.Messages[i];
                        if (i > 0)
                        {
                            result += "\n";
                        }
                        result += message;
                    }
                }
                else
                {
                    if (isRoundTripTest)
                    {
                        result = template.ToText();
                    }
                    else
                    {
                        Assert.NotNull(template.Page);

                        if (!isRoundtrip)
                        {
                            // Dumps the rountrip version
                            var lexerOptionsForTrivia = lexerOptions;
                            lexerOptionsForTrivia.KeepTrivia = true;
                            var templateWithTrivia = Template.Parse(inputText, "input",  parserOptions, lexerOptionsForTrivia);
                            roundtripText = templateWithTrivia.ToText();
                        }

                        try
                        {
                            object model = null;

                            // Setup a default liquid context for the tests, as we can't create object/array in liquid directly
                            if (isLiquid)
                            {
                                var liquidContext = new ScriptObject
                                {
                                    ["page"] = new ScriptObject {["title"] = "This is a title"},
                                    ["user"] = new ScriptObject {["name"] = "John"},
                                    ["product"] = new ScriptObject {["title"] = "Orange Hello World", ["type"] = "fruit"},
                                    ["products"] = new ScriptArray()
                                    {
                                        new ScriptObject {["title"] = "Orange Hello World", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Banana Hello World", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Apple Hello World", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Item1 Hello World", ["type"] = "item"},
                                        new ScriptObject {["title"] = "Item2 Hello World", ["type"] = "item"},
                                        new ScriptObject {["title"] = "Item3 Hello World", ["type"] = "item"},
                                        new ScriptObject {["title"] = "Item4 Hello World", ["type"] = "item"},
                                    }
                                };
                                model = liquidContext;
                            }

                            result = template.Render(model);
                        }
                        catch (ScriptRuntimeException exception)
                        {
                            result = GetReason(exception);
                        }
                    }
                }

                var testContext = isRoundtrip ? "Roundtrip - " : String.Empty;
                Console.WriteLine($"{testContext}Result");
                Console.WriteLine("======================================");
                Console.WriteLine(result);
                Console.WriteLine($"{testContext}Expected");
                Console.WriteLine("======================================");
                Console.WriteLine(expectedOutputText);

                TextAssert.AreEqual(expectedOutputText, result);

                if (isRoundTripTest || isRoundtrip || hasErrors)
                {
                    break;
                }
            }
        }



        private static string GetReason(Exception ex)
        {
            var text = new StringBuilder();
            while (ex != null)
            {
                text.Append(ex);
                if (ex.InnerException != null)
                {
                    text.Append(". Reason: ");
                }
                ex = ex.InnerException;
            }
            return text.ToString();
        }

        public static IEnumerable<object[]> TestFiles
        {
            get
            {
                var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));
                return
                    Directory.EnumerateFiles(baseDir, InputFilePattern, SearchOption.AllDirectories)
                        .Where(f => !f.EndsWith(OutputEndFileExtension))
                        .Select(f => f.StartsWith(baseDir) ? f.Substring(baseDir.Length + 1) : f)
                        .OrderBy(f => f)
                        .Select(x => new object[]
                        {
                            new TestFilePath(x)
                        });
            }
        }

        /// <summary>
        /// Use an internal class to have a better display of the filename in Resharper Unit Tests runner.
        /// </summary>
        public struct TestFilePath
        {
            public TestFilePath(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }

            public override string ToString()
            {
                return FilePath;
            }
        }

        private static string BaseDirectory
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var codebase = new Uri(assembly.CodeBase);
                var path = codebase.LocalPath;
                return Path.GetDirectoryName(path);
            }
        }
    }
}