// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.

using System;
using System.Reflection;
using Scriban.Parsing;
using Scriban.Syntax;
using Scriban.Helpers;

namespace Scriban.Runtime
{
    /// <summary>
    /// Generic function wrapper handling any kind of function parameters.
    /// </summary>
    partial class GenericFunctionWrapper : DynamicCustomFunction
    {
        private readonly object _target;
        private readonly object[] _arguments;

        public GenericFunctionWrapper(object target, MethodInfo method) : base(method)
        {
            _target = target;
            _arguments = new object[Parameters.Length];
        }

        public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            // Check parameters
            if ((_hasObjectParams && arguments.Count < _minimumRequiredParameters - 1) || (!_hasObjectParams && (arguments.Count < _minimumRequiredParameters || arguments.Count > _expectedNumberOfParameters)))
            {
                if (_minimumRequiredParameters != _expectedNumberOfParameters)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `{_minimumRequiredParameters}` arguments");
                }
                else
                {
                    if (arguments.Count > 0)
                    {
                        throw new ScriptArgumentException(arguments.Count - 1, $"Invalid number of arguments `{arguments.Count}` passed in `{callerContext}` while expecting `{_expectedNumberOfParameters}` arguments");
                    }
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed in `{callerContext}` while expecting `{_expectedNumberOfParameters}` arguments");
                }
            }

            // Convert arguments
            object[] paramArguments = null;
            var argMask = 0;
            if (_hasObjectParams)
            {
                var objectParamsCount = arguments.Count - _lastParamsIndex;
                if (_hasTemplateContext)
                {
                    objectParamsCount++;
                    if (_hasSpan)
                    {
                        objectParamsCount++;
                    }
                }
                paramArguments = new object[objectParamsCount];
                _arguments[_lastParamsIndex] = paramArguments;
                argMask |= 1 << _lastParamsIndex;
            }

            // Copy TemplateContext/SourceSpan parameters
            int argOffset = 0;
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
                        var namedArgValue = GetValueFromNamedArgument(context, callerContext, namedArg);
                        arg = namedArgValue.Value;
                        argIndex = namedArgValue.Index;
                        argType = namedArgValue.Type;
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
                    }
                    else
                    {
                        _arguments[argIndex] = argValue;
                        argMask |= 1 << argIndex;
                    }
                }
                catch
                {
                    throw new ScriptArgumentException(i, $"Unable to convert parameter of type `{arguments[i]?.GetType().ScriptFriendlyName()}` to type `{argType?.ScriptFriendlyName()}`");
                }
            }

            // In case we have named arguments we need to verify that all arguments were set
            if (argMask != (1 << Parameters.Length) - 1)
            {
                if (_minimumRequiredParameters != _expectedNumberOfParameters)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `{_minimumRequiredParameters}` arguments");
                }
                else
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `{_expectedNumberOfParameters}` arguments");
                }
            }

            // Call method
            try
            {
                var result = Method.Invoke(_target, _arguments);
                // NOTE: The following line should not be touch as it is being matched by ScribanAsyncCodeGen
                return result;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw exception.InnerException;
                }
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception when calling {callerContext}");
            }
        }
    }
}