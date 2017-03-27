// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using System;
using System.Reflection;
using Scriban.Helpers;

namespace Scriban.Runtime
{
    /// <summary>
    /// Extensions attached to an <see cref="IScriptObject"/>.
    /// </summary>
    public static class ScriptObjectExtensions
    {
        internal static readonly IMemberAccessor Accessor = new ScriptObjectAccessor();

        /// <summary>
        /// Allows to filter a member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        public delegate bool FilterMemberDelegate(string member);

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
        public static void Import(this IScriptObject script, object obj)
        {
            if (obj is IScriptObject)
            {
                script.Import((IScriptObject)obj);
                return;
            }

            script.Import(obj, ScriptMemberImportFlags.All);
        }

        /// <summary>
        /// Tries to set the value and readonly state of the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        /// <returns><c>true</c> if the value could be set; <c>false</c> if a value already exist an is readonly</returns>
        public static bool TrySetValue(this IScriptObject @this, string member, object value, bool readOnly)
        {
            if (@this.IsReadOnly(member))
            {
                return false;
            }
            @this.SetValue(member, value, readOnly);
            return true;
        }

        /// <summary>
        /// Imports the specified <see cref="ScriptObject"/> into this instance by copying the member values into this object.
        /// </summary>
        /// <param name="other">The other <see cref="ScriptObject"/>.</param>
        public static void Import(this IScriptObject @this, IScriptObject other)
        {
            if (other == null)
            {
                return;
            }

            var thisScript = @this.GetScriptObject();
            var otherScript = other.GetScriptObject();

            foreach (var keyValue in otherScript.store)
            {
                var member = keyValue.Key;
                if (thisScript.IsReadOnly(member))
                {
                    continue;
                }
                thisScript.store[keyValue.Key] = keyValue.Value;
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
        /// <param name="obj">The object.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="exportName">Name of the member name replacement. If null, use the default renamer will be used.</param>
        public static void ImportMember(this IScriptObject script, object obj, string memberName, string exportName = null)
        {
            script.Import(obj, ScriptMemberImportFlags.All | ScriptMemberImportFlags.MethodInstance, member => member == memberName, exportName != null ? new DelegateMemberRenamer(name => exportName) : null);
        }

        /// <summary>
        /// Imports the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="flags">The import flags.</param>
        /// <param name="filter">A filter applied on each member</param>
        /// <param name="renamer">The member renamer.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static void Import(this IScriptObject script, object obj, ScriptMemberImportFlags flags, FilterMemberDelegate filter = null, IMemberRenamer renamer = null)
        {
            if (obj == null)
            {
                return;
            }
            if (!ScriptObject.IsImportable(obj))
            {
                throw new ArgumentOutOfRangeException(nameof(obj), $"Unsupported object type [{obj.GetType()}]. Expecting plain class or struct");
            }

            var typeInfo = (obj as Type ?? obj.GetType()).GetTypeInfo();
            bool useStatic = false;
            bool useInstance = false;
            bool useMethodInstance = false;
            if (obj is Type)
            {
                useStatic = true;
                obj = null;
            }
            else
            {
                useInstance = true;
                useMethodInstance = (flags & ScriptMemberImportFlags.MethodInstance) != 0;
            }

            renamer = renamer ?? StandardMemberRenamer.Default;

            if ((flags & ScriptMemberImportFlags.Field) != 0)
            {
                foreach (var field in typeInfo.GetDeclaredFields())
                {
                    if (!field.IsPublic)
                    {
                        continue;
                    }
                    if (filter != null && !filter(field.Name))
                    {
                        continue;
                    }

                    var keep = field.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && ((field.IsStatic && useStatic) || useInstance))
                    {
                        var newFieldName = renamer.GetName(field.Name);
                        if (String.IsNullOrEmpty(newFieldName))
                        {
                            newFieldName = field.Name;
                        }

                        // If field is init only or literal, it cannot be set back so we mark it as read-only
                        script.SetValue(newFieldName, field.GetValue(obj), field.IsInitOnly || field.IsLiteral);
                    }
                }
            }

            if ((flags & ScriptMemberImportFlags.Property) != 0)
            {
                foreach (var property in typeInfo.GetDeclaredProperties())
                {
                    if (!property.CanRead || !property.GetGetMethod().IsPublic)
                    {
                        continue;
                    }

                    if (filter != null && !filter(property.Name))
                    {
                        continue;
                    }

                    var keep = property.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && (((property.GetGetMethod().IsStatic && useStatic) || useInstance)))
                    {
                        var newPropertyName = renamer.GetName(property.Name);
                        if (String.IsNullOrEmpty(newPropertyName))
                        {
                            newPropertyName = property.Name;
                        }

                        script.SetValue(newPropertyName, property.GetValue(obj), property.GetSetMethod() == null || !property.GetSetMethod().IsPublic);
                    }
                }
            }

            if ((flags & ScriptMemberImportFlags.Method) != 0 && (useStatic || useMethodInstance))
            {
                foreach (var method in typeInfo.GetDeclaredMethods())
                {
                    if (filter != null && !filter(method.Name))
                    {
                        continue;
                    }

                    var keep = method.GetCustomAttribute<ScriptMemberIgnoreAttribute>() == null;
                    if (keep && method.IsPublic && ((useMethodInstance && !method.IsStatic) || (useStatic && method.IsStatic)) && !method.IsSpecialName)
                    {
                        var newMethodName = renamer.GetName(method.Name);
                        if (String.IsNullOrEmpty(newMethodName))
                        {
                            newMethodName = method.Name;
                        }

                        script.SetValue(newMethodName, new ObjectFunctionWrapper(obj, method), true);
                    }
                }
            }
        }

        /// <summary>
        /// Imports the delegate to the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="function">The function delegate.</param>
        /// <exception cref="System.ArgumentNullException">if member or function are null</exception>
        public static void Import(this IScriptObject script, string member, Delegate function)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (function == null) throw new ArgumentNullException(nameof(function));

            script.SetValue(member, new ObjectFunctionWrapper(function.Target, function.GetMethodInfo()), true);
        }

        private class ScriptObjectAccessor : IMemberAccessor
        {
            public bool HasMember(object target, string member)
            {
                return ((IScriptObject)target).Contains(member);
            }

            public bool TryGetValue(object target, string member, out object value)
            {
                return ((IScriptObject)target).TryGetValue(member, out value);
            }

            public bool HasReadonly => true;

            public bool TrySetValue(object target, string member, object value)
            {
                return ((IScriptObject)target).TrySetValue(member, value, false);
            }

            public void SetReadOnly(object target, string member, bool isReadOnly)
            {
                ((IScriptObject)target).SetReadOnly(member, isReadOnly);
            }
        }

        private class ObjectFunctionWrapper : IScriptCustomFunction
        {
            private readonly object target;
            private readonly MethodInfo method;
            private readonly ParameterInfo[] parametersInfo;
            private readonly bool hasObjectParams;
            private readonly int lastParamsIndex;

            public ObjectFunctionWrapper(object target, MethodInfo method)
            {
                this.target = target;
                this.method = method;
                parametersInfo = method.GetParameters();
                lastParamsIndex = parametersInfo.Length - 1;
                if (parametersInfo.Length > 0)
                {
                    var lastParam = parametersInfo[lastParamsIndex];

                    if (lastParam.ParameterType == typeof(object[]))
                    {
                        foreach (var param in lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false))
                        {
                            hasObjectParams = true;
                            break;
                        }
                    }
                }
            }

            public object Evaluate(TemplateContext context, ScriptNode callerContext, ScriptArray parameters, ScriptBlockStatement blockStatement)
            {
                // Check parameters
                if ((hasObjectParams && parameters.Count < parametersInfo.Length - 1) || (!hasObjectParams && parameters.Count != parametersInfo.Length))
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed [{parameters.Count}] while expecting [{parametersInfo.Length}] for [{callerContext}]");
                }

                // Convert arguments
                var arguments = new object[parametersInfo.Length];
                object[] paramArguments = null;
                if (hasObjectParams)
                {
                    paramArguments = new object[parameters.Count - lastParamsIndex];
                    arguments[lastParamsIndex] = paramArguments;
                }

                for (int i = 0; i < parameters.Count; i++)
                {
                    var destType = hasObjectParams && i >= lastParamsIndex ? typeof(object) : parametersInfo[i].ParameterType;
                    try
                    {
                        var argValue = ScriptValueConverter.ToObject(callerContext.Span, parameters[i], destType);
                        if (hasObjectParams && i >= lastParamsIndex)
                        {
                            paramArguments[i - lastParamsIndex] = argValue;
                        }
                        else
                        {
                            arguments[i] = argValue;
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Unable to convert parameter #{i} of type [{parameters[i]?.GetType()}] to type [{destType}]", exception);
                    }
                }

                // Call method
                try
                {
                    var result = method.Invoke(target, arguments);
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