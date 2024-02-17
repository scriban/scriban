// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Scriban.Runtime;


namespace Scriban.Tests.TestJsonSupport;

[TestFixture]
public class TestModelWithJsonElement {
    [TestCase("""null""", "")]
    [TestCase("""true""", "true")]
    [TestCase("""false""", "false")]
    [TestCase("""123.45""", "123.45")]
    [TestCase("\"bar\"", "bar")]
    [TestCase("""[1, 2, 3]""", "[1, 2, 3]")]
    [TestCase("""{ "foo": "bar" }""", "{foo: \"bar\"}")]
    public void Can_import_JsonElement_property(string json, string expected)
    {
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        var model = new {
            foo = jsonElement,
        };

        var result = RenderHelper.Render(
            script: "{{ foo }}",
            scriptObject: ScriptObject.From(model)
        );

        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Can_import_boxed_jsonElement()
    {
        var model = JsonSerializer.Deserialize<Dictionary<string, object>>("""{ "model": { "foo": "bar" } }""");

        // ensure we have a boxed JsonElement:
        Assert.AreEqual(typeof(string), model.GetType().GetGenericArguments()[0]);
        Assert.AreEqual(typeof(object), model.GetType().GetGenericArguments()[1]);
        Assert.AreEqual(typeof(JsonElement), model["model"].GetType());

        var result = RenderHelper.Render(
            script: "{{ model.foo }}",
            scriptObject: ScriptObject.From(model)
        );

        Assert.AreEqual("bar", result);
    }

    [Test]
    public void Can_import_jsonElement_in_typed_class()
    {
        var data = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");

        var model = new MyClass("name", data);

        var result = RenderHelper.Render(
            script: """
            Name: {{ name }}
            Data.Foo: {{ data.foo }}
            """,
            scriptObject: ScriptObject.From(model)
        ).ReplaceLineEndings("\n");

        Assert.AreEqual("Name: name\nData.Foo: bar", result);
    }

    [Test]
    public void Can_import_jsonElement_in_typed_struct()
    {
        var data = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");

        var model = new MyStruct("name", data);

        var result = RenderHelper.Render(
            script: """
            Name: {{ name }}
            Data.Foo: {{ data.foo }}
            """,
            scriptObject: ScriptObject.From(model)
        ).ReplaceLineEndings("\n");

        Assert.AreEqual("Name: name\nData.Foo: bar", result);
    }


    private record MyClass(
        string Name,
        JsonElement Data
    );

    private record struct MyStruct(
        string Name,
        JsonElement Data
    );
}