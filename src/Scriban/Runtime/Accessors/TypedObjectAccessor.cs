// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class TypedObjectAccessor : IObjectAccessor
    {
        private readonly MemberFilterDelegate _filter;
        private readonly Type _type;
        private readonly MemberRenamerDelegate _renamer;
        private readonly Dictionary<string, MemberInfo> _members;

        public TypedObjectAccessor(Type targetType, MemberFilterDelegate filter, MemberRenamerDelegate renamer)
        {
            _type = targetType ?? throw new ArgumentNullException(nameof(targetType));
            _filter = filter;
            _renamer = renamer ?? StandardMemberRenamer.Default;
            _members = new Dictionary<string, MemberInfo>();
            PrepareMembers();
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target)
        {
            return _members.Count;
        }

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target)
        {
            return _members.Keys;
        }

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member)
        {
            return _members.ContainsKey(member);
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;
            MemberInfo memberAccessor;
            if (_members.TryGetValue(member, out memberAccessor))
            {
                var fieldAccessor = memberAccessor as FieldInfo;
                if (fieldAccessor != null)
                {
                    value = fieldAccessor.GetValue(target);
                    return true;
                }

                var propertyAccessor = (PropertyInfo) memberAccessor;
                value = propertyAccessor.GetValue(target);
                return true;
            }
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value)
        {
            MemberInfo memberAccessor;
            if (_members.TryGetValue(member, out memberAccessor))
            {
                var fieldAccessor = memberAccessor as FieldInfo;
                if (fieldAccessor != null)
                {
                    fieldAccessor.SetValue(target, value);
                }
                else
                {
                    var propertyAccessor = (PropertyInfo)memberAccessor;
                    propertyAccessor.SetValue(target, value);
                }

                return true;
            }
            return false;
        }

        private void PrepareMembers()
        {
            var type = this._type;

            while (type != null)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && !field.IsStatic && field.IsPublic && !field.IsLiteral && (_filter == null || _filter(field)))
                    {
                        var newFieldName = Rename(field);
                        if (string.IsNullOrEmpty(newFieldName))
                        {
                            newFieldName = field.Name;
                        }

                        if (!_members.ContainsKey(newFieldName))
                        {
                            _members.Add(newFieldName, field);
                        }
                    }
                }

                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    var keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;

                    // Workaround with .NET Core, extension method is not working (retuning null despite doing property.GetMethod), so we need to inline it here
                    var getMethod = property.GetMethod;
                    if (keep && property.CanRead && !getMethod.IsStatic && getMethod.IsPublic && (_filter == null || _filter(property)))
                    {
                        var newPropertyName = Rename(property);
                        if (string.IsNullOrEmpty(newPropertyName))
                        {
                            newPropertyName = property.Name;
                        }

                        if (!_members.ContainsKey(newPropertyName))
                        {
                            _members.Add(newPropertyName, property);
                        }
                    }
                }

                if (type.BaseType == typeof(object))
                {
                    break;
                }
                type = type.BaseType;
            }
        }

        private string Rename(MemberInfo member)
        {
            return _renamer(member);
        }
    }
}