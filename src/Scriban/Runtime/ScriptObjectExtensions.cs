// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Scriban.Parsing;
using Scriban.Syntax;

#if NET
using System.Text.Json;
#endif

namespace Scriban.Runtime
{
    /// <summary>
    /// Extensions attached to an <see cref="IScriptObject"/>.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptObjectExtensions
    {
        /// <summary>
        /// Asserts that the specified script object is not readonly or throws a <see cref="ScriptRuntimeException"/>
        /// </summary>
        /// <param name="scriptObject">The script object.</param>
        /// <exception cref="ScriptRuntimeException">If the object is not readonly</exception>
        public static void AssertNotReadOnly(this IScriptObject scriptObject)
        {
            if (scriptObject.IsReadOnly)
            {
                throw new InvalidOperationException("The object is readonly");
            }
        }

        /// <summary>
        /// Imports the specified object into this <see cref="ScriptObject"/> context. See remarks.
        /// </summary>
        /// <param name="script">The script object to import into</param>
        /// <param name="obj">The object.</param>
        /// <param name="filter">Optional member filterer</param>
        /// <param name="renamer">Optional renamer</param>
        /// <remarks>
        /// <ul>
        /// <li>If <paramref name="obj"/> is a <see cref="System.Type"/>, this method will import only the static field/properties of the specified object.</li>
        /// <li>If <paramref name="obj"/> is a <see cref="ScriptObject"/>, this method will import the members of the specified object into the new object.</li>
        /// <li>If <paramref name="obj"/> is a plain object, this method will import the public fields/properties of the specified object into the <see cref="ScriptObject"/>.</li>
        /// </ul>
        /// </remarks>
        public static void Import(this IScriptObject script, object obj, MemberFilterDelegate filter = null, MemberRenamerDelegate renamer = null)
        {
            if (obj is IScriptObject scriptObj)
            {
                // TODO: Add support for filter, member renamer
                script.Import(scriptObj);
                return;
            }

            if (obj is IDictionary dictionary)
            {
                // TODO: Add support for filter, member renamer
                script.ImportDictionary(dictionary);
                return;
            }

#if NET
            if (obj is JsonElement json) {
                script.Import(json);
            }
            else {
                script.Import(obj, ScriptMemberImportFlags.All, filter, renamer);
            }
#else
            script.Import(obj, ScriptMemberImportFlags.All, filter, renamer);
#endif
        }

#if NET
        public static void Import(this IScriptObject script, JsonElement json)
        {
            if (json.ValueKind is JsonValueKind.Object && script is ScriptObject)
            {
                json.CopyToScriptObject(script);
            }
            else if (json.ValueKind is JsonValueKind.Array && script is ScriptArray array)
            {
                json.CopyToScriptArray(array);
            }
            else if (json.ValueKind is JsonValueKind.Null || json.ValueKind is JsonValueKind.Undefined)
            {
                return;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Unsupported object type `{json.ValueKind}`. Expecting Json {(script is ScriptObject ? "Object" : "Array")}.");
            }
        }
#endif

        public static bool TryGetValue(this IScriptObject @this, string key, out object value)
        {
            return @this.TryGetValue(null, new SourceSpan(), key, out value);
        }

        public static void SetValue(this IScriptObject @this, string member, object value, bool readOnly)
        {
            @this.TrySetValue(null, new SourceSpan(), member, value, readOnly);
        }

        /// <summary>
        /// Imports the specified <see cref="ScriptObject"/> into this instance by copying the member values into this object.
        /// </summary>
        /// <param name="this">The script object to import into</param>
        /// <param name="other">The other <see cref="ScriptObject"/>.</param>
        public static void Import(this IScriptObject @this, IScriptObject other)
        {
            if (other == null)
            {
                return;
            }

            var thisScript = @this.GetScriptObject();
            AssertNotReadOnly(thisScript);
            var otherScript = other.GetScriptObject();

            foreach (var keyValue in otherScript.Store)
            {
                var member = keyValue.Key;
                if (!thisScript.CanWrite(member))
                {
                    continue;
                }
                thisScript.Store[keyValue.Key] = keyValue.Value;
            }
        }

        private static void ImportDictionary(this IScriptObject @this, IDictionary dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (DictionaryEntry entry in dictionary)
            {
                var member = entry.Key?.ToString();
                if (!@this.CanWrite(member))
                {
                    continue;
                }
                var thisScript = @this.GetScriptObject();
                AssertNotReadOnly(thisScript);
                thisScript[member] = ConvertValue(entry.Value);
            }
        }

        /// <summary>
        /// Gets the script object attached to the specified instance.
        /// </summary>
        /// <param name="this">The script object proxy.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Expecting ScriptObject or ScriptArray instance</exception>
        public static ScriptObject GetScriptObject(this IScriptObject @this)
        {
            var script = @this as ScriptObject;
            if (script == null)
            {
                var scriptArray = @this as ScriptArray;
                if (scriptArray == null)
                {
                    throw new ArgumentException("Expecting ScriptObject or ScriptArray instance", nameof(@this));
                }
                script = scriptArray.ScriptObject;
            }
            return script;
        }


        /// <summary>
        /// Imports a specific member from the specified object.
        /// </summary>
        /// <param name="script">The script object to import into</param>
        /// <param name="obj">The object.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="exportName">Name of the member name replacement. If null, use the default renamer will be used.</param>
        public static void ImportMember(this IScriptObject script, object obj, string memberName, string exportName = null)
        {
            script.Import(obj, ScriptMemberImportFlags.All, member => member.Name == memberName, exportName != null ? name => exportName: (MemberRenamerDelegate)null);
        }


        /// <summary>
        /// Imports the specified object.
        /// </summary>
        /// <param name="script">The script object to import into</param>
        /// <param name="obj">The object.</param>
        /// <param name="flags">The import flags.</param>
        /// <param name="filter">A filter applied on each member</param>
        /// <param name="renamer">The member renamer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static void Import(this IScriptObject script, object obj, ScriptMemberImportFlags flags, MemberFilterDelegate filter = null, MemberRenamerDelegate renamer = null)
        {
            if (obj == null)
            {
                return;
            }
            if (!ScriptObject.IsImportable(obj))
            {
                throw new ArgumentOutOfRangeException(nameof(obj), $"Unsupported object type `{obj.GetType()}`. Expecting plain class or struct");
            }

            var typeInfo = (obj as Type ?? obj.GetType());
            bool useStatic = false;
            bool useInstance = false;
            if (obj is Type)
            {
                useStatic = true;
                obj = null;
            }
            else
            {
                useInstance = true;
            }

            renamer = renamer ?? StandardMemberRenamer.Default;

            var typeToImports = new Stack<Type>();
            while (typeInfo != null)
            {
                typeToImports.Push(typeInfo);
                if (typeInfo.BaseType == typeof(object))
                {
                    break;
                }

                typeInfo = typeInfo.BaseType;
            }

            var scriptObj = script as ScriptObject;

            while (typeToImports.Count > 0)
            {
                typeInfo = typeToImports.Pop();

                if ((flags & ScriptMemberImportFlags.Field) != 0)
                {
                    foreach (var field in typeInfo.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    {
                        if (!field.IsPublic || field.IsLiteral)
                        {
                            continue;
                        }
                        if (filter != null && !filter(field))
                        {
                            continue;
                        }

                        var keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                        if (keep && ((field.IsStatic && useStatic) || useInstance))
                        {
                            var newFieldName = renamer(field);
                            if (String.IsNullOrEmpty(newFieldName))
                            {
                                newFieldName = field.Name;
                            }

                            // If field is init only or literal, it cannot be set back, so we mark it as read-only
                            if (scriptObj == null)
                            {
                                script.TrySetValue(null, new SourceSpan(), newFieldName, ConvertValue(field.GetValue(obj)), field.IsInitOnly || field.IsLiteral);
                            }
                            else
                            {
                                scriptObj.SetValue(newFieldName, ConvertValue(field.GetValue(obj)), field.IsInitOnly || field.IsLiteral);
                            }
                        }
                    }
                }

                if ((flags & ScriptMemberImportFlags.Property) != 0)
                {
                    foreach (var property in typeInfo.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public| BindingFlags.DeclaredOnly))
                    {
                        // Workaround with .NET Core, extension method is not working (retuning null despite doing property.GetMethod), so we need to inline it here
                        var getMethod = property.GetMethod;
                        if (!property.CanRead || !getMethod.IsPublic)
                        {
                            continue;
                        }

                        if (filter != null && !filter(property))
                        {
                            continue;
                        }

                        var keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                        if (keep && (((getMethod.IsStatic && useStatic) || useInstance)))
                        {
                            var newPropertyName = renamer(property);
                            if (String.IsNullOrEmpty(newPropertyName))
                            {
                                newPropertyName = property.Name;
                            }

                            // Initially, we were setting readonly depending on the presence of a set method, but this is not compatible with liquid implems, so we remove readonly restriction
                            //script.SetValue(null, new SourceSpan(), newPropertyName, property.GetValue(obj), property.GetSetMethod() == null || !property.GetSetMethod().IsPublic);
                            if (scriptObj == null)
                            {
                                script.TrySetValue(null, new SourceSpan(), newPropertyName, ConvertValue(property.GetValue(obj)), false);
                            }
                            else
                            {
                                if (property.GetIndexParameters().Length==0)
                                    scriptObj.SetValue(newPropertyName, ConvertValue(property.GetValue(obj)), false);
                            }
                        }
                    }
                }

                if ((flags & ScriptMemberImportFlags.Method) != 0 && useStatic)
                {
                    foreach (var method in typeInfo.GetMethods(BindingFlags.Static | BindingFlags.Public| BindingFlags.DeclaredOnly))
                    {
                        if (filter != null && !filter(method))
                        {
                            continue;
                        }

                        var keep = method.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                        if (keep && method.IsPublic && method.IsStatic && !method.IsSpecialName)
                        {
                            var newMethodName = renamer(method);
                            if (String.IsNullOrEmpty(newMethodName))
                            {
                                newMethodName = method.Name;
                            }

                            if (scriptObj == null)
                            {
                                script.TrySetValue(null, new SourceSpan(), newMethodName, DynamicCustomFunction.Create(obj, method), true);
                            }
                            else
                            {
                                scriptObj.SetValue(newMethodName, DynamicCustomFunction.Create(obj, method), true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Imports the delegate to the specified member.
        /// </summary>
        /// <param name="script">The script object to import into</param>
        /// <param name="member">The member.</param>
        /// <param name="function">The function delegate.</param>
        /// <exception cref="System.ArgumentNullException">if member or function are null</exception>
        public static void Import(this IScriptObject script, string member, Delegate function)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (function == null) throw new ArgumentNullException(nameof(function));

            script.TrySetValue(null, new SourceSpan(), member, DynamicCustomFunction.Create(function.Target, function.GetMethodInfo()), true);
        }

        /// <summary>
        /// Converts imported object to a scriban value.
        /// Handles and converts all types that need to be converted to work in the scriban runtime.
        /// </summary>
        /// <param name="value">The object to import as scriban value.</param>
        /// <returns>A scriban compatible value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object ConvertValue(object value)
        {
#if NET
            return value switch {
                JsonElement json => json.ToScriban(),
                _ => value,
            };
#else
            return value;
#endif
        }
    }
}