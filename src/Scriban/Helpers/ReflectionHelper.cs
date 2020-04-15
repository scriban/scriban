// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Scriban.Runtime;

namespace Scriban.Helpers
{
    internal static class ReflectionHelper
    {
        public static bool IsPrimitiveOrDecimal(this Type type)
        {
            var result = type.GetTypeInfo().IsPrimitive || type == typeof(decimal);
            result = result || type == typeof(BigInteger);
            return result;
        }

        public static Type GetBaseOrInterface(this Type type, Type lookInterfaceTypeArg)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (lookInterfaceTypeArg == null)
                throw new ArgumentNullException(nameof(lookInterfaceTypeArg));

            var lookInterfaceType = lookInterfaceTypeArg.GetTypeInfo();

            if (lookInterfaceType.IsGenericTypeDefinition)
            {
                if (lookInterfaceType.IsInterface)
                    foreach (var interfaceType in type.GetTypeInfo().ImplementedInterfaces)
                        if (interfaceType.GetTypeInfo().IsGenericType
                            && interfaceType.GetTypeInfo().GetGenericTypeDefinition()  == lookInterfaceTypeArg)
                            return interfaceType;

                for (var t = type; t != null; t = t.GetTypeInfo().BaseType)
                    if (t.GetTypeInfo().IsGenericType && t.GetTypeInfo().GetGenericTypeDefinition() == lookInterfaceTypeArg)
                        return t;
            }
            else
            {
                if (lookInterfaceType.IsAssignableFrom(type.GetTypeInfo()))
                    return lookInterfaceTypeArg;
            }

            return null;
        }

        public static Type[] GetGenericArguments(this TypeInfo type)
        {
            return type.GenericTypeArguments;
        }

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
        
        public static string ScriptFriendlyName(this Type type)
        {
            if (type == null) return "null";

            if (type == typeof(byte)) return "byte";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(short)) return "short";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(int)) return "int";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(long)) return "long";
            if (type == typeof(string)) return "string";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(BigInteger)) return "BigInteger";

            string name = type.Name;

            // For any Scriban ScriptXxxYyy name, return xxx_yyy
            if (type.Namespace != null && type.Namespace.StartsWith("Scriban."))
            {
                name = type.Name;
                if (name.StartsWith("Script"))
                {
                    name = name.Substring("Script".Length);
                }

                return StandardMemberRenamer.Rename(name);
            }

            return name;
        }
    }
}
