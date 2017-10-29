// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Globalization;
using System.Reflection; // Leave this as it is required by some .NET targets
using System.Text;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban
{
    public partial class TemplateContext
    {
        /// <summary>
        /// Called whenever an objects is converted to a string. This method can be overriden.
        /// </summary>
        /// <param name="span">The current span calling this ToString</param>
        /// <param name="value">The object value to print</param>
        /// <returns>A string representing the object value</returns>
        public virtual string ToString(SourceSpan span, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is bool)
            {
                return ((bool)value) ? "true" : "false";
            }

            if (value is string)
            {
                return (string)value;
            }

            if (value is IScriptCustomFunction)
            {
                return "<function>";
            }

            if (value is DateTime)
            {
                // Output DateTime only if we have the date builtin object accessible (that provides the implementation of the ToString method)
                var dateTimeFunctions = GetValueFromVariable(DateTimeFunctions.DateVariable) as DateTimeFunctions;
                if (dateTimeFunctions != null)
                {
                    return dateTimeFunctions.ToString((DateTime) value, dateTimeFunctions.Format, CurrentCulture);
                }
            }

            // Dump a script object
            var scriptObject = value as ScriptObject;
            if (scriptObject != null)
            {
                return scriptObject.ToString(this, span);
            }

            // If we have a primitive, we can try to convert it
            var type = value.GetType();
            if (type.GetTypeInfo().IsPrimitive)
            {
                try
                {
                    return Convert.ToString(value, CurrentCulture);
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

        /// <summary>
        /// Called when evaluating a value to a boolean. Can be overriden for specific object scenarios.
        /// </summary>
        /// <param name="value">An object value</param>
        /// <returns>The boolean representation of the object</returns>
        public virtual bool ToBool(object value)
        {
            // null -> false
            if (value == null)
            {
                return false;
            }

            if (value is bool)
            {
                return (bool) value;
            }

            return true;
        }

        /// <summary>
        /// Called when evaluating a value to an integer. Can be overriden.
        /// </summary>
        /// <param name="span">The span requiring this conversion</param>
        /// <param name="value">The value of the object to convert</param>
        /// <returns>The integer value</returns>
        public virtual int ToInt(SourceSpan span, object value)
        {
            try
            {
                return Convert.ToInt32(value, CurrentCulture);
            }
            catch (FormatException ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type [{value.GetType()}] to int", ex);
            }
        }

        /// <summary>
        /// Called when evaluating a value to an double. Can be overriden.
        /// </summary>
        /// <param name="span">The span requiring this conversion</param>
        /// <param name="value">The value of the object to convert</param>
        /// <returns>The double value</returns>
        public virtual double ToDouble(SourceSpan span, object value)
        {
            try
            {
                return Convert.ToDouble(value, CurrentCulture);
            }
            catch (FormatException ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type [{value.GetType()}] to double", ex);
            }
        }

        /// <summary>
        /// Called when trying to convert an object to a destination type. Can be overriden.
        /// </summary>
        /// <param name="span">The span requiring this conversion</param>
        /// <param name="value">The value of the object to convert</param>
        /// <param name="destinationType">The destination type to try to convert to</param>
        /// <returns>The object value of possibly the destination type</returns>
        public virtual object ToObject(SourceSpan span, object value, Type destinationType)
        {
            if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));

            // Handle null case
            if (value == null)
            {
                if (destinationType == typeof(bool))
                {
                    return false;
                }

                if (destinationType == typeof(string))
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

            if (destTypeInfo == typeInfo || destTypeInfo.IsAssignableFrom(typeInfo))
            {
                return value;
            }

            if (destinationType == typeof(string))
            {
                return ToString(span, value);
            }

            if (typeInfo.IsPrimitive && destTypeInfo.IsPrimitive)
            {
                try
                {
                    return Convert.ChangeType(value, destinationType, CurrentCulture);
                }
                catch (FormatException ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert type [{value.GetType()}] to [{destinationType}]", ex);
                }
            }

            return value;
        }
    }
}