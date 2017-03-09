# Language

This document describes the syntax of the scriban templating language.

## 1. Blocks

There are 3 types of block of text in a template:

- **Code block**: contains scriban template statements
- **Text block**: a plain block to output *as is*
- **Escape block**: a text block that can escape code blocks 

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
Inside a code block, except for the EOL after each statement, white spaces characters are not affecting the parsing. There is only one case where whitespace is used to disambiguate between an array indexer and an array initializer.   

### 1.2 Text block

Otherwise, any text is considered as a **text block** and simply output as is

```
	Hello this is {{ name }}, welcome to scriban!
    ______________          _____________________
    ^ text block            ^ text block

```

### 1.3 Escape block

Any code and text block can be escaped to produce a text block by enclosing it with `{%{` and `}%}` 

For example the following escape:
-  `{%{Hello this is {{ name }}}%}`
   
> **output**: `Hello this is {{ name }}` 

Any escape block can be also escaped by increasing the number of `%` in the starting and ending block:
- `{%%{This is an escaped block: }%} here}%%}`
   
> **output**: `This is an escaped block: }%} here`

Hence a starting escape block `{%%%%{` will required an ending `}%%%%}`

### 1.4 Whitespace control

By default, any whitespace (including new lines) before or after a code/escape block are copied to the output. You can omit whitespace just before or after a code/escape block by using the character `~`:

Examples with the variable `name = "foo"`:

* Strip whitespaces on the left:

``` 
This is a <       
{{~ name}}> text
``` 

> **output**: `This is a <foo> a text` 

* Strip on the right:

``` 
This is <{{ name ~}} 
> a text:       
``` 

> **output**: `This is a <foo> a text` 

* Strip on both left and right:

``` 
This is <
{{~ name ~}} 
> a text:       
``` 

> **output**: `This is a <foo> a text` 

The `~` character can also be used with **escape blocks** `{%%{~` or `~}%%}`

## 2 Comments

Within a code block, scriban supports single line comments `#` and multi-line comments `##`:

`{{ name   # this is a single line comment }}`

```
{{ ## This 
is a multi
line
comment ## }}
```

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
- **verbatim strings** enclosed by backstick quotes `` `...` ``. They are, for example, useful to use with for regex patterns :
  ``` 
  {{ "this is a text" | regex.split `\s+` }}
  ``` 
  
  will output:
  
  ``` 
  [this, is, a, test]
  ``` 

### 3.2 Numbers

A number in scriban `{{ 100 }}` is similar to a javascript number: 

- Integers: `100`, `1e3`
- Floats: `100.0`, `1.0e3`, `1.0e-3` 

### 3.3 Boolean

The boolean value `{{ true }}` or `{{ false }}`

### 3.4 null

The null value `{{ null }}` 

## 4 Variables

Scriban supports the concept of **global** and **local** variables

A **global/property variable** like `{{ name }}` is a javascript like identifier, starting by a letter and following by a letter `A-Z a-z`, a digit `0-9` or an underscore `_`

A **local variable** like `{{ $name }}` is an identifier starting with `$`. A local variable is only accessible within the same include page or function body.

The **special local variable** `$` alone is an array containing the arguments passed to the current function or include page.

The special local variables `$0` `$1` ... `$n` is a shorthand of `$[0]`, `$[1]` ... `$[n]`. e.g Using `$0` returns the first argument of the current function or including page.

## 5 Objects

Scriban supports javascript like objects `{...}`

An object can be initialized empty :

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

Members of an object can be accessed:

`{{ myobject.member1 }}` also equivalent to `{{ myobject["member1"] }}`

If the object is a "pure" scriban objects (created with a `{...}` or  instantiated by the runtime as a `ScriptObject`), you can also add members to it with a simple assignment:

```
{{
  myobject = {} 
  myobject.member3 = "may be" 
}}
``` 

## 6 Arrays

An array can be initialized empty :

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


## 7 Functions

Scriban allows to define functions:

The following declares a function `inc` that uses its first argument to return an increment of the value.

``` 
{{func inc
   ret $0 + 1
end}}
``` 

This function can then be used:

```
{{inc 1}}
{{5 | inc}}
```

will output:

```
2
6
```

Because functions are object, they can be stored into a property of an object:

```
{{
myobject.myinc = @inc  # Use the @ alias operator to allow to 
                       # use a function without evaluating it
x = 1 | myobject.myinc # x = x + 1
}}

```

The function aliasing operator `@` allows to pass a function as a parameter to another function, enabling powerful function compositions.

## 8 Expressions

Scriban supports conventional unary and binary expressions.

### 8.1 Variable path expressions

A variable path expression contains the path to a variable:

* A simple variable access: `{{ name }}` e.g resolve to the top level variable `name`
* An array access: `{{ myarray[1] }}` e.g resolve to the top level variable `name`
* A member access: `{{ myobject.member1.myarray[2] }}` e.g resolve to the top level variable `myobject`, then the property `member1` this object, the property `myarray` and an indexer to the array returned by `myarray`

Note that a variable path can either point to a simple variable or can result into calling a parameter less function. 

### 8.2 Assign expression

A value can be assigned to a top level variable or to the member of an object/array:

* `{{ name = "foo" }}` e.g Assign the string `"foo"` the variable `name` 

* `{{ myobject.member1.myarray[0] = "foo" }}`

An assign expression must be a top level expression statement and cannot be used within a sub-expression.

### 8.3 Nested expression

An expression enclosed by `(` and `)` 

`{{ name = ('foo' + 'bar') }}`


### 8.4 Arithmetic expressions

#### On numbers

The following binary operators are supported for **numbers**: 

|Operator            | Description
|--------------------|------------
| `<left> + <right>` | add left to right number 
| `<left> - <right>` | substract right number from left
| `<left> * <right>` | multiply left by right number
| `<left> / <right>` | divide left by right number
| `<left> // <right>`| divide left by right number and round to an integer
| `<left> % <right>` | calculates the modulus of left by right 

If left or right is a float and the other is an integer, the result of the operation will be a float.

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

### 8.5 Conditional expressions

A conditional expression produces a boolean by comparing a left and right value.

|Operator            | Description
|--------------------|------------
| `<left> == <right>` | Is left equal to right? 
| `<left> != <right>` | Is left not equal to right?
| `<left> > <right>`  | Is left greater than right? 
| `<left> >= <right>` | Is left greater or equal to right?
| `<left> < <right>`  | Is left less than right?
| `<left> <= <right>` | Is left less or equal to right?

They work with both `numbers` and `strings`.

### 8.6 Unary expressions

|Operator             | Description
|---------------------|------------
| `! <expression>`    | Boolean negate an expression. e.g `if !page` 
| `+ <expression>`    | Arithmetic positive an expression. e.g `+1.5`
| `- <expression>`    | Arithmetic negate an expression  
| `^ <expression>`    | Expand an array passed to arguments of a function call (see function call)
| `@ <expression>`    | Alias the result of an expression that would be evaluated if it was a function call

### 8.7 Range expressions

They are special binary expressions that provides an iterator (used usually with the `for` statement)

The evaluated `left` and `right` expressions must resolve to an integer at runtime.

|Operator             | Description
|---------------------|------------
| `left..right`   | Returns an iterator between `left` and `right` with a step of 1, including `right`. e.g: `1..5` iterates from 1 to 5
| `left..<right`  | Returns an iterator between `left` and `right` with a step of 1, excluding `right`. e.g: `1..<5` iterates from 1 to 4

### 8.8 Function call expression

A function can be called by passing parameters separated by a whitespace:

`{{ myfunction arg1 "arg2" (1+5) }}`

The pipe operator `|` can also be used to pipe the result of an expression to a function:

`{{ date.parse '2016/01/05' | date.to_string '%g' }}` will output `06 Jan 2016`

## 9 Statements

Each statement must be terminated by a code block `}}` or an EOL within a code block.

### 9.1 Single expression

An expression statement:

`{{ value + 1 }}` e.g Evaluates `value + 1` and output the result

```
{{
value + 1       # This is a single line expression statement followed by this comment
}}
```

### 9.2 `if <expression>`, `else`, `else if <expression>`

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

The following values are used for converting literals to boolean:

- `0 -> false`
- `1 -> true` or any non zero value
- `null -> false`
- `non_null_object -> true`
- `"" -> false` An empty string returns false
- `"foo" -> true` 

Example testing a page object:
 
`{{ if !page }}Page is not null{{ else }}Page is null!{{ end }}` 

### 9.3 Loops

#### `for <variable> in <expression> ... end`

```
{{for <variable> in <expression>}} 
  ... 
{{end}}
```

The expression can be an array or a range iterator:

* Loop on an array: `{{ for page in pages }}This is the page {{ page.title }}{{ end }}`  

* Loop on a range: `{{ for x in 1..n }}This is the loop step [{{x}}]{{ end }}`  

#### `while <expression> ... end`

```
{{while <expression>}}
  ...
{{end}}
```

Like the `if` statement, the `expression` is evaluated to a boolean.

#### Special loop variables

The following variables are accessible within a `for` block:

| Name                | Description
| ------------------- | -----------
| `{{for.index}}`     | The current `index` of the for loop
| `{{for.first}}`     | A boolean indicating whether this is the first step in the loop
| `{{for.last}}`      | A boolean indicating whether this is the last step in the loop
| `{{for.even}}`      | A boolean indicating whether this is an even row in the loop
| `{{for.odd}}`       | A boolean indicating whether this is an odd row in the loop

Within a `while` statement, the following variables can be used:

| Name                | Description
| ------------------- | -----------
| `{{while.index}}`     | The current `index` of the while loop
| `{{while.first}}`     | A boolean indicating whether this is the first step in the loop
| `{{while.even}}`      | A boolean indicating whether this is an even row in the loop
| `{{while.odd}}`       | A boolean indicating whether this is an odd row in the loop

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

### 9.4 `capture <variable> ... end`

The `capture <variable> ... end` statement allows to capture the template output to a variable:

For example the following code:

```
{{ capture myvariable }}
This is the result of a capture {{ date.now }} 
{{ end }}
```

will set `myvariable = "This is the result of a capture 06 Jan 2016\n"` 

### 9.5 `readonly <variable>`

The `readonly` statement prevents a variable for subsequent assignments:

```
{{ x = 1 }}
{{ readonly x }}
{{ x = 2 }} <- this will result in a runtime error 
```

### 9.6 `import <variable_path>`

The `import <variable_path>` statement allows to import the members of an object as variables of the current bound: 

```
{{ 
  myobject = { member1: "yes" }
  import myobject
  member1  # will print the "yes" string to the output
}}
``` 

Note that `readonly` variables won't be override. 

### 9.7 `with <variable> ... end`

The `with <variable> ... end` statement will open a new object context with the passed variable, all assignment will result in setting the members of the passed object. 

```
myobject = {}
with myobject
  member1 = "yes"
end
```

### 9.8 `wrap <function> <arg1...argn> ... end`

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

### 9.9 `include <name> arg1?...argn?` 

The include is not a statement but actually a function that allows to parse and render the specified template name. In order to use this function, a delegate to an template loader must be setup on the `TemplateOptions.TemplateLoader` property passed to the `Template.Parse` method.
 
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

### 9.10 `ret <expression>?`

The return statement is used to early exit from a top-level/include page or a function.

```
This is a text
{{~  ret ~}}
This text will not appear
```

will output:

```
This is a text
```

## 10 Built-in functions

Scriban provides default built-in functions. 

### 10.1 Array functions

For all array functions, the last argument is expected to be an array or a range iterator. When used with the pipe operator, it is a convenient way to pass the argument to the array funtions.

#### `array.first`

Returns the first element of an array or a range iterator.

```
{{ [5,6,7,8] | array.first }} 
{{ 5..8 | array.first }} 
```

Will output:

```
5
5
```

#### `array.last`

Returns the last element of an array or a range iterator.

```
{{ [5,6,7,8] | array.last }} 
{{ 5..8 | array.last }} 
```

Will output:

```
8
8
```

#### `array.join <delimiter>`

Concatenates elements of an array or a range iterator separated by a delimter string:

```
{{ [5,6,7,8] | array.join ' , ' }} 
{{ 5..8 | array.join ' , ' }} 
```

Will output:

```
5 , 6 , 7 , 8
5 , 6 , 7 , 8
```

#### `array.size`

Returns the number of elements in the array or range iterator.

```
{{ [5,6,7,8] | array.size }} 
{{ 5..8 | array.size }} 
```

Will output:

```
4
4
```

#### `array.uniq`

Filters an array or range iterator by keeping only unique values, returning an iterator.

```
{{ [1,1,2,3,3] | array.uniq }} 
```

Will output:

```
123
```

#### `array.sort`

Sorts an array or range iterator by its natural ascending order, returning an iterator.

```
{{ [5,1,4,2,3] | array.sort }} 
```

Will output:

```
12345
```

#### `array.map <member>`

For each object in the input array, extract the specified member and return a new list with the member value, returning an iterator.

```
{{ array | array.map `key` | array.sort | array.join '\n'}}  
```

will output the sorted members of the array object: 

```
add
add_range
first
insert_at
join
last
map
remove_at
reverse
size
sort
uniq
```

#### `array.add <expression>` 

Adds the specified value to the input array. Returns the array to allow further piping.

``` 
[1,2,3,4] | array.add 5
```

will output:

```
12345
```

#### `array.add_range <iterator>` 

Adds the specified range of values from an array or a range iterator to the input array. Returns the array to allow further piping.

``` 
[1,2,3,4] | array.add_range [5,6,7,8]
```

will output:

```
12345678
```

#### `array.add_range <iterator>` 

Adds the specified range of values from an array or a range iterator to the input array. Returns the array to allow further piping.

``` 
[1,2,3,4] | array.add_range ([5,6,7,8])
```

will output:

```
12345678
```

#### `array.remove_at <index>` 

Removes at the specified `index` an object from the input the array. Returns the array to allow further piping.

``` 
[1,2,3,4] | array.remove_at 0
```

will output:

```
234
```

#### `array.insert_at <index> <expression>` 

Inserts a value at the specified index of the input array. Returns the array to allow further piping.

``` 
[1,2,3,4] | array.insert_at 1 9
```

will output:

```
19234
```

#### `array.reverse` 

Reverse the order of the elements in the input iterator. Returns an iterator (and not an array)

``` 
[1,2,3,4] | array.reverse
```

will output:

```
4321
```

### 10.2 Math functions

#### `math.ceil`

Returns the smallest integer greater than or equal to the specified number.

```
{{ 4.6 | math.ceil }} 
{{ 4.3 | math.ceil }} 
```

Will output:

```
5
5
```

#### `math.floor`

Returns the largest integer less than or equal to the specified number.

```
{{ 4.6 | math.ceil }} 
{{ 4.3 | math.ceil }} 
```

Will output:

```
4
4
```

#### `math.floor <decimals>?`

Rounds a value to the nearest integer or to the specified number of fractional digits.
```
{{ 4.6 | math.round }}
{{ 4.3 | math.round }}
{{ 4.5612 | math.round 2 }}
```

Will output:

```
5
4
4.56
```

### 10.3 String functions

#### `string.capitalize`

Converts the first character of the passed string to a upper case character.

```
{{ "test" | string.capitalize }}
```

Will output:

```
Test
```

#### `string.downcase` and `string.upcase`

Converts the string to lower case (`downcase`) or uppercase (`upcase`)

```
{{ "TeSt" | string.downcase }}
{{ "test" | string.upcase }}
```

Will output:

```
test
TEST
```

#### `string.handleize`

Converts a string to a handle by keeping only alpha and digit and replacing other sequence of characters by `-`.

```
{{ "This & is a @@^^&%%%%% value" | string.handleize }}
```

Will output:

```
This-is-a-value
```

#### `string.pluralize <single> <plural>`

Returns the first or second arguments depending whether the input number is == 1 or > 1.

```
{{ 5 | string.pluralize 'item' 'items' }}
```

Will output:

```
items
```

#### `string.remove <match>`

Removes all occurrence of the `<match>` string from the input.

```
{{ "This is a test with a test" | string.remove 'test' }}
```

Will output:

```
This is a  with a 
```

#### `string.remove_first <match>`

Removes the first occurrence of the `<match>` string from the input.

```
{{ "This is a test with a test" | string.remove_first 'test' }}
```

Will output:

```
This is a  with a test 
```

#### `string.replace <match> <replace>`

Replaces all occurrence of the `<match>` string by the `<replace>` string from the input.

```
{{ "This is a test with a test" | string.replace 'test' 'boom'}}
```

Will output:

```
This is a boom with a boom 
```

#### `string.replace_first <match> <replace>`

Replaces the first occurrence of the `<match>` string by the `<replace>` string from the input.

```
{{ "This is a test with a test" | string.replace_first 'test' 'boom'}}
```

Will output:

```
This is a boom with a test 
```

#### `string.strip`, `string.rstrip`, `string,lstrip`

Removes any whitespace characters on both side (`strip`), right side only (`rstrip`) or left side only (`lstrip`)

```
{{ "    test     " | string.strip }}
{{ "test     " | string.rstrip }}
{{ "     test" | string.lstrip }}
```

Will output:

```
test
test
test
```

#### `string.slice <index> <length>?`

Extract a sub-string starting at the specified `index` and optional `length` from the input string. A negative number for the `index` will start backward from the end of the string.

```
{{ "test" | string.slice 1 }}
{{ "test" | string.slice (-2) }}
```

Will output:

```
est
st 
```

#### `string.split <delimiter>`

Splits to an array of string the input string with the matching delimiter and removes empty entries.

```
{{ "a/b/c/d/e//f" | string.split '/' | array.join ' | ' }}
```

Will output:

```
a | b | c | d | e | f
```

#### `string.starts_with <match>`

Returns a boolean indicating whether the input string starts with the specified `match` string.

```
{{ "test" | string.starts_with 'test'}}
{{ "test" | string.starts_with 'toto'}}
```

Will output:

```
true
false
```

#### `string.strip_newlines`

Strips all newlines from the input string.

```
{{ "test\r\ntest\r\n" | string.strip_newlines }}
```

Will output:

```
testtest
```

#### `string.truncate <length>`

Truncates the input string up to maximum `length` size, including the trailing `...` that would be added to the string in case of truncation. 

```
{{ "This is a long test with several chars" | string.truncate 15 }}
```

Will output:

```
This is a lo...
```

#### `string.truncatewords <count>`

Truncates the input string up to maximum `count` words. 

```
{{ "This is a test truncated at 5" | string.truncatewords 5 }}
```

Will output:

```
This is a test truncated...
```

### 10.4 Regex


#### `regex.replace <pattern> <replacement> <input>`

Allows to replace a string by matching with a regex pattern. The replacement string can use replacement groups `\1` if the pattern was using groups.

``` 
{{ "this is a teeeeeeeeeeext" | regex.replace "te+xt" "text" }}
``` 

Will output:

``` 
this is a text
``` 

#### `regex.split <pattern> <input>`

Split an input string using a regex pattern.

``` 
{{ "this   is  \t   a  \t   text" | regex.split `\s+` }}
``` 

Will output:

``` 
[this, is, a, text]
``` 

#### `regex.match <pattern> <input>`

Matches a string against a regex pattern and returns an array of strings matched. The first element in the array is the full string being matched and above the groups matched.

```
{{ "this is a text123" | regex.match `(\w+) a ([a-z]+\d+)` }}
```

Will output:

``` 
[is a text123, is, text123]
``` 

If no match are found, an empty array `[]` is returned.

#### `regex.escape <input>` and `regex.unescape <input>` 

Respectively escape and unescape a regex pattern.

```
{{ "..." | regex.escape }}
{{ `\.\.\.` | regex.unescape }}
```

Will output:

```
\.\.\.
...
```

### 10.5 Object

#### `typeof <value>`

Returns the type of the specified value.

```
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

will output:

```

boolean
number
number
string
iterator
array
object
object
```

### 10.6 Datetime

#### Datetime object

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

#### Binary operations

The substract operation `<date1> - <date2>`: Substract `date2` from `date1` and return a timespan internal object (see timespan object below).

Other comparison operators (`==`, `!=`, `<=`, `>=`, `<`, `>`) are also working with date objects.  

A `timespan` and also the added to a `datetime` object.

#### `date.now` 

Returns a datetime object of the current time, including the hour, minutes, seconds and milliseconds.

```
{{ date.now }}

{{ date.now.year  # output the year of the current time }} 

```

#### `date.parse`

Parses the specified input string to a date object. 

```
{{ date.parse '2016/01/05' }}
```

Will output:

```
5 Jan 2016
```

#### `date.add_days <days>`
#### `date.add_months <months>`
#### `date.add_years <years>`

Adds the specified number days/months/years to the input date. 

```
{{ date.parse '2016/01/05' | date.add_days 1 }}
{{ date.parse '2016/01/05' | date.add_months 1 }}
{{ date.parse '2016/01/05' | date.add_years 1 }}

```

Will output:

```
6 Jan 2016
5 Feb 2016
5 Jan 2017
```

#### `date.to_string <format>?`

Converts a datetime object to a textual representation using the specified format string.

By default, if you are using a date, it will use the format specified by `date.format` which defaults to `date.default_format` (readonly) which default to `%d %b %Y`

You can override the format used for formatting all dates by assigning the a new format: `date.format = '%a %b %e %T %Y';`

You can recover the default format by using `date.format = date.default_format;`

By default, the to_string format is using the **current culture**, but you can switch to an invariant culture by using the modifier `%g`

For example, using `%g %d %b %Y` will output the date using an invariant culture.

If you are using `%g` alone, it will output the date with `date.format` using an invariant culture.

Suppose that `date.now` would return the date `2013-09-12 22:49:27 +0530`, the following table explains the format modifiers:

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
| `"%U"` |               | Week number of the current year, starting with the first Sunday as the first day of the first week (00..53)
| `"%W"` |               | Week number of the current year, starting with the first Monday as the first day of the first week (00..53)
| `"%w"` |  `"4"` 	     | Day of week of the time
| `"%x"` |               | Preferred representation for the date alone, no time
| `"%X"` |               | Preferred representation for the time alone, no date
| `"%y"` |  `"13"` 	     | Gives year without century of the time
| `"%Y"` |  `"2013"`     | Year of the time
| `"%Z"` |  `"IST"` 	   | Gives Time Zone of the time
| `"%%"` |  `"%"`        | Output the character `%`

Note that the format is using a good part of the ruby format ([source](http://apidock.com/ruby/DateTime/strftime))

```
date.now | date.to_string `%d %b %Y`
```

will output:

```
5 Jan 2016
```

### 10.7 Timespan

#### Timespan object

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

#### Supported operators

The `+` and `-` are both working with timespan interval objects.

Other comparison operators (`==`, `!=`, `<=`, `>=`, `<`, `>`) are also working with timespan objects.  

A `timespan` and also the added to a `datetime` object.

#### `timespan.zero`

Returns a timespan object that represents a 0 interval

```
{{ (timespan.zero + timespan.from_days 5).days }}
```

will output:
   
```
5
```

#### `timespan.from_days <days>`

Returns a timespan object that represents a `days` interval

```
{{ (timespan.from_days 5).days }}
```

will output:
   
```
5
```

#### `timespan.from_hours <hours>`

Returns a timespan object that represents a `hours` interval

```
{{ (timespan.from_hours 5).hours }}
```

will output:
   
```
5
```

The same functions exists for `timespan.from_minutes`, `timespan_from_seconds`, `timespan.from_milliseconds`.

#### `timespan.parse`

Parses the specified input string into a timespan object. 
