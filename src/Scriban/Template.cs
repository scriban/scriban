// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban
{
    /// <summary>
    /// Basic entry point class to parse templates and render them. For more advanced scenario, you should use <see cref="TemplateContext"/> directly.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class Template
    {
        private Template(ParserOptions? parserOptions, LexerOptions? lexerOptions, string sourceFilePath)
        {
            ParserOptions = parserOptions ?? new ParserOptions();
            LexerOptions = lexerOptions ?? new LexerOptions();
            Messages = new LogMessageBag();
            this.SourceFilePath = sourceFilePath;
        }

        /// <summary>
        /// Gets the source file path.
        /// </summary>
        public string SourceFilePath { get; }

        /// <summary>
        /// Gets the resulting compiled <see cref="ScriptPage"/>. May be null if this template <see cref="HasErrors"/>
        /// </summary>
        public ScriptPage Page { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this template has errors. Error messages are in <see cref="Messages"/>.
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Gets the lexer and parsing messages.
        /// </summary>
        public LogMessageBag Messages { get; }

        /// <summary>
        /// The parser options used by this Template
        /// </summary>
        public ParserOptions ParserOptions { get; }

        /// <summary>
        /// The lexer options used by this template
        /// </summary>
        public LexerOptions LexerOptions { get; }

        /// <summary>
        /// Parses the specified scripting text into a <see cref="Template"/> .
        /// </summary>
        /// <param name="text">The scripting text.</param>
        /// <param name="sourceFilePath">The source file path. Optional, used for better error reporting if the source file has a location on the disk</param>
        /// <param name="parserOptions">The templating parsing parserOptions.</param>
        /// <param name="lexerOptions">The options passed to the lexer</param>
        /// <returns>A template</returns>
        public static Template Parse(string text, string sourceFilePath = null, ParserOptions? parserOptions = null, LexerOptions? lexerOptions = null)
        {
            var template = new Template(parserOptions, lexerOptions, sourceFilePath);
            template.ParseInternal(text, sourceFilePath);
            return template;
        }

        /// <summary>
        /// Parses the specified Liquid script text into a <see cref="Template"/> .
        /// </summary>
        /// <param name="text">The liquid scripting text.</param>
        /// <param name="sourceFilePath">The source file path. Optional, used for better error reporting if the source file has a location on the disk</param>
        /// <param name="parserOptions">The templating parsing parserOptions.</param>
        /// <param name="lexerOptions">The options passed to the lexer</param>
        /// <returns>A template</returns>
        public static Template ParseLiquid(string text, string sourceFilePath = null, ParserOptions? parserOptions = null, LexerOptions? lexerOptions = null)
        {
            var localLexerOptions = lexerOptions ?? new LexerOptions();
            localLexerOptions.Lang = ScriptLang.Liquid;
            return Parse(text, sourceFilePath, parserOptions, localLexerOptions);
        }

        /// <summary>
        /// Parse and evaluates a code only expression (without enclosing `{{` and `}}`) within the specified context.
        /// </summary>
        /// <param name="expression">A code only expression (without enclosing `{{` and `}}`)</param>
        /// <param name="context">The template context</param>
        /// <returns>The result of the evaluation of the expression</returns>
        public static object Evaluate(string expression, TemplateContext context)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var lexerOption = new LexerOptions() {Mode = ScriptMode.ScriptOnly};
            var template = Parse(expression, lexerOptions: lexerOption);
            return template.Evaluate(context);
        }

        /// <summary>
        /// Parse and evaluates a code only expression (without enclosing `{{` and `}}`) within the specified context.
        /// </summary>
        /// <param name="expression">A code only expression (without enclosing `{{` and `}}`)</param>
        /// <param name="model">An object instance used as a model for evaluating this expression</param>
        /// <param name="memberRenamer">The member renamer used to import this .NET object and transitive objects. See member renamer documentation for more details.</param>
        /// <param name="memberFilter">The member filter used to filter members for .NET objects being accessed through the template, including the model being passed to this method.</param>
        /// <returns>The result of the evaluation of the expression</returns>
        public static object Evaluate(string expression, object model, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var lexerOption = new LexerOptions() { Mode = ScriptMode.ScriptOnly };
            var template = Parse(expression, lexerOptions: lexerOption);
            return template.Evaluate(model, memberRenamer, memberFilter);
        }

        /// <summary>
        /// Evaluates the template using the specified context. See remarks.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <exception cref="System.ArgumentNullException">If context is null</exception>
        /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        /// <returns>Returns the result of the last statement</returns>
        public object Evaluate(TemplateContext context)
        {
            var previousOutput = context.EnableOutput;
            try
            {
                context.UseScientific = LexerOptions.Lang == ScriptLang.Scientific;
                context.EnableOutput = false;
                return EvaluateAndRender(context, false);
            }
            finally
            {
                context.EnableOutput = previousOutput;
            }
        }

        /// <summary>
        /// Evaluates the template using the specified context
        /// </summary>
        /// <param name="model">An object model to use with the evaluation.</param>
        /// <param name="memberRenamer">The member renamer used to import this .NET object and transitive objects. See member renamer documentation for more details.</param>
        /// <param name="memberFilter">The member filter used to filter members for .NET objects being accessed through the template, including the model being passed to this method.</param>
        /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        /// <returns>Returns the result of the last statement</returns>
        public object Evaluate(object model = null, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
        {
            var scriptObject = new ScriptObject();
            if (model != null)
            {
                scriptObject.Import(model, renamer: memberRenamer, filter: memberFilter);
            }

            var context = LexerOptions.Lang == ScriptLang.Liquid ? new LiquidTemplateContext() : new TemplateContext();
            context.EnableOutput = false;
            context.MemberRenamer = memberRenamer;
            context.MemberFilter = memberFilter;
            context.UseScientific = LexerOptions.Lang == ScriptLang.Scientific;
            context.PushGlobal(scriptObject);
            var result = Evaluate(context);
            context.PopGlobal();
            return result;
        }

        /// <summary>
        /// Renders this template using the specified context. See remarks.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <exception cref="System.ArgumentNullException">If context is null</exception>
        /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        /// <remarks>
        /// When using this method, the result of rendering this page is output to <see cref="TemplateContext.Output"/>
        /// </remarks>
        public string Render(TemplateContext context)
        {
            EvaluateAndRender(context, true);
            var result = context.Output.ToString();
            var output = context.Output as StringBuilderOutput;
            if (output != null)
            {
                output.Builder.Length = 0;
            }
            return result;
        }

        /// <summary>
        /// Renders this template using the specified object model.
        /// </summary>
        /// <param name="model">The object model.</param>
        /// <param name="memberRenamer">The member renamer used to import this .NET object and transitive objects. See member renamer documentation for more details.</param>
        /// <param name="memberFilter">The member filter used to filter members for .NET objects being accessed through the template, including the model being passed to this method.</param>
        /// <returns>A rendering result as a string </returns>
        public string Render(object model = null, MemberRenamerDelegate memberRenamer = null, MemberFilterDelegate memberFilter = null)
        {
            var scriptObject = new ScriptObject();
            if (model != null)
            {
                scriptObject.Import(model, renamer: memberRenamer, filter: memberFilter);
            }

            var context = LexerOptions.Lang == ScriptLang.Liquid ? new LiquidTemplateContext() : new TemplateContext();
            context.MemberRenamer = memberRenamer;
            context.MemberFilter = memberFilter;
            context.PushGlobal(scriptObject);
            return Render(context);
        }

        /// <summary>
        /// Converts back this template to a textual representation. This is the inverse of <see cref="Parse"/>.
        /// </summary>
        /// <param name="options">The rendering options</param>
        /// <returns>The template converted back to a textual representation of the template</returns>
        public string ToText(ScriptPrinterOptions options = default(ScriptPrinterOptions))
        {
            CheckErrors();
            var writer = new TextWriterOutput();
            var renderContext = new ScriptPrinter(writer, options);
            renderContext.Write(Page);

            return writer.ToString();
        }

        /// <summary>
        /// Evaluates the template using the specified context. See remarks.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="render"><c>true</c> to render the output to the <see cref="TemplateContext.Output"/></param>
        /// <exception cref="System.ArgumentNullException">If context is null</exception>
        /// <exception cref="System.InvalidOperationException">If the template <see cref="HasErrors"/>. Check the <see cref="Messages"/> property for more details</exception>
        private object EvaluateAndRender(TemplateContext context, bool render)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            CheckErrors();

            // Make sure that we are using the same parserOptions
            if (SourceFilePath != null)
            {
                context.PushSourceFile(SourceFilePath);
            }

            try
            {
                context.UseScientific = LexerOptions.Lang == ScriptLang.Scientific;
                var result = context.Evaluate(Page);
                if (render)
                {
                    if (Page != null && context.EnableOutput && result != null)
                    {
                        context.Write(Page.Span, result);
                    }
                }
                return result;
            }
            finally
            {
                if (SourceFilePath != null)
                {
                    context.PopSourceFile();
                }
            }
        }

        private void CheckErrors()
        {
            if (HasErrors) throw new InvalidOperationException("This template has errors. Check the <Template.HasError> and <Template.Messages> before evaluating a template. Messages:\n" + string.Join("\n", Messages));
        }

        private void ParseInternal(string text, string sourceFilePath)
        {
            // Early exit
            if (string.IsNullOrEmpty(text))
            {
                HasErrors = false;
                Page = new ScriptPage() {Span = new SourceSpan(sourceFilePath, new TextPosition(), TextPosition.Eof) };
                return;
            }

            var lexer = new Lexer(text, sourceFilePath, LexerOptions);
            var parser = new Parser(lexer, ParserOptions);

            Page = parser.Run();

            HasErrors = parser.HasErrors;
            Messages.AddRange(parser.Messages);
        }
    }
}