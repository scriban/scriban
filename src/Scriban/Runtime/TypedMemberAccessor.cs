// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Scriban.Helpers;

namespace Scriban.Runtime
{
    class TypedMemberAccessor : IMemberAccessor
    {
        private readonly Type type;
        private readonly IMemberRenamer renamer;
        private readonly Dictionary<string, MemberInfo> members;

        public TypedMemberAccessor(Type targetType, IMemberRenamer renamer)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            this.type = targetType;
            this.renamer = renamer ?? StandardMemberRenamer.Default;
            members = new Dictionary<string, MemberInfo>();
            PrepareMembers();
        }

        public bool HasMember(object target, string member)
        {
            return members.ContainsKey(member);
        }

        public bool TryGetValue(object target, string member, out object value)
        {
            value = null;
            MemberInfo memberAccessor;
            if (members.TryGetValue(member, out memberAccessor))
            {
                var fieldAccessor = memberAccessor as FieldInfo;
                if (fieldAccessor != null)
                {
                    value = fieldAccessor.GetValue(target);
                    return true;
                }
                else
                {
                    var propertyAccessor = (PropertyInfo) memberAccessor;
                    value = propertyAccessor.GetValue(target);
                    return true;
                }
            }
            return false;
        }

        public bool HasReadonly => false;

        public bool TrySetValue(object target, string member, object value)
        {
            MemberInfo memberAccessor;
            if (members.TryGetValue(member, out memberAccessor))
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
            }
            return true;
        }

        public void SetReadOnly(object target, string member, bool isReadOnly)
        {
        }

        private void PrepareMembers()
        {
            foreach (var field in type.GetTypeInfo().GetDeclaredFields())
            {
                var keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                if (keep && !field.IsStatic && field.IsPublic)
                {
                    var newFieldName = Rename(field.Name);
                    if (string.IsNullOrEmpty(newFieldName))
                    {
                        newFieldName = field.Name;
                    }
                    if (!members.ContainsKey(newFieldName))
                    {
                        members.Add(newFieldName, field);
                    }
                }
            }

            foreach (var property in type.GetTypeInfo().GetDeclaredProperties())
            {
                var keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                if (keep && property.CanRead && !property.GetGetMethod().IsStatic && property.GetGetMethod().IsPublic)
                {
                    var newPropertyName = Rename(property.Name);
                    if (string.IsNullOrEmpty(newPropertyName))
                    {
                        newPropertyName = property.Name;
                    }
                    if (!members.ContainsKey(newPropertyName))
                    {
                        members.Add(newPropertyName, property);
                    }
                }
            }
        }

        private string Rename(string name)
        {
            return renamer.GetName(name);
        }
    }
}