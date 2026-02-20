---
title: "Html functions"
---

# `html` functions

Html functions available through the builtin object 'html'.

- [`html.strip`](#htmlstrip)
- [`html.escape`](#htmlescape)
- [`html.newline_to_br`](#htmlnewline_to_br)
- [`html.url_encode`](#htmlurl_encode)
- [`html.url_escape`](#htmlurl_escape)

## `html.strip`

```
html.strip <text>
```

#### Description

Removes any HTML tags from the input string

#### Arguments

- `text`: The input string

#### Returns

The input string removed with any HTML tags

#### Examples

Notice that the implementation of this function is using a simple regex, so it can fail escaping correctly or timeout in case of the malformed html.
If you are looking for a secure HTML stripped, you might want to plug your own HTML function by using [AngleSharp](https://github.com/AngleSharp/AngleSharp) to
strip these HTML tags.

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%3Cp%3EThis%20is%20a%20paragraph%3C%2Fp%3E%22%20%7C%20html.strip%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "<p>This is a paragraph</p>" | html.strip {{ "}}" }}
```
> **output**
```html
This is a paragraph
```

## `html.escape`

```
html.escape <text>
```

#### Description

Escapes a HTML input string (replacing `&` by `&amp;`)

#### Arguments

- `text`: The input string

#### Returns

The input string removed with any HTML tags

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%3Cp%3EThis%20is%20a%20paragraph%3C%2Fp%3E%22%20%7C%20html.escape%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "<p>This is a paragraph</p>" | html.escape {{ "}}" }}
```
> **output**
```html
&lt;p&gt;This is a paragraph&lt;/p&gt;
```

## `html.newline_to_br`

```
html.newline_to_br <text>
```

#### Description

Inserts an HTML line break (`<br />` in front of each newline (`\r\n`, `\r`, `\n`) in a string

#### Arguments

- `text`: The input string

#### Returns

The input string with HTML line breaks

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%5Cnworld%22%20%7C%20html.newline_to_br%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello\nworld" | html.newline_to_br {{ "}}" }}
```
> **output**
```html
Hello<br />
world
```

## `html.url_encode`

```
html.url_encode <text>
```

#### Description

Converts any URL-unsafe characters in a string into percent-encoded characters.

#### Arguments

- `text`: The input string

#### Returns

The input string url encoded

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22john%40liquid.com%22%20%7C%20html.url_encode%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "john@liquid.com" | html.url_encode {{ "}}" }}
```
> **output**
```html
john%40liquid.com
```

## `html.url_escape`

```
html.url_escape <text>
```

#### Description

Identifies all characters in a string that are not allowed in URLS, and replaces the characters with their escaped variants.

#### Arguments

- `text`: The input string

#### Returns

The input string url escaped

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%3Chello%3E%20%26%20%3Cscriban%3E%22%20%7C%20html.url_escape%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "<hello> & <scriban>" | html.url_escape {{ "}}" }}
```
> **output**
```html
%3Chello%3E%20&%20%3Cscriban%3E
```

> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
