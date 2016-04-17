// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using Scriban.Runtime;

namespace Scriban.Helpers
{
    public static class BuiltinFunctions
    {
        /// <summary>
        /// Registers all scriban builtins to the specified <see cref="ScriptObject"/>.
        /// </summary>
        /// <param name="builtins">The builtins object.</param>
        /// <exception cref="System.ArgumentNullException">If builtins is null</exception>
        public static void Register(ScriptObject builtins)
        {
            if (builtins == null) throw new ArgumentNullException(nameof(builtins));

            IncludeFunction.Register(builtins);
            ObjectFunctions.Register(builtins);
            ScriptDate.Register(builtins);
            ScriptTimeSpan.Register(builtins);
            ArrayFunctions.Register(builtins);
            StringFunctions.Register(builtins);
            MathFunctions.Register(builtins);
        }
    }
}