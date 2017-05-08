// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using Scriban.Runtime;

namespace Scriban.Functions
{
    /// <summary>
    /// The object representing the global 'date'  object
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
    public class ScriptDateFunctions : ScriptObject
    {
        private const string FormatKey = "format";

        // This is exposed as well as default_format
        public const string DefaultFormat = "%d %b %Y";

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptDateFunctions"/> class.
        /// </summary>
        public ScriptDateFunctions()
        {
            Format = DefaultFormat;

            this.Import("now", new Func<ScriptDate>(() => new ScriptDate(this, DateTime.Now)));

            this.Import("to_string", new Func<string, ScriptDate, string>((pattern, date) => date.ToString(pattern)));

            this.Import("parse", new Func<string, ScriptDate>((dateAsText) => Parse(dateAsText, this)));
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
        /// <param name="days">The days.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddDays(double days, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddDays(days));
        }

        /// <summary>
        /// Adds years to date.
        /// </summary>
        /// <param name="years">The years.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddYears(int years, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddYears(years));
        }

        /// <summary>
        /// Adds months to date.
        /// </summary>
        /// <param name="months">The months.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddMonths(int months, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddMonths(months));
        }

        /// <summary>
        /// Adds hours to date.
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddHours(double hours, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddHours(hours));
        }

        /// <summary>
        /// Adds minutes to date.
        /// </summary>
        /// <param name="minutes">The minutes.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddMinutes(double minutes, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddMinutes(minutes));
        }

        /// <summary>
        /// Adds seconds to date.
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddSeconds(double seconds, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddSeconds(seconds));
        }

        /// <summary>
        /// Adds millis to date.
        /// </summary>
        /// <param name="millis">The millis.</param>
        /// <param name="date">The date.</param>
        /// <returns>A new date</returns>
        public static ScriptDate AddMilliseconds(double millis, ScriptDate date)
        {
            return new ScriptDate(date.Global, date.Value.AddMilliseconds(millis));
        }

        /// <summary>
        /// Parses the specified text as a <see cref="ScriptDate"/> using the current culture.
        /// </summary>
        /// <param name="text">A text representing a date.</param>
        /// <param name="globalDateObject">The global date object</param>
        /// <returns>A date object</returns>
        internal static ScriptDate Parse(string text, ScriptDateFunctions globalDateObject)
        {
            if (globalDateObject == null) throw new ArgumentNullException(nameof(globalDateObject));
            if (String.IsNullOrEmpty(text))
            {
                return new ScriptDate(globalDateObject, new DateTime());
            }
            DateTime result;
            if (DateTime.TryParse(text, out result))
            {
                return new ScriptDate(globalDateObject, result);
            }

            return new ScriptDate(globalDateObject, new DateTime());
        }
    }
}