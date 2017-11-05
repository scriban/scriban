// ----------------------------------------------------------------------------------
// This file was automatically generated - 06-Nov-17 11:41:37 by Scriban.CodeGen
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
    public static partial class CustomFunction
    {


        static CustomFunction()
        {
            BuiltinFunctions.Add(typeof(Scriban.Functions.MathFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.MathFunctions.IsNumber)), method => new Functionboolobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Contains)), method => new Functionboolstringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.DateTimeFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.DateTimeFunctions.AddDays)), method => new FunctionDateTimeDateTimedouble(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.DateTimeFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.DateTimeFunctions.AddYears)), method => new FunctionDateTimeDateTimeint(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.MathFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.MathFunctions.Abs)), method => new Functiondoubledouble(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Uniq)), method => new FunctionIEnumerableIEnumerable(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Split)), method => new FunctionIEnumerablestringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Sort)), method => new FunctionIEnumerableTemplateContextSourceSpanobjectstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.AddRange)), method => new FunctionIListIListIEnumerable(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.RemoveAt)), method => new FunctionIListIListint(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.InsertAt)), method => new FunctionIListIListintobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Add)), method => new FunctionIListIListobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Size)), method => new FunctionintIEnumerable(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Size)), method => new Functionintstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ObjectFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ObjectFunctions.Size)), method => new FunctionintTemplateContextSourceSpanobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.MathFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.MathFunctions.Round)), method => new Functionobjectdoubleint(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.First)), method => new FunctionobjectIEnumerable(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Concat)), method => new FunctionobjectIEnumerableIEnumerable(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ObjectFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ObjectFunctions.Default)), method => new Functionobjectobjectobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.MathFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.MathFunctions.DividedBy)), method => new FunctionobjectTemplateContextSourceSpandoubleobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.MathFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.MathFunctions.Minus)), method => new FunctionobjectTemplateContextSourceSpanobjectobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Compact)), method => new FunctionScriptArrayIEnumerable(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Limit)), method => new FunctionScriptArrayIEnumerableint(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.RegexFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.RegexFunctions.Split)), method => new FunctionScriptArraystringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Pluralize)), method => new Functionstringintstringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ObjectFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ObjectFunctions.Typeof)), method => new Functionstringobject(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.HtmlFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.HtmlFunctions.Escape)), method => new Functionstringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Truncatewords)), method => new Functionstringstringint(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Slice)), method => new Functionstringstringintint(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Truncate)), method => new Functionstringstringintstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.StringFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.StringFunctions.Append)), method => new Functionstringstringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.RegexFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.RegexFunctions.Replace)), method => new Functionstringstringstringstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.ArrayFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.ArrayFunctions.Join)), method => new FunctionstringTemplateContextSourceSpanIEnumerablestring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.MathFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.MathFunctions.Format)), method => new FunctionstringTemplateContextSourceSpanobjectstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.HtmlFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.HtmlFunctions.Strip)), method => new FunctionstringTemplateContextstring(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.TimeSpanFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.TimeSpanFunctions.FromDays)), method => new FunctionTimeSpandouble(method));
            BuiltinFunctions.Add(typeof(Scriban.Functions.TimeSpanFunctions).GetTypeInfo().GetDeclaredMethod(nameof(Scriban.Functions.TimeSpanFunctions.Parse)), method => new FunctionTimeSpanstring(method));

        }

        /// <summary>
        /// Optimized custom function for: bool (object)
        /// </summary>
        private class Functionboolobject : IScriptCustomFunction
        {
            private delegate bool InternalDelegate(object arg0);

            private readonly InternalDelegate _delegate;

            public Functionboolobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: bool (string, string)
        /// </summary>
        private class Functionboolstringstring : IScriptCustomFunction
        {
            private delegate bool InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public Functionboolstringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: DateTime (DateTime, double)
        /// </summary>
        private class FunctionDateTimeDateTimedouble : IScriptCustomFunction
        {
            private delegate DateTime InternalDelegate(DateTime arg0, double arg1);

            private readonly InternalDelegate _delegate;

            public FunctionDateTimeDateTimedouble(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (DateTime)context.ToObject(callerContext.Span, arguments[0], typeof(DateTime));
                var arg1 = (double)context.ToObject(callerContext.Span, arguments[1], typeof(double));

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: DateTime (DateTime, int)
        /// </summary>
        private class FunctionDateTimeDateTimeint : IScriptCustomFunction
        {
            private delegate DateTime InternalDelegate(DateTime arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public FunctionDateTimeDateTimeint(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (DateTime)context.ToObject(callerContext.Span, arguments[0], typeof(DateTime));
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: double (double)
        /// </summary>
        private class Functiondoubledouble : IScriptCustomFunction
        {
            private delegate double InternalDelegate(double arg0);

            private readonly InternalDelegate _delegate;

            public Functiondoubledouble(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = (double)context.ToObject(callerContext.Span, arguments[0], typeof(double));

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (IEnumerable)
        /// </summary>
        private class FunctionIEnumerableIEnumerable : IScriptCustomFunction
        {
            private delegate IEnumerable InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerableIEnumerable(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (string, string)
        /// </summary>
        private class FunctionIEnumerablestringstring : IScriptCustomFunction
        {
            private delegate IEnumerable InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIEnumerablestringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IEnumerable (TemplateContext, SourceSpan, object, string = ...)
        /// </summary>
        private class FunctionIEnumerableTemplateContextSourceSpanobjectstring : IScriptCustomFunction
        {
            private delegate IEnumerable InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, string arg3);

            private readonly InternalDelegate _delegate;
            private readonly string defaultArg1;

            public FunctionIEnumerableTemplateContextSourceSpanobjectstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
                defaultArg1 = (string)method.GetParameters()[3].DefaultValue;
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count < 1 || arguments.Count > 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `1` arguments");
                }
                var arg0 = arguments[0];
                var arg1 =  arguments.Count >= 2 ? context.ToString(callerContext.Span, arguments[1]) : defaultArg1;

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IList (IList, IEnumerable)
        /// </summary>
        private class FunctionIListIListIEnumerable : IScriptCustomFunction
        {
            private delegate IList InternalDelegate(IList arg0, IEnumerable arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIListIListIEnumerable(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToList(callerContext.Span, arguments[0]);
                var arg1 = (IEnumerable)context.ToObject(callerContext.Span, arguments[1], typeof(IEnumerable));

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IList (IList, int)
        /// </summary>
        private class FunctionIListIListint : IScriptCustomFunction
        {
            private delegate IList InternalDelegate(IList arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIListIListint(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToList(callerContext.Span, arguments[0]);
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: IList (IList, int, object)
        /// </summary>
        private class FunctionIListIListintobject : IScriptCustomFunction
        {
            private delegate IList InternalDelegate(IList arg0, int arg1, object arg2);

            private readonly InternalDelegate _delegate;

            public FunctionIListIListintobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 3)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `3` arguments");
                }
                var arg0 = context.ToList(callerContext.Span, arguments[0]);
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);
                var arg2 = arguments[2];

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: IList (IList, object)
        /// </summary>
        private class FunctionIListIListobject : IScriptCustomFunction
        {
            private delegate IList InternalDelegate(IList arg0, object arg1);

            private readonly InternalDelegate _delegate;

            public FunctionIListIListobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToList(callerContext.Span, arguments[0]);
                var arg1 = arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: int (IEnumerable)
        /// </summary>
        private class FunctionintIEnumerable : IScriptCustomFunction
        {
            private delegate int InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public FunctionintIEnumerable(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: int (string)
        /// </summary>
        private class Functionintstring : IScriptCustomFunction
        {
            private delegate int InternalDelegate(string arg0);

            private readonly InternalDelegate _delegate;

            public Functionintstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: int (TemplateContext, SourceSpan, object)
        /// </summary>
        private class FunctionintTemplateContextSourceSpanobject : IScriptCustomFunction
        {
            private delegate int InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2);

            private readonly InternalDelegate _delegate;

            public FunctionintTemplateContextSourceSpanobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = arguments[0];

                return _delegate(context, callerContext.Span, arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (double, int)
        /// </summary>
        private class Functionobjectdoubleint : IScriptCustomFunction
        {
            private delegate object InternalDelegate(double arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public Functionobjectdoubleint(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (double)context.ToObject(callerContext.Span, arguments[0], typeof(double));
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (IEnumerable)
        /// </summary>
        private class FunctionobjectIEnumerable : IScriptCustomFunction
        {
            private delegate object InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public FunctionobjectIEnumerable(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (IEnumerable, IEnumerable)
        /// </summary>
        private class FunctionobjectIEnumerableIEnumerable : IScriptCustomFunction
        {
            private delegate object InternalDelegate(IEnumerable arg0, IEnumerable arg1);

            private readonly InternalDelegate _delegate;

            public FunctionobjectIEnumerableIEnumerable(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));
                var arg1 = (IEnumerable)context.ToObject(callerContext.Span, arguments[1], typeof(IEnumerable));

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (object, object)
        /// </summary>
        private class Functionobjectobjectobject : IScriptCustomFunction
        {
            private delegate object InternalDelegate(object arg0, object arg1);

            private readonly InternalDelegate _delegate;

            public Functionobjectobjectobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = arguments[0];
                var arg1 = arguments[1];

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, SourceSpan, double, object)
        /// </summary>
        private class FunctionobjectTemplateContextSourceSpandoubleobject : IScriptCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, SourceSpan arg1, double arg2, object arg3);

            private readonly InternalDelegate _delegate;

            public FunctionobjectTemplateContextSourceSpandoubleobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (double)context.ToObject(callerContext.Span, arguments[0], typeof(double));
                var arg1 = arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: object (TemplateContext, SourceSpan, object, object)
        /// </summary>
        private class FunctionobjectTemplateContextSourceSpanobjectobject : IScriptCustomFunction
        {
            private delegate object InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, object arg3);

            private readonly InternalDelegate _delegate;

            public FunctionobjectTemplateContextSourceSpanobjectobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = arguments[0];
                var arg1 = arguments[1];

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: ScriptArray (IEnumerable)
        /// </summary>
        private class FunctionScriptArrayIEnumerable : IScriptCustomFunction
        {
            private delegate ScriptArray InternalDelegate(IEnumerable arg0);

            private readonly InternalDelegate _delegate;

            public FunctionScriptArrayIEnumerable(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: ScriptArray (IEnumerable, int)
        /// </summary>
        private class FunctionScriptArrayIEnumerableint : IScriptCustomFunction
        {
            private delegate ScriptArray InternalDelegate(IEnumerable arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public FunctionScriptArrayIEnumerableint(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: ScriptArray (string, string)
        /// </summary>
        private class FunctionScriptArraystringstring : IScriptCustomFunction
        {
            private delegate ScriptArray InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public FunctionScriptArraystringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (int, string, string)
        /// </summary>
        private class Functionstringintstringstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(int arg0, string arg1, string arg2);

            private readonly InternalDelegate _delegate;

            public Functionstringintstringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 3)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `3` arguments");
                }
                var arg0 = context.ToInt(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);
                var arg2 = context.ToString(callerContext.Span, arguments[2]);

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (object)
        /// </summary>
        private class Functionstringobject : IScriptCustomFunction
        {
            private delegate string InternalDelegate(object arg0);

            private readonly InternalDelegate _delegate;

            public Functionstringobject(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = arguments[0];

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string)
        /// </summary>
        private class Functionstringstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string arg0);

            private readonly InternalDelegate _delegate;

            public Functionstringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, int)
        /// </summary>
        private class Functionstringstringint : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string arg0, int arg1);

            private readonly InternalDelegate _delegate;

            public Functionstringstringint(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, int, int = ...)
        /// </summary>
        private class Functionstringstringintint : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string arg0, int arg1, int arg2);

            private readonly InternalDelegate _delegate;
            private readonly int defaultArg2;

            public Functionstringstringintint(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
                defaultArg2 = (int)method.GetParameters()[2].DefaultValue;
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count < 2 || arguments.Count > 3)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);
                var arg2 =  arguments.Count >= 3 ? context.ToInt(callerContext.Span, arguments[2]) : defaultArg2;

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, int, string = ...)
        /// </summary>
        private class Functionstringstringintstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string arg0, int arg1, string arg2);

            private readonly InternalDelegate _delegate;
            private readonly string defaultArg2;

            public Functionstringstringintstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
                defaultArg2 = (string)method.GetParameters()[2].DefaultValue;
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count < 2 || arguments.Count > 3)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting at least `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToInt(callerContext.Span, arguments[1]);
                var arg2 =  arguments.Count >= 3 ? context.ToString(callerContext.Span, arguments[2]) : defaultArg2;

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, string)
        /// </summary>
        private class Functionstringstringstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public Functionstringstringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (string, string, string)
        /// </summary>
        private class Functionstringstringstringstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(string arg0, string arg1, string arg2);

            private readonly InternalDelegate _delegate;

            public Functionstringstringstringstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 3)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `3` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);
                var arg1 = context.ToString(callerContext.Span, arguments[1]);
                var arg2 = context.ToString(callerContext.Span, arguments[2]);

                return _delegate(arg0, arg1, arg2);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, SourceSpan, IEnumerable, string)
        /// </summary>
        private class FunctionstringTemplateContextSourceSpanIEnumerablestring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, SourceSpan arg1, IEnumerable arg2, string arg3);

            private readonly InternalDelegate _delegate;

            public FunctionstringTemplateContextSourceSpanIEnumerablestring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = (IEnumerable)context.ToObject(callerContext.Span, arguments[0], typeof(IEnumerable));
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, SourceSpan, object, string)
        /// </summary>
        private class FunctionstringTemplateContextSourceSpanobjectstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, SourceSpan arg1, object arg2, string arg3);

            private readonly InternalDelegate _delegate;

            public FunctionstringTemplateContextSourceSpanobjectstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 2)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `2` arguments");
                }
                var arg0 = arguments[0];
                var arg1 = context.ToString(callerContext.Span, arguments[1]);

                return _delegate(context, callerContext.Span, arg0, arg1);
            }
        }

        /// <summary>
        /// Optimized custom function for: string (TemplateContext, string)
        /// </summary>
        private class FunctionstringTemplateContextstring : IScriptCustomFunction
        {
            private delegate string InternalDelegate(TemplateContext arg0, string arg1);

            private readonly InternalDelegate _delegate;

            public FunctionstringTemplateContextstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);

                return _delegate(context, arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: TimeSpan (double)
        /// </summary>
        private class FunctionTimeSpandouble : IScriptCustomFunction
        {
            private delegate TimeSpan InternalDelegate(double arg0);

            private readonly InternalDelegate _delegate;

            public FunctionTimeSpandouble(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = (double)context.ToObject(callerContext.Span, arguments[0], typeof(double));

                return _delegate(arg0);
            }
        }

        /// <summary>
        /// Optimized custom function for: TimeSpan (string)
        /// </summary>
        private class FunctionTimeSpanstring : IScriptCustomFunction
        {
            private delegate TimeSpan InternalDelegate(string arg0);

            private readonly InternalDelegate _delegate;

            public FunctionTimeSpanstring(MethodInfo method)
            {
                _delegate = (InternalDelegate)method.CreateDelegate(typeof(InternalDelegate));
            }

            public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                if (arguments.Count != 1)
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of arguments `{arguments.Count}` passed to `{callerContext}` while expecting `1` arguments");
                }
                var arg0 = context.ToString(callerContext.Span, arguments[0]);

                return _delegate(arg0);
            }
        }

    }
}

