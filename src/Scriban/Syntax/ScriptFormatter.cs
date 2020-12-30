// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptFormatter : ScriptRewriter
    {
        private readonly TemplateContext _context;
        private readonly ScriptFormatterFlags _flags;
        private readonly bool _isScientific;
        private readonly CompressWhitespacesVisitor _compressWhitespacesVisitor;

        public ScriptFormatter(ScriptFormatterOptions options)
        {
            Options = options;
            _flags = options.Flags;
            CopyTrivias = !_flags.HasFlags(ScriptFormatterFlags.RemoveExistingTrivias);
            _isScientific = options.Language == ScriptLang.Scientific;
            _context = options.Context == null && _isScientific
                ? throw new InvalidOperationException("The context within the options cannot be null when the scientific language is used.")
                : options.Context;
            _compressWhitespacesVisitor = _flags.HasFlags(ScriptFormatterFlags.CompressSpaces) ? new CompressWhitespacesVisitor() : null;
        }

        public readonly ScriptFormatterOptions Options;

        public ScriptNode Format(ScriptNode node)
        {
            ScriptNode newNode;
            if (_context != null)
            {
                var contextStrictVariables = _context.StrictVariables;
                var contextEnableRelaxedIndexerAccess = _context.EnableRelaxedIndexerAccess;
                var contextEnableRelaxedMemberAccess = _context.EnableRelaxedMemberAccess;
                var contextEnableRelaxedTargetAccess = _context.EnableRelaxedTargetAccess;
                var contextEnableNullIndexer = _context.EnableNullIndexer;
                var contextIgnoreExceptionsWhileRewritingScientific = _context.IgnoreExceptionsWhileRewritingScientific;

                _context.StrictVariables = false;
                _context.EnableRelaxedIndexerAccess = true;
                _context.EnableRelaxedMemberAccess = true;
                _context.EnableRelaxedTargetAccess = true;
                _context.EnableNullIndexer = true;
                _context.IgnoreExceptionsWhileRewritingScientific = true;

                try
                {
                    newNode = Visit(node);
                }
                finally
                {
                    _context.StrictVariables = contextStrictVariables;
                    _context.EnableRelaxedIndexerAccess = contextEnableRelaxedIndexerAccess;
                    _context.EnableRelaxedMemberAccess = contextEnableRelaxedMemberAccess;
                    _context.EnableRelaxedTargetAccess = contextEnableRelaxedTargetAccess;
                    _context.EnableNullIndexer = contextEnableNullIndexer;
                    _context.IgnoreExceptionsWhileRewritingScientific = contextIgnoreExceptionsWhileRewritingScientific;
                }
            }
            else
            {
                newNode = Visit(node);
            }

            if (_flags.HasFlags(ScriptFormatterFlags.CompressSpaces))
            {
                _compressWhitespacesVisitor.CompressSpaces(newNode);
            }

            return newNode;
        }

        public override ScriptNode Visit(ScriptNode node)
        {
            var newNode = base.Visit(node);
            return newNode;
        }

        public override ScriptNode Visit(ScriptAssignExpression node)
        {
            var newNode = (ScriptAssignExpression)base.Visit(node);

            if (_flags.HasFlags(ScriptFormatterFlags.AddSpaceBetweenOperators))
            {
                newNode.EqualToken.AddLeadingSpace();
                newNode.EqualToken.AddSpaceAfter();
            }
            return newNode;
        }

        public override ScriptNode Visit(ScriptPipeCall node)
        {
            var pipeCall = (ScriptPipeCall) base.Visit(node);

            if (_flags.HasFlags(ScriptFormatterFlags.AddSpaceBetweenOperators))
            {
                pipeCall.PipeToken.AddLeadingSpace();
                pipeCall.PipeToken.AddSpaceAfter();
            }

            return pipeCall;
        }

        public override ScriptNode Visit(ScriptBinaryExpression node)
        {
            if (_isScientific)
            {
                var newNode = ScientificFunctionCallRewriter.Rewrite(_context, node);
                if (newNode != node)
                {
                    return Visit((ScriptNode)newNode);
                }
            }

            var binaryExpression = (ScriptBinaryExpression) base.Visit((ScriptBinaryExpression)node);

            // We don't surround range with spaces
            if (_flags.HasFlags(ScriptFormatterFlags.AddSpaceBetweenOperators))
            {
                if (binaryExpression.Operator < ScriptBinaryOperator.RangeInclude)
                {
                    binaryExpression.OperatorToken.AddLeadingSpace();
                    binaryExpression.OperatorToken.AddSpaceAfter();
                }
            }

            if (_flags.HasFlags(ScriptFormatterFlags.ExplicitParenthesis))
            {
                if (binaryExpression.Operator == ScriptBinaryOperator.Divide || binaryExpression.Operator == ScriptBinaryOperator.DivideRound || (_isScientific && binaryExpression.Operator == ScriptBinaryOperator.Power))
                {
                    if (binaryExpression.Left is ScriptBinaryExpression leftBin && HasSimilarPrecedenceThanMultiply(leftBin.Operator))
                    {
                        binaryExpression.Left = null;
                        var nested = new ScriptNestedExpression(leftBin);
                        binaryExpression.Left = nested;
                        leftBin.MoveTrailingTriviasTo(nested.CloseParen, true);
                    }

                    if (binaryExpression.Right is ScriptBinaryExpression rightBin && HasSimilarPrecedenceThanMultiply(rightBin.Operator))
                    {
                        binaryExpression.Right = null;
                        var nested = new ScriptNestedExpression(rightBin);
                        binaryExpression.Right = nested;
                        rightBin.MoveTrailingTriviasTo(nested.CloseParen, true);
                    }
                }

                if (binaryExpression.Operator == ScriptBinaryOperator.Divide || binaryExpression.Operator == ScriptBinaryOperator.DivideRound || (_isScientific && binaryExpression.Operator == ScriptBinaryOperator.Power))
                {
                    var nested = new ScriptNestedExpression()
                    {
                        Expression = binaryExpression
                    };
                    binaryExpression.MoveTrailingTriviasTo(nested.CloseParen, true);
                    return nested;
                }
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
            var functionCall = (ScriptFunctionCall)base.Visit(node);

            // Make sure that we have always a parenthesis for function calls
            if (_isScientific)
            {
                if (_flags.HasFlags(ScriptFormatterFlags.ExplicitParenthesis) || functionCall.Arguments.Count > 1)
                {
                    functionCall.ExplicitCall = true;
                    functionCall.OpenParent ??= ScriptToken.OpenParen();
                    functionCall.CloseParen ??= ScriptToken.CloseParen();

                    // We remove any trailing spaces after the target cos (x) => cos(x)
                    functionCall.Target.RemoveTrailingSpace();
                    functionCall.Arguments.RemoveLeadingSpace();
                    functionCall.Arguments.MoveTrailingTriviasTo(functionCall.CloseParen, false);
                }
            }

            // Make sure that arguments are separated by a proper comma and space
            for (int i = 0; i < functionCall.Arguments.Count; i++)
            {
                var arg = functionCall.Arguments[i];

                // No need to nest expression for arguments
                arg = DeNestExpression(arg);

                if (i + 1 < functionCall.Arguments.Count)
                {
                    var lastToken = (IScriptTerminal)arg.FindLastTerminal();
                    if (_isScientific)
                    {
                        lastToken.AddCommaAfter();
                    }

                    if (_flags.HasFlags(ScriptFormatterFlags.AddSpaceBetweenOperators))
                    {
                        lastToken.AddSpaceAfter();
                    }
                }

                functionCall.Arguments[i] = arg;
            }

            return functionCall;
        }

        public override ScriptNode Visit(ScriptFunction node)
        {
            ScriptFunction newFunction;
            if (_context != null)
            {
                var hasParams = node.HasParameters;

                if (hasParams)
                {
                    _context.PushGlobal(new ScriptObject());
                }
                else
                {
                    _context.PushLocal();
                }

                try
                {
                    _context.SetValue(ScriptVariable.Arguments, string.Empty, true);

                    if (node.HasParameters)
                    {
                        var glob = _context.CurrentGlobal;
                        for (var i = 0; i < node.Parameters.Count; i++)
                        {
                            var arg = node.Parameters[i];
                            glob.SetValue(arg.Name.Name, string.Empty, false);
                        }
                    }

                    newFunction = (ScriptFunction) base.Visit(node);
                }
                finally
                {
                    if (hasParams)
                    {
                        _context.PopGlobal();
                    }
                    else
                    {
                        _context.PopLocal();
                    }
                }
            }
            else
            {
                newFunction = (ScriptFunction) base.Visit(node);
            }

            if (_flags.HasFlags(ScriptFormatterFlags.Clean) && newFunction.OpenParen != null)
            {
                newFunction.NameOrDoToken.RemoveTrailingSpace();
                newFunction.Parameters.RemoveLeadingSpace();
                newFunction.Parameters.RemoveTrailingSpace();
            }

            if (_flags.HasFlags(ScriptFormatterFlags.AddSpaceBetweenOperators) &&  newFunction.EqualToken != null)
            {
                newFunction.EqualToken.AddLeadingSpace();
                newFunction.EqualToken.AddSpaceAfter();
            }

            return newFunction;
        }

        private ScriptExpression DeNestExpression(ScriptExpression expr)
        {
            if (_flags.HasFlags(ScriptFormatterFlags.MinimizeParenthesisNesting))
            {
                while (expr is ScriptNestedExpression nested)
                {
                    expr = nested.Expression;
                    nested.Expression = null;
                }
            }

            return expr;
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

        private class CompressWhitespacesVisitor : ScriptVisitor
        {
            private ScriptTrivias _leftTrivias;
            private ScriptTrivias _rightTrivias;

            public void CompressSpaces(ScriptNode node)
            {
                if (node == null) return;

                _leftTrivias = null;
                _rightTrivias = null;

                node.RemoveLeadingSpace();
                Visit(node);
                if (_rightTrivias != null)
                {
                    bool previousHasSpaces = false;
                    CompactSpaces(_rightTrivias.After, ref previousHasSpaces);
                }
                node.RemoveTrailingSpace();
            }

            public override void Visit(ScriptNode node)
            {
                if (node == null) return;

                // Visit first to get nodes from left to right
                base.Visit(node);

                if (node is IScriptTerminal terminal)
                {
                    var trivias = terminal.Trivias;
                    if (trivias != null)
                    {
                        if (_leftTrivias == null)
                        {
                            _leftTrivias = trivias;

                            bool previousHasSpaces = false;
                            CompactSpaces(_leftTrivias.Before, ref previousHasSpaces);
                            previousHasSpaces = false;
                            CompactSpaces(_leftTrivias.After, ref previousHasSpaces);
                        }
                        else
                        {
                            if (_rightTrivias != null)
                            {
                                _leftTrivias = _rightTrivias;
                            }
                            _rightTrivias = trivias;

                            bool previousHasSpaces = false;
                            CompactSpaces(_leftTrivias.After, ref previousHasSpaces);
                            CompactSpaces(_rightTrivias.Before, ref previousHasSpaces);
                            previousHasSpaces = false;
                            CompactSpaces(_rightTrivias.After, ref previousHasSpaces);
                        }
                    }
                    else
                    {
                        // Reset if we have a terminal with no trivias
                        _leftTrivias = null;
                        _rightTrivias = null;
                    }
                }
            }

            private static void CompactSpaces(List<ScriptTrivia> trivias, ref bool previousHasSpaces)
            {
                for (var i = 0; i < trivias.Count; i++)
                {
                    var trivia = trivias[i];
                    var isNewLine = trivia.Type.IsNewLine();
                    if (trivia.Type.IsSpace() || isNewLine)
                    {
                        // If we have a new line we keep it, but we remove any trailing spaces
                        if (isNewLine)
                        {
                            trivias[i] = trivia.WithText(trivia.Text.TrimEndKeepNewLine());
                            RemoveSpacesBefore(trivias, ref i);
                        }
                        else
                        {
                            if (previousHasSpaces)
                            {
                                trivias.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                // Replace the existing trivia with a single space
                                trivias[i] = trivia.WithText((ScriptStringSlice) " ");
                            }
                        }

                        previousHasSpaces = true;
                    }
                    else
                    {
                        previousHasSpaces = false;

                        // Remove any whitespace trivias preceding a semi-colon
                        if (trivia.Type == ScriptTriviaType.SemiColon)
                        {
                            RemoveSpacesBefore(trivias, ref i);
                        }
                    }
                }
            }

            private static void RemoveSpacesBefore(List<ScriptTrivia> trivias, ref int i)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var previousTrivia = trivias[j];
                    if (previousTrivia.Type.IsSpace())
                    {
                        trivias.RemoveAt(j);
                        i--;
                    }
                    else if (previousTrivia.Type.IsNewLine())
                    {
                        trivias[j] = previousTrivia.WithText(previousTrivia.Text.TrimEndKeepNewLine());
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}