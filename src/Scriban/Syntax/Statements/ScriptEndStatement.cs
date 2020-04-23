// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    [ScriptSyntax("end statement", "end")]
    public partial class ScriptEndStatement : ScriptStatement
    {
        private ScriptKeyword _endKeyword;

        public ScriptEndStatement()
        {
            EndKeyword = ScriptKeyword.End();
        }

        public ScriptKeyword EndKeyword
        {
            get => _endKeyword;
            set => ParentToThis(ref _endKeyword, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return null;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(EndKeyword).ExpectEos();
        }
    }
}