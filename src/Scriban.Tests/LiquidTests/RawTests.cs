using NUnit.Framework;
using Scriban;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class RawTests
    {
        [Test]
        public void TestTagInRaw ()
        {
            Helper.AssertTemplateResult ("{% comment %} test {% endcomment %}",
                "{% raw %}{% comment %} test {% endcomment %}{% endraw %}");
        }

        [Test]
        public void TestOutputInRaw()
        {
            Helper.AssertTemplateResult("{{ test }}",
                "{% raw %}{{ test }}{% endraw %}");
        }

        [Test]
        public void TestFunctionFileOutputInRaw()
        {
            Helper.AssertTemplateResult("{{ 2 | plus: 2 }} equals 4.",
                "{% raw %}{{ 2 | plus: 2 }} equals 4.{% endraw %}");
        }

        [Test]
        public void TestInputFileOutputInRaw()
        {
            var fileName = "test.liquid";
            System.IO.File.WriteAllText(fileName, "{% raw %}{{ 2 | plus: 2 }} equals 4.{% endraw %}");
            var template = Template.Parse(System.IO.File.ReadAllText(fileName), fileName);
            var result = template.Render();

            Assert.Equals("{{ 2 | plus: 2 }} equals 4.", result);

            System.IO.File.Delete(fileName);
        }

        [Test]
        public void TestRawAndFollowing()
        {
            Helper.AssertTemplateResult("{{ test }}65",
                "{% raw %}{{ test }}{% endraw %}6{{ 5 }}");
        }
    }
}
