using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    public class TestQueryable
    {
        [Test]
        public void TestQueryableAll ()
        {
            var context = new TemplateContext();
                context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
item
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("0123456789", result);
        }

        [Test]
        public void TestQueryableOffset()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data offset:2
item
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("23456789", result);
        }

        [Test]
        public void TestQueryableLimit()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data limit:2
item
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("01", result);
        }

        [Test]
        public void TestQueryableReversed()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data reversed
item
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("9876543210", result);
        }


        [Test]
        public void TestQueryableIndex()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(5, 5).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.index
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("01234", result);
        }

         [Test]
        public void TestQueryableRIndex()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 5).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.rindex
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("43210", result);
        }


        [Test]
        public void TestQueryableFirst()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 4).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.first
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("truefalsefalsefalse", result);
        }

        [Test]
        public void TestQueryableLast()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 5).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.last
end
}}");
            var result = template.Render(context);

            // last will always be false with iqueryable
            TextAssert.AreEqual("falsefalsefalsefalsetrue", result);
        }


        [Test]
        public void TestQueryableEven()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 4).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.even
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("truefalsetruefalse", result);
        }

        [Test]
        public void TestQueryableOdd()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 4).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.odd
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("falsetruefalsetrue", result);
        }

        [Test]
        public void TestQueryableChanged()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => new[] { 0,0,1,1,2 }.AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
for.changed
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("truefalsetruefalsetrue", result);
        }


        [Test]
        public void TestQueryableLoopLimit()
        {
            var context = new TemplateContext
            {
                LoopLimit = 5
            };
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
item
end
}}");
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            TextAssert.AreEqual("<input>(2,1) : error : Exceeding number of iteration limit `5` for loop statement.", exception.Message);

        }

        [Test]
        public void TestQueryableLoopLimitQueryableOverride()
        {
            var context = new TemplateContext
            {
                LoopLimit = 5,
                LoopLimitQueryable = 6,
            };
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
item
end
}}");
            var exception = Assert.Throws<ScriptRuntimeException>(() => template.Render(context));
            TextAssert.AreEqual("<input>(2,1) : error : Exceeding number of iteration limit `6` for loop statement.", exception.Message);

        }

        [Test]
        public void TestQueryableLoopLimitQueryableDisable()
        {
            var context = new TemplateContext
            {
                LoopLimit = 5,
                LoopLimitQueryable = 0,
            };
            context.CurrentGlobal.Import("data", new Func<IQueryable<int>>(() => Enumerable.Range(0, 10).AsQueryable()));

            var template = Template.Parse(@"{{
for item in data
item
end
}}");
            var result = template.Render(context);

            TextAssert.AreEqual("0123456789", result);
        }
    }
}
