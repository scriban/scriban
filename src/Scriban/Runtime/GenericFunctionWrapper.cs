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

        private object[] PrepareArguments(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ref object[] paramsArguments)
        {
            // TODO: optimize arguments allocations
            var reflectArgs = context.GetOrCreateReflectionArguments(Parameters.Length);

            // Copy TemplateContext/SourceSpan parameters
            if (_hasTemplateContext)
            {
                reflectArgs[0] = context;
                if (_hasSpan)
                {
                    reflectArgs[1] = callerContext.Span;
                }
            }

            var allArgCount = scriptArguments.Count;

            // Convert arguments
            paramsArguments = null;
            int firstArgIndex = _firstIndexOfUserParameters;
            if (_hasObjectParams)
            {
                // 0         1        _firstIndexOfUserParameters  _paramsIndex
                // [context, [span]], arg0, arg1...,        ,argn, [varg0,varg1, ...]
                int argCount = _paramsIndex - firstArgIndex;
                var paramsCount = allArgCount - argCount;

                paramsArguments = context.GetOrCreateReflectionArguments(paramsCount);
                reflectArgs[_paramsIndex] = paramsArguments;

                if (argCount > 0)
                {
                    // copy arg0, arg1, ..., argn
                    scriptArguments.CopyTo(0, reflectArgs, firstArgIndex, argCount);
                }

                if (paramsCount > 0)
                {
                    scriptArguments.CopyTo(argCount, paramsArguments, 0, paramsCount);
                }
            }
            else
            {
                scriptArguments.CopyTo(0, reflectArgs, firstArgIndex, allArgCount);
            }

            return reflectArgs;
        }
    }
}