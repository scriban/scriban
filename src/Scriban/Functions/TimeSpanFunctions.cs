// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using Scriban.Runtime;

namespace Scriban.Functions
{
    /// <summary>
    /// A timespan object represents a time interval.
    ///
    /// | Name             | Description
    /// |--------------    |-----------------
    /// | `.days`          | Gets the number of days of this interval
    /// | `.hours`         | Gets the number of hours of this interval
    /// | `.minutes`       | Gets the number of minutes of this interval
    /// | `.seconds`       | Gets the number of seconds of this interval
    /// | `.milliseconds`  | Gets the number of milliseconds of this interval
    /// | `.total_days`    | Gets the total number of days in fractional part
    /// | `.total_hours`   | Gets the total number of hours in fractional part
    /// | `.total_minutes` | Gets the total number of minutes in fractional part
    /// | `.total_seconds` | Gets the total number of seconds  in fractional part
    /// | `.total_milliseconds` | Gets the total number of milliseconds  in fractional part
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class TimeSpanFunctions : ScriptObject
    {
        /// <summary>
        /// Returns a timespan object that represents a 0 interval
        /// </summary>
        /// <returns>A zero timespan object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ (timespan.zero + timespan.from_days 5).days }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan Zero => TimeSpan.Zero;

        /// <summary>
        /// Returns a timespan object that represents a `days` interval
        /// </summary>
        /// <param name="days">The days.</param>
        /// <returns>A timespan object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ (timespan.from_days 5).days }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromDays(double days)
        {
            return TimeSpan.FromDays(days);
        }

        /// <summary>
        /// Returns a timespan object that represents a `hours` interval
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns>A timespan object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ (timespan.from_hours 5).hours }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromHours(double hours)
        {
            return TimeSpan.FromHours(hours);
        }

        /// <summary>
        /// Returns a timespan object that represents a `minutes` interval
        /// </summary>
        /// <param name="minutes">The minutes.</param>
        /// <returns>A timespan object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ (timespan.from_minutes 5).minutes }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromMinutes(double minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        /// <summary>
        /// Returns a timespan object that represents a `seconds` interval
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        /// <returns>A timespan object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ (timespan.from_seconds 5).seconds }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromSeconds(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Returns a timespan object that represents a `milliseconds` interval
        /// </summary>
        /// <param name="millis">The milliseconds.</param>
        /// <returns>A timespan object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ (timespan.from_milliseconds 5).milliseconds }}
        /// ```
        /// ```html
        /// 5
        /// ```
        /// </remarks>
        public static TimeSpan FromMilliseconds(double millis)
        {
            return TimeSpan.FromMilliseconds(millis);
        }

        /// <summary>
        /// Parses the specified input string into a timespan object.
        /// </summary>
        /// <param name="text">A timespan text</param>
        /// <returns>A timespan object parsed from timespan</returns>
        public static TimeSpan Parse(string text)
        {
            return TimeSpan.Parse(text);
        }
    }
}