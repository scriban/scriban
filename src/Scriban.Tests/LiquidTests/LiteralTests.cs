using System;
using System.Collections.Generic;
using NUnit.Framework;
using Scriban;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class LiteralTests
    {
        [Test]
        public void TestEmptyLiteral()
        {
            Template t = Template.ParseLiquid("{% literal %}{% endliteral %}");
            Assert.AreEqual(string.Empty, t.Render());
            t = Template.ParseLiquid("{{{}}}");
            Assert.AreEqual(string.Empty, t.Render());
        }

        [Test]
        public void TestSimpleLiteralValue()
        {
            Template t = Template.ParseLiquid("{% literal %}howdy{% endliteral %}");
            Assert.AreEqual("howdy", t.Render());
        }

        [Test]
        public void TestLiteralsIgnoreLiquidMarkup()
        {
            Template t = Template.ParseLiquid("{% literal %}{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}{% endliteral %}");
            Assert.AreEqual("{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}", t.Render());
        }

        [Test]
        public void TestShorthandSyntax()
        {
            Template t = Template.ParseLiquid("{{{{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}}}}");
            Assert.AreEqual("{% if 'gnomeslab' contains 'liquid' %}yes{ % endif %}", t.Render());
        }

        [Test]
        public void TestLiteralsDontRemoveComments()
        {
            Template t = Template.ParseLiquid("{{{ {# comment #} }}}");
            Assert.AreEqual("{# comment #}", t.Render());
        }

        //[Test]
        //public void TestFromShorthand()
        //{
        //    Assert.AreEqual("{% literal %}gnomeslab{% endliteral %}", Literal.FromShortHand("{{{gnomeslab}}}"));
        //}

        //[Test]
        //public void TestFromShorthandIgnoresImproperSyntax()
        //{
        //    Assert.AreEqual("{% if 'hi' == 'hi' %}hi{% endif %}", Literal.FromShortHand("{% if 'hi' == 'hi' %}hi{% endif %}"));
        //}
    }
}
