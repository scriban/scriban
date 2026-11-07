// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Functions;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests;

[TestFixture]
public class TestResourceGuards
{
    [Test]
    public void ResetClearsCycleStateAndPreservesHostTags()
    {
        var context = new TemplateContext();
        var hostValue = new object();
        context.Tags["host"] = hostValue;
        var template = Template.Parse("{{ array.cycle ['one', 'two', 'three'] }}");

        for (var i = 0; i < 3; i++)
        {
            Assert.AreEqual("one", template.Render(context));
            Assert.AreEqual("two", template.Render(context));
            context.Reset();
            Assert.AreEqual(1, context.Tags.Count);
            Assert.AreSame(hostValue, context.Tags["host"]);
        }
    }

    [Test]
    public void BuiltinStateDoesNotUseHostTags()
    {
        var context = new TemplateContext();
        context.Tags["random"] = "host-owned";
        var template = Template.Parse("{{ array.cycle ['a', 'b'] 'group'; math.random 1 2 }}");

        Assert.AreEqual("a1", template.Render(context));
        context.Reset();
        Assert.AreEqual("a1", template.Render(context));
        Assert.AreEqual(1, context.Tags.Count);
        Assert.AreEqual("host-owned", context.Tags["random"]);
    }

    [TestCase("[1,2,3]")]
    [TestCase("{\"a\":1,\"b\":2,\"c\":3}")]
    [TestCase("[[1],[2]]")]
    [TestCase("{\"a\":{\"b\":1},\"c\":2}")]
    public void FromJsonSharesLoopBudgetAcrossNestedValues(string json)
    {
        var context = new TemplateContext { LoopLimit = 2 };
        var exception = Assert.Throws<ScriptRuntimeException>(() => ObjectFunctions.FromJson(context, json));
        StringAssert.Contains("iteration limit `2`", exception!.Message);
        Assert.AreEqual(1m, ObjectFunctions.FromJson(context, "[1]") is ScriptArray array ? array[0] : null);
    }

    [Test]
    public void FromJsonChecksCancellationAndSupportsDisabledLimit()
    {
        var context = new TemplateContext { LoopLimit = 0 };
        Assert.AreEqual(3, ((ScriptArray)ObjectFunctions.FromJson(context, "[1,2,3]")!).Count);
        context.CancellationToken = new CancellationToken(true);
        Assert.Throws<ScriptAbortException>(() => ObjectFunctions.FromJson(context, "1"));
    }

    [Test]
    public void FromJsonRetainsContextlessHostCompatibility()
    {
        Assert.AreEqual(2, ((ScriptArray)ObjectFunctions.FromJson(null, "[1,2]")!).Count);
    }

    [TestCase("'\\\\\\\\' | string.literal", 5)]
    [TestCase("'abcd' | string.base64_encode", 7)]
    [TestCase("'&&' | html.escape", 9)]
    [TestCase("'\\n\\r\\n' | html.newline_to_br", 14)]
    [TestCase("'a b' | html.url_encode", 4)]
    [TestCase("'a b' | html.url_escape", 4)]
    [TestCase("'😀' | html.url_encode", 11)]
    public void ExpandingBuiltinsRejectOversizedResults(string expression, int limit)
    {
        var context = new TemplateContext { LimitToString = limit, OutputLimit = 0 };
        var exception = Assert.Throws<ScriptRuntimeException>(() => Template.Parse("{{ " + expression + " }}").Evaluate(context));
        StringAssert.Contains("LimitToString", exception!.Message);
    }

    [TestCase("'\\\\\\\\' | string.literal", 6, "\"\\\\\\\\\"")]
    [TestCase("'abcd' | string.base64_encode", 8, "YWJjZA==")]
    [TestCase("'&&' | html.escape", 10, "&amp;&amp;")]
    [TestCase("'\\n\\r\\n' | html.newline_to_br", 15, "<br />\n<br />\r\n")]
    [TestCase("'a b' | html.url_encode", 5, "a%20b")]
    [TestCase("'a b' | html.url_escape", 5, "a%20b")]
    [TestCase("'😀' | html.url_encode", 12, "%F0%9F%98%80")]
    [TestCase("'a&b' | html.url_escape", 3, "a&b")]
    [TestCase("'&&' | html.escape", 0, "&amp;&amp;")]
    public void ExpandingBuiltinsAllowExactAndDisabledLimits(string expression, int limit, string expected)
    {
        var context = new TemplateContext { LimitToString = limit, OutputLimit = 0 };
        Assert.AreEqual(expected, Template.Parse("{{ " + expression + " }}").Evaluate(context));
    }

    [TestCase("{{ s = '\\\\'; for i in 1..20; s = s | string.literal; end }}")]
    [TestCase("{{ s = 'abc'; for i in 1..20; s = s | string.base64_encode; end }}")]
    [TestCase("{{ s = '&'; for i in 1..20; s = s | html.escape; end }}")]
    public void RepeatedBuiltinExpansionStopsAtStringLimit(string script)
    {
        var context = new TemplateContext { LimitToString = 64, OutputLimit = 0 };
        var exception = Assert.Throws<ScriptRuntimeException>(() => Template.Parse(script).Evaluate(context));
        StringAssert.Contains("LimitToString", exception!.Message);
    }

    [TestCase("abcdefghijklmnop")]
    [TestCase("\u0001\u0001\u0001")]
    public void ToJsonEnforcesStringLimit(string value)
    {
        var context = new TemplateContext { LimitToString = 16 };
        Assert.Throws<InvalidOperationException>(() => ObjectFunctions.ToJson(context, value));
    }

    [Test]
    public void ToJsonEnforcesCumulativeStringLimit()
    {
        var context = new TemplateContext { LimitToString = 16 };
        var value = new ScriptArray { "abcd", "efgh", "ijkl" };
        Assert.Throws<InvalidOperationException>(() => ObjectFunctions.ToJson(context, value));
        Assert.AreEqual("\"ab\"", ObjectFunctions.ToJson(context, "ab"));
        context.LimitToString = 0;
        Assert.AreEqual("[\"abcd\",\"efgh\",\"ijkl\"]", ObjectFunctions.ToJson(context, value));
    }

    [Test]
    public void ToJsonAllowsExactLimit()
    {
        var context = new TemplateContext { LimitToString = 6 };
        Assert.AreEqual("\"abcd\"", ObjectFunctions.ToJson(context, "abcd"));
    }

    [Test]
    public void ToJsonBoundsEscapedValuesAndPropertyNames()
    {
        var context = new TemplateContext { LimitToString = 16 };
        Assert.Throws<InvalidOperationException>(() => ObjectFunctions.ToJson(context, new ScriptObject { [new string('k', 32)] = 1 }));
        Assert.Throws<InvalidOperationException>(() => ObjectFunctions.ToJson(context, new ScriptObject { ["a"] = "\u0001\u0001\u0001" }));
        Assert.AreEqual("{\"a\":1}", ObjectFunctions.ToJson(context, new ScriptObject { ["a"] = 1 }));
        context.LoopLimit = 1;
        Assert.Throws<ScriptRuntimeException>(() => ObjectFunctions.ToJson(context, new ScriptObject { ["a"] = new ScriptArray { 1 } }));
    }

    [Test]
    public void ToJsonDoesNotReplaceLoopLimitErrorsWhileCleaningUp()
    {
        var context = new TemplateContext { LimitToString = 8, LoopLimit = 2 };
        var value = new ScriptArray { "abcd", new ScriptArray { 1 } };
        var exception = Assert.Throws<ScriptRuntimeException>(() => ObjectFunctions.ToJson(context, value));
        StringAssert.Contains("iteration limit `2`", exception!.Message);
    }

    [Test]
    public void ToJsonDoesNotReplaceCancellationWhileCleaningUp()
    {
        using var cancellation = new CancellationTokenSource();
        var context = new TemplateContext { LimitToString = 8, CancellationToken = cancellation.Token };
        var value = new ScriptArray { "abcd", new CancellingList(cancellation) { 1 } };
        Assert.Throws<ScriptAbortException>(() => ObjectFunctions.ToJson(context, value));
    }

    [Test]
    public void ContextAwareEncodersMatchLegacyOverloads()
    {
        var context = new TemplateContext { LimitToString = 0 };
        foreach (var text in new[] { null, "", "\"\u0001\\", "éÿ😀\ud800", "a\r\nb\nc\rd", "https://a/b?c='d e'&f=1" })
        {
            Assert.AreEqual(StringFunctions.Literal(text), StringFunctions.Literal(context, text));
            Assert.AreEqual(StringFunctions.Base64Encode(text), StringFunctions.Base64Encode(context, text));
            Assert.AreEqual(HtmlFunctions.Escape(text), HtmlFunctions.Escape(context, text));
            Assert.AreEqual(HtmlFunctions.NewlineToBr(text), HtmlFunctions.NewlineToBr(context, text));
            Assert.AreEqual(HtmlFunctions.UrlEncode(text), HtmlFunctions.UrlEncode(context, text));
            Assert.AreEqual(HtmlFunctions.UrlEscape(text), HtmlFunctions.UrlEscape(context, text));
        }
    }

    [Test]
    public void UrlAndHtmlLimitsCountUnicodeAndInvalidSurrogatesExactly()
    {
        foreach (var text in new[] { "é", "ÿ", "😀", "\ud800", "\udc00", "\ud800a", "&'\"" })
        {
            var url = HtmlFunctions.UrlEncode(text)!;
            Assert.AreEqual(url, HtmlFunctions.UrlEncode(new TemplateContext { LimitToString = url.Length }, text));
            Assert.Throws<InvalidOperationException>(() => HtmlFunctions.UrlEncode(new TemplateContext { LimitToString = url.Length - 1 }, text));
            var html = HtmlFunctions.Escape(text)!;
            Assert.AreEqual(html, HtmlFunctions.Escape(new TemplateContext { LimitToString = html.Length }, text));
            if (html.Length > 1)
            {
                Assert.Throws<InvalidOperationException>(() => HtmlFunctions.Escape(new TemplateContext { LimitToString = html.Length - 1 }, text));
            }
        }
    }

    [Test]
    public async Task AsyncRenderingUsesTheSameGuards()
    {
        var context = new TemplateContext { LimitToString = 8, OutputLimit = 0 };
        Assert.ThrowsAsync<ScriptRuntimeException>(async () =>
            await Template.Parse("{{ '&&' | html.escape }}").RenderAsync(context));
        Assert.AreEqual("YWJjZA==...", await Template.Parse("{{ 'abcd' | string.base64_encode }}").RenderAsync(context));
        context.LoopLimit = 1;
        Assert.ThrowsAsync<ScriptRuntimeException>(async () =>
            await Template.Parse("{{ '[1,2]' | object.from_json }}").RenderAsync(context));
    }

    [TestCase(ScriptLimitBehavior.Truncate)]
    [TestCase(ScriptLimitBehavior.Throw)]
    public void ObjectFormattingStopsBeforeReadingFurtherMembers(ScriptLimitBehavior behavior)
    {
        var context = new TemplateContext { LimitToString = 16, OnStringLimit = behavior };
        var value = new ScriptObject { ["a"] = new string('x', 64), ["b"] = new UnexpectedFormat() };
        if (behavior == ScriptLimitBehavior.Throw)
        {
            Assert.Throws<ScriptRuntimeException>(() => context.ObjectToString(value));
        }
        else
        {
            var text = context.ObjectToString(value)!;
            Assert.AreEqual(19, text.Length);
            StringAssert.EndsWith("...", text);
        }
        Assert.AreEqual("ok", context.ObjectToString("ok"));
    }

    [Test]
    public void NestedObjectsAndEscapedStringsRespectStringLimit()
    {
        var context = new TemplateContext { LimitToString = 16 };
        var value = new ScriptObject { ["a"] = new ScriptObject { ["b"] = new string('\\', 64) } };
        Assert.AreEqual(19, context.ObjectToString(value)!.Length);
        Assert.AreEqual(context.ObjectToString(value), value.ToString(null, context));
        Assert.AreEqual(19, context.ObjectToString(new string('\\', 64), true)!.Length);
    }

    [Test]
    public void EnumerablesStopBeforeReadingFurtherItems()
    {
        var context = new TemplateContext { LimitToString = 16 };
        Assert.AreEqual(19, context.ObjectToString(new object[] { new string('\\', 64), new UnexpectedFormat() })!.Length);
        context.LimitToString = 0;
        Assert.AreEqual("{a: \"ok\"}", context.ObjectToString(new ScriptObject { ["a"] = "ok" }));
    }

    [Test]
    public void FormattingDoesNotConsumeTheEnclosingLoopBudget()
    {
        var context = new TemplateContext { LoopLimit = 2, LimitToString = 0 };
        Assert.AreEqual("{a: [1, 2]}", context.ObjectToString(new ScriptObject { ["a"] = new[] { 1, 2 } }));
        Assert.AreEqual("{a: 1}", context.ObjectToString(new ScriptObject { ["a"] = 1 }));
    }

    [TestCase("{{ for i in 1..400 }}{{ [1,2] }}{{ end }}", 2400)]
    [TestCase("{{ o = {a:1,b:2,c:3}; for i in 1..300 }}{{ o }}{{ end }}", 5400)]
    [TestCase("{{ a = [1,2,3,4]; for i in 1..300; x = 'v' + a; end }}", 0)]
    public void FormattingInsideLoopsPreservesExistingIterationLimits(string script, int length)
    {
        Assert.AreEqual(length, Template.Parse(script).Render(new TemplateContext()).Length);
    }

    [Test]
    public void FormattingHostCollectionsLargerThanLoopLimitStillWorks()
    {
        var context = new TemplateContext();
        var items = Enumerable.Range(0, 1500).ToList();
        Assert.AreEqual("[" + string.Join(", ", items) + "]", context.ObjectToString(items));
        var dictionary = Enumerable.Range(0, 400).ToDictionary(i => "k" + i, i => i);
        Assert.DoesNotThrow(() => context.ObjectToString(dictionary));
    }

    [Test]
    public void FormattingStillChecksCancellation()
    {
        var context = new TemplateContext { CancellationToken = new CancellationToken(true) };
        Assert.Throws<ScriptAbortException>(() => context.ObjectToString(new[] { 1 }));
        Assert.Throws<ScriptAbortException>(() => context.ObjectToString(new ScriptObject { ["a"] = 1 }));
    }

    [Test]
    public void HostObjectFormattingOverrideCanCallScriptObjectToString()
    {
        var context = new HostFormattingContext { LimitToString = 16 };
        var value = new ScriptObject { ["a"] = new string('x', 64) };
        var globals = new ScriptObject { ["o"] = value };
        context.PushGlobal(globals);
        var expected = new TemplateContext { LimitToString = 16 }.ObjectToString(value);
        Assert.AreEqual(expected, context.ObjectToString(value));
        Assert.AreEqual(expected, value.ToString(null, context));
        context.OutputLimit = 0;
        Assert.AreEqual(expected, Template.Parse("{{ o }}").Render(context));
    }

    [Test]
    public void DirectObjectFormattingDoesNotInvokeDerivedFormatterTwice()
    {
        var context = new TemplateContext();
        var value = new DecoratedScriptObject { ["a"] = 1 };
        Assert.AreEqual("<{a: 1}>", value.ToString(null, context));
        Assert.AreEqual("<{a: 1}>", context.ObjectToString(value));
    }

    private sealed class DecoratedScriptObject : ScriptObject
    {
        public override string ToString(string? format, IFormatProvider? formatProvider)
        {
            return "<" + base.ToString(format, formatProvider) + ">";
        }
    }

    private sealed class HostFormattingContext : TemplateContext
    {
        private int _depth;

        public override string? ObjectToString(object? value, bool nested = false)
        {
            // Fail safely rather than overflowing the process stack if the formatter re-enters.
            if (++_depth > 8) Assert.Fail("ObjectToString re-entered the host override.");
            try
            {
                return value is ScriptObject obj ? obj.ToString(null, this) : base.ObjectToString(value, nested);
            }
            finally
            {
                _depth--;
            }
        }
    }

    private sealed class CancellingList : ArrayList
    {
        private readonly CancellationTokenSource _cancellation;

        public CancellingList(CancellationTokenSource cancellation)
        {
            _cancellation = cancellation;
        }

        public override IEnumerator GetEnumerator()
        {
            _cancellation.Cancel();
            return base.GetEnumerator();
        }
    }

    private sealed class UnexpectedFormat : IFormattable
    {
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            Assert.Fail("Formatting continued after the string budget was exhausted.");
            return string.Empty;
        }
    }
}
