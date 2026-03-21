// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using Scriban.Runtime;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("return statement", "return <expression>?")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptReturnStatement : ScriptStatement
    {
        private ScriptExpression? _expression;
        private ScriptKeyword _retKeyword;

        public ScriptReturnStatement()
        {
            _retKeyword = ScriptKeyword.Ret();
            _retKeyword.Parent = this;
        }

        public ScriptKeyword RetKeyword
        {
            get => _retKeyword;
            set => ParentToThis(ref _retKeyword, value);
        }

        public ScriptExpression? Expression
        {
            get => _expression;
            set => ParentToThisNullable(ref _expression, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            var result = Expression is null ? null : context.Evaluate(Expression);
            //ensure that deferred array interators are evaluated before we lose context
            if (result is ScriptRange range)
            {
                result = new ScriptArray(range);
            }
            context.FlowState = ScriptFlowState.Return;
            return result;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(RetKeyword).ExpectSpace();
            printer.Write(Expression).ExpectEos();
        }
    }
}
