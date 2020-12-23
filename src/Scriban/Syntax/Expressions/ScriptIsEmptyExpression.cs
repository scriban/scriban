// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using Scriban.Runtime;
using System.Collections.Generic;

namespace Scriban.Syntax
{
    [ScriptSyntax("empty expression", "<expression>.empty?")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptIsEmptyExpression: ScriptMemberExpression, IScriptVariablePath
    {
        private ScriptToken _questionToken;

        public ScriptIsEmptyExpression()
        {
            QuestionToken = ScriptToken.Question();
        }

        public ScriptToken QuestionToken
        {
            get => _questionToken;
            set => ParentToThis(ref _questionToken, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            base.PrintTo(printer);
            printer.Write(QuestionToken);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override object GetValue(TemplateContext context)
        {
            var targetObject = GetTargetObject(context, false);
            return context.IsEmpty(Span, targetObject);
        }

        public override void SetValue(TemplateContext context, object valueToSet)
        {
            throw new ScriptRuntimeException(Span, $"The `.empty?` property cannot be set");
        }

        public override string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath();
        }

        private object GetTargetObject(TemplateContext context, bool isSet)
        {
            var targetObject = context.GetValue(Target);

            if (targetObject == null)
            {
                if (isSet || !context.EnableRelaxedMemberAccess)
                {
                    throw new ScriptRuntimeException(this.Span, $"Object `{this.Target}` is null. Cannot access property `empty?`");
                }
            }
            return targetObject;
        }
    }
}