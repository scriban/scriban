# Benchmarks

Latest benchmark update: 4 November 2017

This is a simple, non-exhaustive benchmark that should highlight how fast and lightweight Scriban parser and runtime is.

> NOTE: This is a micro benchmark, so results may vary vastly on use cases. The goal here is to demonstrate on a very simple example how the different engines behave
> Also, Scriban has not been optimized for this particular scenario tested here and little has been done so far to optimize the runtime, so even if Scriban is already blazing fast, there is no doubt that it can still be improved!

While Scriban is compared here to `liquid` and `mustache` like templating engines, you should also keep in mind that language-wise, Scriban is allowing a lot more language constructions/expressions.

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

For benchmarking, we are using the fantastic [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)

See the [Scriban.Benchmark/Program.cs](../src/Scriban.Benchmarks/Program.cs) for details of the benchmark implementation.

[:top:](#benchmarks)
## Overall results

For the parser part:

- **Scriban parser is 3x to 6x times** faster compared to liquid based templating parsers
- **Scriban parser takes 3x to 40x times less memory** compared to other templating parsers

For the rendering part:

- **Scriban is 1.2x to x14 times faster** than other templating engines
- **Scriban takes 3x to x65 times less memory** compared to other templating engines

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

                    Method |      Mean |     Error |    StdDev |   Gen 0 |  Gen 1 | Allocated |
-------------------------- |----------:|----------:|----------:|--------:|-------:|----------:|
        'Scriban - Parser' |  12.07 us | 0.0271 us | 0.0253 us |  0.6561 |      - |   2.71 KB |
          'Fluid - Parser' |  27.30 us | 0.1435 us | 0.1272 us |  3.8147 |      - |  15.63 KB |
      'DotLiquid - Parser' |  70.19 us | 0.0828 us | 0.0646 us |  2.1973 |      - |   9.47 KB |
        'Stubble - Parser' |  12.16 us | 0.0267 us | 0.0250 us |  1.6327 |      - |   6.74 KB |
       'Nustache - Parser' |  54.01 us | 0.2199 us | 0.2057 us |  4.0894 |      - |  16.84 KB |
 'Handlebars.NET - Parser' | 997.00 us | 2.0396 us | 1.7031 us | 25.3906 | 1.9531 | 106.81 KB |
         'Cottle - Parser' |  13.40 us | 0.0666 us | 0.0623 us |  1.7090 |      - |   7.02 KB |

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
     Method |      Mean |     Error |    StdDev |     Gen 0 |   Gen 1 |   Gen 2 |   Allocated |
----------- |----------:|----------:|----------:|----------:|--------:|--------:|------------:|
    Scriban |  1.493 ms | 0.0020 ms | 0.0017 ms |   58.5938 | 29.2969 | 29.2969 |   238.63 KB |
      Fluid |  1.707 ms | 0.0042 ms | 0.0037 ms |  292.9688 | 29.2969 | 29.2969 |  1287.34 KB |
  DotLiquid |  7.614 ms | 0.0883 ms | 0.0826 ms |  687.5000 | 31.2500 | 23.4375 |  2923.73 KB |
    Stubble |  3.440 ms | 0.0118 ms | 0.0099 ms |  593.7500 | 39.0625 | 27.3438 |  2522.75 KB |
   Nustache | 21.250 ms | 0.0736 ms | 0.0689 ms | 3718.7500 |       - |       - | 15444.73 KB |
 Handlebars |  3.265 ms | 0.0052 ms | 0.0046 ms |  171.8750 | 31.2500 | 27.3438 |   766.34 KB |
     Cottle |  2.046 ms | 0.0041 ms | 0.0038 ms |  179.6875 | 85.9375 | 27.3438 |   875.04 KB |
 ```

Note that for Stubble, It was not possible to match the behavior of the other engines, so it is including the parsing time (which is anyway insignificant compare to the rendering time in this particular case)

![BenchMark Rendering Time and Memory](../img/benchmark-rendering.png)

[:top:](#benchmarks)
