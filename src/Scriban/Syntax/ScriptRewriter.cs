// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    public class ScriptRewriter : ScriptVisitor<ScriptNode>
    {
        public override ScriptNode Visit(ScriptNode node)
        {
            if (node == null) return null;

            using (Push(node))
            {
                node = node.Accept(this);
            }

            return node;
        }

        public override ScriptNode Visit(ScriptTableRowStatement node)
        {
            var newVariable = (ScriptExpression) Visit(node.Variable);
            var newIterator = (ScriptExpression) Visit(node.Iterator);
            var newNamedArguments = VisitAll(node.NamedArguments);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptCaseStatement node)
        {
            var newValue = (ScriptExpression)Visit(node.Value);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptElseStatement node)
        {
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);
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

        public override ScriptNode Visit(ScriptForStatement node)
        {
            var newVariable = (ScriptExpression)Visit(node.Variable);
            var newIterator = (ScriptExpression)Visit(node.Iterator);
            var newNamedArguments = VisitAll(node.NamedArguments);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptIfStatement node)
        {
            var newCondition = (ScriptExpression)Visit(node.Condition);
            var newThen = (ScriptBlockStatement)Visit((ScriptNode)node.Then);
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

        public override ScriptNode Visit(ScriptWhenStatement node)
        {
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);
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

        public override ScriptNode Visit(ScriptWhileStatement node)
        {
            var newCondition = (ScriptExpression)Visit(node.Condition);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptVariableGlobal node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptVariableLocal node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptVariableLoop node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptArrayInitializerExpression node)
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

        public override ScriptNode Visit(ScriptAssignExpression node)
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

        public override ScriptNode Visit(ScriptBinaryExpression node)
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

        public override ScriptNode Visit(ScriptBlockStatement node)
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

        public override ScriptNode Visit(ScriptCaptureStatement node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptExpressionStatement node)
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

        public override ScriptNode Visit(ScriptFunctionCall node)
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

        public override ScriptNode Visit(ScriptImportStatement node)
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

        public override ScriptNode Visit(ScriptIndexerExpression node)
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

        public override ScriptNode Visit(ScriptIsEmptyExpression node)
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

        public override ScriptNode Visit(ScriptMemberExpression node)
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

        public override ScriptNode Visit(ScriptNamedArgument node)
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

        public override ScriptNode Visit(ScriptNestedExpression node)
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

        public override ScriptNode Visit(ScriptObjectInitializerExpression node)
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

        public override ScriptNode Visit(ScriptPipeCall node)
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

        public override ScriptNode Visit(ScriptRawStatement node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptReturnStatement node)
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

        public override ScriptNode Visit(ScriptThisExpression node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptUnaryExpression node)
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

        public override ScriptNode Visit(ScriptWithStatement node)
        {
            var newName = (ScriptExpression)Visit(node.Name);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptWrapStatement node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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

        public override ScriptNode Visit(ScriptAnonymousFunction node)
        {
            var newFunction = (ScriptFunction)Visit((ScriptNode)node.Function);

            if (newFunction == node.Function)
            {
                return node;
            }

            return new ScriptAnonymousFunction
            {
                Function = newFunction
            }.WithTriviaAndSpanFrom(node);
        }

        public override ScriptNode Visit(ScriptBreakStatement node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptContinueStatement node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptFunction node)
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

        public override ScriptNode Visit(ScriptLiteral node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptNopStatement node)
        {
            return node;
        }

        public override ScriptNode Visit(ScriptReadOnlyStatement node)
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

        public override ScriptNode Visit(ScriptPage node)
        {
            var newFrontMatter = (ScriptBlockStatement)Visit((ScriptNode)node.FrontMatter);
            var newBody = (ScriptBlockStatement)Visit((ScriptNode)node.Body);

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
        
        public override ScriptNode Visit(ScriptArgumentBinary node)
        {
            if (node.OperatorToken != null)
            {
                var newToken = (ScriptToken)Visit((ScriptNode)node.OperatorToken);
                if (newToken != node.OperatorToken)
                {
                    return new ScriptArgumentBinary()
                    {
                        Operator = node.Operator, // TODO support rewriting?
                        OperatorToken = newToken
                    };
                }
            }

            return node;
        }

        public override ScriptNode Visit(ScriptToken node)
        {
            return node;
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
                var newNode = (TNode) Visit(node);
                newNodes.Add(newNode);
                changed |= newNode != node;
            }

            if (changed)
                return newNodes;
            return nodes;
        }
    }
}
