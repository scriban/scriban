// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Globalization;
using Newtonsoft.Json;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    [TestFixture]
    public class TestRuntime
    {
        [Test]
        public void TestCulture()
        {
            var number = 11232.123;
            var customCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
            {
                NumberFormat =
                {
                    NumberDecimalSeparator = ",",
                    NumberGroupSeparator = "."
                }
            };

            var numberAsStr = number.ToString(customCulture);

            var template = Template.Parse("{{ 11232.123 }}");
            var context = new TemplateContext();
            context.PushCulture(customCulture);
            var result = template.Render(context);
            context.PopCulture();

            Assert.AreEqual(numberAsStr, result);
        }


        [Test]
        public void TestEvaluateScriptOnly()
        {
            {
                var lexerOptions = new LexerOptions() {Mode = ScriptMode.ScriptOnly};
                var template = Template.Parse("y = x + 1; y;", lexerOptions: lexerOptions);
                var result = template.Evaluate(new {x = 10});
                Assert.AreEqual(11, result);
            }
            {
                var result = Template.Evaluate("y = x + 1; y;", new { x = 10 });
                Assert.AreEqual(11, result);
            }
        }

        [Test]
        public void TestEvaluateDefault()
        {
            {
                var template = Template.Parse("{{y = x + 1; y;}} yoyo");
                var result = template.Evaluate(new {x = 10});
                Assert.AreEqual(" yoyo", result);
            }
            {
                var template = Template.Parse("{{y = x + 1; y;}} yoyo {{y}}");
                var result = template.Evaluate(new { x = 10 });
                Assert.AreEqual(11, result);
            }
        }

        [Test]
        public void TestReadOnly()
        {
            var template = Template.Parse("Test {{ a.b.c = 1 }}");

            var a = new ScriptObject()
            {
                {"b", new ScriptObject() {IsReadOnly = true}}
            };

            var context = new TemplateContext();
            context.PushGlobal(new ScriptObject()
            {
                {"a", a}
            });
            var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
            var result = exception.ToString();
            Assert.True(result.Contains("a.b.c"), $"The exception string `{result}` does not contain a.b.c");
        }

        [Test]
        public void TestDynamicVariable()
        {
            var context = new TemplateContext
            {
                TryGetVariable = (TemplateContext templateContext, SourceSpan span, ScriptVariable variable, out object value) =>
                {
                    value = null;
                    if (variable.Name == "myvar")
                    {
                        value = "yes";
                        return true;
                    }
                    return false;
                }
            };

            {
                var template = Template.Parse("Test with a dynamic {{ myvar }}");
                context.Evaluate(template.Page);
                var result = context.Output.ToString();

                TextAssert.AreEqual("Test with a dynamic yes", result);
            }

            {
                // Test StrictVariables
                var template = Template.Parse("Test with a dynamic {{ myvar2 }}");
                context.StrictVariables = true;
                var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
                var result = exception.ToString();
                var check = "The variable `myvar2` was not found";
                Assert.True(result.Contains(check), $"The exception string `{result}` does not contain the expected value");
            }
        }

        [Test]
        public void TestDynamicMember()
        {
            var template = Template.Parse("Test with a dynamic {{ a.myvar }}");

            var globalObject = new ScriptObject();
            globalObject.SetValue("a", new ScriptObject(), true);

            var context = new TemplateContext
            {
                TryGetMember = (TemplateContext localContext, SourceSpan span, object target, string member, out object value) =>
                {
                    value = null;
                    if (member == "myvar")
                    {
                        value = "yes";
                        return true;
                    }
                    return false;
                }
            };

            context.PushGlobal(globalObject);
            context.Evaluate(template.Page);
            var result = context.Output.ToString();

            TextAssert.AreEqual("Test with a dynamic yes", result);
        }

        [Test]
        public void TestTemplateLoaderNoArgs()
        {
            var template = Template.Parse("Test with a include {{ include }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
            var expectedString = "Expecting at least the name of the template to include for the <include> function";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `{expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderNotSetup()
        {
            var template = Template.Parse("Test with a include {{ include 'yoyo' }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
            var expectedString = "No TemplateLoader registered";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `{expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderNotNull()
        {
            var template = Template.Parse("Test with a include {{ include null }}");
            var context = new TemplateContext();
            var exception = Assert.Throws<ScriptRuntimeException>(() => context.Evaluate(template.Page));
            var expectedString = "Include template name cannot be null or empty";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        [Test]
        public void TestTemplateLoaderInclude()
        {
            var template = Template.Parse("Test with a include {{ include 'yoyo' }}");

            var context = new TemplateContext() { TemplateLoader = new MyTemplateLoader() };
            context.Evaluate(template.Page);
            var result = context.Output.ToString();

            Assert.True(result.Contains("of `absolute:yoyo`"), "The result does not contain the expected string after include");
        }

        [Test]
        public void TestTemplateLoaderIncludeWithParsingErrors()
        {
            var template = Template.Parse("Test with a include {{ include 'invalid' }}");

            var context = new TemplateContext() {TemplateLoader = new MyTemplateLoader()};
            var exception = Assert.Throws<ScriptParserRuntimeException>(() => context.Evaluate(template.Page));
            Console.WriteLine(exception);
            var expectedString = "Error while parsing template";
            Assert.True(exception.Message.Contains(expectedString), $"The message `{exception.Message}` does not contain the string `${expectedString}`");
        }

        private class MyTemplateLoader : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return $"absolute:{templateName}";
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                if (templatePath == "absolute:invalid")
                {
                    return "Invalid script with syntax error: {{ 1 + }}";
                }
                return $"Loaded content of `{templatePath}`";
            }
        }

        [Test]
        public void TestJson()
        {
            // issue: https://github.com/lunet-io/scriban/issues/11
            // fixed: https://github.com/lunet-io/scriban/issues/15

            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");

            System.Data.DataRow dataRow = dataTable.NewRow();
            dataRow["Column1"] = "Hello";
            dataRow["Column2"] = "World";
            dataTable.Rows.Add(dataRow);

            dataRow = dataTable.NewRow();
            dataRow["Column1"] = "Bonjour";
            dataRow["Column2"] = "le monde";
            dataTable.Rows.Add(dataRow);

            string json = JsonConvert.SerializeObject(dataTable);
            Console.WriteLine("Json: " + json);

            var parsed = JsonConvert.DeserializeObject(json);
            Console.WriteLine("Parsed: " + parsed);

            string myTemplate = @"
[
  { {{ for tbr in tb }}
    ""N"": {{tbr.Column1}},
    ""M"": {{tbr.Column2}}
    {{ end }}
  },
]
{{tb}}
";

            // Parse the template
            var template = Template.Parse(myTemplate);

            // Render
            var context = new TemplateContext { MemberRenamer = name => name };
            var scriptObject = new ScriptObject();
            scriptObject.Import(new { tb = parsed });
            context.PushGlobal(scriptObject);
            var result = template.Render(context);
            context.PopGlobal();

            var expected =
                @"
[
  { 
    ""N"": Hello,
    ""M"": World
    
    ""N"": Bonjour,
    ""M"": le monde
    
  },
]
[[[Hello], [World]], [[Bonjour], [le monde]]]
";

            TextAssert.AreEqual(expected, result);
        }
    }
}