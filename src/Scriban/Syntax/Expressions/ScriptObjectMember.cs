// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    public partial class ScriptObjectMember : ScriptNode
    {
        private ScriptExpression _name;
        private ScriptExpression _value;

        public ScriptObjectMember()
        {
        }

        public ScriptObjectMember(ScriptExpression name, ScriptExpression value)
        {
            Name = name;
            Value = value;
        }

        public ScriptExpression Name
        {
            get => _name;
            set => ParentToThis(ref _name, value);
        }


        public ScriptExpression Value
        {
            get => _value;
            set => ParentToThis(ref _value, value);
        }


        public override object Evaluate(TemplateContext context)
        {
            var variable = Name as ScriptVariable;
            var literal = Name as ScriptLiteral;

            var name = variable?.Name ?? literal?.Value?.ToString();
            context.CurrentGlobal.SetValue(context, Span, name, context.Evaluate(Value), false);
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Name);
            printer.Write(":");
            printer.Write(Value);
        }
    }
}