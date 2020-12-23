// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("assign expression", "<target_expression> = <value_expression>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptAssignExpression : ScriptExpression
    {
        private ScriptExpression _target;
        private ScriptToken _equalToken;
        private ScriptExpression _value;

        public ScriptAssignExpression()
        {
            EqualToken = ScriptToken.Equal();
        }

        public ScriptExpression Target
        {
            get => _target;
            set => ParentToThis(ref _target, value);
        }

        public ScriptToken EqualToken
        {
            get => _equalToken;
            set => ParentToThis(ref _equalToken, value);
        }

        public ScriptExpression Value
        {
            get => _value;
            set => ParentToThis(ref _value, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var valueObject = context.Evaluate(Value);
            context.SetValue(Target, valueObject);
            return null;
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Target);
            printer.Write(EqualToken);
            printer.Write(Value);
        }
    }
}