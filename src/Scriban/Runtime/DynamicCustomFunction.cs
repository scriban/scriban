// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// Creates a reflection based <see cref="IScriptCustomFunction"/> from a <see cref="MethodInfo"/>.
    /// </summary>
    public abstract partial class DynamicCustomFunction : IScriptCustomFunction
    {
        private static readonly Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>> BuiltinFunctions = new Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>>(MethodComparer.Default);

        /// <summary>
        /// Gets the reflection method associated to this dynamic call.
        /// </summary>
        public readonly MethodInfo Method;

        protected readonly ParameterInfo[] Parameters;

        protected DynamicCustomFunction(MethodInfo method)
        {
            Method = method;
            Parameters = method.GetParameters();
        }

        protected object GetNamedArgument(TemplateContext context, ScriptNode callerContext, ScriptNamedArgument namedArg, out int argIndex, out Type argType)
        {
            for (int j = 0; j < Parameters.Length; j++)
            {
                var arg = Parameters[j];
                if (arg.Name == namedArg.Name)
                {
                    argIndex = j;
                    argType = arg.ParameterType;
                    return context.Evaluate(namedArg);
                }
            }
            throw new ScriptRuntimeException(callerContext.Span, $"Invalid argument `{namedArg.Name}` not found for function `{callerContext}`");
        }

        public abstract object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);

        /// <summary>
        /// Returns a <see cref="DynamicCustomFunction"/> from the specified object target and <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="target">A target object - might be null</param>
        /// <param name="method">A MethodInfo</param>
        /// <returns>A custom <see cref="DynamicCustomFunction"/></returns>
        public static DynamicCustomFunction Create(object target, MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            Func<MethodInfo, DynamicCustomFunction> newFunction;
            if (target == null && method.IsStatic && BuiltinFunctions.TryGetValue(method, out newFunction))
            {
                return newFunction(method);
            }
            return new GenericFunctionWrapper(target, method);
        }

        /// <summary>
        /// Generic function wrapper handling any kind of function parameters.
        /// </summary>
        private class GenericFunctionWrapper : DynamicCustomFunction
        {
            private readonly object _target;
            private readonly bool _hasObjectParams;
            private readonly int _lastParamsIndex;
            private readonly bool _hasTemplateContext;
            private readonly bool _hasSpan;
            private readonly object[] _arguments;
            private readonly int _optionalParameterCount;
            private readonly Type _paramsElementType;

            public GenericFunctionWrapper(object target, MethodInfo method) : base(method)
            {
                _target = target;
                _lastParamsIndex = Parameters.Length - 1;
                if (Parameters.Length > 0)
                {
                    // Check if we have TemplateContext+SourceSpan as first parameters
                    if (typeof(TemplateContext).GetTypeInfo().IsAssignableFrom(Parameters[0].ParameterType.GetTypeInfo()))
                    {
                        _hasTemplateContext = true;
                        if (Parameters.Length > 1)
                        {
                            _hasSpan = typeof(SourceSpan).GetTypeInfo().IsAssignableFrom(Parameters[1].ParameterType.GetTypeInfo());
                        }
                    }

                    var lastParam = Parameters[_lastParamsIndex];
                    if (lastParam.ParameterType.IsArray)
                    {
                        foreach (var param in lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false))
                        {
                            _hasObjectParams = true;
                            _paramsElementType = lastParam.ParameterType.GetElementType();
                            break;
                        }
                    }
                }

                if (!_hasObjectParams)
                {
                    for (int i = 0; i < Parameters.Length; i++)
                    {
                        if (Parameters[i].IsOptional)
                        {
                            _optionalParameterCount++;
                        }
                    }
                }

                _arguments = new object[Parameters.Length];
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var expectedNumberOfParameters = Parameters.Length;
                if (_hasTemplateContext)
                {
                    expectedNumberOfParameters--;
                    if (_hasSpan)
                    {
                        expectedNumberOfParameters--;
                    }
                }

                var minimumRequiredParameters = expectedNumberOfParameters - _optionalParameterCount;

                // Check parameters
                if ((_hasObjectParams && arguments.Count < minimumRequiredParameters - 1) || (!_hasObjectParams && arguments.Count < minimumRequiredParameters))
                {
                    if (minimumRequiredParameters != expectedNumberOfParameters)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `{minimumRequiredParameters}` arguments");
                    }
                    else
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `{expectedNumberOfParameters}` arguments");
                    }
                }

                // Convert arguments
                object[] paramArguments = null;
                if (_hasObjectParams)
                {
                    paramArguments = new object[arguments.Count - _lastParamsIndex];
                    _arguments[_lastParamsIndex] = paramArguments;
                }

                // Copy TemplateContext/SourceSpan parameters
                int argOffset = 0;
                var argMask = 0;
                if (_hasTemplateContext)
                {
                    _arguments[0] = context;
                    argOffset++;
                    argMask |= 1;
                    if (_hasSpan)
                    {
                        _arguments[1] = callerContext.Span;
                        argOffset++;
                        argMask |= 2;
                    }
                }

                var argOrderedIndex = argOffset;

                // Setup any default parameters
                if (_optionalParameterCount > 0)
                {
                    for (int i = Parameters.Length - 1; i >= Parameters.Length - _optionalParameterCount; i--)
                    {
                        _arguments[i] = Parameters[i].DefaultValue;
                        argMask |= 1 << i;
                    }
                }

                int paramsIndex = 0;
                for (int i = 0; i < arguments.Count; i++)
                {
                    Type argType = null;
                    try
                    {
                        int argIndex;
                        var arg = arguments[i];
                        var namedArg = arg as ScriptNamedArgument;
                        if (namedArg != null)
                        {
                            arg = GetNamedArgument(context, callerContext, namedArg, out argIndex, out argType);
                            if (_hasObjectParams && argIndex == _lastParamsIndex)
                            {
                                argType = _paramsElementType;
                                argIndex = argIndex + paramsIndex;
                                paramsIndex++;
                            }
                        }
                        else
                        {
                            argIndex = argOrderedIndex;
                            if (_hasObjectParams && argIndex == _lastParamsIndex)
                            {
                                argType = _paramsElementType;
                                argIndex = argIndex + paramsIndex;
                                paramsIndex++;
                            }
                            else
                            {
                                argType = Parameters[argIndex].ParameterType;
                                argOrderedIndex++;
                            }
                        }

                        var argValue = context.ToObject(callerContext.Span, arg, argType);
                        if (paramArguments != null && argIndex >= _lastParamsIndex)
                        {
                            paramArguments[argIndex - _lastParamsIndex] = argValue;
                            argMask |= 1 << _lastParamsIndex;
                        }
                        else
                        {
                            _arguments[argIndex] = argValue;
                            argMask |= 1 << argIndex;
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Unable to convert parameter #{i} of type `{arguments[i]?.GetType()}` to type `{argType}`", exception);
                    }
                }

                // In case we have named arguments we need to verify that all arguments were set
                if (argMask != (1 << Parameters.Length) - 1)
                {
                    if (minimumRequiredParameters != expectedNumberOfParameters)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `{minimumRequiredParameters}` arguments");
                    }
                    else
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `{expectedNumberOfParameters}` arguments");
                    }
                }

                // Call method
                try
                {
                    var result = Method.Invoke(_target, _arguments);
                    return result;
                }
                catch (TargetInvocationException exception)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception when calling {callerContext}", exception.InnerException);
                }
            }
        }

        private class MethodComparer : IEqualityComparer<MethodInfo>
        {
            public static readonly MethodComparer Default = new MethodComparer();

            public bool Equals(MethodInfo method, MethodInfo otherMethod)
            {
                if (method != null && otherMethod != null && method.ReturnType == otherMethod.ReturnType && method.IsStatic == otherMethod.IsStatic)
                {
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