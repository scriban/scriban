// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;

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


    [TestFixture]
    public class TestParser
    {
        private const string RelativeBasePath = @"..\..\TestFiles";
        private const string InputFilePattern = "*.txt";
        private const string OutputEndFileExtension = ".out.txt";

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
            var options = new ParserOptions() {Mode = ScriptMode.FrontMatter};
            var template = ParseTemplate(@"{{ 
variable = 1
name = 'yes'
}}
This is after the frontmatter: {{ name }}
{{
variable + 1
}}", options);

            // Make sure that we have a front matter
            Assert.NotNull(template.Page.FrontMatter);

            var context = new TemplateContext();

            // Evaluate front-matter
            var frontResult = context.Evaluate(template.Page.FrontMatter);
            Assert.Null(frontResult);
            
            // Evaluate page-content
            context.Evaluate(template.Page);
            var pageResult = context.Output.ToString();
            Assert.AreEqual(@"This is after the frontmatter: yes
2", pageResult);
        }

        [Test]
        public void TestScriptOnly()
        {
            var options = new ParserOptions() { Mode = ScriptMode.ScriptOnly };
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

        private static Template ParseTemplate(string text, ParserOptions options = null)
        {
            var template = Template.Parse(text, "text", options);
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
            scriptPage.Evaluate(context);
            context.PopGlobal();

            // Result
            var result = context.Output.ToString();

            Console.WriteLine(result);
        }

        [TestCaseSource("TestFiles")]
        public void Test(TestFilePath testFilePath)
        {
            var inputName = testFilePath.FilePath;
            var baseDir = Path.GetFullPath(Path.Combine(BaseDirectory, RelativeBasePath));

            var inputFile = Path.Combine(baseDir, inputName);
            var inputText = File.ReadAllText(inputFile);

            var expectedOutputFile = Path.ChangeExtension(inputFile, OutputEndFileExtension);
            Assert.True(File.Exists(expectedOutputFile), $"Expecting output result file [{expectedOutputFile}] for input file [{inputName}]");
            var expectedOutputText = File.ReadAllText(expectedOutputFile, Encoding.UTF8);

            var template = Template.Parse(inputText, "text");

            var result = string.Empty;

            if (template.HasErrors)
            {
                for (int i = 0; i < template.Messages.Count; i++)
                {
                    var message = template.Messages[i];
                    if (i > 0)
                    {
                        result += "\r\n";
                    }
                    result += message;
                }
            }
            else
            {

                Assert.NotNull(template.Page);

                try
                {
                    result = template.Render();
                }
                catch (ScriptRuntimeException exception)
                {
                    result = exception.ToString();
                }
            }
            Console.WriteLine("Result");
            Console.WriteLine("======================================");
            Console.WriteLine(result);
            Console.WriteLine("Expected");
            Console.WriteLine("======================================");
            Console.WriteLine(expectedOutputText);

            TextAssert.AreEqual(expectedOutputText, result);
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