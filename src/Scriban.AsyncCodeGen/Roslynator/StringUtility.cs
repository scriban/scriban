// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Roslynator.Text;

namespace Roslynator
{
    internal static class StringUtility
    {
        internal static bool IsNullOrEquals(string s, string value)
        {
            return s == null
                || Equals(s, value);
        }

        internal static bool Equals(string s, string value1, string value2)
        {
            return Equals(s, value1)
                || Equals(s, value2);
        }

        internal static bool Equals(string s, string value)
        {
            return string.Equals(s, value, StringComparison.Ordinal);
        }

        public static string FirstCharToLower(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length > 0)
            {
                return char.ToLower(value[0]) + value.Substring(1);
            }
            else
            {
                return value;
            }
        }

        public static string FirstCharToUpper(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length > 0)
            {
                return char.ToUpper(value[0]) + value.Substring(1);
            }
            else
            {
                return value;
            }
        }

        public static bool IsEmptyOrWhitespace(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }

        public static string GetLeadingWhitespaceExceptNewLine(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];

                if (ch == '\r'
                    || ch == '\n'
                    || !char.IsWhiteSpace(ch))
                {
                    return value.Remove(i);
                }
            }

            return value;
        }

        public static string DoubleBraces(string value)
        {
            return value
                .Replace("{", "{{")
                .Replace("}", "}}");
        }

        public static string EscapeQuote(string value)
        {
            return value.Replace("\"", @"\" + "\"");
        }

        public static string ToCamelCase(string value, bool prefixWithUnderscore = false)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string prefix = (prefixWithUnderscore) ? "_" : "";

            int length = value.Length;

            if (length == 0)
                return prefix;

            int i = 0;

            while (i < length
                && value[i] == '_')
            {
                i++;
            }

            if (i == length)
                return "_";

            StringBuilder sb = StringBuilderCache.GetInstance(length + prefix.Length);

            sb.Append(prefix);

            if (char.IsUpper(value[i]))
            {
                sb.Append(char.ToLower(value[i]));
            }
            else
            {
                sb.Append(value[i]);
            }

            i++;

            sb.Append(value, i, length - i);

            return StringBuilderCache.GetStringAndFree(sb);
        }

        public static bool HasPrefix(string value, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (prefix == null)
                throw new ArgumentNullException(nameof(prefix));

            return prefix.Length > 0
                && value.Length > prefix.Length
                && value.StartsWith(prefix, comparison)
                && IsBoundary(value[prefix.Length - 1], value[prefix.Length]);
        }

        public static bool HasSuffix(string value, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (suffix == null)
                throw new ArgumentNullException(nameof(suffix));

            return suffix.Length > 0
                && value.Length > suffix.Length
                && value.EndsWith(suffix, comparison)
                && IsBoundary(value[value.Length - suffix.Length - 1], value[value.Length - suffix.Length]);
        }

        private static bool IsBoundary(char ch1, char ch2)
        {
            if (IsHyphenOrUnderscore(ch1))
            {
                return !IsHyphenOrUnderscore(ch2);
            }
            else if (char.IsDigit(ch1))
            {
                return IsHyphenOrUnderscore(ch2);
            }
            else if (char.IsLower(ch1))
            {
                return IsHyphenOrUnderscore(ch2) || char.IsUpper(ch2);
            }
            else
            {
                return IsHyphenOrUnderscore(ch2);
            }
        }

        private static bool IsHyphenOrUnderscore(char ch)
        {
            return ch == '-' || ch == '_';
        }

        public static string RemovePrefix(string value, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (HasPrefix(value, prefix, comparison))
            {
                return value.Substring(prefix.Length);
            }
            else
            {
                return value;
            }
        }

        public static bool TryRemovePrefix(string value, string prefix, out string result)
        {
            return TryRemovePrefix(value, prefix, StringComparison.Ordinal, out result);
        }

        public static bool TryRemovePrefix(string value, string prefix, StringComparison comparison, out string result)
        {
            if (HasPrefix(value, prefix, comparison))
            {
                result = value.Substring(prefix.Length);
                return true;
            }
            else
            {
                result = value;
                return false;
            }
        }

        public static string RemoveSuffix(string value, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            if (HasSuffix(value, suffix, comparison))
            {
                return value.Remove(value.Length - suffix.Length);
            }
            else
            {
                return value;
            }
        }

        public static bool TryRemoveSuffix(string value, string suffix, out string result)
        {
            return TryRemoveSuffix(value, suffix, StringComparison.Ordinal, out result);
        }

        public static bool TryRemoveSuffix(string value, string suffix, StringComparison comparison, out string result)
        {
            if (HasSuffix(value, suffix, comparison))
            {
                result = value.Remove(value.Length - suffix.Length);
                return true;
            }
            else
            {
                result = value;
                return false;
            }
        }

        public static bool IsOneOrManyUnderscores(string value)
        {
            if (value == null)
                return false;

            int length = value.Length;

            if (length == 0)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (value[i] != '_')
                    return false;
            }

            return true;
        }

        public static bool IsHexadecimalDigit(char ch)
        {
            return char.IsDigit(ch)
                || (ch >= 'a' && ch <= 'f')
                || (ch >= 'A' && ch <= 'F');
        }

        public static string ReplaceDoubleBracesWithSingleBrace(string s)
        {
            int i = 0;

            if (!FindNextIndex())
                return s;

            var sb = new StringBuilder(s.Length);

            int prevIndex = 0;

            while (true)
            {
                sb.Append(s, prevIndex, i - prevIndex);
                sb.Append(s[i]);
                i++;
                i++;

                prevIndex = i;

                if (!FindNextIndex())
                {
                    sb.Append(s, prevIndex, s.Length - prevIndex);
                    return sb.ToString();
                }
            }

            bool FindNextIndex()
            {
                while (i < s.Length)
                {
                    if (s[i] == '{')
                    {
                        if (i < s.Length - 1
                            && s[i + 1] == '{')
                        {
                            return true;
                        }
                    }
                    else if (s[i] == '}')
                    {
                        if (i < s.Length - 1
                            && s[i + 1] == '}')
                        {
                            return true;
                        }
                    }

                    i++;
                }

                return false;
            }
        }
    }
}
