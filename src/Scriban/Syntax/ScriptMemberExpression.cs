// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.IO;
using System.Reflection;
using Scriban.Helpers;

namespace Scriban.Syntax
{
    [ScriptSyntax("member expression", "<expression>.<variable_name>")]
    public class ScriptMemberExpression : ScriptVariablePath
    {
        public ScriptExpression Target { get; set; }

        public ScriptVariable Member { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        protected override void WriteImpl(RenderContext context)
        {
            Target?.Write(context);
            context.Write(".");
            Member?.Write(context);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override object GetValue(TemplateContext context)
        {
            var targetObject = GetTargetObject(context);
            var accessor = context.GetMemberAccessor(targetObject);

            var memberName = this.Member.Name;

            object value;
            if (!accessor.TryGetValue(context, Span, targetObject, memberName, out value))
            {
                context.TryGetMember?.Invoke(context, Span, targetObject, memberName, out value);
            }
            return value;
        }

        public override void SetValue(TemplateContext context, object valueToSet)
        {
            var targetObject = GetTargetObject(context);
            var accessor = context.GetMemberAccessor(targetObject);

            var memberName = this.Member.Name;

            if (!accessor.TrySetValue(context, this.Span, targetObject, memberName, valueToSet))
            {
                throw new ScriptRuntimeException(this.Member.Span,
                    $"Cannot set a value for the readonly member: {this}"); // unit test: 132-member-accessor-error3.txt
            }
        }

        private object GetTargetObject(TemplateContext context)
        {
            var targetObject = context.GetValue(Target);

            if (targetObject == null)
            {
                throw new ScriptRuntimeException(this.Span, $"Object [{this.Target}] is null. Cannot access member: {this}"); // unit test: 131-member-accessor-error1.txt
            }

            if (targetObject is string || targetObject.GetType().GetTypeInfo().IsPrimitive)
            {
                throw new ScriptRuntimeException(this.Span, $"Cannot get or set a member on the primitive [{targetObject}/{targetObject.GetType()}] when accessing member: {this}"); // unit test: 132-member-accessor-error2.txt
            }

            return targetObject;
        }

        public override string ToString()
        {
            return $"{Target}.{Member}";
        }
    }
}