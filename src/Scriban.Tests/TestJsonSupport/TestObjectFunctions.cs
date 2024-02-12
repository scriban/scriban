// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using Scriban.Runtime;
using Scriban.Syntax;


namespace Scriban.Tests.TestJsonSupport;

[TestFixture]
public class TestObjectFunctions {
    [Test]
    public void Can_parse_json()
    {
        var template = Template.Parse("""
            {{
                json = `{ "foo": { "bar": [{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }] } }`
                obj = json | object.from_json
                obj.foo.bar[1].baz
            }}
            """
        );

        var result = template.Render();

        Assert.AreEqual("2", result);
    }

    [TestCase("""
        null
        """, """
        null
        """)]
    [TestCase("""
        true
        """, """
        true
        """)]
    [TestCase("""
        false
        """, """
        false
        """)]
    [TestCase("""
        "string"
        """, """
        "string"
        """)]
    [TestCase("""
        123
        """, """
        123
        """)]
    [TestCase("""
        123.45
        """, """
        123.45
        """)]
    [TestCase("""
        [1, 2, 3, {foo: "bar"}, { "baz": 123 }]
        """, """
        [1,2,3,{"foo":"bar"},{"baz":123}]
        """)]
    [TestCase("""
        { foo: { bar: [{ baz: 1 }, { baz: 2 }, { baz: 3 }] } }
        """, """
        {"foo":{"bar":[{"baz":1},{"baz":2},{"baz":3}]}}
        """)]
    public void Can_convert_ScribanValue_to_json(string scriban, string json)
    {
        var template = Template.Parse($$$"""
            {{ {{{scriban}}} | object.to_json }}
            """
        );

        var result = template.Render();

        Assert.AreEqual(json, result);
    }

    [Test]
    public void Can_convert_TypedModel_to_json()
    {
        var template = Template.Parse("""
            {{ model | object.to_json }}
            """
        );

        var result = template.Render(new {
            Model = new {
                Foo = "bar",
                Baz = new[] { 1, 2, 3 }
            }
        });

        Assert.AreEqual("""
            {"foo":"bar","baz":[1,2,3]}
            """, result);
    }

    [Test]
    public void Can_handle_MemberRenamer_when_writing_json()
    {
        var template = Template.Parse("""
            {{ Model | object.to_json }}
            """
        );

        var model = new {
            Model = new {
                Foo = "bar",
                Baz = new[] { 1, 2, 3 }
            }
        };

        var result = template.Render(model, member => member.Name);

        Assert.AreEqual("""
            {"Foo":"bar","Baz":[1,2,3]}
            """, result);
    }

    [Test]
    public void Throws_when_serializing_function_to_json()
    {
        var template = Template.Parse("""
            {{
                func myFunc()
                    ret 1
                end

                object.to_json @myFunc
            }}
            """
        );

        var ex = Assert.Throws<ScriptRuntimeException>(() => {
            var result = template.Render();
        })!;

        Assert.AreEqual("<input>(6,20) : error : Can not serialize functions to JSON. (Parameter 'value')", ex.Message);
    }
}