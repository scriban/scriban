---
title: "Math functions"
---

# `math` functions

Math functions available through the object 'math' in scriban.

- [`math.abs`](math#math.abs)
- [`math.ceil`](math#math.ceil)
- [`math.divided_by`](math#math.divided_by)
- [`math.floor`](math#math.floor)
- [`math.format`](math#math.format)
- [`math.is_number`](math#math.is_number)
- [`math.minus`](math#math.minus)
- [`math.modulo`](math#math.modulo)
- [`math.plus`](math#math.plus)
- [`math.round`](math#math.round)
- [`math.times`](math#math.times)
- [`math.uuid`](math#math.uuid)
- [`math.random`](math#math.random)

## `math.abs`

```
math.abs <value>
```

#### Description

Returns the absolute value of a specified number.

#### Arguments

- `value`: The input value

#### Returns

The absolute value of the input value

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20-15.5%7C%20math.abs%20%7D%7D%0A%7B%7B%20-5%7C%20math.abs%20%7D%7D&model={})
```scriban-html
{{ "{{" }} -15.5| math.abs {{ "}}" }}
{{ "{{" }} -5| math.abs {{ "}}" }}
```
> **output**
```html
15.5
5
```

## `math.ceil`

```
math.ceil <value>
```

#### Description

Returns the smallest integer greater than or equal to the specified number.

#### Arguments

- `value`: The input value

#### Returns

The smallest integer greater than or equal to the specified number.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%204.6%20%7C%20math.ceil%20%7D%7D%0A%7B%7B%204.3%20%7C%20math.ceil%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 4.6 | math.ceil {{ "}}" }}
{{ "{{" }} 4.3 | math.ceil {{ "}}" }}
```
> **output**
```html
5
5
```

## `math.divided_by`

```
math.divided_by <value> <divisor>
```

#### Description

Divides the specified value by another value. If the divisor is an integer, the result will
be floor to and converted back to an integer.

#### Arguments

- `value`: The input value
- `divisor`: The divisor value

#### Returns

The division of `value` by `divisor`.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%208.4%20%7C%20math.divided_by%202.0%20%7C%20math.round%201%20%7D%7D%0A%7B%7B%208.4%20%7C%20math.divided_by%202%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 8.4 | math.divided_by 2.0 | math.round 1 {{ "}}" }}
{{ "{{" }} 8.4 | math.divided_by 2 {{ "}}" }}
```
> **output**
```html
4.2
4
```

## `math.floor`

```
math.floor <value>
```

#### Description

Returns the largest integer less than or equal to the specified number.

#### Arguments

- `value`: The input value

#### Returns

The largest integer less than or equal to the specified number.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%204.6%20%7C%20math.floor%20%7D%7D%0A%7B%7B%204.3%20%7C%20math.floor%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 4.6 | math.floor {{ "}}" }}
{{ "{{" }} 4.3 | math.floor {{ "}}" }}
```
> **output**
```html
4
4
```

## `math.format`

```
math.format <value> <format> <culture>?
```

#### Description

Formats a number value with specified [.NET standard numeric format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)

#### Arguments

- `value`: The input value
- `format`: The format string.
- `culture`: The culture as a string (e.g `en-US`). By default the culture from  is used

#### Returns

The largest integer less than or equal to the specified number.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20255%20%7C%20math.format%20%22X4%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 255 | math.format "X4" {{ "}}" }}
```
> **output**
```html
00FF
```

## `math.is_number`

```
math.is_number <value>
```

#### Description

Returns a boolean indicating if the input value is a number

#### Arguments

- `value`: The input value

#### Returns

**true** if the input value is a number; otherwise false.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20255%20%7C%20math.is_number%20%7D%7D%0A%7B%7B%20%22yo%22%20%7C%20math.is_number%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 255 | math.is_number {{ "}}" }}
{{ "{{" }} "yo" | math.is_number {{ "}}" }}
```
> **output**
```html
true
false
```

## `math.minus`

```
math.minus <value> <with>
```

#### Description

Subtracts from the input value the `with` value

#### Arguments

- `value`: The input value
- `with`: The with value to subtract from `value`

#### Returns

The results of the subtraction: `value` - `with`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20255%20%7C%20math.minus%205%7D%7D&model={})
```scriban-html
{{ "{{" }} 255 | math.minus 5{{ "}}" }}
```
> **output**
```html
250
```

## `math.modulo`

```
math.modulo <value> <with>
```

#### Description

Performs the modulo of the input value with the `with` value

#### Arguments

- `value`: The input value
- `with`: The with value to module `value`

#### Returns

The results of the modulo: `value` % `with`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%2011%20%7C%20math.modulo%2010%7D%7D&model={})
```scriban-html
{{ "{{" }} 11 | math.modulo 10{{ "}}" }}
```
> **output**
```html
1
```

## `math.plus`

```
math.plus <value> <with>
```

#### Description

Performs the addition of the input value with the `with` value

#### Arguments

- `value`: The input value
- `with`: The with value to add to`value`

#### Returns

The results of the addition: `value` + `with`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%201%20%7C%20math.plus%202%7D%7D&model={})
```scriban-html
{{ "{{" }} 1 | math.plus 2{{ "}}" }}
```
> **output**
```html
3
```

## `math.round`

```
math.round <value> <precision: 0>?
```

#### Description

Rounds a value to the nearest integer or to the specified number of fractional digits.

#### Arguments

- `value`: The input value
- `precision`: The number of fractional digits in the return value. Default is 0.

#### Returns

A value rounded to the nearest integer or to the specified number of fractional digits.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%204.6%20%7C%20math.round%20%7D%7D%0A%7B%7B%204.3%20%7C%20math.round%20%7D%7D%0A%7B%7B%204.5612%20%7C%20math.round%202%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 4.6 | math.round {{ "}}" }}
{{ "{{" }} 4.3 | math.round {{ "}}" }}
{{ "{{" }} 4.5612 | math.round 2 {{ "}}" }}
```
> **output**
```html
5
4
4.56
```

## `math.times`

```
math.times <value> <with>
```

#### Description

Performs the multiplication of the input value with the `with` value

#### Arguments

- `value`: The input value
- `with`: The with value to multiply to`value`

#### Returns

The results of the multiplication: `value` * `with`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%202%20%7C%20math.times%203%7D%7D&model={})
```scriban-html
{{ "{{" }} 2 | math.times 3{{ "}}" }}
```
> **output**
```html
6
```

## `math.uuid`

```
math.uuid
```

#### Description

Creates a new UUID

#### Arguments


#### Returns

The created UUID, ex. 2dc55d50-3f6c-446a-87d0-a5a4eed23269

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20math.uuid%20%7D%7D&model={})
```scriban-html
{{ "{{" }} math.uuid {{ "}}" }}
```
> **output**
```html
1c0a4aa8-680e-4bd6-95e9-cdbec45ef057
```

## `math.random`

```
math.random <minValue> <maxValue>
```

#### Description

Creates a random number

#### Arguments

- `minValue`: The inclusive lower bound of the random number returned
- `maxValue`: The exclusive upper bound of the random number returned. maxValue must be greater than or equal to minValue.

#### Returns

A random number greater or equal to minValue and less than maxValue

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20math.random%201%2010%20%7D%7D&model={})
```scriban-html
{{ "{{" }} math.random 1 10 {{ "}}" }}
```
> **output**
```html
7
```

> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
