// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace Scriban.Syntax
{
    public partial class ScriptScientificRewriter : ScriptRewriter
    {
        private readonly TemplateContext _context;

        public ScriptScientificRewriter(TemplateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            CopyTrivias = false; // We rewrite trivias entirely here.
        }

        public override ScriptNode Visit(ScriptVariableGlobal node)
        {
            return new ScriptVariableGlobal(node.Name);
        }

        public override ScriptNode Visit(ScriptVariableLocal node)
        {
            return new ScriptVariableLocal(node.Name);
        }

        public override ScriptNode Visit(ScriptVariableLoop node)
        {
            return new ScriptVariableLoop(node.Name);
        }

        public override ScriptNode Visit(ScriptRawStatement node)
        {
            return new ScriptRawStatement() {Text = node.Text, EscapeCount = node.EscapeCount};
        }

        public override ScriptNode Visit(ScriptThisExpression node)
        {
            return new ScriptThisExpression();
        }

        public override ScriptNode Visit(ScriptBreakStatement node)
        {
            return new ScriptBreakStatement();
        }

        public override ScriptNode Visit(ScriptContinueStatement node)
        {
            return new ScriptContinueStatement();
        }

        public override ScriptNode Visit(ScriptLiteral node)
        {
            return new ScriptLiteral(node.Value) { StringQuoteType = node.StringQuoteType };
        }

        public override ScriptNode Visit(ScriptNopStatement node)
        {
            return new ScriptNopStatement();
        }

        public override ScriptNode Visit(ScriptToken node)
        {
            return new ScriptToken(node.Value);
        }

        public override ScriptNode Visit(ScriptAssignExpression node)
        {
            var newNode = (ScriptAssignExpression)base.Visit(node);

            newNode.EqualToken.AddSpaceBefore();
            newNode.EqualToken.AddSpaceAfter();
            return newNode;
        }

        public override ScriptNode Visit(ScriptBinaryExpression node)
        {
            var binaryExpression = (ScriptBinaryExpression) base.Visit((ScriptBinaryExpression)node);

            binaryExpression.OperatorToken.AddSpaceBefore();
            binaryExpression.OperatorToken.AddSpaceAfter();

            if (binaryExpression.Operator == ScriptBinaryOperator.Divide || binaryExpression.Operator == ScriptBinaryOperator.DivideRound)
            {
                return new ScriptNestedExpression()
                {
                    Expression = binaryExpression
                };
            }

            return binaryExpression;
        }

        public override ScriptNode Visit(ScriptFunctionCall node)
        {
            var newNode = node.GetScientificExpression(_context);
            if (newNode != node)
            {
                return newNode.Accept(this);
            }

            var functionCall = (ScriptFunctionCall)base.Visit((ScriptFunctionCall)newNode);
            functionCall.ExplicitCall = true;
            functionCall.OpenParent = ScriptToken.OpenParen();
            functionCall.CloseParen = ScriptToken.CloseParen();

            // Make sure that arguments are separated by a proper comma and space
            for (int i = 0; i < functionCall.Arguments.Count; i++)
            {
                var arg = functionCall.Arguments[i];

                // No need to nest expression for arguments
                if (arg is ScriptNestedExpression nested)
                {
                    functionCall.Arguments[i] = nested.Expression;
                }
                
                if (i > 0)
                {
                    arg.AddComma().AddSpaceBefore();
                }
            }

            return functionCall;
        }
    }
}