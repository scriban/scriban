// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Functions
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class BuiltinFunctions : ScriptObject
    {
        /// <summary>
        /// This object is readonly, should not be modified by any other objects internally.
        /// </summary>
        internal static readonly ScriptObject Default = new DefaultBuiltins();

        public BuiltinFunctions() : base(10)
        {
            ((ScriptObject)Default.Clone(true)).CopyTo(this);
        }

        /// <summary>
        /// Use an internal object to create all default builtins just once to avoid allocations of delegates/IScriptCustomFunction
        /// </summary>
        private class DefaultBuiltins : ScriptObject
        {
            public DefaultBuiltins() : base(11, false)
            {
                SetValue("array", new ArrayFunctions(), true);
                SetValue("empty", EmptyScriptObject.Default, true);
                SetValue("blank", EmptyScriptObject.Default, true);
                SetValue("include", new IncludeFunction(), true);
                SetValue(DateTimeFunctions.DateVariable.Name, new DateTimeFunctions(), true);
                SetValue("html", new HtmlFunctions(), true);
                SetValue("math", new MathFunctions(), true);
                SetValue("object", new ObjectFunctions(), true);
                SetValue("regex", new RegexFunctions(), true);
                SetValue("string", new StringFunctions(), true);
                SetValue("timespan", new TimeSpanFunctions(), true);
            }
        }
    }
}