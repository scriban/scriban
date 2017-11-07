// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// String functions available through the object 'string' in scriban.
    /// </summary>
    public class StringFunctions : ScriptObject
    {
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
        /// 
        /// Will output:
        /// 
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
        /// 
        /// Will output:
        /// 
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

            var builder = new StringBuilder(text);
            builder[0] = char.ToUpper(builder[0]);
            return builder.ToString();
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
        /// 
        /// Will output:
        /// 
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
        /// 
        /// Will output:
        /// 
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
        /// Converts the string to lower case.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string lower case</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "TeSt" | string.downcase }}
        /// ```
        /// 
        /// Will output:
        /// 
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
        /// 
        /// Will output:
        /// 
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
        /// {{ '100% M & Ms!!!' | string.handleize  }}
        /// ```
        /// 
        /// Will output:
        /// 
        /// ```html
        /// 100-m-ms
        /// ```
        /// </remarks>
        public static string Handleize(string text)
        {
            var builder = new StringBuilder();
            char lastChar = (char) 0;
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

        /// <summary>
        /// Removes any whitespace characters on the **left** side of the input string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string without any left whitespace characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ '   too many spaces           ' | string.lstrip  }}
        /// ```
        /// 
        /// Will output:
        /// 
        /// ```html
        /// &lt;!-- Highlight to see the empty spaces to the right of the string --&gt;
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
        /// {{ cart.item_count }} {{cart.item_count | string.pluralize 'item' 'items' }}
        /// ```
        /// 
        /// Will output:
        /// 
        /// ```html
        /// 4 items
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
        /// 
        /// Will output:
        /// 
        /// ```html
        /// Hello World
        /// ```
        /// </remarks>
        public static string Prepend(string text, string by)
        {
            return (by ?? string.Empty) + (text ?? string.Empty);
        }

        public static string Remove(string text, string remove)
        {
            if (string.IsNullOrEmpty(remove) || string.IsNullOrEmpty(text))
            {
                return text;
            }
            return text.Replace(remove, string.Empty);
        }

        public static string RemoveFirst(string text, string remove)
        {
            return ReplaceFirst(text, remove, string.Empty);
        }

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

        public static string ReplaceFirst(string text, string match, string replace)
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

            var indexOfMatch = text.IndexOf(match, StringComparison.OrdinalIgnoreCase);
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

        /// <summary>
        /// Removes any whitespace characters on the **right** side of the input string.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string without any left whitespace characters</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ '   too many spaces           ' | string.lstrip  }}
        /// ```
        /// 
        /// Will output:
        /// 
        /// ```html
        /// &lt;!-- Highlight to see the empty spaces to the right of the string --&gt;
        /// too many spaces           
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
        /// 
        /// Will output:
        /// 
        /// ```
        /// 4
        /// ```
        /// </remarks>
        public static int Size(string text)
        {
            return string.IsNullOrEmpty(text) ? 0 : text.Length;
        }

        public static string Slice(string text, int start, int length = -1)
        {
            if (string.IsNullOrEmpty(text) || start >= text.Length)
            {
                return string.Empty;
            }

            if (start < 0)
            {
                start = start + text.Length;
            }

            if (length < 0)
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

            return text.Substring(start, length);
        }

        // On Liquid: Slice will return 1 character by default, unlike in scriban that returns the rest of the string
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

        public static IEnumerable Split(string text, string match)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Enumerable.Empty<string>();
            }

            match = match ?? string.Empty;

            return text.Split(new[] {match}, StringSplitOptions.RemoveEmptyEntries);
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
        /// 
        /// Will output:
        /// 
        /// ```
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

        public static string Strip(string text)
        {
            return text?.Trim();
        }

        public static string StripNewlines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return Regex.Replace(text, @"\r?\n", string.Empty);
        }

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
                var builder = new StringBuilder(length);
                builder.Append(text, 0, lMinusTruncate < 0 ? 0 : lMinusTruncate);
                builder.Append(ellipsis);
                return builder.ToString();
            }
            return text;
        }

        public static string Truncatewords(string text, int count)
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

        /// <summary>
        /// Converts the string to uppercase
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string upper case</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "test" | string.upcase }}
        /// ```
        /// 
        /// Will output:
        /// 
        /// ```
        /// TEST
        /// ```
        /// </remarks>
        public static string Upcase(string text)
        {
            return text?.ToUpperInvariant();
        }

#if !PCL328 && !NETSTD11
        public static string Md5(string text)
        {
            text = text ?? string.Empty;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return Hash(md5, text);
            }
        }

        public static string Sha1(string text)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                return Hash(sha1, text);
            }
        }

        public static string Sha256(string text)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return Hash(sha256, text);
            }
        }

        public static string HmacSha1(string text, string secretKey)
        {
            using (var hsha1 = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(secretKey ?? string.Empty)))
            {
                return Hash(hsha1, text);
            }
        }

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
            var sb = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; i++)
            {
                var b = hash[i];
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
#endif
    }
}