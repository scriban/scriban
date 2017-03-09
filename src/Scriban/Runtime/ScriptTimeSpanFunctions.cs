// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;

namespace Scriban.Runtime
{
    /// <summary>
    /// Functions exposed for time object
    /// </summary>
    /// <seealso cref="Scriban.Runtime.ScriptObject" />
    public class ScriptTimeSpanFunctions : ScriptObject
    {
        public static TimeSpan Zero => TimeSpan.Zero;

        public static ScriptTimeSpan FromDays(double days)
        {
            return TimeSpan.FromDays(days);
        }

        public static ScriptTimeSpan FromHours(double hours)
        {
            return TimeSpan.FromHours(hours);
        }

        public static ScriptTimeSpan FromMinutes(double minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        public static ScriptTimeSpan FromSeconds(double seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        public static ScriptTimeSpan FromMilliseconds(double milli)
        {
            return TimeSpan.FromMilliseconds(milli);
        }

        public static ScriptTimeSpan Parse(string text)
        {
            return TimeSpan.Parse(text);
        }
    }
}