using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace DotLiquid.Tests.Tags
{
    [TestFixture]
    public class IncludeTagTests
    {
        private class TestFileSystem : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return templateName;
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                switch (templatePath)
                {
                    case "product":
                        return "Product: {{ product.title }} ";

                    case "locale_variables":
                        return "Locale: {{echo1}} {{echo2}}";

                    case "variant":
                        return "Variant: {{ variant.title }}";

                    case "nested_template":
                        return "{% include 'header' %} {% include 'body' %} {% include 'footer' %}";

                    case "body":
                        return "body {% include 'body_detail' %}";

                    case "nested_product_template":
                        return "Product: {{ nested_product_template.title }} {%include 'details'%} ";

                    case "recursively_nested_template":
                        return "-{% include 'recursively_nested_template' %}";

                    case "pick_a_source":
                        return "from TestFileSystem";

                    default:
                        return templatePath;
                }
            }

#if SCRIBAN_ASYNC
            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return new ValueTask<string>(Load(context, callerSpan, templatePath));
            }
#endif
        }

        internal class TestTemplateFileSystem : ITemplateLoader
        {
            private ITemplateLoader _baseFileSystem = null;

            public TestTemplateFileSystem(ITemplateLoader baseFileSystem)
            {
                _baseFileSystem = baseFileSystem;
            }

            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return _baseFileSystem.GetPath(context, callerSpan, templateName);
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return _baseFileSystem.Load(context, callerSpan, templatePath);
            }

#if SCRIBAN_ASYNC
            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return _baseFileSystem.LoadAsync(context, callerSpan, templatePath);
            }
#endif
        }

        private class OtherFileSystem : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return templateName;
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return "from OtherFileSystem";
            }

#if SCRIBAN_ASYNC
            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return new ValueTask<string>(Load(context, callerSpan, templatePath));
            }
#endif
        }

        private class InfiniteFileSystem : ITemplateLoader
        {
            public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
            {
                return templateName;
            }

            public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return "-{% include 'loop' %}";
            }

#if SCRIBAN_ASYNC
            public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
            {
                return new ValueTask<string>(Load(context, callerSpan, templatePath));
            }
#endif
        }

        [SetUp]
        public void SetUp()
        {
            Template.FileSystem = new TestFileSystem();
        }

        //[Test]
        //public void TestIncludeTagMustNotBeConsideredError()
        //{
        //    Assert.AreEqual(0, Template.Parse("{% include 'product_template' %}").Errors.Count);
        //    Assert.DoesNotThrow(() => Template.Parse("{% include 'product_template' %}").Render(null));
        //}

        //[Test]
        //public void TestIncludeTagLooksForFileSystemInRegistersFirst()
        //{
        //    Assert.AreEqual("from OtherFileSystem", Template.Parse("{% include 'pick_a_source' %}").Render(new RenderParameters(CultureInfo.InvariantCulture) { Registers = Hash.FromAnonymousObject(new { file_system = new OtherFileSystem() }) }));
        //}

        [Test]
        public void TestIncludeTagWith()
        {
            Assert.AreEqual("Product: Draft 151cm ", Template.Parse("{% include 'product' with products[0] %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })));
        }

        [Test]
        public void TestIncludeTagWithDefaultName()
        {
            Assert.AreEqual("Product: Draft 151cm ", Template.Parse("{% include 'product' %}").Render(Hash.FromAnonymousObject(new { product = Hash.FromAnonymousObject(new { title = "Draft 151cm" }) })));
        }

        [Test]
        public void TestIncludeTagFor()
        {
            Assert.AreEqual("Product: Draft 151cm Product: Element 155cm ", Template.Parse("{% include 'product' for products %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })));
        }

        [Test]
        public void TestIncludeTagWithLocalVariables()
        {
            Assert.AreEqual("Locale: test123 ", Template.Parse("{% include 'locale_variables' echo1: 'test123' %}").Render());
        }

        [Test]
        public void TestIncludeTagWithMultipleLocalVariables()
        {
            Assert.AreEqual("Locale: test123 test321", Template.Parse("{% include 'locale_variables' echo1: 'test123', echo2: 'test321' %}").Render());
        }

        [Test]
        public void TestIncludeTagWithMultipleLocalVariablesFromContext()
        {
            Assert.AreEqual("Locale: test123 test321",
                Template.Parse("{% include 'locale_variables' echo1: echo1, echo2: more_echos.echo2 %}").Render(Hash.FromAnonymousObject(new { echo1 = "test123", more_echos = Hash.FromAnonymousObject(new { echo2 = "test321" }) })));
        }

        [Test]
        public void TestNestedIncludeTag()
        {
            Assert.AreEqual("body body_detail", Template.Parse("{% include 'body' %}").Render());

            Assert.AreEqual("header body body_detail footer", Template.Parse("{% include 'nested_template' %}").Render());
        }

        [Test]
        public void TestNestedIncludeTagWithVariable()
        {
            Assert.AreEqual("Product: Draft 151cm details ",
                Template.Parse("{% include 'nested_product_template' with product %}").Render(Hash.FromAnonymousObject(new { product = Hash.FromAnonymousObject(new { title = "Draft 151cm" }) })));

            Assert.AreEqual("Product: Draft 151cm details Product: Element 155cm details ",
                Template.Parse("{% include 'nested_product_template' for products %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })));
        }

        //[Test]
        //public void TestRecursivelyIncludedTemplateDoesNotProductEndlessLoop()
        //{
        //    Template.FileSystem = new InfiniteFileSystem();

        //    Assert.Throws<StackLevelException>(() => Template.Parse("{% include 'loop' %}").Render(new RenderParameters(CultureInfo.InvariantCulture) { RethrowErrors = true }));
        //}

        [Test]
        public void TestDynamicallyChosenTemplate()
        {
            Assert.AreEqual("Test123", Template.Parse("{% include template %}").Render(Hash.FromAnonymousObject(new { template = "Test123" })));
            Assert.AreEqual("Test321", Template.Parse("{% include template %}").Render(Hash.FromAnonymousObject(new { template = "Test321" })));

            Assert.AreEqual("Product: Draft 151cm ", Template.Parse("{% include template with product %}").Render(Hash.FromAnonymousObject(new { template = "product", product = Hash.FromAnonymousObject(new { title = "Draft 151cm" }) })));
        }

        [Test]
        public void TestUndefinedTemplateVariableWithTestFileSystem()
        {
            Assert.AreEqual(" hello  world ", Template.Parse(" hello {% include notthere %} world ").Render());
        }

        //[Test]
        //public void TestUndefinedTemplateVariableWithLocalFileSystem()
        //{
        //    Template.FileSystem = new LocalFileSystem(string.Empty);
        //    Assert.Throws<FileSystemException>(() => Template.Parse(" hello {% include notthere %} world ").Render(new RenderParameters(CultureInfo.InvariantCulture)
        //    {
        //        RethrowErrors = true
        //    }));
        //}

        //[Test]
        //public void TestMissingTemplateWithLocalFileSystem()
        //{
        //    Template.FileSystem = new LocalFileSystem(string.Empty);
        //    Assert.Throws<FileSystemException>(() => Template.Parse(" hello {% include 'doesnotexist' %} world ").Render(new RenderParameters(CultureInfo.InvariantCulture)
        //    {
        //        RethrowErrors = true
        //    }));
        //}

        //[Test]
        //public void TestIncludeFromTemplateFileSystem()
        //{
        //    var fileSystem = new TestTemplateFileSystem(new TestFileSystem());
        //    Template.FileSystem = fileSystem;
        //    for (int i = 0; i < 2; ++i)
        //    {
        //        Assert.AreEqual("Product: Draft 151cm ", Template.Parse("{% include 'product' with products[0] %}").Render(Hash.FromAnonymousObject(new { products = new[] { Hash.FromAnonymousObject(new { title = "Draft 151cm" }), Hash.FromAnonymousObject(new { title = "Element 155cm" }) } })));
        //    }
        //    Assert.AreEqual(fileSystem.CacheHitTimes, 1);
        //}

        class Template
        {
            private Scriban.Template _template;
            public Template(Scriban.Template template)
            {
                _template = template;
            }

            public static ITemplateLoader FileSystem;

            public static Template Parse(string text)
            {
                var scriban = Scriban.Template.ParseLiquid(text);

                Console.WriteLine(scriban.ToText());
                return new Template(scriban);
            }

            public string Render(object model = null)
            {
                var context = new LiquidTemplateContext {TemplateLoader = FileSystem};
                var obj = new ScriptObject();
                if (model != null)
                {
                    obj.Import(model);
                }
                context.PushGlobal(obj);
                return _template.Render(context);
            }
        }
    }
}
