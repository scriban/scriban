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
    public static partial class CustomFunction
    {
        private static readonly Dictionary<MethodInfo, Func<MethodInfo, IScriptCustomFunction>> BuiltinFunctions = new Dictionary<MethodInfo, Func<MethodInfo, IScriptCustomFunction>>(MethodComparer.Default);

        /// <summary>
        /// Returns a <see cref="IScriptCustomFunction"/> from the specified object target and <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="target">A target object - might be null</param>
        /// <param name="method">A MethodInfo</param>
        /// <returns>A custom <see cref="IScriptCustomFunction"/></returns>
        public static IScriptCustomFunction Create(object target, MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            Func<MethodInfo, IScriptCustomFunction> newFunction;
            if (target == null && method.IsStatic && BuiltinFunctions.TryGetValue(method, out newFunction))
            {
                return newFunction(method);
            }
            return new GenericFunctionWrapper(target, method);
        }

        /// <summary>
        /// Generic function wrapper handling any kind of function parameters.
        /// </summary>
        private class GenericFunctionWrapper : IScriptCustomFunction
        {
            private readonly object _target;
            private readonly MethodInfo _method;
            private readonly ParameterInfo[] _parametersInfo;
            private readonly bool _hasObjectParams;
            private readonly int _lastParamsIndex;
            private readonly bool _hasTemplateContext;
            private readonly bool _hasSpan;
            private readonly object[] _arguments;
            private readonly int _defaultParameterCount;

            public GenericFunctionWrapper(object target, MethodInfo method)
            {
                _target = target;
                _method = method;
                _parametersInfo = method.GetParameters();
                _lastParamsIndex = _parametersInfo.Length - 1;
                if (_parametersInfo.Length > 0)
                {
                    // Check if we have TemplateContext+SourceSpan as first parameters
                    if (typeof(TemplateContext).GetTypeInfo().IsAssignableFrom(_parametersInfo[0].ParameterType.GetTypeInfo()))
                    {
                        _hasTemplateContext = true;
                        if (_parametersInfo.Length > 1)
                        {
                            _hasSpan = typeof(SourceSpan).GetTypeInfo().IsAssignableFrom(_parametersInfo[1].ParameterType.GetTypeInfo());
                        }
                    }

                    var lastParam = _parametersInfo[_lastParamsIndex];

                    if ((lastParam.Attributes & ParameterAttributes.HasDefault) == ParameterAttributes.HasDefault)
                    {
                        _defaultParameterCount++;
                    }

                    if (lastParam.ParameterType == typeof(object[]))
                    {
                        foreach (var param in lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false))
                        {
                            _hasObjectParams = true;
                            break;
                        }
                    }
                }
                _arguments = new object[_parametersInfo.Length];
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var expectedNumberOfParameters = _parametersInfo.Length;
                if (_hasTemplateContext)
                {
                    expectedNumberOfParameters--;
                    if (_hasSpan)
                    {
                        expectedNumberOfParameters--;
                    }
                }

                var minimumRequiredParameters = expectedNumberOfParameters - _defaultParameterCount;

                // Check parameters
                if ((_hasObjectParams && arguments.Count < minimumRequiredParameters - 1) || (!_hasObjectParams && arguments.Count < minimumRequiredParameters))
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed `{arguments.Count}` while expecting at least `{minimumRequiredParameters}` to `{expectedNumberOfParameters}` arguments for `{callerContext}`");
                }

                // Convert arguments
                object[] paramArguments = null;
                if (_hasObjectParams)
                {
                    paramArguments = new object[arguments.Count - _lastParamsIndex];
                    _arguments[_lastParamsIndex] = paramArguments;
                }

                // Copy TemplateContext/SourceSpan parameters
                int argIndex = 0;
                if (_hasTemplateContext)
                {
                    _arguments[0] = context;
                    argIndex++;
                    if (_hasSpan)
                    {
                        _arguments[1] = callerContext.Span;
                        argIndex++;
                    }
                }

                for (int i = 0; i < arguments.Count; i++, argIndex++)
                {
                    var destType = _hasObjectParams && i >= _lastParamsIndex ? typeof(object) : _parametersInfo[argIndex].ParameterType;
                    try
                    {
                        var argValue = context.ToObject(callerContext.Span, arguments[i], destType);
                        if (paramArguments != null && i >= _lastParamsIndex)
                        {
                            paramArguments[argIndex - _lastParamsIndex] = argValue;
                        }
                        else
                        {
                            _arguments[argIndex] = argValue;
                        }
                    }
                    catch (Exception exception)
                    {
                        throw new ScriptRuntimeException(callerContext.Span, $"Unable to convert parameter #{i} of type `{arguments[i]?.GetType()}` to type `{destType}`", exception);
                    }
                }

                // Setup any default parameters
                if (_defaultParameterCount > 0)
                {
                    for (int i = arguments.Count; i < expectedNumberOfParameters; i++ , argIndex++)
                    {
                        _arguments[argIndex] = _parametersInfo[argIndex].DefaultValue;
                    }
                }

                // Call method
                try
                {
                    var result = _method.Invoke(_target, _arguments);
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