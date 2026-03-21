// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptVariable? _name;
        private ScriptToken? _equalOrTripleDotToken;
        private ScriptLiteral? _defaultValue;

        public ScriptVariable? Name
        {
            get => _name;
            set => ParentToThisNullable(ref _name, value);
        }

        public ScriptToken? EqualOrTripleDotToken
        {
            get => _equalOrTripleDotToken;
            set => ParentToThisNullable(ref _equalOrTripleDotToken, value);
        }

        public ScriptLiteral? DefaultValue
        {
            get => _defaultValue;
            set => ParentToThisNullable(ref _defaultValue, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            throw new InvalidOperationException("A parameter should not be evaluated directly");
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Name);
            if (EqualOrTripleDotToken is not null)
            {
                printer.Write(EqualOrTripleDotToken);
                if (EqualOrTripleDotToken.TokenType == TokenType.Equal && DefaultValue is not null)
                {
                    printer.Write(DefaultValue);
                }
            }
        }
    }
}
