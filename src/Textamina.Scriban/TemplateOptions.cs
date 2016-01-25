// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using Textamina.Scriban.Parsing;
using Textamina.Scriban.Runtime;

namespace Textamina.Scriban
{
    /// <summary>
    /// Template options used to control the behaviour of the <see cref="Lexer"/>, <see cref="Parser"/> and rendering execution of the template.
    /// </summary>
    public class TemplateOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateOptions"/> class.
        /// </summary>
        public TemplateOptions()
        {
            LoopLimit = 1000;
            RecursiveLimit = 100;
            MemberRenamer = StandardMemberRenamer.Default;
            Parser = new ParserOptions();
        }

        public ParserOptions Parser { get; private set; }

        public ITemplateLoader TemplateLoader { get; set; }

        public IMemberRenamer MemberRenamer { get; set; }

        public int LoopLimit { get; set; }

        public int RecursiveLimit { get; set; }

        public TemplateOptions Clone()
        {
            var options = (TemplateOptions) MemberwiseClone();
            options.Parser = options.Parser.Clone();
            return options;
        }
    }
}