// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Scriban.Syntax
{
    public class ScriptVisitor
    {
        private readonly ScriptVisitorContext _context = new ScriptVisitorContext();

        public IScriptVisitorContext Context => _context;

        public ScriptNode Visit(ScriptNode node)
        {
            if (node == null) return null;

            using (_context.Push(node))
            {
                node = DispatchVisit(node);
            }

            return node;
        }

        protected virtual ScriptNode DispatchVisit(ScriptNode node)
        {
            switch (node)
            {
                case ScriptTableRowStatement tableRowStatement:
                    return VisitTableRowStatement(tableRowStatement);
                case ScriptCaseStatement caseStatement:
                    return VisitCaseStatement(caseStatement);
                case ScriptElseStatement elseStatement:
                    return VisitElseStatement(elseStatement);
                case ScriptForStatement forStatement:
                    return VisitForStatement(forStatement);
                case ScriptIfStatement ifStatement:
                    return VisitIfStatement(ifStatement);
                case ScriptWhenStatement whenStatement:
                    return VisitWhenStatement(whenStatement);
                case ScriptWhileStatement whileStatement:
                    return VisitWhileStatement(whileStatement);
                case ScriptVariableGlobal variableGlobal:
                    return VisitVariableGlobal(variableGlobal);
                case ScriptVariableLocal variableLocal:
                    return VisitVariableLocal(variableLocal);
                case ScriptVariableLoop variableLoop:
                    return VisitVariableLoop(variableLoop);
                case ScriptArrayInitializerExpression arrayInitializerExpression:
                    return VisitArrayInitializerExpression(arrayInitializerExpression);
                case ScriptAssignExpression assignExpression:
                    return VisitAssignExpression(assignExpression);
                case ScriptBinaryExpression binaryExpression:
                    return VisitBinaryExpression(binaryExpression);
                case ScriptBlockStatement blockStatement:
                    return VisitBlockStatement(blockStatement);
                case ScriptCaptureStatement captureStatement:
                    return VisitCaptureStatement(captureStatement);
                case ScriptExpressionStatement expressionStatement:
                    return VisitExpressionStatement(expressionStatement);
                case ScriptFunctionCall functionCall:
                    return VisitFunctionCall(functionCall);
                case ScriptImportStatement importStatement:
                    return VisitImportStatement(importStatement);
                case ScriptIndexerExpression indexerExpression:
                    return VisitIndexerExpression(indexerExpression);
                case ScriptIsEmptyExpression isEmptyExpression:
                    return VisitIsEmptyExpression(isEmptyExpression);
                case ScriptMemberExpression memberExpression:
                    return VisitMemberExpression(memberExpression);
                case ScriptNamedArgument namedArgument:
                    return VisitNamedArgument(namedArgument);
                case ScriptNestedExpression nestedExpression:
                    return VisitNestedExpression(nestedExpression);
                case ScriptObjectInitializerExpression objectInitializerExpression:
                    return VisitObjectInitializerExpression(objectInitializerExpression);
                case ScriptPipeCall pipeCall:
                    return VisitPipeCall(pipeCall);
                case ScriptRawStatement rawStatement:
                    return VisitRawStatement(rawStatement);
                case ScriptReturnStatement returnStatement:
                    return VisitReturnStatement(returnStatement);
                case ScriptThisExpression thisExpression:
                    return VisitThisExpression(thisExpression);
                case ScriptUnaryExpression unaryExpression:
                    return VisitUnaryExpression(unaryExpression);
                case ScriptWithStatement withStatement:
                    return VisitWithStatement(withStatement);
                case ScriptWrapStatement wrapStatement:
                    return VisitWrapStatement(wrapStatement);
                case ScriptAnonymousFunction anonymousFunction:
                    return VisitAnonymousFunction(anonymousFunction);
                case ScriptBreakStatement breakStatement:
                    return VisitBreakStatement(breakStatement);
                case ScriptContinueStatement continueStatement:
                    return VisitContinueStatement(continueStatement);
                case ScriptFunction function:
                    return VisitFunction(function);
                case ScriptLiteral literal:
                    return VisitLiteral(literal);
                case ScriptNopStatement nopStatement:
                    return VisitNopStatement(nopStatement);
                case ScriptReadOnlyStatement readOnlyStatement:
                    return VisitReadOnlyStatement(readOnlyStatement);
                case ScriptPage page:
                    return VisitPage(page);
            }

            return node;
        }

        protected virtual ScriptNode VisitTableRowStatement(ScriptTableRowStatement node)
        {
            return new ScriptTableRowStatement
            {
                Variable = (ScriptExpression)Visit(node.Variable),
                Iterator = (ScriptExpression)Visit(node.Iterator),
                NamedArguments = node.NamedArguments?.Select(Visit).Cast<ScriptNamedArgument>().ToList(),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitCaseStatement(ScriptCaseStatement node)
        {
            return new ScriptCaseStatement
            {
                Value = (ScriptExpression)Visit(node.Value),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitElseStatement(ScriptElseStatement node)
        {
            return new ScriptElseStatement
            {
                Body = (ScriptBlockStatement)Visit(node.Body),
                Else = (ScriptConditionStatement)Visit(node.Else),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitForStatement(ScriptForStatement node)
        {
            return new ScriptForStatement
            {
                Variable = (ScriptExpression)Visit(node.Variable),
                Iterator = (ScriptExpression)Visit(node.Iterator),
                NamedArguments = node.NamedArguments?.Select(Visit).Cast<ScriptNamedArgument>().ToList(),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitIfStatement(ScriptIfStatement node)
        {
            return new ScriptIfStatement
            {
                Condition = (ScriptExpression)Visit(node.Condition),
                InvertCondition = node.InvertCondition,
                Then = (ScriptBlockStatement)Visit(node.Then),
                Else = (ScriptConditionStatement)Visit(node.Else),
                IsElseIf = node.IsElseIf,
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitWhenStatement(ScriptWhenStatement node)
        {
            var newStatement = new ScriptWhenStatement
            {
                Body = (ScriptBlockStatement)Visit(node.Body),
                Next = (ScriptConditionStatement)Visit(node.Next),
                Trivias = node.Trivias,
                Span = node.Span
            };

            newStatement.Values.AddRange(node.Values.Select(Visit).Cast<ScriptExpression>());

            return newStatement;
        }

        protected virtual ScriptNode VisitWhileStatement(ScriptWhileStatement node)
        {
            return new ScriptWhileStatement
            {
                Condition = (ScriptExpression)Visit(node.Condition),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitVariableGlobal(ScriptVariableGlobal node)
        {
            return new ScriptVariableGlobal(node.Name)
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitVariableLocal(ScriptVariableLocal node)
        {
            return new ScriptVariableLocal(node.Name)
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitVariableLoop(ScriptVariableLoop node)
        {
            return new ScriptVariableLoop(node.Name)
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitArrayInitializerExpression(ScriptArrayInitializerExpression node)
        {
            var newExpression = new ScriptArrayInitializerExpression
            {
                Trivias = node.Trivias,
                Span = node.Span
            };

            newExpression.Values.AddRange(node.Values.Select(Visit).Cast<ScriptExpression>());

            return newExpression;
        }

        protected virtual ScriptNode VisitAssignExpression(ScriptAssignExpression node)
        {
            return new ScriptAssignExpression
            {
                Target = (ScriptExpression)Visit(node.Target),
                Value = (ScriptExpression)Visit(node.Value),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitBinaryExpression(ScriptBinaryExpression node)
        {
            return new ScriptBinaryExpression
            {
                Left = (ScriptExpression)Visit(node.Left),
                Operator = node.Operator,
                Right = (ScriptExpression)Visit(node.Right),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitBlockStatement(ScriptBlockStatement node)
        {
            var newStatement = new ScriptBlockStatement
            {
                Trivias = node.Trivias,
                Span = node.Span
            };

            newStatement.Statements.AddRange(node.Statements.Select(Visit).Cast<ScriptStatement>());

            return newStatement;
        }

        protected virtual ScriptNode VisitCaptureStatement(ScriptCaptureStatement node)
        {
            return new ScriptCaptureStatement
            {
                Target = (ScriptExpression)Visit(node.Target),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitExpressionStatement(ScriptExpressionStatement node)
        {
            return new ScriptExpressionStatement
            {
                Expression = (ScriptExpression)Visit(node.Expression),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitFunctionCall(ScriptFunctionCall node)
        {
            var newCall = new ScriptFunctionCall
            {
                Target = (ScriptExpression)Visit(node.Target),
                Trivias = node.Trivias,
                Span = node.Span
            };

            newCall.Arguments.AddRange(node.Arguments.Select(Visit).Cast<ScriptExpression>());

            return newCall;
        }

        protected virtual ScriptNode VisitImportStatement(ScriptImportStatement node)
        {
            return new ScriptImportStatement
            {
                Expression = (ScriptExpression)Visit(node.Expression),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitIndexerExpression(ScriptIndexerExpression node)
        {
            return new ScriptIndexerExpression
            {
                Target = (ScriptExpression)Visit(node.Target),
                Index = (ScriptExpression)Visit(node.Index),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitIsEmptyExpression(ScriptIsEmptyExpression node)
        {
            return new ScriptIsEmptyExpression
            {
                Target = (ScriptExpression)Visit(node.Target),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitMemberExpression(ScriptMemberExpression node)
        {
            return new ScriptMemberExpression
            {
                Target = (ScriptExpression)Visit(node.Target),
                Member = (ScriptVariable)Visit(node.Member),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitNamedArgument(ScriptNamedArgument node)
        {
            return new ScriptNamedArgument
            {
                Name = node.Name,
                Value = (ScriptExpression)Visit(node.Value),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitNestedExpression(ScriptNestedExpression node)
        {
            return new ScriptNestedExpression
            {
                Expression = (ScriptExpression)Visit(node.Expression),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitObjectInitializerExpression(ScriptObjectInitializerExpression node)
        {
            var newExpression = new ScriptObjectInitializerExpression
            {
                Trivias = node.Trivias,
                Span = node.Span
            };

            foreach (var member in node.Members)
            {
                var newKey = (ScriptExpression)Visit(member.Key);
                var newValue = (ScriptExpression)Visit(member.Value);
                newExpression.Members.Add(newKey, newValue);
            }

            return newExpression;
        }

        protected virtual ScriptNode VisitPipeCall(ScriptPipeCall node)
        {
            return new ScriptPipeCall
            {
                From = (ScriptExpression)Visit(node.From),
                To = (ScriptExpression)Visit(node.To),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitRawStatement(ScriptRawStatement node)
        {
            return new ScriptRawStatement
            {
                Text = node.Text,
                EscapeCount = node.EscapeCount,
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitReturnStatement(ScriptReturnStatement node)
        {
            return new ScriptReturnStatement
            {
                Expression = (ScriptExpression)Visit(node.Expression),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitThisExpression(ScriptThisExpression node)
        {
            return new ScriptThisExpression
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitUnaryExpression(ScriptUnaryExpression node)
        {
            return new ScriptUnaryExpression
            {
                Operator = node.Operator,
                Right = (ScriptExpression)Visit(node.Right),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitWithStatement(ScriptWithStatement node)
        {
            return new ScriptWithStatement
            {
                Name = (ScriptExpression)Visit(node.Name),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitWrapStatement(ScriptWrapStatement node)
        {
            return new ScriptWrapStatement
            {
                Target = (ScriptExpression)Visit(node.Target),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitAnonymousFunction(ScriptAnonymousFunction node)
        {
            return new ScriptAnonymousFunction
            {
                Function = (ScriptFunction)Visit(node.Function),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitBreakStatement(ScriptBreakStatement node)
        {
            return new ScriptBreakStatement
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitContinueStatement(ScriptContinueStatement node)
        {
            return new ScriptContinueStatement
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitFunction(ScriptFunction node)
        {
            return new ScriptFunction
            {
                Name = (ScriptVariable)Visit(node.Name),
                Body = (ScriptStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitLiteral(ScriptLiteral node)
        {
            return new ScriptLiteral
            {
                Value = node.Value,
                StringQuoteType = node.StringQuoteType,
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitNopStatement(ScriptNopStatement node)
        {
            return new ScriptNopStatement
            {
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitReadOnlyStatement(ScriptReadOnlyStatement node)
        {
            return new ScriptReadOnlyStatement
            {
                Variable = (ScriptVariable)Visit(node.Variable),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        protected virtual ScriptNode VisitPage(ScriptPage node)
        {
            return new ScriptPage
            {
                FrontMatter = (ScriptBlockStatement)Visit(node.FrontMatter),
                Body = (ScriptBlockStatement)Visit(node.Body),
                Trivias = node.Trivias,
                Span = node.Span
            };
        }

        private class ScriptVisitorContext : IScriptVisitorContext
        {
            private readonly Stack<ScriptNode> _ancestors = new Stack<ScriptNode>();

            public ScriptNode Parent => _ancestors.Count > 0 ? _ancestors.Peek() : null;

            public IEnumerable<ScriptNode> Ancestors => _ancestors;

            public ScriptNode Current { get; private set; }

            public IDisposable Push(ScriptNode node)
            {
                if (Current != null)
                    _ancestors.Push(Current);
                Current = node;
                return new Popper(this);
            }

            private void Pop()
            {
                Current = _ancestors.Count > 0 ? _ancestors.Pop() : null;
            }

            class Popper : IDisposable
            {
                private readonly ScriptVisitorContext _context;

                public Popper(ScriptVisitorContext context)
                {
                    _context = context;
                }

                public void Dispose()
                {
                    _context.Pop();
                }
            }
        }
    }
}
