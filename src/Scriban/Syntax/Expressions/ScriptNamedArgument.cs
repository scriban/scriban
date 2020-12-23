// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
        private ScriptVariable _name;
        private ScriptToken _colonToken;
        private ScriptExpression _value;

        public ScriptNamedArgument()
        {
        }

        public ScriptVariable Name
        {
            get => _name;
            set => ParentToThis(ref _name, value);
        }

        public ScriptToken ColonToken
        {
            get => _colonToken;
            set => ParentToThis(ref _colonToken, value);
        }

        public ScriptExpression Value
        {
            get => _value;
            set => ParentToThis(ref _value, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            if (Value != null) return context.Evaluate(Value);
            return true;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Name == null)
            {
                return;
            }
            printer.Write(Name);

            if (Value != null)
            {
                printer.Write(ColonToken);
                printer.Write(Value);
            }
        }
    }
}