// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;

namespace Scriban.Parsing
{
    /// <summary>
    /// A lightweight token struct to avoid GC allocations.
    /// </summary>
    /// <seealso>
    ///     <cref>System.IEquatable{Scriban.Parsing.Token}</cref>
    /// </seealso>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    struct Token : IEquatable<Token>
    {
        public static readonly Token Eof = new Token(TokenType.Eof, TextPosition.Eof, TextPosition.Eof);

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> struct.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public Token(TokenType type, TextPosition start, TextPosition end)
        {
            Type = type;
            Start = start;
            End = end;
        }

        /// <summary>
        /// The type of token.
        /// </summary>
        public readonly TokenType Type;

        /// <summary>
        /// The start position of this token.
        /// </summary>
        public readonly TextPosition Start;

        /// <summary>
        /// The end position of this token.
        /// </summary>
        public TextPosition End;

        public override string ToString()
        {
            return $"{Type}({Start}:{End})";
        }

        public string GetText(string text)
        {
            if (Type == TokenType.Eof)
            {
                return "<eof>";
            }

            if (Start.Offset < text.Length && End.Offset < text.Length)
            {
                return text.Substring(Start.Offset, End.Offset - Start.Offset + 1);
            }

            return "<error>";
        }

        public bool Match(string textToMatch, string lexerText)
        {
            var length = End.Offset - Start.Offset + 1;
            if (textToMatch.Length != length)
            {
                return false;
            }
            return string.CompareOrdinal(textToMatch, 0, lexerText, Start.Offset, length) == 0;
        }

        public bool Equals(Token other)
        {
            return Type == other.Type && Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Token && Equals((Token) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Token left, Token right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Token left, Token right)
        {
            return !left.Equals(right);
        }
    }
}