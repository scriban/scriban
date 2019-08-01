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
            var newVariable = (ScriptExpression) Visit(node.Variable);
            var newIterator = (ScriptExpression) Visit(node.Iterator);
            var newNamedArguments = VisitAll(node.NamedArguments);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newVariable == node.Variable &&
                newIterator == node.Iterator &&
                newNamedArguments == node.NamedArguments &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptTableRowStatement()
            {
                Variable = newVariable,
                Iterator = newIterator,
                NamedArguments = newNamedArguments,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitCaseStatement(ScriptCaseStatement node)
        {
            var newValue = (ScriptExpression)Visit(node.Value);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newValue == node.Value &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptCaseStatement
            {
                Value = newValue,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitElseStatement(ScriptElseStatement node)
        {
            var newBody = (ScriptBlockStatement)Visit(node.Body);
            var newElse = (ScriptConditionStatement)Visit(node.Else);

            if (newBody == node.Body &&
                newElse == node.Else)
            {
                return node;
            }

            return new ScriptElseStatement
            {
                Body = newBody,
                Else = newElse
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitForStatement(ScriptForStatement node)
        {
            var newVariable = (ScriptExpression)Visit(node.Variable);
            var newIterator = (ScriptExpression)Visit(node.Iterator);
            var newNamedArguments = VisitAll(node.NamedArguments);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newVariable == node.Variable &&
                newIterator == node.Iterator &&
                newNamedArguments == node.NamedArguments &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptForStatement
            {
                Variable = newVariable,
                Iterator = newIterator,
                NamedArguments = newNamedArguments,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitIfStatement(ScriptIfStatement node)
        {
            var newCondition = (ScriptExpression)Visit(node.Condition);
            var newThen = (ScriptBlockStatement)Visit(node.Then);
            var newElse = (ScriptConditionStatement)Visit(node.Else);

            if (newCondition == node.Condition &&
                newThen == node.Then &&
                newElse == node.Else)
            {
                return node;
            }

            return new ScriptIfStatement
            {
                Condition = newCondition,
                Then = newThen,
                Else = newElse,
                InvertCondition = node.InvertCondition,
                IsElseIf = node.IsElseIf
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitWhenStatement(ScriptWhenStatement node)
        {
            var newBody = (ScriptBlockStatement)Visit(node.Body);
            var newNext = (ScriptConditionStatement)Visit(node.Next);
            var newValues = VisitAll(node.Values);

            if (newBody == node.Body &&
                newNext == node.Next &&
                newValues == node.Values)
            {
                return node;
            }

            var newStatement = new ScriptWhenStatement
            {
                Body = newBody,
                Next = newNext
            }.WithTriviaAndSpanFrom(node);

            newStatement.Values.AddRange(newValues);

            return newStatement;
        }

        protected virtual ScriptNode VisitWhileStatement(ScriptWhileStatement node)
        {
            var newCondition = (ScriptExpression)Visit(node.Condition);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newCondition == node.Condition &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptWhileStatement
            {
                Condition = newCondition,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitVariableGlobal(ScriptVariableGlobal node)
        {
            return node;
        }

        protected virtual ScriptNode VisitVariableLocal(ScriptVariableLocal node)
        {
            return node;
        }

        protected virtual ScriptNode VisitVariableLoop(ScriptVariableLoop node)
        {
            return node;
        }

        protected virtual ScriptNode VisitArrayInitializerExpression(ScriptArrayInitializerExpression node)
        {
            var newValues = VisitAll(node.Values);

            if (newValues == node.Values)
            {
                return node;
            }

            var newExpression = new ScriptArrayInitializerExpression().WithTriviaAndSpanFrom(node);
            newExpression.Values.AddRange(newValues);

            return newExpression;
        }

        protected virtual ScriptNode VisitAssignExpression(ScriptAssignExpression node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newValue = (ScriptExpression)Visit(node.Value);

            if (newTarget == node.Target &&
                newValue == node.Value)
            {
                return node;
            }

            return new ScriptAssignExpression
            {
                Target = newTarget,
                Value = newValue
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitBinaryExpression(ScriptBinaryExpression node)
        {
            var newLeft = (ScriptExpression)Visit(node.Left);
            var newRight = (ScriptExpression)Visit(node.Right);

            if (newLeft == node.Left &&
                newRight == node.Right)
            {
                return node;
            }

            return new ScriptBinaryExpression
            {
                Left = newLeft,
                Operator = node.Operator,
                Right = newRight
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitBlockStatement(ScriptBlockStatement node)
        {
            var newStatements = VisitAll(node.Statements);

            if (newStatements == node.Statements)
            {
                return node;
            }

            var newBlockStatement = new ScriptBlockStatement().WithTriviaAndSpanFrom(node);
            newBlockStatement.Statements.AddRange(newStatements);

            return newBlockStatement;
        }

        protected virtual ScriptNode VisitCaptureStatement(ScriptCaptureStatement node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newTarget == node.Target &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptCaptureStatement
            {
                Target = newTarget,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitExpressionStatement(ScriptExpressionStatement node)
        {
            var newExpression = (ScriptExpression)Visit(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptExpressionStatement
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitFunctionCall(ScriptFunctionCall node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newArguments = VisitAll(node.Arguments);

            if (newTarget == node.Target &&
                newArguments == node.Arguments)
            {
                return node;
            }

            var newCall = new ScriptFunctionCall
            {
                Target = newTarget
            }.WithTriviaAndSpanFrom(node);

            newCall.Arguments.AddRange(newArguments);

            return newCall;
        }

        protected virtual ScriptNode VisitImportStatement(ScriptImportStatement node)
        {
            var newExpression = (ScriptExpression)Visit(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptImportStatement
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitIndexerExpression(ScriptIndexerExpression node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newIndex = (ScriptExpression)Visit(node.Index);

            if (newTarget == node.Target &&
                newIndex == node.Index)
            {
                return node;
            }

            return new ScriptIndexerExpression
            {
                Target = newTarget,
                Index = newIndex
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitIsEmptyExpression(ScriptIsEmptyExpression node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);

            if (newTarget == node.Target)
            {
                return node;
            }

            return new ScriptIsEmptyExpression
            {
                Target = newTarget
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitMemberExpression(ScriptMemberExpression node)
        {
            var newTarget = (ScriptExpression) Visit(node.Target);
            var newMember = (ScriptVariable) Visit(node.Member);

            if (newTarget == node.Target &&
                newMember == node.Member)
            {
                return node;
            }

            return new ScriptMemberExpression
            {
                Target = newTarget,
                Member = newMember
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitNamedArgument(ScriptNamedArgument node)
        {
            var newValue = (ScriptExpression)Visit(node.Value);

            if (newValue == node.Value)
            {
                return node;
            }

            return new ScriptNamedArgument
            {
                Name = node.Name,
                Value = newValue
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitNestedExpression(ScriptNestedExpression node)
        {
            var newExpression = (ScriptExpression)Visit(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptNestedExpression
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitObjectInitializerExpression(ScriptObjectInitializerExpression node)
        {
            var newMembers = new Dictionary<ScriptExpression, ScriptExpression>();
            bool changed = false;
            foreach (var member in node.Members)
            {
                var newKey = (ScriptExpression)Visit(member.Key);
                changed |= newKey != member.Key;

                var newValue = (ScriptExpression)Visit(member.Value);
                changed |= newValue != member.Value;

                newMembers.Add(newKey, newValue);
            }

            if (!changed)
            {
                return node;
            }

            var newExpression = new ScriptObjectInitializerExpression().WithTriviaAndSpanFrom(node);
            foreach (var member in newMembers)
            {
                newExpression.Members.Add(member.Key, member.Value);
            }

            return newExpression;
        }

        protected virtual ScriptNode VisitPipeCall(ScriptPipeCall node)
        {
            var newFrom = (ScriptExpression)Visit(node.From);
            var newTo = (ScriptExpression)Visit(node.To);

            if (newFrom == node.From &&
                newTo == node.To)
            {
                return node;
            }

            return new ScriptPipeCall
            {
                From = newFrom,
                To = newTo
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitRawStatement(ScriptRawStatement node)
        {
            return node;
        }

        protected virtual ScriptNode VisitReturnStatement(ScriptReturnStatement node)
        {
            var newExpression = (ScriptExpression)Visit(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptReturnStatement
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitThisExpression(ScriptThisExpression node)
        {
            return node;
        }

        protected virtual ScriptNode VisitUnaryExpression(ScriptUnaryExpression node)
        {
            var newRight = (ScriptExpression)Visit(node.Right);

            if (newRight == node.Right)
            {
                return node;
            }

            return new ScriptUnaryExpression
            {
                Operator = node.Operator,
                Right = newRight
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitWithStatement(ScriptWithStatement node)
        {
            var newName = (ScriptExpression)Visit(node.Name);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newName == node.Name &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptWithStatement
            {
                Name = newName,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitWrapStatement(ScriptWrapStatement node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newTarget == node.Target &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptWrapStatement
            {
                Target = newTarget,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitAnonymousFunction(ScriptAnonymousFunction node)
        {
            var newFunction = (ScriptFunction)Visit(node.Function);

            if (newFunction == node.Function)
            {
                return node;
            }

            return new ScriptAnonymousFunction
            {
                Function = newFunction
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitBreakStatement(ScriptBreakStatement node)
        {
            return node;
        }

        protected virtual ScriptNode VisitContinueStatement(ScriptContinueStatement node)
        {
            return node;
        }

        protected virtual ScriptNode VisitFunction(ScriptFunction node)
        {
            var newName = (ScriptVariable)Visit(node.Name);
            var newBody = (ScriptStatement)Visit(node.Body);

            if (newName == node.Name &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptFunction
            {
                Name = newName,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitLiteral(ScriptLiteral node)
        {
            return node;
        }

        protected virtual ScriptNode VisitNopStatement(ScriptNopStatement node)
        {
            return node;
        }

        protected virtual ScriptNode VisitReadOnlyStatement(ScriptReadOnlyStatement node)
        {
            var newVariable = (ScriptVariable)Visit(node.Variable);

            if (newVariable == node.Variable)
            {
                return node;
            }

            return new ScriptReadOnlyStatement
            {
                Variable = newVariable
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual ScriptNode VisitPage(ScriptPage node)
        {
            var newFrontMatter = (ScriptBlockStatement)Visit(node.FrontMatter);
            var newBody = (ScriptBlockStatement)Visit(node.Body);

            if (newFrontMatter == node.FrontMatter &&
                newBody == node.Body)
            {
                return node;
            }

            return new ScriptPage
            {
                FrontMatter = newFrontMatter,
                Body = newBody
            }.WithTriviaAndSpanFrom(node);
        }

        protected virtual List<TNode> VisitAll<TNode>(List<TNode> nodes)
            where TNode : ScriptNode
        {
            if (nodes == null)
                return null;

            var newNodes = new List<TNode>();
            bool changed = false;
            foreach (var node in nodes)
            {
                var newNode = (TNode) Visit(node);
                newNodes.Add(newNode);
                changed |= newNode != node;
            }

            if (changed)
                return newNodes;
            return nodes;
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
