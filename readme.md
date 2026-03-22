# scriban [![ci](https://github.com/scriban/scriban/actions/workflows/CI.yml/badge.svg)](https://github.com/scriban/scriban/actions/workflows/CI.yml) [![Coverage Status](https://coveralls.io/repos/github/scriban/scriban/badge.svg?branch=master)](https://coveralls.io/github/scriban/scriban?branch=master) [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

<img align="right" width="160px" height="160px" src="img/scriban.png">

Scriban is a fast, powerful, safe and lightweight scripting language and engine for .NET, which was primarily developed for text templating with a compatibility mode for parsing `liquid` templates.

Today, not only Scriban can be used in text templating scenarios, but also can be integrated as a **general scripting engine**: For example, Scriban is at the core of the scripting engine for [kalk](https://github.com/xoofx/kalk), a command line calculator application for developers.

```csharp
// Parse a scriban template
var template = Template.Parse("Hello {{name}}!");
var result = template.Render(new { Name = "World" }); // => "Hello World!" 
```

Parse a Liquid template using the Liquid language:

```csharp
// Parse a liquid template
var template = Template.ParseLiquid("Hello {{name}}!");
var result = template.Render(new { Name = "World" }); // => "Hello World!" 
```

The language is very versatile, easy to read and use, similar to [liquid](https://shopify.github.io/liquid/) templates:

```csharp
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

Scriban can also be used in pure scripting context without templating (`{{` and `}}`) and can help you to create your own small DSL.

> [!NOTE]
> By default, Properties and methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](https://scriban.github.io/docs/runtime/member-renamer/#member-renamer) delegate

## Highlights

- Fully visitable AST with `ScriptVisitor`, parent links on `ScriptNode`, and round-trippable formatting with [`Template.ToText`](https://scriban.github.io/docs/runtime/ast/#ast-to-text).
- Flexible language features including hexadecimal/binary numbers, large integers, parametric and inline functions, optional member access (`?.`), and conditional expressions.
- Multiple parsing modes through `ScriptLang` and `ScriptMode`, including Scriban, Liquid, and Scientific parsing.
- Fine-grained runtime control through `TemplateContext` options such as relaxed member, function, target, and indexer access.
- Runtime evaluation helpers such as `object.eval` and `object.eval_template`.
- Async rendering support with `Template.RenderAsync`.
- Native AOT and trimming-friendly APIs on .NET 8+ when using the AOT-safe surface documented in the runtime guides.
  
## Features

- An **extensible sandbox execution model**: You have the full control about which Scripting objects (and so properties and methods) are accessible from Scriban templates.
- Very **efficient**, **fast** parser and a **lightweight** runtime. CPU and Garbage Collector friendly.
- Powered by a Lexer/Parser providing a **full Abstract Syntax Tree, fast, versatile and robust**, more efficient than regex based parsers.
  - Precise source code location (path, column and line) for error reporting
  - **Write an AST to a script textual representation**, with [`Template.ToText`](https://scriban.github.io/docs/runtime/ast/#ast-to-text), allowing to manipulate scripts in memory and re-save them to the disk, useful for **roundtrip script update scenarios**
- **Compatible with `liquid`** by using the `Template.ParseLiquid` method
  - While the `liquid` language is less powerful than scriban, this mode allows to migrate from `liquid` to `scriban` language easily
  - With the [AST to text](https://scriban.github.io/docs/runtime/ast/#ast-to-text) mode, you can convert a `liquid` script to a scriban script using `Template.ToText` on a template parsed with `Template.ParseLiquid`
  - As the liquid language is not strictly defined and there are in fact various versions of liquid syntax, there are restrictions while using liquid templates with scriban, see the document [liquid support in scriban](https://scriban.github.io/docs/liquid-support/) for more details.
- **Extensible runtime** providing many extensibility points
- Support for `async`/`await` evaluation of scripts (e.g `Template.RenderAsync`)
- Precise control of whitespace text output
- Full featured language including `if`/`else`/`for`/`while`, expressions (`x = 1 + 2`), conditions... etc.
- Function calls and pipes (`myvar | string.capitalize`)
  - Custom functions directly into the language via `func` statement and allow **function pointers/delegates** via the `alias @ directive`
  - Bind .NET custom functions from the runtime API with many options for interfacing with .NET objects.
- Complex objects (javascript/json like objects `x = {mymember: 1}`) and arrays (e.g `x = [1,2,3,4]`)
- Allow to pass a block of statements to a function, typically used by the `wrap` statement
- Several built-in functions: `array`, `date`, `html`, `math`, `object`, `regex`, `string`, `timespan`
- Multi-line statements without having to embrace each line by `{{...}}`
- Safe parser and safe runtime, allowing you to control what objects and functions are exposed
- **[AOT and trimming compatible](https://scriban.github.io/docs/runtime/aot-support/)** on .NET 8+. `ScriptObject`-based APIs produce zero linker warnings for Native AOT publishing

## Syntax Coloring

You can install the [Scriban Extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=xoofx.scriban) to get syntax coloring for scriban scripts (without HTML) and scriban html files.

## Documentation

The full documentation is available at **https://scriban.github.io**.

## Installation

Scriban is available as a NuGet package: [![NuGet](https://img.shields.io/nuget/v/Scriban.svg)](https://www.nuget.org/packages/Scriban/)

```sh
dotnet add package Scriban
```

The package targets `netstandard2.0` and `net8.0`, so it works with .NET 6+, .NET Framework 4.7.2+, and other compatible runtimes.

Also the [Scriban.Signed](https://www.nuget.org/packages/Scriban.Signed/) NuGet package provides signed assemblies.

## Source Embedding

The package includes Scriban source files so that you can internalize Scriban into your project instead of consuming it only as a binary dependency. This is useful in environments where NuGet references are not convenient, such as Roslyn source generators.

> [!WARNING]
> Currently, Scriban source files are not marked as read-only in this mode. Do not modify them unless you intend to affect other projects on the same machine that use the embedded sources. Use this feature at your own risk.

In order to activate this feature you need to:

- Set the property `PackageScribanIncludeSource` to `true` in your project:
  ```xml
  <PropertyGroup>
    <PackageScribanIncludeSource>true</PackageScribanIncludeSource>
  </PropertyGroup>
  ```
- Add the `IncludeAssets="Build"` to the NuGet PackageReference for Scriban:
  ```xml
  <ItemGroup>
    <PackageReference Include="Scriban" Version="x.y.z" IncludeAssets="Build" />
  </ItemGroup>
  ```

If you are targeting `netstandard2.0` or `.NET Framework 4.7.2+`, you will also need the supporting packages Scriban compiles against. They can already come from another dependency in your project:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
  <PackageReference Include="System.Threading.Tasks.Extensions" Version="4.6.3" />
</ItemGroup>
```

> [!NOTE]
> In this mode, all Scriban types are marked as `internal`.
>
> `System.Text.Json`-based features are intentionally disabled in source-embedding mode. This includes helpers such as `object.from_json`, `object.to_json`, and direct `JsonElement` import support.

## License

This software is released under the [BSD-Clause 2 license](https://opensource.org/licenses/BSD-2-Clause). 

## Related projects

* [dotliquid](https://github.com/dotliquid/dotliquid): .NET port of the liquid templating engine
* [Fluid](https://github.com/sebastienros/fluid/) .NET liquid templating engine
* [Nustache](https://github.com/jdiamond/Nustache): Logic-less templates for .NET
* [Handlebars.Net](https://github.com/rexm/Handlebars.Net): .NET port of handlebars.js
* [Textrude](https://github.com/NeilMacMullen/Textrude): UI and CLI tools to turn CSV/JSON/YAML models into code using Scriban templates
* [NTypewriter](https://github.com/NeVeSpl/NTypewriter): VS extension to turn C# code into documentation/TypeScript/anything using Scriban templates
  
## Online Demo

* Main site and playground: https://scriban.github.io
* Legacy sample: https://scribanonline.azurewebsites.net/ ASP.NET Core sample

## Sponsors

Supports this project with a monthly donation and help me continue improving it. \[[Become a sponsor](https://github.com/sponsors/xoofx)\]

[<img src="https://github.com/lilith.png?size=200" width="64px;" style="border-radius: 50%" alt="lilith"/>](https://github.com/lilith) Lilith River, author of [Imageflow Server, an easy on-demand
image editing, optimization, and delivery server](https://github.com/imazen/imageflow-server)

## Credits

Adapted logo `Puzzle` by [Andrew Doane](https://thenounproject.com/andydoane/) from the Noun Project

## Author

Alexandre Mutel aka [xoofx](https://xoofx.github.io).
