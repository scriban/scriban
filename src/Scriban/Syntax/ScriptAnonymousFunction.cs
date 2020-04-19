// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections.Generic;

namespace Scriban.Syntax
{
    public partial class ScriptAnonymousFunction : ScriptExpression
    {
        private ScriptFunction _function;
        private ScriptToken _doToken;
        
        public ScriptAnonymousFunction()
        {
            DoToken = new ScriptToken("do");
        }
        
        public ScriptToken DoToken
        {
            get => _doToken;
            set => ParentToThis(ref _doToken, value);
        }
        
        public ScriptFunction Function
        {
            get => _function;
            set => ParentToThis(ref _function, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return Function;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(DoToken).ExpectSpace();
            context.Write(Function);
        }
    }
}