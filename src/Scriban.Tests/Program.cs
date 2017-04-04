// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Tests
{
    class Program
    {

        public class Author
        {
            public string Name { get; set; }
        }

        public class Book
        {
            public string Title { get; set; }
            public Author Author { get; set; }
        }
        static void Main(string[] args)
        {
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

            {
                var template = Template.Parse(myTemplate);
                //var result = template.Render(new {tb = parsed});
                //Console.WriteLine(result);
                var context = new TemplateContext();
                context.MemberRenamer = new DelegateMemberRenamer(name => name);
                var scriptObject = new ScriptObject();
                scriptObject.Import(new { tb = parsed });
                context.PushGlobal(scriptObject);
                template.Render(context);
                Console.WriteLine(context.Output.ToString());
            }
            {
                var a1 = new Author {Name = "John"};
                var b = new Book[2];
                b[0] = new Book
                {
                    Title = "Book1",
                    Author = a1
                };
                b[1] = new Book
                {
                    Title = "Book2",
                    Author = a1
                };

                var globalFunction = new ScriptObject();

                var template = Template.Parse(@"This {{books[0].Title}} {{ ""is"" | string.upcase }} from scriban!");
                var model = new {books = b};
                var context = new TemplateContext();
                context.MemberRenamer = new DelegateMemberRenamer(name => name);
                context.PushGlobal(globalFunction);

                var localFunction = new ScriptObject();
                localFunction.Import(model);
                context.PushGlobal(localFunction);

                template.Render(context);
                context.PopGlobal();


            }
        }
    }
}
