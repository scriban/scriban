---
title: "Date functions"
---

# `date` functions

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

The subtract operation `date1 - date2`: Subtract `date2` from `date1` and return a timespan internal object (see timespan object below).

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
- [`date.parse_to_string`](#dateparse_to_string)
- [`date.to_string`](#dateto_string)

## `date.now`

```
date.now
```

#### Description

Returns a datetime object of the current time, including the hour, minutes, seconds and milliseconds.

#### Arguments


#### Returns



#### Examples

> **input** [Try out](/?template=%7B%7B%20date.now.year%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.now.year {{ "}}" }}
```
> **output**
```html
2026
```

## `date.add_days`

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

> **input** [Try out](/?template=%7B%7B%20date.parse%20%272016%2F01%2F05%27%20%7C%20date.add_days%201%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.parse '2016/01/05' | date.add_days 1 {{ "}}" }}
```
> **output**
```html
06 Jan 2016
```

## `date.add_months`

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

> **input** [Try out](/?template=%7B%7B%20date.parse%20%272016%2F01%2F05%27%20%7C%20date.add_months%201%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.parse '2016/01/05' | date.add_months 1 {{ "}}" }}
```
> **output**
```html
05 Feb 2016
```

## `date.add_years`

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

> **input** [Try out](/?template=%7B%7B%20date.parse%20%272016%2F01%2F05%27%20%7C%20date.add_years%201%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.parse '2016/01/05' | date.add_years 1 {{ "}}" }}
```
> **output**
```html
05 Jan 2017
```

## `date.add_hours`

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



## `date.add_minutes`

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



## `date.add_seconds`

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



## `date.add_milliseconds`

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



## `date.parse`

```
date.parse <text> <pattern>? <culture>?
```

#### Description

Parses the specified input string to a date object.

#### Arguments

- `text`: A text representing a date.
- `pattern`: The date format pattern. See `to_string` method about the format of a pattern.
- `culture`: The culture used to format the datetime. Default is current culture.

#### Returns

A date object

#### Examples

> **input** [Try out](/?template=%7B%7B%20date.parse%20%272016%2F01%2F05%27%20%7D%7D%0A%7B%7B%20date.parse%20%272018--06--17%27%20%27%25Y--%25m--%25d%27%20%7D%7D%0A%7B%7B%20date.parse%20%272021%2F11%2F30%2020%3A50%3A23Z%27%20%7D%7D%0A%7B%7B%20date.parse%20%2720%2F01%2F2022%2008%3A32%3A48%20%2B00%3A00%27%20culture%3A%27en-GB%27%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.parse '2016/01/05' {{ "}}" }}
{{ "{{" }} date.parse '2018--06--17' '%Y--%m--%d' {{ "}}" }}
{{ "{{" }} date.parse '2021/11/30 20:50:23Z' {{ "}}" }}
{{ "{{" }} date.parse '20/01/2022 08:32:48 +00:00' culture:'en-GB' {{ "}}" }}
```
> **output**
```html
05 Jan 2016
17 Jun 2018
30 Nov 2021
20 Jan 2022
```

## `date.parse_to_string`

```
date.parse_to_string <text> <output_pattern>? <output_culture>? <input_pattern>? <input_culture>?
```

#### Description

Parses the specified input string to a formatted date string.

#### Arguments

- `text`: A text representing a date.
- `output_pattern`: The output date format pattern. See `to_string` method about the format of a pattern.
- `output_culture`: The culture used to format the datetime. Default is current culture.
- `input_pattern`: The input date format pattern. See `to_string` method about the format of a pattern.
- `input_culture`: The culture used to parse the input datetime. Default is current culture.

#### Returns

A formatted date string

#### Examples

> **input** [Try out](/?template=%7B%7B%20date.parse_to_string%20%272016%2F01%2F05%27%20%27%25Y--%25m--%25d%27%20%7D%7D%0A%7B%7B%20%2203%2014%2C%202016%22%20%7C%20date.parse_to_string%20%22%25b%20%25d%2C%20%25y%22%20input_pattern%3A%20%22%25m%20%25d%2C%20%25Y%22%20%7D%7D%0A%7B%7B%20%222025-01-01%2014%3A01%3A23%22%20%7C%20date.parse_to_string%20%22%25r%22%20input_pattern%3A%20%22%25Y-%25m-%25d%20%25k%3A%25M%3A%25S%22%20%7D%7D%0A%7B%7B%20%2203%2F01%2F2025%2014%3A01%3A23%22%20%7C%20date.parse_to_string%20%22%25F%22%20input_culture%3A%27en-US%27%20output_culture%3A%27en-GB%27%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.parse_to_string '2016/01/05' '%Y--%m--%d' {{ "}}" }}
{{ "{{" }} "03 14, 2016" | date.parse_to_string "%b %d, %y" input_pattern: "%m %d, %Y" {{ "}}" }}
{{ "{{" }} "2025-01-01 14:01:23" | date.parse_to_string "%r" input_pattern: "%Y-%m-%d %k:%M:%S" {{ "}}" }}
{{ "{{" }} "03/01/2025 14:01:23" | date.parse_to_string "%F" input_culture:'en-US' output_culture:'en-GB' {{ "}}" }}
```
> **output**
```html
2016--01--05
Mar 14, 16
02:01:23 PM
2025-03-01
```

## `date.to_string`

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
| `"%Z"` |  `"+05:30"`       | Gives Time Zone of the time
| `"%%"` |  `"%"`            | Output the character `%`

Note that the format is using a good part of the ruby format ([source](http://apidock.com/ruby/DateTime/strftime))
> **input** [Try out](/?template=%7B%7B%20date.parse%20%272016%2F01%2F05%27%20%7C%20date.to_string%20%27%25d%20%25b%20%25Y%27%20%7D%7D%0A%7B%7B%20date.parse%20%272016%2F01%2F05%27%20%7C%20date.to_string%20%27%25d%20%25B%20%25Y%27%20%27fr-FR%27%20%7D%7D&model=%7B%7D)
```scriban-html
{{ "{{" }} date.parse '2016/01/05' | date.to_string '%d %b %Y' {{ "}}" }}
{{ "{{" }} date.parse '2016/01/05' | date.to_string '%d %B %Y' 'fr-FR' {{ "}}" }}
```
> **output**
```html
05 Jan 2016
05 janvier 2016
```

#### Arguments

- `datetime`: The input datetime to format
- `pattern`: The date format pattern.
- `culture`: The culture used to format the datetime

#### Returns

A  that represents this instance.

#### Examples



> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
