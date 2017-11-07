// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Text.RegularExpressions;
using Scriban.Runtime;

namespace Scriban.Functions
{
    /// <summary>
    /// Functions exposed through `regex` builtin object.
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
    public class RegexFunctions : ScriptObject
    {
        public static string Replace(string text, string pattern, string replace)
        {
            return Regex.Replace(text, pattern, replace);
        }

        public static ScriptArray Split(string text, string pattern)
        {
            return new ScriptArray(Regex.Split(text, pattern));
        }

        public static string Escape(string pattern)
        {
            return Regex.Escape(pattern);
        }

        public static string Unescape(string pattern)
        {
            return Regex.Unescape(pattern);
        }

        public static ScriptArray Match(string text, string pattern)
        {
            var match = Regex.Match(text, pattern);
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