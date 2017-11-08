# Builtins

This document describes the various built-in functions available in from standard scriban template.


- [`array` functions](#array-functions)
  - [`array.add`](#arrayadd)
  - [`array.add_range`](#arrayadd_range)
  - [`array.compact`](#arraycompact)
  - [`array.concat`](#arrayconcat)
  - [`array.cycle`](#arraycycle)
  - [`array.first`](#arrayfirst)
  - [`array.insert_at`](#arrayinsert_at)
  - [`array.join`](#arrayjoin)
  - [`array.last`](#arraylast)
  - [`array.limit`](#arraylimit)
  - [`array.map`](#arraymap)
  - [`array.offset`](#arrayoffset)
  - [`array.remove_at`](#arrayremove_at)
  - [`array.reverse`](#arrayreverse)
  - [`array.size`](#arraysize)
  - [`array.sort`](#arraysort)
  - [`array.uniq`](#arrayuniq)
- [`date` functions](#date-functions)
  - [`date.add_days`](#dateadd_days)
  - [`date.add_months`](#dateadd_months)
  - [`date.add_years`](#dateadd_years)
  - [`date.add_hours`](#dateadd_hours)
  - [`date.add_minutes`](#dateadd_minutes)
  - [`date.add_seconds`](#dateadd_seconds)
  - [`date.add_milliseconds`](#dateadd_milliseconds)
  - [`date.parse`](#dateparse)
  - [`date.to_string`](#dateto_string)
- [`html` functions](#html-functions)
  - [`html.strip`](#htmlstrip)
  - [`html.escape`](#htmlescape)
  - [`html.url_encode`](#htmlurl_encode)
  - [`html.url_escape`](#htmlurl_escape)
- [`math` functions](#math-functions)
  - [`math.abs`](#mathabs)
  - [`math.ceil`](#mathceil)
  - [`math.divided_by`](#mathdivided_by)
  - [`math.floor`](#mathfloor)
  - [`math.format`](#mathformat)
  - [`math.is_number`](#mathis_number)
  - [`math.minus`](#mathminus)
  - [`math.modulo`](#mathmodulo)
  - [`math.plus`](#mathplus)
  - [`math.round`](#mathround)
  - [`math.times`](#mathtimes)
- [`object` functions](#object-functions)
  - [`object.default`](#objectdefault)
  - [`object.has_key`](#objecthas_key)
  - [`object.has_value`](#objecthas_value)
  - [`object.keys`](#objectkeys)
  - [`object.size`](#objectsize)
  - [`object.typeof`](#objecttypeof)
  - [`object.values`](#objectvalues)
- [`regex` functions](#regex-functions)
  - [`regex.escape`](#regexescape)
  - [`regex.match`](#regexmatch)
  - [`regex.replace`](#regexreplace)
  - [`regex.split`](#regexsplit)
  - [`regex.unescape`](#regexunescape)
- [`string` functions](#string-functions)
  - [`string.append`](#stringappend)
  - [`string.capitalize`](#stringcapitalize)
  - [`string.capitalizewords`](#stringcapitalizewords)
  - [`string.contains`](#stringcontains)
  - [`string.downcase`](#stringdowncase)
  - [`string.ends_with`](#stringends_with)
  - [`string.handleize`](#stringhandleize)
  - [`string.lstrip`](#stringlstrip)
  - [`string.pluralize`](#stringpluralize)
  - [`string.prepend`](#stringprepend)
  - [`string.remove`](#stringremove)
  - [`string.remove_first`](#stringremove_first)
  - [`string.replace`](#stringreplace)
  - [`string.replace_first`](#stringreplace_first)
  - [`string.rstrip`](#stringrstrip)
  - [`string.size`](#stringsize)
  - [`string.slice`](#stringslice)
  - [`string.slice1`](#stringslice1)
  - [`string.split`](#stringsplit)
  - [`string.starts_with`](#stringstarts_with)
  - [`string.strip`](#stringstrip)
  - [`string.strip_newlines`](#stringstrip_newlines)
  - [`string.truncate`](#stringtruncate)
  - [`string.truncatewords`](#stringtruncatewords)
  - [`string.upcase`](#stringupcase)
  - [`string.md5`](#stringmd5)
  - [`string.sha1`](#stringsha1)
  - [`string.sha256`](#stringsha256)
  - [`string.hmac_sha1`](#stringhmac_sha1)
  - [`string.hmac_sha256`](#stringhmac_sha256)
- [`timespan` functions](#timespan-functions)
  - [`timespan.from_days`](#timespanfrom_days)
  - [`timespan.from_hours`](#timespanfrom_hours)
  - [`timespan.from_minutes`](#timespanfrom_minutes)
  - [`timespan.from_seconds`](#timespanfrom_seconds)
  - [`timespan.from_milliseconds`](#timespanfrom_milliseconds)
  - [`timespan.parse`](#timespanparse)

[:top:](#builtins)

## `array` functions


[:top:](#builtins)
### `array.add`

```
array.add <list> <value>
```

#### Description

Adds a value to the input list.

#### Arguments

- `list`: The input list
- `value`: The value to add at the end of the list

#### Returns

A new list with the value added

#### Examples

```scriban-html
{{ [1, 2, 3] | array.add 4 }}
```
```html
[1, 2, 3, 4]
```

[:top:](#builtins)
### `array.add_range`

```
array.add_range <list1> <list2>
```

#### Description

Concatenates two lists.

#### Arguments

- `list1`: The 1st input list
- `list2`: The 2nd input list

#### Returns

The concatenation of the two input lists

#### Examples

```scriban-html
{{ [1, 2, 3] | array.concat [4, 5] }}
```
```html
[1, 2, 3, 4, 5]
```

[:top:](#builtins)
### `array.compact`

```
array.compact <list>
```

#### Description

Removes any non-null values from the input list.

#### Arguments

- `list`: An input list

#### Returns

Returns a list with null value removed

#### Examples

```scriban-html
{{ [1, null, 3] | array.compact }}
```
```html
[1, 3]
```

[:top:](#builtins)
### `array.concat`

```
array.concat <list1> <list2>
```

#### Description

Concatenates two lists.

#### Arguments

- `list1`: The 1st input list
- `list2`: The 2nd input list

#### Returns

The concatenation of the two input lists

#### Examples

```scriban-html
{{ [1, 2, 3] | array.concat [4, 5] }}
```
```html
[1, 2, 3, 4, 5]
```

[:top:](#builtins)
### `array.cycle`

```
array.cycle <list> <group>?=
```

#### Description

Loops through a group of strings and outputs them in the order that they were passed as parameters. Each time cycle is called, the next string that was passed as a parameter is output.

#### Arguments

- `list`: An input list
- `group`: The group used. Default is `null`

#### Returns

Returns a list with null value removed

#### Examples

```scriban-html
{{ cycle ['one', 'two', 'three'] }}
{{ cycle ['one', 'two', 'three'] }}
{{ cycle ['one', 'two', 'three'] }}
{{ cycle ['one', 'two', 'three'] }}
```
```html
one
two
three
one
```
`cycle` accepts a parameter called cycle group in cases where you need multiple cycle blocks in one template. 
If no name is supplied for the cycle group, then it is assumed that multiple calls with the same parameters are one group.

[:top:](#builtins)
### `array.first`

```
array.first <list>
```

#### Description

Returns the first element of the input `list`.

#### Arguments

- `list`: The input list

#### Returns

The first element of the input `list`.

#### Examples

```scriban-html
{{ [4, 5, 6] | array.first }}
```
```html
4
```

[:top:](#builtins)
### `array.insert_at`

```
array.insert_at <list> <index> <value>
```

#### Description

Inserts a `value` at the specified index in the input `list`.

#### Arguments

- `list`: The input list
- `index`: The index in the list where to insert the element
- `value`: The value to insert

#### Returns

A new list with the element inserted.

#### Examples

```scriban-html
{{ ["a", "b", "c"] | array.insert_at 2 "Yo" }}
```
```html
[a, b, Yo, c]
```

[:top:](#builtins)
### `array.join`

```
array.join <list> <delimiter>
```

#### Description

Joins the element of a list separated by a delimiter string and return the concatenated string.

#### Arguments

- `list`: The input list
- `delimiter`: The delimiter string to use to separate elements in the output string

#### Returns

A new list with the element inserted.

#### Examples

```scriban-html
{{ [1, 2, 3] | array.join "|" }}
```
```html
1|2|3
```

[:top:](#builtins)
### `array.last`

```
array.last <list>
```

#### Description

Returns the last element of the input `list`.

#### Arguments

- `list`: The input list

#### Returns

The last element of the input `list`.

#### Examples

```scriban-html
{{ [4, 5, 6] | array.last }}
```
```html
6
```

[:top:](#builtins)
### `array.limit`

```
array.limit <list> <count>
```

#### Description

Returns a limited number of elments from the input list

#### Arguments

- `list`: The input list
- `count`: The number of elements to return from the input list

#### Returns



#### Examples

```scriban-html
{{ [4, 5, 6] | array.limit 2 }}
```
```html
[4, 5]
```

[:top:](#builtins)
### `array.map`

```
array.map <list> <member>
```

#### Description

Accepts an array element's attribute as a parameter and creates an array out of each array element's value.

#### Arguments

- `list`: The input list
- `member`: The member to extract the value from

#### Returns



#### Examples

```scriban-html
{{ 
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.map "type" | array.uniq | array.sort }}
```
```html
[electronics, fruit, furniture]
```

[:top:](#builtins)
### `array.offset`

```
array.offset <list> <index>
```

#### Description

Returns the remaining of the list after the specified offset

#### Arguments

- `list`: The input list
- `index`: The index of a list to return elements

#### Returns



#### Examples

```scriban-html
{{ [4, 5, 6, 7, 8] | array.limit 2 }}
```
```html
[6, 7, 8]
```

[:top:](#builtins)
### `array.remove_at`

```
array.remove_at <list> <index>
```

#### Description

Removes an element at the specified `index` from the input `list`

#### Arguments

- `list`: The input list
- `index`: The index of a list to return elements

#### Returns

A new list with the element removed. If index is negative, remove at the end of the list.

#### Examples

```scriban-html
{{ [4, 5, 6, 7, 8] | array.remove_at 2 }}
```
```html
[4, 5, 7, 8]
```
If the `index` is negative, removes at the end of the list:
```scriban-html
{{ [4, 5, 6, 7, 8] | array.remove_at -1 }}
```
```html
[4, 5, 6, 7]
```

[:top:](#builtins)
### `array.reverse`

```
array.reverse <list>
```

#### Description

Reverses the input `list`

#### Arguments

- `list`: The input list

#### Returns

A new list in reversed order.

#### Examples

```scriban-html
{{ [4, 5, 6, 7] | array.reverse }}
```
```html
[7, 6, 5, 4]
```

[:top:](#builtins)
### `array.size`

```
array.size <list>
```

#### Description

Returns the number of elements in the input `list`

#### Arguments

- `list`: The input list

#### Returns

A number of elements in the input `list`.

#### Examples

```scriban-html
{{ [4, 5, 6] | array.size }}
```
```html
3
```

[:top:](#builtins)
### `array.sort`

```
array.sort <list> <member>?=
```

#### Description

Sorts the elements of the input `list` according to the value of each element or the value of the specified `member` of each element

#### Arguments

- `list`: The input list
- `member`: The member name to sort according to its value. Null by default, meaning that the element's value are used instead.

#### Returns

A list sorted according to the value of each element or the value of the specified `member` of each element.

#### Examples

Sorts by element's value: 
```scriban-html
{{ [10, 2, 6] | array.sort }}
```
```html
[2, 6, 10]
```
Sorts by elements member's value: 
```scriban-html
{{
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.sort "title" | array.map "title"
}}
```
```html
[computer, orange, sofa]
```

[:top:](#builtins)
### `array.uniq`

```
array.uniq <list>
```

#### Description

Returns the unique elements of the input `list`.

#### Arguments

- `list`: The input list

#### Returns

A list of unique elements of the input `list`.

#### Examples

```scriban-html
{{ [1, 1, 4, 5, 8, 8] | array.uniq }}
```
```html
[1, 4, 5, 8]
```
[:top:](#builtins)

## `date` functions


[:top:](#builtins)
### `date.add_days`

```
date.add_days <date> <days>
```

#### Description

Adds the specified number of days to the input date. 

#### Arguments

- `date`: The date.
- `days`: The days.

#### Returns

A new date

#### Examples

```
{{ date.parse '2016/01/05' | date.add_days 1 }}
```
```html
2017
```

[:top:](#builtins)
### `date.add_months`

```
date.add_months <date> <months>
```

#### Description

Adds the specified number of months to the input date. 

#### Arguments

- `date`: The date.
- `months`: The months.

#### Returns

A new date

#### Examples

```
{{ date.parse '2016/01/05' | date.add_months 1 }}
```
```html
5 Feb 2016
```

[:top:](#builtins)
### `date.add_years`

```
date.add_years <date> <years>
```

#### Description

Adds the specified number of years to the input date. 

#### Arguments

- `date`: The date.
- `years`: The years.

#### Returns

A new date

#### Examples

```
{{ date.parse '2016/01/05' | date.add_years 1 }}
```
```html
5 Jan 2017
```

[:top:](#builtins)
### `date.add_hours`

```
date.add_hours <date> <hours>
```

#### Description

Adds the specified number of hours to the input date. 

#### Arguments

- `date`: The date.
- `hours`: The hours.

#### Returns

A new date

#### Examples



[:top:](#builtins)
### `date.add_minutes`

```
date.add_minutes <date> <minutes>
```

#### Description

Adds the specified number of minutes to the input date. 

#### Arguments

- `date`: The date.
- `minutes`: The minutes.

#### Returns

A new date

#### Examples



[:top:](#builtins)
### `date.add_seconds`

```
date.add_seconds <date> <seconds>
```

#### Description

Adds the specified number of seconds to the input date. 

#### Arguments

- `date`: The date.
- `seconds`: The seconds.

#### Returns

A new date

#### Examples



[:top:](#builtins)
### `date.add_milliseconds`

```
date.add_milliseconds <date> <millis>
```

#### Description

Adds the specified number of milliseconds to the input date. 

#### Arguments

- `date`: The date.
- `millis`: The milliseconds.

#### Returns

A new date

#### Examples



[:top:](#builtins)
### `date.parse`

```
date.parse <text>
```

#### Description

Parses the specified input string to a date object. 

#### Arguments

- `text`: A text representing a date.

#### Returns

A date object

#### Examples

```
{{ date.parse '2016/01/05' }}
```
```html
5 Jan 2016
```

[:top:](#builtins)
### `date.to_string`

```
date.to_string <datetime> <pattern> <culture>
```

#### Description

Converts a datetime object to a textual representation using the specified format string.

By default, if you are using a date, it will use the format specified by `date.format` which defaults to /// `date.default_format` (readonly) which default to `%d %b %Y`

You can override the format used for formatting all dates by assigning the a new format: `date.format = '%a /// %b %e %T %Y';`

You can recover the default format by using `date.format = date.default_format;`

By default, the to_string format is using the **current culture**, but you can switch to an invariant /// culture by using the modifier `%g`

For example, using `%g %d %b %Y` will output the date using an invariant culture.

If you are using `%g` alone, it will output the date with `date.format` using an invariant culture.

Suppose that `date.now` would return the date `2013-09-12 22:49:27 +0530`, the following table explains the /// format modifiers:

| Format | Result        | Description
|--------|---------------|--------------------------------------------
| `"%a"` |  `"Thu"` 	   | Name of week day in short form of the
| `"%A"` |  `"Thursday"` | Week day in full form of the time
| `"%b"` |  `"Sep"` 	   | Month in short form of the time
| `"%B"` |  `"September"`| Month in full form of the time
| `"%c"` |               | Date and time (%a %b %e %T %Y)
| `"%d"` |  `"12"` 	     | Day of the month of the time
| `"%e"` |  `"12"`       | Day of the month, blank-padded ( 1..31)
| `"%H"` |  `"22"`       | Hour of the time in 24 hour clock format
| `"%I"` |  `"10"` 	     | Hour of the time in 12 hour clock format
| `"%j"` |               | Day of the year (001..366) (3 digits, left padded with zero)
| `"%m"` |  `"09"` 	     | Month of the time
| `"%M"` |  `"49"` 	     | Minutes of the time (2 digits, left padded with zero e.g 01 02)
| `"%p"` |  `"PM"` 	     | Gives AM / PM of the time
| `"%S"` |  `"27"` 	     | Seconds of the time
| `"%U"` |               | Week number of the current year, starting with the first Sunday as the first day /// of the first week (00..53)
| `"%W"` |               | Week number of the current year, starting with the first Monday as the first day /// of the first week (00..53)
| `"%w"` |  `"4"` 	     | Day of week of the time
| `"%x"` |               | Preferred representation for the date alone, no time
| `"%X"` |               | Preferred representation for the time alone, no date
| `"%y"` |  `"13"` 	     | Gives year without century of the time
| `"%Y"` |  `"2013"`     | Year of the time
| `"%Z"` |  `"IST"` 	   | Gives Time Zone of the time
| `"%%"` |  `"%"`        | Output the character `%`

Note that the format is using a good part of the ruby format ([source]/// (http://apidock.com/ruby/DateTime/strftime))
```scriban-html
{{ date.parse '2016/01/05' | date.to_string `%d %b %Y` }}
```
```html
5 Jan 2016
```

#### Arguments

- `datetime`: The input datetime to format
- `pattern`: The date format pattern.
- `culture`: The culture used to format the datetime

#### Returns

A  that represents this instance.

#### Examples


[:top:](#builtins)

## `html` functions


[:top:](#builtins)
### `html.strip`

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

```scriban-html
{{ "<p>This is a paragraph</p>" | html.strip }}
```
```html
This is a paragraph
```

[:top:](#builtins)
### `html.escape`

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

```scriban-html
{{ "<p>This is a paragraph</p>" | html.escape }}
```
```html
&lt;p&gt;This is a paragraph&lt;/p&gt;
```

[:top:](#builtins)
### `html.url_encode`

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

```scriban-html
{{ "john@liquid.com" | html.url_encode }}
```
```html
john%40liquid.com
```

[:top:](#builtins)
### `html.url_escape`

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

```scriban-html
{{ "<hello> & <scriban>" | html.url_escape }}
```
```html
%3Chello%3E%20&%20%3Cscriban%3E
```
[:top:](#builtins)

## `math` functions


[:top:](#builtins)
### `math.abs`

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

```scriban-html
{{ -15.5| math.abs }}
{{ -5| math.abs }}
```
```html
-15.5
-5
```

[:top:](#builtins)
### `math.ceil`

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

```scriban-html
{{ 4.6 | math.ceil }} 
{{ 4.3 | math.ceil }} 
```
```html
5
5
```

[:top:](#builtins)
### `math.divided_by`

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

```scriban-html
{{ 4.6 | math.divided_by 2.0 }} 
{{ 4.6 | math.divided_by 2 }} 
```
```html
2.3
2
```

[:top:](#builtins)
### `math.floor`

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

```scriban-html
{{ 4.6 | math.ceil }} 
{{ 4.3 | math.ceil }} 
```
```html
4
4
```

[:top:](#builtins)
### `math.format`

```
math.format <value> <format>
```

#### Description

Formats a number value with specified [.NET standard numeric format strings](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)

#### Arguments

- `value`: The input value
- `format`: The format string.

#### Returns

The largest integer less than or equal to the specified number.

#### Examples

```scriban-html
{{ 255 | math.format "X4" }} 
```
```html
00FF
```

[:top:](#builtins)
### `math.is_number`

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

```scriban-html
{{ 255 | math.is_number }} 
{{ "yo" | math.is_number }} 
```
```html
true
false
```

[:top:](#builtins)
### `math.minus`

```
math.minus <value> <with>
```

#### Description

Substracts from the input value the `with` value

#### Arguments

- `value`: The input value
- `with`: The with value to substract from `value`

#### Returns

The results of the substraction: `value` - `with`

#### Examples

```scriban-html
{{ 255 | math.minus 5}} 
```
```html
250
```

[:top:](#builtins)
### `math.modulo`

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

```scriban-html
{{ 11 | math.modulo 10}} 
```
```html
1
```

[:top:](#builtins)
### `math.plus`

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

```scriban-html
{{ 1 | math.plus 2}} 
```
```html
3
```

[:top:](#builtins)
### `math.round`

```
math.round <value> <precision>?=0
```

#### Description

Rounds a value to the nearest integer or to the specified number of fractional digits.

#### Arguments

- `value`: The input value
- `precision`: The number of fractional digits in the return value. Default is 0.

#### Returns

A value rounded to the nearest integer or to the specified number of fractional digits.

#### Examples

```scriban-html
{{ 4.6 | math.round }} 
{{ 4.3 | math.round }}
{{ 4.5612 | math.round 2 }}
```
```html
5
4
4.56
```

[:top:](#builtins)
### `math.times`

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

```scriban-html
{{ 2 | math.times 3}} 
```
```html
6
```
[:top:](#builtins)

## `object` functions


[:top:](#builtins)
### `object.default`

```
object.default <value> <default>
```

#### Description

The `default` value is returned if the input `value` is null or an empty string "". A string containing whitespace characters will not resolve to the default value.

#### Arguments

- `value`: The input value to check if it is null or an empty string.
- `default`: The default alue to return if the input `value` is null or an empty string.

#### Returns

The `default` value is returned if the input `value` is null or an empty string "", otherwise it returns `value`

#### Examples

```scriban-html
{{ undefined_var | object.default "Yo" }}
```
```html
Yo
```

[:top:](#builtins)
### `object.has_key`

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

```scriban-html
{{ product | object.has_key "title" }}
```
```html
true
```

[:top:](#builtins)
### `object.has_value`

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

```scriban-html
{{ product | object.has_value "title" }}
```
```html
true
```

[:top:](#builtins)
### `object.keys`

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

```scriban-html
{{ product | object.keys | array.sort }}
```
```html
[title, type]
```

[:top:](#builtins)
### `object.size`

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

```scriban-html
{{ [1, 2, 3] | object.size }}
```
```html
[title, type]
```

[:top:](#builtins)
### `object.typeof`

```
object.typeof <value>
```

#### Description

Returns string representing the type of the input object. The type can be `string`, `boolean`, `number`, `array`, `iterator` and `object` 

#### Arguments

- `value`: The input object.

#### Returns



#### Examples

```scriban-html
{{ null | object.typeof }}
{{ true | object.typeof }}
{{ 1 | object.typeof }}
{{ 1.0 | object.typeof }}
{{ "text" | object.typeof }}
{{ 1..5 | object.typeof }}
{{ [1,2,3,4,5] | object.typeof }}
{{ {} | object.typeof }}
{{ object | object.typeof }}
```
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

[:top:](#builtins)
### `object.values`

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

```scriban-html
{{ product | object.values | array.sort }}
```
```html
[fruit, Orange]
```
[:top:](#builtins)

## `regex` functions


[:top:](#builtins)
### `regex.escape`

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

```scriban-html
"(abc.*)" | regex.escape }}
```
```html
\\(abc\\.\\*\\)
```

[:top:](#builtins)
### `regex.match`

```
regex.match <text> <pattern> <options>?=
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

```scriban-html
{{ "this is a text123" | regex.match `(\w+) a ([a-z]+\d+)` }}
```
```html
[is a text123, is, text123]
```
Notice that the first element returned in the array is the entire regex match, followed by the regex group matches.

[:top:](#builtins)
### `regex.replace`

```
regex.replace <text> <pattern> <replace> <options>?=
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

```scriban-html
{{ "abbbbcccd | regex.replace "b+c+" "-Yo-" }}
```
```html
a-Yo-d
```

[:top:](#builtins)
### `regex.split`

```
regex.split <text> <pattern> <options>?=
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

```scriban-html
{{ "a, b   , c,    d" | regex.split `\s*,\s*" }}
```
```html
[a, b, c, d]
```

[:top:](#builtins)
### `regex.unescape`

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

```scriban-html
"\\(abc\\.\\*\\)" | regex.unescape }}
```
```html
(abc.*)
```
[:top:](#builtins)

## `string` functions


[:top:](#builtins)
### `string.append`

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

```scriban-html
{{ "Hello" | string.append " World" }}
```
```html
Hello World
```

[:top:](#builtins)
### `string.capitalize`

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

```scriban-html
{{ "test" | string.capitalize }}
```
```html
Test
```

[:top:](#builtins)
### `string.capitalizewords`

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

```scriban-html
{{ "This is easy" | string.capitalizewords }}
```
```html
This Is Easy
```

[:top:](#builtins)
### `string.contains`

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

```scriban-html
{{ "This is easy" | string.contains "easy" }}
```
```html
true
```

[:top:](#builtins)
### `string.downcase`

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

```scriban-html
{{ "TeSt" | string.downcase }}
```
```html
test
```

[:top:](#builtins)
### `string.ends_with`

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

```scriban-html
{{ "This is easy" | string.ends_with "easy" }}
```
```html
true
```

[:top:](#builtins)
### `string.handleize`

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

```scriban-html
{{ '100% M & Ms!!!' | string.handleize  }}
```
```html
100-m-ms
```

[:top:](#builtins)
### `string.lstrip`

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

```scriban-html
{{ '   too many spaces           ' | string.lstrip  }}
```
```html
<!-- Highlight to see the empty spaces to the right of the string -->
too many spaces           
```

[:top:](#builtins)
### `string.pluralize`

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

```scriban-html
{{ cart.item_count }} {{cart.item_count | string.pluralize 'item' 'items' }}
```
```html
4 items
```

[:top:](#builtins)
### `string.prepend`

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

```scriban-html
{{ "World" | string.prepend "Hello " }}
```
```html
Hello World
```

[:top:](#builtins)
### `string.remove`

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

```scriban-html
{{ "Hello, world. Goodbye, world." | string.remove "world" }}
```html
Hello, . Goodbye, .
```

[:top:](#builtins)
### `string.remove_first`

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

```scriban-html
{{ "Hello, world. Goodbye, world." | string.remove_first "world" }}
```html
Hello, . Goodbye, world.
```

[:top:](#builtins)
### `string.replace`

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

```scriban-html
{{ "Hello, world. Goodbye, world." | string.replace "world" "buddy" }}
```html
Hello, buddy. Goodbye, buddy.
```

[:top:](#builtins)
### `string.replace_first`

```
string.replace_first <text> <match> <replace>
```

#### Description

Replaces the first occurrence of a string with a substring.

#### Arguments

- `text`: The input string
- `match`: The substring to find in the `text` string
- `replace`: The substring used to replace the string matched by `match` in the input `text`

#### Returns

The input string replaced

#### Examples

```scriban-html
{{ "Hello, world. Goodbye, world." | string.replace_first "world" "buddy" }}
```html
Hello, buddy. Goodbye, world.
```

[:top:](#builtins)
### `string.rstrip`

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

```scriban-html
{{ '   too many spaces           ' | string.rstrip  }}
```
```html
<!-- Highlight to see the empty spaces to the right of the string -->
   too many spaces
```

[:top:](#builtins)
### `string.size`

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

```scriban-html
{{ "test" | string.size }}
```
```html
4
```

[:top:](#builtins)
### `string.slice`

```
string.slice <text> <start> <length>?=0
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

```scriban-html
{{ "hello" | slice: 0 }}
{{ "hello" | slice: 1 }}
{{ "hello" | slice: 1, 3 }}
```
```html
hello
ello
ell
```

[:top:](#builtins)
### `string.slice1`

```
string.slice1 <text> <start> <length>?=1
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

```scriban-html
{{ "hello" | slice: 0 }}
{{ "hello" | slice: 1 }}
{{ "hello" | slice: 1, 3 }}
```
```html
h
e
ell
```

[:top:](#builtins)
### `string.split`

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

```scriban-html
{{ for word in "Hi, how are you today?" | string.split ' ' 
  word }}
{{ endfor }}
```
```html
Hi,
how
are
you
today?
```

[:top:](#builtins)
### `string.starts_with`

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

```scriban-html
{{ "This is easy" | string.starts_with "This" }}
```
```
true
```

[:top:](#builtins)
### `string.strip`

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

```scriban-html
{{ '   too many spaces           ' | string.strip  }}
```
```html
<!-- Highlight to see the empty spaces to the right of the string -->
too many spaces
```

[:top:](#builtins)
### `string.strip_newlines`

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

```scriban-html
{{ "This is a string.
With another string" | string.strip_newlines  }}
```
```html
This is a string.With another string
```

[:top:](#builtins)
### `string.truncate`

```
string.truncate <text> <length> <ellipsis>?=
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

```scriban-html
{{ "The cat came back the very next day" | string.truncate 13 }}
```
```html
The cat ca...
```

[:top:](#builtins)
### `string.truncatewords`

```
string.truncatewords <text> <count> <ellipsis>?=
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

```scriban-html
{{ "The cat came back the very next day" | string.truncatewords 4 }}
```
```html
The cat came back...
```

[:top:](#builtins)
### `string.upcase`

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

```scriban-html
{{ "test" | string.upcase }}
```
```
TEST
```

[:top:](#builtins)
### `string.md5`

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

```scriban-html
{{ "test" | string.md5 }}
```
```
098f6bcd4621d373cade4e832627b4f6
```

[:top:](#builtins)
### `string.sha1`

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

```scriban-html
{{ "test" | string.sha1 }}
```
```
a94a8fe5ccb19ba61c4c0873d391e987982fbbd3
```

[:top:](#builtins)
### `string.sha256`

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

```scriban-html
{{ "test" | string.sha256 }}
```
```
9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08
```

[:top:](#builtins)
### `string.hmac_sha1`

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

```scriban-html
{{ "test" | string.hmac_sha1 }}
```
```
1aa349585ed7ecbd3b9c486a30067e395ca4b356
```

[:top:](#builtins)
### `string.hmac_sha256`

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

```scriban-html
{{ "test" | string.hmac_sha256 }}
```
```
0329a06b62cd16b33eb6792be8c60b158d89a2ee3a876fce9a881ebb488c0914
```
[:top:](#builtins)

## `timespan` functions


[:top:](#builtins)
### `timespan.from_days`

```
timespan.from_days <days>
```

#### Description

Returns a timespan object that represents a `days` interval

#### Arguments

- `days`: The days.

#### Returns

A timespan object

#### Examples

```
{{ (timespan.from_days 5).days }}
```
```html
5
```

[:top:](#builtins)
### `timespan.from_hours`

```
timespan.from_hours <hours>
```

#### Description

Returns a timespan object that represents a `hours` interval

#### Arguments

- `hours`: The hours.

#### Returns

A timespan object

#### Examples

```
{{ (timespan.from_hours 5).hours }}
```
```html
5
```

[:top:](#builtins)
### `timespan.from_minutes`

```
timespan.from_minutes <minutes>
```

#### Description

Returns a timespan object that represents a `minutes` interval

#### Arguments

- `minutes`: The minutes.

#### Returns

A timespan object

#### Examples

```
{{ (timespan.from_minutes 5).minutes }}
```
```html
5
```

[:top:](#builtins)
### `timespan.from_seconds`

```
timespan.from_seconds <seconds>
```

#### Description

Returns a timespan object that represents a `seconds` interval

#### Arguments

- `seconds`: The seconds.

#### Returns

A timespan object

#### Examples

```
{{ (timespan.from_seconds 5).seconds }}
```
```html
5
```

[:top:](#builtins)
### `timespan.from_milliseconds`

```
timespan.from_milliseconds <millis>
```

#### Description

Returns a timespan object that represents a `milliseconds` interval

#### Arguments

- `millis`: The milliseconds.

#### Returns

A timespan object

#### Examples

```
{{ (timespan.from_milliseconds 5).milliseconds }}
```
```html
5
```

[:top:](#builtins)
### `timespan.parse`

```
timespan.parse <text>
```

#### Description

Parses the specified input string into a timespan object. 

#### Arguments

- `text`: A timespan text

#### Returns

A timespan object parsed from timespan

#### Examples


