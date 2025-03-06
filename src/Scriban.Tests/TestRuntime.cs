// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    delegate string Args(object[] args);

    [TestFixture]
    public class TestRuntime
    {
        [Test]
        public void TestFunctionPointerWithPath()
        {
            var script = """
                         {{ ["", "200",  "400"] | array.any @string.contains '20' }}
                         """;
            var template = Scriban.Template.Parse(script);
            var result = template.Render();
            Assert.AreEqual("true", result);

            script = """
                     {{ ["", "200",  "400"] | array.any @string.contains '50' }}
                     """;
            template = Scriban.Template.Parse(script);
            result = template.Render();
            Assert.AreEqual("false", result);
        }
        
        [Test]
        public void TestPipeAndNamedArguments()
        {
            var script = """
                         {{func get_values; ret [{name:'A'},{name:'B'},{name:'C'}]; end;}}4) Breaks: {{ get_values '1' two:'2' three: '3' | array.map 'name' }}
                         """;
            var template = Scriban.Template.Parse(script);
            var result = template.Render();

            Assert.AreEqual("4) Breaks: [\"A\", \"B\", \"C\"]", result);
        }
        
        [Test]
        public void TestNullCoallescingWithStringInterpolation()
        {
            var script = """
                         {{ "hello" ?? $"{" "}world" }}
                         """;

            //var lexer = new Lexer(script);
            //foreach (var token in lexer)
            //{
            //    Console.WriteLine(token);
            //}
            var template = Scriban.Template.Parse(script);
            var result = template.Render();

            template.Page.PrintTo(new ScriptPrinter(new TextWriterOutput(Console.Out)));

            Assert.AreEqual("hello", result);
        }

        [Test]
        public void String_And_Null_Concatenated_Should_Not_Null()
        {
            var context = new TemplateContext()
            {
            };
            var tmplExample1 = Template.Parse("{{'my name is ' + null}}");
            var tmplExample2 = Template.Parse("{{$'my name is {null}'}}");

            var result = tmplExample1.Render(context);
            Assert.AreEqual("my name is ", result);

            result = tmplExample2.Render(context);
            Assert.AreEqual("my name is ", result);
        }

        [Test]
        public void TestAssignValToDictionary()
        {
            var dict = new Dictionary<string, string>();
            dict["name"] = "bob";
            var model = new ScriptObject();
            model.Add("dict", dict);
            var context = new TemplateContext();
            context.PushGlobal(model);

            var input = "{{dict.location = \"home\"}}";
            var template = Template.Parse(input);
            var results = template.Render(context);

            input = "{{dict[\"location\"] = \"home\"}}";
            template = Template.Parse(input);
            results = template.Render(context);
            // Assert.AreEqual("", results);
        }

        [Test]
        public void TestScriptObjectAsDictionary()
        {
            var model = (IDictionary)(new ScriptObject());
            model.Add("name", "John");
            model.Add("age", 20);
            Assert.AreEqual("John", model["name"]);
            Assert.AreEqual(20, model["age"]);
        }

        [Test]
        public void TestLazy()
        {
            var input = @"{{ value }}";
            var template = Template.Parse(input);
            var result = template.Render(new { value = new ScriptLazy<int>(() => 1)});
            Assert.AreEqual("1", result);
        }

        [Test]
        public void TestEval()
        {
            var input = @"{{ x = object.eval '1 + 1' }}";
            var template = Template.Parse(input);
            var context = new TemplateContext();
            var result = template.Render(context);
            Assert.AreEqual("", result);
            Assert.AreEqual(2, ((ScriptObject)context.CurrentGlobal)["x"]);

            input = @"{{ x = object.eval '+' }}";
            template = Template.Parse(input);
            context = new TemplateContext();
            Assert.Throws<ScriptRuntimeException>( () => template.Render(context));
        }

        [Test]
        public void TestEnumerator()
        {
            var input = @"{{
  queue.add 'a'
  for x in queue.flush
    x
    if x == 'a'; queue.add 'b'; end
  end
}}";
            var template = Template.Parse(input);

            var test = template.Render(new { queue = new QueueBuiltin() });
            Assert.AreEqual("ab", test);
        }

        class QueueBuiltin : ScriptObject
        {
            static Queue<string> queue = new();

            public static void Add(string x) => queue.Enqueue(x);

            public static IEnumerable<string> Flush()
            {
                while (queue.TryDequeue(out var x))
                    yield return x;
            }
        }


        [Test]
        public void TestDateParse()
        {
            Template template = Template.Parse(
                @"{{date.format='%FT%T.%N%Z'}}{{ date.parse '2018~06~17~13~59~+08:00' '%Y~%m~%d~%H~%M~%Z' }}");
            var result = template.Render();
            Console.WriteLine(result);
        }


        [Test]
        public void TestLoop()
        {

            var template = Template.Parse(@"{{
my_function(x) = x * i
result = 0
for i in [1,2,3,4]
    result = result + (my_function 10)
end
}}Result: {{ result }}
");
            var result = template.Render();




        }





        [Test]
        public void TestPars()
        {
            string Dump(params object[] args)
            {
                return "hello";
            }

            ScriptObject model = new ScriptObject();
            ScriptObject debug = new ScriptObject();
            Args dump = Dump;

            debug.Import("dump", dump);
            model["debug"] = debug;

            var input = "{{debug.dump(10, \"hello\", [0, 1, 2])}}";
            var template = Template.Parse(input);
            var result = template.Render(model);

            Assert.AreEqual("hello", result);
        }

        [Test]
        public void TestUlong()
        {
            var input = @"{{if value > 0; 1; else; 2; end;}}";

            var template = Template.Parse(input);
            var result = template.Render(new { value = (ulong)1 });
            Assert.AreEqual("1", result);
        }

        [Test]
        public void TestDictionaryInt()
        {
            int MyInt = 1;
            Dictionary<int, string> MyDict = new();
            MyDict.Add(MyInt, "hello");

            string templateTxt = "{{ MyDict[MyInt] }}";

            Template template = Template.Parse(templateTxt);
            var result = template.Render(new { MyDict, MyInt }, member => member.Name);

            Assert.AreEqual("hello", result);
        }

        [Test]
        public void TesterFilterEvaluation()
        {
            var result = Template.Parse("{{['', '200', '','400'] | array.filter @string.empty}}").Evaluate(new TemplateContext());
            Assert.IsInstanceOf<ScriptRange>(result);
            var array = (ScriptRange)result;
            Assert.AreEqual(2, array.Count);
            Assert.AreEqual("", array[0]);
            Assert.AreEqual("", array[1]);
        }

        [Test]
        public void TestGetTypeName()
        {
            var context = new TemplateContext();

            Assert.AreEqual("bool", context.GetTypeName(true));
            Assert.AreEqual("bool", context.GetTypeName(false));
            Assert.AreEqual("byte", context.GetTypeName((byte)1));
            Assert.AreEqual("sbyte", context.GetTypeName((sbyte)1));
            Assert.AreEqual("ushort", context.GetTypeName((ushort)1));
            Assert.AreEqual("short", context.GetTypeName((short)1));
            Assert.AreEqual("uint", context.GetTypeName((uint)1));
            Assert.AreEqual("int", context.GetTypeName((int)1));
            Assert.AreEqual("ulong", context.GetTypeName((ulong)1));
            Assert.AreEqual("long", context.GetTypeName((long)1));
            Assert.AreEqual("float", context.GetTypeName((float)1.5f));
            Assert.AreEqual("double", context.GetTypeName((double)1.5));
            Assert.AreEqual("decimal", context.GetTypeName((decimal)1.5m));
            Assert.AreEqual("bigint", context.GetTypeName(new BigInteger(1)));
            Assert.AreEqual("string", context.GetTypeName("test"));
            Assert.AreEqual("range", context.GetTypeName(new ScriptRange()));
            Assert.AreEqual("array", context.GetTypeName(new ScriptArray()));
            Assert.AreEqual("array", context.GetTypeName(new ScriptArray<float>()));
            Assert.AreEqual("object", context.GetTypeName(new ScriptObject()));
            Assert.AreEqual("function", context.GetTypeName(DelegateCustomAction.Create(() => { })));
            Assert.AreEqual("enum", context.GetTypeName(ScriptLang.Default));
        }

        [Test]
        public void TestLocalVariableReturned()
        {
            var text = @"{{
func hello1
 $hello = 'hello1'
 ret $hello
end

func hello2
 $hello = 'hello2'
 ret [ $hello ]
end

func hello3
 $hello = 'hello3'
 ret { hello: $hello }
end

func hello4
 ret { hello: 'hello4' }
end
~}}
hello1: {{ hello1 }}
hello2: {{ hello2 }}
hello3: {{ hello3 }}
hello4: {{ hello4 }}";

            var template = Template.Parse(text);
            var result = template.Render().Replace("\r\n", "\n");
            TextAssert.AreEqual(@"hello1: hello1
hello2: [""hello2""]
hello3: {hello: ""hello3""}
hello4: {hello: ""hello4""}".Replace("\r\n", "\n"), result);
        }

        [Test]
        public void TestForEach()
        {
            var template = Template.Parse(@"{{ [1,2,3] | array.each do
ret $0 + 4
end
}}");
            var result = template.Render();
            Assert.AreEqual("[5, 6, 7]", result);
        }

        [Test]
        public void TestRecursiveLocal()
        {
            var template = Template.Parse("{{ x = {}; with x; func $tester; if $0 == 0; ret; end; $0; $0 - 1 | $tester; end; export = @$tester; end; x.export 5; }}");
            var result = template.Render();
            Assert.AreEqual("54321", result);
        }

        [Test]
        public void TestReflectionArguments()
        {
            var context = new TemplateContext();

            // Allocating a zero length object[] should return the same instance
            {
                var arg0_0 = context.GetOrCreateReflectionArguments(0);
                var arg0_1 = context.GetOrCreateReflectionArguments(0);
                Assert.AreSame(arg0_0, arg0_1);
                context.ReleaseReflectionArguments(arg0_0);
                context.ReleaseReflectionArguments(arg0_1);

                arg0_0 = context.GetOrCreateReflectionArguments(0);
                Assert.AreSame(arg0_0, arg0_1);
            }

            // Allocating a non-zero length object[] should return the != instance
            const int maxArgument = ScriptFunctionCall.MaximumParameterCount;
            {
                for (int length = 1; length <= maxArgument; length++)
                {
                    var arg0_0 = context.GetOrCreateReflectionArguments(length);
                    AssertAllNulls(arg0_0);
                    var arg0_1 = context.GetOrCreateReflectionArguments(length);
                    AssertAllNulls(arg0_1);

                    Assert.AreNotSame(arg0_0, arg0_1);

                    Array.Fill(arg0_0, (object)1);
                    Array.Fill(arg0_1, (object)1);

                    context.ReleaseReflectionArguments(arg0_0);
                    context.ReleaseReflectionArguments(arg0_1);

                    var arg1_0 = context.GetOrCreateReflectionArguments(length);
                    AssertAllNulls(arg1_0);
                    var arg1_1 = context.GetOrCreateReflectionArguments(length);
                    AssertAllNulls(arg1_1);

                    Assert.AreNotSame(arg1_0, arg1_1);

                    Assert.AreSame(arg0_0, arg1_1);
                    Assert.AreSame(arg0_1, arg1_0);

                    context.ReleaseReflectionArguments(arg1_0);
                    context.ReleaseReflectionArguments(arg1_1);
                }
            }

            {
                var arg0_0 = context.GetOrCreateReflectionArguments(maxArgument + 1);
                AssertAllNulls(arg0_0);
                Array.Fill(arg0_0, (object)1);
                context.ReleaseReflectionArguments(arg0_0);

                var arg0_1 = context.GetOrCreateReflectionArguments(maxArgument + 1);
                AssertAllNulls(arg0_1);
                Assert.AreNotSame(arg0_0, arg0_1);

                context.ReleaseReflectionArguments(arg0_1);
            }
        }

        private static void AssertAllNulls(object[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Assert.Null(array[i], $"Array element {i} is not null. {array[i]}");
            }
        }

        public static class MyPipeFunctions
        {
            public static string A(TemplateContext context, object input, string currencyCode = null)
            {
                return input.ToString() + "A";
            }
            public static string B(object input)
            {
                return input.ToString() + "B";
            }
            public static string T(TemplateContext context, object input, params object[] variables)
            {
                return input + (variables.Length > 0 ? string.Join(",", variables.Select(s => s.ToString()).ToArray()) : string.Empty);
            }
        }

        [Test]
        public void TestFunctionArrayEachAndFunctionCall()
        {
            var template = Template.Parse(@"{{
func f; ret $0 + 1; end
[1, 2, 3] | array.each @f
}} EOL");

            var result = template.Render();

            TextAssert.AreEqual("[2, 3, 4] EOL", result);
        }

        [Test]

        public void TestLoopVariable()
        {
            var template = Template.Parse(@"
{{- for x in [1,2,3,4,5]
y = x
end -}}
x and y = {{ x }} and {{ y }}
{{~ for y in [6,7,8,9,0]
z = y
end ~}}
y and z = {{ y }} and {{ z -}}
");
            var expected = @"x and y =  and 5
y and z = 5 and 0";

            var tc = new TemplateContext();
            var result = template.Render(tc);
            TextAssert.AreEqual(expected, result);
        }


        [Test]
        public void ReturnInTemplate()
        {

            var template = Template.Parse(@"{{ if x }}return{{ ret; end }}not return");

            var tc = new TemplateContext();
            tc.CurrentGlobal.SetValue("x", true, false);
            var result = template.Render(tc);
            Assert.AreEqual("return", result);
            tc.CurrentGlobal.SetValue("x", false, false);
            result = template.Render(tc);
            Assert.AreEqual("not return", result);
        }


        [Test]
        public void TestFunctionCallWithNoReturn()
        {
            {
                var template = Template.Parse(@"
{{-
func g(x); x ; end;
1 + g(2)
-}}
");
                var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
                Assert.Throws<ScriptRuntimeException>(() => template.Render(tc));
            }
            {
                var template = Template.Parse(@"
{{-
g(x) = x * 5;
1 + g(2)
-}}
");
                var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
                var result = template.Render(tc);
                Assert.AreEqual("11", result);
            }
            {
                var template = Template.Parse(@"
{{-
func g(x); if x < 0; ret x + 1; else; ret x + 2; end; end;
1 + g(2) + g(-1)
-}}
");
                var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
                var result = template.Render(tc);
                Assert.AreEqual("5", result);
            }
        }

        [Test]
        public void TestExplicitFunctionCall()
        {
            {
                var template = Template.Parse(@"
{{-
g(x,y,z) = x + y * 2 + z * 10
1 + g(1,2,3) }} {{ g(5,6,7) * g(1,2,3) + 1
}}");
                var tc = new TemplateContext() { ErrorForStatementFunctionAsExpression = true };
                var result = template.Render(tc);
                Assert.AreEqual($"{1 + g(1, 2, 3)} {g(5, 6, 7) * g(1, 2, 3) + 1}", result);
            }

            int g(int x, int y, int z) => x + y * 2 + z * 10;
        }


        [Test]
        public void TestStackOverflow()
        {
            {
                var template = Template.Parse(@"
{{-
f(x) = f(x - 1)
f(1)
-}}
");
                var tc = new TemplateContext();
                Assert.Throws<ScriptRuntimeException>(() => template.Render(tc));
            }
        }

        [Test]
        public void TestFunctionWithTemplateContextAndObjectParams()
        {
            {
                var parsedTemplate = Template.ParseLiquid("{{ 'yoyo' | t }}");
                Assert.False(parsedTemplate.HasErrors);

                var scriptObject = new ScriptObject();
                scriptObject.Import(typeof(MyPipeFunctions));
                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var result = parsedTemplate.Render(context);
                TextAssert.AreEqual("yoyo", result);
            }
            {
                var parsedTemplate = Template.ParseLiquid("{{ 'yoyo' | t 1 2 3}}");
                Assert.False(parsedTemplate.HasErrors);

                var scriptObject = new ScriptObject();
                scriptObject.Import(typeof(MyPipeFunctions));
                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var result = parsedTemplate.Render(context);
                TextAssert.AreEqual("yoyo1,2,3", result);
            }
        }

        [Test]
        public void TestInvalidConvertToInt()
        {
            var template = Template.ParseLiquid("{{html>0}}");
            var ex = Assert.Catch<ScriptRuntimeException>(() => template.Render(new { x = 0 }));
            Assert.AreEqual("<input>(1,7) : error : Unable to convert type `object` to int", ex.Message);
        }

        [Test]
        public void TestPipeAndFunction()
        {
            var template = Template.Parse(@"
{{- func format_number
    ret $0 | math.format '0.00' | string.replace '.' ''
end -}}
{{ 123 | format_number -}}
");
            var result = template.Render();
            TextAssert.AreEqual("12300", result);
        }


        [Test]
        public void TestPipeAndFunctionAndLoop()
        {
            var template = Template.Parse(@"
{{- func format_number
    ret $0 | math.format '0.00' | string.replace '.' ''
end -}}
{{
for $i in 1..3
    temp_variable = $i | format_number
end
-}}
{{ temp_variable -}}
");
            var result = template.Render();
            TextAssert.AreEqual("300", result);
        }

        [Test]
        public void InvalidPipe()
        {
            var parsedTemplate = Template.ParseLiquid("{{ 22.00 | a | b | string.upcase }}");
            Assert.False(parsedTemplate.HasErrors);

            var scriptObject = new ScriptObject();
            scriptObject.Import(typeof(MyPipeFunctions));
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            var result = parsedTemplate.Render(context);
            TextAssert.AreEqual("22AB", result);
        }

        [Test]
        public async Task TestAsyncAwait()
        {
            var text = @"{{ wait_and_see }}";
            // Tax1: {{ 1 | match_tax }}
            var template = Template.Parse(text);
            var context = new TemplateContext();
            const int MinDelay = 100;
            context.CurrentGlobal.Import("wait_and_see", new Func<Task<string>>(async () =>
            {
                await Task.Delay(MinDelay + 10);
                return "yes";
            }));
            var clock = Stopwatch.StartNew();
            var result = await template.RenderAsync(context);
            clock.Stop();
            Console.WriteLine(clock.ElapsedMilliseconds);

            Assert.GreaterOrEqual(clock.ElapsedMilliseconds, MinDelay);

            Assert.AreEqual("yes", result);
        }

        [Test]
        public void CheckReturnInsideLoop()
        {
            var text = @"
{{-
func match_tax
    taxes = [5,6,7,8,9]
    for s in taxes
        if s == $0
            ret true
        end
    end
    ret false
end
-}}
Tax: {{ 7 | match_tax }}";
            // Tax1: {{ 1 | match_tax }}
            var template = Template.Parse(text);
            var context = new TemplateContext();
            var result = template.Render(context);

            //Task<string> x = Task.FromResult("yo");

            Assert.AreEqual("Tax: true", result);
        }

        [Test]
        public void TestOperatorPrecedenceNegate()
        {
            var template = Template.Parse("{{ if -5.32 < 0 }}yo{{ end }}");
            Assert.False(template.HasErrors);
            var text = template.Render();
            Assert.AreEqual("yo", text);
        }


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
            var result = template.Render(new { value = x });
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
                var lexerOptions = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
                var template = Template.Parse("y = x + 1; y;", lexerOptions: lexerOptions);
                var result = template.Evaluate(new { x = 10 });
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
                var result = template.Evaluate(new { x = 10 });
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
            Assert.True(result.Contains("The object is readonly"), $"The exception string `{result}` does not contain \"The object is readonly\"");
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
                var check = "The variable or function `myvar2` was not found";
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

            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Column1");
            dataTable.Columns.Add("Column2");

            DataRow dataRow = dataTable.NewRow();
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
  { {{~ for tbr in tb }}
    ""N"": {{tbr.Column1}},
    ""M"": {{tbr.Column2}}
    {{~ end ~}}
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

            // Check new overrides
            {
                var obj = new ScriptObject();
                obj.Import(typeof(MyStaticObject2));

                Assert.True(obj.ContainsKey("static_yoyo"));
                var function = (IScriptCustomFunction)obj["static_yoyo"];
                var context = new TemplateContext();
                var result = function.Invoke(context, new ScriptFunctionCall(), new ScriptArray() { "a" }, null);
                Assert.AreEqual("yoyo2 a", result);
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
                var context = new TemplateContext { MemberFilter = member => member is PropertyInfo };
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
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.PropertyA)));
                Assert.True(accessor.HasMember(context, new SourceSpan(), obj, nameof(MyObject.PropertyB)));

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

        [Test]
        public void TestRenderRuntimeException()
        {
            var template = Template.Parse("Test {{ 'error' | unknown }} behind");
            var context = new TemplateContext();
            context.RenderRuntimeException = TemplateContext.RenderRuntimeExceptionDefault;
            context.Evaluate(template.Page);
            var result = context.Output.ToString();
            Assert.True(System.Text.RegularExpressions.Regex.IsMatch(result, @"^Test \[.+\] behind$"));
        }

        [Test]
        public void TestRenderRuntimeExceptionWithCustomFormat()
        {
            var template = Template.Parse("Test {{ 'error' | unknown }} behind");
            var context = new TemplateContext();
            context.RenderRuntimeException = ex => string.Format("#Scriban-Exception:{0}#", ex.OriginalMessage);
            context.Evaluate(template.Page);
            var result = context.Output.ToString();
            Assert.True(System.Text.RegularExpressions.Regex.IsMatch(result, @"^Test #Scriban-Exception:.+# behind$"));
        }

        [Test]
        public void TestWithCharProperty()
        {
            var test = new ClassWithChar()
            {
                Char = 'a'
            };

            var template = Template.Parse("{{ model.char }}");
            var context = new TemplateContext();
            var result = template.Render(new { model = test });
            Assert.AreEqual("a", result);
        }

        private class ClassWithChar
        {
            public char Char { get; set; }
        }

        [Test]
        public void TestRelaxedMemberAccess()
        {
            var scriptObject = new ScriptObject
            {
                {"a", new MyObject {PropertyA = "A"}}
            };

            // Test unrelaxed member access.
            {
                var context = new TemplateContext()
                {
                    EnableRelaxedTargetAccess = false,
                    EnableRelaxedMemberAccess = false,
                };
                context.PushGlobal(scriptObject);

                var result = Template.Parse("{{a.property_a").Render(context);
                Assert.AreEqual("A", result);

                result = Template.Parse("{{null_ref?.property_a").Render(context);
                Assert.AreEqual(string.Empty, result);

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{a.property_a.null_ref}}").Render(context));

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{null_ref.null_ref}}").Render(context));
            }

            // Test relaxed member access.
            {
                var context = new TemplateContext
                {
                    EnableRelaxedTargetAccess = true,
                    EnableRelaxedMemberAccess = true
                };
                context.PushGlobal(scriptObject);

                var result = Template.Parse("{{a.property_a").Render(context);
                Assert.AreEqual("A", result);

                result = Template.Parse("{{a.property_a.null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);

                result = Template.Parse("{{null_ref.null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);
            }
        }

        [Test]
        public void TestRelaxedListIndexerAccess()
        {
            var scriptObject = new ScriptObject
            {
                {"list", new List<string> {"value" } }
            };

            // Test unrelaxed indexer access.
            {
                var context = new TemplateContext()
                {
                    EnableRelaxedMemberAccess = false,
                };
                context.PushGlobal(scriptObject);

                var result = Template.Parse("{{list[0]").Render(context);
                Assert.AreEqual("value", result);

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{list[0].null_ref.null_ref}}").Render(context));

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{list[-1].null_ref}}").Render(context));

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{null_ref[-1].null_ref}}").Render(context));
            }

            // Test relaxed member access.
            {
                var context = new TemplateContext
                {
                    EnableNullIndexer = false,
                    EnableRelaxedTargetAccess = true,
                    EnableRelaxedMemberAccess = true
                };
                context.PushGlobal(scriptObject);

                var result = Template.Parse("{{list[0]").Render(context);
                Assert.AreEqual("value", result);

                result = Template.Parse("{{list[0].null_ref.null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);

                result = Template.Parse("{{list[-1].null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);

                result = Template.Parse("{{null_ref[-1].null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);
            }
        }

        [Test]
        public void TestRelaxedDictionaryIndexerAccess()
        {
            var scriptObject = new ScriptObject
            {
                {"dictionary", new Dictionary<string, string> { { "key", "value" } } }
            };

            // Test unrelaxed indexer access.
            {
                var context = new TemplateContext();
                context.PushGlobal(scriptObject);

                var result = Template.Parse("{{dictionary['key']").Render(context);
                Assert.AreEqual("value", result);

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{dictionary['key'].null_ref.null_ref}}").Render(context));

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{dictionary['null_ref'].null_ref}}").Render(context));

                Assert.Catch<ScriptRuntimeException>(() =>
                   Template.Parse("{{null_ref['null_ref'].null_ref}}").Render(context));
            }

            // Test relaxed member access.
            {
                var context = new TemplateContext
                {
                    EnableRelaxedTargetAccess = true,
                    EnableRelaxedMemberAccess = true
                };
                context.PushGlobal(scriptObject);

                var result = Template.Parse("{{dictionary['key']").Render(context);
                Assert.AreEqual("value", result);

                result = Template.Parse("{{dictionary['key'].null_ref.null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);

                result = Template.Parse("{{dictionary['null_ref'].null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);

                result = Template.Parse("{{null_ref['null_ref'].null_ref}}").Render(context);
                Assert.AreEqual(string.Empty, result);
            }
        }

        [Test]
        public void TestIndexerOnNET()
        {
            var myobject = new MyObject() { FieldA = "yo" };
            var result = Template.Parse("{{obj['field_a']}}").Render(new ScriptObject() { { "obj", myobject } });
            Assert.AreEqual("yo", result);
        }

        [Test]
        public void TestItemIndexerOnNET_String_Getter()
        {
            var expected = "One";
            var key = "alpha";
            var myobject = new ClassWithItemIndexerString
            {
                [key] = expected
            };
            var result = Template.Parse($"{{{{obj['{key}']}}}}").Render(new ScriptObject() { { "obj", myobject } });
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void TestItemIndexerOnNET_String_Setter()
        {
            var expected = "One";
            var key = "alpha";
            var myobject = new ClassWithItemIndexerString
            {
                [key] = "Initial"
            };
            _ = Template.Parse($"{{{{obj['{key}'] = '{expected}'}}}}").Render(new ScriptObject() { { "obj", myobject } });
            Assert.AreEqual(expected, myobject[key]);
        }
        [Test]
        public void TestItemIndexerOnNET_Integer_Getter()
        {
            var expected = "One";
            var key = 5;
            var myobject = new ClassWithItemIndexerInteger()
            {
                [key] = expected
            };
            var result = Template.Parse($"{{{{obj[{key}]}}}}").Render(new ScriptObject() { { "obj", myobject } });
            Assert.AreEqual(expected, result);
        }
        [Test]
        public void TestItemIndexerOnNET_Integer_Setter()
        {
            var expected = "One";
            var key = 5;
            var myobject = new ClassWithItemIndexerInteger
            {
                [key] = "Initial"
            };
            _ = Template.Parse($"{{{{obj[{key}] = '{expected}'}}}}").Render(new ScriptObject() { { "obj", myobject } });
            Assert.AreEqual(expected, myobject[key]);
        }

        [Test]
        public void TestCaseInsensitiveLookupOnScriptObject()
        {
            var obj = new ScriptObject(StringComparer.OrdinalIgnoreCase);
            obj["Name"] = "world";
            var context = new TemplateContext();
            context.PushGlobal(obj);
            var template = Template.Parse("Hello {{ name }}!");
            var result = template.Render(context);
            Assert.AreEqual("Hello world!", result);
        }

        [Test]
        public void TestCaseInsensitiveLookupOnHierarchy()
        {
            var obj = new ScriptObject(StringComparer.OrdinalIgnoreCase);
            obj.Import(new { UPPERCASED = new { lowercased = 42 } }, renamer: mi => mi.Name);

            var context = new TemplateContext(StringComparer.OrdinalIgnoreCase);
            context.PushGlobal(obj);

            var result = Template.Parse("{{UPPERCASED.lowercased}}-{{uppercased.LOWERCASED}}-{{UPPERCASED.LOWERCASED}}-{{uppercased.lowercased}}").Render(context);
            TextAssert.AreEqual("42-42-42-42", result);
        }

        private class MyObject : MyStaticObject
        {
            public string FieldA;

#pragma warning disable 649
            public string FieldB;
#pragma warning restore 649

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

        private class MyStaticObject2 : MyStaticObject
        {
            public static new string StaticYoyo(string text)
            {
                return "yoyo2 " + text;
            }
        }

        public class ScriptObjectWithNullable : ScriptObject
        {
            public static string Tester(string text, int? value = null)
            {
                return value.HasValue ? text + " Value: " + value.Value : text;
            }
        }

        public class ClassWithItemIndexerString
        {
            private readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();
            public string this[string key]
            {
                get => _dictionary.GetValueOrDefault(key) ?? string.Empty;
                set
                {
                    if (this._dictionary.ContainsKey(key))
                    {
                        this._dictionary[key] = value;
                    }
                    else
                    {
                        this._dictionary.Add(key, value);
                    }
                }
            }
        }
        public class ClassWithItemIndexerInteger
        {
            private readonly Dictionary<int, string> _dictionary = new Dictionary<int, string>();
            public string this[int key]
            {
                get => _dictionary.GetValueOrDefault(key) ?? string.Empty;
                set
                {
                    if (this._dictionary.ContainsKey(key))
                    {
                        this._dictionary[key] = value;
                    }
                    else
                    {
                        this._dictionary.Add(key, value);
                    }
                }
            }
        }
    }
}