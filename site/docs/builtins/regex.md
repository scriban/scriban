---
title: "Regex functions"
---

# `regex` functions

Functions exposed through `regex` builtin object.

>*Note:* If your regular expression contains backslashes (` \ `), you will need to do one of the following:
>- Anywhere you would use a ` \ `, use two.  For example: `"\d+\.\d+"` becomes `"\\d+\\.\\d+"`
>- Use [verbatim strings](language.md#31-strings).  For example: `"\d+\.\d+"` becomes `` `\d+\.\d+` ``

- [`regex.escape`](#regexescape)
- [`regex.match`](#regexmatch)
- [`regex.matches`](#regexmatches)
- [`regex.replace`](#regexreplace)
- [`regex.split`](#regexsplit)
- [`regex.unescape`](#regexunescape)

## `regex.escape`

```
regex.escape <pattern>
```

#### Description

Escapes a minimal set of characters (`\`, `*`, `+`, `?`, `|`, `{`, `[`, `(`,`)`, `^`, `$`,`.`, `#`, and white space)
by replacing them with their escape codes.
This instructs the regular expression engine to interpret these characters literally rather than as metacharacters.

#### Arguments

- `pattern`: The input string that contains the text to convert.

#### Returns

A string of characters with metacharacters converted to their escaped form.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%28abc.%2A%29%22%20%7C%20regex.escape%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "(abc.*)" | regex.escape {{ "}}" }}
```
> **output**
```html
\(abc\.\*\)
```

## `regex.match`

```
regex.match <text> <pattern> <options>?
```

#### Description

Searches an input string for a substring that matches a regular expression pattern and returns an array with the match occurences.

#### Arguments

- `text`: The string to search for a match.
- `pattern`: The regular expression pattern to match.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching.
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.

#### Returns

An array that contains all the match groups. The first group contains the entire match. The other elements contain regex matched groups `(..)`. An empty array returned means no match.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22this%20is%20a%20text123%22%20%7C%20regex.match%20%60%28%5Cw%2B%29%20a%20%28%5Ba-z%5D%2B%5Cd%2B%29%60%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "this is a text123" | regex.match `(\w+) a ([a-z]+\d+)` {{ "}}" }}
```
> **output**
```html
["is a text123", "is", "text123"]
```
Notice that the first element returned in the array is the entire regex match, followed by the regex group matches.

## `regex.matches`

```
regex.matches <text> <pattern> <options>?
```

#### Description

Searches an input string for multiple substrings that matches a regular expression pattern and returns an array with the match occurences.

#### Arguments

- `text`: The string to search for a match.
- `pattern`: The regular expression pattern to match.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching.
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.

#### Returns

An array of matches that contains all the match groups. The first group contains the entire match. The other elements contain regex matched groups `(..)`. An empty array returned means no match.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22this%20is%20a%20text123%22%20%7C%20regex.matches%20%60%28%5Cw%2B%29%60%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "this is a text123" | regex.matches `(\w+)` {{ "}}" }}
```
> **output**
```html
[["this", "this"], ["is", "is"], ["a", "a"], ["text123", "text123"]]
```
Notice that the first element returned in the sub array is the entire regex match, followed by the regex group matches.

## `regex.replace`

```
regex.replace <text> <pattern> <replace> <options>?
```

#### Description

In a specified input string, replaces strings that match a regular expression pattern with a specified replacement string.

#### Arguments

- `text`: The string to search for a match.
- `pattern`: The regular expression pattern to match.
- `replace`: The replacement string.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching.
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.

#### Returns

A new string that is identical to the input string, except that the replacement string takes the place of each matched string. If pattern is not matched in the current instance, the method returns the current instance unchanged.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22abbbbcccd%22%20%7C%20regex.replace%20%22b%2Bc%2B%22%20%22-Yo-%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "abbbbcccd" | regex.replace "b+c+" "-Yo-" {{ "}}" }}
```
> **output**
```html
a-Yo-d
```

## `regex.split`

```
regex.split <text> <pattern> <options>?
```

#### Description

Splits an input string into an array of substrings at the positions defined by a regular expression match.

#### Arguments

- `text`: The string to split.
- `pattern`: The regular expression pattern to match.
- `options`: A string with regex options, that can contain the following option characters (default is `null`):
            - `i`: Specifies case-insensitive matching.
            - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
            - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
            - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.

#### Returns

A string array.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22a%2C%20b%20%20%20%2C%20c%2C%20%20%20%20d%22%20%7C%20regex.split%20%60%5Cs%2A%2C%5Cs%2A%60%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "a, b   , c,    d" | regex.split `\s*,\s*` {{ "}}" }}
```
> **output**
```html
["a", "b", "c", "d"]
```

## `regex.unescape`

```
regex.unescape <pattern>
```

#### Description

Converts any escaped characters in the input string.

#### Arguments

- `pattern`: The input string containing the text to convert.

#### Returns

A string of characters with any escaped characters converted to their unescaped form.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%22%5C%5C%28abc%5C%5C.%5C%5C%2A%5C%5C%29%22%20%7C%20regex.unescape%20%7D%7D&model={})
```scriban-html
{{ "{{" }} "\\(abc\\.\\*\\)" | regex.unescape {{ "}}" }}
```
> **output**
```html
(abc.*)
```

> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
