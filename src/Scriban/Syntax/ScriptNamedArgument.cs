// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    public partial class ScriptNamedArgument : ScriptExpression
    {
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

        public ScriptExpression Value { get; set; }


        public override object Evaluate(TemplateContext context)
        {
            if (Value != null) return context.Evaluate(Value);
            return true;
        }

        public override void Write(TemplateRewriterContext context)
        {
            if (Name == null)
            {
                return;
            }
            context.Write(Name);

            if (Value != null)
            {
                context.Write(":");
                context.Write(Value);
            }
        }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }
}