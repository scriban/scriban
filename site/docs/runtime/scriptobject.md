---
title: "The ScriptObject"
---

# The `ScriptObject`

The `ScriptObject` is a special implementation of a `Dictionary<string, object>` that runtime properties and functions accessible to a template:

## Accessing as regular dictionary objects

A `ScriptObject` is mainly an extended version of a `IDictionary<string, object>`:

  ```csharp
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("var1", "Variable 1");

  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is var1: `{{ "{{" }}var1{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints: This is var1: `Variable 1`
  Console.WriteLine(result);
  ```

Note that any `IDictionary<string, object>` put as a property will be accessible as well.

## Imports System.Text.Json.JsonElement

A `ScriptObject` or `ScriptArray` can import `JsonElement`.

```csharp
  // objects with ScriptObject
  JsonElement json = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");
  var model = ScriptObject.From(json);

  // arrays with ScriptArray
  JsonElement json = JsonSerializer.Deserialize<JsonElement>("""[1, 2, 3]""");
  var model = ScriptArray.From(json);

  // import to an existing object
  var model = new ScriptObject();
  model.Import(jsonElement);

  // add to an existing object
  var model = new ScriptObject();
  model.Add("foo", jsonElement);

  // render using JsonElement directly
  JsonElement model = JsonSerializer.Deserialize<JsonElement>("""{ "foo": "bar" }""");
  var template = Template.Parse("foo: `{{ "{{" }}foo{{ "}}" }}`");
  var result = template.Render(model);
  // Prints: foo: `bar`
```

**Note**: JsonElement is also supported in properties of custom classes and structs.

## Imports a .NET delegate

Via `ScriptObject.Import(member, Delegate)`. Here we import a `Func<string>`:

  ```csharp
  var scriptObject1 = new ScriptObject();
  // Declare a function `myfunc` returning the string `Yes`
  scriptObject1.Import("myfunc", new Func<string>(() => "Yes"));
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is myfunc: `{{ "{{" }}myfunc{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints: This is myfunc: `Yes`
  Console.WriteLine(result);
  ```

## Imports functions from a .NET class

You can easily import static methods declared in a .NET class via `ScriptObject.Import(typeof(MyFunctions))`

Let's define a class with a static function `Hello`:

  ```csharp
  public static class MyFunctions
  {
      public static string Hello()
      {
          return "hello from method!";
      }
  }
  ```

This function can be imported into a ScriptObject:

  ```csharp
  var scriptObject1 = new ScriptObject();
  scriptObject1.Import(typeof(MyFunctions));
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is MyFunctions.Hello: `{{ "{{" }}hello{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```

> Notice that when using a function with pipe calls like `{{ "{{" }}description | string.strip {{ "}}" }}`, the last argument passed to the `string.strip` function is the result of the previous pipe.
> That's a reason why you will notice in all builtin functions in scriban that they usually take the most relevant parameter as a last parameter instead of the first parameter, to allow proper support for pipe calls.

> **NOTICE**
>
> By default, Properties and static methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](member-renamer#member-renamer) delegate

## Automatic functions import from `ScriptObject`

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

  ```csharp
  var scriptObject1 = new MyCustomFunctions();
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is MyFunctions.Hello: `{{ "{{" }}hello{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```

Notice that if you want to ignore a member when importing a .NET object or .NET class, you can use the attribute `ScriptMemberIgnore`

> NOTE: Because Scriban doesn't support Function overloading, it is required that functions imported from a type must have different names.

> **NOTICE**
>
> By default, Properties and methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](member-renamer#member-renamer) delegate

## Function arguments, optional and `params`

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
{{ "{{" }} hello_opt "test" {{ "}}" }}
{{ "{{" }} hello_opt "test" "my_option" {{ "}}" }}
{{ "{{" }} hello_opt "test" option: "my_option" {{ "}}" }}
{{ "{{" }} hello_opt text: "test"  {{ "}}" }}
{{ "{{" }} hello_args "this" "is" "a" "test"{{ "}}" }}
{{ "{{" }} hello_args "this" "is" args: "a" args: "test"{{ "}}" }}
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

When last parameter is of type `object[]` or `ScriptExpression[]` it is automatically treated as if it was declared with `params` modifier.

## Accessing nested `ScriptObject`

A nested ScriptObject can be accessed indirectly through another `ScriptObject`:

  ```csharp
  var scriptObject1 = new ScriptObject();
  var nestedObject = new ScriptObject();
  nestedObject["x"] = 5;
  scriptObject1.Add("subObject", nestedObject);

  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{ "{{" }}subObject.x{{ "}}" }}`");
  template.Render(context);

  ```

## Imports a `ScriptObject` into another `ScriptObject`

The properties/functions of a `ScriptObject` can be imported into another instance.

  ```csharp
  var scriptObject1 = new ScriptObject();
  scriptObject1.Add("var1", "Variable 1");

  var scriptObject2 = new ScriptObject();
  scriptObject2.Add("var2", "Variable 2");
  
  // After this command, scriptObject2 contains var1 and var2
  // But modifying var2 on scriptObject2 will not modify var2 on scriptObject1!
  scriptObject2.Import(scriptObject1);
  ```

## Imports a .NET object instance

You can easily import a .NET object instance (including its public properties and static methods) into a `ScriptObject`

NOTE that when importing into a ScriptObject, the **import actually copies the property values into the ScriptObject**. The original .NET object is no longer used.

Importing a .NET object instance is thus different from [accessing a .NET object](#accessing-a-net-object) instance through a ScriptObject.

Let's define a standard .NET object:

  ```csharp
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

  ```csharp
  var scriptObject1 = new ScriptObject();
  scriptObject1.Import(new MyObject());
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{ "{{" }}hello{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```


Also any objects inheriting from `IDictionary<TKey, TValue>` or `IDictionary` will be also accessible automatically. Typically, you can usually access directly any generic JSON objects that was parsed by a JSON library.

> **NOTICE**
>
> By default, Properties and static methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](member-renamer#member-renamer) delegate

## Accessing a .NET object

This is an important feature of scriban. Every .NET objects made accessible through a ScriptObject is directly accessible without importing it. It means that Scriban will directly work on the .NET object instance instead of a copy (e.g when we do a `ScriptObject.Import` instead)

> Note that for security reason, only the properties of .NET objects accessed through another `ScriptObject` are made accessible from a Template. Methods and static methods are not automatically imported.

For example, if we re-use the previous `MyObject` directly as a variable in a `ScriptObject`:

  ```csharp
  var scriptObject1 = new ScriptObject();
  // Notice: MyObject is not imported but accessible through
  // the variable myobject
  scriptObject1["myobject"] = new MyObject();
  
  var context = new TemplateContext();
  context.PushGlobal(scriptObject1);
  
  var template = Template.Parse("This is Hello: `{{ "{{" }}myobject.hello{{ "}}" }}`");
  var result = template.Render(context);
  
  // Prints This is MyFunctions.Hello: `hello from method!`
  Console.WriteLine(result);
  ```

> **NOTICE**
>
> By default, Properties and static methods of .NET objects are automatically exposed with lowercase and `_` names. It means that a property like `MyMethodIsNice` will be exposed as `my_method_is_nice`. This is the default convention, originally to match the behavior of liquid templates.
> If you want to change this behavior, you need to use a [`MemberRenamer`](member-renamer#member-renamer) delegate

## Read-only properties

Runtime equivalent of the language `readonly <var>` statement, you can easily define a variable of a `ScriptObject` as read-only

  ```csharp
  var scriptObject1 = new ScriptObject();
  // The variable `var1` is immutable
  scriptObject1.SetValue("var1", "My immutable variable", true);

  // Or or an existing property/function member:
  scriptObject1.SetReadonly("var1", true);
  ```

## The builtin functions

All builtin functions object of Scriban are imported easily by inheriting from a `ScriptObject`:

- The `BuilinsFunctions` object defined [here](https://github.com/lunet-io/scriban/blob/8b374ffde418b8b57714e3be145a66d3085f66e6/src/Scriban/Functions/BuiltinFunctions.cs) and [listed here](https://github.com/lunet-io/scriban/tree/master/src/Scriban/Functions) is directly used as the bottom level stack `ScriptObject` as explained in the [variable stack](variable-stack) page.
- Each sub function objects (e.g `array`, `string`) are also regular `ScriptObject`. For example, the [`string` builtin functions](https://github.com/lunet-io/scriban/blob/8b374ffde418b8b57714e3be145a66d3085f66e6/src/Scriban/Functions/StringFunctions.cs)

The current builtin `ScriptObject` defined for a `TemplateContext` is accessible through the `TemplateContext.BuiltinObject` property.

See section about [ScriptObject advanced usages](extending#scriptobject-advanced-usages) also for more specific usages.
