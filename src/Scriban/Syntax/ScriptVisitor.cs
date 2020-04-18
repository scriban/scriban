// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Syntax
{
    public abstract class ScriptVisitor
    {
        private readonly ScriptVisitorContext _context = new ScriptVisitorContext();

        public IScriptVisitorContext Context => _context;

        protected IDisposable Push(ScriptNode node) => _context.Push(node);

        public virtual void Visit(ScriptNode node)
        {
            if (node == null)
                return;

            using (Push(node))
            {
                node.Accept(this);
            }
        }

        public virtual void DefaultVisit(ScriptNode node)
        {
            if (node == null)
                return;

            foreach (var child in node.Children)
            {
                Visit(child);
            }
        }

        public virtual void Visit(ScriptTableRowStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptCaseStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptElseStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptForStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptIfStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptWhenStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptWhileStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptVariableGlobal node) => DefaultVisit(node);

        public virtual void Visit(ScriptVariableLocal node) => DefaultVisit(node);

        public virtual void Visit(ScriptVariableLoop node) => DefaultVisit(node);

        public virtual void Visit(ScriptArrayInitializerExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptAssignExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptBinaryExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptBlockStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptCaptureStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptExpressionStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptFunctionCall node) => DefaultVisit(node);

        public virtual void Visit(ScriptImportStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptIndexerExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptIsEmptyExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptMemberExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptNamedArgument node) => DefaultVisit(node);

        public virtual void Visit(ScriptNestedExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptObjectInitializerExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptPipeCall node) => DefaultVisit(node);

        public virtual void Visit(ScriptRawStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptReturnStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptThisExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptUnaryExpression node) => DefaultVisit(node);

        public virtual void Visit(ScriptWithStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptWrapStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptAnonymousFunction node) => DefaultVisit(node);

        public virtual void Visit(ScriptBreakStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptContinueStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptFunction node) => DefaultVisit(node);

        public virtual void Visit(ScriptLiteral node) => DefaultVisit(node);

        public virtual void Visit(ScriptNopStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptReadOnlyStatement node) => DefaultVisit(node);

        public virtual void Visit(ScriptPage node) => DefaultVisit(node);
    }

    public abstract class ScriptVisitor<TResult>
    {
        private readonly ScriptVisitorContext _context = new ScriptVisitorContext();

        public IScriptVisitorContext Context => _context;

        protected IDisposable Push(ScriptNode node) => _context.Push(node);

        public virtual TResult Visit(ScriptNode node)
        {
            if (node == null)
                return default;

            using (Push(node))
            {
                return node.Accept(this);
            }
        }

        public virtual TResult DefaultVisit(ScriptNode node)
        {
            if (node == null)
                return default;

            foreach (var child in node.Children)
            {
                Visit(child);
            }

            return default;
        }

        public virtual TResult Visit(ScriptTableRowStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptCaseStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptElseStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptForStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptIfStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptWhenStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptWhileStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptVariableGlobal node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptVariableLocal node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptVariableLoop node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptArrayInitializerExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptAssignExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptBinaryExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptBlockStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptCaptureStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptExpressionStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptFunctionCall node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptImportStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptIndexerExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptIsEmptyExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptMemberExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptNamedArgument node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptNestedExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptObjectInitializerExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptPipeCall node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptRawStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptReturnStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptThisExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptUnaryExpression node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptWithStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptWrapStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptAnonymousFunction node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptBreakStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptContinueStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptFunction node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptLiteral node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptNopStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptReadOnlyStatement node) => DefaultVisit(node);

        public virtual TResult Visit(ScriptPage node) => DefaultVisit(node);
    }
}
