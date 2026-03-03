// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using Scriban.Parsing;
using Scriban.Runtime;


namespace Scriban.Tests.TestJsonSupport;

[TestFixture]
public class TestScriptArrayJson {
    [TestCase("""null""", "{{ array }}", "[]")]
    [TestCase("""[1, 2, 3]""", "{{ array | object.typeof }}", "array")]
    [TestCase("""[1, 2, 3]""", "{{ array[0] }}", "1")]
    [TestCase("""[{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }]""", "{{ array[1].baz }}", "2")]
    public void ScriptArray_can_import_json_array(string json, string script, string expected)
    {
        // Test Import(JsonElement) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            ScriptObjectExtensions.Import(model, json: obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test Import(object) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            ScriptObjectExtensions.Import(model, obj: (object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }
    }


    [TestCase("""[1, 2, 3]""", "{{ array | object.typeof }}", "array")]
    [TestCase("""[1, 2, 3]""", "{{ array[0] }}", "1")]
    [TestCase("""[{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }]""", "{{ array[1].baz }}", "2")]
    public void ScriptArray_From_json_array(string json, string script, string expected)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);
        var model = ScriptArray.From(obj);

        var result = RenderHelper.Render(
            script: script,
            scriptObject: ScriptObject.From(new { array = model })
        );

        Assert.AreEqual(expected, result);
    }

    [TestCase("""true""")]
    [TestCase("""false""")]
    [TestCase("\"bar\"")]
    [TestCase("""123.45""")]
    [TestCase("""{ "foo": "bar" }""")]
    public void ScriptArray_From_json_non_array_throws(string json)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Throws<ArgumentException>(() => ScriptArray.From(obj));
    }

    [Test]
    public void ScriptArray_From_enumerable()
    {
        var list = new List<object> { 1, "two", 3.0 };
        var model = ScriptArray.From(list);

        Assert.AreEqual(3, model.Count);
        Assert.AreEqual(1, model[0]);
        Assert.AreEqual("two", model[1]);
        Assert.AreEqual(3.0, model[2]);
    }

    [Test]
    public void ScriptArray_From_null_throws()
    {
        Assert.Throws<ArgumentNullException>(() => ScriptArray.From(null!));
    }

    [TestCase("""true""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `True`. Expecting Json Array.')")]
    [TestCase("""false""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `False`. Expecting Json Array.')")]
    [TestCase("\"bar\"", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `String`. Expecting Json Array.')")]
    [TestCase("""123.45""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Number`. Expecting Json Array.')")]
    [TestCase("""{ "foo": "bar" }""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Object`. Expecting Json Array.')")]
    public void ScriptArray_can_not_import_json_non_array(string json, string expected)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);

        // Test Import(JsonElement) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptArray();
                ScriptObjectExtensions.Import(model, json: obj);
            })!;
            Assert.AreEqual(expected, ex.Message);
        }

        // Test Import(object) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptArray();
                ScriptObjectExtensions.Import(model, obj: (object) obj);
            })!;
            Assert.AreEqual(expected, ex.Message);
        }
    }
}