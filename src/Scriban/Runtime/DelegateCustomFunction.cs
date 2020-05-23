// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Reflection;
using System.Threading.Tasks;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// Generic function wrapper handling any kind of function parameters.
    /// </summary>
    public partial class DelegateCustomFunction : DynamicCustomFunction
    {
        private readonly Delegate _del;

        public DelegateCustomFunction(Delegate del) : base(del?.Method)
        {
            _del = del ?? throw new ArgumentNullException(nameof(del));
            Target = del.Target;
        }

        public DelegateCustomFunction(object target, MethodInfo method) : base(method)
        {
            Target = target;
        }

        public object Target { get; }

        public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ScriptBlockStatement blockStatement)
        {
            Array paramArguments = null;
            var arguments = PrepareArguments(context, callerContext, scriptArguments, ref paramArguments);
            try
            {
                // Call the method via reflection
                var result = InvokeImpl(context, callerContext.Span, arguments);
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
            }
        }


#if !SCRIBAN_NO_ASYNC
        public override async ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ScriptBlockStatement blockStatement)
        {
            Array paramArguments = null;
            var arguments = PrepareArguments(context, callerContext, scriptArguments, ref paramArguments);
            try
            {
                // Call the method via reflection
                var result = InvokeImpl(context, callerContext.Span, arguments);
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
            }
        }
#endif

        public static DelegateCustomFunction Create(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction(action);
        }

        public static DelegateCustomFunction Create<T>(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T>(action);
        }

        public static DelegateCustomFunction Create<T1, T2>(Action<T1, T2> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2>(action);
        }

        public static DelegateCustomFunction Create<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2, T3>(action);
        }

        public static DelegateCustomFunction Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2, T3, T4>(action);
        }

        public static DelegateCustomFunction Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return new DelegateCustomAction<T1, T2, T3, T4, T5>(action);
        }

        public static DelegateCustomFunction CreateFunc<TResult>(Func<TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, TResult>(Func<T1, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, TResult>(Func<T1, T2, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, T3, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, T3, T4, TResult>(func);
        }

        public static DelegateCustomFunction CreateFunc<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));
            return new InternalDelegateCustomFunction<T1, T2, T3, T4, T5, TResult>(func);
        }

        protected virtual object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            return _del != null ? _del.DynamicInvoke(arguments) : Method.Invoke(Target, arguments);
        }

        private object[] PrepareArguments(TemplateContext context, ScriptNode callerContext, ScriptArray scriptArguments, ref Array paramsArguments)
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

                paramsArguments = _paramsElementType == typeof(object) ? context.GetOrCreateReflectionArguments(paramsCount) : Array.CreateInstance(_paramsElementType, paramsCount);
                reflectArgs[_paramsIndex] = paramsArguments;

                if (argCount > 0)
                {
                    // copy arg0, arg1, ..., argn
                    scriptArguments.CopyTo(0, reflectArgs, firstArgIndex, argCount);
                }

                if (paramsCount > 0)
                {
                    for(int i = 0; i < paramsCount; i++)
                    {
                        paramsArguments.SetValue(scriptArguments[argCount + i], i);
                    }
                }
            }
            else
            {
                scriptArguments.CopyTo(0, reflectArgs, firstArgIndex, allArgCount);
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg = (T)arguments[0];
                return Func(arg);
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg = (T)arguments[0];
                Func(arg);
                return null;
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


            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                return Func(arg1, arg2);
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                Func(arg1, arg2);
                return null;
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                var arg3 = (T3)arguments[2];
                return Func(arg1, arg2, arg3);
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                var arg3 = (T3)arguments[2];
                Func(arg1, arg2, arg3);
                return null;
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                var arg3 = (T3)arguments[2];
                var arg4 = (T4)arguments[3];
                return Func(arg1, arg2, arg3, arg4);
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                var arg3 = (T3)arguments[2];
                var arg4 = (T4)arguments[3];
                Func(arg1, arg2, arg3, arg4);
                return null;
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                var arg3 = (T3)arguments[2];
                var arg4 = (T4)arguments[3];
                var arg5 = (T5)arguments[4];
                return Func(arg1, arg2, arg3, arg4, arg5);
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

            protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
            {
                var arg1 = (T1)arguments[0];
                var arg2 = (T2)arguments[1];
                var arg3 = (T3)arguments[2];
                var arg4 = (T4)arguments[3];
                var arg5 = (T5)arguments[4];
                Func(arg1, arg2, arg3, arg4, arg5);
                return null;
            }
        }
    }

    /// <summary>
    /// A custom action taking 1 argument.
    /// </summary>
    public class DelegateCustomAction : DelegateCustomFunction
    {
        public DelegateCustomAction(Action func) : base(func)
        {
            Func = func;
        }

        public Action Func { get; }

        protected override object InvokeImpl(TemplateContext context, SourceSpan span, object[] arguments)
        {
            Func();
            return null;
        }
    }
}