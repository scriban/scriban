// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using Scriban.Runtime;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("import statement", "import <expression>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptImportStatement : ScriptStatement
    {
        private ScriptKeyword _importKeyword;
        private ScriptExpression? _expression;

        public ScriptImportStatement()
        {
            _importKeyword = ScriptKeyword.Import();
            _importKeyword.Parent = this;
        }

        public ScriptKeyword ImportKeyword
        {
            get => _importKeyword;
            set => ParentToThis(ref  _importKeyword, value);
        }

        public ScriptExpression? Expression
        {
            get => _expression;
            set => ParentToThisNullable(ref _expression, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Expression is null)
            {
                return null;
            }

            var value = context.Evaluate(Expression);
            if (value is null)
            {
                return null;
            }

            context.Import(Expression.Span, value);
            return null;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(ImportKeyword).ExpectSpace();
            printer.Write(Expression);
            printer.ExpectEos();
        }
    }
}
