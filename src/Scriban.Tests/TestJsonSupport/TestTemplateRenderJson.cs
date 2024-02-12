// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Text.Json;


namespace Scriban.Tests.TestJsonSupport;

[TestFixture]
public class TestTemplateRenderJson {
    [Test]
    public void Template_render_accepts_json()
    {
        var json = JsonSerializer.Deserialize<JsonElement>("""{ "foo": { "bar": [{ "baz": 1 }, { "baz": 2 }, { "baz": 3 }] } }""");

        var template = Template.Parse("{{ foo.bar[1].baz }}");
        var result = template.Render(json);

        Assert.AreEqual("2", result);
    }
}