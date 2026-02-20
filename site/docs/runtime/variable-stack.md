---
title: "Variable stack"
---

# The stack of `ScriptObject`

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

var template = Template.Parse("This is var1: `{{ "{{" }}var1{{ "}}" }}` and var2: `{{ "{{" }}var2{{ "}}" }}");
var result = template.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2"
Console.WriteLine(result);
```

The `TemplateContext` stack is setup like this:  `scriptObject2` => `scriptObject1` => `builtins`

As you can see the variable `var1` will be resolved from `scriptObject1` but the variable `var2` will be resolved from `scriptObject2` as there is an override here.

> **NOTE** If a variable is not found, the runtime will not throw an error but will return `null` instead. It allows to check for a variable existence `if !page` for example. In case you want your script to throw an exception if a variable was not found, you can specify `TemplateContext.StrictVariables = true` to enforce checks. See the [safe runtime](safe-runtime#safe-runtime) section for more details.

When writing to a variable, only the `ScriptObject` at the top of the `TemplateContext` will be used. This top object is accessible through `TemplateContext.CurrentGlobal` property. It the previous example, if we had something like this in a template:

```C#
var template2 = Template.Parse("This is var1: `{{ "{{" }}var1{{ "}}" }}` and var2: `{{ "{{" }}var2{{ "}}" }}`{{ "{{" }}var2 = 5{{ "}}" }} and new var2: `{{ "{{" }}var2{{ "}}" }}");

var result = template2.Render(context);

// Prints: "This is var1: `Variable 1` and var2: `Variable 2 - from ScriptObject 2 and new var2: `5`"
Console.WriteLine(result);
```

The `scriptObject2` object will now contain the `var2 = 5`

The stack provides a way to segregate variables between their usages or read-only/accessibility/mutability requirements. Typically, the `builtins` `ScriptObject` is a normal `ScriptObject` that contains all the builtins objects but you cannot modify directly the `builtins` object. But you could modify the sub-builtins objects.

For example, the following code adds a new property `myprop` to the builtin object `string`:

```
{{ "{{" }}
   string.myprop = "Yoyo"
{{ "}}" }}
```

Because scriban allows you to define new functions directly into the language and also allow to store a function pointer by using the alias `@` operator, you can basically extend an existing object with both properties and functions.

## Accessing variables anywhere in the stack from a function

To access a variable at any point in the stack from a function, use `context.GetValue()`.

```C#
public virtual object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
{
    // var1 defined in scriptObject pushed to global anywhere down the stack
    ScriptVariableGlobal scriptVariableGlobal = new ScriptVariableGlobal("var1");

    object contextObject = context.GetValue(scriptVariableGlobal);

    string var1Value = "";
    // cast to the correct type
    if (contextObject != null) {
        var1Value = (string)contextObject;
    }

    return $"var1 is {var1Value}";
}
```


## Accessing a variable only in the current global from a function

To access a variable from _only_ the top of the global stack from a function, use `context.CurrentGlobal.TryGetValue()`.

```C#
public virtual object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
{
    // var1 defined in scriptObject pushed to only the top level of the global stack
    context.CurrentGlobal.TryGetValue(context, span, key, out object contextObject);

    string var1Value = "";
    // cast to the correct type
    if (contextObject != null) {
        var1Value = (string)contextObject;
    }

    return $"var1 is {var1Value}";
}
```

## The `with` statement with the stack

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
{{ "{{" }}
    myvar = {} 
    with myvar   # Equivalent of calling context.PushGlobal(myvar)
        x = 5    # Equivalent to set myvar.x = 5
        y = 6    
    end          # Equivalent of calling context.PopGlobal()
{{ "}}" }}");

template.Render(context);

// Contains 5
Console.WriteLine(((ScriptObject)scriptObject1["myvar"])["x"]);
```
