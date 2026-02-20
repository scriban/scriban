# Scriban — Codex Agent Instructions

Scriban is a fast, powerful, safe and lightweight scripting language and engine for .NET.

Paths/commands below are relative to this directory.

## Orientation

- Library: `src/Scriban/`
- Tests: `src/Scriban.Tests/` (MSTest)
- Code generators
  - `src/Scriban.DocGen/`, tooling to generate API reference from XML docs (`site/docs/builtins/*.md`)
  - `src/Scriban.AsyncCodeGen/`, tooling to generate async APIs from sync ones
  - `src/Scriban.DelegateCodeGen/`, tooling to generate delegate types for built-in functions
- Docs to keep in sync with behavior: `readme.md` and the docs under `site/docs/` (e.g., `site/docs/**/*.md`)
- Website: `site/` — Lunet-based documentation site (https://scriban.github.io)

## Build & Test

```sh
# from the project root (this folder)
cd src
dotnet build -c Release
dotnet test -c Release
```

All tests must pass and docs must be updated before submitting.

## Website (Lunet)

The project website lives in `site/` and is built with [Lunet](https://lunet.io), a static site generator.

```sh
# Prerequisites: install lunet as a .NET global tool
dotnet tool install -g lunet

# Build the site (from the project root)
cd site
lunet build          # production build → .lunet/build/www/
lunet serve          # dev server with live reload at http://localhost:4000
```

### Regenerating built-in function docs

`Scriban.DocGen` generates per-group files under `site/docs/builtins/`:

```sh
cd src
dotnet run --project Scriban.DocGen -c Release
```

### Escaping `{{` `}}` in site Markdown

Because the site is processed by Scriban via Lunet, any literal `{{` or `}}` in Markdown documentation must be escaped as `{{ "{{" }}` and `{{ "}}" }}` respectively. The `Scriban.DocGen` tool does this automatically for generated files; hand-written pages must be escaped manually.

## Contribution Rules (Do/Don't)

- Keep diffs focused; avoid drive-by refactors/formatting and unnecessary dependencies.
- Follow existing patterns and naming; prefer clarity over cleverness.
- New/changed behavior requires tests; bug fix = regression test first, then fix.
- All public APIs require XML docs (avoid CS1591) and should document thrown exceptions.

## C# Conventions (Project Defaults)

- Naming: `PascalCase` public/types/namespaces, `camelCase` locals/params, `_camelCase` private fields, `I*` interfaces.
- Style: file-scoped namespaces; `using` outside namespace (`System` first); `var` when the type is obvious.
- Nullability: enabled — respect annotations; use `ArgumentNullException.ThrowIfNull()`; prefer `is null`/`is not null`; don't suppress warnings without a justification comment.
- Exceptions: validate inputs early; throw specific exceptions (e.g., `ArgumentException`/`ArgumentNullException`) with meaningful messages.
- Async: `Async` suffix; no `async void` (except event handlers); use `ConfigureAwait(false)` in library code; consider `ValueTask<T>` on hot paths.

## Performance / AOT / Trimming

- Minimize allocations (`Span<T>`, `stackalloc`, `ArrayPool<T>`, `StringBuilder` in loops).
- Keep code AOT/trimmer-friendly: avoid reflection; prefer source generators; use `[DynamicallyAccessedMembers]` when reflection is unavoidable.
- Use `sealed` for non-inheritable classes; prefer `ReadOnlySpan<char>` for parsing.

## API Design

- Follow .NET guidelines; keep APIs small and hard to misuse.
- Prefer overloads over optional parameters (binary compatibility); consider `Try*` methods alongside throwing versions.
- Mark APIs `[Obsolete("message", error: false)]` before removal once stable (can be skipped while pre-release).

## Git / Pre-Submit

- Commits: commit after each self-contained logical step; imperative subject, < 72 chars; one logical change per commit; reference issues when relevant; don't delete unrelated local files.
- Checklist: each self-contained step is committed; build+tests pass; docs updated if behavior changed; public APIs have XML docs; changes covered by unit tests.
