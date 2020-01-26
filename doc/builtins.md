# Builtins

This document describes the various built-in functions available in scriban.


- [`array` functions](#array-functions)
- [`date` functions](#date-functions)
- [`html` functions](#html-functions)
- [`math` functions](#math-functions)
- [`object` functions](#object-functions)
- [`regex` functions](#regex-functions)
- [`string` functions](#string-functions)
- [`timespan` functions](#timespan-functions)

[:top:](#builtins)

## `array` functions

Array functions available through the object 'array' in scriban.

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
- [`array.contains`](#arraycontains)

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

> **input**
```scriban-html
{{ [1, 2, 3] | array.add 4 }}
```
> **output**
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

> **input**
```scriban-html
{{ [1, 2, 3] | array.concat [4, 5] }}
```
> **output**
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

> **input**
```scriban-html
{{ [1, null, 3] | array.compact }}
```
> **output**
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

> **input**
```scriban-html
{{ [1, 2, 3] | array.concat [4, 5] }}
```
> **output**
```html
[1, 2, 3, 4, 5]
```

[:top:](#builtins)
### `array.cycle`

```
array.cycle <list> <group>?
```

#### Description

Loops through a group of strings and outputs them in the order that they were passed as parameters. Each time cycle is called, the next string that was passed as a parameter is output.

#### Arguments

- `list`: An input list
- `group`: The group used. Default is `null`

#### Returns

Returns a list with null value removed

#### Examples

> **input**
```scriban-html
{{ array.cycle ['one', 'two', 'three'] }}
{{ array.cycle ['one', 'two', 'three'] }}
{{ array.cycle ['one', 'two', 'three'] }}
{{ array.cycle ['one', 'two', 'three'] }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6] | array.first }}
```
> **output**
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

> **input**
```scriban-html
{{ ["a", "b", "c"] | array.insert_at 2 "Yo" }}
```
> **output**
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

> **input**
```scriban-html
{{ [1, 2, 3] | array.join "|" }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6] | array.last }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6] | array.limit 2 }}
```
> **output**
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

> **input**
```scriban-html
{{ 
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.map "type" | array.uniq | array.sort }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6, 7, 8] | array.offset 2 }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6, 7, 8] | array.remove_at 2 }}
```
> **output**
```html
[4, 5, 7, 8]
```
If the `index` is negative, removes at the end of the list (notice that we need to put -1 in parenthesis to avoid confusing the parser with a binary `-` operation):
> **input**
```scriban-html
{{ [4, 5, 6, 7, 8] | array.remove_at (-1) }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6, 7] | array.reverse }}
```
> **output**
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

> **input**
```scriban-html
{{ [4, 5, 6] | array.size }}
```
> **output**
```html
3
```

[:top:](#builtins)
### `array.sort`

```
array.sort <list> <member>?
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
> **input**
```scriban-html
{{ [10, 2, 6] | array.sort }}
```
> **output**
```html
[2, 6, 10]
```
Sorts by elements member's value: 
> **input**
```scriban-html
{{
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.sort "title" | array.map "title"
}}
```
> **output**
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

> **input**
```scriban-html
{{ [1, 1, 4, 5, 8, 8] | array.uniq }}
```
> **output**
```html
[1, 4, 5, 8]
```

[:top:](#builtins)
### `array.contains`

```
array.contains <list> <item>
```

#### Description

Returns if an `list` contains an specifique element 

#### Arguments

- `list`: the input list
- `item`: the input item

#### Returns

**true** if element is in `list`; otherwise **false**

#### Examples

> **input**
```scriban-html
{{ [1, 2, 3, 4] | array.contains 4 }}
```
> **output**
```html
true
```
[:top:](#builtins)

## `date` functions

A datetime object represents an instant in time, expressed as a date and time of day. 

| Name             | Description
|--------------    |-----------------
| `.year`          | Gets the year of a date object 
| `.month`         | Gets the month of a date object
| `.day`           | Gets the day in the month of a date object
| `.day_of_year`   | Gets the day within the year
| `.hour`          | Gets the hour of the date object
| `.minute`        | Gets the minute of the date object
| `.second`        | Gets the second of the date object
| `.millisecond`   | Gets the millisecond of the date object

[:top:](#builtins)
#### Binary operations

The substract operation `date1 - date2`: Substract `date2` from `date1` and return a timespan internal object (see timespan object below).

Other comparison operators(`==`, `!=`, `<=`, `>=`, `<`, `>`) are also working with date objects.

A `timespan` and also the added to a `datetime` object.

- [`date.now`](#datenow)
- [`date.add_days`](#dateadd_days)
- [`date.add_months`](#dateadd_months)
- [`date.add_years`](#dateadd_years)
- [`date.add_hours`](#dateadd_hours)
- [`date.add_minutes`](#dateadd_minutes)
- [`date.add_seconds`](#dateadd_seconds)
- [`date.add_milliseconds`](#dateadd_milliseconds)
- [`date.parse`](#dateparse)
- [`date.to_string`](#dateto_string)

[:top:](#builtins)
### `date.now`

```
date.now
```

#### Description

Returns a datetime object of the current time, including the hour, minutes, seconds and milliseconds.

#### Arguments


#### Returns



#### Examples

> **input**
```scriban-html
{{ date.now.year }}
```
> **output**
```html
2020
```

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

> **input**
```scriban-html
{{ date.parse '2016/01/05' | date.add_days 1 }}
```
> **output**
```html
06 Jan 2016
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

> **input**
```scriban-html
{{ date.parse '2016/01/05' | date.add_months 1 }}
```
> **output**
```html
05 Feb 2016
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

> **input**
```scriban-html
{{ date.parse '2016/01/05' | date.add_years 1 }}
```
> **output**
```html
05 Jan 2017
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

> **input**
```scriban-html
{{ date.parse '2016/01/05' }}
```
> **output**
```html
05 Jan 2016
```

[:top:](#builtins)
### `date.to_string`

```
date.to_string <datetime> <pattern> <culture>
```

#### Description

Converts a datetime object to a textual representation using the specified format string.

By default, if you are using a date, it will use the format specified by `date.format` which defaults to `date.default_format` (readonly) which default to `%d %b %Y`

You can override the format used for formatting all dates by assigning the a new format: `date.format = '%a %b %e %T %Y';`

You can recover the default format by using `date.format = date.default_format;`

By default, the to_string format is using the **current culture**, but you can switch to an invariant culture by using the modifier `%g`

For example, using `%g %d %b %Y` will output the date using an invariant culture.

If you are using `%g` alone, it will output the date with `date.format` using an invariant culture.

Suppose that `date.now` would return the date `2013-09-12 22:49:27 +0530`, the following table explains the format modifiers:

| Format | Result            | Description
|--------|-------------------|--------------------------------------------
| `"%a"` |  `"Thu"`          | Name of week day in short form of the
| `"%A"` |  `"Thursday"`     | Week day in full form of the time
| `"%b"` |  `"Sep"`          | Month in short form of the time
| `"%B"` |  `"September"`    | Month in full form of the time
| `"%c"` |                   | Date and time (%a %b %e %T %Y)
| `"%C"` |  `"20"`           | Century of the time
| `"%d"` |  `"12"`           | Day of the month of the time
| `"%D"` |  `"09/12/13"`     | Date (%m/%d/%y)
| `"%e"` |  `"12"`           | Day of the month, blank-padded ( 1..31)
| `"%F"` |  `"2013-09-12"`   | ISO 8601 date (%Y-%m-%d)
| `"%h"` |  `"Sep"`          | Alias for %b
| `"%H"` |  `"22"`           | Hour of the time in 24 hour clock format
| `"%I"` |  `"10"`           | Hour of the time in 12 hour clock format
| `"%j"` |  `"255"`          | Day of the year (001..366) (3 digits, left padded with zero)
| `"%k"` |  `"22"`           | Hour of the time in 24 hour clock format, blank-padded ( 0..23)
| `"%l"` |  `"10"`           | Hour of the time in 12 hour clock format, blank-padded ( 0..12)
| `"%L"` |  `"000"`          | Millisecond of the time (3 digits, left padded with zero)
| `"%m"` |  `"09"`           | Month of the time
| `"%M"` |  `"49"`           | Minutes of the time (2 digits, left padded with zero e.g 01 02)
| `"%n"` |                   | Newline character (\n)
| `"%N"` |  `"000000000"`    | Nanoseconds of the time (9 digits, left padded with zero)
| `"%p"` |  `"PM"`           | Gives AM / PM of the time
| `"%P"` |  `"pm"`           | Gives am / pm of the time
| `"%r"` |  `"10:49:27 PM"`  | Long time in 12 hour clock format (%I:%M:%S %p)
| `"%R"` |  `"22:49"`        | Short time in 24 hour clock format (%H:%M)
| `"%s"` |                   | Number of seconds since 1970-01-01 00:00:00 +0000
| `"%S"` |  `"27"`           | Seconds of the time
| `"%t"` |                   | Tab character (\t)
| `"%T"` |  `"22:49:27"`     | Long time in 24 hour clock format (%H:%M:%S)
| `"%u"` |  `"4"`            | Day of week of the time (from 1 for Monday to 7 for Sunday)
| `"%U"` |  `"36"`           | Week number of the current year, starting with the first Sunday as the first day of the first week (00..53)
| `"%v"` |  `"12-SEP-2013"`  | VMS date (%e-%b-%Y) (culture invariant)
| `"%V"` |  `"37"`           | Week number of the current year according to ISO 8601 (01..53)
| `"%W"` |  `"36"`           | Week number of the current year, starting with the first Monday as the first day of the first week (00..53)
| `"%w"` |  `"4"`            | Day of week of the time (from 0 for Sunday to 6 for Saturday)
| `"%x"` |                   | Preferred representation for the date alone, no time
| `"%X"` |                   | Preferred representation for the time alone, no date
| `"%y"` |  `"13"`           | Gives year without century of the time
| `"%Y"` |  `"2013"`         | Year of the time
| `"%Z"` |  `"IST"`          | Gives Time Zone of the time
| `"%%"` |  `"%"`            | Output the character `%`

Note that the format is using a good part of the ruby format ([source](http://apidock.com/ruby/DateTime/strftime))
> **input**
```scriban-html
{{ date.parse '2016/01/05' | date.to_string `%d %b %Y` }}
```
> **output**
```html
05 Jan 2016
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

Html functions available through the builtin object 'html'.

- [`html.strip`](#htmlstrip)
- [`html.escape`](#htmlescape)
- [`html.url_encode`](#htmlurl_encode)
- [`html.url_escape`](#htmlurl_escape)

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

> **input**
```scriban-html
{{ "<p>This is a paragraph</p>" | html.strip }}
```
> **output**
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

> **input**
```scriban-html
{{ "<p>This is a paragraph</p>" | html.escape }}
```
> **output**
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

> **input**
```scriban-html
{{ "john@liquid.com" | html.url_encode }}
```
> **output**
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

> **input**
```scriban-html
{{ "<hello> & <scriban>" | html.url_escape }}
```
> **output**
```html
%3Chello%3E%20&%20%3Cscriban%3E
```
[:top:](#builtins)

## `math` functions

Math functions available through the object 'math' in scriban.

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

> **input**
```scriban-html
{{ -15.5| math.abs }}
{{ -5| math.abs }}
```
> **output**
```html
15.5
5
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

> **input**
```scriban-html
{{ 4.6 | math.ceil }}
{{ 4.3 | math.ceil }}
```
> **output**
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

> **input**
```scriban-html
{{ 8.4 | math.divided_by 2.0 | math.round 1 }}
{{ 8.4 | math.divided_by 2 }}
```
> **output**
```html
4.2
4
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

> **input**
```scriban-html
{{ 4.6 | math.floor }}
{{ 4.3 | math.floor }}
```
> **output**
```html
4
4
```

[:top:](#builtins)
### `math.format`

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

> **input**
```scriban-html
{{ 255 | math.format "X4" }}
```
> **output**
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

> **input**
```scriban-html
{{ 255 | math.is_number }}
{{ "yo" | math.is_number }}
```
> **output**
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

> **input**
```scriban-html
{{ 255 | math.minus 5}}
```
> **output**
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

> **input**
```scriban-html
{{ 11 | math.modulo 10}}
```
> **output**
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

> **input**
```scriban-html
{{ 1 | math.plus 2}}
```
> **output**
```html
3
```

[:top:](#builtins)
### `math.round`

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

> **input**
```scriban-html
{{ 4.6 | math.round }}
{{ 4.3 | math.round }}
{{ 4.5612 | math.round 2 }}
```
> **output**
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

> **input**
```scriban-html
{{ 2 | math.times 3}}
```
> **output**
```html
6
```
[:top:](#builtins)

## `object` functions

Object functions available through the builtin object 'object'.

- [`object.default`](#objectdefault)
- [`object.format`](#objectformat)
- [`object.has_key`](#objecthas_key)
- [`object.has_value`](#objecthas_value)
- [`object.keys`](#objectkeys)
- [`object.size`](#objectsize)
- [`object.typeof`](#objecttypeof)
- [`object.values`](#objectvalues)

[:top:](#builtins)
### `object.default`

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

> **input**
```scriban-html
{{ undefined_var | object.default "Yo" }}
```
> **output**
```html
Yo
```

[:top:](#builtins)
### `object.format`

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

> **input**
```scriban-html
{{ 255 | object.format "X4" }}
{{ 1523 | object.format "N" "fr-FR" }}
```
> **output**
```html
00FF
1 523,00
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

> **input**
```scriban-html
{{ product | object.has_key "title" }}
```
> **output**
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

> **input**
```scriban-html
{{ product | object.has_value "title" }}
```
> **output**
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

> **input**
```scriban-html
{{ product | object.keys | array.sort }}
```
> **output**
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

> **input**
```scriban-html
{{ [1, 2, 3] | object.size }}
```
> **output**
```html
3
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

> **input**
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

> **input**
```scriban-html
{{ product | object.values | array.sort }}
```
> **output**
```html
[fruit, Orange]
```
[:top:](#builtins)

## `regex` functions

Functions exposed through `regex` builtin object.

- [`regex.escape`](#regexescape)
- [`regex.match`](#regexmatch)
- [`regex.replace`](#regexreplace)
- [`regex.split`](#regexsplit)
- [`regex.unescape`](#regexunescape)

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

> **input**
```scriban-html
{{ "(abc.*)" | regex.escape }}
```
> **output**
```html
\(abc\.\*\)
```

[:top:](#builtins)
### `regex.match`

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

> **input**
```scriban-html
{{ "this is a text123" | regex.match `(\w+) a ([a-z]+\d+)` }}
```
> **output**
```html
[is a text123, is, text123]
```
Notice that the first element returned in the array is the entire regex match, followed by the regex group matches.

[:top:](#builtins)
### `regex.replace`

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

> **input**
```scriban-html
{{ "abbbbcccd" | regex.replace "b+c+" "-Yo-" }}
```
> **output**
```html
a-Yo-d
```

[:top:](#builtins)
### `regex.split`

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

> **input**
```scriban-html
{{ "a, b   , c,    d" | regex.split `\s*,\s*` }}
```
> **output**
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

> **input**
```scriban-html
{{ "\\(abc\\.\\*\\)" | regex.unescape }}
```
> **output**
```html
(abc.*)
```
[:top:](#builtins)

## `string` functions

String functions available through the builtin object 'string`.

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
- [`string.to_int`](#stringto_int)
- [`string.to_long`](#stringto_long)
- [`string.to_float`](#stringto_float)
- [`string.to_double`](#stringto_double)
- [`string.truncate`](#stringtruncate)
- [`string.truncatewords`](#stringtruncatewords)
- [`string.upcase`](#stringupcase)
- [`string.md5`](#stringmd5)
- [`string.sha1`](#stringsha1)
- [`string.sha256`](#stringsha256)
- [`string.hmac_sha1`](#stringhmac_sha1)
- [`string.hmac_sha256`](#stringhmac_sha256)
- [`string.pad_left`](#stringpad_left)
- [`string.pad_right`](#stringpad_right)
- [`string.base64_encode`](#stringbase64_encode)
- [`string.base64_decode`](#stringbase64_decode)

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

> **input**
```scriban-html
{{ "Hello" | string.append " World" }}
```
> **output**
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

> **input**
```scriban-html
{{ "test" | string.capitalize }}
```
> **output**
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

> **input**
```scriban-html
{{ "This is easy" | string.capitalizewords }}
```
> **output**
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

> **input**
```scriban-html
{{ "This is easy" | string.contains "easy" }}
```
> **output**
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

> **input**
```scriban-html
{{ "TeSt" | string.downcase }}
```
> **output**
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

> **input**
```scriban-html
{{ "This is easy" | string.ends_with "easy" }}
```
> **output**
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

> **input**
```scriban-html
{{ '100% M & Ms!!!' | string.handleize  }}
```
> **output**
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

> **input**
```scriban-html
{{ '   too many spaces           ' | string.lstrip  }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
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

> **input**
```scriban-html
{{ products.size }} {{products.size | string.pluralize 'product' 'products' }}
```
> **output**
```html
7 products
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

> **input**
```scriban-html
{{ "World" | string.prepend "Hello " }}
```
> **output**
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

> **input**
```scriban-html
{{ "Hello, world. Goodbye, world." | string.remove "world" }}
```
> **output**
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

> **input**
```scriban-html
{{ "Hello, world. Goodbye, world." | string.remove_first "world" }}
```
> **output**
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

> **input**
```scriban-html
{{ "Hello, world. Goodbye, world." | string.replace "world" "buddy" }}
```
> **output**
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

> **input**
```scriban-html
{{ "Hello, world. Goodbye, world." | string.replace_first "world" "buddy" }}
```
> **output**
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

> **input**
```scriban-html
{{ '   too many spaces           ' | string.rstrip  }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
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

> **input**
```scriban-html
{{ "test" | string.size }}
```
> **output**
```html
4
```

[:top:](#builtins)
### `string.slice`

```
string.slice <text> <start> <length: 0>?
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

> **input**
```scriban-html
{{ "hello" | string.slice 0 }}
{{ "hello" | string.slice 1 }}
{{ "hello" | string.slice 1 3 }}
{{ "hello" | string.slice 1 length:3 }}
```
> **output**
```html
hello
ello
ell
ell
```

[:top:](#builtins)
### `string.slice1`

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

> **input**
```scriban-html
{{ "hello" | string.slice1 0 }}
{{ "hello" | string.slice1 1 }}
{{ "hello" | string.slice1 1 3 }}
{{ "hello" | string.slice1 1 length: 3 }}
```
> **output**
```html
h
e
ell
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

> **input**
```scriban-html
{{ for word in "Hi, how are you today?" | string.split ' ' ~}}
{{ word }}
{{ end ~}}
```
> **output**
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

> **input**
```scriban-html
{{ "This is easy" | string.starts_with "This" }}
```
> **output**
```html
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

> **input**
```scriban-html
{{ '   too many spaces           ' | string.strip  }}
```
> Highlight to see the empty spaces to the right of the string
> **output**
```html
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

> **input**
```scriban-html
{{ "This is a string.\r\n With \nanother \rstring" | string.strip_newlines  }}
```
> **output**
```html
This is a string. With another string
```

[:top:](#builtins)
### `string.to_int`

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

> **input**
```scriban-html
{{ "123" | string.to_int + 1 }}
```
> **output**
```html
124
```

[:top:](#builtins)
### `string.to_long`

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

> **input**
```scriban-html
{{ "123678912345678" | string.to_long + 1 }}
```
> **output**
```html
123678912345679
```

[:top:](#builtins)
### `string.to_float`

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

> **input**
```scriban-html
{{ "123.4" | string.to_float + 1 }}
```
> **output**
```html
124.4
```

[:top:](#builtins)
### `string.to_double`

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

> **input**
```scriban-html
{{ "123.4" | string.to_double + 1 }}
```
> **output**
```html
124.4
```

[:top:](#builtins)
### `string.truncate`

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

> **input**
```scriban-html
{{ "The cat came back the very next day" | string.truncate 13 }}
```
> **output**
```html
The cat ca...
```

[:top:](#builtins)
### `string.truncatewords`

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

> **input**
```scriban-html
{{ "The cat came back the very next day" | string.truncatewords 4 }}
```
> **output**
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

> **input**
```scriban-html
{{ "test" | string.upcase }}
```
> **output**
```html
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

> **input**
```scriban-html
{{ "test" | string.md5 }}
```
> **output**
```html
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

> **input**
```scriban-html
{{ "test" | string.sha1 }}
```
> **output**
```html
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

> **input**
```scriban-html
{{ "test" | string.sha256 }}
```
> **output**
```html
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

> **input**
```scriban-html
{{ "test" | string.hmac_sha1 "secret" }}
```
> **output**
```html
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

> **input**
```scriban-html
{{ "test" | string.hmac_sha256 "secret" }}
```
> **output**
```html
0329a06b62cd16b33eb6792be8c60b158d89a2ee3a876fce9a881ebb488c0914
```

[:top:](#builtins)
### `string.pad_left`

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

> **input**
```scriban-html
hello{{ "world" | string.pad_left 10 }}
```
> **output**
```html
hello     world
```

[:top:](#builtins)
### `string.pad_right`

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

> **input**
```scriban-html
{{ "hello" | string.pad_right 10 }}world
```
> **output**
```html
hello     world
```

[:top:](#builtins)
### `string.base64_encode`

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

> **input**
```scriban-html
{{ "hello" | string.base64_encode }}
```
> **output**
```html
aGVsbG8=
```

[:top:](#builtins)
### `string.base64_decode`

```
string.base64_decode <text>
```

#### Description

Decodes a Base64-encoded string to a byte array.
he encoding of the bytes is assumed to be UTF-8.

#### Arguments

- `text`: The string to decode

#### Returns

The decoded string

#### Examples

> **input**
```scriban-html
{{ "aGVsbG8=" | string.base64_decode }}
```
> **output**
```html
hello
```
[:top:](#builtins)

## `timespan` functions

A timespan object represents a time interval.

| Name             | Description
|--------------    |-----------------
| `.days`          | Gets the number of days of this interval 
| `.hours`         | Gets the number of hours of this interval
| `.minutes`       | Gets the number of minutes of this interval
| `.seconds`       | Gets the number of seconds of this interval
| `.milliseconds`  | Gets the number of milliseconds of this interval 
| `.total_days`    | Gets the total number of days in fractional part
| `.total_hours`   | Gets the total number of hours in fractional part
| `.total_minutes` | Gets the total number of minutes in fractional part
| `.total_seconds` | Gets the total number of seconds  in fractional part
| `.total_milliseconds` | Gets the total number of milliseconds  in fractional part

- [`timespan.from_days`](#timespanfrom_days)
- [`timespan.from_hours`](#timespanfrom_hours)
- [`timespan.from_minutes`](#timespanfrom_minutes)
- [`timespan.from_seconds`](#timespanfrom_seconds)
- [`timespan.from_milliseconds`](#timespanfrom_milliseconds)
- [`timespan.parse`](#timespanparse)

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

> **input**
```scriban-html
{{ (timespan.from_days 5).days }}
```
> **output**
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

> **input**
```scriban-html
{{ (timespan.from_hours 5).hours }}
```
> **output**
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

> **input**
```scriban-html
{{ (timespan.from_minutes 5).minutes }}
```
> **output**
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

> **input**
```scriban-html
{{ (timespan.from_seconds 5).seconds }}
```
> **output**
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

> **input**
```scriban-html
{{ (timespan.from_milliseconds 5).milliseconds }}
```
> **output**
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



> Note: This document was automatically generated from the sourcecode using `Scriban.DocGen` program
