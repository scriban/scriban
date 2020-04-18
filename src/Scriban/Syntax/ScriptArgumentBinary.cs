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
    public class ScriptArgumentBinary : ScriptExpression
    {
        public ScriptBinaryOperator Operator { get; set; }
        
        public ScriptToken OperatorToken { get; set; }
        
        public override object Evaluate(TemplateContext context)
        {
            throw new NotImplementedException();
        }

        public override void Write(TemplateRewriterContext context)
        {
            
        }

        public override void Accept(ScriptVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }

        protected override IEnumerable<ScriptNode> GetChildren()
        {
            if (OperatorToken != null) yield return OperatorToken;
        }

        public override string ToString()
        {
            return OperatorToken?.ToString() ?? Operator.ToText();
        }
    }
}