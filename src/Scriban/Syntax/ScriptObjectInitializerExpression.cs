// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("object initializer expression", "{ member1: <expression>, member2: ... }")]
    public partial class ScriptObjectInitializerExpression : ScriptExpression
    {
        private ScriptList<ScriptObjectMember> _members;

        public ScriptObjectInitializerExpression()
        {
            Members = new ScriptList<ScriptObjectMember>();
        }

        public ScriptList<ScriptObjectMember> Members
        {
            get => _members;
            set => ParentToThis(ref _members, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var obj = new ScriptObject();
            context.PushGlobal(obj);
            try
            {
                foreach (var member in Members)
                {
                    member.Evaluate(context);
                }
            }
            finally
            {
                context.PopGlobal();
            }
            return obj;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("{");
            bool isAfterFirst = false;
            foreach(var member in Members)
            {
                if (isAfterFirst)
                {
                    context.Write(",");
                }

                context.Write(member);

                // If the value didn't have any Comma Trivia, we can emit it
                isAfterFirst = !member.Value.HasTrivia(ScriptTriviaType.Comma, false);
            }
            context.Write("}");
        }
        public override string ToString()
        {
            return "{...}";
        }
    }
}