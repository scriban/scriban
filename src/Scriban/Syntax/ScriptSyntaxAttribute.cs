// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Reflection;

namespace Scriban.Syntax
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class ScriptTypeNameAttribute : Attribute
    {
        public ScriptTypeNameAttribute(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct)]
    public class ScriptSyntaxAttribute : ScriptTypeNameAttribute
    {
        public ScriptSyntaxAttribute(string typeName, string example) : base(typeName)
        {
            Example = example;
        }

        public string Example { get; }

        public static ScriptSyntaxAttribute Get(object obj)
        {
            if (obj == null) return null;
            return Get(obj.GetType());
        }

        public static ScriptSyntaxAttribute Get(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var attribute = type.GetCustomAttribute<ScriptSyntaxAttribute>() ??
                            new ScriptSyntaxAttribute(type.Name, "...");
            return attribute;
        }
    }
}