using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class AssignTests
    {
        [Test]
        public void TestAssignedVariable()
        {
            Helper.AssertTemplateResult(".foo.", "{% assign foo = values %}.{{ foo[0] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
            Helper.AssertTemplateResult(".bar.", "{% assign foo = values %}.{{ foo[1] }}.",
                Hash.FromAnonymousObject(new { values = new[] { "foo", "bar", "baz" } }));
        }

        [Test]
        public void TestAssignDecimal()
        {
            Helper.AssertTemplateResult(string.Format("10{0}05", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                "{% assign foo = decimal %}{{ foo }}",
                Hash.FromAnonymousObject(new { @decimal = 10.05d }));
        }

        [Test]
        public void TestAssignDecimalInlineWithEnglishDecimalSeparator()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2.5 %}{{ foo }}");
            }
        }

        [Test]
        public void TestAssignDecimalInlineWithEnglishGroupSeparator()
        {
            using (CultureHelper.SetCulture("en-GB"))
            {
                Helper.AssertTemplateResult("2500",
                    "{% assign foo = 2,500 %}{{ foo }}");
            }
        }

        [Test]
        public void TestAssignDecimalInlineWithFrenchDecimalSeparator()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2,5 %}{{ foo }}");
            }
        }

        [Test]
        public void TestAssignDecimalInlineWithInvariantDecimalSeparatorInFrenchCulture()
        {
            using (CultureHelper.SetCulture("fr-FR"))
            {
                Helper.AssertTemplateResult(string.Format("2{0}5", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator),
                    "{% assign foo = 2.5 %}{{ foo }}");
            }
        }

        [Test]
        public void TestAssignWithFilter()
        {
            Helper.AssertTemplateResult(".bar.", "{% assign foo = values | split: ',' %}.{{ foo[1] }}.",
                Hash.FromAnonymousObject(new { values = "foo,bar,baz" }));
        }

        private class AssignDrop : Drop
        {
            public string MyProperty
            {
                get { return "MyValue"; }
            }
        }

        [Test]
        public void TestAssignWithDrop()
        {
            Helper.AssertTemplateResult(".MyValue.", @"{% assign foo = value %}.{{ foo.my_property }}.",
                Hash.FromAnonymousObject(new { value = new AssignDrop() }));
        }
    }
}
