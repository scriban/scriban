// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    public partial class ScriptExplicitRewriter : ScriptRewriter
    {
        private readonly TemplateContext _context;

        public ScriptExplicitRewriter(TemplateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            CopyTrivias = false; // We rewrite trivias entirely here.
        }

        public override ScriptNode Visit(ScriptAssignExpression node)
        {
            var newNode = (ScriptAssignExpression)base.Visit(node);

            newNode.EqualToken.AddSpaceBefore();
            newNode.EqualToken.AddSpaceAfter();
            return newNode;
        }
        
        private static bool HasSimilarPrecedenceThanMultiply(ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                case ScriptBinaryOperator.Power:
                    return true;
            }

            return false;
        }

        public override ScriptNode Visit(ScriptPipeCall node)
        {
            var pipeCall = (ScriptPipeCall) base.Visit(node);

            pipeCall.PipeToken.AddSpaceBefore();
            pipeCall.PipeToken.AddSpaceAfter();

            return pipeCall;
        }

        public override ScriptNode Visit(ScriptBinaryExpression node)
        {
            var binaryExpression = (ScriptBinaryExpression) base.Visit((ScriptBinaryExpression)node);

            // We don't surround range with spaces
            if (binaryExpression.Operator < ScriptBinaryOperator.RangeInclude)
            {
                binaryExpression.OperatorToken.AddSpaceBefore();
                binaryExpression.OperatorToken.AddSpaceAfter();
            }
            
            if (binaryExpression.Operator == ScriptBinaryOperator.Divide || binaryExpression.Operator == ScriptBinaryOperator.DivideRound || (_context.UseScientific && binaryExpression.Operator == ScriptBinaryOperator.Power))
            {
                if (binaryExpression.Left is ScriptBinaryExpression leftBin && HasSimilarPrecedenceThanMultiply(leftBin.Operator))
                {
                    binaryExpression.Left = null;
                    binaryExpression.Left = new ScriptNestedExpression(leftBin);
                }

                if (binaryExpression.Right is ScriptBinaryExpression rightBin && HasSimilarPrecedenceThanMultiply(rightBin.Operator))
                {
                    binaryExpression.Right = null;
                    binaryExpression.Right = new ScriptNestedExpression(rightBin);
                }
            }

            if (binaryExpression.Operator == ScriptBinaryOperator.Divide || binaryExpression.Operator == ScriptBinaryOperator.DivideRound || (_context.UseScientific && binaryExpression.Operator == ScriptBinaryOperator.Power))
            {
                return new ScriptNestedExpression()
                {
                    Expression = binaryExpression
                };
            }

            return binaryExpression;
        }

        public override ScriptNode Visit(ScriptExpressionStatement node)
        {
            var stmt = (ScriptExpressionStatement) base.Visit(node);

            // De-nest top-level expressions
            stmt.Expression = DeNestExpression(stmt.Expression);
            return stmt;
        }

        public override ScriptNode Visit(ScriptFunctionCall node)
        {
            var newNode = node.GetScientificExpression(_context);
            if (newNode != node)
            {
                return Visit((ScriptNode) newNode);
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
                functionCall.Arguments[i] = DeNestExpression(functionCall.Arguments[i]);
                
                if (i > 0)
                {
                    arg.AddComma().AddSpaceBefore();
                }
            }

            return functionCall;
        }

        public override ScriptNode Visit(ScriptFunction node)
        {
            var newFunction = (ScriptFunction) base.Visit(node);

            if (newFunction.EqualToken != null)
            {
                newFunction.EqualToken.AddSpaceBefore();
                newFunction.EqualToken.AddSpaceAfter();
            }

            return newFunction;
        }

        private static ScriptExpression DeNestExpression(ScriptExpression expr)
        {
            while (expr is ScriptNestedExpression nested)
            {
                expr = nested.Expression;
                nested.Expression = null;
            }

            return expr;
        }
    }
}