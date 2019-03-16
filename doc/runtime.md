# Runtime

This document describes the runtime API to manipulate scriban text templating.

Scriban provides a **safe runtime**, meaning it doesn't expose any .NET objects that haven't been made explicitly available to a Template. 

The runtime is composed of two main parts:

- The **parsing/compiler** infrastructure that is responsible for parsing a text template and build a runtime representation of it (we will call this an `Abstract Syntax Tree`)
- The **rendering/evaluation** infrastructure that is responsible to render a compiled template to a string. We will see also that we can evaluate expressions without rendering.

The scriban runtime was designed to provide an easy, powerful and extensible infrastructure. For example, we are making sure that nothing in the runtime is using a static, so that you can correctly override all the behaviors of the runtime.

- [Parsing a template](#parsing-a-template)
  - [Parsing modes](#parsing-modes)
  - [Liquid support](#liquid-support)
- [Rendering a template](#rendering-a-template)
  - [Overview](#overview)
  - [The <code>TemplateContext</code> execution model](#the-templatecontext-execution-model)
  - [The <code>ScriptObject</code>](#the-scriptobject)
    - [Accessing as regular dictionary objects](#accessing-as-regular-dictionary-objects)
    - [Imports a .NET delegate](#imports-a-net-delegate)
    - [Imports functions from a .NET class](#imports-functions-from-a-net-class)
    - [Automatic functions import from <code>ScriptObject</code>](#automatic-functions-import-from-scriptobject)
    - [Function arguments, optional and <code>params</code>](#function-arguments-optional-and-params)
    - [Accessing nested <code>ScriptObject</code>](#accessing-nested-scriptobject)
    - [Imports a <code>ScriptObject</code> into another <code>ScriptObject</code>](#imports-a-scriptobject-into-another-scriptobject)
    - [Imports a .NET object instance](#imports-a-net-object-instance)
    - [Accessing a .NET object](#accessing-a-net-object)
    - [read-only properties](#read-only-properties)
    - [The builtin functions](#the-builtin-functions)
  - [The stack of <code>ScriptObject</code>](#the-stack-of-scriptobject)
    - [The <code>with</code> statement with the stack](#the-with-statement-with-the-stack)
- [Advanced usages](#advanced-usages)
  - [Member renamer](#member-renamer)
  - [Member filter](#member-filter)
  - [Include and <code>ITemplateLoader</code>](#include-and-itemplateloader)
  - [The Lexer and Parser](#the-lexer-and-parser)
  - [Abstract Syntax Tree](#abstract-syntax-tree)
    - [AST to Text](#ast-to-text)
  - [Extending <code>TemplateContext</code>](#extending-templatecontext)
  - [<code>ScriptObject</code> advanced usages](#scriptobject-advanced-usages)
    - [Advanced custom functions](#advanced-custom-functions)
    - [Hyper custom functions<code>IScriptCustomFunction</code>](#hyper-custom-functionsiscriptcustomfunction)
  - [Evaluating an expression](#evaluating-an-expression)
  - [Changing the Culture](#changing-the-culture)
  - [Safe Runtime](#safe-runtime)
      
[:top:](#runtime)
## Parsing a template

The `Scriban.Template` class is a main entry point to easily parse a template and renders it. The action of parsing consist of compiling the template to a faster runtime representation, suitable later for rendering the template.

This class is mostly a user friendly frontend to the underlying classes used to parse a template. See [The Lexer and Parser](#the-lexer-and-parser) section for advanded usages.

The `Template.Parse ` method is a convenient method to parse a template from a string and returns the compiled Template:

```c#
var inputTemplateAsText = "This is a {{ name }} template";

// Parse the template
var template = Template.Parse(inputTemplateAsText);

// Check for any errors
if (template.HasErrors)
{
    foreach(var error in template.Messages)
    {
        Console.WriteLine(error);
    }
    return; // or throw...etc.
}
```

The returned `Template` object has the following relevant properties:

- `ScriptPage Page {get;}` that contains the compiled template to a root Abstract Syntax Tree (AST). From this object you can navigate through all the statements parsed from the template if necessary. See the section about the [Abstract Syntax Tree](#abstract-syntax-tree)
- `bool HasErrors {get;}` to check if the parsed template has any errors. In that case, the `ScriptPage Page` property is `null`.
- `List<LogMessage> Messages {get;}` contains the list of warning and error messages while parsing the template.

If you are using the `Template.Parse` method, it is important to verify `HasErrors` is `false`, otherwise you will get a null `ScriptPage` object from the `Template.Page` property.

The parse method can take an additional argument `sourceFilePath` used when reporting syntax errors, typically used to associate a template file read from the disk or an editor and you want to report the exact error to the user.

```c#
// Parse the template
var template = Template.Parse(File.ReadAllText(filePath), filePath);
```

> Note that the `sourceFilePath` is not used for accessing the disk (it could be a logical path to a zip file, or the name of tab opened in an editor...etc.). It is only a logical name that is used when reporting errors, but also you will see with the include directive and the setup of the [Template Loader](#include-and-itemplateloader) that this value can be used to perform an include operation in the relative context to the template path being processed.

[:top:](#runtime)
### Parsing modes

By default, when parsing a template, the template is expected to have mixed content of text and scriban code blocks enclosed by `{{` and `}}`. But you can modify the way a template is parsed by passing a `LexerOptions` to the `Template.Parse` method.

The parsing mode is defined by the `LexerOptions.Mode` property which is `ScriptMode.Default` by default (i.e. mixed text and code).

But you can also parse a template that contains directly scripting code (without enclosing `{{` `}}`), in that case, you can use the `ScriptMode.ScriptOnly` mode.

For example illustrate how to use the `ScriptOnly` mode:

```c#
// Create a template in ScriptOnly mode
var lexerOptions = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
// Notice that code is not enclosed by `{{` and `}}`
var template = Template.Parse("y = x + 1; y;", lexerOptions: lexerOptions);
// Renders it with the specified parameter
var result = template.Evaluate(new {x = 10});
// Prints 11
Console.WriteLine(result);
```

> Note: As we will see in the following section about rendering, you can also avoid rendering a script only mode by evaluating the template instead of rendering. 

[:top:](#runtime)
### Liquid support

Scriban supports a Lexer and Parser that can understand a Liquid template instead, while still translating it to a Scriban Runtime AST.

You can easily parse an existing liquid template using the `Template.ParseLiquid` method:

```c#
// An Liquid 
var inputTemplateAsText = "This is a {{ name }} template";

// Parse the template
var template = Template.ParseLiquid(inputTemplateAsText);

// Renders the template with the variable `name` exposed to the template
var result = template.Render(new { name = "Hello World"});

// Prints the result: "This is a Hello World template"
Console.WriteLine(result);
```

Also, in terms of runtime, Liquid builtin functions are supported. They are created with the `LiquidTemplateContext` which inherits from the `TemplateContext`.

[:top:](#runtime)
## Rendering a template

### Overview

In order to render a template, you need pass a context for the variables, objects, functions that will be accessed by the template.

In the following examples, we have a variable `name` that is used by the template:

```c#
var inputTemplateAsText = "This is a {{ name }} template";

// Parse the template
var template = Template.Parse(inputTemplateAsText);

// Renders the template with the variable `name` exposed to the template
var result = template.Render(new { name = "Hello World"});

// Prints the result: "This is a Hello World template"
Console.WriteLine(result);
```

As we can see, we are passing an anonymous objects that has this field/property `name` and by calling the Render method, the template is executed with this data model context.

While passing an anonymous object is nice for a hello world example, it is not always enough for more advanced data model scenarios. 

In this case, you want to use more directly the `TemplateContext` (used by the method `Template.Render(object)`) and a `ScriptObject` which are both at the core of scriban rendering architecture to provide more powerful constructions & hooks of the data model exposed (variables but also functions...etc.).

[:top:](#runtime)
### The `TemplateContext` execution model

The `TemplateContext` provides:

- **an execution context** when evaluating a template. The same instance can be used with many different templates, depending on your requirements.
- A **stack of `ScriptObject`** that provides the actual variables/functions accessible to the template, accessible through `Template.PushGlobal(scriptObj)` and `Template.PopGlobal()`. Why a stack and how to use this stack is described below.
- The **text output** when evaluating a template, which is accessible through the `Template.Output` property as a `StringBuilder` but because you can have nested rendering happening, it is possible to use `Template.PushOutput()` and `Template.PopOutput()` to redirect temporarily the output to a new output. This functionality is typically used by the [`capture` statement](language.md#94-capture-variable--end).
- Caching of templates previously loaded by an `include` directive (see [`include` and `ITemplateLoader`](#include-and-itemplateloader) section )
- Various possible overrides to allow fine grained extensibility (evaluation of an expression, conversion to a string, enter/exit/step into a loop...etc.)

Note that a `TemplateContext` is not thread safe, so it is recommended to have one `TemplateContext` per thread.

[:top:](#runtime)
### The `ScriptObject`

The `ScriptObject` is a special implementation of a `Dictionary<string, object>` that runtime properties and functions accessible to a template:

#### Accessing as regular dictionary objects

A `ScriptObject` is mainly an extended version of a `IDictionary<string, object>`:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("var1", "Variable 1");

  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is var1: `{{var1}}`");
  var result = template.Render(context);
  
  // Prints: This is var1: `Variable 1`
  Console.WriteLine(result);
  ```

Note that any `IDictionary<string, object>` put as a property will be accessible as well.

#### Imports a .NET delegate

Via `ScriptObject.Import(member, Delegate)`. Here we import a `Func<string>`:

  ```C#
  var scriptObject1 = new ScriptObject();
  // Declare a function `myfunc` returning the string `Yes`
  scriptObject1.Import("myfunc", new Func<string>(() => "Yes"));
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is myfunc: `{{myfunc}}`");
  var result = template.Render(context);
  
  // Prints: This is myfunc: `Yes`
  Console.WriteLine(result);
  ```

#### Imports functions from a .NET class

You can easily import static methods declared in a .NET class via `ScriptObject.Import(typeof(MyFunctions))`

Let's define a class with a static function `Hello`:

  ```C#
  public static class MyFunctions
  {
      public static string Hello()
      {
          return "hello from method!";
      }
  }
  ```

This function can be imported into a ScriptObject:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Import(typeof(MyFunctions));
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is MyFunctions.Hello: `{{hello}}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```

> Notice that when using a function with pipe calls like `{{description | string.strip }}``, the last argument passed to the `string.strip` function is the result of the previous pipe.
> That's a reason why you will notice in all builtin functions in scriban that they usually take the most relevant parameter as a last parameter instead of the first parameter, to allow proper support for pipe calls.

> **NOTICE**
>
> By default, Properties and static methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](#member-renamer) delegate

#### Automatic functions import from `ScriptObject`

When inheriting from a `ScriptObject`, the inherited object will automatically import all public static methods and properties from the class:

  ``` C#
  // We simply inherit from ScriptObject
  // All functions defined in the object will be imported
  public class MyCustomFunctions : ScriptObject
  {
      public static string Hello()
      {
          return "hello from method!";
      }

      [ScriptMemberIgnore] // This method won't be imported
      public static void NotImported()
      {
          // ...
      }
  }
  ```

Then using directly this custom `ScriptObject` as a regular object:

  ```C#
  var scriptObject1 = new MyCustomFunctions();
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is MyFunctions.Hello: `{{hello}}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```

Notice that if you want to ignore a member when importing a .NET object or .NET class, you can use the attribute `ScriptMemberIgnore`

> NOTE: Because Scriban doesn't support Function overloading, it is required that functions imported from a type must have different names.

> **NOTICE**
>
> By default, Properties and methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](#member-renamer) delegate

#### Function arguments, optional and `params`

Scriban runtime supports regular function arguments, optional arguments (with a default value) and `params XXX[] array` arguments:

  ``` C#
  // We simply inherit from ScriptObject
  // All functions defined in the object will be imported
  public class MyCustomFunctions : ScriptObject
  {
      // A function an optional argument
      public static string HelloOpt(string text, string option = null)
      {
          return $"hello {text} with option:{option}";
      }

      // A function with params
      public static string HelloArgs(params object[] args)
      {
          return $"hello {(string.Join(",", args))}";
      }
  }
  ```

Using the function above from a script could be like this:

> **input**
```scriban-html
{{ hello_opt "test" }}
{{ hello_opt "test" "my_option" }}
{{ hello_opt "test" option: "my_option" }}
{{ hello_opt text: "test"  }}
{{ hello_args "this" "is" "a" "test"}}
{{ hello_args "this" "is" args: "a" args: "test"}}
```

> **output**
```scriban-html
hello test with option:
hello test with option:my_option
hello test with option:my_option
hello test with option:
hello this,is,a,test
hello this,is,a,test
```

Notice that we can have a mix of regular and named arguments, assuming that named arguments are always coming last when calling a function.

Also, we can see that named arguments are also working with `params` arguments.

If a regular argument (not optional) is missing, the runtime will complain about the missing argument giving precise source location of the error.

#### Accessing nested `ScriptObject`

A nested ScriptObject can be accessed indirectly through another `ScriptObject`:

  ```C#
  var scriptObject1 = new ScriptObject();
  var nestedObject = new ScriptObject();
  nestedObject["x"] = 5;
  scriptObject1.Add("subObject", nestedObject);

  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{subObject.x}}`");
  template.Render(context);

  ```

#### Imports a `ScriptObject` into another `ScriptObject`

The properties/functions of a `ScriptObject` can be imported into another instance.

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("var1", "Variable 1");

  var scriptObject2 = new ScriptObject();
  scriptObject2.Add("var2", "Variable 2");
  
  // After this command, scriptObject2 contains var1 and var2
  // But modifying var2 on scriptObject2 will not modify var2 on scriptObject1!
  scriptObject2.Import(scriptObject1);
  ```

#### Imports a .NET object instance

You can easily import a .NET object instance (including its public properties and static methods) into a `ScriptObject`

NOTE that when importing into a ScriptObject, the **import actually copies the property values into the ScriptObject**. The original .NET object is no longer used.

Importing a .NET object instance is thus different from [accessing a .NET object](#accessing-a-net-object) instance through a ScriptObject.

Let's define a standard .NET object:

  ```C#
  public class MyObject
  {
      public MyObject()
      {
          Hello = "hello from property!";
      }
  
      public string Hello { get; set; }
  }
  ```

and import the properties/functions of this object into a ScriptObject, via `ScriptObject.Import(object)`:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Import(new MyObject());
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{hello}}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```


Also any objects inheriting from `IDictionary<TKey, TValue>` or `IDictionary` will be also accessible automatically. Typically, you can usually access directly any generic JSON objects that was parsed by a JSON library.

> **NOTICE**
>
> By default, Properties and static methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](#member-renamer) delegate

#### Accessing a .NET object

This is an important feature of scriban. Every .NET objects made accessible through a ScriptObject is directly accessible without importing it. It means that Scriban will directly work on the .NET object instance instead of a copy (e.g when we do a `ScriptObject.Import` instead)

> Note that for security reason, only the properties of .NET objects accessed through another `ScriptObject` are made accessible from a Template. Methods and static methods are not automatically imported.

For example, if we re-use the previous `MyObject` directly as a variable in a `ScriptObject`:

  ```C#
  var scriptObject1 = new ScriptObject();
  // Notice: MyObject is not imported but accessible through
  // the variable myobject
  scriptObject1["myobject"] = new MyObject();
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{myobject.hello}}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```

> **NOTICE**
>
> By default, Properties and static methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](#member-renamer) delegate

#### read-only properties

Runtime equivalent of the language `readonly <var>` statement, you can easily define a variable of a `ScriptObject` as read-only

  ```C#
  var scriptObject1 = new ScriptObject();
  // The variable `var1` is immutable
  scriptObject1.SetValue("var1", "My immutable variable", true);

  // Or or an existing property/function member:
  scriptObject1.SetReadonly("var1", true);
  ```

#### The builtin functions

For example, all builtin functions object of Scriban are imported easily by inheriting from a `ScriptObject`:

- The `BuilinsFunctions` object defined [here](https://github.com/lunet-io/scriban/blob/8b374ffde418b8b57714e3be145a66d3085f66e6/src/Scriban/Functions/BuiltinFunctions.cs) and [listed here](https://github.com/lunet-io/scriban/tree/master/src/Scriban/Functions) is directly used as the bottom level stack `ScriptObject` as explained below.
- Each sub function objects (e.g `array`, `string`) are also regular `ScriptObject`. For example, the [`string` builtin functions](https://github.com/lunet-io/scriban/blob/8b374ffde418b8b57714e3be145a66d3085f66e6/src/Scriban/Functions/StringFunctions.cs)

The current builtin `ScriptObject` defined for a `TemplateContext` is accessible through the `TemplateContext.BuiltinObject` property.

See section about [ScriptObject advanced usages](#scriptobject-advanced-usages) also for more specific usages.

[:top:](#runtime)
### The stack of `ScriptObject`

A `TemplateContext` maintains a stack of `ScriptObject` that defines the state of the variables accessible from the current template. 

When evaluating a template and **resolving a variable**, the `TemplateContext` will lookup to the stack of `ScriptObject` for the specified variable. From the top of the stack (the latest `PushGlobal`) to the bottom of the stack, when a variable is accessed from a template, the closest variable in the stack will be returned.

By default, the `TemplateContext` is initialized with a builtin `ScriptObject` which contains all the default builtin functions provided by scriban. You can pass your own builtin object if you want when creating a new `TemplateContext`.

Then, each time you do a `TemplateContext.PushGlobal(scriptObject)`, you push a new `ScriptObject` accessible for **resolving variable**

Let's look at the following example:

```C#
// Creates scriptObject1
var scriptObject1 = new ScriptObject();
scriptObject1.Add("var1", "Variable 1");
scriptObject1.Add("var2", "Variable 2");

// Creates scriptObject2
var scriptObject2 = new ScriptObject();
// overrides the variable "var2" 
scriptObject2.Add("var2", "Variable 2 - from ScriptObject 2");

// Creates a template with (builtins) + scriptObject1 + scriptObject2 variables
var context = new TemplateContext();
context.PushGlobal(scriptObject1);
context.PushGlobal(scriptObject2);

var template = Template.Parse("This is var1: `{{var1}}` and var2: `{{var2}}");
var result = template.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2"
Console.WriteLine(result);
```

The `TemplateContext` stack is setup like this:  `scriptObject2` => `scriptObject1` => `builtins`

As you can see the variable `var1` will be resolved from `scriptObject1` but the variable `var2` will be resolved from `scriptObject2` as there is an override here.

> **NOTE** If a variable is not found, the runtime will not throw an error but will return `null` instead. It allows to check for a variable existence `if !page` for example. In case you want your script to throw an exception if a variable was not found, you can specify `TemplateContext.StrictVariables = true` to enforce checks. See the [safe runtime](#safe-runtime) section for more details.

When writing to a variable, only the `ScriptObject` at the top of the `TemplateContext` will be used. This top object is accessible through `TemplateContext.CurrentGlobal` property. It the previous example, if we had something like this in a template:

```C#
var template2 = Template.Parse("This is var1: `{{var1}}` and var2: `{{var2}}`{{var2 = 5}} and new var2: `{{var2}}");

var result = template2.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2 and new var2: `5`"
Console.WriteLine(result);
```

The `scriptObject2` object will now contain the `var2 = 5`

The stack provides a way to segregate variables between their usages or read-only/accessibility/mutability requirements. Typically, the `builtins` `ScriptObject` is a normal `ScriptObject` that contains all the builtins objects but you cannot modify directly the `builtins` object. But you could modify the sub-builtins objects.

For example, the following code adds a new property `myprop` to the builtin object `string`:

```
{{
   string.myprop = "Yoyo"
}}
```

Because scriban allows you to define new functions directly into the language and also allow to store a function pointer by using the alias `@` operator, you can basically extend an existing object with both properties and functions.

#### The `with` statement with the stack

When using the `with` statement with a script object, it is relying on this concept of stack:

- `with <scriptobject>` is equivalent of calling `TemplateContext.PushGlobal(scriptObject)`
- Assigning a variable enclosed by a `with` statement will set variable on the target object of the `with` statement.
- Ending a with is equivalent of calling `context.PopGlobal()`

```c#
var scriptObject1 = new ScriptObject();
var context = new TemplateContext();
context.PushGlobal(scriptObject1);

var template = Template.Parse(@"
   Create a variable 
{{
    myvar = {} 
    with myvar   # Equivalent of calling context.PushGlobal(myvar)
        x = 5    # Equivalent to set myvar.x = 5
        y = 6    
    end          # Equivalent of calling context.PopGlobal()
}}");

template.Render(context);

// Contains 5
Console.WriteLine(((ScriptObject)scriptObject1["myvar"])["x"]);
```

[:top:](#runtime)
## Advanced usages

### Member renamer

By default, .NET objects accessed through a `ScriptObject` are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of `liquid` templates.

A renamer is simply a delegate that takes an input MemberInfo and return a new member name:

```C#
namespace Scriban.Runtime
{
    public delegate string MemberRenamerDelegate(MemberInfo member);
}
```

The [`StandardMemberRenamer`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/StandardMemberRenamer.cs) is used to convert string camel/pascal case strings to "ruby" like strings.

If you want to import a .NET object without changing the cases, you can use the simple nop member renamer `member => member.Name`.

Note that renaming can be changed at two levels:

- When importing a .NET object into a `ScriptObject` by passing a renamer delegate, before passing an object to a `TemplateContext`:

  ```C#
  var scriptObject1 = new ScriptObject();
  // Here the renamer will just return a same member name as the original
  // hence importing .NET member name as-is
  scriptObject1.Import(new MyObject(), renamer: member => member.Name);
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{Hello}}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```
- By setting the default member renamer on the `TemplateContext`

  ```C#
  // Setup a default renamer at the `TemplateContext` level
  var context = new TemplateContext {MemberRenamer = member => member.Name};
  ```

  It is important to setup this on the `TemplateContext` for any .NET objects that might be accessed indirectly through another `ScriptObject` so that when a .NET object is exposed, it is exposed with the correct naming convention. 

The method `Template.Render(object, renamer)` takes also a member renamer, imports the object model with the renamer and setup correctly the renamer on the underlying `TemplateContext`.

So you can rewrite the previous example with the shorter version:

```C#
var template = Template.Parse("This is Hello: `{{Hello}}`");
template.Render(new MyObject(), member => member.Name);
```

[:top:](#runtime)
### Member filter

Similar to the member renamer, by default, .NET objects accessed through a `ScriptObject` are automatically exposing all public instance fields and properties of .NET objects.

A filter is simply a delegate that takes an input MemberInfo and return a boolean to indicate whether to expose the member (`true`) or discard the member (`false`)

```C#
namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to filter a member while importing a .NET object into a ScriptObject 
    /// or while exposing a .NET instance through a ScriptObject, 
    /// by returning <c>true</c> to keep the member; or false to discard it.
    /// </summary>
    /// <param name="member">A member info</param>
    /// <returns><c>true</c> to keep the member; otherwise <c>false</c> to remove the member</returns>
    public delegate bool MemberFilterDelegate(MemberInfo member);
}
```

- You can use a MemberFilter when importing a an instance:

  ```C#
  var scriptObject1 = new ScriptObject();
  // Imports only properties that contains the word "Yo"
  scriptObject1.Import(new MyObject(), filter: member => member is PropertyInfo && member.Name.Contains("Yo"));
  ```
- By setting the default member filter on the `TemplateContext`, so that .NET objects automatically exposed via a `ScriptObject` will follow the global filtering rules defined on the context:

  ```C#
  // Setup a default filter at the `TemplateContext` level
  var context = new TemplateContext {MemberFilter = member => member is PropertyInfo && member.Name.Contains("Yo") };
  ```

As for the member renamer, it is important to setup this on the `TemplateContext` for any .NET objects that might be accessed indirectly through another `ScriptObject` so that when a .NET object is exposed, it is exposed with the same filtering convention 

[:top:](#runtime)
### Include and `ITemplateLoader`

The `include` directives requires that a template loader is setup on the `TemplateContext.TemplateLoader` property

A template loader is responsible for providing the text template from an include directive. The interface of a [`ITemplateLoader`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/ITemplateLoader.cs) is defined like this:

```C#
/// <summary>
/// Interface used for loading a template.
/// </summary>
public interface ITemplateLoader
{
    /// <summary>
    /// Gets an absolute path for the specified include template name. Note that it is not necessarely a path on a disk, 
    /// but an absolute path that can be used as a dictionary key for caching)
    /// </summary>
    /// <param name="context">The current context called from</param>
    /// <param name="callerSpan">The current span called from</param>
    /// <param name="templateName">The name of the template to load</param>
    /// <returns>An absolute path or unique key for the specified template name</returns>
    string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName);

    /// <summary>
    /// Loads a template using the specified template path/key.
    /// </summary>
    /// <param name="context">The current context called from</param>
    /// <param name="callerSpan">The current span called from</param>
    /// <param name="templatePath">The path/key previously returned by <see cref="GetPath"/></param>
    /// <returns>The content string loaded from the specified template path/key</returns>
    string Load(TemplateContext context, SourceSpan callerSpan, string templatePath);
}
```

In order to use the `include` directive, the template loader should provide:

- The `GetPath` method translates a `templateName` (the argument passed to the `include <templateName>` directive) to a logical/phyisical path that the `ITemplateLoader.Load` method will understand. 
- The `Load` method to actually load the the text template code from the specified `templatePath` (previously returned by `GetPath` method)


The 2 step methods, `GetPath` and then `Load` allows to cache intermediate results. If a template loader returns the same `template path` for a `template name` any existing cached templates will be returned instead. Cached templates are stored in the `TemplateContext.CachedTemplates` property.

A typical implementation of `ITemplateLoader` could read data from the disk:

```C#

```C#
/// <summary>
/// A very simple ITemplateLoader loading directly from the disk, without any checks...etc.
/// </summary>
public class MyIncludeFromDisk : ITemplateLoader
{
    string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return Path.Combine(Environment.CurrentDirectory, templateName);
    }

    string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        // Template path was produced by the `GetPath` method above in case the Template has 
        // not been loaded yet
        return File.ReadAllText(templatePath);
    }
}
```

[:top:](#runtime)
### The Lexer and Parser

- The [`Lexer`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/Lexer.cs) class is responsible for extracting `Tokens` from a text template.
- The [`Parser`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/Parser.cs) class is responsible for creating `ScriptNode` AST from input tokens (extracted from the `Lexer`)

The lexer has a few [`LexerOptions`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/LexerOptions.cs) to control the way the lexer is behaving, as described with the [parsing modes](#parsing-modes)

The parser has a [`ParserOptions`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/ParserOptions.cs) only used for securing nested statements/blocks to avoid any stack overflow exceptions while parsing a document.

[:top:](#runtime)
### Abstract Syntax Tree

The base object used by the syntax for all scriban elements is the class `Scriban.Syntax.ScriptNode`:

```c#
/// <summary>
/// Base class for the abstract syntax tree of a scriban program.
/// </summary>
public abstract class ScriptNode
{
    /// <summary>
    /// The source span of this node.
    /// </summary>
    public SourceSpan Span;

    /// <summary>
    /// Evaluates this instance with the specified context.
    /// </summary>
    /// <param name="context">The template context.</param>
    public abstract object Evaluate(TemplateContext context);
}
```

As you can see, each `ScriptNode` contains a method to evaluate it against a `TemplateContext`. You can go through the all the [Syntax classes](https://github.com/lunet-io/scriban/tree/master/src/Scriban/Syntax) in the codebase and you will see that it is very easy to create a new `SyntaxNode`

[:top:](#runtime)
#### AST to Text

Scriban allows to write back an AST to a textual representation:

```C#
var template = Template.Parse("This is a {{ name }} template");

// Prints "This is a {{name}} template"
Console.WriteLine(template.ToText());
```

In the previous example, you can notice that whitespace were removed from the original template. The reason is by default, the parser doesn't keep all hidden symbols when parsing, to still allow fast parsing for the regular case.

But you can specify the parser to keep all the hidden symbols from the original template, directly by activating the `IsKeepTrivia` on the `LexerOptions`

In the following example, you can see that it keep all the whitespace and comment:

```C#
// Specifying the KeepTrivia allow to keep as much as hidden symbols from the original template (white spaces, newlines...etc.)
var template = Template.Parse(@"This is a {{ name   +   ## With some comment ## '' }} template", lexerOptions: new LexerOptions() { KeepTrivia = true });

// Prints "This is a {{ name   +   ## With some comment ## '' }} template"
Console.WriteLine(template.ToText());
```

[:top:](#runtime)
### Extending `TemplateContext`

You may need to extend a `TemplateContext` to overrides some methods there, tyically in cases you want:

- To hook into whenever a `ScriptNode` AST node is evaluated
- To catch if a property/member is accessed and should not be null
- Provides a `IObjectAccessor` for non .NET, non `Dictionary<string, object>` in case you are looking to expose a specific object to the runtime that requires a specific access pattern. By overriding the method `GetMemberAccessorImpl` you can override this aspect.
- To override `ToString(span, object)` method to provide custom `ToString` for specifics .NET objects.
- ...etc.

[:top:](#runtime)
### `ScriptObject` advanced usages

It is sometimes required for a custom function to have access to the current `TemplateContext` or to tha access to original location of the text code, where a particular expression is occurring (via a `SourceSpan` that gives a `line`, `column` and `sourcefile` )

#### Advanced custom functions

In the [`ScriptObject`](#the-ScriptObject) section we described how to easily import a custom function either by using a delegate or a pre-defined .NET static functions/properties.

In some cases, you also need to have access to the current `TemplateContext` and also, the current `SourceSpan` (original location position in the text template code).
By simply adding as a first parameter `TemplateContext`, and optionally as a second parameter, a `SourceSpan` a custom function can have access to the current evaluation context:

```C#
var scriptObject1 = new ScriptObject();
// Here, we can have access to the `TemplateContext`
scriptObject1.Import("contextAccess", new Func<TemplateContext, string>(templateContext => "Yes"));
```

[:top:](#runtime)
#### Hyper custom functions`IScriptCustomFunction` 

Some custom functions can require deeper access to the internals for exposing a function. Scriban provides the interface [`IScriptCustomFunction`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Runtime/IScriptCustomFunction.cs) for this matter. If an object inherits from this interface and is accessed another `ScriptObject`, it will call the method `IScriptCustomFunction.Invoke`.

```C#
namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to create a custom function object.
    /// </summary>
    public interface IScriptCustomFunction
    {
        /// <summary>
        /// Calls the custom function object.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="callerContext">The script node originating this call</param>
        /// <param name="parameters">The parameters of the call</param>
        /// <param name="blockStatement">The current block statement this call is made</param>
        /// <returns>The result of the call</returns>
        object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement);
    }
}
```

As you can see, the `IScriptCustomFunction` gives you access to:

- The current `TemplateContext` evaluating the current `Template`
- The AST node context from the `Template` that is calling this custom functions, so you can precisely get information about the location of the parameters in the original source code...etc.
- The parameters already evaluated
- The block statement (not yet used for custom functions - but used by the `wrap` statement)

The `include` expression is typically implemented via a `IScriptCustomFunction`. You can have a look at the details [here](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Functions/IncludeFunction.cs)

[:top:](#runtime)
### Evaluating an expression

It is sometimes convenient to evaluate a script expression without rendering it to a string.

First, there is an option in `TemplateContext.EnableOutput` that can be set to disable the output to the `TemplateContext.Output` StringBuilder.

Also, as in the [Abstract Syntax Tree](#abstract-syntax-tree) section, all AST `ScriptNode` have an `Evaluate` method that returns the result of an evaluation.

Lastly, you can use the convenient static method `Template.Evaluate` to quickly evaluate an expression relative to a `TemplateContext`:

```C#
var scriptObject1 = new ScriptObject();
scriptObject1.Add("var1", 5);

var context = new TemplateContext();
context.PushGlobal(scriptObject1);

var result = Template.Evaluate("var1 * 5 + 2", context);
// Prints `27`
Console.WriteLine(result);
```
When using `Template.Evaluate`, the underlying code will use the `ScriptMode.ScriptOnly` when compiling the expression and will disable the output on the `TemplateContext`.

[:top:](#runtime)
### Changing the Culture

The default culture when running a template is `CultureInfo.InvariantCulture`

You can change the culture that is used when rendering numbers/date/time and parsing date/time by pushing a new Culture to a `TemplateContext`

```C#
var context = new TemplateContext();
context.PushCulture(CultureInfo.CurrentCulture);
// ...
context.PopCulture();
```

> Notice that the parsing of numbers in the language is not culture dependent but is baked into the language specs instead.

[:top:](#runtime)

### Safe Runtime

The `TemplateContext` provides a few properties to control the runtime and make it safer. You can tweak the following properties:

- `LoopLimit` (default is `1000`): If a script performs a loop over 1000 iteration, the runtime will throw a `ScriptRuntimeException`
- `RecursiveLimit` (default is `100`): If a script performs a recursive call over 100 depth, the runtime will throw a `ScriptRuntimeException`
- `StrictVariables` (default is `false`): If set to `true`, any variables that were not found during variable resolution will throw a `ScriptRuntimeException`
- `RegexTimeOut` (default is `10s`): If a builtin function is using a regular expression that is taking more than 10s to complete, the runtime will throw an exception

[:top:](#runtime)
