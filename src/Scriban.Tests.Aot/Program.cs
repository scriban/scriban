// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

// This project is compiled with PublishAot=true to verify that Scriban's
// AOT-safe APIs do not generate any trimming or AOT warnings.

using Scriban;
using Scriban.Runtime;

// 1. Basic template parsing and rendering
var template = Template.Parse("Hello {{name}}!");
var scriptObject = new ScriptObject { { "name", "World" } };
var context = new TemplateContext();
context.PushGlobal(scriptObject);
var result = template.Render(context);
Console.WriteLine(result);

// 2. Using ScriptObject as a dictionary
var model = new ScriptObject();
model["greeting"] = "Hi";
model["items"] = new ScriptArray { 1, 2, 3 };

var template2 = Template.Parse("{{greeting}} - {{items.size}} items");
var context2 = new TemplateContext();
context2.PushGlobal(model);
Console.WriteLine(template2.Render(context2));

// 3. Built-in functions
var template3 = Template.Parse("{{ 'hello world' | string.upcase }}");
var context3 = new TemplateContext();
context3.PushGlobal(new ScriptObject());
Console.WriteLine(template3.Render(context3));

// 4. Nested ScriptObject
var nested = new ScriptObject();
nested["inner"] = "value";
model["nested"] = nested;
var template4 = Template.Parse("{{nested.inner}}");
var context4 = new TemplateContext();
context4.PushGlobal(model);
Console.WriteLine(template4.Render(context4));

// 5. Contextless interface helpers, including generic arrays
foreach (IScriptObject value in new IScriptObject[] { new ScriptObject(), new ScriptArray(), new ScriptArray<int>() })
{
    value.SetValue("name", "expected", false);
    if (!value.TryGetValue("name", out var actual) || !Equals(actual, "expected"))
    {
        throw new InvalidOperationException("Contextless interface helper failed.");
    }
}

// 6. Guarded encoding builtins use precompiled delegates, including under Native AOT.
var encoded = Template.Parse("{{ 'x' | string.literal }}|{{ 'abcd' | string.base64_encode }}|{{ '&' | html.escape }}|{{ 'a b' | html.url_encode }}|{{ 'a b' | html.url_escape }}|{{ 'a\\n' | html.newline_to_br }}")
    .Render(new TemplateContext());
if (encoded != "\"x\"|YWJjZA==|&amp;|a%20b|a%20b|a<br />\n")
{
    throw new InvalidOperationException("Guarded encoding builtin failed.");
}

Console.WriteLine("AOT test passed!");
return 0;
