#nullable disable

using System;
using System.Collections.Generic;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime.Accessors
{
    class PrimitiveAccessor : IObjectAccessor, IListAccessor
    {
        public static readonly PrimitiveAccessor Default = new PrimitiveAccessor();

        private PrimitiveAccessor()
        {
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return 0;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            yield break;
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return false;
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            if (!context.EnableRelaxedMemberAccess)
            {
                throw new ScriptRuntimeException(span, $"Cannot get or set a member on the primitive `{target}/{context.GetTypeName(target)}` when accessing member: {member}"); // unit test: 132-member-accessor-error2.txt
            }

            // If this is relaxed, set the target object to null
            value = null;
            return true;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            throw new ScriptRuntimeException(span, $"Cannot get or set a member on the primitive `{target}/{context.GetTypeName(target)}` when accessing member: {member}"); // unit test: 132-member-accessor-error2.txt
        }

        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
        {
            if (!context.EnableRelaxedMemberAccess)
            {
                throw new ScriptRuntimeException(span, $"Cannot get or set a member on the primitive `{target}/{context.GetTypeName(target)}` when accessing index: {index}");
            }

            // If this is relaxed, set the target object to null
            value = null;
            return true;
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
        {
            throw new ScriptRuntimeException(span, $"Cannot get or set a member on the primitive `{target}/{context.GetTypeName(target)}` when accessing index: {index}");
        }

        public bool HasIndexer => false;

        public Type IndexType => null;

        public int GetLength(TemplateContext context, SourceSpan span, object target)
        {
            throw new ScriptRuntimeException(span, $"Cannot use the {context.GetTypeName(target)} primitive `{target}` as a list.");
        }

        public object GetValue(TemplateContext context, SourceSpan span, object target, int index)
        {
            throw new ScriptRuntimeException(span, $"Cannot index the {context.GetTypeName(target)} primitive `{target}`."); // unit test: 130-indexer-accessor-error4.txt
        }

        public void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value)
        {
            throw new ScriptRuntimeException(span, $"Cannot index the {context.GetTypeName(target)} primitive `{target}`."); // unit test: 130-indexer-accessor-error4.txt
        }
    }
}