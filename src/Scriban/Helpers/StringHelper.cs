// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections;
using System.Text;

namespace Scriban.Helpers
{
    internal class StringHelper
    {
        public static string Join(string separator, IEnumerable items)
        {
            var builder = new StringBuilder();
            bool isFirst = true;
            foreach (var item in items)
            {
                if (!isFirst)
                {
                    builder.Append(separator);
                }
                builder.Append(item);
                isFirst = false;
            }
            return builder.ToString();
        }
    }
}