// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Runtime;

namespace Scriban.Tests;

public class TestAsync
{
    [Test]
    public async Task AccessDirectlyOnFunctionResult()
    {
        var templateBody = "{{my_function().value}}";

        var templateContext = new TemplateContext
        {
            EnableRelaxedMemberAccess = false,
            StrictVariables = true
        };

        var template = Template.Parse(templateBody);
        Assert.That(template.HasErrors, Is.False);

        var so = new ScriptObject();
        so.Import("my_function", new Func<Task<ValueWrapper>>(async () =>
        {
            await Task.Delay(1);
            return new ValueWrapper("hello");
        }));

        templateContext.PushGlobal(so);

        var result = await template.RenderAsync(templateContext);

        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public async Task IndirectAccess()
    {
        var templateBody = @"{{v = my_function()
v.value}}";

        var templateContext = new TemplateContext
        {
            EnableRelaxedMemberAccess = false,
            StrictVariables = true
        };

        var template = Template.Parse(templateBody);
        Assert.That(template.HasErrors, Is.False);

        var so = new ScriptObject();
        so.Import("my_function", new Func<Task<ValueWrapper>>(async () =>
        {
            await Task.Delay(1);
            return new ValueWrapper("hello");
        }));

        templateContext.PushGlobal(so);

        var result = await template.RenderAsync(templateContext);

        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public async Task NullConditionalShouldShortCircuitFollowingIndexersAsync()
    {
        var template = Template.Parse("{{ a?.b[0][1] }}");

        var nullResult = await template.RenderAsync(new { a = (object?)null });
        Assert.That(nullResult, Is.EqualTo(string.Empty));

        var valueResult = await template.RenderAsync(new { a = new { b = new[] { new[] { "skip", "ok" } } } });
        Assert.That(valueResult, Is.EqualTo("ok"));
    }

    [Test]
    public async Task RenderAsyncShouldAwaitTaskMemberValues()
    {
        var template = Template.Parse("{{ value }}|{{ value + 1 }}");

        var result = await template.RenderAsync(new { value = Task.FromResult(41) });

        Assert.That(result, Is.EqualTo("41|42"));
    }

    [Test]
    public async Task RenderAsyncShouldAwaitValueTaskMemberValues()
    {
        var template = Template.Parse("{{ value }}");

        var result = await template.RenderAsync(new { value = ValueTask.FromResult("hello") });

        Assert.That(result, Is.EqualTo("hello"));
    }

    public class ValueWrapper
    {
        public string Value { get; set; }


        public ValueWrapper(string value)
        {
            Value = value;
        }
    }

}
