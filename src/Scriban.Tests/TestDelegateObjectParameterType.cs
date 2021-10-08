using System;
using NUnit.Framework;
using Scriban.Runtime;

namespace Scriban.Tests
{
    public class TestDelegateObjectParameterType
    {
        public class TargetType
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }

            public InnerTypeClass Value3 { get; set; }

            public class InnerTypeClass
            {
                public string Value4 { get; set; }
            }
        }

        [Test]
        public void TestParameterScribanObject()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("type", new Func<object, string>((parameter) => parameter.GetType().FullName));

            var template = Template.Parse(@"type {}", lexerOptions: new() { Mode = Parsing.ScriptMode.ScriptOnly });
            var result = template.Render(context);

            TextAssert.AreEqual("Scriban.Runtime.ScriptObject", result);
        }

        [Test]
        public void TestParameterTargetType()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("type", new Func<object, string>((parameter) => parameter.GetType().FullName), new Type[] { typeof(TargetType) });

            var template = Template.Parse(@"type {}", lexerOptions: new() { Mode = Parsing.ScriptMode.ScriptOnly });
            var result = template.Render(context);

            TextAssert.AreEqual(typeof(TargetType).FullName, result);
        }

        [Test]
        public void TestParameterTargetTypeValues()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("type", new Func<object, string>((parameter) =>
            {
                var targetValue = (TargetType)parameter;
                return $"{targetValue.Value1}:{targetValue.Value2}";
            }), new Type[] { typeof(TargetType) });

            var template = Template.Parse(@"type {
value1: 'string',
value2: 123
}", lexerOptions: new() { Mode = Parsing.ScriptMode.ScriptOnly });
            var result = template.Render(context);

            TextAssert.AreEqual("string:123", result);
        }

        [Test]
        public void TestParameterTargetInnerTypeValues()
        {
            var context = new TemplateContext();
            context.CurrentGlobal.Import("type", new Func<object, string>((parameter) =>
            {
                var targetValue = (TargetType)parameter;
                return $"{targetValue?.Value3?.Value4}";
            }), new Type[] { typeof(TargetType) });

            var template = Template.Parse(@"type {
value3: {
  value4: 'innerValue'
}
}", lexerOptions: new() { Mode = Parsing.ScriptMode.ScriptOnly });
            var result = template.Render(context);

            TextAssert.AreEqual("innerValue", result);
        }


        [Test]
        public void TestParameterMismatchException()
        {
            var context = new TemplateContext();
            Assert.Throws<ArgumentOutOfRangeException>(() => context.CurrentGlobal.Import("type", new Func<object, string>((parameter) => ""), Array.Empty<Type>()));
        }
    }
}
