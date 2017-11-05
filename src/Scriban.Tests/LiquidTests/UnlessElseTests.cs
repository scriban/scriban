using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class UnlessElseTests
    {
        [Test]
        public void TestUnless()
        {
            Helper.AssertTemplateResult("  ", " {% unless true %} this text should not go into the output {% endunless %} ");
            Helper.AssertTemplateResult("  this text should go into the output  ", " {% unless false %} this text should go into the output {% endunless %} ");
            Helper.AssertTemplateResult("  you rock ?", "{% unless true %} you suck {% endunless %} {% unless false %} you rock {% endunless %}?");
        }

        [Test]
        public void TestUnlessElse()
        {
            Helper.AssertTemplateResult(" YES ", "{% unless true %} NO {% else %} YES {% endunless %}");
            Helper.AssertTemplateResult(" YES ", "{% unless false %} YES {% else %} NO {% endunless %}");
            Helper.AssertTemplateResult(" YES ", "{% unless 'foo' %} NO {% else %} YES {% endunless %}");
        }

        [Test]
        public void TestUnlessInLoop()
        {
            Helper.AssertTemplateResult("23", "{% for i in choices %}{% unless i %}{{ forloop.index }}{% endunless %}{% endfor %}",
                Hash.FromAnonymousObject(new { choices = new object[] { 1, null, false } }));
        }

        [Test]
        public void TestUnlessElseInLoop()
        {
            Helper.AssertTemplateResult(" TRUE  2  3 ", "{% for i in choices %}{% unless i %} {{ forloop.index }} {% else %} TRUE {% endunless %}{% endfor %}",
                Hash.FromAnonymousObject(new { choices = new object[] { 1, null, false } }));
        }
    }
}
