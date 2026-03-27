---
title: "Runtime API"
---

## Runtime API

This document describes the runtime API to manipulate scriban text templating.

Scriban provides a **safe runtime**, meaning it doesn't expose any .NET objects that haven't been made explicitly available to a Template. 

The runtime is composed of two main parts:

- The **parsing/compiler** infrastructure that is responsible for parsing a text template and build a runtime representation of it (we will call this an `Abstract Syntax Tree`)
- The **rendering/evaluation** infrastructure that is responsible to render a compiled template to a string. We will see also that we can evaluate expressions without rendering.

The scriban runtime was designed to provide an easy, powerful and extensible infrastructure. For example, we are making sure that nothing in the runtime is using a static, so that you can correctly override all the behaviors of the runtime.

| Topic | Description |
|-------|-------------|
| [Parsing a template](parsing.md) | Parse templates, choose parsing modes and languages, Liquid support |
| [Rendering a template](rendering.md) | Render templates and understand the `TemplateContext` execution model |
| [The `ScriptObject`](scriptobject.md) | Import variables, delegates, .NET classes/objects, JSON, and builtins |
| [Variable stack](variable-stack.md) | How the stack of `ScriptObject` works, the `with` statement, accessing variables from functions |
| [Member renamer and filter](member-renamer.md) | Customize how .NET member names are exposed and filtered |
| [Include and `ITemplateLoader`](includes.md) | Load templates dynamically with the `include` directive |
| [Lexer, Parser and AST](ast.md) | Low-level parsing, the Abstract Syntax Tree and AST-to-text round-tripping |
| [Extending and custom functions](extending.md) | Extend `TemplateContext`, advanced and hyper custom functions |
| [Safe runtime](safe-runtime.md) | Understand Scriban's sandbox model, evaluate expressions, and configure `TemplateContext` runtime limits and execution switches |
