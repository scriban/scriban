// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.

using System;
using System.Reflection;
using System.Threading.Tasks;
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

        public GenericFunctionWrapper(object target, MethodInfo method) : base(method)
        {
            _target = target;
        }

        public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ScriptBlockStatement blockStatement)
        {
            object[] paramArguments = null;
            var arguments = PrepareArguments(context, callerContext, scriptArguments, ref paramArguments);
            try
            {
                // Call the method via reflection
                var result = Method.Invoke(_target, arguments);
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
            finally
            {
                context.ReleaseReflectionArguments(arguments);
                context.ReleaseReflectionArguments(paramArguments);
            }
        }

#if !SCRIBAN_NO_ASYNC
        public override async ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ScriptBlockStatement blockStatement)
        {
            object[] paramArguments = null;
            var arguments = PrepareArguments(context, callerContext, scriptArguments, ref paramArguments);
            try
            {
                // Call the method via reflection
                var result = Method.Invoke(_target, arguments);
                return IsAwaitable ? await ConfigureAwait(result) : result;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw exception.InnerException;
                }
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected exception when calling {callerContext}");
            }
            finally
            {
                context.ReleaseReflectionArguments(arguments);
                context.ReleaseReflectionArguments(paramArguments);
            }
        }
#endif

        private object[] PrepareArguments(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ref object[] paramArguments)
        {
            // TODO: optimize arguments allocations
            var arguments = context.GetOrCreateReflectionArguments(Parameters.Length);

            // Convert arguments
            paramArguments = null;
            if (_hasObjectParams)
            {
                var objectParamsCount = scriptArguments.Count - _paramsIndex;
                if (_hasTemplateContext)
                {
                    objectParamsCount++;
                    if (_hasSpan)
                    {
                        objectParamsCount++;
                    }
                }

                paramArguments = context.GetOrCreateReflectionArguments(objectParamsCount);
                arguments[_paramsIndex] = paramArguments;
            }

            // Copy TemplateContext/SourceSpan parameters
            int argOffset = 0;
            if (_hasTemplateContext)
            {
                arguments[0] = context;
                argOffset++;
                if (_hasSpan)
                {
                    arguments[1] = callerContext.Span;
                    argOffset++;
                }
            }

            var argIndex = argOffset;
            int paramsIndex = 0;
            for (int i = 0; i < scriptArguments.Count; i++)
            {
                var argValue = scriptArguments[i];
                if (_hasObjectParams && paramArguments != null && argIndex >= _paramsIndex)
                {
                    paramArguments[paramsIndex] = argValue;
                    paramsIndex++;
                }
                else
                {
                    arguments[argIndex++] = argValue;
                }
            }

            return arguments;
        }
    }
}