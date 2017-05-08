// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Text;

namespace Scriban.Runtime
{
    public sealed class StandardMemberRenamer : IMemberRenamer
    {
        public static readonly StandardMemberRenamer Default = new StandardMemberRenamer();

        private StandardMemberRenamer()
        {
        }

        public string GetName(string member)
        {
            var builder = new StringBuilder();
            bool previousUpper = false;
            for (int i = 0; i < member.Length; i++)
            {
                var c = member[i];
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