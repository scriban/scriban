# Runtime

This document describes the runtime API to manipulate scriban text templating.

Scriban provides a **safe runtime**, meaning it doesn't expose any .NET objects that haven't been made explicitly available to a Template. 

The runtime is composed of two main parts:

- The **parsing/compiler** infrastructure that is responsible for parsing a text template and build a runtime representation of it (we will call this an `Abstract Syntax Tree`)
- The **rendering/evaluation** infrastructure that is responsible to render a compiled template to a string. We will see also that we can evaluate expressions without rendering.

The scriban runtime was designed to provide an easy, powerful and extensible infrastructure. For example, we are making sure that nothing in the runtime is using a static, so that you can correctly overrides all the behaviors of the runtime.

- [Parsing a template](#parsing-a-template)
  - [Parsing modes](#parsing-modes)
- [Rendering a template](#rendering-a-template)
  - [Overview](#overview)
  - [The <code>TemplateContext</code> execution model](#the-templatecontext-execution-model)
  - [The <code>ScriptObject</code>](#the-scriptobject)
  - [The stack of <code>ScriptObject</code>](#the-stack-of-scriptobject)
- [Advanced usages](#advanced-usages)
  - [Member renamer](#member-renamer)
  - [Include and Template Loader](#include-and-itemplateloader)
  - [The Lexer and Parser](#the-lexer-and-parser)
  - [Abstract Syntax Tree](#abstract-syntax-tree)
  - [Extending <code>TemplateContext</code>](#extending-templatecontext)
  - [<code>ScriptObject</code> advanced usages](#scriptobject-advanced-usages)
    - [Advanced custom functions](#advanced-custom-functions)
    - [Hyper custom functions<code>IScriptCustomFunction</code>](#hyper-custom-functionsiscriptcustomfunction)

[:top:](#runtime)
## Parsing a template

The `Scriban.Template` class is a main entry point to easily parse a template and renders it. The action of parsing consist of compiling the template to a faster runtime representation, suitable later for rendering the template.

This class is mostly a user friendly frontend to the underlying classes used to parse a template. See [The Lexer and Parser](#TODO) section for advanded usages.

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
- `bool HasErrors {get;}` to check if the parsed template has any errors. In that case, the `ScriptPage Page` property is null.
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

- **Accessing as regular dictionary objects**:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("var1", "Variable 1");
  ```

- **Imports a delegate** via `ScriptObject.Import(member, Delegate)`. Here we import a `Func<string>`:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Import("myfunc", new Func<string>(() => "Yes"));
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is myfunc: `{{myfunc}}`");
  template.Render(context);
  
  // Prints: This is myfunc: `Yes`
  Console.WriteLine(context.Output.ToString());
  ```

- **Imports functions from a .NET class**:

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

  This function can be imported into a ScriptObject via `ScriptObject.Import(typeof(MyFunctions))`:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Import(typeof(MyFunctions));
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is MyFunctions.Hello: `{{hello}}`");
  template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(context.Output.ToString());
  ```

- **Automatic import** of all functions and members when **inheriting** from a `ScriptObject`:

  ``` C#
  // We simply inherit from ScriptObject
  // All functions defined in the object will be imported
  public class MyCustomFunctions : ScriptObject
  {
      public static string Hello()
      {
          return "hello from method!";
      }
  }
  ```

  Then using directly this custom `ScriptObject` as a regular object:

  ```C#
  var scriptObject1 = new MyCustomFunctions();
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is MyFunctions.Hello: `{{hello}}`");
  template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(context.Output.ToString());
  ```

- Accessing **nested** `ScriptObject`

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("subObject", new ScriptObject());
  ```

- **Imports `ScriptObject` instance** into another instance

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("var1", "Variable 1");

  var scriptObject2 = new ScriptObject();
  scriptObject2.Add("var2", "Variable 2");
  
  // After this command, scriptObject2 contains var1 and var2
  // But modifying var2 on scriptObject2 will not modify var2 on scriptObject1!
  scriptObject2.Import(scriptObject1);
  ```

- **Imports a .NET object instance** into another `ScriptObject`

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
  template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(context.Output.ToString());
  ```

  > You will notice that the members of a .NET object are exposed using only lowercase characters and introducing `_` whenever there is a uppercase character. It means that by default the string `MyMethodIsNice`  will be exposed `my_method_is_nice`. This is done via a member renamer delegate. You can setup a member renamer when importing an existing .NET object but also a default member renamer on the `TemplateContext`. See [Member renamer](#member-renamer) in advanced usages about this topic.

- **Accessing a .NET object** through a `ScriptObject`

  This is an important feature of scriban. Every .NET objects made accessible through a ScriptObject is directly accessible without importing it. It means that Scriban will directly work on the .NET object instance instead of a copy (e.g when we do a `ScriptObject.Import` instead)

  For example, if we re-use the previous `MyObject` directly as a variable in a `ScriptObject`:

  ```C#
  var scriptObject1 = new ScriptObject();
  scriptObject1["myobject"] = new MyObject();
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{myobject.hello}}`");
  template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(context.Output.ToString());
  ```

- Using **read-only properties** on a `ScriptObject`:

  ```C#
  var scriptObject1 = new ScriptObject();
  // The variable `var1` is immutable
  scriptObject1.SetValue("var1", "My immutable variable", true);

  // Or or an existing property/function member:
  scriptObject1.SetReadonly("var1", true);
  ```

For example, all builtin functions object of Scriban are imported easily why inheriting `ScriptObject`:

- The `BuilinsFunctions` object defined [here](https://github.com/lunet-io/scriban/blob/8b374ffde418b8b57714e3be145a66d3085f66e6/src/Scriban/Functions/BuiltinFunctions.cs) and [listed here](https://github.com/lunet-io/scriban/tree/master/src/Scriban/Functions) is directly used as the bottom level stack `ScriptObject` as explained below.
- Each sub function objects (e.g `array`, `string`) are also regular `ScriptObject`. For example, the [`string` builtin functions](https://github.com/lunet-io/scriban/blob/8b374ffde418b8b57714e3be145a66d3085f66e6/src/Scriban/Functions/StringFunctions.cs)

See section about [ScriptObject advanced usages](#scriptobject-advanced-usages) also for more specific usages.

[:top:](#runtime)
### The stack of `ScriptObject`

A `TemplateContext` maintains a stack of `ScriptObject` that defines the state of the variables accessible from the current template. 

When evaluating a template and **resolving a variable**, the `TemplateContext` will lookup to the stack of `ScriptObject` for the specified variable. The first variable found will be returned.

By default, the `TemplateContext` is initialized with a builtin `ScriptObject` which contains all the default builtin functions provided by scriban. You can pass your own builtin object if you want when creating a new `TemplateContext`.

Then, each time you do a `TemplateContext.PushGlobal(scriptObject)`, you push a new `ScriptObject` accessible for ** resolving variable**

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
template.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2"
Console.WriteLine(context.Output.ToString());
```
The `TemplateContext` stack is setup like this:  `scriptObject2` => `scriptObject1` => `builtins`

As you can see the variable `var1` will be resolved from `scriptObject1` but the variable `var2` will be resolved from `scriptObject2` as there is an override here.

When writing to a variable, only the `ScriptObject` at the top of the `TemplateContext` will be used. It the previous example, if we had something like this in a template:

```C#
var template2 = Template.Parse("This is var1: `{{var1}}` and var2: `{{var2}}`{{var2 = 5}} and new var2: `{{var2}}");

template2.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2 and new var2: `5`"
Console.WriteLine(context.Output.ToString());
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

When using the `with` statement with a script object, it is relying on this concept of stack:

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
template.Render(context);
}}");

template.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2 and new var2: `5`"
Console.WriteLine(context.Output.ToString());
```

[:top:](#runtime)
## Advanced usages

### Member renamer

By default, .NET objects accessed through a `ScriptObject` are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of `liquid` templates.

A renamer is simply a delegate that takes an input string (that is a member name) an return a new member name:

```C#
namespace Scriban.Runtime
{
    public delegate string MemberRenamerDelegate(string member);
}
```

The [`StandardMemberRenamer`](https://github.com/lunet-io/scriban/blob/d5d0423b0ab587cb67253812a9355c85361096e4/src/Scriban/Runtime/StandardMemberRenamer.cs) is used to convert string camel/pascal case strings to "ruby" like strings.

If you want to import a .NET object without changing the cases, you can use the simple nop member renamer `member => member`.

Note that renaming can be changed at two levels:

- When importing a .NET object into a `ScriptObject` by passing a renamer delegate, before passing an object to a `TemplateContext`:

  ```C#
  var scriptObject1 = new ScriptObject();
  // Here the renamer will just return a same member name as the original
  // hence importing .NET member name as-is
  scriptObject1.Import(new MyObject(), renamer: member => member);
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{Hello}}`");
  template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(context.Output.ToString());
  ```
- By setting the default member renamer on the `TemplateContext`

  ```C#
  // Setup a default renamer at the `TemplateContext` level
  var context = new TemplateContext {MemberRenamer = member => member};
  ```

  It is important to setup this on the `TemplateContext` for any .NET objects that might be accessed indirectly through another `ScriptObject` so that when a .NET object is exposed, it is exposed with the correct naming conventions. 

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
### Extending `TemplateContext`

You may need to extend a `TemplateContext` to overrides some methods there, tyically in cases you want:

- To hook into whenever a `ScriptNode` AST node is evaluated
- To catch if a propery/member is accessed and should not be null
- Provides a `IObjectAccessor` for non .NET, non `Dictionary<string, object>` in case you are looking to expose a specific object to the runtime that requires a specific access pattern. By overriding the method `GetMemberAccessorImpl` you can override this aspect.
- To override `ToString(span, object)` method to provide custom `ToString` for specifics .NET objects.
- ...etc.

[:top:](#runtime)
### `ScriptObject` advanced usages

It is sometimes required for a custom function to have access to the current `TemplateContext` or to tha access to original location of the text code, where a particular expression is occurring (via a `SourceSpan` that gives a `line`, `column` and `sourcefile` )

#### Advanced custom functions

In the [`ScriptObject`](#the-ScriptObject) section we described how to easily import a custom function either by using a delegate or a pre-defined .NET static/instance functions.

In some cases, you also need to have acccess to the current `TemplateContext` and also, the current `SourceSpan` (original location position in the text template code).
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
- The AST node context from the `Template` that is calling this custom functions
- The parameters - not yet evaluated, so you can precisely get information about the location of the parameters in the original source code...etc.

The `include` expression is typically implemented via a `IScriptCustomFunction`. You can have a look at the details [here](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Functions/IncludeFunction.cs)

[:top:](#runtime)

