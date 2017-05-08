// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Scriban.Helpers;
using Scriban.Model;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    public static class ScriptValueConverter
    {
        public static string ToString(SourceSpan span, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is bool)
            {
                return ((bool) value) ? "true" : "false";
            }

            if (value is string)
            {
                return (string) value;
            }

            if (value is IScriptCustomFunction)
            {
                return "<function>";
            }

            // Dump a script object
            var scriptObject = value as ScriptObject;
            if (scriptObject != null)
            {
                return scriptObject.ToString(span);
            }

            var type = value.GetType();
            if (type.GetTypeInfo().IsPrimitive)
            {
                try
                {
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert value of type [{value.GetType()}] to string", ex);
                }
            }

            // If the value is formattable, use the formatter directly
            var fomattable = value as IFormattable;
            if (fomattable != null)
            {
                return fomattable.ToString();
            }

            // If we have an enumeration, we dump it
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                var result = new StringBuilder();
                result.Append("[");
                bool isFirst = true;
                foreach (var item in enumerable)
                {
                    if (!isFirst)
                    {
                        result.Append(", ");
                    }
                    result.Append(ToString(span, item));
                    isFirst = false;
                }
                result.Append("]");
                return result.ToString();
            }

            // Else just end-up trying to emit the ToString
            return value.ToString();
        }

        public static bool ToBool(object value)
        {
            // null -> false
            if (value == null)
            {
                return false;
            }

            // Special case for strings
            var valueStr = value as string;
            if (valueStr != null)
            {
                // If string is empty, we return false
                return valueStr != string.Empty;
            }

            var customType = value as IScriptCustomType;
            if (customType != null)
            {
                object result;
                if (customType.TryConvertTo(typeof (bool), out result))
                {
                    return (bool) result;
                }
            }

            try
            {
                // Try to use IConvertible only for primitives
                if (value.GetType().GetTypeInfo().IsPrimitive)
                {
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
            }
            catch (FormatException)
            {
                // We don't throw an error and return true by default as the object is not null
            }

            return true;
        }

        public static int ToInt(SourceSpan span, object value)
        {
            try
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type [{value.GetType()}] to int", ex);
            }
        }

        public static double ToDouble(SourceSpan span, object value)
        {
            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type [{value.GetType()}] to double", ex);
            }
        }

        public static object ToObject(SourceSpan span, object value, Type destinationType)
        {
            if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
            if (value == null)
            {
                if (destinationType == typeof (bool))
                {
                    return false;
                }

                if (destinationType == typeof (string))
                {
                    return string.Empty;
                }

                // TODO: Couldn't we get a converter method that support null to -> 0?
                if (destinationType == typeof(int))
                {
                    return (int)0;
                }
                else if (destinationType == typeof(double))
                {
                    return (double)0.0;
                }
                else if (destinationType == typeof(float))
                {
                    return (float)0.0f;
                }
                else if (destinationType == typeof(long))
                {
                    return (long)0L;
                }

                return null;
            }

            var type = value.GetType();
            var typeInfo = type.GetTypeInfo();
            var destTypeInfo = destinationType.GetTypeInfo();

            if (destTypeInfo.IsAssignableFrom(typeInfo))
            {
                return value;
            }

            if (destinationType == typeof (string))
            {
                return ToString(span, value);
            }

            if (typeInfo.IsPrimitive && destTypeInfo.IsPrimitive)
            {
                try
                {
                    return Convert.ChangeType(value, destinationType, CultureInfo.InvariantCulture);
                }
                catch (FormatException ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert type [{value.GetType()}] to [{destinationType}]", ex);
                }
            }

            var customType = value as IScriptCustomType;
            if (customType != null)
            {
                object result;
                if (customType.TryConvertTo(destinationType, out result))
                {
                    return result;
                }
            }

            return value;
        }
    }
}