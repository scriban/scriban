// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Scriban.Runtime;

namespace Scriban.Tests
{
    [TestFixture]
    public class TestRuntime
    {
        [Test]
        public void TestDynamicVariable()
        {
            var template = Template.Parse("Test with a dynamic {{ myvar }}");

            var context = new TemplateContext
            {
                TryGetVariable = variable =>
                {
                    Assert.AreEqual("myvar", variable.Name);
                    return "yes";
                }
            };

            context.Evaluate(template.Page);
            var result = context.Output.ToString();

            TextAssert.AreEqual("Test with a dynamic yes", result);
        }

        [Test]
        public void TestDynamicMember()
        {
            var template = Template.Parse("Test with a dynamic {{ a.myvar }}");

            var globalObject = new ScriptObject();
            globalObject.SetValue("a", new ScriptObject(), true);

            var context = new TemplateContext
            {
                TryGetMember = (object target, string member, out object value) =>
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
            var context = new TemplateContext { MemberRenamer = new DelegateMemberRenamer(name => name) };
            var scriptObject = new ScriptObject();
            scriptObject.Import(new { tb = parsed });
            context.PushGlobal(scriptObject);
            template.Render(context);
            context.PopGlobal();

            var result = context.Output.ToString();
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