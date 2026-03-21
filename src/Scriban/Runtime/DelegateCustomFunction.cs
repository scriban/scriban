// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// Generic function wrapper handling any kind of function parameters.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class DelegateCustomFunction : DynamicCustomFunction
    {
        private readonly Delegate? _del;

        public DelegateCustomFunction(Delegate del) : base(GetDelegateMethod(del), GetDelegateParameterInfos(del))
        {
            _del = del ?? throw new ArgumentNullException(nameof(del));
            Target = del.Target;
        }

        public DelegateCustomFunction(object? target, MethodInfo method) : base(method)
        {
            Target = target;
        }

        public object? Target { get; }

        public override object? Invoke(TemplateContext context, ScriptNode? callerContext, ScriptArray scriptArguments, ScriptBlockStatement? blockStatement)
        {
            Array? paramArguments = null;
            var arguments = PrepareArguments(context, callerContext, scriptArguments, ref paramArguments);
            var callerSpan = callerContext?.Span ?? context.CurrentSpan;
            try
            {
                // Call the method via reflection
                var result = InvokeImpl(context, callerSpan, arguments);
                return result;
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException is not null)
                {
                    throw exception.InnerException;
                }

                throw new ScriptRuntimeException(callerSpan, $"Unexpected exception when calling {callerContext}");
            }
            finally
            {
                context.ReleaseReflectionArguments(arguments);
            }
        }


#if !SCRIBAN_NO_ASYNC
        public override async ValueTask<object?> InvokeAsync(TemplateContext context, ScriptNode? callerContext, ScriptArray scriptArguments, ScriptBlockStatement? blockStatement)
        {
            Array? paramArguments = null;
            var arguments = PrepareArguments(context, callerContext, scriptArguments, ref paramArguments);
            var callerSpan = callerContext?.Span ?? context.CurrentSpan;
            try
            {
                // Call the method via reflection
                var result = InvokeImpl(context, callerSpan, arguments);
                if (!IsAwaitable)
                {
                    return result;
                }

                return await ConfigureAwait(result);
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException is not null)
                {
                    throw exception.InnerException;
                }
                throw new ScriptRuntimeException(callerSpan, $"Unexpected exception when calling {callerContext}");
            }
            finally
            {
                context.ReleaseReflectionArguments(arguments);
            }
        }
#endif

        public static DelegateCustomFunction Create(Action action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction(action);
        }

        public static DelegateCustomFunction Create<T>(Action<T> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T>(action);
        }

        public static DelegateCustomFunction Create<T1, T2>(Action<T1, T2> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2>(action);
        }

        public static DelegateCustomFunction Create<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2, T3>(action);
        }

        public static DelegateCustomFunction Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2, T3, T4>(action);
        }

        public static DelegateCustomFunction Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2, T3, T4, T5>(action);
        }

        public static DelegateCustomFunction CreateFunc<TResult>(Func<TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, TResult>(Func<T1, TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, T3, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, T3, T4, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            if (func is null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, T3, T4, T5, TResult>(func);
        }

        protected virtual object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
        {
            return _del is not null ? _del.DynamicInvoke(arguments) : Method.Invoke(Target, arguments);
        }

        private static MethodInfo GetDelegateMethod(Delegate del)
        {
            if (del is null) throw new ArgumentNullException(nameof(del));
            return del.Method;
        }

        private static ParameterInfo[] GetDelegateParameterInfos(Delegate del)
        {
            if (del is null) throw new ArgumentNullException(nameof(del));

            var delegateInvokeMethod = del.GetType().GetMethod(nameof(Action.Invoke)) ?? throw new ArgumentException("Unable to resolve delegate Invoke method.", nameof(del));
            var methodParameters = del.Method.GetParameters();
            var invokeParameters = delegateInvokeMethod.GetParameters();

            if (methodParameters.Length != invokeParameters.Length)
            {
                return methodParameters;
            }

            var mergedParameters = new ParameterInfo[methodParameters.Length];
            for (int i = 0; i < methodParameters.Length; i++)
            {
                var methodParameter = methodParameters[i];
                var invokeParameter = invokeParameters[i];
                mergedParameters[i] = invokeParameter.HasDefaultValue && !methodParameter.HasDefaultValue ? invokeParameter : methodParameter;
            }

            return mergedParameters;
        }

        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Array element type is known from the method's parameter type at import time.")]
        private object?[] PrepareArguments(TemplateContext context, ScriptNode? callerContext, ScriptArray scriptArguments, ref Array? paramsArguments)
        {
            // TODO: optimize arguments allocations
            var reflectArgs = context.GetOrCreateReflectionArguments(Parameters.Length);
            var callerSpan = callerContext?.Span ?? context.CurrentSpan;

            // Copy TemplateContext/SourceSpan parameters
            if (_hasTemplateContext)
            {
                reflectArgs[0] = context;
                if (_hasSpan)
                {
                    reflectArgs[1] = callerSpan;
                }
            }

            // Convert arguments
            paramsArguments = null;
            if (_varParamKind == ScriptVarParamKind.LastParameter)
            {
                // 0         1        _firstIndexOfUserParameters  _paramsIndex
                // [context, [span]], arg0, arg1...,        ,argn, [varg0,varg1, ...]
                var varArgs = scriptArguments[scriptArguments.Count - 1] as ScriptArray ?? new ScriptArray();
                var paramsElementType = _paramsElementType ?? typeof(object);

                // Copy all normal arguments
                var normalArgCount = scriptArguments.Count - 1;
                for (int i = 0; i < normalArgCount; i++)
                {
                    var destIndex = _firstIndexOfUserParameters + i;
                    reflectArgs[destIndex] = context.ToObject(callerSpan, scriptArguments[i], Parameters[destIndex].ParameterType);
                }

                paramsArguments = paramsElementType == typeof(object) ? context.GetOrCreateReflectionArguments(varArgs.Count) : Array.CreateInstance(paramsElementType, varArgs.Count);
                reflectArgs[_paramsIndex] = paramsArguments;

                // Convert each argument
                for(int i = 0; i < varArgs.Count; i++)
                {
                    var destValue = context.ToObject(callerSpan, varArgs[i], paramsElementType);
                    paramsArguments.SetValue(destValue, i);
                }
            }
            else
            {
                for (int i = 0; i < scriptArguments.Count; i++)
                {
                    var destIndex = _firstIndexOfUserParameters + i;
                    reflectArgs[destIndex] = context.ToObject(callerSpan, scriptArguments[i], Parameters[destIndex].ParameterType);
                }
            }

            for (int i = _firstIndexOfUserParameters + scriptArguments.Count; i < Parameters.Length; i++)
            {
                reflectArgs[i] = Parameters[i].HasDefaultValue ? Parameters[i].DefaultValue : null;
            }

            return reflectArgs;
        }

        /// <summary>
        /// A custom function taking one argument.
        /// </summary>
        /// <typeparam name="TResult">Type result</typeparam>
        private class InternalDelegateCustomFunction<TResult> : DelegateCustomFunction
        {
            public InternalDelegateCustomFunction(Func<TResult> func) : base(func)
            {
                Func = func;
            }

            public Func<TResult> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return Func();
            }
        }

        /// <summary>
        /// A custom function taking one argument.
        /// </summary>
        /// <typeparam name="T">Func 0 arg type</typeparam>
        /// <typeparam name="TResult">Type result</typeparam>
        private class InternalDelegateCustomFunction<T, TResult> : DelegateCustomFunction
        {
            public InternalDelegateCustomFunction(Func<T, TResult> func) : base(func)
            {
                Func = func;
            }

            public Func<T, TResult> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom action taking 1 argument.
        /// </summary>
        /// <typeparam name="T">Func 0 arg type</typeparam>
        private class DelegateCustomAction<T> : DelegateCustomFunction
        {
            public DelegateCustomAction(Action<T> func) : base(func)
            {
                Func = func;
            }

            public Action<T> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom function taking 2 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="TResult">Type result</typeparam>
        private class InternalDelegateCustomFunction<T1, T2, TResult> : DelegateCustomFunction
        {
            public InternalDelegateCustomFunction(Func<T1, T2, TResult> func) : base(func)
            {
                Func = func;
            }

            public Func<T1, T2, TResult> Func { get; }


            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom action taking 2 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        private class DelegateCustomAction<T1, T2> : DelegateCustomFunction
        {
            public DelegateCustomAction(Action<T1, T2> func) : base(func)
            {
                Func = func;
            }

            public Action<T1, T2> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom function taking 3 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="T3">Func 2 arg type</typeparam>
        /// <typeparam name="TResult">Type result</typeparam>
        private class InternalDelegateCustomFunction<T1, T2, T3, TResult> : DelegateCustomFunction
        {
            public InternalDelegateCustomFunction(Func<T1, T2, T3, TResult> func) : base(func)
            {
                Func = func;
            }

            public Func<T1, T2, T3, TResult> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom action taking 3 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="T3">Func 2 arg type</typeparam>
        private class DelegateCustomAction<T1, T2, T3> : DelegateCustomFunction
        {
            public DelegateCustomAction(Action<T1, T2, T3> func) : base(func)
            {
                Func = func;
            }

            public Action<T1, T2, T3> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom function taking 4 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="T3">Func 2 arg type</typeparam>
        /// <typeparam name="T4">Func 3 arg type</typeparam>
        /// <typeparam name="TResult">Type result</typeparam>
        private class InternalDelegateCustomFunction<T1, T2, T3, T4, TResult> : DelegateCustomFunction
        {
            public InternalDelegateCustomFunction(Func<T1, T2, T3, T4, TResult> func) : base(func)
            {
                Func = func;
            }

            public Func<T1, T2, T3, T4, TResult> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom action taking 4 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="T3">Func 2 arg type</typeparam>
        /// <typeparam name="T4">Func 3 arg type</typeparam>
        private class DelegateCustomAction<T1, T2, T3, T4> : DelegateCustomFunction
        {
            public DelegateCustomAction(Action<T1, T2, T3, T4> func) : base(func)
            {
                Func = func;
            }

            public Action<T1, T2, T3, T4> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom function taking 5 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="T3">Func 2 arg type</typeparam>
        /// <typeparam name="T4">Func 3 arg type</typeparam>
        /// <typeparam name="T5">Func 4 arg type</typeparam>
        /// <typeparam name="TResult">Type result</typeparam>
        private class InternalDelegateCustomFunction<T1, T2, T3, T4, T5, TResult> : DelegateCustomFunction
        {
            public InternalDelegateCustomFunction(Func<T1, T2, T3, T4, T5, TResult> func) : base(func)
            {
                Func = func;
            }

            public Func<T1, T2, T3, T4, T5, TResult> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }

        /// <summary>
        /// A custom action taking 5 arguments.
        /// </summary>
        /// <typeparam name="T1">Func 0 arg type</typeparam>
        /// <typeparam name="T2">Func 1 arg type</typeparam>
        /// <typeparam name="T3">Func 2 arg type</typeparam>
        /// <typeparam name="T4">Func 3 arg type</typeparam>
        /// <typeparam name="T5">Func 4 arg type</typeparam>
        private class DelegateCustomAction<T1, T2, T3, T4, T5> : DelegateCustomFunction
        {
            public DelegateCustomAction(Action<T1, T2, T3, T4, T5> func) : base(func)
            {
                Func = func;
            }

            public Action<T1, T2, T3, T4, T5> Func { get; }

            protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
            {
                return base.InvokeImpl(context, span, arguments);
            }
        }
    }

    /// <summary>
    /// A custom action taking 1 argument.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class DelegateCustomAction : DelegateCustomFunction
    {
        public DelegateCustomAction(Action func) : base(func)
        {
            Func = func;
        }

        public Action Func { get; }

        protected override object? InvokeImpl(TemplateContext context, SourceSpan span, object?[] arguments)
        {
            Func();
            return null;
        }
    }
}
