# Benchmarks

Latest benchmark update: 4 November 2017

> NOTE: This is a micro benchmark, so results may vary vastly on use cases. The goal here is to demonstrate on a very simple example how the different engines behave
> Also, while Scriban is compared here to `liquid` and `mustache` like templating engines, you should  keep in mind that language-wise, Scriban is allowing a lot more language constructions/expressions.

The benchmark was performed on two aspects of the libraries:

- The [**Parser Benchmark**](#parser-benchmarks): How long does it take to parse a template to a runtime representation? How much memory is used?
- The [**Rendering Benchmark**](#rendering-benchmarks): How long does it take to render a template with some input datas? How much memory is used?

Libraries used in this comparison:

- Scriban (0.11.0), Syntax: Scriban
- [Fluid](https://github.com/sebastienros/fluid/) (Fluid.Core.1.0.0-beta-9334), Syntax: Liquid based
- [DotLiquid](https://github.com/dotliquid/dotliquid) (2.0.200), Syntax: Liquid based
- [Stubble](https://github.com/StubbleOrg/Stubble) (1.0.42-alpha17), Syntax: Mustache+ based
- [Nustache](https://github.com/jdiamond/Nustache) (1.16.0.4), Syntax: Mustache based
- [Handlebars.NET](https://github.com/rexm/Handlebars.Net) (1.9.0), Syntax: Handlebars based
- [Cottle](https://github.com/r3c/cottle) (1.4.0.4), Syntax: Cottle

We are also adding [Razor](https://github.com/aspnet/Razor) (2.0.0), Syntax: Razor/C#, not in the charts and in the raw results. This is not a relevant comparison for the fact that it a not a "end-user" text templating engine (not safe) but it gives some insights about the best raw performance you can achieve with it for the rendering part, as it is generating very raw pre-compiler C# code that is basically issuing a bunch of `WriteLiteral(text_as_is)`, so you can't really do better here in terms of performance.

For benchmarking, we are using the fantastic [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)

See the [Scriban.Benchmark/Program.cs](../src/Scriban.Benchmarks/Program.cs) for details of the benchmark implementation.

[:top:](#benchmarks)
## Overall results

For the parser part:

- **Scriban parser is 3x to 6x times** faster compared to liquid based templating parsers
- **Scriban parser takes 3x to 40x times less memory** compared to other templating parsers

If you look at Razor (which is again, not really fair), `scriban` is roughly 1000x times faster than Razor for parsing a template. Which is perfectly normal, as Razor is involving the full Roslyn/C# compiler here. It is taking a lot more memory...etc. But it is generating an ultra efficient renderer.

For the rendering part:

- **Scriban is 1.2x to x14 times faster** than other templating engines
- **Scriban takes 3x to x65 times less memory** compared to other templating engines

In comparison to Razor, `scriban` is only 4-5 times slower than Razor, which is fairly honorable, considering how much raw is a compiled Razor template.

In the following sections, you will find benchmark details.

[:top:](#benchmarks)
## Parser Benchmarks

The methodology is to compile the following Scriban script:

```html
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product.name }}</h2>
           Only {{ product.price }}
           {{ product.description | string.truncate 15 }}
    </li>
  {{ end }}
</ul>
```

Or the equivalent Liquid script

```html
<ul id='products'>
  {% for product in products %}
    <li>
      <h2>{{ product.name }}</h2>
           Only {{ product.price }}
           {{ product.description | truncate: 15 }}
    </li>
  {% endfor %}
</ul>
```

Or the pseudo-equivalent Mustache script:

```html
<ul id='products'>
  {{#products}}
    <li>
      <h2>{{ name }}</h2>
           Only {{ price }}
           {{#truncate}}{{description}}{{/truncate}}
    </li>
  {{/products}}
</ul>
```

Or the pseudo-equivalent Cottle script:

```html
<ul id='products'>
  { for product in products:
    <li>
      <h2>{ product.Name }</h2>
           Only { product.Price }
           { string.truncate(product.Description, 15) }
    </li>
  }
</ul>
```

The raw results of the benchmarks are:

```
// * Summary *

BenchmarkDotNet=v0.10.9, OS=Windows 10.0.16299
Processor=Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), ProcessorCount=8
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2556.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2556.0

                    Method |         Mean |       Error |        StdDev |    Gen 0 |    Gen 1 |   Gen 2 |  Allocated |
-------------------------- |-------------:|------------:|--------------:|---------:|---------:|--------:|-----------:|
        'Scriban - Parser' |     12.74 us |   0.0427 us |     0.0399 us |   0.6561 |        - |       - |    2.72 KB |
      'DotLiquid - Parser' |     71.24 us |   0.2168 us |     0.2028 us |   2.1973 |        - |       - |    9.47 KB |
        'Stubble - Parser' |     12.09 us |   0.0884 us |     0.0827 us |   1.6327 |        - |       - |    6.74 KB |
       'Nustache - Parser' |     53.35 us |   0.0965 us |     0.0806 us |   4.0894 |        - |       - |   16.84 KB |
 'Handlebars.NET - Parser' |  1,009.46 us |  17.2727 us |    16.1569 us |  25.3906 |   1.9531 |       - |  106.81 KB |
         'Cottle - Parser' |     13.51 us |   0.1446 us |     0.1352 us |   1.7090 |        - |       - |    7.02 KB |
          'Fluid - Parser' |     27.41 us |   0.2426 us |     0.2151 us |   3.8147 |        - |       - |   15.63 KB |
          'Razor - Parser' | 14,517.78 us | 455.0174 us | 1,341.6292 us | 471.2500 | 269.6875 | 76.2500 | 2524.49 KB |

// * Legends *
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Gen 0     : GC Generation 0 collects per 1k Operations
  Gen 1     : GC Generation 1 collects per 1k Operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
```

About the results, we couldn't include Handlebars.NET in the following chart, as it is compiling to IL so it takes a lot more time to compile a template.

![BenchMark Parser Time and Memory](../img/benchmark-parsing.png)

[:top:](#benchmarks)
## Rendering Benchmarks

The methodology is to use the previously compiled script and use it with a list of 500 Products to output a final string

```
     Method |        Mean |      Error |     StdDev |     Gen 0 |   Gen 1 |   Gen 2 |   Allocated |
----------- |------------:|-----------:|-----------:|----------:|--------:|--------:|------------:|
    Scriban |  1,490.0 us |  17.650 us |  16.509 us |   62.6302 | 25.3906 | 25.3906 |   254.55 KB |
  DotLiquid |  7,707.7 us | 140.868 us | 131.768 us |  703.1250 | 54.6875 | 15.6250 |  2923.88 KB |
    Stubble |  3,273.2 us |  60.054 us |  56.174 us |  613.2813 | 58.5938 | 19.5313 |  2538.35 KB |
   Nustache | 21,855.4 us | 427.661 us | 457.592 us | 3750.0000 |       - |       - | 15460.25 KB |
 Handlebars |  3,391.3 us |  66.125 us |  78.718 us |  187.5000 | 41.9271 | 19.5313 |   782.18 KB |
     Cottle |  2,162.3 us |  22.151 us |  20.720 us |  175.0488 | 98.1445 | 22.9492 |   890.75 KB |
      Fluid |  1,794.1 us |  30.251 us |  28.297 us |  314.5559 | 66.8174 | 25.3906 |  1287.47 KB |
      Razor |    326.1 us |   3.023 us |   2.680 us |   64.4531 | 34.1146 | 22.4609 |   264.66 KB |
 ```

Note that for Stubble, It was not possible to match the behavior of the other engines, so it is including the parsing time (which is anyway insignificant compare to the rendering time in this particular case)

![BenchMark Rendering Time and Memory](../img/benchmark-rendering.png)

[:top:](#benchmarks)
