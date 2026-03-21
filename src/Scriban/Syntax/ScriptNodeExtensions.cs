// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptNodeExtensions
    {
        public static ScriptNode? FindFirstTerminal(this ScriptNode? node)
        {
            if (node is null) return null;
            if (node is IScriptTerminal)
            {
                return node;
            }

            var count = node.ChildrenCount;
            for (int i = 0; i < count; i++)
            {
                var child = node.GetChildren(i);
                if (child is not null)
                {
                    // TODO: could be optimized with a stack
                    var first = FindFirstTerminal(child);
                    if (first is not null)
                    {
                        return first;
                    }
                }
            }
            return null;
        }

        public static ScriptNode? FindLastTerminal(this ScriptNode? node)
        {
            if (node is null) return null;
            if (node is IScriptTerminal)
            {
                return node;
            }

            var count = node.ChildrenCount;
            for (int i = count - 1; i >= 0; i--)
            {
                var child = node.GetChildren(i);
                if (child is not null)
                {
                    // TODO: could be optimized with a stack
                    var last = FindLastTerminal(child);
                    if (last is not null)
                    {
                        return last;
                    }
                }
            }
            return null;
        }

        public static T RemoveLeadingSpace<T>(this T node) where T : ScriptNode
        {
            var firstTerminal = FindFirstTerminal(node) as IScriptTerminal;
            var trivias = firstTerminal?.Trivias;
            if (trivias is not null)
            {
                var triviasBefore = trivias.Before;
                if (triviasBefore.Count > 0)
                {
                    for (int i = 0; i < triviasBefore.Count; i++)
                    {
                        var trivia = triviasBefore[i];
                        if (trivia.Type.IsSpaceOrNewLine())
                        {
                            triviasBefore.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return node;
        }

        public static T RemoveTrailingSpace<T>(this T node) where T : ScriptNode
        {
            var lastTerminal = FindLastTerminal(node) as IScriptTerminal;
            var trivias = lastTerminal?.Trivias;
            if (trivias is not null)
            {
                var triviasAfter = trivias.After;
                if (triviasAfter.Count > 0)
                {
                    for (var i = triviasAfter.Count - 1; i >= 0; i--)
                    {
                        var trivia = triviasAfter[i];
                        if (trivia.Type.IsSpaceOrNewLine())
                        {
                            triviasAfter.RemoveAt(i);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return node;
        }

        public static void MoveLeadingTriviasTo<T>(this ScriptNode node, T destinationNode) where T : ScriptNode, IScriptTerminal
        {
            var firstTerminal = node.FindFirstTerminal() as IScriptTerminal;

            var trivias = firstTerminal?.Trivias;
            if (trivias is not null)
            {
                var before = trivias.Before;
                foreach (var trivia in before)
                {
                    destinationNode.AddTrivia(trivia, true);
                }
                before.Clear();
            }
        }

        public static void MoveTrailingTriviasTo<T>(this ScriptNode node, T destinationNode, bool before) where T : ScriptNode, IScriptTerminal
        {
            var lastTerminal = node.FindLastTerminal() as IScriptTerminal;

            var trivias = lastTerminal?.Trivias;
            if (trivias is not null)
            {
                var after = trivias.After;
                if (before)
                {
                    for (var i = after.Count - 1; i >= 0; i--)
                    {
                        var trivia = after[i];
                        destinationNode.InsertTrivia(trivia, false);
                    }
                }
                else
                {
                    foreach (var trivia in after)
                    {
                        destinationNode.AddTrivia(trivia, false);
                    }
                }
                after.Clear();
            }
        }

        public static void AddLeadingSpace(this IScriptTerminal node)
        {
            if (!node.HasLeadingSpaceTrivias())
            {
                node.AddTrivia(ScriptTrivia.Space, true);
            }
        }

        public static void AddCommaAfter(this IScriptTerminal node)
        {
            if (!node.HasTrivia(ScriptTriviaType.Comma, false))
            {
                node.AddTrivia(ScriptTrivia.Comma, false);
            }
        }

        public static void AddSemiColonAfter(this IScriptTerminal node)
        {
            if (!node.HasTrivia(ScriptTriviaType.SemiColon, false))
            {
                node.AddTrivia(ScriptTrivia.SemiColon, false);
            }
        }

        public static void AddSpaceAfter(this IScriptTerminal node)
        {
            if (!node.HasTrailingSpaceTrivias())
            {
                node.AddTrivia(ScriptTrivia.Space, false);
            }
        }

        public static void AddTrivia(this IScriptTerminal node, ScriptTrivia trivia, bool before)
        {
            var trivias = node.Trivias;
            if (trivias is null)
            {
                node.Trivias = trivias = new ScriptTrivias();
            }
            (before ? trivias.Before : trivias.After).Add(trivia);
        }

        public static void InsertTrivia(this IScriptTerminal node, ScriptTrivia trivia, bool before)
        {
            var trivias = node.Trivias;
            if (trivias is null)
            {
                node.Trivias = trivias = new ScriptTrivias();
            }

            (before ? trivias.Before : trivias.After).Insert(0, trivia);
        }

        public static void AddTrivias<T>(this IScriptTerminal node, T trivias, bool before) where T : IEnumerable<ScriptTrivia>
        {
            foreach (var trivia in trivias)
            {
                node.AddTrivia(trivia, before);
            }
        }

        public static bool HasLeadingSpaceTrivias(this IScriptTerminal node)
        {
            if (node.Trivias is null)
            {
                return false;
            }
            foreach (var trivia in node.Trivias.Before)
            {
                if (trivia.Type.IsSpaceOrNewLine())
                {
                    return true;
                }
                else
                {
                    break;
                }
            }
            return false;
        }

        public static bool HasTrailingSpaceTrivias(this IScriptTerminal node)
        {
            if (node.Trivias is null)
            {
                return false;
            }

            var triviasAfter = node.Trivias.After;
            if (triviasAfter.Count > 0)
            {
                var trivia = triviasAfter[triviasAfter.Count - 1];
                if (trivia.Type.IsSpaceOrNewLine())
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasTrivia(this IScriptTerminal node, ScriptTriviaType triviaType, bool before)
        {
            if (node.Trivias is null)
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

        public static bool HasTriviaEndOfStatement(this IScriptTerminal node, bool before)
        {
            if (node.Trivias is null)
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
    }
}
