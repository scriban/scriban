# scriban [![Build Status](https://github.com/lunet-io/scriban/workflows/ci/badge.svg?branch=master)](https://github.com/lunet-io/scriban/actions) [![Coverage Status](https://coveralls.io/repos/github/lunet-io/scriban/badge.svg?branch=master)](https://coveralls.io/github/lunet-io/scriban?branch=master) [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

<img align="right" width="160px" height="160px" src="img/scriban.png">

Scriban is a fast, powerful, safe and lightweight text templating language and engine for .NET, with a compatibility mode for parsing `liquid` templates.

```C#
// Parse a scriban template
var template = Template.Parse("Hello {{name}}!");
var result = template.Render(new { Name = "World" }); // => "Hello World!" 
```

Parse a Liquid template using the Liquid language:

```C#
// Parse a liquid template
var template = Template.ParseLiquid("Hello {{name}}!");
var result = template.Render(new { Name = "World" }); // => "Hello World!" 
```

The language is very versatile, easy to read and use, similar to [liquid](https://shopify.github.io/liquid/) templates:

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
var result = template.Render(new { Products = this.ProductList });
```

> **NOTICE**
>
> By default, Properties and methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](doc/runtime.md#member-renamer) delegate

## Features

- Very **efficient**, **fast** parser and a **lightweight** runtime. CPU and Garbage Collector friendly. Check the [benchmarks](doc/benchmarks.md) for more details.
- Powered by a Lexer/Parser providing a **full Abstract Syntax Tree, fast, versatile and robust**, more efficient than regex based parsers.
  - Precise source code location (path, column and line) for error reporting
  - **Write an AST to a script textual representation**, with [`Template.ToText`](doc/runtime.md#ast-to-text), allowing to manipulate scripts in memory and re-save them to the disk, useful for **roundtrip script update scenarios**
- **Compatible with `liquid`** by using the `Template.ParseLiquid` method
  - While the `liquid` language is less powerful than scriban, this mode allows to migrate from `liquid` to `scriban` language easily
  - With the [AST to text](doc/runtime.md#ast-to-text) mode, you can convert a `liquid` script to a scriban script using `Template.ToText` on a template parsed with `Template.ParseLiquid`
  - As the liquid language is not strictly defined and there are in fact various versions of liquid syntax, there are restrictions while using liquid templates with scriban, see the document [liquid support in scriban](doc/liquid-support.md) for more details.
- **Extensible runtime** providing many extensibility points
- Support for `async`/`await` evaluation of scripts (e.g `Template.RenderAsync`)
- [Precise control of whitespace text output](doc/language.md#14-whitespace-control)
- [Full featured language](doc/language.md) including `if`/`else`/`for`/`while`, [expressions](doc/language.md#8-expressions) (`x = 1 + 2`), conditions... etc.
- [Function calls and pipes](doc/language.md#89-function-call-expression) (`myvar | string.capitalize`)
  - [Custom functions](doc/language.md#7-functions) directly into the language via `func` statement and allow **function pointers/delegates** via the `alias @ directive`
  - Bind [.NET custom functions](doc/runtime.md#imports-functions-from-a-net-class) from the runtime API with [many options](doc/runtime.md#the-scriptobject) for interfacing with .NET objects.
- [Complex objects](doc/language.md#5-objects) (javascript/json like objects `x = {mymember: 1}`) and [arrays](doc/language.md#6-arrays) (e.g `x = [1,2,3,4]`)
- Allow to pass [a block of statements](doc/language.md#98-wrap-function-arg1argn--end) to a function, typically used by the `wrap` statement
- Several [built-in functions](doc/builtins.md):
  - [`arrays functions`](doc/builtins.md#array-functions)
  - [`date`](doc/builtins.md#date-functions)
  - [`html`](doc/builtins.md#html-functions)
  - [`maths functions`](doc/builtins.md#math-functions)
  - [`object`](doc/builtins.md#object-functions)
  - [`regex functions`](doc/builtins.md#regex-functions)
  - [`string functions`](doc/builtins.md#string-functions)
  - [`timespan`](doc/builtins.md#timespan-functions)
- [Multi-line statements](doc/language.md#11-code-block) without having to embrace each line by `{{...}}`
- [Safe parser](doc/runtime.md#the-lexer-and-parser) and [safe runtime](doc/runtime.md#safe-runtime), allowing you to control what objects and functions are exposed

## Syntax Coloring

You can install the [Scriban Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=xoofx.scriban) to get syntax coloring for scriban scripts (without HTML) and scriban html files.

## Documentation

* See the [Language](doc/language.md) document for a description of the language syntax.
* See the [Built-in functions](doc/builtins.md) document for the list of the built-in functions.
* See the [Runtime](doc/runtime.md) document for a description of the .NET runtime API to compile and run templates.
* See the [Liquid support](doc/liquid-support.md) document for more details about the support of liquid templates.
* See my blog post "[Implementing a Text Templating Engine for .NET](http://xoofx.com/blog/2017/11/13/implementing-a-text-templating-language-and-engine-for-dotnet/)" for some behind the scene details.

## Binaries

Scriban is available as a NuGet package: [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

Compatible with the following .NET framework profiles:

- .NET Framework 3.5
- .NET Framework 4.0
- .NET Framework 4.5+ (supports asynchronous code and timeouts for regular expressions)
- .NET Standard1.1+ (some features are not available)
- .NET Standard1.3+ (which means .NET Core, Xamarin, UWP, Unity etc.)

Also the [Scriban.Signed](https://www.nuget.org/packages/Scriban.Signed/) NuGet package provides signed assemblies.

## Benchmarks

**Scriban is blazing fast**! For more details, you can check the [benchmarks document](doc/benchmarks.md).

## License

This software is released under the [BSD-Clause 2 license](https://opensource.org/licenses/BSD-2-Clause). 

## Related projects

* [dotliquid](https://github.com/dotliquid/dotliquid): .NET port of the liquid templating engine
* [Fluid](https://github.com/sebastienros/fluid/) .NET liquid templating engine
* [Nustache](https://github.com/jdiamond/Nustache): Logic-less templates for .NET
* [Handlebars.Net](https://github.com/rexm/Handlebars.Net): .NET port of handlebars.js

## Online Demo

* (https://scribanonline.azurewebsites.net/): ASP.NET Core Sample.

## Credits

Adapted logo `Puzzle` by [Andrew Doane](https://thenounproject.com/andydoane/) from the Noun Project

## Author

Alexandre Mutel aka [xoofx](http://xoofx.com).
