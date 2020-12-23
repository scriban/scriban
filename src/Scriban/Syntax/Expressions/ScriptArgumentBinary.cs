// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;

namespace Scriban.Syntax
{
    /// <summary>
    /// A binary operation argument used with <see cref="ScriptFunctionCall"/>
    /// when parsing with scientific mode.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptArgumentBinary : ScriptExpression
    {
        private ScriptToken _operatorToken;

        public ScriptBinaryOperator Operator { get; set; }

        public ScriptToken OperatorToken
        {
            get => _operatorToken;
            set => ParentToThis(ref _operatorToken, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            throw new InvalidOperationException("This node should not be evaluated");
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (OperatorToken != null)
            {
                printer.Write(OperatorToken);
            }
            else
            {
                printer.Write(Operator.ToText());
            }
        }
    }
}