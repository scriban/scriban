// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    public class TestIncludes
    {
        [Test]
        public void TestJekyllInclude()
        {
            var input = "{% include /this/is/a/test.htm %}";
            var template = Template.ParseLiquid(input, lexerOptions: new LexerOptions() { EnableIncludeImplicitString = true, Mode = ScriptMode.Liquid });
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
            var expectedString = "Expecting at least the name of the template to include for the <include> function";
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
            TestParser.AssertTemplate("", "{% include a %}", true);
        }

        [Test]
        public void TestLiquidWith()
        {
            TestParser.AssertTemplate("with_product: Orange", "{% include 'with_product' with product %}", true);
        }

        [Test]
        public void TestLiquidFor()
        {
            TestParser.AssertTemplate("for_product: Orange for_product: Banana for_product: Apple for_product: Computer for_product: Mobile Phone for_product: Table for_product: Sofa ", "{% include 'for_product' for products %}", true);
        }
        
        [Test]
        public void TestLiquidArguments()
        {
            TestParser.AssertTemplate("1 + yoyo", "{% include 'arguments' var1: 1, var2: 'yoyo' %}", true);
        }

        [Test]
        public void TestLiquidWithAndArguments()
        {
            TestParser.AssertTemplate("tada : 1 + yoyo", "{% include 'with_arguments' with 'tada' var1: 1, var2: 'yoyo' %}", true);
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
    }
}