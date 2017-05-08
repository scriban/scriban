// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using Scriban.Helpers;

namespace Scriban.Runtime.Accessors
{
    public class TypedObjectAccessor : IObjectAccessor
    {
        private readonly Type _type;
        private readonly IMemberRenamer _renamer;
        private readonly Dictionary<string, MemberInfo> _members;

        public TypedObjectAccessor(Type targetType, IMemberRenamer renamer)
        {
            _type = targetType ?? throw new ArgumentNullException(nameof(targetType));
            _renamer = renamer ?? StandardMemberRenamer.Default;
            _members = new Dictionary<string, MemberInfo>();
            PrepareMembers();
        }

        public bool HasMember(object target, string member)
        {
            return _members.ContainsKey(member);
        }

        public bool TryGetValue(object target, string member, out object value)
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

        public bool TrySetValue(object target, string member, object value)
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
            }
            return true;
        }

        private void PrepareMembers()
        {
            foreach (var field in _type.GetTypeInfo().GetDeclaredFields())
            {
                var keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                if (keep && !field.IsStatic && field.IsPublic)
                {
                    var newFieldName = Rename(field.Name);
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

            foreach (var property in _type.GetTypeInfo().GetDeclaredProperties())
            {
                var keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                if (keep && property.CanRead && !property.GetGetMethod().IsStatic && property.GetGetMethod().IsPublic)
                {
                    var newPropertyName = Rename(property.Name);
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
        }

        private string Rename(string name)
        {
            return _renamer.GetName(name);
        }
    }
}