// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Scriban.Runtime
{
    /// <summary>
    /// String functions available through the object 'string' in scriban.
    /// </summary>
    public class StringFunctions : ScriptObject
    {
        public StringFunctions()
        {
            // We need to handle "slice"/"truncate" differently as we have an optional parameters
            this.SetValue("slice", new DelegateCustomFunction(Slice), true);
        }

        public static string Upcase(string text)
        {
            return text?.ToUpperInvariant();
        }

        public static string Downcase(string text)
        {
            return text?.ToLowerInvariant();
        }

        public static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text) || char.IsUpper(text[0]))
            {
                return text ?? string.Empty;
            }

            var builder = new StringBuilder(text);
            builder[0] = char.ToUpper(builder[0]);
            return builder.ToString();
        }

        public static string Capitalizewords(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(text.Length);
            var previousSpace = true;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    previousSpace = true;
                }
                else if (previousSpace && char.IsLetter(c))
                {
                    // TODO: Handle culture
                    c = char.ToUpper(c);
                    previousSpace = false;
                }
                builder.Append(c);
            }
            return builder.ToString();
        }

        public static string Pluralize(string single, string multiple, int number)
        {
            return number == 1 ? single : multiple;
        }

        public static string Remove(string remove, string text)
        {
            if (string.IsNullOrEmpty(remove) || string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Replace(remove, string.Empty);
        }

        public static string RemoveFirst(string remove, string text)
        {
            return ReplaceFirst(remove, string.Empty, text);
        }

        public static string Strip(string text)
        {
            return text?.Trim();
        }

        public static string LStrip(string text)
        {
            return text?.TrimStart();
        }

        public static string RStrip(string text)
        {
            return text?.TrimEnd();
        }

        public static IEnumerable Split(string pattern, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Enumerable.Empty<string>();
            }

            pattern = pattern ?? string.Empty;

            return text.Split(new[] {pattern}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string StripNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return Regex.Replace(text, @"\r?\n", string.Empty);
        }

        public static string Truncate(int length, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            int lMinusTruncate = length - "...".Length;
            return text.Length > length ? text.Substring(0, lMinusTruncate < 0 ? 0 : lMinusTruncate) + "..." : text;
        }

        public static string Truncatewords(int count, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            bool isFirstWord = true;
            foreach (var word in Regex.Split(text, @"\s+"))
            {
                if (count <= 0)
                {
                    break;
                }

                if (!isFirstWord)
                {
                    builder.Append(' ');
                }

                builder.Append(word);

                isFirstWord = false;
                count--;
            }
            builder.Append("...");
            return builder.ToString();
        }

        public static string Replace(string match, string replace, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            match = match ?? string.Empty;
            replace = replace ?? string.Empty;

            return text.Replace(match, replace);
        }

        public static string ReplaceFirst(string match, string replace, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(match))
            {
                return text;
            }
            replace = replace ?? string.Empty;

            var indexOfMatch = text.IndexOf(match);
            if (indexOfMatch < 0)
            {
                return text;
            }

            var builder = new StringBuilder();
            builder.Append(text.Substring(0, indexOfMatch));
            builder.Append(replace);
            builder.Append(text.Substring(indexOfMatch + match.Length));

            return builder.ToString();
        }

        public static bool StartsWith(string start, string text)
        {
            if (string.IsNullOrEmpty(start))
            {
                throw new ArgumentException("Invalid null start string to match against text", nameof(start));
            }
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Invalid null text", nameof(text));
            }

            return text.StartsWith(start);
        }

        [ScriptMemberIgnore]
        public static string Slice(string text, int start, int length = -1)
        {
            if (text == null || start > text.Length)
            {
                return string.Empty;
            }

            if (length < 0)
            {
                length = text.Length - start;
            }

            if (start < 0)
            {
                start = Math.Max(start + text.Length, 0);
            }
            var end = start + length;
            if (end <= start)
            {
                return string.Empty;
            }
            if (end > text.Length)
            {
                length = text.Length - start;
            }

            return text.Substring(start, length);
        }

        public static string Handleize(string text)
        {
            var builder = new StringBuilder();
            char lastChar = (char)0;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsLetterOrDigit(c))
                {
                    lastChar = c;
                    builder.Append(c);
                }
                else if (lastChar != '-')
                {
                    builder.Append('-');
                    lastChar = '-';
                }
            }
            if (builder.Length > 0 && builder[builder.Length - 1] == '-')
            {
                builder.Length--;
            }
            return builder.ToString();
        }

        private static object Slice(TemplateContext context, ScriptNode callerContext, ScriptArray parameters)
        {
            if (parameters.Count < 2 || parameters.Count > 3)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected number of arguments [{parameters.Count}] for slice. Expecting at least 2 parameters <start> <length>? <text>");
            }

            var text = ScriptValueConverter.ToString(callerContext.Span, parameters[parameters.Count - 1]);
            var start = ScriptValueConverter.ToInt(callerContext.Span, parameters[0]);
            var length = -1;
            if (parameters.Count == 3)
            {
                length = ScriptValueConverter.ToInt(callerContext.Span, parameters[1]);
            }

            return Slice(text, start, length);
        }
    }
}