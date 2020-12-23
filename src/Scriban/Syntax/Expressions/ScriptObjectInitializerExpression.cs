// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.IO;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("object initializer expression", "{ member1: <expression>, member2: ... }")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptObjectInitializerExpression : ScriptExpression
    {
        private ScriptToken _openBrace;
        private ScriptList<ScriptObjectMember> _members;
        private ScriptToken _closeBrace;

        public ScriptObjectInitializerExpression()
        {
            OpenBrace = ScriptToken.OpenBrace();
            Members = new ScriptList<ScriptObjectMember>();
            CloseBrace = ScriptToken.CloseBrace();
        }

        public ScriptToken OpenBrace
        {
            get => _openBrace;
            set => ParentToThis(ref _openBrace, value);
        }

        public ScriptList<ScriptObjectMember> Members
        {
            get => _members;
            set => ParentToThis(ref _members, value);
        }

        public ScriptToken CloseBrace
        {
            get => _closeBrace;
            set => ParentToThis(ref _closeBrace, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            var obj = new ScriptObject();
            context.PushGlobalOnly(obj);
            try
            {
                foreach (var member in Members)
                {
                    member.Evaluate(context);
                }
            }
            finally
            {
                context.PopGlobalOnly();
            }
            return obj;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(OpenBrace);
            printer.WriteListWithCommas(Members);
            printer.Write(CloseBrace);
        }
    }
}