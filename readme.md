# scriban [![Build status](https://ci.appveyor.com/api/projects/status/ig5kv8r63bqjsd9a?svg=true)](https://ci.appveyor.com/project/xoofx/scriban)   [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

<img align="right" width="160px" height="160px" src="img/scriban.png">

Scriban is a fast, powerful and lightweight text templating language and engine for .NET

```C#
var template = Template.Parse("Hello {{name}}!")
template.Render(new { name = "World" }); // => "Hello World!" 
```

The language is very versatile, easy to read and use, similar to [liquid](http://liquidmarkup.org/) templates:

```C#
var template = Template.Parse(@"
<ul id='products'>
  {{ for product in products }}
    <li>
      <h2>{{ product.name }}</h2>
           Price: {{ product.price }}
           {{ product.description | string.truncate 15 }}
    </li>
  {{ end }}
</ul>
");
var result = template.Render(new { products = this.ProductList });
```

## Features

- Very **efficient**, **fast** parser and a **lightweight** runtime. CPU and Garbage Collector friendly. Check the benchmarks below.
- Powered by a Lexer/Parser providing a **full Abstract Syntax Tree, fast, versatile and robust**, more efficient than regex based parsers.
- **Extensible runtime** providing many extensibility points
- [Precise control of whitespace text output](doc/language.md#14-whitespace-control)
- [Full featured language](doc/language.md) including `if`/`else`/`for`/`while`, [expressions](doc/language.md#8-expressions) (`x = 1 + 2`), conditions... etc.
- [Function calls and pipes](doc/language.md#88-function-call-expression) (`myvar | string.capitalize`)
  - [Custom functions](doc/language.md#7-functions) directly into the language via `func` statement and allow **function pointers/delegates** via the `alias @ directive`
  - Bind [.NET custom functions](doc/runtime.md#imports-functions-from-a-net-class) from the runtime API with [many options](doc/runtime.md#the-scriptobject) for interfacing with .NET objects.
- [Complex objects](doc/language.md#5-objects) (javascript/json like objects `x = {mymember: 1}`) and [arrays](doc/language.md#6-arrays) (e.g `x = [1,2,3,4]`)
- Allow to pass [a block of statements](doc/language.md#98-wrap-function-arg1argn--end) to a function, typically used by the `wrap` statement
- Several builtins objects/functions:
  - [`arrays functions`](doc/language.md#101-array-functions)
  - [`maths functions`](doc/language.md#102-math-functions)
  - [`string functions`](doc/language.md#103-string-functions)
  - [`regex functions`](doc/language.md#104-regex)
  - [`date`](doc/language.md#106-datetime)/[`time`](doc/language.md#107-timespan)
- [Multi-line statements](doc/language.md#11-code-block) without having to embrace each line by `{{...}}`
- Safe language and runtime, allowing you to control what objects and functions are exposed

## Documentation

* The [Language](doc/language.md) for a description of the script language syntax and all the built-in functions.
* The [Runtime](doc/runtime.md) for the a description of the runtime API and how to use it from a .NET application.

## Binaries

Scriban is available as a NuGet package: [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

Compatible with the following .NET framework profiles:

- `.NET3.5`
- `.NET4.0+` via the PCL profile `portable40-net40+sl5+win8+wp8+wpa81`
- `UAP10.0+`
- `NetStandard1.1` running on `CoreCLR`

Also [Scriban.Signed](https://www.nuget.org/packages/Scriban.Signed/) NuGet package provides signed assemblies.

## Benchmarks

Latest benchmark update: 23 October 2017

This is a simple, non-exhaustive benchmark that should highlight how fast and lightweight Scriban parser and runtime is.

> NOTE: This is a micro benchmark, so results may vary vastly on use cases. The goal here is to demonstrate on a very simple example how the different engines behave
> Also, Scriban has not been optimized for this particular scenario tested here and little has been done so far to optimize the runtime, so even if Scriban is already blazing fast, there is no doubt that it can still be improved!

While Scriban is compared here to `liquid` and `mustache` like templating engines, you should also keep in mind that language-wise, Scriban is allowing more language constructions/expressions and thus more versatile.

The benchmark was performed on two aspects of the libraries:

- The **Parser Benchmark**: How long does it take to parse a template to a runtime representation? How much memory is used?
- The **Runtime Benchmark**: How long does it take to execute a template with some input datas? How much memory is used?

Libraries used in this comparison:

- Scriban (0.9.0)
- [DotLiquid](https://github.com/dotliquid/dotliquid) (2.0.200)
- [Nustache](https://github.com/jdiamond/Nustache) (1.16.0.4) - Mustache based
- [Stubble](https://github.com/StubbleOrg/Stubble) (1.0.42-alpha17) - Mustache+ based
- [Handlebars.NET](https://github.com/rexm/Handlebars.Net) (1.9.0)

For benchmarking, we are using the fantastic [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)

### Overall results

- **Scriban is 1.5x to x10 times faster** than other templating engines
- **Scriban is 3x to 30x more memory efficient** both at parsing and runtime time, than other templating engines (except with Handlebars.NET at rendering time which is on par)

In the following sections, you will find benchmark details.

### Parser Benchmarks

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
See the [Scriban.Benchmark/Program.cs](src/Scriban.Benchmarks/Program.cs) for details.

The raw results of the benchmarks are:

```
// * Summary *

BenchmarkDotNet=v0.10.9, OS=Windows 10.0.16299
Processor=Intel Core i7-7700K CPU 4.20GHz (Kaby Lake), ProcessorCount=8
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2556.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2556.0


                    Method |        Mean |     Error |    StdDev |   Gen 0 |  Gen 1 | Allocated |
-------------------------- |------------:|----------:|----------:|--------:|-------:|----------:|
        'Scriban - Parser' |    10.59 us | 0.0624 us | 0.0521 us |  0.6104 |      - |   2.54 KB |
      'DotLiquid - Parser' |    71.45 us | 1.1307 us | 1.0024 us |  2.1973 |      - |   9.47 KB |
        'Stubble - Parser' |    12.29 us | 0.0982 us | 0.0918 us |  1.6327 |      - |   6.74 KB |
       'Nustache - Parser' |    53.98 us | 0.1557 us | 0.1300 us |  4.0894 |      - |  16.84 KB |
 'Handlebars.NET - Parser' | 1,005.38 us | 6.5709 us | 5.8250 us | 25.3906 | 1.9531 | 106.81 KB |

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

![BenchMark Parser Time and Memory](img/benchmark-parsing.png)

### Runtime Benchmarks

The methodology is to use the previously compiled script and use it with a list of 500 Products to output a final string

```
     Method |      Mean |     Error |    StdDev |     Gen 0 |   Gen 1 |   Gen 2 |   Allocated |
----------- |----------:|----------:|----------:|----------:|--------:|--------:|------------:|
    Scriban |  1.976 ms | 0.0069 ms | 0.0062 ms |  148.4375 | 27.3438 | 27.3438 |   647.41 KB |
  DotLiquid |  7.810 ms | 0.0460 ms | 0.0408 ms |  687.5000 | 31.2500 | 23.4375 |  2923.73 KB |
    Stubble |  3.507 ms | 0.0274 ms | 0.0256 ms |  593.7500 | 39.0625 | 27.3438 |  2522.75 KB |
   Nustache | 21.598 ms | 0.2369 ms | 0.1978 ms | 3718.7500 |       - |       - | 15444.73 KB |
 Handlebars |  3.404 ms | 0.0236 ms | 0.0209 ms |  171.8750 | 31.2500 | 27.3438 |   766.34 KB |
 ```

Note that for Stubble, It was not possible to match the behavior of the other engines, so it is including the parsing time (which is anyway insignificant compare to the rendering time in this particular case)

![BenchMark Rendering Time and Memory](img/benchmark-rendering.png)


## License

This software is released under the [BSD-Clause 2 license](http://opensource.org/licenses/BSD-2-Clause). 

## Related projects

* [dotliquid](https://github.com/dotliquid/dotliquid): .NET port of the liquid templating engine by @tgjones
* [Nustache](https://github.com/jdiamond/Nustache): Logic-less templates for .NET
* [Handlebars.Net](https://github.com/rexm/Handlebars.Net): .NET port of handlebars.js by @rexm

## Credits

Adapted logo `Puzzle` by [Andrew Doane](https://thenounproject.com/andydoane/) from the Noun Project

## Author

Alexandre Mutel aka [xoofx](http://xoofx.com).
