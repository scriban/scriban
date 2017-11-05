// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// The object representing the global 'date'  object
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
    public class DateTimeFunctions : ScriptObject, IScriptCustomFunction
    {
        private const string FormatKey = "format";

        // This is exposed as well as default_format
        public const string DefaultFormat = "%d %b %Y";

        [ScriptMemberIgnore]
        public static readonly ScriptVariable DateVariable = new ScriptVariableGlobal("date");

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
        /// Initializes a new instance of the <see cref="DateTimeFunctions"/> class.
        /// </summary>
        public DateTimeFunctions()
        {
            Format = DefaultFormat;

            this.Import("now", new Func<DateTime>(() => DateTime.Now));

            this.Import("to_string", new Func<TemplateContext, DateTime, string, string>((context, date, pattern) => ToString(date, pattern, context.CurrentCulture)));

            this.Import("parse", new Func<string, DateTime>(Parse));
        }

        /// <summary>
        /// Gets or sets the format used to format all dates
        /// </summary>
        public string Format
        {
            get { return this.GetSafeValue<string>(FormatKey) ?? DefaultFormat; }
            set
            {
                this.SetValue(FormatKey, value, false);
            }
        }

        /// <summary>
        /// Adds days to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="days">The days.</param>
        /// <returns>A new date</returns>
        public static DateTime AddDays(DateTime date, double days)
        {
            return date.AddDays(days);
        }

        /// <summary>
        /// Adds years to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="years">The years.</param>
        /// <returns>A new date</returns>
        public static DateTime AddYears(DateTime date, int years)
        {
            return date.AddYears(years);
        }

        /// <summary>
        /// Adds months to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="months">The months.</param>
        /// <returns>A new date</returns>
        public static DateTime AddMonths(DateTime date, int months)
        {
            return date.AddMonths(months);
        }

        /// <summary>
        /// Adds hours to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="hours">The hours.</param>
        /// <returns>A new date</returns>
        public static DateTime AddHours(DateTime date, double hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// Adds minutes to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="minutes">The minutes.</param>
        /// <returns>A new date</returns>
        public static DateTime AddMinutes(DateTime date, double minutes)
        {
            return date.AddMinutes(minutes);
        }

        /// <summary>
        /// Adds seconds to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="seconds">The seconds.</param>
        /// <returns>A new date</returns>
        public static DateTime AddSeconds(DateTime date, double seconds)
        {
            return date.AddSeconds(seconds);
        }

        /// <summary>
        /// Adds millis to date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="millis">The millis.</param>
        /// <returns>A new date</returns>
        public static DateTime AddMilliseconds(DateTime date, double millis)
        {
            return date.AddMilliseconds(millis);
        }

        /// <summary>
        /// Parses the specified text as a <see cref="ScriptDate"/> using the current culture.
        /// </summary>
        /// <param name="text">A text representing a date.</param>
        /// <returns>A date object</returns>
        internal static DateTime Parse(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new DateTime();
            }
            DateTime result;
            if (DateTime.TryParse(text, out result))
            {
                return result;
            }
            return new DateTime();
        }

        public override IScriptObject Clone(bool deep)
        {
            var dateFunctions = (DateTimeFunctions)base.Clone(deep);
            dateFunctions.Import("to_string", new Func<TemplateContext, DateTime, string, string>((context, date, pattern) => dateFunctions.ToString(date, pattern, context.CurrentCulture)));
            return dateFunctions;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="pattern">The date format pattern.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        [ScriptMemberIgnore]
        public virtual string ToString(DateTime datetime, string pattern, CultureInfo culture)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            // If pattern is %g only, use the default date
            if (pattern == "%g")
            {
                pattern = "%g " + Format;
            }

            var builder = new StringBuilder();

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
                        culture = CultureInfo.InvariantCulture;
                        continue;
                    }

                    if (Formats.TryGetValue(format, out formatter))
                    {
                        builder.Append(formatter.Invoke(datetime, culture));
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

        public object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            // If we access `date` without any parameter, it calls by default the "parse" function
            // otherwise it is the 'date' object itself
            switch (arguments.Count)
            {
                case 0:
                    return this;
                case 1:
                    return Parse(context.ToString(callerContext.Span, arguments[0]));
                default:
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of parameters `{arguments.Count}` for `date` object/function.");
            }
        }
    }
}