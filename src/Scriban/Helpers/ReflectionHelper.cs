// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Scriban.Runtime;

namespace Scriban.Helpers
{
    public static class ReflectionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrimitiveOrDecimal(this Type type)
        {
            return type.IsPrimitive || type == typeof(decimal) || type == typeof(BigInteger);
        }

        internal static Type GetBaseOrInterface(this Type type, Type lookInterfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (lookInterfaceType == null)
                throw new ArgumentNullException(nameof(lookInterfaceType));

            if (lookInterfaceType.IsGenericTypeDefinition)
            {
                if (lookInterfaceType.IsInterface)
                    foreach (var interfaceType in type.GetInterfaces())
                        if (interfaceType.IsGenericType
                            && interfaceType.GetGenericTypeDefinition()  == lookInterfaceType)
                            return interfaceType;

                for (var t = type; t != null; t = t.BaseType)
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == lookInterfaceType)
                        return t;
            }
            else
            {
                if (lookInterfaceType.IsAssignableFrom(type))
                    return lookInterfaceType;
            }

            return null;
        }

        public static string ScriptPrettyName(this Type type)
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
            if (type == typeof(ScriptArray)) return "[]";

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
