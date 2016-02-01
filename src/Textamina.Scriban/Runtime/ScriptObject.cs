// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Textamina.Scriban.Runtime
{
    /// <summary>
    /// Base runtime object used to store properties.
    /// </summary>
    /// <seealso cref="System.Collections.IEnumerable" />
    public class ScriptObject : IEnumerable
    {
        internal static readonly IMemberAccessor Accessor = new ScriptObjectAccessor();

        private readonly Dictionary<string, InternalValue> store;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptObject"/> class.
        /// </summary>
        public ScriptObject()
        {
            store = new Dictionary<string, InternalValue>();
        }

        /// <summary>
        /// Clears all members stored in this object.
        /// </summary>
        public void Clear()
        {
            store.Clear();
        }

        /// <summary>
        /// Gets the number of members.
        /// </summary>
        public int Count => store.Count;

        /// <summary>
        /// Determines whether this object contains the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if this object contains the specified member; <c>false</c> otherwise</returns>
        /// <exception cref="System.ArgumentNullException">If member is null</exception>
        public bool Contains(string member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            return store.ContainsKey(member);
        }

        /// <summary>
        /// Tries the get the value of the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was retrieved</returns>
        public bool TryGetValue(string member, out object value)
        {
            InternalValue internalValue;
            var result = store.TryGetValue(member, out internalValue);
            value = internalValue.Value;
            return result;
        }

        /// <summary>
        /// Determines whether the specified member is read-only.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if the specified member is read-only</returns>
        public bool IsReadOnly(string member)
        {
            InternalValue internalValue;
            store.TryGetValue(member, out internalValue);
            return internalValue.IsReadOnly;
        }

        /// <summary>
        /// Tries to set the value and readonly state of the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        /// <returns><c>true</c> if the value could be set; <c>false</c> if a value already exist an is readonly</returns>
        public bool TrySetValue(string member, object value, bool readOnly)
        {
            InternalValue internalValue;
            if (store.TryGetValue(member, out internalValue))
            {
                if (internalValue.IsReadOnly)
                {
                    return false;
                }
            }
            store[member] = new InternalValue(value) {IsReadOnly = readOnly};
            return true;
        }

        /// <summary>
        /// Sets the value and readonly state of the specified member. This method overrides previous readonly state.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        public void SetValue(string member, object value, bool readOnly)
        {
            store[member] = new InternalValue(value, readOnly);
        }

        /// <summary>
        /// Removes the specified member from this object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if it was removed</returns>
        public bool Remove(string member)
        {
            return store.Remove(member);
        }

        /// <summary>
        /// Sets to read only the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        public void SetReadOnly(string member, bool readOnly)
        {
            InternalValue internalValue;
            if (store.TryGetValue(member, out internalValue))
            {
            }
            internalValue.IsReadOnly = readOnly;
            store[member] = internalValue;
        }

        /// <summary>
        /// Creates a <see cref="ScriptObject"/> by importing from the specified object. See remarks.
        /// </summary>
        /// <param name="obj">The object or a type.</param>
        /// <returns>A script object</returns>
        /// <remarks>
        /// <ul>
        /// <li>If <paramref name="obj"/> is a <see cref="System.Type"/>, this method will import only the static field/properties of the specified object.</li>
        /// <li>If <paramref name="obj"/> is a <see cref="ScriptObject"/>, this method will import the members of the specified object into the new object.</li>
        /// <li>If <paramref name="obj"/> is a plain object, this method will import the public fields/properties of the specified object into the <see cref="ScriptObject"/>.</li>
        /// </ul>
        /// </remarks>
        public static ScriptObject From(object obj)
        {
            var scriptObject = new ScriptObject();
            scriptObject.Import(obj);
            return scriptObject;
        }

        /// <summary>
        /// Imports the specified object intto this <see cref="ScriptObject"/> context. See remarks.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <remarks>
        /// <ul>
        /// <li>If <paramref name="obj"/> is a <see cref="System.Type"/>, this method will import only the static field/properties of the specified object.</li>
        /// <li>If <paramref name="obj"/> is a <see cref="ScriptObject"/>, this method will import the members of the specified object into the new object.</li>
        /// <li>If <paramref name="obj"/> is a plain object, this method will import the public fields/properties of the specified object into the <see cref="ScriptObject"/>.</li>
        /// </ul>
        /// </remarks>
        public void Import(object obj)
        {
            if (obj is ScriptObject)
            {
                Import((ScriptObject)obj);
                return;
            }

            Import(obj, ScriptMemberImportFlags.All);
        }

        /// <summary>
        /// Imports the specified <see cref="ScriptObject"/> into this instance by copying the member values into this object.
        /// </summary>
        /// <param name="other">The other <see cref="ScritObject"/>.</param>
        public void Import(ScriptObject other)
        {
            if (other == null)
            {
                return;
            }

            foreach (var keyValue in other.store)
            {
                var member = keyValue.Key;
                InternalValue value;
                if (store.TryGetValue(member, out value))
                {
                    if (value.IsReadOnly)
                    {
                        continue;
                    }
                }
                store[keyValue.Key] = keyValue.Value;
            }
        }

        /// <summary>
        /// Determines whether the specified object is importable by the method <see cref="Import(object)"/>
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns><c>true</c> if the object is importable; <c>false</c> otherwise</returns>
        public static bool IsImportable(object obj)
        {
            if (obj == null)
            {
                return true;
            }

            TypeInfo typeInfo = null;
            if (obj is Type)
            {
                typeInfo = ((Type) obj).GetTypeInfo();
            }
            else
            {
                typeInfo = obj.GetType().GetTypeInfo();
            }
            return !(obj is string || typeInfo.IsPrimitive || typeInfo.IsEnum || typeInfo.IsArray);
        }

        /// <summary>
        /// Imports the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="flags">The import flags.</param>
        /// <param name="renamer">The member renamer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void Import(object obj, ScriptMemberImportFlags flags, IMemberRenamer renamer = null)
        {
            if (obj == null)
            {
                return;
            }
            if (!IsImportable(obj))
            {
                throw new ArgumentOutOfRangeException(nameof(obj), $"Unsupported object type [{obj.GetType()}]. Expecting plain class or struct");
            }

            TypeInfo typeInfo = null;
            bool useStatic = false;
            bool useInstance = false;
            if (obj is Type)
            {
                useStatic = true;
                typeInfo = ((Type)obj).GetTypeInfo();
                obj = null;
            }
            else
            {
                useInstance = true;
                typeInfo = obj.GetType().GetTypeInfo();
            }

            renamer = renamer ?? StandardMemberRenamer.Default;

            if ((flags & ScriptMemberImportFlags.Field) != 0)
            {
                foreach (var field in typeInfo.DeclaredFields)
                {
                    if (!field.IsPublic)
                    {
                        continue;
                    }

                    var keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && ((field.IsStatic && useStatic) || useInstance))
                    {
                        var newFieldName = renamer.GetName(field.Name);
                        if (string.IsNullOrEmpty(newFieldName))
                        {
                            newFieldName = field.Name;
                        }

                        // If field is init only or literal, it cannot be set back so we mark it as read-only
                        SetValue(newFieldName, field.GetValue(obj), field.IsInitOnly || field.IsLiteral);
                    }
                }
            }

            if ((flags & ScriptMemberImportFlags.Property) != 0)
            {
                foreach (var property in typeInfo.DeclaredProperties)
                {
                    if (!property.CanRead || !property.GetMethod.IsPublic)
                    {
                        continue;
                    }

                    var keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && (((property.GetMethod.IsStatic && useStatic) || useInstance)))
                    {
                        var newPropertyName = renamer.GetName(property.Name);
                        if (string.IsNullOrEmpty(newPropertyName))
                        {
                            newPropertyName = property.Name;
                        }
                        
                        SetValue(newPropertyName, property.GetValue(obj), property.SetMethod == null || !property.SetMethod.IsPublic);
                    }
                }
            }

            if ((flags & ScriptMemberImportFlags.Method) != 0 && useStatic)
            {
                foreach (var method in typeInfo.DeclaredMethods)
                {
                    var keep = method.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && method.IsPublic && method.IsStatic && !method.IsSpecialName)
                    {
                        var newMethodName = renamer.GetName(method.Name);
                        if (string.IsNullOrEmpty(newMethodName))
                        {
                            newMethodName =  method.Name;
                        }

                        SetValue(newMethodName, new StaticFunctionWrapper(method), true);
                    }
                }
            }
        }

        private class ScriptObjectAccessor : IMemberAccessor
        {
            public bool HasMember(object target, string member)
            {
                return ((ScriptObject) target).store.ContainsKey(member);
            }

            public object GetValue(object target, string member)
            {
                object value;
                ((ScriptObject)target).TryGetValue(member, out value);
                return value;
            }
            public bool HasReadonly => true;

            public bool TrySetValue(object target, string member, object value)
            {
                return ((ScriptObject)target).TrySetValue(member, value, false);
            }

            public void SetReadOnly(object target, string member, bool isReadOnly)
            {
                ((ScriptObject) target).SetReadOnly(member, isReadOnly);
            }
        }

        private struct InternalValue
        {
            public InternalValue(object value, bool isReadOnly)
            {
                Value = value;
                IsReadOnly = isReadOnly;
            }

            public InternalValue(object value) : this()
            {
                Value = value;
            }

            public object Value { get; }

            public bool IsReadOnly { get; set; }
        }

        public IEnumerator GetEnumerator()
        {
            return store.Select(item => new KeyValuePair<string, object>(item.Key, item.Value)).GetEnumerator();
        }

        private class StaticFunctionWrapper : IScriptCustomFunction
        {
            private readonly MethodInfo method;
            private readonly ParameterInfo[] parametersInfo;
            public StaticFunctionWrapper(MethodInfo method)
            {
                this.method = method;
                parametersInfo = method.GetParameters();
            }

            public object Evaluate(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement)
            {
                // Check parameters
                if (parameters.Count != parametersInfo.Length)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed [{parameters.Count}] while expecting [{parametersInfo.Length}] for [{callerContext}]");
                }

                // Convert arguments
                var arguments = new object[parameters.Count];
                for (int i = 0; i < parameters.Count; i++)
                {
                    var destType = parametersInfo[i].ParameterType;
                    try
                    {
                        arguments[i] = ScriptValueConverter.ToObject(callerContext.Span, parameters[i], destType);
                    }
                    catch (Exception exception)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Unable to convert parameter #{i} of type [{parameters[i]?.GetType()}] to type [{destType}]", exception);
                    }
                }

                // Call method
                try
                {
                    var result = method.Invoke(null, arguments);
                    return result;
                }
                catch (Exception exception)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception when calling {callerContext}", exception);
                }
            }
        }
    }
}