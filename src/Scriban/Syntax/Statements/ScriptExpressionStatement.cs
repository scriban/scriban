// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;
using System.IO;

namespace Scriban.Syntax
{
    [ScriptSyntax("expression statement", "<expression>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptExpressionStatement : ScriptStatement
    {
        private ScriptExpression? _expression;

        public ScriptExpression? Expression
        {
            get => _expression;
            set
            {
                ParentToThisNullable(ref _expression, value);
                CanOutput = value is not ScriptAssignExpression;
            }
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Expression is null)
            {
                return null;
            }

            var result = context.Evaluate(Expression);
            // This code is necessary for wrap to work
            var codeDelegate = result as ScriptNode;
            if (codeDelegate is not null)
            {
                return context.Evaluate(codeDelegate);
            }
            return result;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Expression).ExpectEos();
        }
    }
}
