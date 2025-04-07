// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    public class TestIncludes
    {

        internal class DummyLoader : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
                => templateName;

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
                => "some text";

            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
                => ValueTask.FromResult(Load(context, callerSpan, templatePath));
        }

        [Test]
        public void IncludeShouldNotThrowWhenStrictVariablesSet()
        {
            var text = @"{{include 'testfile'}}";
            var context = new TemplateContext { TemplateLoader = new DummyLoader() };
            //NOTE - setting strict variables causes the test to fail
            context.StrictVariables = true;
            var compiledTemplate = Template.Parse(text);
            context.PushGlobal(new ScriptObject());

            var result = compiledTemplate.Render(context);
            Assert.AreEqual(result,"some text");
        }


        [Test]
        public void TestLoopWithInclude()
        {
            var context = new TemplateContext()
            {
                TemplateLoader = new LoaderLoopWithInclude()
            };
            var template = Template.Parse(@"{{ for my_loop_variable in 1..3; include 'test'; end; }}");

            var result = template.Render(context);
            TextAssert.AreEqual("123", result);
        }

        public class LoaderLoopWithInclude : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return templateName;
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return "{{ my_loop_variable }}";
            }

            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                throw new System.NotImplementedException();
            }
        }

        [Test]
        public void TestIndentedNestedIncludes()
        {
            var context = new TemplateContext
            {
                TemplateLoader = new LoaderIndentedNestedIncludes(),
                IndentWithInclude = true
            };

            var template = Template.Parse(@"Test
        {{include 'test' ~}}
    {{include 'test' ~}}
");
            var result = template.Render(context).Replace("\r\n", "\n");

            TextAssert.AreEqual(@"Test
        AA
        BB
        CC
          DD
          EE
          FF
        DD
        EE
        FF
    AA
    BB
    CC
      DD
      EE
      FF
    DD
    EE
    FF
".Replace("\r\n", "\n"), result);

        }

        public class LoaderIndentedNestedIncludes : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return templateName;
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return templatePath == "test" ? "AA\r\nBB\r\nCC\r\n  {{include 'nested'}}{{include 'nested'}}" : "DD\r\nEE\r\nFF\r\n";
            }

            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                throw new System.NotImplementedException();
            }
        }


        [Test]
        public void TestIndentedIncludes2()
        {
            var template = Template.Parse(@"Test
{{ include 'header' }}
");
            var context = new TemplateContext();
            context.TemplateLoader = new CustomTemplateLoader();
            context.IndentWithInclude = true;

            var text = template.Render(context).Replace("\r\n", "\n");
            var expected = @"Test
This is a header
".Replace("\r\n", "\n");
            TextAssert.AreEqual(expected, text);
        }

        [Test]
        public void TestIncludeNamedArguments()
        {
            var template = Template.Parse(@"{{ include 'named_arguments' this_arg: 5 }}");
            var context = new TemplateContext();
            context.TemplateLoader = new CustomTemplateLoader();

            var text = template.Render(context).Replace("\r\n", "\n");
            var expected = @"5";
            TextAssert.AreEqual(expected, text);
        }

        [Test]
        public void TestIndentedIncludes()
        {
            var template = Template.Parse(@"  {{ include 'header' }}
    {{ include 'multilines' }}
Test1
      {{ include 'nested_templates_with_indent' }}
Test2
");
            var context = new TemplateContext();
            context.TemplateLoader = new CustomTemplateLoader();
            context.IndentWithInclude = true;

            var text = template.Render(context).Replace("\r\n", "\n");
            var expectedText = @"  This is a header
    Line 1
    Line 2
    Line 3
Test1
        Line 1
        Line 2
        Line 3
Test2
".Replace("\r\n", "\n");

            TextAssert.AreEqual(expectedText, text);
        }

        [Test]
        public void TestJekyllInclude()
        {
            var input = "{% include /this/is/a/test.htm %}";
            var template = Template.ParseLiquid(input, lexerOptions: new LexerOptions() { EnableIncludeImplicitString = true, Lang = ScriptLang.Liquid });
            var context = new TemplateContext { TemplateLoader = new LiquidCustomTemplateLoader() };
            var result = template.Render(context);
            TextAssert.AreEqual("/this/is/a/test.htm", result);
        }

        [Test]
        public void TestTemplateLoaderNoArgs()
        {
            var template = Template.Parse("Test with a include {{ include }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = "Invalid number of arguments";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `{expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderNotSetup()
        {
            var template = Template.Parse("Test with a include {{ include 'yoyo' }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = "No TemplateLoader registered";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `{expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderNotNull()
        {
            var template = Template.Parse("Test with a include {{ include null }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            var expectedString = "Include template name cannot be null or empty";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Test]
        public void TestSimple()
        {
            TestParser.AssertTemplate("Test with a include yoyo", "Test with a include {{ include 'yoyo' }}");
        }

        [Test]
        public void TestArguments()
        {
            TestParser.AssertTemplate("1 + 2", "{{ include 'arguments' 1 2 }}");
        }

        [Test]
        public void TestProduct()
        {
            TestParser.AssertTemplate("product: Orange", "{{ include 'product' }}");
        }

        [Test]
        public void TestNested()
        {
            TestParser.AssertTemplate("This is a header body This is a body_detail This is a footer", "{{ include 'nested_templates' }}");
        }

        [Test]
        public void TestRecursiveNested()
        {
            TestParser.AssertTemplate("56789", "{{ include 'recursive_nested_templates' 5 }}");
        }

        [Test]
        public void TestLiquidNull()
        {
            TestParser.AssertTemplate("", "{% include a %}", ScriptLang.Liquid);
        }

        [Test]
        public void TestLiquidWith()
        {
            TestParser.AssertTemplate("with_product: Orange", "{% include 'with_product' with product %}", ScriptLang.Liquid);
        }

        [Test]
        public void TestLiquidFor()
        {
            TestParser.AssertTemplate("for_product: Orange for_product: Banana for_product: Apple for_product: Computer for_product: Mobile Phone for_product: Table for_product: Sofa ", "{% include 'for_product' for products %}", ScriptLang.Liquid);
        }

        [Test]
        public void TestLiquidArguments()
        {
            TestParser.AssertTemplate("1 + yoyo", "{% include 'arguments' var1: 1, var2: 'yoyo' %}", ScriptLang.Liquid);
        }

        [Test]
        public void TestLiquidWithAndArguments()
        {
            TestParser.AssertTemplate("tada : 1 + yoyo", "{% include 'with_arguments' with 'tada' var1: 1, var2: 'yoyo' %}", ScriptLang.Liquid);
        }

        [Test]
        public void TestTemplateLoaderIncludeWithParsingErrors()
        {
            var template = Template.Parse("Test with a include {{ include 'invalid' }}");
            var context = new TemplateContext() { TemplateLoader = new CustomTemplateLoader() };
            var exception = Assert.Throws<ScriptParserRuntimeException>(() => template.Render(context));
            Console.WriteLine(exception);
            var expectedString = "Error while parsing template";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderIncludeWithLexerErrors()
        {
            var template = Template.Parse("Test with a include {{ include 'invalid2' }}");
            var context = new TemplateContext() { TemplateLoader = new CustomTemplateLoader() };
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Console.WriteLine(exception);
            var expectedString = "The result of including";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderIncludeWithNullGetPath()
        {
            var template = Template.Parse("{{ include 'null' }}");
            var context = new TemplateContext() { TemplateLoader = new CustomTemplateLoader() };
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            Console.WriteLine(exception);
            var expectedString = "Include template path is null";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        
        [Test]
        public void TestIncludeJoin()
        {
            var template = Template.Parse("{{ include_join ['first', 'second', 'third'] ' ' 'begin ' ' end' }}");
            var context = new TemplateContext() { TemplateLoader = new DummyLoader() };
            var expectedString = "begin some text some text some text end";
            Assert.AreEqual(expectedString, template.Render(context));
        }

        
        [Test]
        public void TestIncludeJoinwithParams()
        {
            var template = Template.Parse("{{ include_join joinTemplateNames ' ' 'begin ' ' end' }}");
            var scriptObject = new BuiltinFunctions();
            scriptObject.SetValue("joinTemplateNames", new List<string>() {"first", "second", "third"}, false);
            var context = new TemplateContext(scriptObject) { TemplateLoader = new DummyLoader() };
            var expectedString = "begin some text some text some text end";
            Assert.AreEqual(expectedString, template.Render(context));
        }

        [Test]
        public void TestIncludeJoinWithTemplateDelimiters()
        {
            var template = Template.Parse("{{ include_join ['first', 'second', 'third'] 'tpl: ' 'tpl:begin ' 'tpl: end' }}");
            var context = new TemplateContext() { TemplateLoader = new DummyLoader() };
            var expectedString = "some textsome textsome textsome textsome textsome textsome text";
            Assert.AreEqual(expectedString, template.Render(context));
        }
    }
}