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


    [TestCase("""null""", "")]
    [TestCase("""true""", "boolean")]
    [TestCase("""false""", "boolean")]
    [TestCase("\"bar\"", "string")]
    [TestCase("""123.45""", "number")]
    [TestCase("""[1, 2, 3]""", "array")]
    [TestCase("""{ "bar": "baz" }""", "object")]
    public void ScriptArray_can_add_json_values(string json, string expected)
    {
        var script = "{{ array[0] | object.typeof }}";

        // Test ScriptArray.Add(JsonElement)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            model.Add(obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptArray.Add(object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            model.Add((object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test IDictionary<string, object>.Add(string, object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            ((IList<object>) model).Add(obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test IDictionary.Add(string, object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            ((IList) model).Add(obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptArray.SetValue(string, JsonElement, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            model.SetValue("foo", obj, false);

            var result = RenderHelper.Render(
                script: "{{ array.foo | object.typeof }}",
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptArray.SetValue(string, object, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            model.SetValue("foo", (object) obj, false);

            var result = RenderHelper.Render(
                script: "{{ array.foo | object.typeof }}",
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptArray.TrySetValue(TemplateContext, SourceSpan, string, JsonElement, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            model.TrySetValue(null!, new SourceSpan(), "foo", obj, false);

            var result = RenderHelper.Render(
                script: "{{ array.foo | object.typeof }}",
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptArray.TrySetValue(TemplateContext, SourceSpan, string, object, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptArray();
            model.TrySetValue(null!, new SourceSpan(), "foo", (object) obj, false);

            var result = RenderHelper.Render(
                script: "{{ array.foo | object.typeof }}",
                scriptObject: ScriptObject.From(new { array = model })
            );

            Assert.AreEqual(expected, result);
        }
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