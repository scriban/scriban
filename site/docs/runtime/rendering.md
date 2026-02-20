---
title: "Rendering a template"
---

# Rendering a template

## Overview

In order to render a template, you need pass a context for the variables, objects, functions that will be accessed by the template.

In the following examples, we have a variable `name` that is used by the template:

```c#
var inputTemplateAsText = "This is a {{ "{{" }} name {{ "}}" }} template";

// Parse the template
var template = Template.Parse(inputTemplateAsText);

// Renders the template with the variable `name` exposed to the template
var result = template.Render(new { name = "Hello World"});

// Prints the result: "This is a Hello World template"
Console.WriteLine(result);
```

As we can see, we are passing an anonymous objects that has this field/property `name` and by calling the Render method, the template is executed with this data model context.

While passing an anonymous object is nice for a hello world example, it is not always enough for more advanced data model scenarios. 

In this case, you want to use more directly the `TemplateContext` (used by the method `Template.Render(object)`) and a [`ScriptObject`](scriptobject) which are both at the core of scriban rendering architecture to provide more powerful constructions & hooks of the data model exposed (variables but also functions...etc.).


## The `TemplateContext` execution model

The `TemplateContext` provides:

- **an execution context** when evaluating a template. The same instance can be used with many different templates, depending on your requirements.
- A **stack of [`ScriptObject`](variable-stack)** that provides the actual variables/functions accessible to the template, accessible through `Template.PushGlobal(scriptObj)` and `Template.PopGlobal()`. Why a stack and how to use this stack is described in the [variable stack](variable-stack) page.
- The **text output** when evaluating a template, which is accessible through the `Template.Output` property as a `StringBuilder` but because you can have nested rendering happening, it is possible to use `Template.PushOutput()` and `Template.PopOutput()` to redirect temporarily the output to a new output. This functionality is typically used by the [`capture` statement](../language/readme.md#94-capture-variable--end).
- Caching of templates previously loaded by an `include` directive (see [`include` and `ITemplateLoader`](includes#include-and-itemplateloader) section )
- Various possible overrides to allow fine grained extensibility (evaluation of an expression, conversion to a string, enter/exit/step into a loop...etc.)

Note that a `TemplateContext` is not thread safe, so it is recommended to have one `TemplateContext` per thread.
