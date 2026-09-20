// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Runtime;
using Scriban.Syntax;

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

    [Test]
    public async Task RenderAsyncShouldUseFunctionScopeForParametricFunctions()
    {
        var template = Template.Parse(@"
{{-
my_global_var = 1

func mutate_global(x)
    my_global_var += 1
end

mutate_global 0
my_global_var
-}}
");

        var result = await template.RenderAsync();

        Assert.That(result, Is.EqualTo("2"));
    }

    [Test]
    public void RenderAsyncShouldShareNestedIterationLoopLimit()
    {
        var context = new TemplateContext
        {
            LoopLimit = 10
        };
        var template = Template.Parse("{{ for i in 1..2; [1, 2, 3, 4, 5] | array.reverse | array.size; end }}");

        var exception = Assert.ThrowsAsync<ScriptRuntimeException>(async () => await template.RenderAsync(context));

        StringAssert.Contains("iteration limit `10`", exception!.Message);
    }

    [Test]
    public async Task RenderAsyncShouldResetCumulativeOutputTracking()
    {
        var context = new TemplateContext
        {
            LimitToString = 5
        };
        var largeTemplate = Template.Parse("{{ 'abc' }}{{ 'def' }}");
        var smallTemplate = Template.Parse("{{ 'xy' }}");

        Assert.That(await largeTemplate.RenderAsync(context), Is.EqualTo("abcde..."));
        Assert.That(await smallTemplate.RenderAsync(context), Is.EqualTo("xy"));
    }

    [TestCase(0, "abc...abc...")]
    [TestCase(8, "abc...ab...")]
    public async Task RenderAsyncShouldUseIndependentOutputLimit(int outputLimit, string expected)
    {
        var context = new TemplateContext { LimitToString = 3, OutputLimit = outputLimit };
        var template = Template.Parse("{{ 'abcd' }}{{ 'abcd' }}");

        Assert.That(await template.RenderAsync(context), Is.EqualTo(expected));
        Assert.That(await template.RenderAsync(context), Is.EqualTo(expected));
    }

    [Test]
    public async Task RenderAsyncShouldThrowOnOutputLimitAndRecover()
    {
        var context = new TemplateContext { OutputLimit = 5, OnOutputLimit = ScriptLimitBehavior.Throw };

        var exception = Assert.ThrowsAsync<ScriptRuntimeException>(async () => await Template.Parse("abc{{ 'def' }}").RenderAsync(context));

        StringAssert.Contains("OutputLimit `5`", exception!.Message);
        Assert.That(context.Output.ToString(), Is.EqualTo("abc"));
        context.Reset();
        Assert.That(await Template.Parse("abcde").RenderAsync(context), Is.EqualTo("abcde"));
    }

    [Test]
    public void RenderAsyncShouldThrowOnStringLimit()
    {
        var context = new TemplateContext { LimitToString = 3, OutputLimit = 0, OnStringLimit = ScriptLimitBehavior.Throw };

        var exception = Assert.ThrowsAsync<ScriptRuntimeException>(async () => await Template.Parse("{{ 'abcd' }}").RenderAsync(context));

        StringAssert.Contains("LimitToString `3`", exception!.Message);
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
