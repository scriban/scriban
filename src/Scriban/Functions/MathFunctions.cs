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
        public static object DividedBy(TemplateContext context, SourceSpan span, double value, object by)
        {
            var result = ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Divide, value, by);

            // If the divisor is an integer, return a an integer
            if (by is int)
            {
                if (result is double)
                {
                    return (int)Math.Floor((double)result);
                }
                if (result is float)
                {
                    return (int)Math.Floor((float)result);
                }
            }
            return result;
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

        public static double Round(double value, int precision = 0)
        {
            return Math.Round(value, precision);
        }

        public static object Minus(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Substract, value, with);
        }

        public static object Plus(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Add, value, with);
        }

        public static object Modulo(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Modulus, value, with);
        }

        public static object Times(TemplateContext context, SourceSpan span, object value, object with)
        {
            return ScriptBinaryExpression.Evaluate(context, span, ScriptBinaryOperator.Multiply, value, with);
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
   }
}