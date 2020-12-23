// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptObjectMember : ScriptNode
    {
        private ScriptExpression _name;
        private ScriptExpression _value;
        private ScriptToken _colonToken;

        public ScriptObjectMember()
        {
            ColonToken = ScriptToken.Colon();
        }

        public ScriptExpression Name
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
            var variable = Name as ScriptVariable;
            var literal = Name as ScriptLiteral;

            var name = variable?.Name ?? literal?.Value?.ToString();
            context.CurrentGlobal.TrySetValue(context, Span, name, context.Evaluate(Value), false);
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Name);
            printer.Write(ColonToken);
            printer.Write(Value);
        }
    }
}