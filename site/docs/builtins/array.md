---
title: "Array functions"
---

# `array` functions

Array functions available through the object 'array' in scriban.

- [`array.add`](#arrayadd)
- [`array.add_range`](#arrayadd_range)
- [`array.compact`](#arraycompact)
- [`array.concat`](#arrayconcat)
- [`array.cycle`](#arraycycle)
- [`array.any`](#arrayany)
- [`array.each`](#arrayeach)
- [`array.filter`](#arrayfilter)
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

## `array.add`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%202%2C%203%5D%20%7C%20array.add%204%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 2, 3] | array.add 4 {{ "}}" }}
```
> **output**
```html
[1, 2, 3, 4]
```

## `array.add_range`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%202%2C%203%5D%20%7C%20array.add_range%20%5B4%2C%205%5D%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 2, 3] | array.add_range [4, 5] {{ "}}" }}
```
> **output**
```html
[1, 2, 3, 4, 5]
```

## `array.compact`

```
array.compact <list>
```

#### Description

Removes any null values from the input list.

#### Arguments

- `list`: An input list

#### Returns

Returns a list with null value removed

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%20null%2C%203%5D%20%7C%20array.compact%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, null, 3] | array.compact {{ "}}" }}
```
> **output**
```html
[1, 3]
```

## `array.concat`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%202%2C%203%5D%20%7C%20array.concat%20%5B4%2C%205%5D%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 2, 3] | array.concat [4, 5] {{ "}}" }}
```
> **output**
```html
[1, 2, 3, 4, 5]
```

## `array.cycle`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20array.cycle%20%5B%27one%27%2C%20%27two%27%2C%20%27three%27%5D%20%7D%7D%0A%7B%7B%20array.cycle%20%5B%27one%27%2C%20%27two%27%2C%20%27three%27%5D%20%7D%7D%0A%7B%7B%20array.cycle%20%5B%27one%27%2C%20%27two%27%2C%20%27three%27%5D%20%7D%7D%0A%7B%7B%20array.cycle%20%5B%27one%27%2C%20%27two%27%2C%20%27three%27%5D%20%7D%7D&model={})
```scriban-html
{{ "{{" }} array.cycle ['one', 'two', 'three'] {{ "}}" }}
{{ "{{" }} array.cycle ['one', 'two', 'three'] {{ "}}" }}
{{ "{{" }} array.cycle ['one', 'two', 'three'] {{ "}}" }}
{{ "{{" }} array.cycle ['one', 'two', 'three'] {{ "}}" }}
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

## `array.any`

```
array.any <list> <function> <args>
```

#### Description

Returns the distinct elements of the input `list`.

#### Arguments

- `list`: An input list
- `function`: The function to apply to each item in the list that returns a boolean.
- `args`: The arguments to pass to the function

#### Returns

A boolean indicating if one of the item in the list satisfied the function.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B%22%20hello%22%2C%20%22%20world%22%2C%20%2220%22%5D%20%7C%20array.any%20%40string.contains%20%2220%22%7D%7D%0A%7B%7B%20%5B%22%20hello%22%2C%20%22%20world%22%2C%20%2220%22%5D%20%7C%20array.any%20%40string.contains%20%2230%22%7D%7D&model={})
```scriban-html
{{ "{{" }} [" hello", " world", "20"] | array.any @string.contains "20"{{ "}}" }}
{{ "{{" }} [" hello", " world", "20"] | array.any @string.contains "30"{{ "}}" }}
```
> **output**
```html
true
false
```

## `array.each`

```
array.each <list> <function>
```

#### Description

Applies the specified function to each element of the input.

#### Arguments

- `list`: An input list
- `function`: The function to apply to each item in the list

#### Returns

Returns a list with each item being transformed by the function.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B%22%20a%22%2C%20%22%205%22%2C%20%226%20%22%5D%20%7C%20array.each%20%40string.strip%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [" a", " 5", "6 "] | array.each @string.strip {{ "}}" }}
```
> **output**
```html
["a", "5", "6"]
```

## `array.filter`

```
array.filter <list> <function>
```

#### Description

Filters the input list according the supplied filter function.

#### Arguments

- `list`: An input list
- `function`: The function used to test each elemement of the list

#### Returns

Returns a new list which contains only those elements which match the filter function.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%5B%22%22%2C%20%22200%22%2C%20%22%22%2C%22400%22%5D%20%7C%20array.filter%20%40string.empty%7D%7D&model={})
```scriban-html
{{ "{{" }}["", "200", "","400"] | array.filter @string.empty{{ "}}" }}
```
> **output**
```html
["", ""]
```

## `array.first`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%5D%20%7C%20array.first%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6] | array.first {{ "}}" }}
```
> **output**
```html
4
```

## `array.insert_at`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B%22a%22%2C%20%22b%22%2C%20%22c%22%5D%20%7C%20array.insert_at%202%20%22Yo%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} ["a", "b", "c"] | array.insert_at 2 "Yo" {{ "}}" }}
```
> **output**
```html
["a", "b", "Yo", "c"]
```

## `array.join`

```
array.join <list> <delimiter> <function>?
```

#### Description

Joins the element of a list separated by a delimiter string and return the concatenated string.

#### Arguments

- `list`: The input list
- `delimiter`: The delimiter string to use to separate elements in the output string
- `function`: An optional function that will receive the string representation of the item to join and can transform the text before joining.

#### Returns

A new list with the element inserted.

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%202%2C%203%5D%20%7C%20array.join%20%22%7C%22%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 2, 3] | array.join "|" {{ "}}" }}
```
> **output**
```html
1|2|3
```

## `array.last`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%5D%20%7C%20array.last%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6] | array.last {{ "}}" }}
```
> **output**
```html
6
```

## `array.limit`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%5D%20%7C%20array.limit%202%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6] | array.limit 2 {{ "}}" }}
```
> **output**
```html
[4, 5]
```

## `array.map`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%0Aproducts%20%3D%20%5B%7Btitle%3A%20%22orange%22%2C%20type%3A%20%22fruit%22%7D%2C%20%7Btitle%3A%20%22computer%22%2C%20type%3A%20%22electronics%22%7D%2C%20%7Btitle%3A%20%22sofa%22%2C%20type%3A%20%22furniture%22%7D%5D%0Aproducts%20%7C%20array.map%20%22type%22%20%7C%20array.uniq%20%7C%20array.sort%20%7D%7D&model={})
```scriban-html
{{ "{{" }}
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.map "type" | array.uniq | array.sort {{ "}}" }}
```
> **output**
```html
["electronics", "fruit", "furniture"]
```

## `array.offset`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%2C%207%2C%208%5D%20%7C%20array.offset%202%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6, 7, 8] | array.offset 2 {{ "}}" }}
```
> **output**
```html
[6, 7, 8]
```

## `array.remove_at`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%2C%207%2C%208%5D%20%7C%20array.remove_at%202%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6, 7, 8] | array.remove_at 2 {{ "}}" }}
```
> **output**
```html
[4, 5, 7, 8]
```
If the `index` is negative, removes at the end of the list (notice that we need to put -1 in parenthesis to avoid confusing the parser with a binary `-` operation):
> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%2C%207%2C%208%5D%20%7C%20array.remove_at%20%28-1%29%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6, 7, 8] | array.remove_at (-1) {{ "}}" }}
```
> **output**
```html
[4, 5, 6, 7]
```

## `array.reverse`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%2C%207%5D%20%7C%20array.reverse%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6, 7] | array.reverse {{ "}}" }}
```
> **output**
```html
[7, 6, 5, 4]
```

## `array.size`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B4%2C%205%2C%206%5D%20%7C%20array.size%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [4, 5, 6] | array.size {{ "}}" }}
```
> **output**
```html
3
```

## `array.sort`

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
> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B10%2C%202%2C%206%5D%20%7C%20array.sort%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [10, 2, 6] | array.sort {{ "}}" }}
```
> **output**
```html
[2, 6, 10]
```
Sorts by elements member's value:
> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%0Aproducts%20%3D%20%5B%7Btitle%3A%20%22orange%22%2C%20type%3A%20%22fruit%22%7D%2C%20%7Btitle%3A%20%22computer%22%2C%20type%3A%20%22electronics%22%7D%2C%20%7Btitle%3A%20%22sofa%22%2C%20type%3A%20%22furniture%22%7D%5D%0Aproducts%20%7C%20array.sort%20%22title%22%20%7C%20array.map%20%22title%22%0A%7D%7D&model={})
```scriban-html
{{ "{{" }}
products = [{title: "orange", type: "fruit"}, {title: "computer", type: "electronics"}, {title: "sofa", type: "furniture"}]
products | array.sort "title" | array.map "title"
{{ "}}" }}
```
> **output**
```html
["computer", "orange", "sofa"]
```

## `array.uniq`

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

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%201%2C%204%2C%205%2C%208%2C%208%5D%20%7C%20array.uniq%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 1, 4, 5, 8, 8] | array.uniq {{ "}}" }}
```
> **output**
```html
[1, 4, 5, 8]
```

## `array.contains`

```
array.contains <list> <item>
```

#### Description

Returns if a `list` contains a specific `item`.

#### Arguments

- `list`: The input list
- `item`: The input item

#### Returns

**true** if `item` is in `list`; otherwise **false**

#### Examples

> **input** [:fast_forward: Try out](https://scribanonline.azurewebsites.net/?template=%7B%7B%20%5B1%2C%202%2C%203%2C%204%5D%20%7C%20array.contains%204%20%7D%7D&model={})
```scriban-html
{{ "{{" }} [1, 2, 3, 4] | array.contains 4 {{ "}}" }}
```
> **output**
```html
true
```

> Note: This document was automatically generated from the source code using `Scriban.DocGen`.
