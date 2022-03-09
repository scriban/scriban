// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;


namespace Scriban.Runtime
{
    /// <summary>
    /// Creates a reflection based <see cref="IScriptCustomFunction"/> from a <see cref="MethodInfo"/>.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract partial class DynamicCustomFunction : IScriptCustomFunction
    {
        private static readonly Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>> BuiltinFunctionDelegates = new Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>>(MethodComparer.Default);

        /// <summary>
        /// Gets the reflection method associated to this dynamic call.
        /// </summary>
        public readonly MethodInfo Method;

        protected readonly ParameterInfo[] Parameters;
        private readonly Type _returnType;
        private readonly ScriptParameterInfo[] _parameterInfos;

#if !SCRIBAN_NO_ASYNC
        protected readonly bool IsAwaitable;
#endif
        protected readonly ScriptVarParamKind _varParamKind;
        protected readonly int _paramsIndex;
        protected readonly bool _hasTemplateContext;
        protected readonly bool _hasSpan;
        protected readonly int _optionalParameterCount;
        protected readonly Type _paramsElementType;
        protected readonly int _expectedNumberOfParameters;
        protected readonly int _minimumRequiredParameters;
        protected readonly int _firstIndexOfUserParameters;

        protected DynamicCustomFunction(MethodInfo method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            _returnType = method.ReturnType;

            Parameters = method.GetParameters();
#if !SCRIBAN_NO_ASYNC
            IsAwaitable = method.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
#endif

            _paramsIndex = -1;
            if (Parameters.Length > 0)
            {
                // Check if we have TemplateContext+SourceSpan as first parameters
                if (typeof(TemplateContext).IsAssignableFrom(Parameters[0].ParameterType))
                {
                    _hasTemplateContext = true;
                    if (Parameters.Length > 1)
                    {
                        _hasSpan = typeof(SourceSpan).IsAssignableFrom(Parameters[1].ParameterType);
                    }
                }

                var lastParam = Parameters[Parameters.Length - 1];
                if (lastParam.ParameterType.IsArray)
                {
                    if (lastParam.ParameterType == typeof(object[]) || lastParam.ParameterType == typeof(ScriptExpression[]) || lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                    {
                        _varParamKind = ScriptVarParamKind.LastParameter;
                        _paramsElementType = lastParam.ParameterType.GetElementType();
                        _paramsIndex = Parameters.Length - 1;
                    }
                }
            }

            _expectedNumberOfParameters = Parameters.Length;
            _firstIndexOfUserParameters = 0;

            if (_varParamKind == ScriptVarParamKind.None)
            {
                for (int i = 0; i < Parameters.Length; i++)
                {
                    if (Parameters[i].IsOptional)
                    {
                        _optionalParameterCount++;
                    }
                }
            }

            if (_hasTemplateContext)
            {
                _firstIndexOfUserParameters++;
                if (_hasSpan)
                {
                    _firstIndexOfUserParameters++;
                }
            }

            _expectedNumberOfParameters -= _firstIndexOfUserParameters;
            _minimumRequiredParameters = _expectedNumberOfParameters - _optionalParameterCount;
            if (_varParamKind == ScriptVarParamKind.LastParameter)
            {
                _minimumRequiredParameters--;
            }

            // Compute parameters
            _parameterInfos = new ScriptParameterInfo[_expectedNumberOfParameters];
            for (int i = 0; i < _expectedNumberOfParameters; i++)
            {
                var realIndex = _firstIndexOfUserParameters + i;
                var parameterInfo = Parameters[realIndex];
                var parameterType = realIndex == Parameters.Length - 1 && _varParamKind == ScriptVarParamKind.LastParameter ? _paramsElementType : parameterInfo.ParameterType;
                _parameterInfos[i] = parameterInfo.HasDefaultValue
                    ? new ScriptParameterInfo(parameterType, parameterInfo.Name, parameterInfo.DefaultValue)
                    : new ScriptParameterInfo(parameterType, parameterInfo.Name);
            }
        }

#if !SCRIBAN_NO_ASYNC
        protected async ValueTask<object> ConfigureAwait(object result)
        {
            switch (result)
            {
                case Task<object> taskObj:
                    return await taskObj.ConfigureAwait(false);
                case Task<string> taskStr:
                    return await taskStr.ConfigureAwait(false);
            }
            return await (dynamic)result;
        }
#endif

        protected ArgumentValue GetValueFromNamedArgument(TemplateContext context, ScriptNode callerContext, ScriptNamedArgument namedArg)
        {
            for (int j = 0; j < Parameters.Length; j++)
            {
                var arg = Parameters[j];
                if (arg.Name == namedArg.Name.Name)
                {
                    return new ArgumentValue(j, arg.ParameterType, context.Evaluate(namedArg));
                }
            }
            throw new ScriptRuntimeException(callerContext.Span, $"Invalid argument `{namedArg.Name}` not found for function `{callerContext}`");
        }

        public abstract object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);

        public int RequiredParameterCount => _minimumRequiredParameters;

        public int ParameterCount => _expectedNumberOfParameters;

        public ScriptVarParamKind VarParamKind => _varParamKind;

        public Type ReturnType => _returnType;

        /// <summary>
        /// Get or set an object tag for this instance.
        /// </summary>
        public object Tag { get; set; }

        public ScriptParameterInfo GetParameterInfo(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Argument index must be >= 0");
            if (index >= _parameterInfos.Length)
            {
                if (_varParamKind == ScriptVarParamKind.LastParameter)
                {
                    index = _parameterInfos.Length - 1;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"Argument index must be < {ParameterCount}");
                }
            }
            return _parameterInfos[index];
        }

#if !SCRIBAN_NO_ASYNC
        public virtual ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(Invoke(context, callerContext, arguments, blockStatement));
        }
#endif

        /// <summary>
        /// Returns a <see cref="DynamicCustomFunction"/> from the specified object target and <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="target">A target object - might be null</param>
        /// <param name="method">A MethodInfo</param>
        /// <returns>A custom <see cref="DynamicCustomFunction"/></returns>
        public static DynamicCustomFunction Create(object target, MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            if (target == null && method.IsStatic && BuiltinFunctionDelegates.TryGetValue(method, out var newFunction))
            {
                return newFunction(method);
            }
            return new DelegateCustomFunction(target, method);
        }

        /// <summary>
        /// Returns a <see cref="DynamicCustomFunction"/> from the specified delegate.
        /// </summary>
        /// <param name="del">A delegate</param>
        /// <returns>A custom <see cref="DynamicCustomFunction"/></returns>
        public static DynamicCustomFunction Create(Delegate del)
        {
            if (del == null) throw new ArgumentNullException(nameof(del));
            return new DelegateCustomFunction(del);
        }

        protected struct ArgumentValue
        {
            public ArgumentValue(int index, Type type, object value)
            {
                Index = index;
                Type = type;
                Value = value;
            }

            public readonly int Index;

            public readonly Type Type;

            public readonly object Value;
        }

        private class MethodComparer : IEqualityComparer<MethodInfo>
        {
            public static readonly MethodComparer Default = new MethodComparer();

            public bool Equals(MethodInfo method, MethodInfo otherMethod)
            {
                if (method != null && otherMethod != null && method.ReturnType == otherMethod.ReturnType && method.IsStatic == otherMethod.IsStatic)
                {
                    if (method.DeclaringType.FullName != otherMethod.DeclaringType.FullName)
                        return false;
                    if (method.Name != otherMethod.Name)
                        return false;
                    var parameters = method.GetParameters();
                    var otherParameters = otherMethod.GetParameters();
                    var length = parameters.Length;
                    if (length == otherParameters.Length)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            var param = parameters[i];
                            var otherParam = otherParameters[i];
                            if (param.ParameterType != otherParam.ParameterType || param.IsOptional != otherParam.IsOptional)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            public int GetHashCode(MethodInfo method)
            {
                var hash = method.ReturnType.GetHashCode();
                if (!method.IsStatic)
                {
                    hash = (hash * 397) ^ 1;
                }
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    hash = (hash * 397) ^ parameters[i].ParameterType.GetHashCode();
                }
                return hash;
            }
        }
    }
}