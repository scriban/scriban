# scriban

Scriban is a fast, powerful and lightweight text templating language and engine for .NET

```C#
var template = Template.Parse("Hello {{name}}!")
template.Render(new { name = "foo" }); // => "hi foo!" 
```

Scriban is similar to [liquid](http://liquidmarkup.org/) or [handlebars](http://handlebarsjs.com/) but provides additional support for:

- Full featured language including expressions, functions as objects (with support for pipes)
- Multi-line statements without having to embrace each line by `{{...}}`
- Declare and directly use custom functions via the `func` statement. 
- Javascript/json like objects `x = {mymember: 1}` and arrays `x = [1,2,3,4]`

## Documentation

* See the [Language](doc/language.md) for a description of the language syntax and the built-in functions
* See the [Runtime](doc/runtime.md) for the a description of the runtime API. 

## Binaries

Compatible with the following .NET framework profiles:

- `NET46`
- `UWP 10`
- `CoreCLR`

## License
This software is released under the [BSD-Clause 2 license](http://opensource.org/licenses/BSD-2-Clause). 

## Related projects

* [dotliquid](https://github.com/dotliquid/dotliquid): .NET port of the liquid templating engine by @tgjones
* [Handlebars.Net](https://github.com/rexm/Handlebars.Net): .NET port of handlebars.js by @rexm

## Author

Alexandre Mutel aka [xoofx](http://xoofx.com).
