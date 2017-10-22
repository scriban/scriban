using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Scriban.Benchmarks
{
    /// <summary>
    /// Simple benchmark between Scriban, DotLiquid, Stubble, Nustache and HandleBars.NET
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            //var program = new BenchRenderers();
            //var result1 = program.TestScriban();
            //var result2 = program.TestDotLiquid();
            //var result3 = program.TestStubble();
            //var result4 = program.TestNustache();
            //var result5 = program.TestHandlebars();
            BenchmarkRunner.Run<BenchParsers>();
            BenchmarkRunner.Run<BenchRenderers>();
        }
    }

    /// <summary>
    /// A benchmark for template parsers.
    /// </summary>
    [MemoryDiagnoser]
    public class BenchParsers
    {
        public BenchParsers()
        {
            // Due to issue https://github.com/rexm/Handlebars.Net/issues/105 cannot do the same as others, so 
            // working around this here
            HandlebarsDotNet.Handlebars.RegisterHelper("truncate", (output, options, context, arguments) => {
                output.Write(Scriban.Functions.StringFunctions.Truncate(15, context["description"]));
            });
        }

        protected const string TextTemplateMarkdig = @"
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product.name }}</h2>
           Only {{ product.price }}
           {{ product.description | string.truncate 15 }}
    </li>
  {{ end }}
</ul>
";

        protected const string TextTemplateDotLiquid = @"
<ul id='products'>
  {% for product in products %}
    <li>
      <h2>{{ product.name }}</h2>
           Only {{ product.price }}
           {{ product.description | truncate: 15 }}
    </li>
  {% endfor %}
</ul>
";

        public const string TextTemplateMustache = @"
<ul id='products'>
  {{#products}}
    <li>
      <h2>{{ name }}</h2>
           Only {{ price }}
           {{#truncate}}{{description}}{{/truncate}}
    </li>
  {{/products}}
</ul>
";

        [Benchmark(Description = "Scriban - Parser")]
        public Scriban.Template TestScriban()
        {
            return Template.Parse(TextTemplateMarkdig);
        }

        [Benchmark(Description = "DotLiquid - Parser")]
        public DotLiquid.Template TestDotLiquid()
        {
            return DotLiquid.Template.Parse(TextTemplateDotLiquid);
        }

        [Benchmark(Description = "Stubble - Parser")]
        public Stubble.Core.Tokens.MustacheTemplate TestStubble()
        {
            return new Stubble.Core.Settings.RendererSettingsBuilder().BuildSettings().Parser.Parse(TextTemplateMustache);
        }

        [Benchmark(Description = "Nustache - Parser")]
        public Nustache.Core.Template TestNustache()
        {
            var template = new Nustache.Core.Template();
            template.Load(new StringReader(TextTemplateMustache));
            return template;
        }

        [Benchmark(Description = "Handlebars.NET - Parser")]
        public Func<object, string> TestHandlebars()
        {
            return HandlebarsDotNet.Handlebars.Compile(TextTemplateMustache);
        }
    }

    /// <summary>
    /// A benchmark for template renderers
    /// </summary>
    [MemoryDiagnoser]
    public class BenchRenderers
    {
        private readonly Scriban.Template _scribanTemplate;
        private readonly DotLiquid.Template _dotLiquidTemplate;
        private readonly Stubble.Core.Settings.RendererSettings _stubbleSettings;
        private readonly Stubble.Core.Tokens.MustacheTemplate _stubbleTemplate;
        private readonly Nustache.Core.Template _nustacheTemplate;
        private readonly Func<object, string> _handlebarsTemplate;

        private const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

        private readonly List<Product> _products;
        private readonly List<DotLiquid.Hash> _dotLiquidProducts;

        public BenchRenderers()
        {
            var parsers = new BenchParsers();
            _scribanTemplate = parsers.TestScriban();
            _dotLiquidTemplate = parsers.TestDotLiquid();
            _stubbleTemplate = parsers.TestStubble();
            _stubbleSettings = new Stubble.Core.Settings.RendererSettingsBuilder().BuildSettings();
            _nustacheTemplate = parsers.TestNustache();
            _handlebarsTemplate = parsers.TestHandlebars();

            const int ProductCount = 500;
            _products = new List<Product>(ProductCount);
            _dotLiquidProducts = new List<DotLiquid.Hash>(ProductCount);
            for (int i = 0; i < ProductCount; i++)
            {
                var product = new Product("Name" + i, i, Lorem);
                _products.Add(product);
                _dotLiquidProducts.Add(DotLiquid.Hash.FromAnonymousObject(product));
            }
        }

        [Benchmark(Description = "Scriban")]
        public string TestScriban()
        {
            return _scribanTemplate.Render(new { products = _products });
        }

        [Benchmark(Description = "DotLiquid")]
        public string TestDotLiquid()
        {
            return _dotLiquidTemplate.Render(DotLiquid.Hash.FromAnonymousObject(new { products = _dotLiquidProducts }));
        }

        [Benchmark(Description = "Stubble")]
        public string TestStubble()
        {
            var renderer = new Stubble.Core.StubbleVisitorRenderer();
            var props = new Dictionary<string, object> { ["products"] = _dotLiquidProducts };
            int i = 0;
            props["truncate"] = new Func<string, object>((str) => Scriban.Functions.StringFunctions.Truncate(15, renderer.Render(str, _dotLiquidProducts[i++])));
            return renderer.Render(BenchParsers.TextTemplateMustache, props);
        }

        [Benchmark(Description = "Nustache")]
        public string TestNustache()
        {
            int i = 0;
            return Nustache.Core.Render.StringToString(BenchParsers.TextTemplateMustache, new
            {
                products = _dotLiquidProducts,
                truncate = new Func<string, object>((str) => Scriban.Functions.StringFunctions.Truncate(15, Nustache.Core.Render.StringToString(str, _dotLiquidProducts[i++])))
            });
        }

        [Benchmark(Description = "Handlebars")]
        public string TestHandlebars()
        {
            int i = 0;
            return _handlebarsTemplate(new
            {
                products = _dotLiquidProducts
            });
        }

        public class Product
        {
            public Product(string name, float price, string description)
            {
                Name = name;
                Price = price;
                Description = description;
            }

            public string Name { get; set; }

            public float Price { get; set; }

            public string Description { get; set; }
        }
    }
}
