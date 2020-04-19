// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("readonly statement", "readonly <variable>")]
    public partial class ScriptReadOnlyStatement : ScriptStatement
    {
        private ScriptVariable _variable;

        public ScriptVariable Variable
        {
            get => _variable;
            set => ParentToThis(ref _variable, value);
        }
        
        public override object Evaluate(TemplateContext context)
        {
            context.SetReadOnly(Variable);
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("readonly").ExpectSpace();
            context.Write(Variable);
            context.ExpectEos();
        }
    }
}