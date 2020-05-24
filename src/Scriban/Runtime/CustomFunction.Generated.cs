// ----------------------------------------------------------------------------------
// This file was automatically generated - 05/24/2020 19:23:26 by Scriban.DelegateCodeGen
// DOT NOT EDIT THIS FILE MANUALLY
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    public abstract partial class DynamicCustomFunction
    {


        static DynamicCustomFunction()
        {
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Contains), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionbool_IEnumerable_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.IsNumber), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionbool_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Contains), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionbool_string_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.DateTimeFunctions).GetMethod(nameof(Scriban.Functions.DateTimeFunctions.Now), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionDateTime(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.DateTimeFunctions).GetMethod(nameof(Scriban.Functions.DateTimeFunctions.AddDays), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionDateTime_DateTime_double(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.DateTimeFunctions).GetMethod(nameof(Scriban.Functions.DateTimeFunctions.AddMonths), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionDateTime_DateTime_int(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.Ceil), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functiondouble_double(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.Round), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functiondouble_double_int___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Compact), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_IEnumerable(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.AddRange), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_IEnumerable_IEnumerable(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Limit), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_IEnumerable_int(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.InsertAt), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_IEnumerable_int_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Add), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_IEnumerable_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Split), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_string_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Sort), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_TemplateContext_SourceSpan_object_string___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Map), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIEnumerable_TemplateContext_SourceSpan_object_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.RemoveAt), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionIList_IList_int(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Size), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionint_IEnumerable(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ObjectFunctions).GetMethod(nameof(Scriban.Functions.ObjectFunctions.Size), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionint_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Size), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionint_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.First), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_IEnumerable(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ObjectFunctions).GetMethod(nameof(Scriban.Functions.ObjectFunctions.Default), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_object_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.DividedBy), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_TemplateContext_SourceSpan_double_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Cycle), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_TemplateContext_SourceSpan_IList_object___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.Abs), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_TemplateContext_SourceSpan_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.Minus), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_TemplateContext_SourceSpan_object_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.ToInt), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionobject_TemplateContext_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ObjectFunctions).GetMethod(nameof(Scriban.Functions.ObjectFunctions.Keys), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionScriptArray_TemplateContext_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.RegexFunctions).GetMethod(nameof(Scriban.Functions.RegexFunctions.Match), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionScriptArray_TemplateContext_string_string_string___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Pluralize), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_int_string_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ObjectFunctions).GetMethod(nameof(Scriban.Functions.ObjectFunctions.Typeof), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_object(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.HtmlFunctions).GetMethod(nameof(Scriban.Functions.HtmlFunctions.Escape), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.PadLeft), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_string_int(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Slice1), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_string_int_int___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Truncate), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_string_int_string___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Append), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_string_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.StringFunctions).GetMethod(nameof(Scriban.Functions.StringFunctions.Replace), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_string_string_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.ArrayFunctions).GetMethod(nameof(Scriban.Functions.ArrayFunctions.Join), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_TemplateContext_SourceSpan_IEnumerable_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.MathFunctions).GetMethod(nameof(Scriban.Functions.MathFunctions.Format), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_TemplateContext_SourceSpan_object_string_string___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.HtmlFunctions).GetMethod(nameof(Scriban.Functions.HtmlFunctions.Strip), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_TemplateContext_string(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.RegexFunctions).GetMethod(nameof(Scriban.Functions.RegexFunctions.Replace), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new Functionstring_TemplateContext_string_string_string_string___Opt(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.TimeSpanFunctions).GetMethod(nameof(Scriban.Functions.TimeSpanFunctions.FromDays), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionTimeSpan_double(method));
            BuiltinFunctionDelegates.Add(typeof(Scriban.Functions.TimeSpanFunctions).GetMethod(nameof(Scriban.Functions.TimeSpanFunctions.Parse), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly), method => new FunctionTimeSpan_string(method));

        }

        /// <summary>
        /// Optimized custom function for: bool (IEnumerable, object)
        /// </summary>
        private partial class Functionbool_IEnumerable_object : DynamicCustomFunction
        {
            private delegate bool InternalDelegate(IEnumerable arg0, object arg1);

            private readonly InternalDelegate _delegate;

            public Functionbool_IEnumerable_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];
                var arg1 = (object)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: bool (object)
        /// </summary>
        private partial class Functionbool_object : DynamicCustomFunction
        {
            private delegate bool InternalDelegate(object arg0);

            private readonly InternalDelegate _delegate;

            public Functionbool_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: bool (string, string)
        /// </summary>
        private partial class Functionbool_string_string : DynamicCustomFunction
        {
            private delegate bool InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public Functionbool_string_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (string)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: DateTime ()
        /// </summary>
        private partial class FunctionDateTime : DynamicCustomFunction
        {
            private delegate DateTime InternalDelegate();

            private readonly InternalDelegate _delegate;

            public FunctionDateTime(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {

                return _delegate();
            }
        }

        /// <summary>
        /// Optimized custom function for: DateTime (DateTime, double)
        /// </summary>
        private partial class FunctionDateTime_DateTime_double : DynamicCustomFunction
        {
            private delegate DateTime InternalDelegate(DateTime arg0, double arg1);

            private readonly InternalDelegate _delegate;

            public FunctionDateTime_DateTime_double(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (DateTime)arguments[0];
                var arg1 = (double)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: DateTime (DateTime, int)
        /// </summary>
        private partial class FunctionDateTime_DateTime_int : DynamicCustomFunction
        {
            private delegate DateTime InternalDelegate(DateTime arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public FunctionDateTime_DateTime_int(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (DateTime)arguments[0];
                var arg1 = (int)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: double (double)
        /// </summary>
        private partial class Functiondouble_double : DynamicCustomFunction
        {
            private delegate double InternalDelegate(double arg0);

            private readonly InternalDelegate _delegate;

            public Functiondouble_double(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (double)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: double (double, int = ...)
        /// </summary>
        private partial class Functiondouble_double_int___Opt : DynamicCustomFunction
        {
            private delegate double InternalDelegate(double arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public Functiondouble_double_int___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (double)arguments[0];
                var arg1 = (int)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (IEnumerable)
        /// </summary>
        private partial class FunctionIEnumerable_IEnumerable : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_IEnumerable(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (IEnumerable, IEnumerable)
        /// </summary>
        private partial class FunctionIEnumerable_IEnumerable_IEnumerable : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(IEnumerable arg0, IEnumerable arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_IEnumerable_IEnumerable(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];
                var arg1 = (IEnumerable)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (IEnumerable, int)
        /// </summary>
        private partial class FunctionIEnumerable_IEnumerable_int : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(IEnumerable arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_IEnumerable_int(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];
                var arg1 = (int)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (IEnumerable, int, object)
        /// </summary>
        private partial class FunctionIEnumerable_IEnumerable_int_object : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(IEnumerable arg0, int arg1, object arg2);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_IEnumerable_int_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];
                var arg1 = (int)arguments[1];
                var arg2 = (object)arguments[2];

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (IEnumerable, object)
        /// </summary>
        private partial class FunctionIEnumerable_IEnumerable_object : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(IEnumerable arg0, object arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_IEnumerable_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];
                var arg1 = (object)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (string, string)
        /// </summary>
        private partial class FunctionIEnumerable_string_string : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_string_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (string)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (TemplateContext, SourceSpan, object, string = ...)
        /// </summary>
        private partial class FunctionIEnumerable_TemplateContext_SourceSpan_object_string___Opt : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, string arg3);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_TemplateContext_SourceSpan_object_string___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];
                var arg1 = (string)arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (TemplateContext, SourceSpan, object, string)
        /// </summary>
        private partial class FunctionIEnumerable_TemplateContext_SourceSpan_object_string : DynamicCustomFunction
        {
            private delegate IEnumerable InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, string arg3);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerable_TemplateContext_SourceSpan_object_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];
                var arg1 = (string)arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IList (IList, int)
        /// </summary>
        private partial class FunctionIList_IList_int : DynamicCustomFunction
        {
            private delegate IList InternalDelegate(IList arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIList_IList_int(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IList)arguments[0];
                var arg1 = (int)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: int (IEnumerable)
        /// </summary>
        private partial class Functionint_IEnumerable : DynamicCustomFunction
        {
            private delegate int InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public Functionint_IEnumerable(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: int (object)
        /// </summary>
        private partial class Functionint_object : DynamicCustomFunction
        {
            private delegate int InternalDelegate(object arg0);

            private readonly InternalDelegate _delegate;

            public Functionint_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: int (string)
        /// </summary>
        private partial class Functionint_string : DynamicCustomFunction
        {
            private delegate int InternalDelegate(string arg0);

            private readonly InternalDelegate _delegate;

            public Functionint_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (IEnumerable)
        /// </summary>
        private partial class Functionobject_IEnumerable : DynamicCustomFunction
        {
            private delegate object InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public Functionobject_IEnumerable(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (object, object)
        /// </summary>
        private partial class Functionobject_object_object : DynamicCustomFunction
        {
            private delegate object InternalDelegate(object arg0, object arg1);

            private readonly InternalDelegate _delegate;

            public Functionobject_object_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];
                var arg1 = (object)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, SourceSpan, double, object)
        /// </summary>
        private partial class Functionobject_TemplateContext_SourceSpan_double_object : DynamicCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, SourceSpan arg1, double arg2, object arg3);

            private readonly InternalDelegate _delegate;

            public Functionobject_TemplateContext_SourceSpan_double_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (double)arguments[0];
                var arg1 = (object)arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, SourceSpan, IList, object = ...)
        /// </summary>
        private partial class Functionobject_TemplateContext_SourceSpan_IList_object___Opt : DynamicCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, SourceSpan arg1, IList arg2, object arg3);

            private readonly InternalDelegate _delegate;

            public Functionobject_TemplateContext_SourceSpan_IList_object___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IList)arguments[0];
                var arg1 = (object)arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, SourceSpan, object)
        /// </summary>
        private partial class Functionobject_TemplateContext_SourceSpan_object : DynamicCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2);

            private readonly InternalDelegate _delegate;

            public Functionobject_TemplateContext_SourceSpan_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];

                return _delegate(context, callerContext.Span, arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, SourceSpan, object, object)
        /// </summary>
        private partial class Functionobject_TemplateContext_SourceSpan_object_object : DynamicCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, object arg3);

            private readonly InternalDelegate _delegate;

            public Functionobject_TemplateContext_SourceSpan_object_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];
                var arg1 = (object)arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, string)
        /// </summary>
        private partial class Functionobject_TemplateContext_string : DynamicCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public Functionobject_TemplateContext_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];

                return _delegate(context, arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: ScriptArray (TemplateContext, object)
        /// </summary>
        private partial class FunctionScriptArray_TemplateContext_object : DynamicCustomFunction
        {
            private delegate ScriptArray InternalDelegate(TemplateContext arg0, object arg1);

            private readonly InternalDelegate _delegate;

            public FunctionScriptArray_TemplateContext_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];

                return _delegate(context, arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: ScriptArray (TemplateContext, string, string, string = ...)
        /// </summary>
        private partial class FunctionScriptArray_TemplateContext_string_string_string___Opt : DynamicCustomFunction
        {
            private delegate ScriptArray InternalDelegate(TemplateContext arg0, string arg1, string arg2, string arg3);

            private readonly InternalDelegate _delegate;

            public FunctionScriptArray_TemplateContext_string_string_string___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (string)arguments[1];
                var arg2 = (string)arguments[2];

                return _delegate(context, arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (int, string, string)
        /// </summary>
        private partial class Functionstring_int_string_string : DynamicCustomFunction
        {
            private delegate string InternalDelegate(int arg0, string arg1, string arg2);

            private readonly InternalDelegate _delegate;

            public Functionstring_int_string_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (int)arguments[0];
                var arg1 = (string)arguments[1];
                var arg2 = (string)arguments[2];

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (object)
        /// </summary>
        private partial class Functionstring_object : DynamicCustomFunction
        {
            private delegate string InternalDelegate(object arg0);

            private readonly InternalDelegate _delegate;

            public Functionstring_object(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string)
        /// </summary>
        private partial class Functionstring_string : DynamicCustomFunction
        {
            private delegate string InternalDelegate(string arg0);

            private readonly InternalDelegate _delegate;

            public Functionstring_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, int)
        /// </summary>
        private partial class Functionstring_string_int : DynamicCustomFunction
        {
            private delegate string InternalDelegate(string arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public Functionstring_string_int(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (int)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, int, int = ...)
        /// </summary>
        private partial class Functionstring_string_int_int___Opt : DynamicCustomFunction
        {
            private delegate string InternalDelegate(string arg0, int arg1, int arg2);

            private readonly InternalDelegate _delegate;

            public Functionstring_string_int_int___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (int)arguments[1];
                var arg2 = (int)arguments[2];

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, int, string = ...)
        /// </summary>
        private partial class Functionstring_string_int_string___Opt : DynamicCustomFunction
        {
            private delegate string InternalDelegate(string arg0, int arg1, string arg2);

            private readonly InternalDelegate _delegate;

            public Functionstring_string_int_string___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (int)arguments[1];
                var arg2 = (string)arguments[2];

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, string)
        /// </summary>
        private partial class Functionstring_string_string : DynamicCustomFunction
        {
            private delegate string InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public Functionstring_string_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (string)arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, string, string)
        /// </summary>
        private partial class Functionstring_string_string_string : DynamicCustomFunction
        {
            private delegate string InternalDelegate(string arg0, string arg1, string arg2);

            private readonly InternalDelegate _delegate;

            public Functionstring_string_string_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (string)arguments[1];
                var arg2 = (string)arguments[2];

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, SourceSpan, IEnumerable, string)
        /// </summary>
        private partial class Functionstring_TemplateContext_SourceSpan_IEnumerable_string : DynamicCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, SourceSpan arg1, IEnumerable arg2, string arg3);

            private readonly InternalDelegate _delegate;

            public Functionstring_TemplateContext_SourceSpan_IEnumerable_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (IEnumerable)arguments[0];
                var arg1 = (string)arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, SourceSpan, object, string, string = ...)
        /// </summary>
        private partial class Functionstring_TemplateContext_SourceSpan_object_string_string___Opt : DynamicCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, string arg3, string arg4);

            private readonly InternalDelegate _delegate;

            public Functionstring_TemplateContext_SourceSpan_object_string_string___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (object)arguments[0];
                var arg1 = (string)arguments[1];
                var arg2 = (string)arguments[2];

                return _delegate(context, callerContext.Span, arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, string)
        /// </summary>
        private partial class Functionstring_TemplateContext_string : DynamicCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public Functionstring_TemplateContext_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];

                return _delegate(context, arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, string, string, string, string = ...)
        /// </summary>
        private partial class Functionstring_TemplateContext_string_string_string_string___Opt : DynamicCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, string arg1, string arg2, string arg3, string arg4);

            private readonly InternalDelegate _delegate;

            public Functionstring_TemplateContext_string_string_string_string___Opt(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];
                var arg1 = (string)arguments[1];
                var arg2 = (string)arguments[2];
                var arg3 = (string)arguments[3];

                return _delegate(context, arg0, arg1, arg2, arg3);
            }
        }

        /// <summary>
        /// Optimized custom function for: TimeSpan (double)
        /// </summary>
        private partial class FunctionTimeSpan_double : DynamicCustomFunction
        {
            private delegate TimeSpan InternalDelegate(double arg0);

            private readonly InternalDelegate _delegate;

            public FunctionTimeSpan_double(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (double)arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: TimeSpan (string)
        /// </summary>
        private partial class FunctionTimeSpan_string : DynamicCustomFunction
        {
            private delegate TimeSpan InternalDelegate(string arg0);

            private readonly InternalDelegate _delegate;

            public FunctionTimeSpan_string(MethodInfo method) : base(method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public override object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var arg0 = (string)arguments[0];

                return _delegate(arg0);
            }
        }

    }
}

