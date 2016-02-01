// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Reflection;
using Textamina.Scriban.Runtime;

namespace Textamina.Scriban.Helpers
{
    /// <summary>
    /// Object functions available through the object 'object' in scriban.
    /// </summary>
    public static class ObjectFunctions
    {
        public static string Typeof(object value)
        {
            if (value == null)
            {
                return null;
            }
            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            if (type == typeof (string))
            {
                return "string";
            }

            if (type == typeof (bool))
            {
                return "boolean";
            }

            // We assume that we are only using int/double/long for integers and shortcut to IsPrimitive
            if (typeInfo.IsPrimitive)
            {
                return "number";
            }

            // Test first IList, then IEnumerable
            if (typeof (IList).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return "array";
            }

            if ((!typeof(ScriptObject).GetTypeInfo().IsAssignableFrom(typeInfo) && !typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeInfo)) && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return "iterator";
            }

            return "object";
        }

        /// <summary>
        /// Registers the builtins provided by this class to the specified <see cref="ScriptObject"/>.
        /// </summary>
        /// <param name="builtins">The builtins object.</param>
        /// <exception cref="System.ArgumentNullException">If builtins is null</exception>
        [ScriptMemberIgnore]
        public static void Register(ScriptObject builtins)
        {
            if (builtins == null) throw new ArgumentNullException(nameof(builtins));
            builtins.SetValue("object", ScriptObject.From(typeof(ObjectFunctions)), true);
        }
    }
}