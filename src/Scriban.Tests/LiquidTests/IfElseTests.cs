using System;
using System.Collections;
using NUnit.Framework;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class IfElseTests
    {
        [Test]
        public void TestIf()
        {
            Helper.AssertTemplateResult("  ", " {% if false %} this text should not go into the output {% endif %} ");
            Helper.AssertTemplateResult("  this text should go into the output  ", " {% if true %} this text should go into the output {% endif %} ");
            Helper.AssertTemplateResult("  you rock ?", "{% if false %} you suck {% endif %} {% if true %} you rock {% endif %}?");
        }

        [Test]
        public void TestIfElse()
        {
            Helper.AssertTemplateResult(" YES ", "{% if false %} NO {% else %} YES {% endif %}");
            Helper.AssertTemplateResult(" YES ", "{% if true %} YES {% else %} NO {% endif %}");
            Helper.AssertTemplateResult(" YES ", "{% if 'foo' %} YES {% else %} NO {% endif %}");
        }

        [Test]
        public void TestIfBoolean()
        {
            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = true }));
        }

        [Test]
        public void TestIfOr()
        {
            Helper.AssertTemplateResult(" YES ", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = true, b = true }));
            Helper.AssertTemplateResult(" YES ", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = true, b = false }));
            Helper.AssertTemplateResult(" YES ", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = false, b = true }));
            Helper.AssertTemplateResult("", "{% if a or b %} YES {% endif %}", Hash.FromAnonymousObject(new { a = false, b = false }));

            Helper.AssertTemplateResult(" YES ", "{% if a or b or c %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = false, b = false, c = true }));
            Helper.AssertTemplateResult("", "{% if a or b or c %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = false, b = false, c = false }));
        }

        [Test]
        public void TestIfOrWithOperators()
        {
            Helper.AssertTemplateResult(" YES ", "{% if a == true or b == true %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = true, b = true }));
            Helper.AssertTemplateResult(" YES ", "{% if a == true or b == false %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = true, b = true }));
            Helper.AssertTemplateResult("", "{% if a == false or b == false %} YES {% endif %}",
                Hash.FromAnonymousObject(new { a = true, b = true }));
        }

        [Test]
        public void TestComparisonOfStringsContainingAndOrOr()
        {
            Assert.DoesNotThrow(() =>
            {
                const string awfulMarkup = "a == 'and' and b == 'or' and c == 'foo and bar' and d == 'bar or baz' and e == 'foo' and foo and bar";
                Hash assigns = Hash.FromAnonymousObject(new { a = "and", b = "or", c = "foo and bar", d = "bar or baz", e = "foo", foo = true, bar = true });
                Helper.AssertTemplateResult(" YES ", "{% if " + awfulMarkup + " %} YES {% endif %}", assigns);
            });
        }

        [Test]
        public void TestIfAnd()
        {
            Helper.AssertTemplateResult(" YES ", "{% if true and true %} YES {% endif %}");
            Helper.AssertTemplateResult("", "{% if false and true %} YES {% endif %}");
            Helper.AssertTemplateResult("", "{% if false and true %} YES {% endif %}");
        }

        [Test]
        public void TestHashMissGeneratesFalse()
        {
            Helper.AssertTemplateResult("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = new Hash() }));
        }

        [Test]
        public void TestIfFromVariable()
        {
            const object nullValue = null;

            Helper.AssertTemplateResult("", "{% if var %} NO {% endif %}", Hash.FromAnonymousObject(new { var = false }));
            Helper.AssertTemplateResult("", "{% if var %} NO {% endif %}", Hash.FromAnonymousObject(new { var = nullValue }));
            Helper.AssertTemplateResult("", "{% if foo.bar %} NO {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = false }) }));
            Helper.AssertTemplateResult("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = new Hash() }));
            Helper.AssertTemplateResult("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = nullValue }));
            Helper.AssertTemplateResult("", "{% if foo.bar %} NO {% endif %}", Hash.FromAnonymousObject(new { foo = true }));

            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = "text" }));
            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = true }));
            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = 1 }));
            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = new Hash() }));
            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% endif %}", Hash.FromAnonymousObject(new { var = new object[] { } }));
            Helper.AssertTemplateResult(" YES ", "{% if 'foo' %} YES {% endif %}");
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = true }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "text" }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = 1 }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = new Hash() }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = new object[] { } }) }));

            Helper.AssertTemplateResult(" YES ", "{% if var %} NO {% else %} YES {% endif %}", Hash.FromAnonymousObject(new { var = false }));
            Helper.AssertTemplateResult(" YES ", "{% if var %} NO {% else %} YES {% endif %}", Hash.FromAnonymousObject(new { var = nullValue }));
            Helper.AssertTemplateResult(" YES ", "{% if var %} YES {% else %} NO {% endif %}", Hash.FromAnonymousObject(new { var = true }));
            Helper.AssertTemplateResult(" YES ", "{% if 'foo' %} YES {% else %} NO {% endif %}", Hash.FromAnonymousObject(new { var = "text" }));

            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = false }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% else %} NO {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = true }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} YES {% else %} NO {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { bar = "text" }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = Hash.FromAnonymousObject(new { notbar = true }) }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { foo = new Hash() }));
            Helper.AssertTemplateResult(" YES ", "{% if foo.bar %} NO {% else %} YES {% endif %}",
                Hash.FromAnonymousObject(new { notfoo = Hash.FromAnonymousObject(new { bar = true }) }));
        }

        [Test]
        public void TestNestedIf()
        {
            Helper.AssertTemplateResult("", "{% if false %}{% if false %} NO {% endif %}{% endif %}");
            Helper.AssertTemplateResult("", "{% if false %}{% if true %} NO {% endif %}{% endif %}");
            Helper.AssertTemplateResult("", "{% if true %}{% if false %} NO {% endif %}{% endif %}");
            Helper.AssertTemplateResult(" YES ", "{% if true %}{% if true %} YES {% endif %}{% endif %}");

            Helper.AssertTemplateResult(" YES ",
                "{% if true %}{% if true %} YES {% else %} NO {% endif %}{% else %} NO {% endif %}");
            Helper.AssertTemplateResult(" YES ",
                "{% if true %}{% if false %} NO {% else %} YES {% endif %}{% else %} NO {% endif %}");
            Helper.AssertTemplateResult(" YES ",
                "{% if false %}{% if true %} NO {% else %} NONO {% endif %}{% else %} YES {% endif %}");
        }

        [Test]
        public void TestComparisonsOnNull()
        {
            Helper.AssertTemplateResult("", "{% if null < 10 %} NO {% endif %}");
            Helper.AssertTemplateResult("", "{% if null <= 10 %} NO {% endif %}");
            Helper.AssertTemplateResult("", "{% if null >= 10 %} NO {% endif %}");
            Helper.AssertTemplateResult("", "{% if null > 10 %} NO {% endif %}");

            Helper.AssertTemplateResult("", "{% if 10 < null %} NO {% endif %}");
            Helper.AssertTemplateResult("", "{% if 10 <= null %} NO {% endif %}");
            Helper.AssertTemplateResult("", "{% if 10 >= null %} NO {% endif %}");
            Helper.AssertTemplateResult("", "{% if 10 > null %} NO {% endif %}");
        }

        [Test]
        public void TestElseIf()
        {
            Helper.AssertTemplateResult("0", "{% if 0 == 0 %}0{% elsif 1 == 1%}1{% else %}2{% endif %}");
            Helper.AssertTemplateResult("1", "{% if 0 != 0 %}0{% elsif 1 == 1%}1{% else %}2{% endif %}");
            Helper.AssertTemplateResult("2", "{% if 0 != 0 %}0{% elsif 1 != 1%}1{% else %}2{% endif %}");

            Helper.AssertTemplateResult("elsif", "{% if false %}if{% elsif true %}elsif{% endif %}");
        }
    }
}
