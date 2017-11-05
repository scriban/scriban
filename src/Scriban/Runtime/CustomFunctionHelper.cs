// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    public static class CustomFunctionHelper
    {
        /// <summary>
        /// Returns a <see cref="IScriptCustomFunction"/> from the specified object target and <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="target">A target object - might be null</param>
        /// <param name="method">A MethodInfo</param>
        /// <returns>A custom <see cref="IScriptCustomFunction"/></returns>
        public static IScriptCustomFunction GetCustomFunction(object target, MethodInfo method)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            var parameters = method.GetParameters();
            if (target == null)
            {
                if (method.ReturnType == typeof(string))
                {
                    if (parameters.Length == 1)
                    {
                        var arg0 = parameters[0].ParameterType;

                        if (arg0 == typeof(string))
                        {
                            return new StringToStringFunction(method);
                        }
                    }
                    else if (parameters.Length == 2)
                    {
                        var arg0 = parameters[0].ParameterType;
                        var arg1 = parameters[1].ParameterType;

                        if (arg1 == typeof(string))
                        {
                            if (arg0 == typeof(int))
                            {
                                return new IntStringToStringFunction(method);
                            }
                            if (arg0 == typeof(string))
                            {
                                return new StringStringToStringFunction(method);
                            }
                        }
                    }
                    else if (parameters.Length == 3)
                    {
                        var arg0 = parameters[0].ParameterType;
                        var arg1 = parameters[1].ParameterType;
                        var arg2 = parameters[2].ParameterType;
                        if (arg0 == typeof(string) && arg1 == typeof(string) && arg2 == typeof(string))
                        {
                            return new StringStringStringToStringFunction(method);
                        }
                    }
                }
            }

            return new GenericFunctionWrapper(target, method);
        }

        /// <summary>
        /// Optimized custom function for: string XXX(int, string)
        /// </summary>
        private class IntStringToStringFunction : IScriptCustomFunction
        {
            private delegate string InternalDelegate(int value, string text);

            private readonly InternalDelegate _delegate;

            public IntStringToStringFunction(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments,
                ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed `{arguments.Count}` while expecting `{2}` for `{callerContext}`");
                }
                var arg0 = context.ToInt(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string XXX(string, string)
        /// </summary>
        private class StringStringToStringFunction : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string value, string text);

            private readonly InternalDelegate _delegate;

            public StringStringToStringFunction(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed `{arguments.Count}` while expecting `{2}` for `{callerContext}`");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string XXX(string, string, string)
        /// </summary>
        private class StringStringStringToStringFunction : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string a, string b, string c);

            private readonly InternalDelegate _delegate;

            public StringStringStringToStringFunction(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 3)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed `{arguments.Count}` while expecting `{3}` for `{callerContext}`");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);
                var arg2 = context.ToString(callerContext.Span, arguments[2]);

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string XXX(string)
        /// </summary>
        private class StringToStringFunction : IScriptCustomFunction
        {
            private delegate string StringToStringDelegate(string value);

            private readonly StringToStringDelegate _delegate;

            public StringToStringFunction(MethodInfo method)
            {
                _delegate = (StringToStringDelegate)method.CreateDelegate(typeof(StringToStringDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments passed `{arguments.Count}` while expecting `{1}` for `{callerContext}`");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                return _delegate(arg0);
            }
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
    }
}