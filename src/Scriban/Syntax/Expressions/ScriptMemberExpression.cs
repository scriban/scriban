// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptExpression? _target;
        private ScriptToken _dotToken = ScriptToken.Dot();
        private ScriptVariable? _member;
        public ScriptMemberExpression()
        {
            _dotToken.Parent = this;
        }

        public ScriptExpression? Target
        {
            get => _target;
            set => ParentToThisNullable(ref _target, value);
        }

        public ScriptToken DotToken
        {
            get => _dotToken;
            set => ParentToThis(ref _dotToken, value);
        }

        public ScriptVariable? Member
        {
            get => _member;
            set => ParentToThisNullable(ref _member, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Target is not null)
            {
                printer.Write(Target);
            }
            printer.Write(DotToken);
            if (Member is not null)
            {
                printer.Write(Member);
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public virtual object? GetValue(TemplateContext context)
        {
            var member = Member;
            if (member is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid member expression. Member is required.");
            }

            var targetObject = GetTargetObject(context, false);
            // In case TemplateContext.EnableRelaxedMemberAccess
            if (targetObject is null)
            {
                if (context.EnableRelaxedTargetAccess || DotToken.TokenType == TokenType.QuestionDot)
                {
                    return null;
                }

                throw new ScriptRuntimeException(member.Span, $"Cannot get the member {this} for a null object.");
            }

            var accessor = context.GetMemberAccessor(targetObject);

            var memberName = member.Name;

            object? value;
            if (!accessor.TryGetValue(context, member.Span, targetObject, memberName, out value))
            {
                var result = context.TryGetMember?.Invoke(context, member.Span, targetObject, memberName, out value);
                if (!context.EnableRelaxedMemberAccess && (!result.HasValue || !result.Value))
                {
                    throw new ScriptRuntimeException(member.Span, $"Cannot get member with name {memberName}."); // unit test: 132-member-accessor-error2.txt
                }
            }
            return value;
        }

        public virtual void SetValue(TemplateContext context, object? valueToSet)
        {
            var member = Member;
            if (member is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid member expression. Member is required.");
            }

            var targetObject = GetTargetObject(context, true);
            if (targetObject is null)
            {
                throw new ScriptRuntimeException(member.Span, $"Object `{Target}` is null. Cannot access member: {this}");
            }
            var accessor = context.GetMemberAccessor(targetObject);

            var memberName = member.Name;

            if (!accessor.TrySetValue(context, member.Span, targetObject, memberName, valueToSet))
            {
                throw new ScriptRuntimeException(member.Span, $"Cannot set a value for the readonly member: {this}"); // unit test: 132-member-accessor-error3.txt
            }
        }

        public virtual string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath() ?? string.Empty;
        }

        private object? GetTargetObject(TemplateContext context, bool isSet)
        {
            var member = Member;
            var target = Target;
            if (member is null || target is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid member expression. Target and member are required.");
            }

            var targetObject = context.GetValue(target);

            if (targetObject is null)
            {
                if (isSet || (context.EnableRelaxedMemberAccess == false && DotToken.TokenType != TokenType.QuestionDot))
                {
                    throw new ScriptRuntimeException(member.Span, $"Object `{target}` is null. Cannot access member: {this}"); // unit test: 131-member-accessor-error1.txt
                }
            }
            return targetObject;
        }
    }
}
