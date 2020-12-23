// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Diagnostics;

namespace Scriban.Syntax
{
    /// <summary>
    /// Slice of a string
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    readonly struct ScriptStringSlice : IEquatable<ScriptStringSlice>, IComparable<ScriptStringSlice>, IComparable<string>
    {
        public static readonly ScriptStringSlice Empty = new ScriptStringSlice(string.Empty);

        public ScriptStringSlice(string fullText)
        {
            FullText = fullText;
            Index = 0;
            Length = fullText?.Length ?? 0;
        }

        public ScriptStringSlice(string fullText, int index, int length)
        {
            if (index < 0 || fullText != null && index >= fullText.Length) throw new ArgumentOutOfRangeException(nameof(index));
            if (length < 0 || fullText != null && index + length > fullText.Length) throw new ArgumentOutOfRangeException(nameof(length));
            FullText = fullText;
            Index = index;
            Length = length;
        }

        /// <summary>
        /// The text of this slice.
        /// </summary>
        public readonly string FullText;

        /// <summary>
        /// Index into the text
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Length of the slice
        /// </summary>
        public readonly int Length;

        public char this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
                return FullText[Index + index];
            }
        }

        public string Substring(int index)
        {
            if (index == Length) return "";
            if ((uint)index > (uint)Length) throw new ArgumentOutOfRangeException(nameof(index));
            return FullText?.Substring(Index + index, Length - index);
        }

        public override string ToString()
        {
            return FullText?.Substring(Index, Length);
        }

        public bool Equals(ScriptStringSlice other)
        {
            if (Length != other.Length) return false;
            if (FullText == null && other.FullText == null) return true;
            if (FullText == null || other.FullText == null) return false;
            return string.CompareOrdinal(FullText, Index, other.FullText, other.Index, Length) == 0;
        }

        public override bool Equals(object obj)
        {
            return obj is ScriptStringSlice other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (FullText == null) return 0;

                // TODO: optimize with Span for >= netstandard 2.1
                var hashCode = Length;
                for (int i = Index; i < Length; i++)
                {
                    hashCode = (hashCode * 397) ^ FullText[i];
                }

                return hashCode;
            }
        }

        public static bool operator ==(ScriptStringSlice left, ScriptStringSlice right) => left.Equals(right);

        public static bool operator !=(ScriptStringSlice left, ScriptStringSlice right) => !left.Equals(right);

        public static bool operator ==(ScriptStringSlice left, string right) => left.CompareTo(right) == 0;

        public static bool operator !=(ScriptStringSlice left, string right) => left.CompareTo(right) != 0;

        public static bool operator ==(string left, ScriptStringSlice right) => right.CompareTo(left) == 0;

        public static bool operator !=(string left, ScriptStringSlice right) => right.CompareTo(left) != 0;

        public int CompareTo(ScriptStringSlice other)
        {
            if (FullText == null || other.FullText == null)
            {
                if (object.ReferenceEquals(FullText, other.FullText))
                {
                    return 0;
                }

                return FullText == null ? -1 : 1;
            }

            if (Length == 0 && other.Length == 0) return 0;


            var minLength = Math.Min(Length, other.Length);

            var textComparison = string.CompareOrdinal(FullText, Index, other.FullText, other.Index, minLength);
            return textComparison != 0 ? textComparison < 0 ? -1 : 1 : Length.CompareTo(other.Length);
        }

        public int CompareTo(string other)
        {
            if (FullText == null || other == null)
            {
                if (object.ReferenceEquals(FullText, other))
                {
                    return 0;
                }

                return FullText == null ? -1 : 1;
            }

            if (Length == 0 && other.Length == 0) return 0;


            var minLength = Math.Min(Length, other.Length);

            var textComparison = string.CompareOrdinal(FullText, Index, other, 0, minLength);
            return textComparison != 0 ?  textComparison < 0 ? -1 : 1 : Length.CompareTo(other.Length);
        }

        public static explicit operator ScriptStringSlice(string text) => new ScriptStringSlice(text);

        public static explicit operator string(ScriptStringSlice slice) => slice.ToString();

        public ScriptStringSlice TrimStart()
        {
            var text = FullText;
            for (int i = 0; i < Length; i++)
            {
                var c = text[Index + i];
                if (!char.IsWhiteSpace(c))
                {
                    return new ScriptStringSlice(text, Index + i, Length - i);
                }
            }
            return Empty;
        }

        public ScriptStringSlice TrimEnd()
        {
            var text = FullText;
            for (int i = Length - 1; i >= 0; i--)
            {
                var c = text[Index + i];
                if (!char.IsWhiteSpace(c))
                {
                    return new ScriptStringSlice(text, Index, i + 1);
                }
            }

            return Empty;
        }

        public ScriptStringSlice TrimEndKeepNewLine()
        {
            var text = FullText;
            for (int i = Length - 1; i >= 0; i--)
            {
                var c = text[Index + i];
                if (!char.IsWhiteSpace(c) || c == '\n')
                {
                    return new ScriptStringSlice(text, Index, i + 1);
                }
            }

            return Empty;
        }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptStringSliceExtensions
    {
        public static ScriptStringSlice Slice(this string text, int index)
        {
            return new ScriptStringSlice(text, index, text.Length - index);
        }

        public static ScriptStringSlice Slice(this string text, int index, int length)
        {
            return new ScriptStringSlice(text, index, length);
        }
    }
}