// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class TemplateContext : IFormatProvider
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
        private int _currentToStringLength;

        /// <summary>
        /// Called whenever an objects is converted to a string. This method can be overriden.
        /// </summary>
        /// <param name="value">The object value to print</param>
        /// <param name="nested">True if value is a string, the string should be escaped</param>
        /// <returns>A string representing the object value</returns>
        public virtual string ObjectToString(object value, bool nested = false)
        {
            bool shouldEscapeString = nested || _objectToStringLevel > 0;
            if (_objectToStringLevel == 0)
            {
                _currentToStringLength = 0;
            }
            try
            {
                _objectToStringLevel++;
                if (ObjectRecursionLimit != 0 && _objectToStringLevel > ObjectRecursionLimit)
                    throw new InvalidOperationException("Structure is too deeply nested or contains reference loops.");
                var result = ObjectToStringImpl(value, shouldEscapeString);
                if (LimitToString > 0 && _objectToStringLevel  == 1 && result != null && result.Length >= LimitToString)
                {
                    return result + "...";
                }
                return result;
            }
            finally
            {
                _objectToStringLevel--;
            }
        }

        private string ObjectToStringImpl(object value, bool nested)
        {
            if (LimitToString > 0 && _currentToStringLength >= LimitToString) return string.Empty;

            if (value is string str)
            {
                if (LimitToString > 0 && _currentToStringLength + str.Length >= LimitToString)
                {
                    var index = LimitToString - _currentToStringLength;
                    if (index <= 0) return string.Empty;
                    str = str.Substring(0, index);
                    return nested ? $"\"{StringFunctions.Escape(str)}" : (string)value;
                }

                return nested ? $"\"{StringFunctions.Escape(str)}\"" : (string) value;
            }

            if (value == null || value == EmptyScriptObject.Default)
            {
                return nested ? "null" : null;
            }

            if (value is bool b)
            {
                return b ? "true" : "false";
            }

            if (value is DateTime dt)
            {
                // Output DateTime only if we have the date builtin object accessible (that provides the implementation of the ToString method)
                bool isStrict = StrictVariables;
                try
                {
                    StrictVariables = false;
                    if (GetValue(DateTimeFunctions.DateVariable) is DateTimeFunctions dateTimeFunctions)
                    {
                        return dateTimeFunctions.ToString(dt, dateTimeFunctions.Format, CurrentCulture);
                    }
                }
                finally
                {
                    StrictVariables = isStrict;
                }
            }

            // If the value is formattable, use the formatter directly
            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, this);
            }

            if (value is IConvertible convertible)
            {
                return convertible.ToString(this);
            }

            // If we have an enumeration, we dump it
            if (value is IEnumerable enumerable)
            {
                var result = new StringBuilder();
                result.Append("[");
                _currentToStringLength++;
                bool isFirst = true;
                foreach (var item in enumerable)
                {
                    if (!isFirst)
                    {
                        result.Append(", ");
                        _currentToStringLength += 2;
                    }

                    var itemStr = ObjectToString(item);
                    result.Append(itemStr);
                    if (itemStr != null) _currentToStringLength += itemStr.Length;

                    // Limit to size
                    if (LimitToString > 0 && _currentToStringLength >= LimitToString)
                    {
                        return result.ToString();
                    }

                    isFirst = false;
                }
                result.Append("]");
                _currentToStringLength += 1;
                return result.ToString();
            }

            // Special case to display KeyValuePair as key, value
            var type = value.GetType();
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

                if (value is uint uintValue)
                {
                    return checked((int)uintValue);
                }

                if (value is ulong ulongValue)
                {
                    return checked((int)ulongValue);
                }

                return Convert.ToInt32(value, CurrentCulture);
            }
            catch (Exception ex)
            {
                throw new ScriptRuntimeException(span, $"Unable to convert type `{GetTypeName(value)}` to int", ex);
            }
        }

        public virtual string GetTypeName(object value)
        {
            if (value == null) return "null";

            if (value is Type type)
            {
                return type.ScriptPrettyName();
            }

            if (value is IScriptCustomTypeInfo customTypeInfo) return customTypeInfo.TypeName;

            return value.GetType().ScriptPrettyName();
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
            bool isNullable;
            (isNullable, destinationType) = GetNullableInfo(destinationType);

            var type = value?.GetType();

            // Early exit if types are already equal
            if (destinationType == type)
            {
                return value;
            }

            if (isNullable && value is null)
            {
                return null;
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

            if (destinationType.IsEnum)
            {
                try
                {
                    if (value is string str)
                    {
                        return Enum.Parse(destinationType, str);
                    }
                    return Enum.ToObject(destinationType, value);
                }
                catch (Exception ex)
                {
                    throw new ScriptRuntimeException(span, $"Unable to convert type `{GetTypeName(value)}` to `{GetTypeName(destinationType)}`", ex);
                }
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
                    throw new ScriptRuntimeException(span, $"Unable to convert type `{GetTypeName(value)}` to `{GetTypeName(destinationType)}`", ex);
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

            throw new ScriptRuntimeException(span, $"Unable to convert type `{GetTypeName(value)}` to `{GetTypeName(destinationType)}`");

            static (bool IsNullable, Type DestinationType) GetNullableInfo(Type destinationType)
            {
                destinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
                var underlyingType = Nullable.GetUnderlyingType(destinationType);
                return underlyingType is null ? (false, destinationType) : (true, underlyingType);
            }
        }

    }
}