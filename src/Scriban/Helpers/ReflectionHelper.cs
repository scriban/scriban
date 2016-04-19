// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Scriban.Helpers
{
    internal static class ReflectionHelper
    {
#if NETPRE45
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type)
        {
            return
                type.GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance |
                               BindingFlags.Static);
        }
        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type)
        {
            return
                type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                   BindingFlags.Static);
        }
        public static object GetValue(this PropertyInfo propInfo, object obj)
        {
            return propInfo.GetValue(obj, null);
        }
        public static void SetValue(this PropertyInfo propInfo, object obj, object value)
        {
            propInfo.SetValue(obj, value, null);
        }
        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type)
        {
            return type.GetMethods(BindingFlags.Public| BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly);
        }
        public static T GetCustomAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            foreach (var attribute in memberInfo.GetCustomAttributes(true))
            {
                var attributeT = attribute as T;
                if (attributeT != null)
                {
                    return attributeT;
                }
            }
            return (T)null;
        }
#else
        public static IEnumerable<FieldInfo> GetDeclaredFields(this TypeInfo type)
        {
            return type.DeclaredFields;
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this TypeInfo type)
        {
            return type.DeclaredProperties;
        }
        public static IEnumerable<MethodInfo> GetDeclaredMethods(this TypeInfo type)
        {
            return type.DeclaredMethods;
        }
#endif
    }
}
