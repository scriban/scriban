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

        private ScriptNode VisitCore(ScriptNode node)
        {
            if (node == null) return null;

            using (_context.Push(node))
            {
                node = node.Accept(this);
            }

            return node;
        }

        public virtual ScriptNode Visit(ScriptTableRowStatement node)
        {
            var newVariable = (ScriptExpression) VisitCore(node.Variable);
            var newIterator = (ScriptExpression) VisitCore(node.Iterator);
            var newNamedArguments = VisitAll(node.NamedArguments);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptCaseStatement node)
        {
            var newValue = (ScriptExpression)VisitCore(node.Value);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptElseStatement node)
        {
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);
            var newElse = (ScriptConditionStatement)VisitCore(node.Else);

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

        public virtual ScriptNode Visit(ScriptForStatement node)
        {
            var newVariable = (ScriptExpression)VisitCore(node.Variable);
            var newIterator = (ScriptExpression)VisitCore(node.Iterator);
            var newNamedArguments = VisitAll(node.NamedArguments);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptIfStatement node)
        {
            var newCondition = (ScriptExpression)VisitCore(node.Condition);
            var newThen = (ScriptBlockStatement)VisitCore(node.Then);
            var newElse = (ScriptConditionStatement)VisitCore(node.Else);

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

        public virtual ScriptNode Visit(ScriptWhenStatement node)
        {
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);
            var newNext = (ScriptConditionStatement)VisitCore(node.Next);
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

        public virtual ScriptNode Visit(ScriptWhileStatement node)
        {
            var newCondition = (ScriptExpression)VisitCore(node.Condition);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptVariableGlobal node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptVariableLocal node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptVariableLoop node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptArrayInitializerExpression node)
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

        public virtual ScriptNode Visit(ScriptAssignExpression node)
        {
            var newTarget = (ScriptExpression)VisitCore(node.Target);
            var newValue = (ScriptExpression)VisitCore(node.Value);

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

        public virtual ScriptNode Visit(ScriptBinaryExpression node)
        {
            var newLeft = (ScriptExpression)VisitCore(node.Left);
            var newRight = (ScriptExpression)VisitCore(node.Right);

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

        public virtual ScriptNode Visit(ScriptBlockStatement node)
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

        public virtual ScriptNode Visit(ScriptCaptureStatement node)
        {
            var newTarget = (ScriptExpression)VisitCore(node.Target);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptExpressionStatement node)
        {
            var newExpression = (ScriptExpression)VisitCore(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptExpressionStatement
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptFunctionCall node)
        {
            var newTarget = (ScriptExpression)VisitCore(node.Target);
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

        public virtual ScriptNode Visit(ScriptImportStatement node)
        {
            var newExpression = (ScriptExpression)VisitCore(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptImportStatement
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptIndexerExpression node)
        {
            var newTarget = (ScriptExpression)VisitCore(node.Target);
            var newIndex = (ScriptExpression)VisitCore(node.Index);

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

        public virtual ScriptNode Visit(ScriptIsEmptyExpression node)
        {
            var newTarget = (ScriptExpression)VisitCore(node.Target);

            if (newTarget == node.Target)
            {
                return node;
            }

            return new ScriptIsEmptyExpression
            {
                Target = newTarget
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptMemberExpression node)
        {
            var newTarget = (ScriptExpression) VisitCore(node.Target);
            var newMember = (ScriptVariable) VisitCore(node.Member);

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

        public virtual ScriptNode Visit(ScriptNamedArgument node)
        {
            var newValue = (ScriptExpression)VisitCore(node.Value);

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

        public virtual ScriptNode Visit(ScriptNestedExpression node)
        {
            var newExpression = (ScriptExpression)VisitCore(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptNestedExpression
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptObjectInitializerExpression node)
        {
            var newMembers = new Dictionary<ScriptExpression, ScriptExpression>();
            bool changed = false;
            foreach (var member in node.Members)
            {
                var newKey = (ScriptExpression)VisitCore(member.Key);
                changed |= newKey != member.Key;

                var newValue = (ScriptExpression)VisitCore(member.Value);
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

        public virtual ScriptNode Visit(ScriptPipeCall node)
        {
            var newFrom = (ScriptExpression)VisitCore(node.From);
            var newTo = (ScriptExpression)VisitCore(node.To);

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

        public virtual ScriptNode Visit(ScriptRawStatement node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptReturnStatement node)
        {
            var newExpression = (ScriptExpression)VisitCore(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return new ScriptReturnStatement
            {
                Expression = newExpression
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptThisExpression node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptUnaryExpression node)
        {
            var newRight = (ScriptExpression)VisitCore(node.Right);

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

        public virtual ScriptNode Visit(ScriptWithStatement node)
        {
            var newName = (ScriptExpression)VisitCore(node.Name);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptWrapStatement node)
        {
            var newTarget = (ScriptExpression)VisitCore(node.Target);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptAnonymousFunction node)
        {
            var newFunction = (ScriptFunction)VisitCore(node.Function);

            if (newFunction == node.Function)
            {
                return node;
            }

            return new ScriptAnonymousFunction
            {
                Function = newFunction
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptBreakStatement node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptContinueStatement node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptFunction node)
        {
            var newName = (ScriptVariable)VisitCore(node.Name);
            var newBody = (ScriptStatement)VisitCore(node.Body);

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

        public virtual ScriptNode Visit(ScriptLiteral node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptNopStatement node)
        {
            return node;
        }

        public virtual ScriptNode Visit(ScriptReadOnlyStatement node)
        {
            var newVariable = (ScriptVariable)VisitCore(node.Variable);

            if (newVariable == node.Variable)
            {
                return node;
            }

            return new ScriptReadOnlyStatement
            {
                Variable = newVariable
            }.WithTriviaAndSpanFrom(node);
        }

        public virtual ScriptNode Visit(ScriptPage node)
        {
            var newFrontMatter = (ScriptBlockStatement)VisitCore(node.FrontMatter);
            var newBody = (ScriptBlockStatement)VisitCore(node.Body);

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

        protected List<TNode> VisitAll<TNode>(List<TNode> nodes)
            where TNode : ScriptNode
        {
            if (nodes == null)
                return null;

            var newNodes = new List<TNode>();
            bool changed = false;
            foreach (var node in nodes)
            {
                var newNode = (TNode) VisitCore(node);
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
