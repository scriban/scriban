// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Scriban.Runtime;
using Scriban.Syntax;
using Enum = System.Enum;

namespace Scriban.Helpers
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ReflectionHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPrimitiveOrDecimal(this Type type)
        {
            return type.IsPrimitive || type == typeof(decimal) || type == typeof(BigInteger);
        }

        public static bool IsNumber(this Type type)
        {
            return (type.IsPrimitive && type != typeof(bool)) || type == typeof(decimal) || type == typeof(BigInteger);
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

            if (type == typeof(bool)) return "bool";
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
            if (type == typeof(BigInteger)) return "bigint";
            if (type.IsEnum) return "enum";
            if (type == typeof(ScriptRange)) return "range";
            if (type == typeof(ScriptArray) || typeof(System.Collections.IList).IsAssignableFrom(type)) return "array";
            if (typeof(IScriptObject).IsAssignableFrom(type)) return "object";
            if (typeof(IScriptCustomFunction).IsAssignableFrom(type)) return "function";

            string name = type.Name;

            var indexOfGenerics = name.IndexOf('`');
            if (indexOfGenerics > 0)
            {
                name = name.Substring(0, indexOfGenerics);

                var builder = new StringBuilder();
                builder.Append(name);
                builder.Append('<');
                var genericArguments = type.GenericTypeArguments;
                for (var i = 0; i < genericArguments.Length; i++)
                {
                    var argType = genericArguments[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(ScriptPrettyName(argType));
                }
                builder.Append('>');
                name = builder.ToString();
            }

            var typeNameAttr = type.GetCustomAttribute<ScriptTypeNameAttribute>();
            if (typeNameAttr != null)
            {
                return typeNameAttr.TypeName;
            }

            // For any Scriban ScriptXxxYyy name, return xxx_yyy
            if (type.Namespace != null && type.Namespace.StartsWith("Scriban."))
            {
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
