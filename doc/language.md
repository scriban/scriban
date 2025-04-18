# Language

This document describes the syntax of the scriban language in a templating context (within `{{` and `}}`).

The language rules are the same in a pure scripting context.

> NOTE: This document does not describe the `liquid` language. Check the [`liquid website`](https://shopify.github.io/liquid/) directly.

## Table of Contents

- [Table of Contents](#table-of-contents)
- [1. Blocks](#1-blocks)
  - [1.1 Code block](#11-code-block)
  - [1.2 Text block](#12-text-block)
  - [1.3 Escape block](#13-escape-block)
  - [1.4 Whitespace control](#14-whitespace-control)
  - [1.5 Auto indentation](#15-auto-indentation)
- [2 Comments](#2-comments)
- [3 Literals](#3-literals)
  - [3.1 Strings](#31-strings)
  - [3.2 Numbers](#32-numbers)
  - [3.3 Boolean](#33-boolean)
  - [3.4 null](#34-null)
- [4 Variables](#4-variables)
  - [4.1 The special variable `this`](#41-the-special-variable-this)
  - [4.2 The special variable `empty`](#42-the-special-variable-empty)
- [5 Objects](#5-objects)
  - [5.1 The special property `empty?`](#51-the-special-property-empty)
- [6 Arrays](#6-arrays)
  - [6.1 Array with properties](#61-array-with-properties)
  - [6.2 The special `size` property](#62-the-special-size-property)
- [7 Functions](#7-functions)
  - [7.1 Simple functions](#71-simple-functions)
  - [7.2 Anonymous functions](#72-anonymous-functions)
  - [7.3 Parametric functions](#73-parametric-functions)
  - [7.4 Inline functions](#74-inline-functions)
  - [7.5 Function Pointers](#75-function-pointers)
- [8 Expressions](#8-expressions)
  - [8.1 Variable path expressions](#81-variable-path-expressions)
  - [8.2 Assign expression](#82-assign-expression)
  - [8.3 Nested expression](#83-nested-expression)
  - [8.4 Arithmetic expressions](#84-arithmetic-expressions)
    - [On numbers](#on-numbers)
    - [On strings](#on-strings)
  - [8.5 Conditional expressions](#85-conditional-expressions)
  - [8.6 Unary expressions](#86-unary-expressions)
  - [8.7 Range expressions](#87-range-expressions)
  - [8.8 The null-coalescing operators `??`, `?!`](#88-the-null-coalescing-operators--)
  - [8.9 Function call expression](#89-function-call-expression)
    - [Named arguments](#named-arguments)
- [9 Statements](#9-statements)
  - [9.1 Single expression](#91-single-expression)
  - [9.2 Compound Assignment](#92-compound-assignment)
  - [9.3 `if <expression>`, `else`, `else if <expression>`](#93-if-expression-else-else-if-expression)
    - [Truthy and Falsy](#truthy-and-falsy)
  - [9.4 `case` and `when`](#94-case-and-when)
  - [9.5 Loops](#95-loops)
    - [`for <variable> in <expression> ... end`](#for-variable-in-expression--end)
      - [The `offset` parameter](#the-offset-parameter)
      - [The `limit` parameter](#the-limit-parameter)
      - [The `reversed` parameter](#the-reversed-parameter)
    - [`while <expression> ... end`](#while-expression--end)
    - [`tablerow <variable> in <expression> ... end`](#tablerow-variable-in-expression--end)
      - [The `cols` parameter](#the-cols-parameter)
    - [Special loop variables](#special-loop-variables)
    - [`break` and `continue`](#break-and-continue)
  - [9.6 `capture <variable> ... end`](#96-capture-variable--end)
  - [9.7 `readonly <variable>`](#97-readonly-variable)
  - [9.8 `import <variable_path>`](#98-import-variable_path)
  - [9.9 `with <variable> ... end`](#99-with-variable--end)
  - [9.10 `wrap <function> <arg1...argn> ... end`](#910-wrap-function-arg1argn--end)
  - [9.11 `include <name> arg1?...argn?` and `include_join <names> <separator> <begin?> <end?>`](#911-include-name-arg1argn)
  - [9.12 `ret <expression>?`](#912-ret-expression)

[:top:](#language)
## 1. Blocks

There are 3 types of block of text in a template:

- **Code block**: contains scriban template statements
- **Text block**: a plain block to output *as is*
- **Escape block**: a text block that can escape code blocks 

[:top:](#language)
### 1.1 Code block

A text enclosed by `{{` and `}}` is a scriban **code block** that will be evaluated by the scriban templating engine.

A scriban code block may contain:

- a **single line expression statement**:
   `{{ name }}`
- or a **multiline statements**:
    ```
    {{
      if !name
        name = "default"
      end
      name
    }}
    ```
- or **statements separated by a semi-colon `;`** to allow compact forms in some use cases:
    ```
    {{if !name; name = "default"; end; name }}
    ```

In a code block, white space characters have no impact on parsing, with the exception of the end-of-line character following each statement. The only exception is when white space is used to distinguish between an array indexer and an array initializer. 

Additionally, when a statement is an expression (but not an assignment expression), the result of the expression will be displayed in the template's output:

> **input**
```scriban-html
{{
  x = 5     # This assignment will not output anything
  x         # This expression will print 5
  x + 1     # This expression will print 6
}}
```
> **output**
```html
56
```
Note that in the previous example, there is no EOL between `5` and `6` because we are inside a code block. 
You can still use a plain string with an EOL inside a code block `"\n"` or you could use mixed code and text blocks:

> **input**
```scriban-html
{{ x = 5 }}
{{ x }}
{{ x + 1 }}
```
> **output**
```html
5
6
```

[:top:](#language)
### 1.2 Text block

Otherwise, any text is treated as a **text block** and is outputted without modification.

```
Hello this is {{ name }}, welcome to scriban!
______________          _____________________
^ text block            ^ text block

```

[:top:](#language)
### 1.3 Escape block

Any code and text block can be escaped to produce a text block by enclosing it with `{%{` and `}%}` 

For example the following escape:
> **input**: `{%{Hello this is {{ name }}}%}`   
> **output**: `Hello this is {{ name }}` 

If you want to escape an escape block, you can increase the number of % in the starting and ending block:
> **input**: `{%%{This is an escaped block: }%} here}%%}`
> **output**: `This is an escaped block: }%} here`

This allow effectively to nest escape blocks and still be able to escape them.

This allows for effective nesting of escape blocks and the ability to escape them.
For example, a starting escape block {%%%%{ will require an ending }%%%%}"

[:top:](#language)
### 1.4 Whitespace control

By default, any whitespace (including new lines) before or after a code/escape block are copied as-is to the output. 

Scriban provides **two modes** for controlling whitespace:

- The **greedy mode** using the character `-` (e.g `{{-` or `-}}`), **removes any whitespace, including newlines** 
  Examples with the variable `name = "foo"`:
  
  * Strip whitespace on the left:  
    > **input**
    ```scriban-html
    This is a <       
    {{- name}}> text
    ``` 
    > **output**
    ```html
    This is a <foo> text
    ```
    
  * Strip on the right:  
    > **input**
    ```scriban-html
    This is a <{{ name -}} 
    > text:       
    ``` 
    > **output**
    ```html
    This is a <foo> text
    ```
  
  * Strip on both left and right:  
    > **input**
    ```scriban-html
    This is a <
    {{- name -}} 
    > text:       
    ```
    > **output**
    ```html
    This is a <foo> text
    ```

- The **non greedy mode** using the character `~`
  - Using a `{{~` will remove any **preceeding whitespace** until it reaches a **non whitespace character such as a newline or letter**
  - Using a `~}}` will remove any **following whitespace including the first newline** until it reaches a **non whitespace character or a second newline**

  This mode is very convenient when you want to use only a scriban statement on a line, but want that line to be completely 
  removed from the output, but to keep spaces before and after this line intact.

  In the following example, we want to remove entirely the lines `{{~ for product in products ~}}` and `{{~ end ~}}`, but we want
  for example to keep the indentation of the opening `<li>`.

  Using the greedy mode `{{-` or `-}}` would have removed all whitespace and lines and would have put the results on a single line.

  > **input**
  ```
  <ul>
      {{~ for product in products ~}}
      <li>{{ product.name }}</li>
      {{~ end ~}}
  </ul>
  ```

  > **output**
  ```
  <ul>
      <li>Orange</li>
      <li>Banana</li>
      <li>Apple</li>
  </ul>
  ```

Both mode `~` and '-' can also be used with **escape blocks** `{%%{~` or `~}%%}` or `{%%{-` or `-}%%}`

[:top:](#language)
### 1.5 Auto indentation

By default, when a code enter is without a left strip (e.g `{{-` or `{{~`) and is preceded by only whitespace on the same line, the content of the block will be indented accordingly to the number of whitespace before the code enter. 

> **input**
```
{{ a_multi_line_value = "test1\ntest2\ntest3\n" ~}}
   {{ a_multi_line_value }}Hello
```

Notice the 3 whitespace characters `   ` before the expression `{{ a_multi_line_value }}`.

> **output**
```
   test1
   test2
   test3
Hello   
```

The output is auto-indented. This feature can be turned off on by setting `TemplateContext.AutoIndent = false`.

Note that if the previous line contains a greedy right strip `-}}`, the indent will be skipped on the next code enter of the next line.

> **input**
```
{{ a_multi_line_value = "test1\ntest2\ntest3\n" -}}
   {{ a_multi_line_value }}Hello
```

> **output**
```
test1
test2
test3
Hello   
```

[:top:](#language)
## 2 Comments

Within a code block, scriban supports single line comments `#` and multi-line comments `##`:

`{{ name   # this is a single line comment }}`

> **input**
```scriban-html
{{ ## This 
is a multi
line
comment ## }}
```
> **output**
```html

```

As you can notice, both single line and multi-line comments can be closed by the presence of a code block exit tag `}}`

[:top:](#language)
## 3 Literals

### 3.1 Strings

Scriban supports two types of strings:

- **regular strings** enclosed by double quotes `"..."` or simple quotes `'...'`. Regular strings supports multiline and will interpret the following escape sequences:
  - `\'` single quote
  - `\"` double quote
  - `\\` backslash
  - `\n` new line
  - `\r` carriage return
  - `\t` tab
  - `\b` backspace
  - `\f` form feed
  - `\uxxxx` where xxxx is a unicode hexa code number `0000` to `ffff` 
  - `\x00-\xFF` a hexadecimal ranging from `0x00` to `0xFF`

- **verbatim strings** enclosed by backstick quotes `` `...` ``. They are, for example, useful to use with for regex patterns :
  > **input**
  ```scriban-html
  {{ "this is a text" | regex.split `\s+` }}
  ``` 
  > **output**
  ```html 
  [this, is, a, text]
  ``` 

- **Interpolated strings** starting with a `$` enclosed by double quotes `$"..."` or simple quotes `$'...'`.
  > **input**
  ```scriban-html
  {{ $"this is an interpolated string with an expression {1 + 2} and a substring {"Hello"}" }}
  ``` 
  > **output**
  ```html 
  this is an interpolated string with an expression 3 and a substring Hello
  ``` 
  
[:top:](#language)
### 3.2 Numbers

A number in scriban `{{ 100 }}` is similar to a javascript number: 

- Integers: `100`, `1e3`
  - Hexadecimal integers: `0x1ef` and unsigned `0x80000000u`
- Floats: `100.0`, `1.0e3`, `1.0e-3`
  - 32-bit floats: `100.0f`
  - 64-bit floats: `100.0d`
  - 128-bit decimals: `100.0m` 

[:top:](#language)
### 3.3 Boolean

The boolean value `{{ true }}` or `{{ false }}`

> **input**
```scriban-html
{{ true }}
{{ false }}
```
> **output**
```scriban-html
true
false
```

[:top:](#language)
### 3.4 null

The null value `{{ null }}` 

When resolving to a string output, the null value will output an empty string:

> **input**
```scriban-html
{{ null }}
```
> **output**
```html

```

[:top:](#language)
## 4 Variables

Scriban supports the concept of **global** and **local** variables

A **global/property variable** like `{{ name }}` is a liquid like handle, starting by a letter or underscore `_` and following by a letter `A-Z a-z`, a digit `0-9`, an underscore `_`

The following text are valid variable names:

- `var` 
- `var9`
- `_var`

> NOTE: In liquid, the character `-` is allowed in a variable name, but when translating it to a scriban, you will have to enclose it into a quoted string

A **local variable** like `{{ $name }}` is an identifier starting with `$`. A local variable is only accessible within the same include page or function body.

The **special local variable** `$` alone is an array containing the arguments passed to the current function or include page.

The special local variables `$0` `$1` ... `$n` is a shorthand of `$[0]`, `$[1]` ... `$[n]`. e.g Using `$0` returns the first argument of the current function or including page.

### 4.1 The special variable `this`

The `this` variable gives you access to the current object bound where you have access to all local variables for the current scope.

Thus the following variable access are equivalent:

> **input**
```scriban-html
{{
a = 5
a    # output 5
this.a = 6
a    # output 6
this["a"] = 7
a    # output 7
}}
```
> **output**
```html
567
```
In the case of the `with` statement, the this operator refers to the object passed to `with`:

> **input**
```scriban-html
{{
a = {x: 1, y: 2}
with a
    b = this
end
b.x
}}
```
> **output**
```html
1
```

[:top:](#language)
### 4.2 The special variable `empty`

The empty variable represents an empty object and is primarily used for compatibility with Liquid templates. It provides a way to compare an object with the empty variable to determine if it is empty or not:

> **input**
```scriban-html
{{
a = {}
b = [1, 2]~}}
{{a == empty}}
{{b == empty}}
```
> **output**
```html
true
false
```

[:top:](#language)
## 5 Objects

Scriban supports javascript like objects `{...}`

An object can be initialized empty:

`{{ myobject = {} }}` 

An object can be initialized with some members:

`{{ myobject = { member1: "yes", member2: "no" } }}`

or use a json syntax:

`{{ myobject = { "member1": "yes", "member2": "no" } }}`

An object can be initialized with some members over multiple lines:

```
{{
  myobject = { 
      member1: "yes", 
      member2: "no" 
  } 
}}
```

Members of an object can be accessed using dot notation or square bracket notation:

`{{ myobject.member1 }}` also equivalent to `{{ myobject["member1"] }}`


You can access optional members in chain via the optional member operator `?.` (instead of the regular member operator: `.` ) (**New in 3.0**)

`{{ myobject.member1?.submember1?.submember2 ?? "nothing" }}` will return `"nothing"` as `member1` doesn't contain a `submember1`/`submember2`.

If the object is a "pure" scriban objects (created with a `{...}` or  instantiated by the runtime as a `ScriptObject`), you can also add members to it with a simple assignment:

> **input**
```scriban-html
{{
  myobject = {} 
  myobject.member3 = "may be" 
  myobject.member3
}}
``` 
> **output**
```html
may be
``` 

> **NOTICE**
>
> By default, Properties and methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](runtime.md#member-renamer) delegate

### 5.1 The special property `empty?`

Any object can respond the the property `.empty?` to check if it is empty or not:

> **input**
```scriban-html
{{
a = {}
b = [1, 2]~}}
{{a.empty?}}
{{b.empty?}}
```
> **output**
```html
true
false
```

[:top:](#language)
## 6 Arrays

An array can be initialized empty:

`{{ myarray = [] }}` 

An array can be initialized with some items:

`{{ myarray = [1, 2, 3, 4] }}`

An array can be initialized with some items over multiple lines:

```
{{
  myarray = [ 
    1,
    2,
    3,
    4,
  ] 
}}
```

Items of an array can be zero-based indexed:

`{{ myarray[0] }}`

If the array is a "pure" scriban array (created with a `[...]` or  instantiated by the runtime as a `ScriptArray`), you can also add items to it with a simple assignment that will expand automatically the array depending on the index:

```
{{
  myarray = [] 
  myarray[0] = 1 
  myarray[1] = 2 
  myarray[2] = 3 
  myarray[3] = 4 
}}
``` 

You can also manipulate arrays with the [`array` builtin object](#array-builtin).

> **Important notice**
> 
> While whitespace characters are mostly not relevant while parsing in scriban, there is a case where a **whitespace helps to disambiguate between an array  indexer and an array initializer**.
>  
> For instance, if a whitespace is found before a `[` and the previous expression was a variable path expressions (see later), the following expression `[...]` will be considered as an array initializer instead of an array indexer:
> 
> ```
> {{
> myfunction [1]  # There is a whitespace after myfunction. 
>                 # It will result in a call to myfunction passing an array as an argument
> 
> myvariable[1]   # Without a whitespace, this is accessing 
>                 # an element in the array provided by myvariable
> }}    
> ```

### 6.1 Array with properties

An array can also contains attached properties:

> **input**
```scriban-html
{{
a = [5, 6, 7]
a.x = "yes"
a.x + a[0]
}}
```
> **output**
```html
yes5
```

[:top:](#language)
### 6.2 The special `size` property

Arrays have a `size` property that can be used to query the number of elements in the array:

> **input**
```scriban-html
{{
a = [1, 2, 3]
a.size
}}
```
> **output**
```html
3
```

[:top:](#language)
## 7 Functions

Scriban allows for the definition of four different types of functions:

- Simple functions
- Anonymous functions
- Parametric functions (**New in 3.0**)
- Inline functions (**New in 3.0**)

### 7.1 Simple functions

The following declares a function `sub` that takes two arguments, `a` and `b`, and subtracts the value of `b` from `a`:

``` 
{{func sub
   ret $0 - $1
end}}
``` 

All argument are passed to the special variable `$` that will contain the list of direct arguments
and named arguments:

- `$0` or `$[0]` will access the first argument
- `$1` or `$[1]` will access the second argument
- `$[-1]` will access the last argument
- `$.named` will access the named argument `named` 

This function can then be used:

> **input**
```
{{sub 5 1}}
{{5 | sub 1}}
```
> **output**
```
4
4
```

As you can notice from the example above, when using the pipe, the result of the pipe is pushed as the **first argument** of the pipe receiver.

Note that a function can have mixed text statements as well:

``` 
{{func inc}}
   This is a text with the following argument {{ $0 + 1 }}
{{end}}
```

> NOTE: Setting a non-local variable (e.g `a = 10`) in a simple function will be set at the global level and not at the function level.
>
> Parametric functions are solving this behavior by introducing a new variable scope inside the function that includes parameters. 
 
### 7.2 Anonymous functions

Anonymous functions are like simple functions but can be used in expressions (e.g as the last argument of function call)


> **input**
```
{{ sub = do; ret $0 - $1; end; 1 | sub 3 }}
```
> **output**
```
-2
```

They are very convenient to build custom block functions:

> **input**
```
{{ func launch; ret $0 1 2; end
launch do 
    ret $0 + $1
end
}}
```
> **output**
```
3
``` 
 
### 7.3 Parametric functions

They are similar to simple functions but they are declared with parenthesis, while also supporting declaration of different kind of parameters (normal, optional, variable).

Another difference with simple functions is that they require function calls and arguments to match the expected function parameters. 

- A function with normal parameters:

``` 
{{func sub(x,y)
   ret x - y
end}}
``` 

> **input**
```
{{sub 5 1}}
{{5 | sub 1}}
```
> **output**
```
4
4
```


- A function with normal parameters and optional parameters with default values:

``` 
{{func sub_opt(x, y, z = 1, w = 2)
   ret x - y - z - w
end}}
``` 

> **input**
```
{{sub_opt 5 1}}
{{5 | sub_opt 1}}
```
> **output**
```
1
1
```

Here we override the value of `z` and set it to `0` instead of default `1`:

> **input**
```
{{sub_opt 5 1 0 }}
{{5 | sub_opt 1 0}}
```
> **output**
```
2
2
```

- A function with normal parameters and optional parameters with default values:

``` 
{{func sub_variable(x, y...)
   ret x - (y[0] ?? 0) - (y[1] ?? 0)
end}}
``` 

> **input**
```
{{sub_variable 5 1 -1}
{{5 | sub_variable 1 -1}}
```
> **output**
```
5
5
```

> NOTE: The special variable `$` is still accessible in parametric functions and represent the direct list of arguments. In the example above, `$ =  [5, [1, -1]]` 
 
### 7.4 Inline functions

For simple functions, it is convenient to define simple functions like mathematical functions:

```
{{ sub(x,y) = x - y }}
```

Inline functions are similar to parametric functions but they only support normal parameters. They don't support optional or variable parameters.



### 7.5 Function Pointers

Because functions are object, they can be stored into a property of an object by using the alias `@` operator:

```
{{
myobject.myinc = @inc  # Use the @ alias operator to allow to 
                       # use a function without evaluating it
x = 1 | myobject.myinc # x = x + 1
}}

```

The function aliasing operator `@` allows to pass a function as a parameter to another function, enabling powerful function compositions.

[:top:](#language)
## 8 Expressions

Scriban supports conventional unary and binary expressions.

[:top:](#language)
### 8.1 Variable path expressions

A variable path expression contains the path to a variable:

* A simple variable access: `{{ name }}` e.g resolve to the top level variable `name`
* An array access: `{{ myarray[1] }}` e.g resolve to the top level variable `myarray` and an indexer to the array
* A member access: `{{ myobject.member1.myarray[2] }}` e.g resolve to the top level variable `myobject`, then the property `member1` this object, the property `myarray` and an indexer to the array returned by `myarray`

Note that a variable path can either point to a simple variable or can result into calling a parameter less function. 

[:top:](#language)
### 8.2 Assign expression

A value can be assigned to a top level variable or to the member of an object/array:

* `{{ name = "foo" }}` e.g Assign the string `"foo"` the variable `name` 

* `{{ myobject.member1.myarray[0] = "foo" }}`

An assign expression must be a top level expression statement and cannot be used within a sub-expression.

[:top:](#language)
### 8.3 Nested expression

An expression enclosed by `(` and `)` 

`{{ name = ('foo' + 'bar') }}`


[:top:](#language)
### 8.4 Arithmetic expressions

#### On numbers

The following binary operators are supported for **numbers**: 

|Operator            | Description
|--------------------|------------
| `<left> + <right>` | add left to right number 
| `<left> - <right>` | subtract right number from left
| `<left> * <right>` | multiply left by right number
| `<left> / <right>` | divide left by right number
| `<left> // <right>`| divide left by right number and round to an integer
| `<left> % <right>` | calculates the modulus of left by right 

If left or right is a float and the other is an integer, the result of the operation will be a float.

[:top:](#language)
#### On strings

The following binary operators are supported for **strings**: 

|Operator            | Description
|--------------------|------------
| `'left' + <right>` | concatenates left to right string: `"ab" + "c" -> "abc"`
| `'left' * <right>` | concatenates the left string `right` times: `'a' * 5  -> aaaaa`. left and right and be swapped as long as there is one string and one number.

As long as there is a string in a binary operation, the other part will be automatically converted to a string.

The following literals are converted to plain strings:

* `null -> ""`. e.g: `"aaaa" + null -> "aaaa"`
* `0 -> "0"`
* `1.0 -> "1.0"`
* `true -> "true"`
* `false -> "false"`

[:top:](#language)
### 8.5 Conditional expressions

A boolean expression produces a boolean by comparing a left and right value.

|Operator            | Description
|--------------------|------------
| `<left> == <right>` | Is left equal to right? 
| `<left> != <right>` | Is left not equal to right?
| `<left> > <right>`  | Is left greater than right? 
| `<left> >= <right>` | Is left greater or equal to right?
| `<left> < <right>`  | Is left less than right?
| `<left> <= <right>` | Is left less or equal to right?

They work with both `numbers`, `strings` and datetimes.

You can combine conditional expressions with `&&` (and operator) and `||` (or operator).
Unlike in `javascript` it always returns `boolean` and never `<left>` or `<right>`.

|Operator            | Description
|--------------------|------------
| `<left> && <right>` | Is left true and right true?
| `<left> \|\| <right>` | Is left true or right true?

The conditional expression `cond ? left : right` allow to return `left` if `cond` is `true` otherwise `right`. (**New in 3.0**)

[:top:](#language)
### 8.6 Unary expressions

|Operator             | Description
|---------------------|------------
| `! <expression>`    | Boolean negate an expression. e.g `if !page` 
| `+ <expression>`    | Arithmetic positive an expression. e.g `+1.5`
| `- <expression>`    | Arithmetic negate an expression  
| `^ <expression>`    | Expand an array passed to arguments of a function call (see function call)
| `@ <expression>`    | Alias the result of an expression that would be evaluated if it was a function call
| `++ <variable>`     | Increments the variable.  Expression is evaluated to the value *after* it is incremented.
| `-- <variable>`     | Decrements the variable.  Expression is evaluated to the value *after* it is decremented.
| `<variable> ++`     | Increments the variable.  Expression is evaluated to the value *before* it is incremented.
| `<variable> --`     | Decrements the variable.  Expression is evaluated to the value *before* it is decremented.

> *Note:* For the increment an decrement operators, the operand must be a variable, property or indexer

[:top:](#language)
### 8.7 Range expressions

They are special binary expressions that provides an iterator (used usually with the `for` statement)

The evaluated `left` and `right` expressions must resolve to an integer at runtime.

|Operator             | Description
|---------------------|------------
| `left..right`   | Returns an iterator between `left` and `right` with a step of 1, including `right`. e.g: `1..5` iterates from 1 to 5
| `left..<right`  | Returns an iterator between `left` and `right` with a step of 1, excluding `right`. e.g: `1..<5` iterates from 1 to 4

### 8.8 The null-coalescing operators `??`, `?!` 

The operator `left ?? right` can be used to return the `right` value if `left` is null.

The operator `left ?! right` can be used to return the `right` value if `left` is not null.

[:top:](#language)
### 8.9 Function call expression

A function can be called by passing parameters separated by a whitespace:

`{{ myfunction arg1 "arg2" (1+5) }}`

The pipe operator `|` can also be used to pipe the result of an expression to a function:

`{{ date.parse '2016/01/05' | date.to_string '%g' }}` will output `06 Jan 2016`

> Notice that when a function receives the result of a pipe call (e.g `date.to_string` in the example above), it is passed as the **first argument of the call**. This is valid for both .NET custom functions as well as for Scriban integrated functions.

Pipes are *greedy* with respect to whitespace.  This allow them to be chained across multiple lines:  

```
{{-
"text"                        |
      string.append "END"     |
      string.prepend "START"
-}}
```
      
will output `STARTtextEND`

#### Named arguments

When passing multiple arguments to an existing .NET function, you may want to use named arguments.

Suppose you have declared a .NET function like this:

```c#
public static string MyProcessor(string left, string right, int count, string options = null)
{
    // ...
}
```

You can call this function from scriban with the following syntax:

```scriban-html
{{ my_processor "Hello" "World" count: 15 options: "optimized" }}
```

with a pipe we could rewrite this to:

```scriban-html
{{ "Hello" | my_processor "World" count: 15 options: "optimized" }}
```
> Note that once arguments are named, the following arguments must be all named.

In a custom function declared with `func` named arguments are accessible through the variable arguments variable `$`, but as properties (and not as part of the default array arguments):

> **input**
```scriban-html
{{
    func my_processor
        "Argument count:" + $.count
        "Argument options:" + $["options"]
        for $x in $
            "arg[" + $x + "]: " + $x
        end
    end

    my_processor "Hello" "World" count: 15 options: "optimized"
}}
```

> **output**
```html
Argument count: 15
Argument options: optimized
arg[0]: Hello
arg[1]: World
```

[:top:](#language)
## 9 Statements

Each statement must be terminated by a code block `}}` or an EOL within a code block, or a semicolon to separate multiple statements on a single line within a code block.

[:top:](#language)
### 9.1 Single expression

An expression statement:

`{{ value + 1 }}` e.g Evaluates `value + 1` and output the result

```
{{
value + 1       # This is a single line expression statement followed by this comment
}}
```

[:top:](#language)
### 9.2 Compound Assignment

The following compound assignment operators are supported for **numbers**:

|Operator             | Description
|---------------------|------------
| `<left> += <right>` | add left to right number, and assigns the result to left
| `<left> -= <right>` | subtract right number from left, and assigns the result to left
| `<left> *= <right>` | multiply left by right number, and assigns the result to left
| `<left> /= <right>` | divide left by right number, and assigns the result to left
| `<left> //= <right>`| divide left by right number and round to an integer, and assigns the result to left
| `<left> %= <right>` | calculates the modulus of left by right, and assigns the result to left

If left or right is a float and the other is an integer, the result of the operation will be a float.

> *Note:* The left-hand side of the assignment statement must be a variable, property or indexer

[:top:](#language)
### 9.3 `if <expression>`, `else`, `else if <expression>`

The general syntax is:

```
{{
if <expression>
  ...
else if <expression>
  ...
else 
  ...
end
}}
```

An `if` statement must be closed by an `end` or followed by a `else` or `else if` statement. An `else` or `else if` statement must be followed by a `else`, `else if` or closed by an `end` statement.

An expression evaluated for a `if` or `else if` will be converted to a boolean.

#### Truthy and Falsy

By default, only the `null` and boolean `false` are considered as `false` when evaluated as booleans.

The following values are used for converting literals to boolean:

- `0 -> true`
- `1 -> true` or any non zero value
- **`null -> false`**
- **`false -> false`**
- `non_null_object -> true`
- `"" -> true` An empty string returns **true**
- `"foo" -> true` 

Example testing a page object:
 
`{{ if page }}Page is not null{{ else }}Page is null!{{ end }}` 


[:top:](#language)
### 9.4 `case` and `when`

This is the equivalent of `switch` statement in C#, a selection statement that chooses a single switch section to execute from a list of candidates based on a value matching. 

- `case <expression>` opens a switch with an expression
- `when <match>` allows to match with the specified expression and the case expression
  - `when` can also be used with multiple values separated by `,` or `||`
- A final `else` can be used to as a default handler in case nothing matched.

> **input**
```scriban-html
{{
    x = 5
    case x
      when 1, 2, 3
          "Value is 1 or 2 or 3"
      when 5
          "Value is 5"
      else
          "Value is " + x
    end
}}
```

> **output**
```html
Value is 5
```

[:top:](#language)
### 9.5 Loops

#### `for <variable> in <expression> ... end`

```
{{for <variable> in <expression>}} 
  ... 
{{end}}
```

The expression can be an array or a range iterator:

* Loop on an array: `{{ for page in pages }}This is the page {{ page.title }}{{ end }}`  

* Loop on a range: `{{ for x in 1..n }}This is the loop step [{{x}}]{{ end }}`  

The for loop (along with the `tablerow` statement below) supports additional parameters, `offset`, `limit` and `reversed` that can also be used togethers:

##### The `offset` parameter

Allows to start the iteration of the loop at the specified zero-based index:

> **input**
```scriban-html
{{~ for $i in (4..9) offset:2 ~}}
 {{ $i }}
{{~ end ~}}
```
> **output**
```html
6
7
8
9
```

##### The `limit` parameter

Limits the iteration of the loop to a specified count:

> **input**
```scriban-html
{{~ for $i in (4..9) limit:2 ~}}
 {{ $i }}
{{~ end ~}}
```
> **output**
```html
4
5
```

##### The `reversed` parameter

Reverses the iteration of the elements:

> **input**
```scriban-html
{{~ for $i in (1..3) reversed ~}}
 {{ $i }}
{{~ end ~}}
```
> **output**
```html
3
2
1
```

[:top:](#language)
#### `while <expression> ... end`

```
{{while <expression>}}
  ...
{{end}}
```

Like the `if` statement, the `expression` is evaluated to a boolean.

#### `tablerow <variable> in <expression> ... end`

This function generates HTML rows compatible with an HTML table. Must be wrapped in an opening `<table>` and closing `</table>` HTML tags.

This statement is mainly for compatibility reason with the liquid `tablerow` tag.
It uses similar syntax to a `for` statement (supporting the same parameters).

```
{{tablerow <variable> in <expression>}} 
  ... 
{{end}}
```
> **input**
```scriban-html
<table>
  {{~ tablerow $p in products | array.sort "title" -}}
    {{ $p.title -}}
  {{ end ~}}
</table>
```
> **output**
```html
<table>
<tr class="row1"><td class="col1">Apple</td></tr>
<tr class="row2"><td class="col1">Banana</td></tr>
<tr class="row3"><td class="col1">Computer</td></tr>
<tr class="row4"><td class="col1">Mobile Phone</td></tr>
<tr class="row5"><td class="col1">Orange</td></tr>
<tr class="row6"><td class="col1">Sofa</td></tr>
<tr class="row7"><td class="col1">Table</td></tr>
</table>
```

##### The `cols` parameter

Defines the number of columns to output:

> **input**
```scriban-html
<table>
  {{~ tablerow $p in (products | array.sort "title") limit: 4 cols: 2 -}}
    {{ $p.title -}}
  {{ end ~}}
</table>
```
> **output**
```html
<table>
<tr class="row1"><td class="col1">Apple</td><td class="col2">Banana</td></tr>
<tr class="row2"><td class="col1">Computer</td><td class="col2">Mobile Phone</td></tr>
</table>
```

[:top:](#language)
#### Special loop variables

The following variables are accessible within a `for` block:

| Name                | Description
| ------------------- | -----------
| `{{for.index}}`     | The current `index` of the for loop
| `{{for.rindex}}`    | The current `index` of the for loop starting from the end of the list
| `{{for.first}}`     | A boolean indicating whether this is the first step in the loop
| `{{for.last}}`      | A boolean indicating whether this is the last step in the loop
| `{{for.even}}`      | A boolean indicating whether this is an even row in the loop
| `{{for.odd}}`       | A boolean indicating whether this is an odd row in the loop
| `{{for.changed}}`   | A boolean indicating whether a current value of this iteration changed from previous step

Within a `while` statement, the following variables can be used:

| Name                | Description
| ------------------- | -----------
| `{{while.index}}`     | The current `index` of the while loop
| `{{while.first}}`     | A boolean indicating whether this is the first step in the loop
| `{{while.even}}`      | A boolean indicating whether this is an even row in the loop
| `{{while.odd}}`       | A boolean indicating whether this is an odd row in the loop

[:top:](#language)
#### `break` and `continue`

The `break` statement allows to early exit a loop

```
{{ for i in 1..5
  if i > 2
    break
  end
end }}
```

The `continue` statement allows to skip the rest of a loop and continue on the next step 

```
{{ for i in 1..5
  if i == 2
    continue
  end
}}
[{{i}}]] step 
{{ end }}
```

Will output:

```
[1] step
[3] step
[4] step
[5] step
```

[:top:](#language)
### 9.6 `capture <variable> ... end`

The `capture <variable> ... end` statement allows to capture the template output to a variable:

For example the following code:

```
{{ capture myvariable }}
This is the result of a capture {{ date.now }} 
{{ end }}
```

will set `myvariable = "This is the result of a capture 06 Jan 2016\n"` 

[:top:](#language)
### 9.7 `readonly <variable>`

The `readonly` statement prevents a variable for subsequent assignments:

```
{{ x = 1 }}
{{ readonly x }}
{{ x = 2 }} <- this will result in a runtime error 
```

[:top:](#language)
### 9.8 `import <variable_path>`

The `import <variable_path>` statement allows to import the members of an object as variables of the current bound: 

```
{{ 
  myobject = { member1: "yes" }
  import myobject
  member1  # will print the "yes" string to the output
}}
``` 

Note that `readonly` variables won't be override. 

[:top:](#language)
### 9.9 `with <variable> ... end`

The `with <variable> ... end` statement will open a new object context with the passed variable, all assignment will result in setting the members of the passed object. 

```
myobject = {}
with myobject
  member1 = "yes"
end
```

[:top:](#language)
### 9.10 `wrap <function> <arg1...argn> ... end`

Pass a block of statements to a function that will be able to evaluate it using the special variable `$$`

```
{{
func wrapped
	for $i in 1..<$0
		$$   # This special variable evaluates the block pass 
             # to the wrap statement
	end
end

wrap wrapped 5
	$i + " -> This is inside the wrap!\r\n"
end
}}
```

will output:

```
1 -> This is inside the wrap!
2 -> This is inside the wrap!
3 -> This is inside the wrap!
4 -> This is inside the wrap!
```

Note that variables declared outside the `with` block are accessible within.

[:top:](#language)
### 9.11 `include <name> arg1?...argn?` and `include_join <names> <separator> <begin?> <end?>`

`include` is not a statement but rather a function that allows you to parse and render a specified template. To use this function, a delegate to a template loader must be setup on the [`TemplateOptions.TemplateLoader`](runtime.md#include-and-itemplateloader) property passed to the `Template.Parse` method.
 
```
include 'myinclude.html'
x = include 'myinclude.html'
x + " modified"
```

assuming that `myinclude.html` is
```
{{ y = y + 1 ~}}
This is a string with the value {{ y }}
```

will output:

```
This is a string with the value 1
This is a string with the value 2 modified
```  

`include_join` also exists to allow rendering multiple templates with a separator and begin/end delimiters. This function has the same requirement as `include`. The separator and begin/end delimiters also support templates by prefixing their name with `tpl:`

usage exemple 1

```
include_join ['myinclude1.html', 'myinclude2.html', 'myinclude3.html'] '<br/>' 'tpl:begin.html' 'tpl:end.html'  
```

This would output all templates separated by an html new line and the whole result would be prefixed with begin.html template and suffixed by end.html template.

usage exemple 2

```
include_join ['myinclude1.html', 'myinclude2.html', 'myinclude3.html'] 'tpl:separator.html' '<div>' '</div>'  
```
This would output all templates separated by the template separator.html encapsulated in a div block.

Note

If the result of the separated templates is empty, the prefix and suffix will not be output.
The result of this function can also be stored in a variable.

[:top:](#language)
### 9.12 `ret <expression>?`

The return statement is used to early exit from a top-level/include page or a function.

```
This is a text
{{~ ret ~}}
This text will not appear
```

will output:

```
This is a text
```

[:top:](#language)
