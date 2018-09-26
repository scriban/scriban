// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Globalization;
using System.Reflection;
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
        public void TestNullDateTime()
        {
            var template = Template.Parse("{{ null | date.to_string '%g' }}");
            var context = new TemplateContext();
            var result = template.Render(context);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void TestDecimal()
        {
            var template = Template.Parse("{{ if value > 0 }}yes{{end}}");
            decimal x = 5;
            var result = template.Render(new { value =  x });
            Assert.AreEqual("yes", result);
        }

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
            var context = new TemplateContext { MemberRenamer = member => member.Name };
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

        [Test]
        public void TestScriptObjectImport()
        {
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject));

                Assert.That(obj, Does.ContainKey("static_field_a"));
                Assert.AreEqual("ValueStaticFieldA", obj["static_field_a"]);
                Assert.True(obj.ContainsKey("static_field_b"));
                Assert.AreEqual("ValueStaticFieldB", obj["static_field_b"]);
                Assert.True(obj.ContainsKey("static_property_a"));
                Assert.AreEqual("ValueStaticPropertyA", obj["static_property_a"]);
                Assert.True(obj.ContainsKey("static_property_b"));
                Assert.AreEqual("ValueStaticPropertyB", obj["static_property_b"]);
                Assert.True(obj.ContainsKey("static_yoyo"));
                Assert.False(obj.ContainsKey("invalid"));
            }

            // Test MemberFilterDelegate
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject), filter: member => member.Name.Contains("Property"));

                Assert.That(obj, Does.Not.ContainKey("static_field_a"));
                Assert.That(obj, Does.Not.ContainKey("static_field_b"));
                Assert.That(obj, Does.ContainKey("static_property_a"));
                Assert.AreEqual("ValueStaticPropertyA", obj["static_property_a"]);
                Assert.That(obj, Does.ContainKey("static_property_b"));
                Assert.AreEqual("ValueStaticPropertyB", obj["static_property_b"]);
                Assert.That(obj, Does.Not.ContainKey("static_yoyo"));
                Assert.That(obj, Does.Not.ContainKey("invalid"));
            }

            // Test MemberRenamerDelegate
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject), renamer: member => member.Name);

                Assert.That(obj, Does.ContainKey(nameof(MyStaticObject.StaticFieldA)));
                Assert.That(obj, Does.ContainKey(nameof(MyStaticObject.StaticFieldB)));
                Assert.That(obj, Does.ContainKey(nameof(MyStaticObject.StaticPropertyA)));
                Assert.AreEqual("ValueStaticPropertyA", obj[nameof(MyStaticObject.StaticPropertyA)]);
                Assert.That(obj, Does.ContainKey(nameof(MyStaticObject.StaticPropertyB)));
                Assert.AreEqual("ValueStaticPropertyB", obj[nameof(MyStaticObject.StaticPropertyB)]);
                Assert.That(obj, Does.ContainKey(nameof(MyStaticObject.StaticYoyo)));
                Assert.That(obj, Does.Not.ContainKey(nameof(MyStaticObject.Invalid)));
            }

            {
                var obj = new ScriptObject();
                obj.Import(new MyObject2(), renamer: member => member.Name);

                Assert.AreEqual(9, obj.Count);
                Assert.That(obj, Does.ContainKey(nameof(MyStaticObject.StaticFieldA)));
                Assert.That(obj, Does.ContainKey(nameof(MyObject.PropertyA)));
                Assert.That(obj, Does.ContainKey(nameof(MyObject2.PropertyC)));
            }
        }


        [Test]
        public void TestScriptObjectAccessor()
        {
            {
                var context = new TemplateContext();
                var obj = new MyObject();
                var accessor = context.GetMemberAccessor(obj);

                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "field_a"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "field_b"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_a"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_b"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_b"));
            }

            // Test Filter
            {
                var context = new TemplateContext {MemberFilter = member => member is PropertyInfo};
                var obj = new MyObject();
                var accessor = context.GetMemberAccessor(obj);

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "field_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "field_b"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_a"));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, "property_b"));

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_field_b"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_a"));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, "static_property_b"));
            }


            // Test Renamer
            {
                var context = new TemplateContext { MemberRenamer = member => member.Name };
                var obj = new MyObject();
                var accessor = context.GetMemberAccessor(obj);

                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.FieldA)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.FieldB)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj,  nameof(MyObject.PropertyA)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj,  nameof(MyObject.PropertyB)));

                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticFieldA)));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticFieldB)));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticPropertyA)));
                Assert.False(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyStaticObject.StaticPropertyB)));
            }
        }

        [Test]
        public void TestNullableArgument()
        {
            var template = Template.Parse("{{ tester 'input1' 1 }}");
            var context = new TemplateContext();
            var testerObj = new ScriptObjectWithNullable();
            context.PushGlobal(testerObj);
            var result = template.Render(context);
            TextAssert.AreEqual("input1 Value: 1", result);
        }

        [Test]
        public void TestPropertyInheritance()
        {
            var scriptObject = new ScriptObject
            {
                {"a", new MyObject {PropertyA = "ClassA"}},
                {"b", new MyObject2 {PropertyA = "ClassB", PropertyC = "ClassB-PropC"}}
            };

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var result = Template.Parse("{{a.property_a}}-{{b.property_a}}-{{b.property_c}}").Render(context);
            TextAssert.AreEqual("ClassA-ClassB-ClassB-PropC", result);
        }

        private class MyObject : MyStaticObject
        {
            public string FieldA;

            public string FieldB;

            public string PropertyA { get; set; }

            public string PropertyB { get; set; }

        }

        private class MyObject2 : MyObject
        {
            public string PropertyC { get; set; }            
        }

        private class MyStaticObject
        {
            static MyStaticObject()
            {
                StaticPropertyA = "ValueStaticPropertyA";
                StaticPropertyB = "ValueStaticPropertyB";
            }

            public static string StaticFieldA = "ValueStaticFieldA";

            public static string StaticFieldB = "ValueStaticFieldB";

            public static string StaticPropertyA { get; set; }

            public static string StaticPropertyB { get; set; }

            public string Invalid()
            {
                return null;
            }

            public static string StaticYoyo(string text)
            {
                return "yoyo " + text;
            }
        }

        public class ScriptObjectWithNullable : ScriptObject
        {
            public static string Tester(string text, int? value = null)
            {
                return value.HasValue ? text + " Value: " + value.Value : text;
            }
        }
    }
}