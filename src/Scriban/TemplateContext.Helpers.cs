// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection; // Leave this as it is required by some .NET targets
using System.Text;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban
{
    public partial class TemplateContext : IFormatProvider
    {
        public object GetFormat(Type formatType)
        {
            return CurrentCulture.GetFormat(formatType);
        }

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
                throw new ScriptRuntimeException(span, $"Unexpected list value. Expecting an array, list or iterator. Unable to convert to a list.");
            }
            return new ScriptArray(iterator);
        }

        private int _objectToStringLevel;

        /// <summary>
        /// Called whenever an objects is converted to a string. This method can be overriden.
        /// </summary>
        /// <param name="value">The object value to print</param>
        /// <param name="escapeString">True if value is a string, the string should be escaped</param>
        /// <returns>A string representing the object value</returns>
        public virtual string ObjectToString(object value, bool escapeString = false)
        {
            bool shouldEscapeString = escapeString || _objectToStringLevel > 0;
            try
            {
                _objectToStringLevel++;
                return ObjectToStringImpl(value, shouldEscapeString);
            }
            finally
            {
                _objectToStringLevel--;
            }
        }

        private string ObjectToStringImpl(object value, bool escapeString)
        {
            if (value is string)
            {
                return escapeString ? $"\"{StringFunctions.Escape((string) value)}\"" : (string) value;
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
                return ((IFormattable)value).ToString(null, this);
            }

            if (type == typeof(DateTime))
            {
                // Output DateTime only if we have the date builtin object accessible (that provides the implementation of the ToString method)
                var dateTimeFunctions = GetValue(DateTimeFunctions.DateVariable) as DateTimeFunctions;
                if (dateTimeFunctions != null)
                {
                    return dateTimeFunctions.ToString((DateTime)value, dateTimeFunctions.Format, CurrentCulture);
                }
            }

            // If the value is formattable, use the formatter directly
            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, this);
            }

            // If we have an enumeration, we dump it
            if (value is IEnumerable enumerable)
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
                    result.Append(ObjectToString(item));
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
                return ObjectToString(keyValuePair);
            }

            if (value is IScriptCustomFunction && !(value is ScriptFunction))
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

            if (UseScientific)
            {
                var type = value.GetType();
                if (type.IsPrimitive || type == typeof(decimal))
                {
                    return Convert.ToBoolean(value);
                }
                if (value is BigInteger bigInt)
                {
                    return bigInt != BigInteger.Zero;
                }
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
            if (value == null) return 0;
            if (value is int intValue) return intValue;
            try
            {
                if (value is BigInteger bigInt)
                {
                    return (int) bigInt;
                }

                if (value is IScriptConvertibleTo convertible && convertible.TryConvertTo(this, span, typeof(int), out var intObj))
                {
                    return (int) intObj;
                }
                return Convert.ToInt32(value, CurrentCulture);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type `{value.GetType().ScriptPrettyName()}` to int", ex);
            }
        }

        public T ToObject<T>(SourceSpan span, object value)
        {
            return (T) ToObject(span, value, typeof(T));
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

            var type = value?.GetType();

            // Early exit if types are already equal
            if (destinationType == type)
            {
                return value;
            }

            if (destinationType == typeof(string))
            {
                return ObjectToString(value);
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

                if (destinationType == typeof(BigInteger))
                {
                    return new BigInteger(0);
                }
                return null;
            }

            if (value is IScriptConvertibleTo convertible && convertible.TryConvertTo(this, span, destinationType, out var result))
            {
                return result;
            }

            if (typeof(IScriptConvertibleFrom).IsAssignableFrom(destinationType))
            {
                var dest = (IScriptConvertibleFrom)Activator.CreateInstance(destinationType);
                if (dest.TryConvertFrom(this, span, value))
                {
                    return dest;
                }
            }

            // Check for inheritance
            var typeInfo = type;

            if (type.IsPrimitiveOrDecimal() && destinationType.IsPrimitiveOrDecimal())
            {
                try
                {
                    if (destinationType == typeof(BigInteger))
                    {
                        if (type == typeof(char))
                        {
                            return new BigInteger((char)value);
                        }
                        if (type == typeof(bool))
                        {
                            return new BigInteger((bool)value ? 1 : 0);
                        }
                        if (type == typeof(float))
                        {
                            return new BigInteger((float)value);
                        }
                        if (type == typeof(double))
                        {
                            return new BigInteger((double)value);
                        }
                        if (type == typeof(int))
                        {
                            return new BigInteger((int)value);
                        }
                        if (type == typeof(uint))
                        {
                            return new BigInteger((uint)value);
                        }
                        if (type == typeof(long))
                        {
                            return new BigInteger((long)value);
                        }
                        if (type == typeof(ulong))
                        {
                            return new BigInteger((ulong)value);
                        }
                    }
                    else if (type == typeof(BigInteger))
                    {
                        if (destinationType == typeof(char))
                        {
                            return (char)(int)(BigInteger) value;
                        }
                        if (destinationType == typeof(float))
                        {
                            return (float)(BigInteger)value;
                        }
                        if (destinationType == typeof(double))
                        {
                            return (double)(BigInteger)value;
                        }
                        if (destinationType == typeof(uint))
                        {
                            return (uint)(BigInteger)value;
                        }
                        if (destinationType == typeof(long))
                        {
                            return (long)(BigInteger)value;
                        }
                        if (destinationType == typeof(ulong))
                        {
                            return (ulong) (BigInteger) value;
                        }
                    }
                    return Convert.ChangeType(value, destinationType, CurrentCulture);
                }
                catch (Exception ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert type `{type.ScriptPrettyName()}` to `{destinationType.ScriptPrettyName()}`", ex);
                }
            }

            if (destinationType == typeof(IList))
            {
                return ToList(span, value);
            }

            if (destinationType.IsAssignableFrom(typeInfo))
            {
                return value;
            }

            throw new ScriptRuntimeException(span, $"Unable to convert type `{type.ScriptPrettyName()}` to `{destinationType.ScriptPrettyName()}`");
        }

    }
}