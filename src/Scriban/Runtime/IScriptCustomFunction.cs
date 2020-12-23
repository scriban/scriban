// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// Allows to create a custom function object.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptCustomFunction : IScriptFunctionInfo
    {
        /// <summary>
        /// Calls the custom function object.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="callerContext">The script node originating this call</param>
        /// <param name="arguments">The parameters of the call</param>
        /// <param name="blockStatement">The current block statement this call is made</param>
        /// <returns>The result of the call</returns>
        object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);

#if !SCRIBAN_NO_ASYNC
        /// <summary>
        /// Calls the custom function object asynchronously.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="callerContext">The script node originating this call</param>
        /// <param name="arguments">The parameters of the call</param>
        /// <param name="blockStatement">The current block statement this call is made</param>
        /// <returns>The result of the call</returns>
        ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);
#endif
    }


#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptVarParamKind
    {
        None,
        Direct,
        LastParameter
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptFunctionInfo
    {
        int RequiredParameterCount { get; }

        int ParameterCount { get; }

        ScriptVarParamKind VarParamKind { get; }

        Type ReturnType { get; }

        ScriptParameterInfo GetParameterInfo(int index);
    }


#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptFunctionInfoExtensions
    {
        public static bool IsParameterType<T>(this IScriptFunctionInfo functionInfo, int index)
        {
            var paramInfo = functionInfo.GetParameterInfo(index);
            return typeof(T).IsAssignableFrom(paramInfo.ParameterType);
        }
    }


    [DebuggerDisplay("{ParameterType} {Name}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    readonly struct ScriptParameterInfo : IEquatable<ScriptParameterInfo>
    {
        public ScriptParameterInfo(Type parameterType, string name)
        {
            ParameterType = parameterType;
            Name = name;
            HasDefaultValue = false;
            DefaultValue = null;
        }

        public ScriptParameterInfo(Type parameterType, string name, object defaultValue)
        {
            ParameterType = parameterType;
            Name = name;
            HasDefaultValue = true;
            DefaultValue = defaultValue;
        }

        public readonly Type ParameterType;

        public readonly string Name;

        public readonly bool HasDefaultValue;

        public readonly object DefaultValue;

        public bool Equals(ScriptParameterInfo other)
        {
            return ParameterType == other.ParameterType && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is ScriptParameterInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ParameterType.GetHashCode() * 397) ^ (Name?.GetHashCode() ?? 0);
            }
        }

        public static bool operator ==(ScriptParameterInfo left, ScriptParameterInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScriptParameterInfo left, ScriptParameterInfo right)
        {
            return !left.Equals(right);
        }
    }
}