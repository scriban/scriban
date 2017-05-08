// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("readonly statement", "readonly <variable>")]
    public class ScriptReadOnlyStatement : ScriptStatement
    {
        public ScriptVariable Variable { get; set; }


        public override void Evaluate(TemplateContext context)
        {
            context.SetReadOnly(Variable);
        }
    }
}