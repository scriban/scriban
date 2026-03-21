// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptNamedArgument : ScriptExpression
    {
        private ScriptVariable? _name;
        private ScriptToken? _colonToken;
        private ScriptExpression? _value;
        public ScriptNamedArgument()
        {
        }

        public ScriptVariable? Name
        {
            get => _name;
            set => ParentToThisNullable(ref _name, value);
        }

        public ScriptToken? ColonToken
        {
            get => _colonToken;
            set => ParentToThisNullable(ref _colonToken, value);
        }

        public ScriptExpression? Value
        {
            get => _value;
            set => ParentToThisNullable(ref _value, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Value is not null) return context.Evaluate(Value);
            return true;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Name is null)
            {
                return;
            }
            printer.Write(Name);

            if (Value is not null)
            {
                if (ColonToken is not null)
                {
                    printer.Write(ColonToken);
                }
                printer.Write(Value);
            }
        }
    }
}
