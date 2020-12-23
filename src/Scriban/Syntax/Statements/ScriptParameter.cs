// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using Scriban.Parsing;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptParameter : ScriptNode
    {
        private ScriptVariable _name;
        private ScriptToken _equalOrTripleDotToken;
        private ScriptLiteral _defaultValue;

        public ScriptVariable Name
        {
            get => _name;
            set => ParentToThis(ref _name, value);
        }

        public ScriptToken EqualOrTripleDotToken
        {
            get => _equalOrTripleDotToken;
            set => ParentToThis(ref _equalOrTripleDotToken, value);
        }

        public ScriptLiteral DefaultValue
        {
            get => _defaultValue;
            set => ParentToThis(ref _defaultValue, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            throw new InvalidOperationException("A parameter should not be evaluated directly");
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Name);
            if (EqualOrTripleDotToken != null)
            {
                printer.Write(EqualOrTripleDotToken);
                if (EqualOrTripleDotToken.TokenType == TokenType.Equal)
                {
                    printer.Write(DefaultValue);
                }
            }
        }
    }
}