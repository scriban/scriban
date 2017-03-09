// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Simplified datetime object used for scripting, accessible through the "date" variable (e.g {{date.now}})
    /// </summary>
    /// <seealso cref="IScriptCustomType" />
    public class ScriptDate : ScriptObject, IScriptCustomType, IComparable
    {
        /// <summary>
        /// The global `date` object has to be accessible from any ScriptDate in order to reuse date.format
        /// </summary>
        public readonly ScriptDateFunctions Global;

        // Code from DotLiquid https://github.com/dotliquid/dotliquid/blob/master/src/DotLiquid/Util/StrFTime.cs
        // Apache License, Version 2.0
        private static readonly Dictionary<char, Func<DateTime, CultureInfo, string>> Formats = new Dictionary<char, Func<DateTime, CultureInfo, string>>
        {
            { 'a', (dateTime, cultureInfo) => dateTime.ToString("ddd", cultureInfo) },
            { 'A', (dateTime, cultureInfo) => dateTime.ToString("dddd", cultureInfo) },
            { 'b', (dateTime, cultureInfo) => dateTime.ToString("MMM", cultureInfo) },
            { 'B', (dateTime, cultureInfo) => dateTime.ToString("MMMM", cultureInfo) },
            { 'c', (dateTime, cultureInfo) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", cultureInfo) },
            { 'd', (dateTime, cultureInfo) => dateTime.ToString("dd", cultureInfo) },
            { 'e', (dateTime, cultureInfo) => dateTime.ToString("%d", cultureInfo).PadLeft(2, ' ') },
            { 'H', (dateTime, cultureInfo) => dateTime.ToString("HH", cultureInfo) },
            { 'I', (dateTime, cultureInfo) => dateTime.ToString("hh", cultureInfo) },
            { 'j', (dateTime, cultureInfo) => dateTime.DayOfYear.ToString().PadLeft(3, '0') },
            { 'm', (dateTime, cultureInfo) => dateTime.ToString("MM", cultureInfo) },
            { 'M', (dateTime, cultureInfo) => dateTime.Minute.ToString().PadLeft(2, '0') },
            { 'p', (dateTime, cultureInfo) => dateTime.ToString("tt", cultureInfo) },
            { 'S', (dateTime, cultureInfo) => dateTime.ToString("ss", cultureInfo) },
            { 'U', (dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString().PadLeft(2, '0') },
            { 'W', (dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString().PadLeft(2, '0') },
            { 'w', (dateTime, cultureInfo) => ((int) dateTime.DayOfWeek).ToString() },
            { 'x', (dateTime, cultureInfo) => dateTime.ToString("d", cultureInfo) },
            { 'X', (dateTime, cultureInfo) => dateTime.ToString("T", cultureInfo) },
            { 'y', (dateTime, cultureInfo) => dateTime.ToString("yy", cultureInfo) },
            { 'Y', (dateTime, cultureInfo) => dateTime.ToString("yyyy", cultureInfo) },
            { 'Z', (dateTime, cultureInfo) => dateTime.ToString("zzz", cultureInfo) },
            { '%', (dateTime, cultureInfo) => "%" }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptDate"/> struct.
        /// </summary>
        /// <param name="global">The global date object (used to get the default_format)</param>
        /// <param name="date">The date.</param>
        internal ScriptDate(ScriptDateFunctions global, DateTime date)
        {
            if (global == null) throw new ArgumentNullException(nameof(global));
            Global = global;
            this.Value = date;
            this.SetValue("year", Value.Year, true);
            this.SetValue("month", Value.Month, true);
            this.SetValue("day", Value.Day, true);
            this.SetValue("day_of_year", Value.DayOfYear, true);
            this.SetValue("hour", Value.Hour, true);
            this.SetValue("minute", Value.Minute, true);
            this.SetValue("second", Value.Second, true);
            this.SetValue("millisecond", Value.Millisecond, true);
        }

        public DateTime Value { get; }

        public override string ToString()
        {
            return ToString(Global.Format);
        }

        public override string ToString(SourceSpan span)
        {
            return ToString();
        }

        bool IScriptCustomType.TryConvertTo(Type destinationType, out object outValue)
        {
            outValue = null;
            if (destinationType == typeof(bool))
            {
                outValue = true;
                return true;
            }
            return false;
        }

        object IScriptCustomType.EvaluateUnaryExpression(ScriptUnaryExpression expression)
        {
            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for date");
        }

        object IScriptCustomType.EvaluateBinaryExpression(ScriptBinaryExpression expression, object left, object right)
        {
            if (left is ScriptDate && right is ScriptDate)
            {
                return EvaluateBinaryExpression(expression, (ScriptDate)left, (ScriptDate)right);
            }

            if (left is ScriptDate && right is ScriptTimeSpan)
            {
                return EvaluateBinaryExpression(expression, (ScriptDate)left, (ScriptTimeSpan)right);
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for between [{left?.GetType()}] and [{right?.GetType()}]");
        }

        private object EvaluateBinaryExpression(ScriptBinaryExpression expression, ScriptDate left,
            ScriptDate right)
        {
            switch (expression.Operator)
            {
                case ScriptBinaryOperator.Substract:
                    return new ScriptTimeSpan(left.Value - right.Value);
                case ScriptBinaryOperator.CompareEqual:
                    return left.Value == right.Value;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left.Value != right.Value;
                case ScriptBinaryOperator.CompareLess:
                    return left.Value < right.Value;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left.Value <= right.Value;
                case ScriptBinaryOperator.CompareGreater:
                    return left.Value > right.Value;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left.Value >= right.Value;
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for timespan");
        }

        private object EvaluateBinaryExpression(ScriptBinaryExpression expression, ScriptDate left,
            ScriptTimeSpan right)
        {
            switch (expression.Operator)
            {
                case ScriptBinaryOperator.Add:
                    return new ScriptDate(left.Global, left.Value + right);
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for between <date> and <timespan>");
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (!(obj is ScriptDate))
            {
                throw new ArgumentException($"Object [{obj.GetType()}] cannot be compare to a date object");
            }
            var scriptDate = (ScriptDate)obj;

            return Value.CompareTo(scriptDate.Value);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="pattern">The date format pattern.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            // If pattern is %g only, use the default date
            if (pattern == "%g")
            {
                pattern = "%g " + Global.Format;
            }

            var builder = new StringBuilder();

            var defaultCulture = CultureInfo.CurrentCulture;

            for (int i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                if (c == '%' && (i + 1) < pattern.Length)
                {
                    i++;
                    Func<DateTime, CultureInfo, string> formatter;
                    var format = pattern[i];

                    // Switch to invariant culture
                    if (format == 'g')
                    {
                        defaultCulture = CultureInfo.InvariantCulture;
                        continue;
                    }

                    if (Formats.TryGetValue(format, out formatter))
                    {
                        builder.Append(formatter.Invoke(Value, defaultCulture));
                    }
                    else
                    {
                        builder.Append('%');
                        builder.Append(format);
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();

        }
    }
}
