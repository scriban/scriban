// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Globalization;
using System.Reflection;
using Scriban.Helpers;
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
        /// <summary>
        /// Returns the absolute value of a specified number.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="value">The input value</param>
        /// <returns>The absolute value of the input value</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ -15.5| math.abs }}
        /// {{ -5| math.abs }}
        /// ```
        /// ```html
        /// -15.5
        /// -5
        /// ```
        /// </remarks>
        public static object Abs(TemplateContext context, SourceSpan span, object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is int)
            {
                return Math.Abs((int) value);
            }
            if (value is float)
            {
                return Math.Abs((float)value);
            }
            if (value is double)
            {
                return Math.Abs((double)value);
            }
            if (value is sbyte)
            {
                return Math.Abs((sbyte)value);
            }
            if (value is short)
            {
                return Math.Abs((short)value);
            }
            if (value is long)
            {
                return Math.Abs((long)value);
            }

            // If it is a primitive it is already unsigned
            if (value.GetType().GetTypeInfo().IsPrimitive)
            {
                return value;
            }

            // otherwise we don't have a number, throw an exception just in case
            throw new ScriptRuntimeException(span, $"The value `{value}` is not a number");
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to the specified number.
        /// </summary>
        /// <param name="value">The input value</param>
        /// <returns>The smallest integer greater than or equal to the specified number.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ 4.6 | math.ceil }} 
        /// {{ 4.3 | math.ceil }} 
        /// ```
        /// ```html
        /// 5
        /// 5
        /// ```
        /// </remarks>
        public static double Ceil(double value)
        {
            return Math.Ceiling(value);
        }

        public static object DividedBy(TemplateContext context, SourceSpan span, double value, object by)
        {
            var result = ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Divide, value, by);

            // If the divisor is an integer, return a an integer
            if (by is int)
            {
                if (result is double)
                {
                    return (int) Math.Floor((double) result);
                }
                if (result is float)
                {
                    return (int) Math.Floor((float) result);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        /// <param name="value">The input value</param>
        /// <returns>The largest integer less than or equal to the specified number.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ 4.6 | math.ceil }} 
        /// {{ 4.3 | math.ceil }} 
        /// ```
        /// ```html
        /// 4
        /// 4
        /// ```
        /// </remarks>
        public static double Floor(double value)
        {
            return Math.Floor(value);
        }

        public static string Format(TemplateContext context, SourceSpan span, object value, string format)
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
            return formattable.ToString(format, context.CurrentCulture);
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

        public static object Minus(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Substract, value, with);
        }

        public static object Modulo(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Modulus, value, with);
        }

        public static object Plus(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Add, value, with);
        }

        /// <summary>
        /// Rounds a value to the nearest integer or to the specified number of fractional digits.
        /// </summary>
        /// <param name="value">The input value</param>
        /// <param name="precision">The number of fractional digits in the return value. Default is 0.</param>
        /// <returns>A value rounded to the nearest integer or to the specified number of fractional digits.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ 4.6 | math.round }} 
        /// {{ 4.3 | math.round }}
        /// {{ 4.5612 | math.round 2 }}
        /// ```
        /// ```html
        /// 5
        /// 4
        /// 4.56
        /// ```
        /// </remarks>
        public static double Round(double value, int precision = 0)
        {
            return Math.Round(value, precision);
        }

        public static object Times(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Multiply, value, with);
        }
    }
}