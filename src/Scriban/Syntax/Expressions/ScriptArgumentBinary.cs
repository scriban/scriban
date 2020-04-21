// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    /// <summary>
    /// A binary operation argument used with <see cref="ScriptFunctionCall"/>
    /// when parsing with scientific mode.
    /// </summary>
    public partial class ScriptArgumentBinary : ScriptExpression
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
            throw new NotImplementedException();
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(OperatorToken?.ToString() ?? Operator.ToText());
        }
    }
}