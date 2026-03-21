// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptAnonymousFunction : ScriptExpression
    {
        private ScriptFunction? _function;

        public ScriptFunction? Function
        {
            get => _function;
            set => ParentToThisNullable(ref _function, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            return Function;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Function is not null)
            {
                printer.Write(Function);
            }
        }
    }
}
