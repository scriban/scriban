// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
#if SCRIBAN_ASYNC
using System.Threading.Tasks;
#endif
using Scriban.Helpers;
using Scriban.Syntax;


namespace Scriban.Runtime
{
    /// <summary>
    /// Creates a reflection based <see cref="IScriptCustomFunction"/> from a <see cref="MethodInfo"/>.
    /// </summary>
    public abstract partial class DynamicCustomFunction : IScriptCustomFunction
    {
        private static readonly Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>> BuiltinFunctionDelegates = new Dictionary<MethodInfo, Func<MethodInfo, DynamicCustomFunction>>(MethodComparer.Default);

        /// <summary>
        /// Gets the reflection method associated to this dynamic call.
        /// </summary>
        public readonly MethodInfo Method;

        protected readonly ParameterInfo[] Parameters;

#if SCRIBAN_ASYNC
        protected readonly bool IsAwaitable;
#endif

        protected DynamicCustomFunction(MethodInfo method)
        {
            Method = method;
            Parameters = method.GetParameters();
#if SCRIBAN_ASYNC
            IsAwaitable = method.ReturnType.GetTypeInfo().GetDeclaredMethod(nameof(Task.GetAwaiter)) != null;
#endif
        }


#if SCRIBAN_ASYNC
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
                if (arg.Name == namedArg.Name)
                {
                    return new ArgumentValue(j, arg.ParameterType, context.Evaluate(namedArg));
                }
            }
            throw new ScriptRuntimeException(callerContext.Span, $"Invalid argument `{namedArg.Name}` not found for function `{callerContext}`");
        }

        public abstract object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement);

#if SCRIBAN_ASYNC
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
            return new GenericFunctionWrapper(target, method);
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