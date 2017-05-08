// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using Scriban.Model;
using Scriban.Runtime;

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

        private static object Round(TemplateContext context, ScriptNode callerContext, ScriptArray parameters)
        {
            if (parameters.Count < 1 || parameters.Count > 2)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected number of arguments [{parameters.Count}] for math.round. Expecting at least 1 parameter <precision>? <value>");
            }

            var value = ScriptValueConverter.ToDouble(callerContext.Span, parameters[parameters.Count - 1]);
            int precision = 0;
            if (parameters.Count == 2)
            {
                precision = ScriptValueConverter.ToInt(callerContext.Span, parameters[0]);
            }

            return Round(precision, value);
        }
    }
}