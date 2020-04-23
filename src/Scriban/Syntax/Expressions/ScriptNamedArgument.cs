// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    public partial class ScriptNamedArgument : ScriptExpression
    {
        private ScriptExpression _value;

        public ScriptNamedArgument()
        {
        }

        public ScriptNamedArgument(string name)
        {
            Name = name;
        }

        public ScriptNamedArgument(string name, ScriptExpression value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

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
                printer.Write(":");
                printer.Write(Value);
            }
        }
    }
}