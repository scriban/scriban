// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    [ScriptSyntax("member expression", "<expression>.<variable_name>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptMemberExpression : ScriptExpression, IScriptVariablePath
    {
        private ScriptExpression _target;
        private ScriptToken _dotToken;
        private ScriptVariable _member;

        public ScriptMemberExpression()
        {
            DotToken = ScriptToken.Dot();
        }

        public ScriptExpression Target
        {
            get => _target;
            set => ParentToThis(ref _target, value);
        }

        public ScriptToken DotToken
        {
            get => _dotToken;
            set => ParentToThis(ref _dotToken, value);
        }

        public ScriptVariable Member
        {
            get => _member;
            set => ParentToThis(ref _member, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Target);
            printer.Write(DotToken);
            printer.Write(Member);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public virtual object GetValue(TemplateContext context)
        {
            var targetObject = GetTargetObject(context, false);
            // In case TemplateContext.EnableRelaxedMemberAccess
            if (targetObject == null)
            {
                if (context.EnableRelaxedTargetAccess || DotToken.TokenType == TokenType.QuestionDot)
                {
                    return null;
                }

                throw new ScriptRuntimeException(this.Member.Span, $"Cannot get the member {this} for a null object.");
            }

            var accessor = context.GetMemberAccessor(targetObject);

            var memberName = this.Member.Name;

            object value;
            if (!accessor.TryGetValue(context, Member.Span, targetObject, memberName, out value))
            {
                var result = context.TryGetMember?.Invoke(context, Member.Span, targetObject, memberName, out value);
                if (!context.EnableRelaxedMemberAccess && (!result.HasValue || !result.Value))
                {
                    throw new ScriptRuntimeException(this.Member.Span, $"Cannot get member with name {memberName}."); // unit test: 132-member-accessor-error2.txt
                }
            }
            return value;
        }

        public virtual void SetValue(TemplateContext context, object valueToSet)
        {
            var targetObject = GetTargetObject(context, true);
            var accessor = context.GetMemberAccessor(targetObject);

            var memberName = this.Member.Name;

            if (!accessor.TrySetValue(context, this.Member.Span, targetObject, memberName, valueToSet))
            {
                throw new ScriptRuntimeException(this.Member.Span, $"Cannot set a value for the readonly member: {this}"); // unit test: 132-member-accessor-error3.txt
            }
        }

        public virtual string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath();
        }

        private object GetTargetObject(TemplateContext context, bool isSet)
        {
            var targetObject = context.GetValue(Target);

            if (targetObject == null)
            {
                if (isSet || (context.EnableRelaxedMemberAccess == false && DotToken.TokenType != TokenType.QuestionDot))
                {
                    throw new ScriptRuntimeException(this.Member.Span, $"Object `{this.Target}` is null. Cannot access member: {this}"); // unit test: 131-member-accessor-error1.txt
                }
            }
            return targetObject;
        }
    }
}