// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
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
        /// Returns a boolean indicating whether the against object is empty (array/list count = 0, null, or no members for a dictionary/script object)
        /// </summary>
        /// <param name="span"></param>
        /// <param name="against"></param>
        /// <returns></returns>
        public virtual object IsEmpty(SourceSpan span, object against)
        {
            if (against == null)
            {
                return null;
            }
            if (against is IList)
            {
                return ((IList)against).Count == 0;
            }
            if (against is IEnumerable)
            {
                return !((IEnumerable)against).GetEnumerator().MoveNext();
            }
            if (against.GetType().IsPrimitiveOrDecimal())
            {
                return false;
            }
            return GetMemberAccessor(against).GetMemberCount(this, span, against) > 0;
        }

        public virtual IList ToList(SourceSpan span, object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IList)
            {
                return (IList) value;
            }
            var iterator = value as IEnumerable;
            if (iterator == null)
            {
                throw new ScriptRuntimeException(span, $"Unexpected list value. Expecting an array, list or iterator. Unablet to convert to a list");
            }
            return new ScriptArray(iterator);
        }

        /// <summary>
        /// Called whenever an objects is converted to a string. This method can be overriden.
        /// </summary>
        /// <param name="span">The current span calling this ToString</param>
        /// <param name="value">The object value to print</param>
        /// <returns>A string representing the object value</returns>
        public virtual string ToString(SourceSpan span, object value)
        {
            if (value is string)
            {
                return (string)value;
            }

            if (value == null || value == EmptyScriptObject.Default)
            {
                return null;
            }

            if (value is bool)
            {
                return ((bool)value) ? "true" : "false";
            }

            // If we have a primitive, we can try to convert it
            var type = value.GetType();
            if (type.IsPrimitiveOrDecimal())
            {
                try
                {
                    return Convert.ToString(value, CurrentCulture);
                }
                catch (Exception ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert value of type `{value.GetType()}` to string", ex);
                }
            }

            if (value is DateTime)
            {
                // Output DateTime only if we have the date builtin object accessible (that provides the implementation of the ToString method)
                var dateTimeFunctions = GetValue(DateTimeFunctions.DateVariable) as DateTimeFunctions;
                if (dateTimeFunctions != null)
                {
                    return dateTimeFunctions.ToString((DateTime)value, dateTimeFunctions.Format, CurrentCulture);
                }
            }

            // Dump a script object
            var scriptObject = value as ScriptObject;
            if (scriptObject != null)
            {
                return scriptObject.ToString(this, span);
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

            // Special case to display KeyValuePair as key, value
            var typeName = type.FullName;
            if (typeName != null && typeName.StartsWith("System.Collections.Generic.KeyValuePair"))
            {
                var keyValuePair = new ScriptObject(2);
                keyValuePair.Import(value, renamer: this.MemberRenamer);
                return ToString(span, keyValuePair);
            }

            if (value is IScriptCustomFunction)
            {
                return "<function>";
            }

            // Else just end-up trying to emit the ToString
            return value.ToString();
        }

        /// <summary>
        /// Called when evaluating a value to a boolean. Can be overriden for specific object scenarios.
        /// </summary>
        /// <param name="span">The span requiring this conversion</param>
        /// <param name="value">An object value</param>
        /// <returns>The boolean representation of the object</returns>
        public virtual bool ToBool(SourceSpan span, object value)
        {
            // null -> false
            if (value == null || value == EmptyScriptObject.Default)
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
                if (value == null) return 0;
                if (value is int) return (int) value;
                return Convert.ToInt32(value, CurrentCulture);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type `{value.GetType()}` to int", ex);
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

            // Make sure that we are using the underlying type of a a Nullable type
            destinationType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

            if (destinationType == typeof(string))
            {
                return ToString(span, value);
            }

            if (destinationType == typeof(int))
            {
                return ToInt(span, value);
            }

            if (destinationType == typeof(bool))
            {
                return ToBool(span, value);
            }

            // Handle null case
            if (value == null)
            {
                if (destinationType == typeof(double))
                {
                    return (double)0.0;
                }

                if (destinationType == typeof(float))
                {
                    return (float)0.0f;
                }

                if (destinationType == typeof(long))
                {
                    return (long)0L;
                }

                if (destinationType == typeof(decimal))
                {
                    return (decimal)0;
                }

                return null;
            }

            var type = value.GetType();
            if (destinationType == type)
            {
                return value;
            }

            // Check for inheritance
            var typeInfo = type.GetTypeInfo();
            var destTypeInfo = destinationType.GetTypeInfo();

            if (type.IsPrimitiveOrDecimal() && destinationType.IsPrimitiveOrDecimal())
            {
                try
                {
                    return Convert.ChangeType(value, destinationType, CurrentCulture);
                }
                catch (Exception ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert type `{value.GetType()}` to `{destinationType}`", ex);
                }
            }

            if (destinationType == typeof(IList))
            {
                return ToList(span, value);
            }

            if (destTypeInfo.IsAssignableFrom(typeInfo))
            {
                return value;
            }

            throw new ScriptRuntimeException(span, $"Unable to convert type `{value.GetType()}` to `{destinationType}`");
        }
    }
}