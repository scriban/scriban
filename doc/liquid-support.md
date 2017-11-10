# Liquid Support

Scriban supports all the [core liquid syntax](https://shopify.github.io/liquid/) types, operators, tags and filters.

- [Known issues](#known-issues)
- [Supported types](#supported-types)
- [Supported operators](#supported-operators)
- [Supported tags](#supported-tags)
  - [Variable and properties accessors](#variable-and-properties-accessors)
  - [<code>comment</code> tag](#comment-tag)
  - [<code>raw</code> tag](#raw-tag)
  - [<code>assign</code> tag](#assign-tag)
  - [<code>if</code> tag](#if-tag)
  - [<code>unless</code> tag](#unless-tag)
  - [<code>case</code> and <code>when</code> tags](#case-and-when-tags)
  - [<code>for</code> tag](#for-tag)
  - [<code>tablerow</code> tag](#tablerow-tag)
  - [<code>capture</code> tag](#capture-tag)
  - [Pipe calls](#pipe-calls)
- [Supported filters](#supported-filters)
- [Converting <code>liquid</code> to <code>scriban</code> using <code>liquid2scriban</code>](#converting-liquid-to-scriban-using-liquid2scriban)

## Known issues

> NOTE: The liquid syntax has never been strictly formalized, and custom tags implementation can choose whatever syntax for their arguments.
>
> This is a known issue in liquid itself, for example:
>  - [issue 507: Custom tags: what’s the preferred method of providing arguments containing quotes](https://github.com/Shopify/liquid/issues/507)
>  - [issue 671: Using liquid class libraries inside Liquid::Tag](https://github.com/Shopify/liquid/issues/671)
>  - [issue 560: Unified syntax for tag arguments [RFC]](https://github.com/Shopify/liquid/issues/560)
>
> For example in liquid, you usually pass arguments to tags and filters like this (supported by scriban):
>
> ```liquid
> {{ "this is a string" | function "arg1" 15 16 }}
> ```
>
> ```liquid
> {% custom_tag "arg1" 15 16 %}
> ```
>
> But some liquid tag/filter implementations may in fact choose to accept different ways of passing arguments:
>
> ```liquid
> {% avatar user=author size=24 %}
> ```
>
> There is in fact multiple versions of the liquid language, supporting different syntaxes for tags, which are completely arbitrary and not unified.
>
> As a consequence, **the liquid parser implemented in Scriban cannot parse any custom liquid tags/filters that are using custom arguments parsing**
> but only regular arguments (strings, numbers, variables, variable properties) separated by spaces.

[:top:](#liquid-support)
## Supported types

Liquid types are translated to the same types in scriban:

- [string](language.md#31-strings)
- [number](language.md#32-numbers)
- [boolean](language.md#33-boolean)

The `nil` value (which can't be expressed in liquid) is equivalent to the expression [`null`](language.md#34-null) in scriban.

- [array](language.md#6-arrays) are also supported, except that scriban allows to create arrays directly from the language unlike liquid

In addition to this, scriban supports the creation of an [`object`](language.md#5-objects)

[:top:](#liquid-support)
## Supported operators

Liquid supports only conditional expressions and they directly translate to [conditionnal expressions](language.md#85-conditional-expressions) in scriban.

In addition to this, scriban supports:

- [binary operators](language.md#84-arithmetic-expressions)
- [unary operators](language.md#86-unary-expressions)
- [range `1..x` expressions](language.md#87-range-expressions)
- [The null coalescing operator `??`](language.md#88-the-null-coalescing-operator-)

[:top:](#liquid-support)
## Supported tags

In the following sections, you will find a list of the supported liquid tags and how scriban translates a liquid template into a scriban template. 

> NOTE: All the following examples are using the feature [**Ast to text**](runtime.md#ast-to-text) that
allowed to translate liquid code into scriban code automatically

[:top:](#liquid-support)
### Variable and properties accessors

| Liquid                           | Scriban
|----------------------------------|-----------------------------------
| `{% assign variable = 1 %}`      | `{{ variable = 1 }}`
| `{{ variable }}`                 | `{{ variable }}`
| `{{ my-handle }}`                | `{{ this["my-handle" }}`
| `{{ page.title }}`               | `{{ page.title }}`
| `{% assign for = 5 %}`           | `{{ (for) = 5 }}` (for keyword needs parenthesis in scriban)
| `{{ for }}`                      | `{{ (for) }}`
| `{{ products[0].title }}`        | `{{ products[0].title }}`
| `{{ product.empty? }}`           | `{{ product.empty? }}`

[:top:](#liquid-support)
### `comment` tag

Liquid `comment`/`endcomment` tags translate to a code block `{{` ... `}}` embracing a [multiline comments `##`](language.md#2-comments)

> **liquid**
```liquid
This is plain {% comment %}This is comment {% with ## some tag %} and comment{% endcomment %}
```
> **scriban**
```scriban
This is plain {{## This is comment {% with \#\# some tag %\} and comment ##}}
```

[:top:](#liquid-support)
### `raw` tag

Liquid raw tag block translate to an [escape block](language.md#13-escape-block)

> **liquid**
```liquid
This is plain {% raw %}This is raw {% with some tag %} and raw{% endraw %}
```
> **scriban**
```scriban
This is plain {%{This is raw {% with some tag %} and raw}%}
```

[:top:](#liquid-support)
### `assign` tag

Liquid `assign` tag translates to a simple [assignment expression](language.md#82-assign-expression)

> **liquid**
```liquid
{% assign variable = 1 %}
{{ variable }}
```
> **scriban**
```scriban
{{ variable = 1 }}
{{ variable }}
```

[:top:](#liquid-support)
### `if` tag

Liquid `if <expression>`/`endif` tags translate to a [`if <expression>`/`end`](language.md#92-if-expression-else-else-if-expression)

> **liquid**
```liquid
{% assign variable = 1 %}
{% if variable == 1 %}
  This is a variable with 1
{% endif %}
```
> **scriban**
```scriban
{{ variable = 1 }}
{{ if variable == 1 }}
  This is a variable with 1
{{ end }}
```

[:top:](#liquid-support)
### `unless` tag

Liquid `unless <expression>`/`endunless` tags translate to a [`if <expression>`/`end`](language.md#92-if-expression-else-else-if-expression) with a reversed nested `!(expression)`

> **liquid**
```liquid
{% assign variable = 1 %}
{% unless variable == 1 %}
  This is not a variable with 1
{% endunless %}
```

> **scriban**
```scriban
{{ variable = 1 }}
{{ if!( variable == 1 )}}
  This is not a variable with 1
{{ end }}
```

[:top:](#liquid-support)
### `case` and `when` tags

Liquid `case <variable>`/`when <expression>`/`endcase` tags translate to a [`case <expression>`/`when <expression>`/`end`](language.md#93-case-and-when)

> **liquid**
```liquid
{%- assign variable = 5 -%}
{%- case variable -%}
    {%- when 6 -%}
        Yo 6
    {%- when 7 -%}
        Yo 7
    {%- when 5 -%}
        Yo 5
{% endcase -%}
```

> **scriban**
```scriban
{{ variable = 5 -}}
{{ case variable -}}
    {{ when 6 -}}
        Yo 6
    {{- when 7 -}}
        Yo 7
    {{- when 5 -}}
        Yo 5
{{ end }}
```

[:top:](#liquid-support)
### `for` tag

Liquid `for <variable> in <expression>`/`endfor` tags translate to the same [`for`/`end`](language.md#for-variable-in-expression--end)

> **liquid**
```liquid
{%- for variable in (1..5) -%}
    This is variable {{variable}}
{% endfor -%}
```

> **scriban**
```scriban
{{ for variable in (1..5) -}}
    This is variable {{variable}}
{{ end }}
```

> NOTE: Scriban supports all tags arguments: `limit`, `offset`, `reversed`

[:top:](#liquid-support)
### `tablerow` tag

Liquid `tablerow <variable> in <expression>`/`endtablerow` tags translate to the same [`tablerow`/`end`](language.md#tablerow-variable-in-expression--end)

> **liquid**
```liquid
{%- tablerow variable in (1..5) -%}
    This is variable {{variable}}
{% endtablerow -%}
```

> **scriban**
```scriban
{{ tablerow variable in (1..5) -}}
    This is variable {{variable}}
{{ end }}
```

> NOTE: Scriban supports all tags arguments for `tablerow`: `cols`, `limit`, `offset`, `reversed`

[:top:](#liquid-support)
### `capture` tag

Liquid `capture <variable>`/`endcapture` tags translate to a [`capture <expression>`/`end`](language.md#94-capture-variable--end)

> **liquid**
```liquid
{%- capture variable -%}
    This is a capture
{%- endcapture -%}
{{ variable }}
```

> **scriban**
```scriban
{{ capture variable -}}
    This is a capture
{{- end -}}
{{ variable }}
```

[:top:](#liquid-support)
### Pipe calls

Liquid pipe call translates to the same [`pipe call`](language.md#89-function-call-expression)

> **liquid**
```liquid
{% assign test = "abcdef" %}
{{ test | truncate: 5 }}
```

> **scriban**
```scriban
{{ test = "abcdef" }}
{{ test |  string.truncate 5 }}
```
As you can notice, Scriban will translate a call to a liquid tag to the corresponding scriban tag. But scriban also provides supports for direct tags calls using the `LiquidTemplateContext`. See [liquid support in runtime](runtime.md#liquid-support)

[:top:](#liquid-support)
## Supported filters

By default, all liquid filters are translated to scriban [builtin functions](builtins.md) (through objects like `string` or `array`)

The translation is performed by the [TryLiquidToScriban](https://github.com/lunet-io/scriban/blob/4ecb68deab3065c3163d46e3b51956712ec75e49/src/Scriban/Functions/LiquidBuiltinsFunctions.cs#L30) function at parsing time.

This translation can be disabled by setting the `ParserOptions.LiquidFunctionsToScriban` to `false`

[:top:](#liquid-support)
## Converting `liquid` to `scriban` using `liquid2scriban`

If you compile this repository, you will find a tool `liquid2scriban` that allows to convert a liquid script to scriban.

The `liquid2scriban` has one option that allows to parse Jekyll liquid templates that are passing to the include directive raw strings without quotes (e.g `{% include /this/is/partial.html %}`)
In that case, you can pass the option `--relaxed-include` to liquid2scriban, to allow the convertor to recognize this parameter as an implicit string instead.
