---
title: "Object functions"
---

# `object` functions

Object functions available through the builtin object 'object'.

- [`object.default`](object#object.default)
- [`object.eval`](object#object.eval)
- [`object.eval_template`](object#object.eval_template)
- [`object.format`](object#object.format)
- [`object.has_key`](object#object.has_key)
- [`object.has_value`](object#object.has_value)
- [`object.keys`](object#object.keys)
- [`object.size`](object#object.size)
- [`object.typeof`](object#object.typeof)
- [`object.kind`](object#object.kind)
- [`object.values`](object#object.values)
- [`object.from_json`](object#object.from_json)
- [`object.to_json`](object#object.to_json)

## `object.default`

```
object.default <value> <default>
```

#### Description

The `default` value is returned if the input `value` is null or an empty string "". A string containing whitespace characters will not resolve to the default value.

#### Arguments

- `value`: The input value to check if it is null or an empty string.
- `default`: The default value to return if the input `value` is null or an empty string.

#### Returns

The `default` value is returned if the input `value` is null or an empty string "", otherwise it returns `value`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20undefined_var%20%7C%20object.default%20%22Yo%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} undefined_var | object.default "Yo" {{ "}}" }}
```
> **output**
```html
Yo
```

## `object.eval`

```
object.eval <value>
```

#### Description

The evaluates a string as a scriban expression or evaluate the passed function or return the passed value.

#### Arguments

- `value`: The input value, either a scriban template in a string, or an alias function or directly a value.

#### Returns

The evaluation of the input value.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%221%20%2B%202%22%20%7C%20object.eval%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "1 + 2" | object.eval {{ "}}" }}
```
> **output**
```html
3
```

## `object.eval_template`

```
object.eval_template <value>
```

#### Description

The evaluates a string as a scriban template or evaluate the passed function or return the passed value.

#### Arguments

- `value`: The input value, either a scriban template in a string, or an alias function or directly a value.

#### Returns

The evaluation of the input value.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22This%20is%20a%20template%20text%20%7B%7B%201%20%2B%202%20%7D%7D%22%20%7C%20object.eval_template%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "This is a template text {{ "{{" }} 1 + 2 {{ "}}" }}" | object.eval_template {{ "}}" }}
```
> **output**
```html
This is a template text 3
```

## `object.format`

```
object.format <value> <format> <culture>?
```

#### Description

Formats an object using specified format.

#### Arguments

- `value`: The input value
- `format`: The format string.
- `culture`: The culture as a string (e.g `en-US`). By default the culture from  is used

#### Returns



#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20255%20%7C%20object.format%20%22X4%22%20%7D%7D%0A%7B%7B%201523%20%7C%20object.format%20%22N2%22%20%22en-US%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 255 | object.format "X4" {{ "}}" }}
{{ "{{" }} 1523 | object.format "N2" "en-US" {{ "}}" }}
```
> **output**
```html
00FF
1,523.00
```

## `object.has_key`

```
object.has_key <value> <key>
```

#### Description

Checks if the specified object as the member `key`

#### Arguments

- `value`: The input object.
- `key`: The member name to check its existence.

#### Returns

**true** if the input object contains the member `key`; otherwise **false**

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20product%20%7C%20object.has_key%20%22title%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} product | object.has_key "title" {{ "}}" }}
```
> **output**
```html
true
```

## `object.has_value`

```
object.has_value <value> <key>
```

#### Description

Checks if the specified object as a value for the member `key`

#### Arguments

- `value`: The input object.
- `key`: The member name to check the existence of its value.

#### Returns

**true** if the input object contains the member `key` and has a value; otherwise **false**

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20product%20%7C%20object.has_value%20%22title%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} product | object.has_value "title" {{ "}}" }}
```
> **output**
```html
true
```

## `object.keys`

```
object.keys <value>
```

#### Description

Gets the members/keys of the specified value object.

#### Arguments

- `value`: The input object.

#### Returns

A list with the member names/key of the input object

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20product%20%7C%20object.keys%20%7C%20array.sort%20%7D%7D&model={})
```scriban-html
{{ "{{" }} product | object.keys | array.sort {{ "}}" }}
```
> **output**
```html
["title", "type"]
```

## `object.size`

```
object.size <value>
```

#### Description

Returns the size of the input object.
- If the input object is a string, it will return the length
- If the input is a list, it will return the number of elements
- If the input is an object, it will return the number of members

#### Arguments

- `value`: The input object.

#### Returns

The size of the input object.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%202%2C%203%5D%20%7C%20object.size%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 2, 3] | object.size {{ "}}" }}
```
> **output**
```html
3
```

## `object.typeof`

```
object.typeof <value>
```

#### Description

Returns string representing the type of the input object. The type can be `string`, `boolean`, `number`, `array`, `iterator` and `object`

#### Arguments

- `value`: The input object.

#### Returns



#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20null%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%20true%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%201%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%201.0%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%20%22text%22%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%201..5%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%20%5B1%2C2%2C3%2C4%2C5%5D%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%20%7B%7D%20%7C%20object.typeof%20%7D%7D%0A%7B%7B%20object%20%7C%20object.typeof%20%7D%7D&model={})
```scriban-html
{{ "{{" }} null | object.typeof {{ "}}" }}
{{ "{{" }} true | object.typeof {{ "}}" }}
{{ "{{" }} 1 | object.typeof {{ "}}" }}
{{ "{{" }} 1.0 | object.typeof {{ "}}" }}
{{ "{{" }} "text" | object.typeof {{ "}}" }}
{{ "{{" }} 1..5 | object.typeof {{ "}}" }}
{{ "{{" }} [1,2,3,4,5] | object.typeof {{ "}}" }}
{{ "{{" }} {} | object.typeof {{ "}}" }}
{{ "{{" }} object | object.typeof {{ "}}" }}
```
> **output**
```html

boolean
number
number
string
iterator
array
object
object
```

## `object.kind`

```
object.kind <value>
```

#### Description

Returns string representing the type of the input object. The type can be `string`, `bool`, `byte`, `sbyte`, `ushort`, `short`, `uint`, `int`,
`ulong`, `long`, `float`, `double`, `decimal`, `bigint`, `enum`, `range`, `array`, `function` and `object`

#### Arguments

- `value`: The input object.

#### Returns



#### Examples

This function is newer than object.typeof and returns more detailed results about the types (e.g instead of `number`, returns `int` or `double`)

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20null%20%7C%20object.kind%20%7D%7D%0A%7B%7B%20true%20%7C%20object.kind%20%7D%7D%0A%7B%7B%201%20%7C%20object.kind%20%7D%7D%0A%7B%7B%201.0%20%7C%20object.kind%20%7D%7D%0A%7B%7B%20%22text%22%20%7C%20object.kind%20%7D%7D%0A%7B%7B%201..5%20%7C%20object.kind%20%7D%7D%0A%7B%7B%20%5B1%2C2%2C3%2C4%2C5%5D%20%7C%20object.kind%20%7D%7D%0A%7B%7B%20%7B%7D%20%7C%20object.kind%20%7D%7D%0A%7B%7B%20object%20%7C%20object.kind%20%7D%7D&model={})
```scriban-html
{{ "{{" }} null | object.kind {{ "}}" }}
{{ "{{" }} true | object.kind {{ "}}" }}
{{ "{{" }} 1 | object.kind {{ "}}" }}
{{ "{{" }} 1.0 | object.kind {{ "}}" }}
{{ "{{" }} "text" | object.kind {{ "}}" }}
{{ "{{" }} 1..5 | object.kind {{ "}}" }}
{{ "{{" }} [1,2,3,4,5] | object.kind {{ "}}" }}
{{ "{{" }} {} | object.kind {{ "}}" }}
{{ "{{" }} object | object.kind {{ "}}" }}
```
> **output**
```html

bool
int
double
string
range
array
object
object
```

## `object.values`

```
object.values <value>
```

#### Description

Gets the member's values of the specified value object.

#### Arguments

- `value`: The input object.

#### Returns

A list with the member values of the input object

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20product%20%7C%20object.values%20%7C%20array.sort%20%7D%7D&model={})
```scriban-html
{{ "{{" }} product | object.values | array.sort {{ "}}" }}
```
> **output**
```html
["fruit", "Orange"]
```

## `object.from_json`

```
object.from_json <json>
```

#### Description

Converts the json to a scriban value. Object, Array, string, etc. Only available in net7.0+

#### Arguments

- `json`: The json to deserialize.

#### Returns

Returns the scriban value

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%0A%20%20%20obj%20%3D%20%60%7B%20%22foo%22%3A%20123%20%7D%60%20%7C%20object.from_json%0A%20%20%20obj.foo%0A%7D%7D&model={})
```scriban-html
{{ "{{" }}
   obj = `{ "foo": 123 }` | object.from_json
   obj.foo
{{ "}}" }}
```
> **output**
```html
123
```

## `object.to_json`

```
object.to_json <value>
```

#### Description

Converts the scriban value to JSON. Only available in net7.0+

#### Arguments

- `value`: The input object.

#### Returns

A JSON representation of the value

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%7B%20foo%3A%20%22bar%22%2C%20baz%3A%20%5B1%2C%202%2C%203%5D%20%7D%20%7C%20object.to_json%20%7D%7D%0A%7B%7B%20true%20%7C%20object.to_json%20%7D%7D%0A%7B%7B%20null%20%7C%20object.to_json%20%7D%7D&model={})
```scriban-html
{{ "{{" }} { foo: "bar", baz: [1, 2, 3] } | object.to_json {{ "}}" }}
{{ "{{" }} true | object.to_json {{ "}}" }}
{{ "{{" }} null | object.to_json {{ "}}" }}
```
> **output**
```html
{"foo":"bar","baz":[1,2,3]}
true
null
```

> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
