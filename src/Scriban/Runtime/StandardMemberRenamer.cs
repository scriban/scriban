// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Reflection;
using System.Text;

namespace Scriban.Runtime
{
    /// <summary>
    /// The standard rename make a camel/pascalcase name changed by `_` and lowercase. e.g `ThisIsAnExample` becomes `this_is_an_example`.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed class StandardMemberRenamer
    {
        public static readonly MemberRenamerDelegate Default = Rename;

        /// <summary>
        /// Renames a camel/pascalcase member to a lowercase and `_` name. e.g `ThisIsAnExample` becomes `this_is_an_example`.
        /// </summary>
        /// <param name="member">The member to rename</param>
        /// <returns>The member name renamed</returns>
        public static string Rename(MemberInfo member) => Rename(member.Name);

        /// <summary>
        /// Renames a camel/pascalcase method name to a lowercase and `_` name. e.g `ThisIsAnExample` becomes `this_is_an_example`.
        /// </summary>
        /// <param name="name">The method name to rename</param>
        /// <returns>The renamed method name</returns>
        public static string Rename(string name)
        {
            var builder = new StringBuilder();
            var previousUpper = false;
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (char.IsUpper(c))
                {
                    if (i > 0 && !previousUpper)
                    {
                        builder.Append("_");
                    }
                    builder.Append(char.ToLowerInvariant(c));
                    previousUpper = true;
                }
                else
                {
                    builder.Append(c);
                    previousUpper = false;
                }
            }
            return builder.ToString();
        }
    }
}