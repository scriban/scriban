// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System.Text.RegularExpressions;

namespace Scriban.Runtime
{
    /// <summary>
    /// Functions exposed through `regex` builtin object.
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
    public class RegexFunctions : ScriptObject
    {
        public static string Replace(string pattern, string replace, string input)
        {
            return Regex.Replace(input, pattern, replace);
        }

        public static ScriptArray Split(string pattern, string input)
        {
            return new ScriptArray(Regex.Split(input, pattern));
        }

        public static string Escape(string pattern)
        {
            return Regex.Escape(pattern);
        }

        public static string Unescape(string pattern)
        {
            return Regex.Unescape(pattern);
        }

        public static ScriptArray Match(string pattern, string input)
        {
            var match = Regex.Match(input, pattern);
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
    }
}