// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Scriban.Parsing;
using Scriban.Runtime;


namespace Scriban.Tests.TestJsonSupport;

[TestFixture]
public class TestScriptObjectJson {
    [TestCase("""null""", "{{ foo }}", "")]
    [TestCase("""{ "foo": null }""", "{{ foo }}", "")]
    [TestCase("""{ "foo": true }""", "{{ foo }}", "true")]
    [TestCase("""{ "foo": false }""", "{{ foo }}", "false")]
    [TestCase("""{ "foo": "bar" }""", "{{ foo }}", "bar")]
    [TestCase("""{ "foo": 123.45 }""", "{{ foo }}", "123.45")]
    [TestCase("""{ "foo": [1, 2, 3] }""", "{{ foo | object.typeof }}", "array")]
    [TestCase("""{ "foo": [1, 2, 3] }""", "{{ foo[0] }}", "1")]
    [TestCase("""{ "foo": { "bar": "baz" } }""", "{{ foo }}", """{bar: "baz"}""")]
    [TestCase("""{ "foo": { "bar": "baz" } }""", "{{ foo.bar }}", "baz")]
    [TestCase("""{ "foo": { "bar": [1, 2, 3] } }""", "{{ foo.bar[0] }}", "1")]
    [TestCase("""{ "foo": { "bar": [{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }] } }""", "{{ foo.bar[1].baz }}", "2")]
    public void ScriptObject_can_import_json_object(string json, string script, string expected)
    {
        // Test ScriptObject.From(JsonElement)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = ScriptObject.From(obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptObject.From(object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = ScriptObject.From((object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test Import(JsonElement) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            ScriptObjectExtensions.Import(model, json: obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test Import(object) extension
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            ScriptObjectExtensions.Import(model, obj: (object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
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
    [TestCase("""{ "foo": null }""", "")]
    [TestCase("""{ "foo": true }""", "boolean")]
    [TestCase("""{ "foo": false }""", "boolean")]
    [TestCase("""{ "foo": "bar" }""", "string")]
    [TestCase("""{ "foo": 123.45 }""", "number")]
    [TestCase("""{ "foo": [1, 2, 3] }""", "array")]
    [TestCase("""{ "foo": { "bar": "baz" } }""", "object")]
    public void ScriptObject_can_add_json_values(string json, string expected)
    {
        const string script = """
            {{
                if (object.typeof model) != "object"
                    model | object.typeof
                else
                    model?.foo | object.typeof
                end
            }}
            """;

        // Test ScriptObject.Add(string, JsonElement)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            model.Add("model", obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptObject.Add(string, object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            model.Add("model", (object) obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test IDictionary<string, object>.Add(string, object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            ((IDictionary<string, object>) model).Add("model", obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test IDictionary.Add(string, object)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            ((IDictionary) model).Add("model", obj);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptObject.SetValue(string, JsonElement, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            model.SetValue("model", obj, false);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptObject.SetValue(string, object, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            model.SetValue("model", (object) obj, false);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptObject.TrySetValue(TemplateContext, SourceSpan, string, JsonElement, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            model.TrySetValue(null!, new SourceSpan(), "model", obj, false);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }

        // Test ScriptObject.TrySetValue(TemplateContext, SourceSpan, string, object, bool)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            var model = new ScriptObject();
            model.TrySetValue(null!, new SourceSpan(), "model", (object) obj, false);

            var result = RenderHelper.Render(
                script: script,
                scriptObject: model
            );

            Assert.AreEqual(expected, result);
        }
    }


    [TestCase("""true""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `True`. Expecting Json Object.')")]
    [TestCase("""false""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `False`. Expecting Json Object.')")]
    [TestCase("\"bar\"", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `String`. Expecting Json Object.')")]
    [TestCase("""123.45""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Number`. Expecting Json Object.')")]
    [TestCase("""[1, 2, 3]""", "Specified argument was out of the range of valid values. (Parameter 'Unsupported object type `Array`. Expecting Json Object.')")]
    public void ScriptObject_can_not_import_json_non_object(string json, string expected)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(json);

        // Test ScriptObject.From(JsonElement)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = ScriptObject.From(obj);
            })!;
            Assert.AreEqual(expected, ex.Message);
        }

        // Test ScriptObject.From(object)
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = ScriptObject.From((object) obj);
            })!;
            Assert.AreEqual(expected, ex.Message);
        }

        // Test Import(JsonElement) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptObject();
                ScriptObjectExtensions.Import(model, json: obj);
            })!;
            Assert.AreEqual(expected, ex.Message);
        }

        // Test Import(object) extension
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => {
                var model = new ScriptObject();
                ScriptObjectExtensions.Import(model, obj: (object) obj);
            })!;
            Assert.AreEqual(expected, ex.Message);
        }
    }
}

public class RenderHelper {
    /// <summary>
    /// Runs the specified script with the specified scriptObject in global scope
    /// with an <b>invariant culture</b> to ensure predictable formatting!
    /// </summary>
    public static string Render(string script, ScriptObject scriptObject)
    {
        var context = new TemplateContext();
        context.PushGlobal(scriptObject);
        context.PushCulture(CultureInfo.InvariantCulture);

        var template = Template.Parse(script);
        return template.Render(context);
    }
}