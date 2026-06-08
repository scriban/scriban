// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Text;
using System.Text.RegularExpressions;
using Scriban.Runtime;

namespace Scriban.Functions
{
    /// <summary>
    /// Html functions available through the builtin object 'html'.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class HtmlFunctions : ScriptObject
    {
        // From https://stackoverflow.com/a/17668453/1356325
        private const string RegexMatchHtml = @"<script.*?</script>|<!--.*?-->|<style.*?</style>|<(?:[^>=]|='[^']*'|=""[^""]*""|=[^'""][^\s>]*)*>";

        /// <summary>
        /// Removes any HTML tags from the input string
        /// </summary>
        /// <param name="context">The template context (used for <see cref="TemplateContext.RegexTimeOut"/>)</param>
        /// <param name="text">The input string</param>
        /// <returns>The input string removed with any HTML tags</returns>
        /// <remarks>
        /// Notice that the implementation of this function is using a simple regex, so it can fail escaping correctly or timeout in case of the malformed html.
        /// If you are looking for a secure HTML stripped, you might want to plug your own HTML function by using [AngleSharp](https://github.com/AngleSharp/AngleSharp) to
        /// strip these HTML tags.
        ///
        /// ```scriban-html
        /// {{ "&lt;p&gt;This is a paragraph&lt;/p&gt;" | html.strip }}
        /// ```
        /// ```html
        /// This is a paragraph
        /// ```
        /// </remarks>
        public static string? Strip(TemplateContext context, string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            var stripHtml = new Regex(RegexMatchHtml, RegexOptions.IgnoreCase|RegexOptions.Singleline, context.RegexTimeOut);
            return stripHtml.Replace(text, string.Empty);
        }

        /// <summary>
        /// Escapes a HTML input string (replacing `&amp;` by `&amp;amp;`)
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string removed with any HTML tags</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "&lt;p&gt;This is a paragraph&lt;/p&gt;" | html.escape }}
        /// ```
        /// ```html
        /// &amp;lt;p&amp;gt;This is a paragraph&amp;lt;/p&amp;gt;
        /// ```
        /// </remarks>
        public static string? Escape(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return System.Net.WebUtility.HtmlEncode(text);
        }

        private static readonly Regex _matchNewLine = new Regex(@"\r\n|\r|\n");

        /// <summary>
        /// Inserts an HTML line break (`&lt;br /&gt;` in front of each newline (`\r\n`, `\r`, `\n`) in a string
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string with HTML line breaks</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "Hello\nworld" | html.newline_to_br }}
        /// ```
        /// ```html
        /// Hello&lt;br /&gt;
        /// world
        /// ```
        /// </remarks>
        public static string? NewlineToBr(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return _matchNewLine.Replace(text, "<br />$0");
        }


        /// <summary>
        /// Percent-encodes a string for use as a URL component.
        /// This function encodes URL syntax characters such as `:`, `/`, `?`, `#`, `&amp;`, `=`, and `'`. Spaces are encoded as `%20`.
        /// It does not validate complete URLs and does not make arbitrary input safe for every output context. When writing a value
        /// into an HTML attribute, validate complete URLs separately and HTML-escape the attribute value.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string URL encoded</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "john@liquid.com" | html.url_encode }}
        /// ```
        /// ```html
        /// john%40liquid.com
        /// ```
        /// </remarks>
        public static string? UrlEncode(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// Escapes characters that are not valid in a complete URL while preserving URL syntax characters.
        /// This function is intended for already-formed, trusted or validated URLs and paths. It preserves reserved URL syntax
        /// characters such as `:`, `/`, `?`, `#`, `&amp;`, `=`, and `'`. It does not validate schemes or hosts and must not be used
        /// as a sanitizer for untrusted `href` or `src` values. Use `html.url_encode` for untrusted URL components or query values.
        /// </summary>
        /// <param name="text">The input string</param>
        /// <returns>The input string URL escaped</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "&lt;hello&gt; &amp; &lt;scriban&gt;" | html.url_escape }}
        /// ```
        /// ```html
        /// %3Chello%3E%20&amp;%20%3Cscriban%3E
        /// ```
        /// </remarks>
        public static string? UrlEscape(string? text)
        {
            if (text is null || text.Length == 0)
            {
                return text;
            }

            StringBuilder? builder = null;
            var unescapedStart = 0;
            var escapeStart = -1;

            for (var i = 0; i < text.Length; i++)
            {
                if (IsAllowedInEscapedUrl(text[i]))
                {
                    if (escapeStart >= 0)
                    {
                        builder ??= new StringBuilder(text.Length);
                        builder.Append(text, unescapedStart, escapeStart - unescapedStart);
                        builder.Append(Uri.EscapeDataString(text.Substring(escapeStart, i - escapeStart)));
                        unescapedStart = i;
                        escapeStart = -1;
                    }
                }
                else if (escapeStart < 0)
                {
                    escapeStart = i;
                }
            }

            if (escapeStart >= 0)
            {
                builder ??= new StringBuilder(text.Length);
                builder.Append(text, unescapedStart, escapeStart - unescapedStart);
                builder.Append(Uri.EscapeDataString(text.Substring(escapeStart)));
            }
            else if (builder is not null)
            {
                builder.Append(text, unescapedStart, text.Length - unescapedStart);
            }

            return builder?.ToString() ?? text;
        }

        private static bool IsAllowedInEscapedUrl(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
            {
                return true;
            }

            return c is '-' or '.' or '_' or '~'
                // Reserved URL syntax characters that must be preserved when escaping a complete URL.
                or ':' or '/' or '?' or '#' or '[' or ']' or '@'
                or '!' or '$' or '&' or '\'' or '(' or ')' or '*'
                or '+' or ',' or ';' or '=';
        }
    }
}
