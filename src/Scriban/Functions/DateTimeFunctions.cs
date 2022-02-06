// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// A datetime object represents an instant in time, expressed as a date and time of day.
    ///
    /// | Name             | Description
    /// |--------------    |-----------------
    /// | `.year`          | Gets the year of a date object
    /// | `.month`         | Gets the month of a date object
    /// | `.day`           | Gets the day in the month of a date object
    /// | `.day_of_year`   | Gets the day within the year
    /// | `.hour`          | Gets the hour of the date object
    /// | `.minute`        | Gets the minute of the date object
    /// | `.second`        | Gets the second of the date object
    /// | `.millisecond`   | Gets the millisecond of the date object
    ///
    /// [:top:](#builtins)
    /// #### Binary operations
    ///
    /// The subtract operation `date1 - date2`: Subtract `date2` from `date1` and return a timespan internal object (see timespan object below).
    ///
    /// Other comparison operators(`==`, `!=`, `&lt;=`, `&gt;=`, `&lt;`, `&gt;`) are also working with date objects.
    ///
    /// A `timespan` and also the added to a `datetime` object.
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class DateTimeFunctions : ScriptObject, IScriptCustomFunction, IScriptFunctionInfo
    {
        private const string FormatKey = "format";

        // This is exposed as well as default_format
        public const string DefaultFormat = "%d %b %Y";

        [ScriptMemberIgnore]
        public static readonly ScriptVariable DateVariable = new ScriptVariableGlobal("date");

        // Code from DotLiquid https://github.com/dotliquid/dotliquid/blob/master/src/DotLiquid/Util/StrFTime.cs
        // Apache License, Version 2.0
        private static readonly Dictionary<char, (Func<DateTime, CultureInfo, string>, string)> Formats = new Dictionary<char, (Func<DateTime, CultureInfo, string>, string)>
        {
            { 'a', ((dateTime, cultureInfo) => dateTime.ToString("ddd", cultureInfo), "ddd") },
            { 'A', ((dateTime, cultureInfo) => dateTime.ToString("dddd", cultureInfo), "dddd") },
            { 'b', ((dateTime, cultureInfo) => dateTime.ToString("MMM", cultureInfo), "MMM") },
            { 'B', ((dateTime, cultureInfo) => dateTime.ToString("MMMM", cultureInfo), "MMM") },
            { 'c', ((dateTime, cultureInfo) => dateTime.ToString("ddd MMM dd HH:mm:ss yyyy", cultureInfo), "ddd MMM dd HH:mm:ss yyyy") },
            { 'C', ((dateTime, cultureInfo) => (dateTime.Year / 100).ToString("D2", cultureInfo), null) },
            { 'd', ((dateTime, cultureInfo) => dateTime.ToString("dd", cultureInfo), "dd") },
            { 'D', ((dateTime, cultureInfo) => dateTime.ToString("MM/dd/yy", cultureInfo), "MM/dd/yy") },
            { 'e', ((dateTime, cultureInfo) => dateTime.ToString("%d", cultureInfo).PadLeft(2, ' '), "%d") },
            { 'F', ((dateTime, cultureInfo) => dateTime.ToString("yyyy-MM-dd", cultureInfo), "yyyy-MM-dd") },
            { 'h', ((dateTime, cultureInfo) => dateTime.ToString("MMM", cultureInfo), "MMM") },
            { 'H', ((dateTime, cultureInfo) => dateTime.ToString("HH", cultureInfo), "HH") },
            { 'I', ((dateTime, cultureInfo) => dateTime.ToString("hh", cultureInfo), "hh") },
            { 'j', ((dateTime, cultureInfo) => dateTime.DayOfYear.ToString("D3", cultureInfo), null) },
            { 'k', ((dateTime, cultureInfo) => dateTime.ToString("%H", cultureInfo).PadLeft(2, ' '), "%H") },
            { 'l', ((dateTime, cultureInfo) => dateTime.ToString("%h", cultureInfo).PadLeft(2, ' '), "%h") },
            { 'L', ((dateTime, cultureInfo) => dateTime.ToString("fff", cultureInfo), "fff") },
            { 'm', ((dateTime, cultureInfo) => dateTime.ToString("MM", cultureInfo), "MM") },
            { 'M', ((dateTime, cultureInfo) => dateTime.ToString("mm", cultureInfo), "mm") },
            { 'n', ((dateTime, cultureInfo) => "\n", null) },
            { 'N', ((dateTime, cultureInfo) => dateTime.ToString("fffffff00", cultureInfo), "fffffff00") },
            { 'p', ((dateTime, cultureInfo) => dateTime.ToString("tt", cultureInfo), "tt") },
            { 'P', ((dateTime, cultureInfo) => dateTime.ToString("tt", cultureInfo).ToLowerInvariant(), "tt") },
            { 'r', ((dateTime, cultureInfo) => dateTime.ToString("hh:mm:ss tt", cultureInfo), "hh:mm:ss tt") },
            { 'R', ((dateTime, cultureInfo) => dateTime.ToString("HH:mm", cultureInfo), "HH:mm") },
            { 's', ((dateTime, cultureInfo) => ((dateTime.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) / TimeSpan.TicksPerSecond).ToString(cultureInfo), null) },
            { 'S', ((dateTime, cultureInfo) => dateTime.ToString("ss", cultureInfo), "ss") },
            { 't', ((dateTime, cultureInfo) => "\t", null) },
            { 'T', ((dateTime, cultureInfo) => dateTime.ToString("HH:mm:ss", cultureInfo), "HH:mm:ss") },
            { 'u', ((dateTime, cultureInfo) => ((dateTime.DayOfWeek == DayOfWeek.Sunday) ? 7 : (int) dateTime.DayOfWeek).ToString(cultureInfo), null) },
            { 'U', ((dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, DayOfWeek.Sunday).ToString("D2", cultureInfo), null) },
            { 'v', ((dateTime, cultureInfo) => string.Format(CultureInfo.InvariantCulture, "{0,2}-{1}-{2:D4}", dateTime.Day, dateTime.ToString("MMM", CultureInfo.InvariantCulture).ToUpper(), dateTime.Year), null) },
            { 'V', ((dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday).ToString("D2", cultureInfo), null) },
            { 'W', ((dateTime, cultureInfo) => cultureInfo.Calendar.GetWeekOfYear(dateTime, cultureInfo.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday).ToString("D2", cultureInfo), null) },
            { 'w', ((dateTime, cultureInfo) => ((int) dateTime.DayOfWeek).ToString(cultureInfo), null) },
            { 'x', ((dateTime, cultureInfo) => dateTime.ToString("d", cultureInfo), "d") },
            { 'X', ((dateTime, cultureInfo) => dateTime.ToString("T", cultureInfo), "T") },
            { 'y', ((dateTime, cultureInfo) => dateTime.ToString("yy", cultureInfo), "yy") },
            { 'Y', ((dateTime, cultureInfo) => dateTime.ToString("yyyy", cultureInfo), "yyyy") },
            { 'Z', ((dateTime, cultureInfo) => dateTime.ToString("zzz", cultureInfo), "zzz") },
            { '%', ((dateTime, cultureInfo) => "%", null) }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeFunctions"/> class.
        /// </summary>
        public DateTimeFunctions()
        {
            this["default_format"] = DefaultFormat;
            Format = DefaultFormat;
            CreateImportFunctions();
        }

        /// <summary>
        /// Gets or sets the format used to format all dates
        /// </summary>
        public string Format
        {
            get => GetSafeValue<string>(FormatKey) ?? DefaultFormat;
            set => SetValue(FormatKey, value, false);
        }

        /// <summary>
        /// Returns a datetime object of the current time, including the hour, minutes, seconds and milliseconds.
        /// </summary>
        /// <remarks>
        /// ```scriban-html
        /// {{ date.now.year }}
        /// ```
        /// ```html
        /// 2022
        /// ```
        /// </remarks>
        public static DateTime Now() => DateTime.Now;

        /// <summary>
        /// Adds the specified number of days to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="days">The days.</param>
        /// <returns>A new date</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ date.parse '2016/01/05' | date.add_days 1 }}
        /// ```
        /// ```html
        /// 06 Jan 2016
        /// ```
        /// </remarks>
        public static DateTime AddDays(DateTime date, double days)
        {
            return date.AddDays(days);
        }

        /// <summary>
        /// Adds the specified number of months to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="months">The months.</param>
        /// <returns>A new date</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ date.parse '2016/01/05' | date.add_months 1 }}
        /// ```
        /// ```html
        /// 05 Feb 2016
        /// ```
        /// </remarks>
        public static DateTime AddMonths(DateTime date, int months)
        {
            return date.AddMonths(months);
        }

        /// <summary>
        /// Adds the specified number of years to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="years">The years.</param>
        /// <returns>A new date</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ date.parse '2016/01/05' | date.add_years 1 }}
        /// ```
        /// ```html
        /// 05 Jan 2017
        /// ```
        /// </remarks>
        public static DateTime AddYears(DateTime date, int years)
        {
            return date.AddYears(years);
        }

        /// <summary>
        /// Adds the specified number of hours to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="hours">The hours.</param>
        /// <returns>A new date</returns>
        public static DateTime AddHours(DateTime date, double hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// Adds the specified number of minutes to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="minutes">The minutes.</param>
        /// <returns>A new date</returns>
        public static DateTime AddMinutes(DateTime date, double minutes)
        {
            return date.AddMinutes(minutes);
        }

        /// <summary>
        /// Adds the specified number of seconds to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="seconds">The seconds.</param>
        /// <returns>A new date</returns>
        public static DateTime AddSeconds(DateTime date, double seconds)
        {
            return date.AddSeconds(seconds);
        }

        /// <summary>
        /// Adds the specified number of milliseconds to the input date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="millis">The milliseconds.</param>
        /// <returns>A new date</returns>
        public static DateTime AddMilliseconds(DateTime date, double millis)
        {
            return date.AddMilliseconds(millis);
        }

        private static readonly Regex PlusFollowedByNumberRegex = new Regex(@"\+\d");

        /// <summary>
        /// Parses the specified input string to a date object.
        /// </summary>
        /// <param name="context">The template context.</param>
        /// <param name="text">A text representing a date.</param>
        /// <param name="pattern">The date format pattern. See `to_string` method about the format of a pattern.</param>
        /// <param name="culture">The culture used to format the datetime. Default is current culture.</param>
        /// <returns>A date object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ date.parse '2016/01/05' }}
        /// {{ date.parse '2018--06--17' '%Y--%m--%d' }}
        /// {{ date.parse '2021/11/30 20:50:23Z' }}
        /// {{ date.parse '20/01/2022 08:32:48 +00:00' culture:'en-GB' }}
        /// ```
        /// ```html
        /// 05 Jan 2016
        /// 17 Jun 2018
        /// 30 Nov 2021
        /// 20 Jan 2022
        /// ```
        /// </remarks>
        public static DateTime? Parse(TemplateContext context, string text, string pattern = null, string culture = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            bool hasOffset = PlusFollowedByNumberRegex.IsMatch(text);
            bool hasZ = text.TrimEnd().EndsWith("Z", StringComparison.OrdinalIgnoreCase);
            var dateTimeStyle = hasOffset || hasZ ? DateTimeStyles.None : DateTimeStyles.AssumeLocal;

            var currentCulture = (culture != null ? CultureInfo.GetCultureInfo(culture) : context.CurrentCulture) ?? context.CurrentCulture;
            string customFormat = null;
            if (pattern != null)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < pattern.Length; i++)
                {
                    var c = pattern[i];
                    if (c == '%' && (i + 1) < pattern.Length)
                    {
                        i++;
                        var format = pattern[i];

                        // Switch to invariant culture
                        if (format == 'g')
                        {
                            currentCulture = CultureInfo.InvariantCulture;
                            continue;
                        }

                        if (Formats.TryGetValue(format, out var formatterPair))
                        {
                            if (formatterPair.Item2 == null)
                            {
                                throw new ArgumentException($"The pattern %{format} is not supported for the parse method", nameof(pattern));
                            }
                            builder.Append(formatterPair.Item2);
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
                customFormat = builder.ToString();
            }

            var result = new DateTime();
            if (customFormat != null)
            {
                if (hasOffset || hasZ)
                {
                    if (DateTimeOffset.TryParseExact(text, customFormat, currentCulture, dateTimeStyle, out var dateTimeOffset))
                    {
                        result = dateTimeOffset.LocalDateTime;
                    }
                }
                else if (DateTime.TryParseExact(text, customFormat, currentCulture, dateTimeStyle, out result))
                {
                    return result;
                }
            }
            else
            {
                if (hasOffset || hasZ)
                {
                    if (DateTimeOffset.TryParse(text, currentCulture, dateTimeStyle, out var dateTimeOffset))
                    {
                        result = dateTimeOffset.LocalDateTime;
                    }
                }
                else
                {
                    DateTime.TryParse(text, currentCulture, dateTimeStyle, out result);
                }
            }

            return result;
        }

        public override IScriptObject Clone(bool deep)
        {
            var dateFunctions = (DateTimeFunctions)base.Clone(deep);
            // This is important to call the CreateImportFunctions as it is instance specific (using DefaultFormat from `date` object)
            dateFunctions.CreateImportFunctions();
            return dateFunctions;
        }

        /// <summary>
        /// Converts a datetime object to a textual representation using the specified format string.
        ///
        /// By default, if you are using a date, it will use the format specified by `date.format` which defaults to `date.default_format` (readonly) which default to `%d %b %Y`
        ///
        /// You can override the format used for formatting all dates by assigning the a new format: `date.format = '%a %b %e %T %Y';`
        ///
        /// You can recover the default format by using `date.format = date.default_format;`
        ///
        /// By default, the to_string format is using the **current culture**, but you can switch to an invariant culture by using the modifier `%g`
        ///
        /// For example, using `%g %d %b %Y` will output the date using an invariant culture.
        ///
        /// If you are using `%g` alone, it will output the date with `date.format` using an invariant culture.
        ///
        /// Suppose that `date.now` would return the date `2013-09-12 22:49:27 +0530`, the following table explains the format modifiers:
        ///
        /// | Format | Result            | Description
        /// |--------|-------------------|--------------------------------------------
        /// | `"%a"` |  `"Thu"`          | Name of week day in short form of the
        /// | `"%A"` |  `"Thursday"`     | Week day in full form of the time
        /// | `"%b"` |  `"Sep"`          | Month in short form of the time
        /// | `"%B"` |  `"September"`    | Month in full form of the time
        /// | `"%c"` |                   | Date and time (%a %b %e %T %Y)
        /// | `"%C"` |  `"20"`           | Century of the time
        /// | `"%d"` |  `"12"`           | Day of the month of the time
        /// | `"%D"` |  `"09/12/13"`     | Date (%m/%d/%y)
        /// | `"%e"` |  `"12"`           | Day of the month, blank-padded ( 1..31)
        /// | `"%F"` |  `"2013-09-12"`   | ISO 8601 date (%Y-%m-%d)
        /// | `"%h"` |  `"Sep"`          | Alias for %b
        /// | `"%H"` |  `"22"`           | Hour of the time in 24 hour clock format
        /// | `"%I"` |  `"10"`           | Hour of the time in 12 hour clock format
        /// | `"%j"` |  `"255"`          | Day of the year (001..366) (3 digits, left padded with zero)
        /// | `"%k"` |  `"22"`           | Hour of the time in 24 hour clock format, blank-padded ( 0..23)
        /// | `"%l"` |  `"10"`           | Hour of the time in 12 hour clock format, blank-padded ( 0..12)
        /// | `"%L"` |  `"000"`          | Millisecond of the time (3 digits, left padded with zero)
        /// | `"%m"` |  `"09"`           | Month of the time
        /// | `"%M"` |  `"49"`           | Minutes of the time (2 digits, left padded with zero e.g 01 02)
        /// | `"%n"` |                   | Newline character (\n)
        /// | `"%N"` |  `"000000000"`    | Nanoseconds of the time (9 digits, left padded with zero)
        /// | `"%p"` |  `"PM"`           | Gives AM / PM of the time
        /// | `"%P"` |  `"pm"`           | Gives am / pm of the time
        /// | `"%r"` |  `"10:49:27 PM"`  | Long time in 12 hour clock format (%I:%M:%S %p)
        /// | `"%R"` |  `"22:49"`        | Short time in 24 hour clock format (%H:%M)
        /// | `"%s"` |                   | Number of seconds since 1970-01-01 00:00:00 +0000
        /// | `"%S"` |  `"27"`           | Seconds of the time
        /// | `"%t"` |                   | Tab character (\t)
        /// | `"%T"` |  `"22:49:27"`     | Long time in 24 hour clock format (%H:%M:%S)
        /// | `"%u"` |  `"4"`            | Day of week of the time (from 1 for Monday to 7 for Sunday)
        /// | `"%U"` |  `"36"`           | Week number of the current year, starting with the first Sunday as the first day of the first week (00..53)
        /// | `"%v"` |  `"12-SEP-2013"`  | VMS date (%e-%b-%Y) (culture invariant)
        /// | `"%V"` |  `"37"`           | Week number of the current year according to ISO 8601 (01..53)
        /// | `"%W"` |  `"36"`           | Week number of the current year, starting with the first Monday as the first day of the first week (00..53)
        /// | `"%w"` |  `"4"`            | Day of week of the time (from 0 for Sunday to 6 for Saturday)
        /// | `"%x"` |                   | Preferred representation for the date alone, no time
        /// | `"%X"` |                   | Preferred representation for the time alone, no date
        /// | `"%y"` |  `"13"`           | Gives year without century of the time
        /// | `"%Y"` |  `"2013"`         | Year of the time
        /// | `"%Z"` |  `"+05:30"`       | Gives Time Zone of the time
        /// | `"%%"` |  `"%"`            | Output the character `%`
        ///
        /// Note that the format is using a good part of the ruby format ([source](http://apidock.com/ruby/DateTime/strftime))
        /// ```scriban-html
        /// {{ date.parse '2016/01/05' | date.to_string '%d %b %Y' }}
        /// {{ date.parse '2016/01/05' | date.to_string '%d %B %Y' 'fr-FR' }}
        /// ```
        /// ```html
        /// 05 Jan 2016
        /// 05 janvier 2016
        /// ```
        /// </summary>
        /// <param name="datetime">The input datetime to format</param>
        /// <param name="pattern">The date format pattern.</param>
        /// <param name="culture">The culture used to format the datetime</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public virtual string ToString(DateTime? datetime, string pattern, CultureInfo culture)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (!datetime.HasValue) return null;

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
                    var format = pattern[i];

                    // Switch to invariant culture
                    if (format == 'g')
                    {
                        culture = CultureInfo.InvariantCulture;
                        continue;
                    }

                    if (Formats.TryGetValue(format, out var formatterPair))
                    {
                        var formatter = formatterPair.Item1;
                        builder.Append(formatter.Invoke(datetime.Value, culture));
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

        public virtual object Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            // If we access `date` without any parameter, it calls by default the "parse" function
            // otherwise it is the 'date' object itself
            switch (arguments.Count)
            {
                case 0:
                    return this;
                case 1:
                    return Parse(context, context.ObjectToString(arguments[0]));
                default:
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid number of parameters `{arguments.Count}` for `date` object/function.");
            }
        }

        public int RequiredParameterCount => 0;

        public int ParameterCount => 0;

        public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;

        public Type ReturnType => typeof(object);

        private const string Parameter1Name = "text";

        public ScriptParameterInfo GetParameterInfo(int index)
        {
            return new ScriptParameterInfo(typeof(object), Parameter1Name);
        }

#if !SCRIBAN_NO_ASYNC
        public ValueTask<object> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
        {
            return new ValueTask<object>(Invoke(context, callerContext, arguments, blockStatement));
        }
#endif

        private void CreateImportFunctions()
        {
            // This function is very specific, as it is calling a member function of this instance
            // in order to retrieve the `date.format`
            this.Import("to_string", new Func<TemplateContext, DateTime?, string, string, string>(ToStringTrampoline));
        }

        private string ToStringTrampoline(TemplateContext context, DateTime? date, string pattern, string culture = null)
        {
            var cultureObject = culture != null ? CultureInfo.GetCultureInfo(culture) : null;
            return ToString(date, pattern, cultureObject ?? context.CurrentCulture);
        }
    }
}
