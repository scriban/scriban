// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptObjectMember : ScriptNode
    {
        private ScriptExpression? _name;
        private ScriptExpression? _value;
        private ScriptToken _colonToken = ScriptToken.Colon();
        public ScriptObjectMember()
        {
            _colonToken.Parent = this;
        }

        public ScriptExpression? Name
        {
            get => _name;
            set => ParentToThisNullable(ref _name, value);
        }

        public ScriptToken ColonToken
        {
            get => _colonToken;
            set => ParentToThis(ref _colonToken, value);
        }

        public ScriptExpression? Value
        {
            get => _value;
            set => ParentToThisNullable(ref _value, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Name is null || Value is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid object member. Name and value are required.");
            }

            var variable = Name as ScriptVariable;
            var literal = Name as ScriptLiteral;

            var name = variable?.Name ?? literal?.Value?.ToString();
            if (string.IsNullOrEmpty(name))
            {
                throw new ScriptRuntimeException(Span, "Object member name cannot be empty.");
            }

            var currentGlobal = context.CurrentGlobal ?? throw new ScriptRuntimeException(Span, "No current global object is available.");
            var memberName = name ?? throw new ScriptRuntimeException(Span, "Object member name cannot be empty.");
            currentGlobal.TrySetValue(context, Span, memberName, context.Evaluate(Value), false);
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Name is not null)
            {
                printer.Write(Name);
            }
            printer.Write(ColonToken);
            if (Value is not null)
            {
                printer.Write(Value);
            }
        }
    }
}
