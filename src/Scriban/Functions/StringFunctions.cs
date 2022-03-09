// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// String functions available through the builtin object 'string`.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class StringFunctions : ScriptObject
    {
        [ThreadStatic] private static StringBuilder _tlsBuilder;

        private static StringBuilder GetTempStringBuilder()
        {
            var builder = _tlsBuilder;
            if (builder == null) builder = _tlsBuilder = new StringBuilder(1024);
            return builder;
        }

        private static void ReleaseBuilder(StringBuilder builder) => builder.Length = 0;

        /// <summary>
        /// Escapes a string with escape characters.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The two strings concatenated</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hel\tlo\n\"W\\orld" | string.escape }}
        /// ```
        /// ```html
        /// Hel\tlo\n\"W\\orld
        /// ```
        /// </remarks>
        public static string Escape(string text)
        {
            if (text == null) return text;
            StringBuilder builder = null;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c < 32 || c == '"' || c == '\\')
                {
                    string appendText;
                    switch (c)
                    {
                        case '"':
                            appendText = "\\\"";
                            break;
                        case '\\':
                            appendText = "\\\\";
                            break;
                        case '\a':
                            appendText = "\\a";
                            break;
                        case '\b':
                            appendText = "\\b";
                            break;
                        case '\t':
                            appendText = "\\t";
                            break;
                        case '\r':
                            appendText = "\\r";
                            break;
                        case '\v':
                            appendText = "\\v";
                            break;
                        case '\f':
                            appendText = "\\f";
                            break;
                        case '\n':
                            appendText = "\\n";
                            break;
                        default:
                            appendText = $"\\x{(int)c:x2}";
                            break;
                    }

                    if (builder == null)
                    {
                        builder = new StringBuilder(text.Length + 10);
                        if (i > 0) builder.Append(text, 0, i);
                    }

                    builder.Append(appendText);
                }
                else if (builder != null)
                {
                    // TODO: could be more optimized by adding range
                    builder.Append(c);
                }
            }

            return builder != null ? builder.ToString() : text;
        }

        /// <summary>
        /// Concatenates two strings
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="with">The text to append</param>
        /// <returns>The two strings concatenated</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello" | string.append " World" }}
        /// ```
        /// ```html
        /// Hello World
        /// ```
        /// </remarks>
        public static string Append(string text, string with)
        {
            return (text ?? string.Empty) + (with ?? string.Empty);
        }

        /// <summary>
        /// Converts the first character of the passed string to a upper case character.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The capitalized input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.capitalize }}
        /// ```
        /// ```html
        /// Test
        /// ```
        /// </remarks>
        public static string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text) || char.IsUpper(text[0]))
            {
                return text ?? string.Empty;
            }

            var builder = GetTempStringBuilder();
            builder.Append(char.ToUpper(text[0]));
            if (text.Length > 1)
            {
                builder.Append(text, 1, text.Length - 1);
            }
            var result = builder.ToString();
            ReleaseBuilder(builder);
            return result;
        }

        /// <summary>
        /// Converts the first character of each word in the passed string to a upper case character.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The capitalized input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "This is easy" | string.capitalizewords }}
        /// ```
        /// ```html
        /// This Is Easy
        /// ```
        /// </remarks>
        public static string Capitalizewords(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = GetTempStringBuilder();
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
            var result = builder.ToString();
            ReleaseBuilder(builder);
            return result;
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string contains the specified string `value`.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="value">The string to look for</param>
        /// <returns><c>true</c> if `text` contains the string `value`</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "This is easy" | string.contains "easy" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool Contains(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return false;
            }
            return text.Contains(value);
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string is an empty string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns><c>true</c> if `text` is an empty string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "" | string.empty }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool Empty(string text)
        {
            return string.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string is empty or contains only whitespace characters.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns><c>true</c> if `text` is empty string or contains only whitespace characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "" | string.whitespace }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool Whitespace(string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// Converts the string to lower case.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string lower case</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "TeSt" | string.downcase }}
        /// ```
        /// ```html
        /// test
        /// ```
        /// </remarks>
        public static string Downcase(string text)
        {
            return text?.ToLowerInvariant();
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string ends with the specified string `value`.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="value">The string to look for</param>
        /// <returns><c>true</c> if `text` ends with the specified string `value`</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "This is easy" | string.ends_with "easy" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool EndsWith(string text, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text.EndsWith(value);
        }

        /// <summary>
        /// Returns a url handle from the input string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>A url handle</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ '100% M &amp; Ms!!!' | string.handleize  }}
        /// ```
        /// ```html
        /// 100-m-ms
        /// ```
        /// </remarks>
        public static string Handleize(string text)
        {
            var builder = GetTempStringBuilder();
            char lastChar = (char) 0;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsLetterOrDigit(c))
                {
                    lastChar = c;
                    builder.Append(char.ToLowerInvariant(c));
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
            var result = builder.ToString();
            ReleaseBuilder(builder);
            return result;
        }

        /// <summary>
        /// Return a string literal enclosed with double quotes of the input string.
        /// </summary>
        /// <param name="text">The string to return a literal from.</param>
        /// <returns>The literal of a string.</returns>
        /// <remarks>
        /// If the input string has non printable characters or they need contain a double quote, they will be escaped.
        /// ```scriban-html
        /// {{ 'Hello\n"World"' | string.literal }}
        /// ```
        /// ```html
        /// "Hello\n\"World\""
        /// ```
        /// </remarks>
        public static string Literal(string text)
        {
            return text == null ? null : $"\"{Escape(text)}\"";
        }

        /// <summary>
        /// Removes any whitespace characters on the **left** side of the input string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string without any left whitespace characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ '   too many spaces' | string.lstrip  }}
        /// ```
        /// > Highlight to see the empty spaces to the right of the string
        /// ```html
        /// too many spaces
        /// ```
        /// </remarks>
        public static string LStrip(string text)
        {
            return text?.TrimStart();
        }

        /// <summary>
        /// Outputs the singular or plural version of a string based on the value of a number.
        /// </summary>
        /// <param name="number">The number to check</param>
        /// <param name="singular">The singular string to return if number is == 1</param>
        /// <param name="plural">The plural string to return if number is != 1</param>
        /// <returns>The singular or plural string based on number</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ products.size }} {{products.size | string.pluralize 'product' 'products' }}
        /// ```
        /// ```html
        /// 7 products
        /// ```
        /// </remarks>
        public static string Pluralize(int number, string singular, string plural)
        {
            return number == 1 ? singular : plural;
        }

        /// <summary>
        /// Concatenates two strings by placing the `by` string in from of the `text` string
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="by">The string to prepend to `text`</param>
        /// <returns>The two strings concatenated</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "World" | string.prepend "Hello " }}
        /// ```
        /// ```html
        /// Hello World
        /// ```
        /// </remarks>
        public static string Prepend(string text, string by)
        {
            return (by ?? string.Empty) + (text ?? string.Empty);
        }

        /// <summary>
        /// Removes all occurrences of a substring from a string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="remove">The substring to remove from the `text` string</param>
        /// <returns>The input string with the all occurence of a substring removed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello, world. Goodbye, world." | string.remove "world" }}
        /// ```
        /// ```html
        /// Hello, . Goodbye, .
        /// ```
        /// </remarks>
        public static string Remove(string text, string remove)
        {
            if (string.IsNullOrEmpty(remove) || string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Replace(remove, string.Empty);
        }

        /// <summary>
        /// Removes the first occurrence of a substring from a string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="remove">The first occurence of substring to remove from the `text` string</param>
        /// <returns>The input string with the first occurence of a substring removed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello, world. Goodbye, world." | string.remove_first "world" }}
        /// ```
        /// ```html
        /// Hello, . Goodbye, world.
        /// ```
        /// </remarks>
        public static string RemoveFirst(string text, string remove)
        {
            return ReplaceFirst(text, remove, string.Empty);
        }

        /// <summary>
        /// Removes the last occurrence of a substring from a string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="remove">The last occurence of substring to remove from the `text` string</param>
        /// <returns>The input string with the first occurence of a substring removed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello, world. Goodbye, world." | string.remove_last "world" }}
        /// ```
        /// ```html
        /// Hello, world. Goodbye, .
        /// ```
        /// </remarks>
        public static string RemoveLast(string text, string remove)
        {
            return ReplaceFirst(text, remove, string.Empty, true);
        }

        /// <summary>
        /// Replaces all occurrences of a string with a substring.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="match">The substring to find in the `text` string</param>
        /// <param name="replace">The substring used to replace the string matched by `match` in the input `text`</param>
        /// <returns>The input string replaced</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello, world. Goodbye, world." | string.replace "world" "buddy" }}
        /// ```
        /// ```html
        /// Hello, buddy. Goodbye, buddy.
        /// ```
        /// </remarks>
        public static string Replace(string text, string match, string replace)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            match = match ?? string.Empty;
            replace = replace ?? string.Empty;

            return text.Replace(match, replace);
        }

        /// <summary>
        /// Replaces the first occurrence of a string with a substring.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="match">The substring to find in the `text` string</param>
        /// <param name="replace">The substring used to replace the string matched by `match` in the input `text`</param>
        /// <param name="fromEnd">if true start match from end</param>
        /// <returns>The input string replaced</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello, world. Goodbye, world." | string.replace_first "world" "buddy" }}
        /// ```
        /// ```html
        /// Hello, buddy. Goodbye, world.
        /// ```
        /// </remarks>
        public static string ReplaceFirst(string text, string match, string replace, bool fromEnd = false)
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

            var indexOfMatch = fromEnd
                ? text.LastIndexOf(match, StringComparison.OrdinalIgnoreCase)
                : text.IndexOf(match, StringComparison.OrdinalIgnoreCase);

            if (indexOfMatch < 0)
            {
                return text;
            }

            var builder = GetTempStringBuilder();
            builder.Append(text.Substring(0, indexOfMatch));
            builder.Append(replace);
            builder.Append(text.Substring(indexOfMatch + match.Length));

            var result = builder.ToString();
            ReleaseBuilder(builder);
            return result;
        }

        /// <summary>
        /// Removes any whitespace characters on the **right** side of the input string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string without any left whitespace characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ '   too many spaces           ' | string.rstrip  }}
        /// ```
        /// > Highlight to see the empty spaces to the right of the string
        /// ```html
        ///    too many spaces
        /// ```
        /// </remarks>
        public static string RStrip(string text)
        {
            return text?.TrimEnd();
        }

        /// <summary>
        /// Returns the number of characters from the input string
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The length of the input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.size }}
        /// ```
        /// ```html
        /// 4
        /// ```
        /// </remarks>
        public static int Size(string text)
        {
            return string.IsNullOrEmpty(text) ? 0 : text.Length;
        }

        /// <summary>
        /// The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring.
        /// If no second parameter is given, a substring with the remaining characters will be returned.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="start">The starting index character where the slice should start from the input `text` string</param>
        /// <param name="length">The number of character. Default is 0, meaning that the remaining of the string will be returned.</param>
        /// <returns>The input string sliced</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "hello" | string.slice 0 }}
        /// {{ "hello" | string.slice 1 }}
        /// {{ "hello" | string.slice 1 3 }}
        /// {{ "hello" | string.slice 1 length:3 }}
        /// ```
        /// ```html
        /// hello
        /// ello
        /// ell
        /// ell
        /// ```
        /// </remarks>
        public static string Slice(string text, int start, int? length = null)
        {
            if (string.IsNullOrEmpty(text) || start >= text.Length)
            {
                return string.Empty;
            }

            if (start < 0)
            {
                start = start + text.Length;
            }

            if (!length.HasValue)
            {
                length = text.Length;
            }

            if (start < 0)
            {
                if (start + length <= 0)
                {
                    return string.Empty;
                }
                length = length + start;
                start = 0;
            }

            if (start + length > text.Length)
            {
                length = text.Length - start;
            }

            return text.Substring(start, length.Value);
        }

        /// <summary>
        /// The slice returns a substring, starting at the specified index. An optional second parameter can be passed to specify the length of the substring.
        /// If no second parameter is given, a substring with the first character will be returned.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="start">The starting index character where the slice should start from the input `text` string</param>
        /// <param name="length">The number of character. Default is 1, meaning that only the first character at `start` position will be returned.</param>
        /// <returns>The input string sliced</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "hello" | string.slice1 0 }}
        /// {{ "hello" | string.slice1 1 }}
        /// {{ "hello" | string.slice1 1 3 }}
        /// {{ "hello" | string.slice1 1 length: 3 }}
        /// ```
        /// ```html
        /// h
        /// e
        /// ell
        /// ell
        /// ```
        /// </remarks>
        public static string Slice1(string text, int start, int length = 1)
        {
            if (string.IsNullOrEmpty(text) || start > text.Length || length <= 0)
            {
                return string.Empty;
            }

            if (start < 0)
            {
                start = start + text.Length;
            }

            if (start < 0)
            {
                length = length + start;
                start = 0;
            }

            if (start + length > text.Length)
            {
                length = text.Length - start;
            }

            return text.Substring(start, length);
        }

        /// <summary>
        /// The `split` function takes on a substring as a parameter.
        /// The substring is used as a delimiter to divide a string into an array. You can output different parts of an array using `array` functions.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="match">The string used to split the input `text` string</param>
        /// <returns>An enumeration of the substrings</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ for word in "Hi, how are you today?" | string.split ' ' ~}}
        /// {{ word }}
        /// {{ end ~}}
        /// ```
        /// ```html
        /// Hi,
        /// how
        /// are
        /// you
        /// today?
        /// ```
        /// </remarks>
        public static IEnumerable Split(string text, string match)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ScriptRange(Enumerable.Empty<string>());
            }

            return new ScriptRange(text.Split(new[] {match}, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// Returns a boolean indicating whether the input string starts with the specified string `value`.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="value">The string to look for</param>
        /// <returns><c>true</c> if `text` starts with the specified string `value`</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "This is easy" | string.starts_with "This" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool StartsWith(string text, string value)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text.StartsWith(value);
        }

        /// <summary>
        /// Removes any whitespace characters on the **left** and **right** side of the input string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string without any left and right whitespace characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ '   too many spaces           ' | string.strip  }}
        /// ```
        /// > Highlight to see the empty spaces to the right of the string
        /// ```html
        /// too many spaces
        /// ```
        /// </remarks>
        public static string Strip(string text)
        {
            return text?.Trim();
        }

        /// <summary>
        /// Removes any line breaks/newlines from a string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string without any breaks/newlines characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "This is a string.\r\n With \nanother \rstring" | string.strip_newlines  }}
        /// ```
        /// ```html
        /// This is a string. With another string
        /// ```
        /// </remarks>
        public static string StripNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return Regex.Replace(text, @"\r\n|\r|\n", string.Empty);
        }

        /// <summary>
        /// Converts a string to an integer
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string</param>
        /// <returns>A 32 bit integer or null if conversion failed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "123" | string.to_int + 1 }}
        /// ```
        /// ```html
        /// 124
        /// ```
        /// </remarks>
        public static object ToInt(TemplateContext context, string text)
        {
            return int.TryParse(text, NumberStyles.Integer, context.CurrentCulture, out int result) ? (object) result : null;
        }

        /// <summary>
        /// Converts a string to a long 64 bit integer
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string</param>
        /// <returns>A 64 bit integer or null if conversion failed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "123678912345678" | string.to_long + 1 }}
        /// ```
        /// ```html
        /// 123678912345679
        /// ```
        /// </remarks>
        public static object ToLong(TemplateContext context, string text)
        {
            return long.TryParse(text, NumberStyles.Integer, context.CurrentCulture, out long result) ? (object)result : null;
        }

        /// <summary>
        /// Converts a string to a float
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string</param>
        /// <returns>A 32 bit float or null if conversion failed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "123.4" | string.to_float + 1 }}
        /// ```
        /// ```html
        /// 124.4
        /// ```
        /// </remarks>
        public static object ToFloat(TemplateContext context, string text)
        {
            return float.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, context.CurrentCulture, out float result) ? (object)result : null;
        }

        /// <summary>
        /// Converts a string to a double
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="text">The input string</param>
        /// <returns>A 64 bit float or null if conversion failed</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "123.4" | string.to_double + 1 }}
        /// ```
        /// ```html
        /// 124.4
        /// ```
        /// </remarks>
        public static object ToDouble(TemplateContext context, string text)
        {
            return double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands, context.CurrentCulture, out double result) ? (object)result : null;
        }

        /// <summary>
        /// Truncates a string down to the number of characters passed as the first parameter.
        /// An ellipsis (...) is appended to the truncated string and is included in the character count
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="length">The maximum length of the output string, including the length of the `ellipsis`</param>
        /// <param name="ellipsis">The ellipsis to append to the end of the truncated string</param>
        /// <returns>The truncated input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "The cat came back the very next day" | string.truncate 13 }}
        /// ```
        /// ```html
        /// The cat ca...
        /// ```
        /// </remarks>
        public static string Truncate(string text, int length, string ellipsis = null)
        {
            ellipsis = ellipsis ?? "...";
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            int lMinusTruncate = length - ellipsis.Length;
            if (text.Length > length)
            {
                var builder = GetTempStringBuilder();
                builder.Append(text, 0, lMinusTruncate < 0 ? 0 : lMinusTruncate);
                builder.Append(ellipsis);
                text = builder.ToString();
                ReleaseBuilder(builder);
            }
            return text;
        }

        /// <summary>
        /// Truncates a string down to the number of words passed as the first parameter.
        /// An ellipsis (...) is appended to the truncated string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="count">The number of words to keep from the input `text` string before appending the `ellipsis`</param>
        /// <param name="ellipsis">The ellipsis to append to the end of the truncated string</param>
        /// <returns>The truncated input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "The cat came back the very next day" | string.truncatewords 4 }}
        /// ```
        /// ```html
        /// The cat came back...
        /// ```
        /// </remarks>
        public static string Truncatewords(string text, int count, string ellipsis = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = GetTempStringBuilder();
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
            var result = builder.ToString();
            ReleaseBuilder(builder);
            return result;
        }

        /// <summary>
        /// Converts the string to uppercase
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string upper case</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.upcase }}
        /// ```
        /// ```html
        /// TEST
        /// ```
        /// </remarks>
        public static string Upcase(string text)
        {
            return text?.ToUpperInvariant();
        }

        /// <summary>
        /// Computes the `md5` hash of the input string
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The `md5` hash of the input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.md5 }}
        /// ```
        /// ```html
        /// 098f6bcd4621d373cade4e832627b4f6
        /// ```
        /// </remarks>
        public static string Md5(string text)
        {
            text = text ?? string.Empty;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return Hash(md5, text);
            }
        }

        /// <summary>
        /// Computes the `sha1` hash of the input string
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The `sha1` hash of the input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.sha1 }}
        /// ```
        /// ```html
        /// a94a8fe5ccb19ba61c4c0873d391e987982fbbd3
        /// ```
        /// </remarks>
        public static string Sha1(string text)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                return Hash(sha1, text);
            }
        }

        /// <summary>
        /// Computes the `sha256` hash of the input string
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The `sha256` hash of the input string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.sha256 }}
        /// ```
        /// ```html
        /// 9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08
        /// ```
        /// </remarks>
        public static string Sha256(string text)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return Hash(sha256, text);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-1 hash using a hash message authentication code (HMAC). Pass the secret key for the message as a parameter to the function.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="secretKey">The secret key</param>
        /// <returns>The `SHA-1` hash of the input string using a hash message authentication code (HMAC)</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.hmac_sha1 "secret" }}
        /// ```
        /// ```html
        /// 1aa349585ed7ecbd3b9c486a30067e395ca4b356
        /// ```
        /// </remarks>
        public static string HmacSha1(string text, string secretKey)
        {
            using (var hsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)))
            {
                return Hash(hsha1, text);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash using a hash message authentication code (HMAC). Pass the secret key for the message as a parameter to the function.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="secretKey">The secret key</param>
        /// <returns>The `SHA-256` hash of the input string using a hash message authentication code (HMAC)</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.hmac_sha256 "secret" }}
        /// ```
        /// ```html
        /// 0329a06b62cd16b33eb6792be8c60b158d89a2ee3a876fce9a881ebb488c0914
        /// ```
        /// </remarks>
        public static string HmacSha256(string text, string secretKey)
        {
            using (var hsha256 = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)))
            {
                return Hash(hsha256, text);
            }
        }

        private static string Hash(System.Security.Cryptography.HashAlgorithm algo, string text)
        {
            text = text ?? string.Empty;
            var bytes = Encoding.UTF8.GetBytes(text);
            var hash = algo.ComputeHash(bytes);
            var sb = GetTempStringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                var b = hash[i];
                sb.Append(b.ToString("x2"));
            }
            var result = sb.ToString();
            ReleaseBuilder(sb);
            return result;
        }

        /// <summary>
        /// Pads a string with leading spaces to a specified total length.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="width">The number of characters in the resulting string</param>
        /// <returns>The input string padded</returns>
        /// <remarks>
        /// ```scriban-html
        /// hello{{ "world" | string.pad_left 10 }}
        /// ```
        /// ```html
        /// hello     world
        /// ```
        /// </remarks>
        public static string PadLeft(string text, int width)
        {
            return (text ?? string.Empty).PadLeft(width);
        }

        /// <summary>
        /// Pads a string with trailing spaces to a specified total length.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <param name="width">The number of characters in the resulting string</param>
        /// <returns>The input string padded</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "hello" | string.pad_right 10 }}world
        /// ```
        /// ```html
        /// hello     world
        /// ```
        /// </remarks>
        public static string PadRight(string text, int width)
        {
            return (text ?? string.Empty).PadRight(width);
        }

        /// <summary>
        /// Encodes a string to its Base64 representation.
        /// Its character encoded will be UTF-8.
        /// </summary>
        /// <param name="text">The string to encode</param>
        /// <returns>The encoded string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "hello" | string.base64_encode }}
        /// ```
        /// ```html
        /// aGVsbG8=
        /// ```
        /// </remarks>
        public static string Base64Encode(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? string.Empty));
        }

        /// <summary>
        ///  Decodes a Base64-encoded string to a byte array.
        /// The encoding of the bytes is assumed to be UTF-8.
        /// </summary>
        /// <param name="text">The string to decode</param>
        /// <returns>The decoded string</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "aGVsbG8=" | string.base64_decode }}
        /// ```
        /// ```html
        /// hello
        /// ```
        /// </remarks>
        public static string Base64Decode(string text)
        {
            var decoded = Convert.FromBase64String(text ?? string.Empty);
            return Encoding.UTF8.GetString(decoded);
        }


        /// <summary>
        /// Reports the zero-based index of the first occurrence of the specified string in this instance.
        /// The search starts at a specified character position and examines a specified number of character positions.
        /// </summary>
        /// <param name="text">The string to search</param>
        /// <param name="search">The string to find the index of.</param>
        /// <param name="startIndex">
        /// If provided, the search starting position.
        /// If <see langword="null"/>, search will start at the beginning of <paramref name="text"/>.
        /// </param>
        /// <param name="count">
        /// If provided, the number of character positions to examine.
        /// If <see langword="null"/>, all character positions will be considered.
        /// </param>
        /// <param name="stringComparison">
        /// If provided, the comparison rules for the search.
        /// If <see langword="null"/>, <see cref="StringComparison.CurrentCulture"/>
        /// Allowed values are one of the following:
        ///     'CurrentCulture', 'CurrentCultureIgnoreCase', 'InvariantCulture', 'InvariantCultureIgnoreCase', 'Ordinal', 'OrdinalIgnoreCase'
        /// </param>
        /// <returns>
        /// The zero-based index position of the <paramref name="search"/> parameter from the start of <paramref name="text"/>
        /// if <paramref name="search"/> is found, or -1 if it is not. If value is <see cref="String.Empty"/>,
        /// the return value is <paramref name="startIndex"/> (if <paramref name="startIndex"/> is not provided, the return value would be zero).
        /// </returns>
        public static int IndexOf(string text, string search, int? startIndex = null, int? count = null, string stringComparison = null)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            search = search ?? throw new ArgumentNullException(nameof(search));
            var comparison = GetComparison(stringComparison, StringComparison.CurrentCulture, throwExceptions: true);
            var start = startIndex ?? 0;
            var ct = count ?? text.Length - start;
            return text.IndexOf(search, start, ct, comparison);
        }

        private static StringComparison GetComparison(string stringComparison, StringComparison defaultValue, bool throwExceptions)
            => (Value: stringComparison, Throw: throwExceptions) switch
        {
            (Value: null, Throw: _) => defaultValue,
            (Value: _, Throw: _) when Enum.TryParse<StringComparison>(stringComparison, out var value) => value,
            (Value: _, Throw: true) => throw new ArgumentException($"'{stringComparison}' is not a valid {nameof(stringComparison)}", nameof(stringComparison)),
            (Value: _, Throw: false) => defaultValue
        };
    }
}