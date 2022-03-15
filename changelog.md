# Changelog

> This changelog is no longer used for newer version. Please visits https://github.com/scriban/scriban/releases

## 5.4.0 (6 Feb 2022)
- Fix async when accessing variables (#382)
- Fix doc for object.kind (#394)
- Fix comparison between ulong and int (#395)
- Add a notice for html.strip that it can fail/timeout in case of malformed HTML (#406)
- Log an error instead of an exception in an invalid syntax with for/if/else (#393)

## 5.3.0 (6 Feb 2022)
- Add support for auto-indent (#398)

## 5.2.0 (6 Feb 2022)
- Add support for specifying custom format and culture for date.parse

## 5.1.0 (5 Feb 2022)
- Fix accessing null?.property when EnableRelaxedMemberAccess == False (#401)
- Migrate tests to net6.0 (#410)
- Add support for anykind of expressions in when clauses (#409)

## 5.0.0 (20 Oct 2021)
- Fix scope for variable (global vs local) (#385)

## 4.1.0 (7 October 2021)
- Add `IQueryable<T>` support in `for`-loops. 
    - Parameter `offset:count` will be translated to `IQueryable<T>.Skip(count)`
    - Parameter `reversed` will be translated to `IQueryable<T>.Reverse()`
    - Parameter `limit:count` will be translated to `IQueryable<T>.Take(count)`
    - [Special for loop variables](https://github.com/scriban/scriban/blob/master/doc/language.md#special-loop-variables) are supported with the exception of `rindex` and `last`
    - Optional `LoopLimitQueryable`-property on `TemplateContext` object to control looplimit in case of `IQueryable<T>` (defaults to LoopLimit)

## 4.0.2 (22 September 2021)
- Fix issue with object instance conversion between different object types

## 4.0.1 (3 June 2021)
- Fix ternary operator with binary expression (#355)

## 4.0.0 (2 June 2021)
- Remove `IItemAccessor` and merge features into `IObjectAccessor`.
- Add support for non-string indexers for regular dictionaries (#364)

## 3.9.0 (31 May 2021)
- Improve AST to generate a function call when a pipecall is used with a parameterless function
- Add `TemplateContext.Reset` method to allow to cleanup the state of the `TemplateContext` 

## 3.8.2 (27 May 2021)
- Fix regession with new interface `IItemAccessor` not compatible with source distribution (#363)

## 3.8.1 (25 May 2021)
- Fix regression with ternary operator (#360)

## 3.8.0 (25 May 2021)
- Add special assignment operators like `x += 1` (#353)
- Add `blank` alias to `empty` (#347)
- Add support for .NET indexers (#346)
- Fix parsing of function call AST in case of a no-arg function call with a pipe (#344)
- Add support for opposite of null-coalescing operator `?!`
- Allow to disable safe runtime checks if `TemplateContext.LoopLimit` or `RecurseLimit` are equal to `0`
- Fix issue with `%L` date formatter to not drop 0 milliseconds (#349)  
- Rename ScriptBinaryOperator.Substract to Subtract

## 3.7.0 (1 May 2021)
- Add `TemplateContext.ObjectRecursionLimit` to protect against recursion limits (#340)
- Add `string.remove_last` function (#339)
- Fix evaluation of string (#342)

## 3.6.0 (20 Mar 2021)
- Add `math.uuid` and `math.random` functions

## 3.5.0 (13 Feb 2021)
- Add EnableDisplayInnerException for ScriptRuntimeException (#329)
- Add support for multi-line pipes
- Fix named arguments for include

## 3.4.2 (1 Feb 2021)
- Fix evaluation when an enumeration is returned (#324) 
- Improve returned AST when an valid part of member expression (#318)
- Implement missing Collection interface methods for ScriptArray (#326)

## 3.4.1 (23 Jan 2021)
- Fix to ensure that attempting to render object with indexed property does not cause crash (#312)
- Fix to ensure badly formed function declarations do not break parser
- Fix array.each/join stopping evaluation of following statements after using a function
- Add array.filter function (#316)

## 3.4.0 (21 Jan 2021)
- Add string.whitespace and string.empty (#307)
- `Template.Parse` is not returning a partial AST even in case of a parser error (#308)
- Add string.literal function
- Add array.each function
- Change array.join to take an optional formatting function

## 3.3.3 (14 Jan 2021)
- Fix NullReferenceException with malformed input (#293)
- Fix include operation failing when strictVariables set (#300)
- Fix internals of Scriban.DelegateCodeGen

## 3.3.2 (30 Dec 2020)
- Fix implicit function calls which take ScriptExpression as an argument.

## 3.3.1 (30 Dec 2020)
- Fix operator precedence issue with implicit function calls in scientific language mode.

## 3.3.0 (29 Dec 2020)
- Fix regression with include that was not propagating caller variables
- Make TemplateContext.SetLoopVariable virtual.

## 3.2.2 (24 Dec 2020)
- Add `#nullable disable` to allow embedding Scriban as sources in a project that has enabled nullable

## 3.2.1 (21 Dec 2020)
- Fix support for Scriban package bundled with sources

## 3.2.0 (21 Dec 2020)
- Add support for Scriban package bundled with sources 

## 3.1.0 (21 Dec 2020)
- Fix issue with end of statement not followed by an EOS (#275)
- Fix parent node span does not include all its children (#280)
- Add object.kind method and fix TemplateContext.GetTypeName for enum

## 3.0.7 (15 Dec 2020)
- Fix unary minus operator missing in Liquid

## 3.0.6 (6 Dec 2020)
- Fix subtraction in scientific mode when no whitespace was involved (e.g `1-2`) reslting in an implicit multiplication (1 * -2).

## 3.0.5 (1 Dec 2020)
- Fix indentation for nested includes when using TemplateContext.IndentWithInclude

## 3.0.4 (30 Nov 2020)
- Fix formatting/ToString of char.
- Fix issue when using a local variable inside an object initializer.
- Fix Template.Evaluate if used with Liquid.
- Fix argument out of the range when using TemplateContext.IndentWithInclude.

## 3.0.3 (29 Nov 2020)
- Fix parsing of comment with a code exit when parsing mode is script only.

## 3.0.2 (23 Nov 2020)
- Fix array multiply

## 3.0.1 (22 Nov 2020)
- Fix TemplateContext.CurrentNode when current node is a value

## 3.0.0 (15 Nov 2020)
- Final release of 3.0.0

## 3.0.0-alpha.9 (20 Aug 2020)
- Fix global scope variable for function with parameters

## 3.0.0-alpha.8 (7 Aug 2020)
- Add support for custom functions with optional and variable parameters (e.g `func f(x, y = 2, z...); end;`)
- Add support for safe dot expression (e.g `target?.member1?.sub_member1`)

## 3.0.0-alpha.6 (31 Jul 2020)
- Add support for conditional ternary expression (e.g `condition ? thenValue : elseValue`)

## 3.0.0-alpha.4 (25 Jul 2020)
- Allow to pass regular arguments to function calls in-between named arguments

## 3.0.0-alpha.3 (16 Jul 2020)
- Improve TemplateContext.GetTypeName
- Added upcase string filter to list of built-in Liquid functions

## 3.0.0-alpha.2 (6 Jul 2020)
-  Add object.eval function.

## 3.0.0-alpha.1 (18 Jun 2020)
-  First alpha version for 3.0.0, no changelog yet.

## 2.1.3 (5 Jun 2020)
- Fix comparison != with null (#189)
- Fix flow state after a return in a template to allow to reuse the same TemplateContext (#182)
- Fix EnableRelaxedMemberAccess for function call (#200) 
- Fix overrides when importing an object overriding base class methods (#227)

## 2.1.2 (8 Mar 2020)
- Case insensitive member lookup in ScriptObject

## 2.1.1 (06 Dec 2019)
- Fix issue with binary operator (a && b) that was still evaluating b even if a was already true (#191)

## 2.1.0 (30 Jun 2019)
- Allow assignments only in top level expression statements (#169)
- Fix issue with parameter less wrap function (#177)
- Make for variable only local to the loop (#172)

## 2.0.1 (11 May 2019)
- Fix accessing object property using indexer notation when this is a .NET object (#116)
- Fix for appearance parser errors in ASP.NET Core developer exception page Continue to fix problem described in this PR https://github.com/lunet-io/scriban/pull/109
- Fix issue with operator || evaluating right expression even if left is true (#166)

## 2.0.0 (08 Mar 2019)
- Fix Template.ParseLiquid throws NullReferenceException #120
- Fix ArgumentOutOfRangeException when parsing invalid escape \u \x in strings (#121)
- Fix InvalidCastException when an invalid conversion is occuring (#122)
- Fix operator precedence for unary operators (#136)

## 2.0.0-alpha-006 (07 Mar 2019)
- Improve performance of for loops
- Reduce allocations for string functions
- Add support for rendering exceptions
- Add base64 functions
- Limit the supported .NET platforms to `net35`, `net40`, `net45`, `netstandard1.1`, `netstandard1.3`, `netstandard2.0`

## 2.0.0-alpha-005 (15 Jan 2019)
- Allow binary compare to work on any objects and object.Equals for non primitives (#109)
- Fix issue with return statement not being propagated properly when used with pipe functions (#105)

## 2.0.0-alpha-004 (04 Jan 2019)
- Fix invalid handling of pipe arguments (#103)
- Fix issue OverflowException when using a script function with a TemplateContext and object params (#104)

## 2.0.0-alpha-003 (03 Jan 2019)
- Fix issue with endraw not being parsed correctly if there is anything after (#102)

## 2.0.0-alpha-002 (01 Jan 2019)
- Fix precedence for binary operations (#100)

## 2.0.0-alpha-001 (28 Dec 2018)
- Breaking change: Add support for async/await template evaluation

## 1.2.9 (21 Dec 2018)
- Fix ret statement not returning the value when used inside a loop

## 1.2.8 (17 Dec 2018)
- Add missing datetime formatting codes (#81)
- Add Array.Contains functionality (#76)
- Add range operator for longs (#92)
- Add support for relaxed indexer access (#93)

## 1.2.7 (6 Oct 2018)
- Fix numeric literals not being parsed with the invariant culture (#74)
- Fix string to number conversion functions to use the context culture (#78)

## 1.2.6 (26 Sep 2018)
- Fix a an exception when using a decimal in a binary operation (#72)

## 1.2.5 (29 Aug 2018)
- Fix a bug with `netstandard2.0` throwing a `NullReferenceException` when using internally reflection

## 1.2.4 (21 Aug 2018)
- Add support for passing culture info directly to `math.format` and `object.format` (#68)
- Add support for netstandard2.0
- Add support for github sourcelink debugging

## 1.2.3 (20 July 2018)
- Add support for passing `MemberFilterDelegate` directly to `Template.Render` and `Template.Evaluate` (#64)

## 1.2.2 (3 July 2018)

- Fix `date.now` that was actually caching the value on initialization (#60)

## 1.2.1 (1 June 2018)

- Add `string.to_int` `string.to_long` `string.to_float` `string.to_double`. (#55)

## 1.2.0 (10 Feb 2018)

- Remove support for importing method instance as this is confusing and actually not supported. Update the documentation. (#44)

## 1.1.1 (22 Jan 2018)

- Take into account inheritance when accessing properties for auto-import .NET object (#43)

## 1.1.0 (22 Jan 2018)

- Fix `date.to_string` and `date.parse` to accept/return nullable DateTime and return null accordingly (#42)

## 1.0.0 (24 Dec 2017)

- Bump to 1.0.0

## 1.0.0-beta-006 (12 Dec 2017)

- Add member renamer parameter to the method `Template.Render(object, renamer)`

## 1.0.0-beta-005 (03 Dec 2017)

- Fix bug when importing an object to import also parent properties/fields/methods (#35)

## 1.0.0-beta-004 (19 Nov 2017)

- Add better exception with span if an error occured when getPath/Load a template include

## 1.0.0-beta-003 (19 Nov 2017)

- Add extension method IScripObject.SetValue

## 1.0.0-beta-002 (13 Nov 2017)

- Add support for decimal
- Add support for nullable types for user functions

## 1.0.0-beta-001 (12 Nov 2017)

- Bump version from 0.16.0 to 1.0.0-beta-001

## 0.16.0 (11 Nov 2017)

- Work towards 1.0.0-beta
- Change MemberRenamer to receive a MemberInfo instead
- Change MemberFilter to receive a MemberInfo instead. Add support to setup a MemberFilter on a TemplateContext as a MemberRenamer
- Rename `RenderContext`/`RenderOptions` to `TemplateRewriterContext`/`TemplateRewriterOptions`
- Allow liquid parser to accept anykind of tags in tag sections and not only the defaults

## 0.15.0 (09 Nov 2017)

- Add changelog.md
- Work towards 1.0.0-beta (#29)
- Named arguments (#28)
- Add documentation to all .NET builtin functions (59fa7c5, ece713d)
- Generate markdown docs from .NET code for all builtin functions (c8350bb9181c8728ca6223a0e2e8e5de4aa712d4)
- Add documentation for named arguments (da32cee)
- Add documentation for `this` variable (96b31ff, fcd60ce)
- Generate tests from builtin functions documentation directly (to verify that examples are actually compiling and correct) (1db628e)
- Add documentation for `empty` variable (9884067)
- Add documentation for `obj.empty?` (9884067)
- Add documentation for .NET functions with object `params` (4755e5b)
- Add documentation for `for` statements with limit, offset, reversed. (a433209)
- Add documentation for `tablerow` statements with limit, offset, reversed. (5f7274e)
- Add documentation about `for.changed` and `for.rindex` variables (ea3c5d9)
- Add documentation for `when`/`case` (b150a5d)
- Add documentation for liquid support (d88a931)

## 0.14.0 (07 Nov 2017)

- Add support for named arguments to function calls (#28)
- Update all builtin functions to use proper argument names (for named arguments)
- Start to add markdown documentation to functions directly into .NET code (to extract them later)

## 0.13.0 (07 Nov 2017)

- Change named parameters in scriban for `for`/`tablerow` statement to match liquid's behavior (no comma but separated by space)

## 0.12.1 (06 Nov 2017)

- Enable hyphenated variables (my-variable) only for liquid and convert it to scriban with the this indexer (`this["my-variable"]`)

## 0.12.0 (05 Nov 2017)

- Improve API towards 1.0.0-beta (#29)
- Improve tests coverage

## 0.11.0 (04 Nov 2017)

- Improve support for liquid
- Add more tests and coverage

## 0.10.0 (01 Nov 2017)

- Add support for liquid compatible parser
- Add support for Ast-to-text mode
- Fix various parsing to improve compatibility with liquid templates
- Update benchmarks
- Add documentation

## 0.9.1 (27 Oct 2017)

- Add string.append, string.prepend, string.md5/sha1/sha256/hmac_sha1/hmac_sha256, html.escape, html.url_encode, html.url_escape, html.strip
- Add documentation

## 0.9.0 (25 Oct 2017)

- Refactoring breaking changes. Change namespace Scriban.Model to Scriban.Syntax. Remove usage of interfaces for renamer and template loader and use delegates instead.
- Fix a few internals in TemplateContext
- Makes the IListAccessor behaves like IObjectAccessor with TemplateContext/SourceSpan
- Change IScriptCustomFunction.Evaluate to IScriptCustomFunction.Invoke
- Add static `Template.Evaluate ` method to evalaute an expression directly (#20)
- Update documentation (runtime.md, language.md)
- Add benchmark project

## 0.7.0 (12 Oct 2017)

- Add math.is_number and math.format (#6)
- Propagate TemplateContext and Span to IScriptObject (#22)
- Major refactoring of internals of TemplateContext and ScriptNode.  Replace ScriptDate by DateTime instead (fix for #23)
- Allow bool comparison (#24)

## 0.6.0 (16 May 2017)

- Make array functions modifying an IList to instead return a modified copy of it
- Add virtual to most ScriptArray methods
- Make ScriptObject.IsReadOnly virtual

## 0.5.0 (16 May 2017)

- Breaking changes. Rename IScriptObject.IsReadOnly to CanWrite. Add IScriptObject.IsReadOnly for global object locking
- Catch exceptions while evaluating an expression and rethrow them wrapped into a ScriptRuntimeExecption if it is not already the case.

## 0.4.0 (08 May 2017)

- Make ScriptObject methods virtual
- Split namespace for Runtime to Model and Functions

## 0.3.1 (04 Apr 2017)

- Add support for binding variable/member access (#19)
- Migrate to new csproj
- Improve performance of raw statements
- Add support for front matter

## 0.3.0 (09 Mar 2017)

- Add support for regex functions
- Add support for verbatim strings using backsticks
- Add support for default format date

## 0.2.2 (02 Feb 2017)

- Add support for accessing generic dictionary (#11, #15)

## 0.2.1 (01 Feb 2017)

- Fix string.capitalize bug returning an empty string when the input was already capitalized (#13)

## 0.2.0 (27 Jun 2016)

- Switch to .NETCore RTM

## 0.1.0 (31 May 2016)

- Initial version
