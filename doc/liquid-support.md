# Liquid Support in Scriban

Scriban supports all the [core liquid syntax](https://shopify.github.io/liquid/) tags and filters, except the following known issue.

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
> but only regular arguments separated by spaces.

## Examples

In the following sections, you will find a few examples about how scriban translates a liquid template into a scriban template. 

> NOTE: All the following examples are using the feature [**Ast to text**](runtime.md#ast-to-text) that
allowed to translate liquid code into scriban code automatically

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

### `comment` tag

> **liquid**
```liquid
This is plain {% comment %}This is comment {% with ## some tag %} and comment{% endcomment %}
```
> **scriban**
```scriban
This is plain {{## This is comment {% with \#\# some tag %\} and comment ##}}
```

### `raw` tag

> **liquid**
```liquid
This is plain {% raw %}This is raw {% with some tag %} and raw{% endraw %}
```
> **scriban**
```scriban
This is plain {%{This is raw {% with some tag %} and raw}%}
```

### `if` tag

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
### `unless` tag

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

### `case` and `when` tags

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

### `for` tag

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

### `capture` tag

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

### Pipe calls

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

## Converting to scriban using `liquid2scriban`

If you compile this repository, you will find a tool `liquid2scriban` that allows to convert a liquid script to scriban.

The `liquid2scriban` has one option that allows to parse Jekyll liquid templates that are passing to the include directive raw strings without quotes (e.g `{% include /this/is/partial.html %}`)
In that case, you can pass the option `--relaxed-include` to liquid2scriban, to allow the convertor to recognize this parameter as an implicit string instead.
