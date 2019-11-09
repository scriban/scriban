// #define EnableTokensOutput
// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif
using DotLiquid.Tests.Tags;
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
        private const string RelativeBasePath = @"..\..\..\TestFiles";
        private const string BuiltinMarkdownDocFile = @"..\..\..\..\..\doc\builtins.md";
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
        public void TestLiquidMissingClosingBrace()
        {
            var template = Template.ParseLiquid("{%endunless");
            Assert.True(template.HasErrors);
            Assert.AreEqual(1, template.Messages.Count);
            Assert.AreEqual("<input>(1,3) : error : Unable to find a pending `unless` for this `endunless`", template.Messages[0].ToString());
        }

        [Test]
        public void TestLiquidInvalidStringEscape()
        {
            var template = Template.ParseLiquid(@"{%""\u""");
            Assert.True(template.HasErrors);
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
            var text = @" {{ capture variable -}}
    This is a capture
{{- end -}}
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
            var dateNow = DateTime.Now.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            var template = ParseTemplate(@"{{ date.now }}");
            var result = template.Render();
            Assert.AreEqual(dateNow, result);

            template = ParseTemplate(@"{{ date.format = '%Y'; date.now }}");
            result = template.Render();
            Assert.AreEqual(DateTime.Now.ToString("yyyy", CultureInfo.InvariantCulture), result);

            template = ParseTemplate(@"{{ date.format = '%Y'; date.now | date.add_years 1 }}");
            result = template.Render();
            Assert.AreEqual(DateTime.Now.AddYears(1).ToString("yyyy", CultureInfo.InvariantCulture), result);
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
            Assert.Null(page.Body);

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

        [TestCaseSource("ListTestFiles", new object[] { "000-basic" }, Category= "Basic")]
        public static void A000_basic(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListTestFiles", new object[] { "010-literals" }, Category = "Basic")]
        public static void A010_literals(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListTestFiles", new object[] { "100-expressions" }, Category = "Basic")]
        public static void A100_expressions(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListTestFiles", new object[] { "200-statements" }, Category = "Basic")]
        public static void A200_statements(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListTestFiles", new object[] { "300-functions" }, Category = "Basic")]
        public static void A300_functions(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListTestFiles", new object[] { "400-builtins" }, Category = "Basic")]
        public static void A400_builtins(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListTestFiles", new object[] { "500-liquid" }, Category = "Basic")]
        public static void A500_liquid(string inputName)
        {
            TestFile(inputName);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "array" })]
        public static void Doc_array(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "date" })]
        public static void Doc_date(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "html" })]
        public static void Doc_html(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "math" })]
        public static void Doc_math(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "object" })]
        public static void Doc_object(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "regex" })]
        public static void Doc_regex(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "string" })]
        public static void Doc_string(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        [TestCaseSource("ListBuiltinFunctionTests", new object[] { "timespan" })]
        public static void Doc_timespan(string inputName, string input, string output)
        {
            AssertTemplate(output, input);
        }

        private static void TestFile(string inputName)
        {
            var filename = Path.GetFileName(inputName);
            var isSupportingExactRoundtrip = !NotSupportingExactRoundtrip.Contains(filename);

            var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));

            var inputFile = Path.Combine(baseDir, inputName);
            var inputText = File.ReadAllText(inputFile);

            var expectedOutputFile = Path.ChangeExtension(inputFile, OutputEndFileExtension);
            Assert.True(File.Exists(expectedOutputFile), $"Expecting output result file `{expectedOutputFile}` for input file `{inputName}`");
            var expectedOutputText = File.ReadAllText(expectedOutputFile, Encoding.UTF8);

            var isLiquid = inputName.Contains("liquid");

            AssertTemplate(expectedOutputText, inputText, isLiquid, false, isSupportingExactRoundtrip, expectParsingErrorForRountrip: filename == "513-liquid-statement-for.variables.txt");
        }

        private void AssertRoundtrip(string inputText, bool isLiquid = false)
        {
            inputText = inputText.Replace("\r\n", "\n");
            AssertTemplate(inputText, inputText, isLiquid, true);
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

        public static void AssertTemplate(string expected, string input, bool isLiquid = false, bool isRoundtripTest = false, bool supportExactRoundtrip = true, object model = null, bool specialLiquid = false, bool expectParsingErrorForRountrip = false)
        {
            var parserOptions = new ParserOptions()
            {
                LiquidFunctionsToScriban = isLiquid
            };
            var lexerOptions = new LexerOptions()
            {
                Mode = isLiquid ? ScriptMode.Liquid : ScriptMode.Default
            };

            if (isRoundtripTest)
            {
                lexerOptions.KeepTrivia = true;
            }

            if (specialLiquid)
            {
                parserOptions.ExpressionDepthLimit = 500;
            }

#if EnableTokensOutput
            {
                Console.WriteLine("Tokens");
                Console.WriteLine("======================================");
                var lexer = new Lexer(input, options: lexerOptions);
                foreach (var token in lexer)
                {
                    Console.WriteLine($"{token.Type}: {token.GetText(input)}");
                }
                Console.WriteLine();
            }
#endif
            string roundtripText = null;

            // We loop first on input text, then on roundtrip
            while (true)
            {
                bool isRoundtrip = roundtripText != null;
                bool hasErrors = false;
#if SCRIBAN_ASYNC
                bool hasException = false;
#endif
                if (isRoundtrip)
                {
                    Console.WriteLine("Roundtrip");
                    Console.WriteLine("======================================");
                    Console.WriteLine(roundtripText);
                    lexerOptions.Mode = ScriptMode.Default;

                    if (lexerOptions.Mode == ScriptMode.Default && !isLiquid && supportExactRoundtrip)
                    {
                        Console.WriteLine("Checking Exact Roundtrip - Input");
                        Console.WriteLine("======================================");
                        TextAssert.AreEqual(input, roundtripText);
                    }
                    input = roundtripText;
                }
                else
                {
                    Console.WriteLine("Input");
                    Console.WriteLine("======================================");
                    Console.WriteLine(input);
                }

                var template = Template.Parse(input, "text", parserOptions, lexerOptions);

                var result = string.Empty;
#if SCRIBAN_ASYNC
                var resultAsync = string.Empty;
#endif
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
                    if (specialLiquid && !isRoundtrip)
                    {
                        throw new InvalidOperationException("Parser errors: " + result);
                    }
                }
                else
                {
                    if (isRoundtripTest)
                    {
                        result = template.ToText();
                    }
                    else
                    {
                        Assert.NotNull(template.Page);

                        if (!isRoundtrip)
                        {
                            // Dumps the roundtrip version
                            var lexerOptionsForTrivia = lexerOptions;
                            lexerOptionsForTrivia.KeepTrivia = true;
                            var templateWithTrivia = Template.Parse(input, "input",  parserOptions, lexerOptionsForTrivia);
                            roundtripText = templateWithTrivia.ToText();
                        }

                        try
                        {
                            // Setup a default model context for the tests
                            if (model == null)
                            {
                                var scriptObj = new ScriptObject
                                {
                                    ["page"] = new ScriptObject {["title"] = "This is a title"},
                                    ["user"] = new ScriptObject {["name"] = "John"},
                                    ["product"] = new ScriptObject {["title"] = "Orange", ["type"] = "fruit"},
                                    ["products"] = new ScriptArray()
                                    {
                                        new ScriptObject {["title"] = "Orange", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Banana", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Apple", ["type"] = "fruit"},
                                        new ScriptObject {["title"] = "Computer", ["type"] = "electronics"},
                                        new ScriptObject {["title"] = "Mobile Phone", ["type"] = "electronics"},
                                        new ScriptObject {["title"] = "Table", ["type"] = "furniture"},
                                        new ScriptObject {["title"] = "Sofa", ["type"] = "furniture"},
                                    }
                                };
                                scriptObj.Import(typeof(SpecialFunctionProvider));
                                model = scriptObj;
                            }
                            
                            // Render sync
                            {
                                var context = NewTemplateContext(isLiquid);
                                context.PushOutput(new TextWriterOutput(new StringWriter() {NewLine = "\n"}));
                                var contextObj = new ScriptObject();
                                contextObj.Import(model);
                                context.PushGlobal(contextObj);
                                result = template.Render(context);
                            }

#if SCRIBAN_ASYNC
                            // Render async
                            {
                                var asyncContext = NewTemplateContext(isLiquid);
                                asyncContext.PushOutput(new TextWriterOutput(new StringWriter() {NewLine = "\n"}));
                                var contextObj = new ScriptObject();
                                contextObj.Import(model);
                                asyncContext.PushGlobal(contextObj);
                                resultAsync = template.RenderAsync(asyncContext).Result;
                            }
#endif
                        }
                        catch (Exception exception)
                        {
#if SCRIBAN_ASYNC
                            hasException = true;
#endif
                            if (specialLiquid)
                            {
                                throw;
                            }
                            else
                            {
                                result = GetReason(exception);
                            }
                        }
                    }
                }

                var testContext = isRoundtrip ? "Roundtrip - " : String.Empty;
                Console.WriteLine($"{testContext}Result");
                Console.WriteLine("======================================");
                Console.WriteLine(result);
                Console.WriteLine($"{testContext}Expected");
                Console.WriteLine("======================================");
                Console.WriteLine(expected);

                if (isRoundtrip && expectParsingErrorForRountrip)
                {
                    Assert.True(hasErrors, "The roundtrip test is expecting an error");
                    Assert.AreNotEqual(expected, result);
                }
                else
                {
                    TextAssert.AreEqual(expected, result);
                }

#if SCRIBAN_ASYNC
                if (!isRoundtrip && !isRoundtripTest && !hasErrors && !hasException)
                {
                    Console.WriteLine("Checking async");
                    Console.WriteLine("======================================");
                    TextAssert.AreEqual(expected, resultAsync);
                }
#endif

                if (isRoundtripTest || isRoundtrip || hasErrors)
                {
                    break;
                }
            }
        }

        private static TemplateContext NewTemplateContext(bool isLiquid)
        {
            var context = isLiquid
                ? new LiquidTemplateContext()
                {
                    TemplateLoader = new LiquidCustomTemplateLoader()
                }
                : new TemplateContext()
                {
                    TemplateLoader = new CustomTemplateLoader()
                };
            // We use a custom output to make sure that all output is using the "\n"
            context.NewLine = "\n";
            return context;
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

        public static IEnumerable ListBuiltinFunctionTests(string functionObject)
        {
            var builtinDocFile = Path.GetFullPath(Path.Combine(BaseDirectory, BuiltinMarkdownDocFile));
            var lines = File.ReadAllLines(builtinDocFile);

            var matchFunctionSection = new Regex($@"^###\s+`({functionObject}\.\w+)`");

            var tests = new List<TestCaseData>();

            string nextFunctionName = null;
            int processState = 0;
            // states:
            // - 0 function section or wait for ```scriban-html (input)
            // - 2 parse input (wait for ```)
            // - 3 wait for ```html (output)
            // - 4 parse input (wait for ```)
            var input = new StringBuilder();
            var output = new StringBuilder();
            foreach (var line in lines)
            {
                // Match first:
                //### `array.add_range`
                switch (processState)
                {
                    case 0:
                        var match = matchFunctionSection.Match(line);
                        if (match.Success)
                        {
                            nextFunctionName = match.Groups[1].Value;
                        }

                        // We have reached another object section, we can exit
                        if (line.StartsWith("## ") && nextFunctionName != null)
                        {
                            return tests;
                        }

                        if (nextFunctionName != null && line.StartsWith("```scriban-html"))
                        {
                            processState = 1;
                            input = new StringBuilder();
                            output = new StringBuilder();
                        }
                        break;
                    case 1:
                        if (line.Equals("```"))
                        {
                            processState = 2;
                        }
                        else
                        {
                            input.AppendLine(line);
                        }
                        break;
                    case 2:
                        if (line.StartsWith("```html"))
                        {
                            processState = 3;
                        }
                        break;
                    case 3:
                        if (line.Equals("```"))
                        {
                            tests.Add(new TestCaseData(nextFunctionName, input.ToString(), output.ToString()));
                            processState = 0;
                        }
                        else
                        {
                            output.AppendLine(line);
                        }
                        break;
                }
            }

            return tests;
        }

        public static IEnumerable ListTestFiles(string folder)
        {
            var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));
            foreach (var file in
                Directory.GetFiles(Path.Combine(baseDir, folder), InputFilePattern, SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(OutputEndFileExtension))
                    .Select(f => f.StartsWith(baseDir) ? f.Substring(baseDir.Length + 1) : f)
                    .OrderBy(f => f))
            {
                var caseData = new TestCaseData(file);
                var category = Path.GetDirectoryName(file);
                caseData.TestName = category + "/" + Path.GetFileNameWithoutExtension(file);
                caseData.SetCategory(category);
                yield return caseData;
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
                Category = Path.GetDirectoryName(filePath);
            }

            public string FilePath { get; }

            public string Category { get; }

            public override string ToString()
            {
                return FilePath;
            }
        }

        private static string BaseDirectory
        {
            get
            {
#if !NETCOREAPP1_0 && !NETCOREAPP1_1
                var assembly = Assembly.GetExecutingAssembly();
                var codebase = new Uri(assembly.CodeBase);
                var path = codebase.LocalPath;
                return Path.GetDirectoryName(path);
#else
                return Directory.GetCurrentDirectory();
#endif
            }
        }
    }
}