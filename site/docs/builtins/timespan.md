---
title: "Timespan functions"
---

# `timespan` functions

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

- [`timespan.from_days`](timespan#timespan.from_days)
- [`timespan.from_hours`](timespan#timespan.from_hours)
- [`timespan.from_minutes`](timespan#timespan.from_minutes)
- [`timespan.from_seconds`](timespan#timespan.from_seconds)
- [`timespan.from_milliseconds`](timespan#timespan.from_milliseconds)
- [`timespan.parse`](timespan#timespan.parse)

## `timespan.from_days`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%28timespan.from_days%205%29.days%20%7D%7D&model={})
```scriban-html
{{ "{{" }} (timespan.from_days 5).days {{ "}}" }}
```
> **output**
```html
5
```

## `timespan.from_hours`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%28timespan.from_hours%205%29.hours%20%7D%7D&model={})
```scriban-html
{{ "{{" }} (timespan.from_hours 5).hours {{ "}}" }}
```
> **output**
```html
5
```

## `timespan.from_minutes`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%28timespan.from_minutes%205%29.minutes%20%7D%7D&model={})
```scriban-html
{{ "{{" }} (timespan.from_minutes 5).minutes {{ "}}" }}
```
> **output**
```html
5
```

## `timespan.from_seconds`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%28timespan.from_seconds%205%29.seconds%20%7D%7D&model={})
```scriban-html
{{ "{{" }} (timespan.from_seconds 5).seconds {{ "}}" }}
```
> **output**
```html
5
```

## `timespan.from_milliseconds`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%28timespan.from_milliseconds%205%29.milliseconds%20%7D%7D&model={})
```scriban-html
{{ "{{" }} (timespan.from_milliseconds 5).milliseconds {{ "}}" }}
```
> **output**
```html
5
```

## `timespan.parse`

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



> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
