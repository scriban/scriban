// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Functions
{
    public class BuiltinFunctions : ScriptObject
    {
        public BuiltinFunctions()
        {
            SetValue("include", new IncludeFunction(), true);
            SetValue("object", new ObjectFunctions(), true);
            SetValue(DateTimeFunctions.DateVariable.Name, new DateTimeFunctions(), true);
            SetValue("timespan", new TimeSpanFunctions(), true);
            SetValue("html", new HtmlFunctions(), true);
            SetValue("array", new ArrayFunctions(), true);
            SetValue("string", new StringFunctions(), true);
            SetValue("math", new MathFunctions(), true);
            SetValue("regex", new RegexFunctions(), true);
        }
    }
}