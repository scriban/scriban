// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    /// <summary>
    /// Base class for the abstract syntax tree of a scriban program.
    /// </summary>
    public abstract class ScriptNode
    {
        /// <summary>
        /// The source span of this node.
        /// </summary>
        public SourceSpan Span;

        /// <summary>
        /// Trivias, null if <see cref="LexerOptions.KeepTrivia"/> was false.
        /// </summary>
        public ScriptTrivias Trivias { get; set; }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public ScriptNode Parent { get; internal set; }

        /// <summary>
        /// Evaluates this instance with the specified context.
        /// </summary>
        /// <param name="context">The template context.</param>
        public abstract object Evaluate(TemplateContext context);

        public virtual int ChildrenCount => 0;

        /// <summary>
        /// Clones this node including its trivias.
        /// </summary>
        /// <returns>Return a clone of this node.</returns>
        public ScriptNode Clone()
        {
            return Clone(true);
        }

        /// <summary>
        /// Clones this node.
        /// </summary>
        /// <param name="withTrivias"><c>true</c> to copy the trivias.</param>
        /// <returns>Return a clone of this node.</returns>
        public ScriptNode Clone(bool withTrivias)
        {
            var cloner = withTrivias ? ScriptCloner.WithTrivias : ScriptCloner.Instance;
            return cloner.Visit(this);
        }

        /// <summary>
        /// Gets a children at the specified index.
        /// </summary>
        /// <param name="index">Index of the children</param>
        /// <returns>A children at the specified index</returns>
        public ScriptNode GetChildren(int index)
        {
            if (index < 0) throw ThrowHelper.GetIndexNegativeArgumentOutOfRangeException();
            if (index > ChildrenCount) throw ThrowHelper.GetIndexArgumentOutOfRangeException(ChildrenCount);
            return GetChildrenImpl(index);
        }

        /// <summary>
        /// Gets a children at the specified index.
        /// </summary>
        /// <param name="index">Index of the children</param>
        /// <returns>A children at the specified index</returns>
        /// <remarks>The index is safe to use</remarks>
        protected virtual ScriptNode GetChildrenImpl(int index) => null;

#if !SCRIBAN_NO_ASYNC
        public virtual ValueTask<object> EvaluateAsync(TemplateContext context)
        {
            return new ValueTask<object>(Evaluate(context));
        }
#endif

        public virtual bool CanHaveLeadingTrivia()
        {
            return true;
        }

        public abstract void Write(TemplateRewriterContext context);

        public virtual void Accept(ScriptVisitor visitor) => throw new NotImplementedException($"This method must be implemented by the type {this.GetType()}");

        public virtual TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => throw new NotImplementedException($"This method must be implemented by the type {this.GetType()}");

        public IEnumerable<ScriptNode> Children
        {
            get
            {
                var count = ChildrenCount;
                for (int i = 0; i < count; i++)
                {
                    yield return GetChildrenImpl(i);
                }
            }
        }

        /// <summary>
        /// Helper method to deparent/parent a node to this instance.
        /// </summary>
        /// <typeparam name="TSyntaxNode">Type of the node</typeparam>
        /// <param name="set">The previous child node parented to this instance</param>
        /// <param name="node">The new child node to parent to this instance</param>
        protected void ParentToThis<TSyntaxNode>(ref TSyntaxNode set, TSyntaxNode node) where TSyntaxNode : ScriptNode
        {
            if (node == set) return;
            if (node?.Parent != null) throw ThrowHelper.GetExpectingNoParentException();
            if (set != null)
            {
                set.Parent = null;
            }
            if (node != null)
            {
                node.Parent = this;
            }
            set = node;
        }


        public sealed override string ToString()
        {
            var strOutput = new StringBuilderOutput();
            var template = new TemplateRewriterContext(strOutput , new TemplateRewriterOptions() { Mode = ScriptMode.ScriptOnly });
            template.Write(this);
            return strOutput.ToString();
        }
    }

    public static class ScriptNodeExtensions
    {
        public static ScriptNode AddSpaceBefore(this ScriptNode node)
        {
            node.AddTrivia(ScriptTrivia.Space, true);
            return node;
        }

        public static ScriptNode AddComma(this ScriptNode node)
        {
            node.AddTrivia(ScriptTrivia.Comma, true);
            return node;
        }

        public static ScriptNode AddSemiColon(this ScriptNode node)
        {
            node.AddTrivia(ScriptTrivia.SemiColon, true);
            return node;
        }

        public static ScriptNode AddSpaceAfter(this ScriptNode node)
        {
            node.AddTrivia(ScriptTrivia.Space, false);
            return node;
        }

        public static void AddTrivia(this ScriptNode node, ScriptTrivia trivia, bool before)
        {
            var trivias = node.Trivias;
            if (trivias == null)
            {
                node.Trivias = trivias = new ScriptTrivias();
            }

            (before ? trivias.Before : trivias.After).Add(trivia);
        }

        public static void AddTrivias<T>(this ScriptNode node, T trivias, bool before) where T : IEnumerable<ScriptTrivia>
        {
            foreach (var trivia in trivias)
            {
                node.AddTrivia(trivia, before);
            }
        }

        public static bool HasTrivia(this ScriptNode node, ScriptTriviaType triviaType, bool before)
        {
            if (node.Trivias == null)
            {
                return false;
            }

            foreach (var trivia in (before ? node.Trivias.Before : node.Trivias.After))
            {
                if (trivia.Type == triviaType)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasTriviaEndOfStatement(this ScriptNode node, bool before)
        {
            if (node.Trivias == null)
            {
                return false;
            }

            foreach (var trivia in (before ? node.Trivias.Before : node.Trivias.After))
            {
                if (trivia.Type == ScriptTriviaType.NewLine || trivia.Type == ScriptTriviaType.SemiColon)
                {
                    return true;
                }
            }
            return false;
        }

        public static TNode WithTriviaAndSpanFrom<TNode>(this TNode node, ScriptNode sourceNode)
            where TNode : ScriptNode
        {
            node.Trivias = sourceNode?.Trivias;
            node.Span = sourceNode?.Span ?? default;
            return node;
        }
    }

    public class ScriptTrivias
    {
        public ScriptTrivias()
        {
            Before = new List<ScriptTrivia>();
            After = new List<ScriptTrivia>();
        }

        public List<ScriptTrivia> Before { get; }

        public List<ScriptTrivia> After { get; }
    }

    public readonly struct ScriptTrivia
    {
        public static readonly ScriptTrivia Space = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.Whitespace, " ");

        public static readonly ScriptTrivia Comma = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.Comma, ",");

        public static readonly ScriptTrivia SemiColon = new ScriptTrivia(new SourceSpan(), ScriptTriviaType.SemiColon, ";");

        public ScriptTrivia(SourceSpan span, ScriptTriviaType type)
        {
            Span = span;
            Type = type;
            Text = null;
        }

        public ScriptTrivia(SourceSpan span, ScriptTriviaType type, string text)
        {
            Span = span;
            Type = type;
            Text = text;
        }

        public readonly SourceSpan Span;

        public readonly ScriptTriviaType Type;

        public readonly string Text;

        public void Write(TemplateRewriterContext context)
        {
            var rawText = ToString();

            bool isRawComment = Type == ScriptTriviaType.CommentMulti && !rawText.StartsWith("##");
            if (isRawComment)
            {
                // Escape any # by \#
                rawText = rawText.Replace("#", "\\#");
                // Escape any }}
                rawText = rawText.Replace("}", "\\}");
                context.Write("## ");
            }

            context.Write(rawText);

            if (isRawComment)
            {
                context.Write(" ##");
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ScriptTriviaType.Empty:
                    return string.Empty;
                case ScriptTriviaType.Comma:
                    return ",";
                case ScriptTriviaType.SemiColon:
                    return ";";
            }
            var length = Span.End.Offset - Span.Start.Offset + 1;
            return Text?.Substring(Span.Start.Offset, length);
        }
    }

    public enum ScriptTriviaType
    {
        Empty = 0,

        Whitespace,

        WhitespaceFull,

        Comment,

        Comma,

        CommentMulti,

        NewLine,

        SemiColon,
    }
}