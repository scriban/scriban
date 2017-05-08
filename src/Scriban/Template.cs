// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Model;

namespace Scriban
{
    /// <summary>
    /// Entry point class to parse templates and render them.
    /// </summary>
    public class Template
    {
        private readonly ParserOptions _parserOptions;
        private readonly LexerOptions _lexerOptions;

        private Template(ParserOptions parserOptions, LexerOptions lexerOptions, string sourceFilePath)
        {
            this._parserOptions = parserOptions == null ? new ParserOptions() : parserOptions.Clone();
            this._lexerOptions = lexerOptions;
            Messages = new List<LogMessage>();
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
        public List<LogMessage> Messages { get; private set; }

        /// <summary>
        /// Parses the specified scripting text into a <see cref="Template"/> .
        /// </summary>
        /// <param name="text">The scripting text.</param>
        /// <param name="sourceFilePath">The source file path. Optional, used for better error reporting if the source file has a location on the disk</param>
        /// <param name="parserOptions">The templating parsing parserOptions.</param>
        /// <returns>A template</returns>
        public static Template Parse(string text, string sourceFilePath = null, ParserOptions parserOptions = null, LexerOptions lexerOptions = default(LexerOptions))
        {
            var template = new Template(parserOptions, lexerOptions, sourceFilePath);
            template.ParseInternal(text, sourceFilePath);
            return template;
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
        public void Render(TemplateContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (HasErrors) throw new InvalidOperationException("This template has errors. Check the <Messages> property for more details");

            // Make sure that we are using the same parserOptions
            if (SourceFilePath != null)
            {
                context.PushSourceFile(SourceFilePath);
            }

            try
            {
                context.Result = null;
                Page?.Evaluate(context);

                if (Page != null && context.EnableOutput && context.Result != null)
                {
                    context.Write(Page.Span, context.Result);
                    context.Result = null;
                }
            }
            finally
            {
                if (SourceFilePath != null)
                {
                    context.PopSourceFile();
                }
            }
        }

        /// <summary>
        /// Renders this template using the specified object model.
        /// </summary>
        /// <param name="model">The object model.</param>
        /// <returns>A rendering result as a string </returns>
        public string Render(object model = null)
        {
            var scriptObject = new ScriptObject();
            if (model != null)
            {
                scriptObject.Import(model);
            }

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            Render(context);
            context.PopGlobal();

            return context.Output.ToString();
        }

        private void ParseInternal(string text, string sourceFilePath)
        {
            // Early exit
            if (string.IsNullOrEmpty(text))
            {
                HasErrors = false;
                Messages = new List<LogMessage>();
                return;
            }

            var lexer = new Lexer(text, sourceFilePath, _lexerOptions);
            var parser = new Parser(lexer, _parserOptions);

            Page = parser.Run();

            HasErrors = parser.HasErrors;
            Messages = parser.Messages;
        }
    }
}