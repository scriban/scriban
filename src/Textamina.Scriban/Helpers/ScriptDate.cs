// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Textamina.Scriban.Runtime;

namespace Textamina.Scriban.Helpers
{
    /// <summary>
    /// Simplified datetime object used for scripting, accessible through the "date" variable (e.g {{date.now}})
    /// </summary>
    /// <seealso cref="IScriptCustomType" />
    public struct ScriptDate : IScriptCustomType, IComparable
    {
        private DateTime value;

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
        /// <param name="date">The date.</param>
        public ScriptDate(DateTime date)
        {
            this.value = date;
        }

        /// <summary>
        /// Gets the year.
        /// </summary>
        public int Year => value.Year;

        /// <summary>
        /// Gets the month.
        /// </summary>
        public int Month => value.Month;

        /// <summary>
        /// Gets the day.
        /// </summary>
        public int Day => value.Day;

        /// <summary>
        /// Gets the day of year.
        /// </summary>
        public int DayOfYear => value.DayOfYear;

        /// <summary>
        /// Gets the hour.
        /// </summary>
        public int Hour => value.Hour;

        /// <summary>
        /// Gets the minute.
        /// </summary>
        public int Minute => value.Minute;

        /// <summary>
        /// Gets the second.
        /// </summary>
        public int Second => value.Second;

        /// <summary>
        /// Gets the millisecond.
        /// </summary>
        public int Millisecond => value.Millisecond;

        /// <summary>
        /// Gets the current date.
        /// </summary>
        public static ScriptDate Now => DateTime.Now;

        /// <summary>
        /// Adds days to date.
        /// </summary>
        /// <param name="days">The days.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddDays(double days, ScriptDate date)
        {
            return date.value.AddDays(days);
        }

        /// <summary>
        /// Adds years to date.
        /// </summary>
        /// <param name="years">The years.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddYears(int years, ScriptDate date)
        {
            return date.value.AddYears(years);
        }

        /// <summary>
        /// Adds months to date.
        /// </summary>
        /// <param name="months">The months.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddMonths(int months, ScriptDate date)
        {
            return date.value.AddMonths(months);
        }

        /// <summary>
        /// Adds hours to date.
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddHours(double hours, ScriptDate date)
        {
            return date.value.AddHours(hours);
        }

        /// <summary>
        /// Adds minutes to date.
        /// </summary>
        /// <param name="minutes">The minutes.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddMinutes(double minutes, ScriptDate date)
        {
            return date.value.AddMinutes(minutes);
        }

        /// <summary>
        /// Adds seconds to date.
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddSeconds(double seconds, ScriptDate date)
        {
            return date.value.AddSeconds(seconds);
        }

        /// <summary>
        /// Adds millis to date.
        /// </summary>
        /// <param name="millis">The millis.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddMilliseconds(double millis, ScriptDate date)
        {
            return date.value.AddMilliseconds(millis);
        }

        /// <summary>
        /// Parses the specified text as a <see cref="ScriptDate"/> using the current culture.
        /// </summary>
        /// <param name="text">A text representing a date.</param>
        /// <returns>A date object</returns>
        public static ScriptDate Parse(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ScriptDate();
            }
            DateTime result;
            if (DateTime.TryParse(text, out result))
            {
                return new ScriptDate(result);
            }
            return result;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="pattern">The date format pattern.</param>
        /// <param name="date">The date to format.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public static string ToString(string pattern, ScriptDate date)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return date.ToString();
            }

            // If pattern is %g only, use the default date
            if (pattern == "%g")
            {
                return ToString("%g %d %b %Y", date);
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
                        builder.Append(formatter.Invoke(date, defaultCulture));
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
                    return new ScriptTimeSpan(left.value - right.value);
                case ScriptBinaryOperator.CompareEqual:
                    return left.value == right.value;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left.value != right.value;
                case ScriptBinaryOperator.CompareLess:
                    return left.value < right.value;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left.value <= right.value;
                case ScriptBinaryOperator.CompareGreater:
                    return left.value > right.value;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left.value >= right.value;
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for timespan");
        }

        private object EvaluateBinaryExpression(ScriptBinaryExpression expression, ScriptDate left,
            ScriptTimeSpan right)
        {
            switch (expression.Operator)
            {
                case ScriptBinaryOperator.Add:
                    return new ScriptDate((DateTime)left + right);
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for between <date> and <timespan>");
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="DateTime"/> to <see cref="ScriptDate"/>.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [ScriptMemberIgnore]
        public static implicit operator ScriptDate(DateTime date)
        {
            return new ScriptDate(date);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ScriptDate"/> to <see cref="DateTime"/>.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        [ScriptMemberIgnore]
        public static implicit operator DateTime(ScriptDate date)
        {
            return date.value;
        }

        [ScriptMemberIgnore]
        public static void Register(ScriptObject builtins)
        {
            if (builtins == null) throw new ArgumentNullException(nameof(builtins));
            builtins.SetValue("date", ScriptObject.From(typeof(ScriptDate)), true);
        }

        public override string ToString()
        {
            return ToString("%d %b %Y", this);
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

            return value.CompareTo(scriptDate.value);
        }
    }
}