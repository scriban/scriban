---
title: "Parsing a template"
---

# Parsing a template

The `Scriban.Template` class is a main entry point to easily parse a template and renders it. The action of parsing consist of compiling the template to a faster runtime representation, suitable later for rendering the template.

This class is mostly a user friendly frontend to the underlying classes used to parse a template. See [The Lexer and Parser](ast#the-lexer-and-parser) section for advanded usages.

The `Template.Parse ` method is a convenient method to parse a template from a string and returns the compiled Template:

```csharp
var inputTemplateAsText = "This is a {{ "{{" }} name {{ "}}" }} template";

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

- `ScriptPage Page {get;}` that contains the compiled template to a root Abstract Syntax Tree (AST). From this object you can navigate through all the statements parsed from the template if necessary. See the section about the [Abstract Syntax Tree](ast#abstract-syntax-tree)
- `bool HasErrors {get;}` to check if the parsed template has any errors. In that case, the `ScriptPage Page` property is `null`.
- `List<LogMessage> Messages {get;}` contains the list of warning and error messages while parsing the template.

If you are using the `Template.Parse` method, it is important to verify `HasErrors` is `false`, otherwise you will get a null `ScriptPage` object from the `Template.Page` property.

The parse method can take an additional argument `sourceFilePath` used when reporting syntax errors, typically used to associate a template file read from the disk or an editor and you want to report the exact error to the user.

```csharp
// Parse the template
var template = Template.Parse(File.ReadAllText(filePath), filePath);
```

> Note that the `sourceFilePath` is not used for accessing the disk (it could be a logical path to a zip file, or the name of tab opened in an editor...etc.). It is only a logical name that is used when reporting errors, but also you will see with the include directive and the setup of the [Template Loader](includes#include-and-itemplateloader) that this value can be used to perform an include operation in the relative context to the template path being processed.


## Parsing modes

By default, when parsing a template, the template is expected to have mixed content of text and scriban code blocks enclosed by `{{ "{{" }}` and `{{ "}}" }}`. But you can modify the way a template is parsed by passing a `LexerOptions` to the `Template.Parse` method.

The parsing mode is defined by the `LexerOptions.Mode` property which is `ScriptMode.Default` by default (i.e. mixed text and code).

But you can also parse a template that contains directly scripting code (without enclosing `{{ "{{" }}` `{{ "}}" }}`), in that case, you can use the `ScriptMode.ScriptOnly` mode.

For example illustrate how to use the `ScriptOnly` mode:

```csharp
// Create a template in ScriptOnly mode
var lexerOptions = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
// Notice that code is not enclosed by `{{ "{{" }}` and `{{ "}}" }}`
var template = Template.Parse("y = x + 1; y;", lexerOptions: lexerOptions);
// Renders it with the specified parameter
var result = template.Evaluate(new {x = 10});
// Prints 11
Console.WriteLine(result);
```


## Parsing languages

Scriban provides 3 languages through the `ScriptLang` enum:

- `ScriptLang.Default`: which is the default Scriban Language
- `ScriptLang.Liquid`: which is used to parse the language with liquid syntax.
- `ScriptLang.Scientific`: which is similar to the default, but handles expression slight differently:
  - Arguments separated by a space will convert to a multiplication: `2 x` will be evaluated as `2 * x`
  - Except if a function is taking one argument, and in that case it resolves to a function call `cos x` resolves to `cos(x)`
  - Otherwise function calls need to use explicit parenthesis `myfunction(1,2,3)`

The language is defined by the `LexerOptions.Lang` property which.

For example illustrate how to use the the `ScriptLang.Scientific` and the `ScriptOnly` mode:

```csharp
// Create a template in ScriptOnly mode
var lexerOptions = new LexerOptions() { Lang = ScriptLang.Scientific, Mode = ScriptMode.ScriptOnly };
// Notice that code is not enclosed by `{{ "{{" }}` and `{{ "}}" }}`
var template = Template.Parse("y = x + 1; 2y;", lexerOptions: lexerOptions);
// Renders it with the specified parameter
var result = template.Evaluate(new {x = 10});
// Prints 22
Console.WriteLine(result);
```


## Liquid support

Scriban supports a Lexer and Parser that can understand a Liquid template instead, while still translating it to a Scriban Runtime AST.

You can easily parse an existing liquid template using the `Template.ParseLiquid` method:

```csharp
// An Liquid 
var inputTemplateAsText = "This is a {{ "{{" }} name {{ "}}" }} template";

// Parse the template
var template = Template.ParseLiquid(inputTemplateAsText);

// Renders the template with the variable `name` exposed to the template
var result = template.Render(new { name = "Hello World"});

// Prints the result: "This is a Hello World template"
Console.WriteLine(result);
```

Also, in terms of runtime, Liquid builtin functions are supported. They are created with the `LiquidTemplateContext` which inherits from the `TemplateContext`.
