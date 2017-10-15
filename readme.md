# scriban [![Build status](https://ci.appveyor.com/api/projects/status/ig5kv8r63bqjsd9a?svg=true)](https://ci.appveyor.com/project/xoofx/scriban)   [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

Scriban is a fast, powerful and lightweight text templating language and engine for .NET

```C#
var template = Template.Parse("Hello {{name}}!")
template.Render(new { name = "foo" }); // => "hi foo!" 
```

## Features

Scriban is similar to [liquid](http://liquidmarkup.org/) or [handlebars](http://handlebarsjs.com/) but provides additional support for:

- Real Lexer/Parser providing a **full Abstract Syntax Tree, fast, versatile and robust**, more efficient than a regex based parser.
- [Precise control of whitespace text output](doc/language.md#14-whitespace-control)
- [Full featured expressions](doc/language.md#8-expressions) (`x = 1 + 2`)
- [function call and pipes](doc/language.md#88-function-call-expression) (`myvar | string.capitalize`)
- [Complex objects](doc/language.md#5-objects) (javascript/json like objects `x = {mymember: 1}`) and [arrays](doc/language.md#6-arrays) (e.g `x = [1,2,3,4]`)
- [Custom functions](doc/language.md#7-functions) via `func` statement and allow **function pointers/delegates** via the `alias @ directive`
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

* See the [Language](doc/language.md) for a description of the language syntax and the built-in functions
* See the [Runtime](doc/runtime.md) for the a description of the runtime API. 

## Binaries

Compatible with the following .NET framework profiles:

- `.NET3.5`
- `.NET4.0+` via the PCL profile `portable40-net40+sl5+win8+wp8+wpa81`
- `NetStandard1.1` running on `CoreCLR`

## License

This software is released under the [BSD-Clause 2 license](http://opensource.org/licenses/BSD-2-Clause). 

## Related projects

* [dotliquid](https://github.com/dotliquid/dotliquid): .NET port of the liquid templating engine by @tgjones
* [Handlebars.Net](https://github.com/rexm/Handlebars.Net): .NET port of handlebars.js by @rexm

## Author

Alexandre Mutel aka [xoofx](http://xoofx.com).
