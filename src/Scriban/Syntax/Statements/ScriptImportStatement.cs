// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Scriban.Runtime;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("import statement", "import <expression>")]
    public partial class ScriptImportStatement : ScriptStatement
    {
        private ScriptKeyword _importKeyword;
        private ScriptExpression _expression;

        public ScriptImportStatement()
        {
            ImportKeyword = ScriptKeyword.Import();
        }

        public ScriptKeyword ImportKeyword
        {
            get => _importKeyword;
            set => ParentToThis(ref  _importKeyword, value);
        }

        public ScriptExpression Expression
        {
            get => _expression;
            set => ParentToThis(ref _expression, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var value = context.Evaluate(Expression);
            if (value == null)
            {
                return null;
            }
            var scriptObject = value as ScriptObject;
            if (scriptObject == null)
            {
                throw new ScriptRuntimeException(Expression.Span, $"Unexpected value `{value.GetType()}` for import. Expecting an plain script object {{}}");
            }

            context.CurrentGlobal.Import(scriptObject);
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(ImportKeyword).ExpectSpace();
            context.Write(Expression);
            context.ExpectEos();
        }
    }
}