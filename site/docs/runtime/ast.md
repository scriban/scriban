---
title: "Lexer, Parser and AST"
---

# The Lexer and Parser

- The [`Lexer`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/Lexer.cs) class is responsible for extracting `Tokens` from a text template.
- The [`Parser`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/Parser.cs) class is responsible for creating `ScriptNode` AST from input tokens (extracted from the `Lexer`)

The lexer has a few [`LexerOptions`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/LexerOptions.cs) to control the way the lexer is behaving, as described with the [parsing modes](parsing#parsing-modes)

The parser has a [`ParserOptions`](https://github.com/lunet-io/scriban/blob/master/src/Scriban/Parsing/ParserOptions.cs) only used for securing nested statements/blocks to avoid any stack overflow exceptions while parsing a document.


# Abstract Syntax Tree

The base object used by the syntax for all scriban elements is the class `Scriban.Syntax.ScriptNode`:

```csharp
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


## AST to Text

Scriban allows to write back an AST to a textual representation:

```csharp
var template = Template.Parse("This is a {{ "{{" }} name {{ "}}" }} template");

// Prints "This is a {{ "{{" }}name{{ "}}" }} template"
Console.WriteLine(template.ToText());
```

In the previous example, you can notice that whitespace were removed from the original template. The reason is by default, the parser doesn't keep all hidden symbols when parsing, to still allow fast parsing for the regular case.

But you can specify the parser to keep all the hidden symbols from the original template, directly by activating the `IsKeepTrivia` on the `LexerOptions`

In the following example, you can see that it keep all the whitespace and comment:

```csharp
// Specifying the KeepTrivia allow to keep as much as hidden symbols from the original template (white spaces, newlines...etc.)
var template = Template.Parse(@"This is a {{ "{{" }} name   +   ## With some comment ## '' {{ "}}" }} template", lexerOptions: new LexerOptions() { KeepTrivia = true });

// Prints "This is a {{ "{{" }} name   +   ## With some comment ## '' {{ "}}" }} template"
Console.WriteLine(template.ToText());
```
