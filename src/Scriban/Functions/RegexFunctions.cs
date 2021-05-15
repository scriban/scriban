// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Text.RegularExpressions;
using Scriban.Runtime;

namespace Scriban.Functions
{
    /// <summary>
    /// Functions exposed through `regex` builtin object.
    ///
    /// >*Note:* If your regular expression contains backslashes (` \ `), you will need to do one of the following:
    /// >- Anywhere you would use a ` \ `, use two.  For example: `"\d+\.\d+"` becomes `"\\d+\\.\\d+"`
    /// >- Use [verbatim strings](language.md#31-strings).  For example: `"\d+\.\d+"` becomes `` `\d+\.\d+` ``
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class RegexFunctions : ScriptObject
    {
        /// <summary>
        /// Escapes a minimal set of characters (`\`, `*`, `+`, `?`, `|`, `{`, `[`, `(`,`)`, `^`, `$`,`.`, `#`, and white space)
        /// by replacing them with their escape codes.
        /// This instructs the regular expression engine to interpret these characters literally rather than as metacharacters.
        /// </summary>
        /// <param name="pattern">The input string that contains the text to convert.</param>
        /// <returns>A string of characters with metacharacters converted to their escaped form.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "(abc.*)" | regex.escape }}
        /// ```
        /// ```html
        /// \(abc\.\*\)
        /// ```
        /// </remarks>
        public static string Escape(string pattern)
        {
            return Regex.Escape(pattern);
        }

        /// <summary>
        /// Searches an input string for a substring that matches a regular expression pattern and returns an array with the match occurences.
        /// </summary>
        /// <param name="context">The template context (to fetch the timeout configuration)</param>
        /// <param name="text">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A string with regex options, that can contain the following option characters (default is `null`):
        /// - `i`: Specifies case-insensitive matching.
        /// - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
        /// - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
        /// - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.
        /// </param>
        /// <returns>An array that contains all the match groups. The first group contains the entire match. The other elements contain regex matched groups `(..)`. An empty array returned means no match.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "this is a text123" | regex.match `(\w+) a ([a-z]+\d+)` }}
        /// ```
        /// ```html
        /// ["is a text123", "is", "text123"]
        /// ```
        /// Notice that the first element returned in the array is the entire regex match, followed by the regex group matches.
        /// </remarks>
        public static ScriptArray Match(TemplateContext context, string text, string pattern, string options = null)
        {
            var match = Regex.Match(text, pattern, GetOptions(options), context.RegexTimeOut);
            var matchObject = new ScriptArray();

            if (match.Success)
            {
                foreach (Group group in match.Groups)
                {
                    matchObject.Add(group.Value);
                }
            }

            // otherwise return an empty array
            return matchObject;
        }

        /// <summary>
        /// Searches an input string for multiple substrings that matches a regular expression pattern and returns an array with the match occurences.
        /// </summary>
        /// <param name="context">The template context (to fetch the timeout configuration)</param>
        /// <param name="text">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A string with regex options, that can contain the following option characters (default is `null`):
        /// - `i`: Specifies case-insensitive matching.
        /// - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
        /// - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
        /// - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.
        /// </param>
        /// <returns>An array of matches that contains all the match groups. The first group contains the entire match. The other elements contain regex matched groups `(..)`. An empty array returned means no match.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "this is a text123" | regex.matches `(\w+)` }}
        /// ```
        /// ```html
        /// [["this", "this"], ["is", "is"], ["a", "a"], ["text123", "text123"]]
        /// ```
        /// Notice that the first element returned in the sub array is the entire regex match, followed by the regex group matches.
        /// </remarks>
        public static ScriptArray Matches(TemplateContext context, string text, string pattern, string options = null)
        {
            var matches = Regex.Matches(text, pattern, GetOptions(options), context.RegexTimeOut);
            var allMatches = new ScriptArray();

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var matchObject = new ScriptArray();
                    for (var i = 0; i < match.Groups.Count; i++)
                    {
                        Group @group = match.Groups[i];
                        matchObject.Add(@group.Value);
                    }

                    allMatches.Add(matchObject);
                }
            }

            // otherwise return an empty array
            return allMatches;
        }


        /// <summary>
        /// In a specified input string, replaces strings that match a regular expression pattern with a specified replacement string.
        /// </summary>
        /// <param name="context">The template context (to fetch the timeout configuration)</param>
        /// <param name="text">The string to search for a match.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="replace">The replacement string.</param>
        /// <param name="options">A string with regex options, that can contain the following option characters (default is `null`):
        /// - `i`: Specifies case-insensitive matching.
        /// - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
        /// - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
        /// - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.
        /// </param>
        /// <returns>A new string that is identical to the input string, except that the replacement string takes the place of each matched string. If pattern is not matched in the current instance, the method returns the current instance unchanged.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "abbbbcccd" | regex.replace "b+c+" "-Yo-" }}
        /// ```
        /// ```html
        /// a-Yo-d
        /// ```
        /// </remarks>
        public static string Replace(TemplateContext context, string text, string pattern, string replace, string options = null)
        {
            return Regex.Replace(text, pattern, replace, GetOptions(options), context.RegexTimeOut);
        }

        /// <summary>
        /// Splits an input string into an array of substrings at the positions defined by a regular expression match.
        /// </summary>
        /// <param name="context">The template context (to fetch the timeout configuration)</param>
        /// <param name="text">The string to split.</param>
        /// <param name="pattern">The regular expression pattern to match.</param>
        /// <param name="options">A string with regex options, that can contain the following option characters (default is `null`):
        /// - `i`: Specifies case-insensitive matching.
        /// - `m`: Multiline mode. Changes the meaning of `^` and `$` so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.
        /// - `s`: Specifies single-line mode. Changes the meaning of the dot `.` so it matches every character (instead of every character except `\n`).
        /// - `x`: Eliminates unescaped white space from the pattern and enables comments marked with `#`.
        /// </param>
        /// <returns>A string array.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "a, b   , c,    d" | regex.split `\s*,\s*` }}
        /// ```
        /// ```html
        /// ["a", "b", "c", "d"]
        /// ```
        /// </remarks>
        public static ScriptArray Split(TemplateContext context, string text, string pattern, string options = null)
        {
            return new ScriptArray(Regex.Split(text, pattern, GetOptions(options), context.RegexTimeOut));
        }

        /// <summary>
        /// Converts any escaped characters in the input string.
        /// </summary>
        /// <param name="pattern">The input string containing the text to convert.</param>
        /// <returns>A string of characters with any escaped characters converted to their unescaped form.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "\\(abc\\.\\*\\)" | regex.unescape }}
        /// ```
        /// ```html
        /// (abc.*)
        /// ```
        /// </remarks>
        public static string Unescape(string pattern)
        {
            return Regex.Unescape(pattern);
        }

        private static RegexOptions GetOptions(string options)
        {
            if (options == null)
            {
                return RegexOptions.None;
            }
            var regexOptions = RegexOptions.None;
            for (int i = 0; i < options.Length; i++)
            {
                switch (options[i])
                {
                    case 'i':
                        regexOptions |= RegexOptions.IgnoreCase;
                        break;
                    case 'm':
                        regexOptions |= RegexOptions.Multiline;
                        break;
                    case 's':
                        regexOptions |= RegexOptions.Singleline;
                        break;
                    case 'x':
                        regexOptions |= RegexOptions.IgnorePatternWhitespace;
                        break;
                }
            }
            return regexOptions;
        }
    }
}