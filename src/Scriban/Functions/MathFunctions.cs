// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Globalization;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// Math functions available through the object 'math' in scriban.
    /// </summary>
    public class MathFunctions : ScriptObject
    {
        public MathFunctions()
        {
            SetValue("round", new DelegateCustomFunction(Round), true);
        }

        public static double Abs(double value)
        {
            return Math.Abs(value);
        }

        public static double Ceil(double value)
        {
            return Math.Ceiling(value);
        }

        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        [ScriptMemberIgnore]
        public static object Round(int precision, double value)
        {
            return Math.Round(value, precision);
        }

        public static object Minus(TemplateContext context, SourceSpan span, object subValue, object input)
        {
            return ScriptBinaryExpression.Calculate(context, span, ScriptBinaryOperator.Substract, input, input?.GetType(), subValue, subValue?.GetType());
        }

        public static object Plus(TemplateContext context, SourceSpan span, object addValue, object input)
        {
            return ScriptBinaryExpression.Calculate(context, span, ScriptBinaryOperator.Add, input, input?.GetType(), addValue, addValue?.GetType());
        }

        public static object Modulo(TemplateContext context, SourceSpan span, object modValue, object input)
        {
            return ScriptBinaryExpression.Calculate(context, span, ScriptBinaryOperator.Modulus, input, input?.GetType(), modValue, modValue?.GetType());
        }

        public static object Times(TemplateContext context, SourceSpan span, object mulValue, object input)
        {
            return ScriptBinaryExpression.Calculate(context, span, ScriptBinaryOperator.Multiply, input, input?.GetType(), mulValue, mulValue?.GetType());
        }

        public static string Format(TemplateContext context, SourceSpan span, string format, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            format = format ?? string.Empty;
            var formattable = value as IFormattable;
            if (!IsNumber(value) || formattable == null)
            {
                throw new ScriptRuntimeException(span, $"Unexpected `{value}`. Must be a formattable number");
            }
            return formattable.ToString(format, NumberFormatInfo.CurrentInfo);
        }

        public static bool IsNumber(object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }

        private static object Round(TemplateContext context, ScriptNode callerContext, ScriptArray parameters)
        {
            if (parameters.Count < 1 || parameters.Count > 2)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected number of arguments [{parameters.Count}] for math.round. Expecting at least 1 parameter <precision>? <value>");
            }

            var value = context.ToDouble(callerContext.Span, parameters[parameters.Count - 1]);
            int precision = 0;
            if (parameters.Count == 2)
            {
                precision = context.ToInt(callerContext.Span, parameters[0]);
            }

            return Round(precision, value);
        }
    }
}