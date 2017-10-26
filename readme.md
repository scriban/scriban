# scriban [![Build status](https://ci.appveyor.com/api/projects/status/ig5kv8r63bqjsd9a?svg=true)](https://ci.appveyor.com/project/xoofx/scriban)   [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

<img align="right" width="160px" height="160px" src="img/scriban.png">

Scriban is a fast, powerful and lightweight text templating language and engine for .NET

```C#
var template = Template.Parse("Hello {{name}}!")
var result = template.Render(new { name = "World" }); // => "Hello World!" 
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

- Very **efficient**, **fast** parser and a **lightweight** runtime. CPU and Garbage Collector friendly. Check the [benchmarks](doc/benchmarks.md) for more details.
- Powered by a Lexer/Parser providing a **full Abstract Syntax Tree, fast, versatile and robust**, more efficient than regex based parsers.
  - Precise source code location (path, column and line) for error reporting
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

## Syntax Coloring

You can install the [Scriban Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=xoofx.scriban) to get syntax coloring for scriban scripts (without HTML) and scriban html files.

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

**Scriban is 1.5x to x10 times faster** than existing templating engines, you will find more details in our [benchmarks document](doc/benchmarks.md).

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
