// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    /// <summary>
    /// Base class for a script rewriter.
    /// </summary>
    public abstract class ScriptRewriter : ScriptVisitor<ScriptNode>
    {
        protected ScriptRewriter()
        {
            CopyTrivias = true;
        }
        
        public bool CopyTrivias { get; set; }
        
        public override ScriptNode Visit(ScriptNode node)
        {
            if (node == null) return null;

            using (Push(node))
            {
                node = node.Accept(this);
            }

            return node;
        }

        protected virtual ScriptNode Normalize(ScriptNode newNode, ScriptNode previousNode)
        {
            if (CopyTrivias && newNode != previousNode)
            {
                return newNode.WithTriviaAndSpanFrom(previousNode);
            }
            return newNode;
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

            return Normalize(new ScriptTableRowStatement()
            {
                Variable = newVariable,
                Iterator = newIterator,
                NamedArguments = newNamedArguments,
                Body = newBody
            }, node);
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

            return Normalize(new ScriptCaseStatement
            {
                Value = newValue,
                Body = newBody
            }, node);
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

            return Normalize(new ScriptElseStatement
            {
                Body = newBody,
                Else = newElse
            }, node);
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

            return Normalize(new ScriptForStatement
            {
                Variable = newVariable,
                Iterator = newIterator,
                NamedArguments = newNamedArguments,
                Body = newBody
            }, node);
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

            return Normalize(new ScriptIfStatement
            {
                Condition = newCondition,
                Then = newThen,
                Else = newElse,
                InvertCondition = node.InvertCondition,
                IsElseIf = node.IsElseIf
            }, node);
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

            var newStatement = (ScriptWhenStatement)Normalize(new ScriptWhenStatement
            {
                Body = newBody,
                Next = newNext
            }, node);

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

            return Normalize(new ScriptWhileStatement
            {
                Condition = newCondition,
                Body = newBody
            }, node);
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

            var newExpression = (ScriptArrayInitializerExpression)Normalize(new ScriptArrayInitializerExpression(), node);
            newExpression.Values.AddRange(newValues);

            return newExpression;
        }

        public override ScriptNode Visit(ScriptAssignExpression node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);
            var newEqual = node.EqualToken != null ? (ScriptToken) Visit(node.EqualToken) : ScriptToken.Equal();
            var newValue = (ScriptExpression)Visit(node.Value);

            if (newTarget == node.Target &&
                newValue == node.Value &&
                newEqual == node.EqualToken)
            {
                return node;
            }

            return Normalize(new ScriptAssignExpression
            {
                Target = newTarget,
                EqualToken = newEqual,
                Value = newValue
            }, node);
        }

        public override ScriptNode Visit(ScriptBinaryExpression node)
        {
            var newLeft = (ScriptExpression)Visit(node.Left);
            var newRight = (ScriptExpression)Visit(node.Right);
            var newOperatorToken = node.OperatorToken != null ? (ScriptToken)Visit(node.OperatorToken) : node.Operator.ToToken();

            if (newLeft == node.Left &&
                newRight == node.Right &&
                newOperatorToken == node.OperatorToken)
            {
                return node;
            }

            return Normalize(new ScriptBinaryExpression
            {
                Left = newLeft,
                Operator = node.Operator,
                OperatorToken = newOperatorToken,
                Right = newRight
            }, node);
        }

        public override ScriptNode Visit(ScriptBlockStatement node)
        {
            var newStatements = VisitAll(node.Statements);

            if (newStatements == node.Statements)
            {
                return node;
            }

            var newBlockStatement = (ScriptBlockStatement)Normalize(new ScriptBlockStatement(), node);
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

            return Normalize(new ScriptCaptureStatement
            {
                Target = newTarget,
                Body = newBody
            }, node);
        }

        public override ScriptNode Visit(ScriptExpressionStatement node)
        {
            var newExpression = (ScriptExpression)Visit(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return Normalize(new ScriptExpressionStatement
            {
                Expression = newExpression
            }, node);
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

            var newCall = (ScriptFunctionCall)Normalize(new ScriptFunctionCall
            {
                Target = newTarget
            }, node);

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

            return Normalize(new ScriptImportStatement
            {
                Expression = newExpression
            }, node);
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

            return Normalize(new ScriptIndexerExpression
            {
                Target = newTarget,
                Index = newIndex
            }, node);
        }

        public override ScriptNode Visit(ScriptIsEmptyExpression node)
        {
            var newTarget = (ScriptExpression)Visit(node.Target);

            if (newTarget == node.Target)
            {
                return node;
            }

            return Normalize(new ScriptIsEmptyExpression
            {
                Target = newTarget
            }, node);
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

            return Normalize(new ScriptMemberExpression
            {
                Target = newTarget,
                Member = newMember
            }, node);
        }

        public override ScriptNode Visit(ScriptNamedArgument node)
        {
            var newValue = (ScriptExpression)Visit(node.Value);

            if (newValue == node.Value)
            {
                return node;
            }

            return Normalize(new ScriptNamedArgument
            {
                Name = node.Name,
                Value = newValue
            }, node);
        }

        public override ScriptNode Visit(ScriptNestedExpression node)
        {
            var newExpression = (ScriptExpression)Visit(node.Expression);

            if (newExpression == node.Expression)
            {
                return node;
            }

            return Normalize(new ScriptNestedExpression
            {
                Expression = newExpression
            }, node);
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

            var newExpression = (ScriptObjectInitializerExpression)Normalize(new ScriptObjectInitializerExpression(), node);
            foreach (var member in newMembers)
            {
                newExpression.Members.Add(member.Key, member.Value);
            }

            return newExpression;
        }

        public override ScriptNode Visit(ScriptPipeCall node)
        {
            var newFrom = (ScriptExpression)Visit(node.From);
            var newPipeToken = (ScriptToken)Visit(node.PipeToken);
            var newTo = (ScriptExpression)Visit(node.To);

            if (newFrom == node.From &&
                newPipeToken == node.PipeToken &&
                newTo == node.To)
            {
                return node;
            }

            return Normalize(new ScriptPipeCall
            {
                From = newFrom,
                PipeToken = newPipeToken,
                To = newTo
            }, node);
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

            return Normalize(new ScriptReturnStatement
            {
                Expression = newExpression
            }, node);
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

            return Normalize(new ScriptUnaryExpression
            {
                Operator = node.Operator,
                Right = newRight
            }, node);
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

            return Normalize(new ScriptWithStatement
            {
                Name = newName,
                Body = newBody
            }, node);
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

            return Normalize(new ScriptWrapStatement
            {
                Target = newTarget,
                Body = newBody
            }, node);
        }

        public override ScriptNode Visit(ScriptAnonymousFunction node)
        {
            var newFunction = (ScriptFunction)Visit((ScriptNode)node.Function);

            if (newFunction == node.Function)
            {
                return node;
            }

            return Normalize(new ScriptAnonymousFunction
            {
                Function = newFunction
            }, node);
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

            return Normalize(new ScriptFunction
            {
                Name = newName,
                Body = newBody
            }, node);
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

            return Normalize(new ScriptReadOnlyStatement
            {
                Variable = newVariable
            }, node);
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

            return Normalize(new ScriptPage
            {
                FrontMatter = newFrontMatter,
                Body = newBody
            }, node);
        }
        
        public override ScriptNode Visit(ScriptArgumentBinary node)
        {
            if (node.OperatorToken != null)
            {
                var newToken = (ScriptToken)Visit((ScriptNode)node.OperatorToken);
                if (newToken != node.OperatorToken)
                {
                    return Normalize(new ScriptArgumentBinary()
                    {
                        Operator = node.Operator, // TODO support rewriting?
                        OperatorToken = newToken
                    }, node);
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
