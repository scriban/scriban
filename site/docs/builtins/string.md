---
title: "String functions"
---

# `string` functions

String functions available through the builtin object 'string`.

- [`string.escape`](string#string.escape)
- [`string.append`](string#string.append)
- [`string.capitalize`](string#string.capitalize)
- [`string.capitalizewords`](string#string.capitalizewords)
- [`string.contains`](string#string.contains)
- [`string.empty`](string#string.empty)
- [`string.whitespace`](string#string.whitespace)
- [`string.downcase`](string#string.downcase)
- [`string.ends_with`](string#string.ends_with)
- [`string.equals_ignore_case`](string#string.equals_ignore_case)
- [`string.handleize`](string#string.handleize)
- [`string.literal`](string#string.literal)
- [`string.lstrip`](string#string.lstrip)
- [`string.pluralize`](string#string.pluralize)
- [`string.prepend`](string#string.prepend)
- [`string.remove`](string#string.remove)
- [`string.remove_first`](string#string.remove_first)
- [`string.remove_last`](string#string.remove_last)
- [`string.replace`](string#string.replace)
- [`string.replace_first`](string#string.replace_first)
- [`string.rstrip`](string#string.rstrip)
- [`string.size`](string#string.size)
- [`string.slice`](string#string.slice)
- [`string.slice1`](string#string.slice1)
- [`string.split`](string#string.split)
- [`string.starts_with`](string#string.starts_with)
- [`string.strip`](string#string.strip)
- [`string.strip_newlines`](string#string.strip_newlines)
- [`string.to_int`](string#string.to_int)
- [`string.to_long`](string#string.to_long)
- [`string.to_float`](string#string.to_float)
- [`string.to_double`](string#string.to_double)
- [`string.truncate`](string#string.truncate)
- [`string.truncatewords`](string#string.truncatewords)
- [`string.upcase`](string#string.upcase)
- [`string.md5`](string#string.md5)
- [`string.sha1`](string#string.sha1)
- [`string.sha256`](string#string.sha256)
- [`string.sha512`](string#string.sha512)
- [`string.hmac_sha1`](string#string.hmac_sha1)
- [`string.hmac_sha256`](string#string.hmac_sha256)
- [`string.hmac_sha512`](string#string.hmac_sha512)
- [`string.pad_left`](string#string.pad_left)
- [`string.pad_right`](string#string.pad_right)
- [`string.base64_encode`](string#string.base64_encode)
- [`string.base64_decode`](string#string.base64_decode)
- [`string.index_of`](string#string.index_of)

## `string.escape`

```
string.escape <text>
```

#### Description

Escapes a string with escape characters.

#### Arguments

- `text`: The input string

#### Returns

The two strings concatenated

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hel%5Ctlo%5Cn%5C%22W%5C%5Corld%22%20%7C%20string.escape%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hel\tlo\n\"W\\orld" | string.escape {{ "}}" }}
```
> **output**
```html
Hel\tlo\n\"W\\orld
```

## `string.append`

```
string.append <text> <with>
```

#### Description

Concatenates two strings

#### Arguments

- `text`: The input string
- `with`: The text to append

#### Returns

The two strings concatenated

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%22%20%7C%20string.append%20%22%20World%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello" | string.append " World" {{ "}}" }}
```
> **output**
```html
Hello World
```

## `string.capitalize`

```
string.capitalize <text>
```

#### Description

Converts the first character of the passed string to a upper case character.

#### Arguments

- `text`: The input string

#### Returns

The capitalized input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.capitalize%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.capitalize {{ "}}" }}
```
> **output**
```html
Test
```

## `string.capitalizewords`

```
string.capitalizewords <text>
```

#### Description

Converts the first character of each word in the passed string to a upper case character.

#### Arguments

- `text`: The input string

#### Returns

The capitalized input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22This%20is%20easy%22%20%7C%20string.capitalizewords%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "This is easy" | string.capitalizewords {{ "}}" }}
```
> **output**
```html
This Is Easy
```

## `string.contains`

```
string.contains <text> <value>
```

#### Description

Returns a boolean indicating whether the input string contains the specified string `value`.

#### Arguments

- `text`: The input string
- `value`: The string to look for

#### Returns

 if `text` contains the string `value`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22This%20is%20easy%22%20%7C%20string.contains%20%22easy%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "This is easy" | string.contains "easy" {{ "}}" }}
```
> **output**
```html
true
```

## `string.empty`

```
string.empty <text>
```

#### Description

Returns a boolean indicating whether the input string is an empty string.

#### Arguments

- `text`: The input string

#### Returns

 if `text` is an empty string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%22%20%7C%20string.empty%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "" | string.empty {{ "}}" }}
```
> **output**
```html
true
```

## `string.whitespace`

```
string.whitespace <text>
```

#### Description

Returns a boolean indicating whether the input string is empty or contains only whitespace characters.

#### Arguments

- `text`: The input string

#### Returns

 if `text` is empty string or contains only whitespace characters

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%22%20%7C%20string.whitespace%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "" | string.whitespace {{ "}}" }}
```
> **output**
```html
true
```

## `string.downcase`

```
string.downcase <text>
```

#### Description

Converts the string to lower case.

#### Arguments

- `text`: The input string

#### Returns

The input string lower case

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22TeSt%22%20%7C%20string.downcase%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "TeSt" | string.downcase {{ "}}" }}
```
> **output**
```html
test
```

## `string.ends_with`

```
string.ends_with <text> <value>
```

#### Description

Returns a boolean indicating whether the input string ends with the specified string `value`.

#### Arguments

- `text`: The input string
- `value`: The string to look for

#### Returns

 if `text` ends with the specified string `value`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22This%20is%20easy%22%20%7C%20string.ends_with%20%22easy%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "This is easy" | string.ends_with "easy" {{ "}}" }}
```
> **output**
```html
true
```

## `string.equals_ignore_case`

```
string.equals_ignore_case <text> <value>
```

#### Description

Returns a boolean indicating whether the input string is equal to specified string 'value'. Comparison is case insensitive.

#### Arguments

- `text`: The input string
- `value`: The string to compare

#### Returns

 if `text` is equal to string `value`, ignoring case

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Scriban%22%20%7C%20string.equals_ignore_case%20%22SCRIBAN%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Scriban" | string.equals_ignore_case "SCRIBAN" {{ "}}" }}
```
> **output**
```html
true
```

## `string.handleize`

```
string.handleize <text>
```

#### Description

Returns a url handle from the input string.

#### Arguments

- `text`: The input string

#### Returns

A url handle

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%27100%25%20M%20%26%20Ms%21%21%21%27%20%7C%20string.handleize%20%20%7D%7D&model={})
```scriban-html
{{ "{{" }} '100% M & Ms!!!' | string.handleize  {{ "}}" }}
```
> **output**
```html
100-m-ms
```

## `string.literal`

```
string.literal <text>
```

#### Description

Return a string literal enclosed with double quotes of the input string.

#### Arguments

- `text`: The string to return a literal from.

#### Returns

The literal of a string.

#### Examples

If the input string has non printable characters or they need contain a double quote, they will be escaped.
> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%27Hello%5Cn%22World%22%27%20%7C%20string.literal%20%7D%7D&model={})
```scriban-html
{{ "{{" }} 'Hello\n"World"' | string.literal {{ "}}" }}
```
> **output**
```html
"Hello\n\"World\""
```

## `string.lstrip`

```
string.lstrip <text>
```

#### Description

Removes any whitespace characters on the **left** side of the input string.

#### Arguments

- `text`: The input string

#### Returns

The input string without any left whitespace characters

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%27%20%20%20too%20many%20spaces%27%20%7C%20string.lstrip%20%20%7D%7D&model={})
```scriban-html
{{ "{{" }} '   too many spaces' | string.lstrip  {{ "}}" }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
too many spaces
```

## `string.pluralize`

```
string.pluralize <number> <singular> <plural>
```

#### Description

Outputs the singular or plural version of a string based on the value of a number.

#### Arguments

- `number`: The number to check
- `singular`: The singular string to return if number is == 1
- `plural`: The plural string to return if number is != 1

#### Returns

The singular or plural string based on number

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20products.size%20%7D%7D%20%7B%7Bproducts.size%20%7C%20string.pluralize%20%27product%27%20%27products%27%20%7D%7D&model={})
```scriban-html
{{ "{{" }} products.size {{ "}}" }} {{ "{{" }}products.size | string.pluralize 'product' 'products' {{ "}}" }}
```
> **output**
```html
7 products
```

## `string.prepend`

```
string.prepend <text> <by>
```

#### Description

Concatenates two strings by placing the `by` string in from of the `text` string

#### Arguments

- `text`: The input string
- `by`: The string to prepend to `text`

#### Returns

The two strings concatenated

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22World%22%20%7C%20string.prepend%20%22Hello%20%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "World" | string.prepend "Hello " {{ "}}" }}
```
> **output**
```html
Hello World
```

## `string.remove`

```
string.remove <text> <remove>
```

#### Description

Removes all occurrences of a substring from a string.

#### Arguments

- `text`: The input string
- `remove`: The substring to remove from the `text` string

#### Returns

The input string with the all occurence of a substring removed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%2C%20world.%20Goodbye%2C%20world.%22%20%7C%20string.remove%20%22world%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello, world. Goodbye, world." | string.remove "world" {{ "}}" }}
```
> **output**
```html
Hello, . Goodbye, .
```

## `string.remove_first`

```
string.remove_first <text> <remove>
```

#### Description

Removes the first occurrence of a substring from a string.

#### Arguments

- `text`: The input string
- `remove`: The first occurence of substring to remove from the `text` string

#### Returns

The input string with the first occurence of a substring removed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%2C%20world.%20Goodbye%2C%20world.%22%20%7C%20string.remove_first%20%22world%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello, world. Goodbye, world." | string.remove_first "world" {{ "}}" }}
```
> **output**
```html
Hello, . Goodbye, world.
```

## `string.remove_last`

```
string.remove_last <text> <remove>
```

#### Description

Removes the last occurrence of a substring from a string.

#### Arguments

- `text`: The input string
- `remove`: The last occurence of substring to remove from the `text` string

#### Returns

The input string with the first occurence of a substring removed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%2C%20world.%20Goodbye%2C%20world.%22%20%7C%20string.remove_last%20%22world%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello, world. Goodbye, world." | string.remove_last "world" {{ "}}" }}
```
> **output**
```html
Hello, world. Goodbye, .
```

## `string.replace`

```
string.replace <text> <match> <replace>
```

#### Description

Replaces all occurrences of a string with a substring.

#### Arguments

- `text`: The input string
- `match`: The substring to find in the `text` string
- `replace`: The substring used to replace the string matched by `match` in the input `text`

#### Returns

The input string replaced

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%2C%20world.%20Goodbye%2C%20world.%22%20%7C%20string.replace%20%22world%22%20%22buddy%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello, world. Goodbye, world." | string.replace "world" "buddy" {{ "}}" }}
```
> **output**
```html
Hello, buddy. Goodbye, buddy.
```

## `string.replace_first`

```
string.replace_first <text> <match> <replace> <fromEnd: False>?
```

#### Description

Replaces the first occurrence of a string with a substring.

#### Arguments

- `text`: The input string
- `match`: The substring to find in the `text` string
- `replace`: The substring used to replace the string matched by `match` in the input `text`
- `fromEnd`: if true start match from end

#### Returns

The input string replaced

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22Hello%2C%20world.%20Goodbye%2C%20world.%22%20%7C%20string.replace_first%20%22world%22%20%22buddy%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "Hello, world. Goodbye, world." | string.replace_first "world" "buddy" {{ "}}" }}
```
> **output**
```html
Hello, buddy. Goodbye, world.
```

## `string.rstrip`

```
string.rstrip <text>
```

#### Description

Removes any whitespace characters on the **right** side of the input string.

#### Arguments

- `text`: The input string

#### Returns

The input string without any left whitespace characters

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%27%20%20%20too%20many%20spaces%20%20%20%20%20%20%20%20%20%20%20%27%20%7C%20string.rstrip%20%20%7D%7D&model={})
```scriban-html
{{ "{{" }} '   too many spaces           ' | string.rstrip  {{ "}}" }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
   too many spaces
```

## `string.size`

```
string.size <text>
```

#### Description

Returns the number of characters from the input string

#### Arguments

- `text`: The input string

#### Returns

The length of the input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.size%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.size {{ "}}" }}
```
> **output**
```html
4
```

## `string.slice`

```
string.slice <text> <start> <length>?
```

#### Description

The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring.
If no second parameter is given, a substring with the remaining characters will be returned.

#### Arguments

- `text`: The input string
- `start`: The starting index character where the slice should start from the input `text` string
- `length`: The number of character. Default is 0, meaning that the remaining of the string will be returned.

#### Returns

The input string sliced

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22hello%22%20%7C%20string.slice%200%20%7D%7D%0A%7B%7B%20%22hello%22%20%7C%20string.slice%201%20%7D%7D%0A%7B%7B%20%22hello%22%20%7C%20string.slice%201%203%20%7D%7D%0A%7B%7B%20%22hello%22%20%7C%20string.slice%201%20length%3A3%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "hello" | string.slice 0 {{ "}}" }}
{{ "{{" }} "hello" | string.slice 1 {{ "}}" }}
{{ "{{" }} "hello" | string.slice 1 3 {{ "}}" }}
{{ "{{" }} "hello" | string.slice 1 length:3 {{ "}}" }}
```
> **output**
```html
hello
ello
ell
ell
```

## `string.slice1`

```
string.slice1 <text> <start> <length: 1>?
```

#### Description

The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring.
If no second parameter is given, a substring with the first character will be returned.

#### Arguments

- `text`: The input string
- `start`: The starting index character where the slice should start from the input `text` string
- `length`: The number of character. Default is 1, meaning that only the first character at `start` position will be returned.

#### Returns

The input string sliced

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22hello%22%20%7C%20string.slice1%200%20%7D%7D%0A%7B%7B%20%22hello%22%20%7C%20string.slice1%201%20%7D%7D%0A%7B%7B%20%22hello%22%20%7C%20string.slice1%201%203%20%7D%7D%0A%7B%7B%20%22hello%22%20%7C%20string.slice1%201%20length%3A%203%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "hello" | string.slice1 0 {{ "}}" }}
{{ "{{" }} "hello" | string.slice1 1 {{ "}}" }}
{{ "{{" }} "hello" | string.slice1 1 3 {{ "}}" }}
{{ "{{" }} "hello" | string.slice1 1 length: 3 {{ "}}" }}
```
> **output**
```html
h
e
ell
ell
```

## `string.split`

```
string.split <text> <match>
```

#### Description

The `split` function takes on a substring as a parameter.
The substring is used as a delimiter to divide a string into an array. You can output different parts of an array using `array` functions.

#### Arguments

- `text`: The input string
- `match`: The string used to split the input `text` string

#### Returns

An enumeration of the substrings

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20for%20word%20in%20%22Hi%2C%20how%20are%20you%20today%3F%22%20%7C%20string.split%20%27%20%27%20~%7D%7D%0A%7B%7B%20word%20%7D%7D%0A%7B%7B%20end%20~%7D%7D&model={})
```scriban-html
{{ "{{" }} for word in "Hi, how are you today?" | string.split ' ' ~{{ "}}" }}
{{ "{{" }} word {{ "}}" }}
{{ "{{" }} end ~{{ "}}" }}
```
> **output**
```html
Hi,
how
are
you
today?
```

## `string.starts_with`

```
string.starts_with <text> <value>
```

#### Description

Returns a boolean indicating whether the input string starts with the specified string `value`.

#### Arguments

- `text`: The input string
- `value`: The string to look for

#### Returns

 if `text` starts with the specified string `value`

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22This%20is%20easy%22%20%7C%20string.starts_with%20%22This%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "This is easy" | string.starts_with "This" {{ "}}" }}
```
> **output**
```html
true
```

## `string.strip`

```
string.strip <text>
```

#### Description

Removes any whitespace characters on the **left** and **right** side of the input string.

#### Arguments

- `text`: The input string

#### Returns

The input string without any left and right whitespace characters

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%27%20%20%20too%20many%20spaces%20%20%20%20%20%20%20%20%20%20%20%27%20%7C%20string.strip%20%20%7D%7D&model={})
```scriban-html
{{ "{{" }} '   too many spaces           ' | string.strip  {{ "}}" }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
too many spaces
```

## `string.strip_newlines`

```
string.strip_newlines <text>
```

#### Description

Removes any line breaks/newlines from a string.

#### Arguments

- `text`: The input string

#### Returns

The input string without any breaks/newlines characters

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22This%20is%20a%20string.%5Cr%5Cn%20With%20%5Cnanother%20%5Crstring%22%20%7C%20string.strip_newlines%20%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "This is a string.\r\n With \nanother \rstring" | string.strip_newlines  {{ "}}" }}
```
> **output**
```html
This is a string. With another string
```

## `string.to_int`

```
string.to_int <text>
```

#### Description

Converts a string to an integer

#### Arguments

- `text`: The input string

#### Returns

A 32 bit integer or null if conversion failed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22123%22%20%7C%20string.to_int%20%2B%201%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "123" | string.to_int + 1 {{ "}}" }}
```
> **output**
```html
124
```

## `string.to_long`

```
string.to_long <text>
```

#### Description

Converts a string to a long 64 bit integer

#### Arguments

- `text`: The input string

#### Returns

A 64 bit integer or null if conversion failed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22123678912345678%22%20%7C%20string.to_long%20%2B%201%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "123678912345678" | string.to_long + 1 {{ "}}" }}
```
> **output**
```html
123678912345679
```

## `string.to_float`

```
string.to_float <text>
```

#### Description

Converts a string to a float

#### Arguments

- `text`: The input string

#### Returns

A 32 bit float or null if conversion failed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22123.4%22%20%7C%20string.to_float%20%2B%201%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "123.4" | string.to_float + 1 {{ "}}" }}
```
> **output**
```html
124.4
```

## `string.to_double`

```
string.to_double <text>
```

#### Description

Converts a string to a double

#### Arguments

- `text`: The input string

#### Returns

A 64 bit float or null if conversion failed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22123.4%22%20%7C%20string.to_double%20%2B%201%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "123.4" | string.to_double + 1 {{ "}}" }}
```
> **output**
```html
124.4
```

## `string.truncate`

```
string.truncate <text> <length> <ellipsis>?
```

#### Description

Truncates a string down to the number of characters passed as the first parameter.
An ellipsis (...) is appended to the truncated string and is included in the character count

#### Arguments

- `text`: The input string
- `length`: The maximum length of the output string, including the length of the `ellipsis`
- `ellipsis`: The ellipsis to append to the end of the truncated string

#### Returns

The truncated input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22The%20cat%20came%20back%20the%20very%20next%20day%22%20%7C%20string.truncate%2013%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "The cat came back the very next day" | string.truncate 13 {{ "}}" }}
```
> **output**
```html
The cat ca...
```

## `string.truncatewords`

```
string.truncatewords <text> <count> <ellipsis>?
```

#### Description

Truncates a string down to the number of words passed as the first parameter.
An ellipsis (...) is appended to the truncated string.

#### Arguments

- `text`: The input string
- `count`: The number of words to keep from the input `text` string before appending the `ellipsis`
- `ellipsis`: The ellipsis to append to the end of the truncated string

#### Returns

The truncated input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22The%20cat%20came%20back%20the%20very%20next%20day%22%20%7C%20string.truncatewords%204%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "The cat came back the very next day" | string.truncatewords 4 {{ "}}" }}
```
> **output**
```html
The cat came back...
```

## `string.upcase`

```
string.upcase <text>
```

#### Description

Converts the string to uppercase

#### Arguments

- `text`: The input string

#### Returns

The input string upper case

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.upcase%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.upcase {{ "}}" }}
```
> **output**
```html
TEST
```

## `string.md5`

```
string.md5 <text>
```

#### Description

Computes the `md5` hash of the input string

#### Arguments

- `text`: The input string

#### Returns

The `md5` hash of the input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.md5%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.md5 {{ "}}" }}
```
> **output**
```html
098f6bcd4621d373cade4e832627b4f6
```

## `string.sha1`

```
string.sha1 <text>
```

#### Description

Computes the `sha1` hash of the input string

#### Arguments

- `text`: The input string

#### Returns

The `sha1` hash of the input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.sha1%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.sha1 {{ "}}" }}
```
> **output**
```html
a94a8fe5ccb19ba61c4c0873d391e987982fbbd3
```

## `string.sha256`

```
string.sha256 <text>
```

#### Description

Computes the `sha256` hash of the input string

#### Arguments

- `text`: The input string

#### Returns

The `sha256` hash of the input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.sha256%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.sha256 {{ "}}" }}
```
> **output**
```html
9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08
```

## `string.sha512`

```
string.sha512 <text>
```

#### Description

Computes the `sha512` hash of the input string

#### Arguments

- `text`: The input string

#### Returns

The `sha512` hash of the input string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.sha512%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.sha512 {{ "}}" }}
```
> **output**
```html
ee26b0dd4af7e749aa1a8ee3c10ae9923f618980772e473f8819a5d4940e0db27ac185f8a0e1d5f84f88bc887fd67b143732c304cc5fa9ad8e6f57f50028a8ff
```

## `string.hmac_sha1`

```
string.hmac_sha1 <text> <secretKey>
```

#### Description

Converts a string into a SHA-1 hash using a hash message authentication code (HMAC). Pass the secret key for the message as a parameter to the function.

#### Arguments

- `text`: The input string
- `secretKey`: The secret key

#### Returns

The `SHA-1` hash of the input string using a hash message authentication code (HMAC)

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.hmac_sha1%20%22secret%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.hmac_sha1 "secret" {{ "}}" }}
```
> **output**
```html
1aa349585ed7ecbd3b9c486a30067e395ca4b356
```

## `string.hmac_sha256`

```
string.hmac_sha256 <text> <secretKey>
```

#### Description

Converts a string into a SHA-256 hash using a hash message authentication code (HMAC). Pass the secret key for the message as a parameter to the function.

#### Arguments

- `text`: The input string
- `secretKey`: The secret key

#### Returns

The `SHA-256` hash of the input string using a hash message authentication code (HMAC)

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.hmac_sha256%20%22secret%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.hmac_sha256 "secret" {{ "}}" }}
```
> **output**
```html
0329a06b62cd16b33eb6792be8c60b158d89a2ee3a876fce9a881ebb488c0914
```

## `string.hmac_sha512`

```
string.hmac_sha512 <text> <secretKey>
```

#### Description

Converts a string into a SHA-512 hash using a hash message authentication code (HMAC). Pass the secret key for the message as a parameter to the function.

#### Arguments

- `text`: The input string
- `secretKey`: The secret key

#### Returns

The `SHA-512` hash of the input string using a hash message authentication code (HMAC)

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22test%22%20%7C%20string.hmac_sha512%20%22secret%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "test" | string.hmac_sha512 "secret" {{ "}}" }}
```
> **output**
```html
f8a4f0a209167bc192a1bffaa01ecdb09e06c57f96530d92ec9ccea0090d290e55071306d6b654f26ae0c8721f7e48a2d7130b881151f2cec8d61d941a6be88a
```

## `string.pad_left`

```
string.pad_left <text> <width>
```

#### Description

Pads a string with leading spaces to a specified total length.

#### Arguments

- `text`: The input string
- `width`: The number of characters in the resulting string

#### Returns

The input string padded

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=hello%7B%7B%20%22world%22%20%7C%20string.pad_left%2010%20%7D%7D&model={})
```scriban-html
hello{{ "{{" }} "world" | string.pad_left 10 {{ "}}" }}
```
> **output**
```html
hello     world
```

## `string.pad_right`

```
string.pad_right <text> <width>
```

#### Description

Pads a string with trailing spaces to a specified total length.

#### Arguments

- `text`: The input string
- `width`: The number of characters in the resulting string

#### Returns

The input string padded

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22hello%22%20%7C%20string.pad_right%2010%20%7D%7Dworld&model={})
```scriban-html
{{ "{{" }} "hello" | string.pad_right 10 {{ "}}" }}world
```
> **output**
```html
hello     world
```

## `string.base64_encode`

```
string.base64_encode <text>
```

#### Description

Encodes a string to its Base64 representation.
Its character encoded will be UTF-8.

#### Arguments

- `text`: The string to encode

#### Returns

The encoded string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22hello%22%20%7C%20string.base64_encode%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "hello" | string.base64_encode {{ "}}" }}
```
> **output**
```html
aGVsbG8=
```

## `string.base64_decode`

```
string.base64_decode <text>
```

#### Description

Decodes a Base64-encoded string to a byte array.
The encoding of the bytes is assumed to be UTF-8.

#### Arguments

- `text`: The string to decode

#### Returns

The decoded string

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22aGVsbG8%3D%22%20%7C%20string.base64_decode%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "aGVsbG8=" | string.base64_decode {{ "}}" }}
```
> **output**
```html
hello
```

## `string.index_of`

```
string.index_of <text> <search> <startIndex>? <count>? <stringComparison>?
```

#### Description

Reports the zero-based index of the first occurrence of the specified string in this instance.
The search starts at a specified character position and examines a specified number of character positions.

#### Arguments

- `text`: The string to search
- `search`: The string to find the index of.
- `startIndex`: If provided, the search starting position.
If , search will start at the beginning of .
- `count`: If provided, the number of character positions to examine.
If , all character positions will be considered.
- `stringComparison`: If provided, the comparison rules for the search.
If , Allowed values are one of the following:
    'CurrentCulture', 'CurrentCultureIgnoreCase', 'InvariantCulture', 'InvariantCultureIgnoreCase', 'Ordinal', 'OrdinalIgnoreCase'

#### Returns

The zero-based index position of the  parameter from the start of if  is found, or -1 if it is not. If value is ,
            the return value is  (if  is not provided, the return value would be zero).

#### Examples



> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
