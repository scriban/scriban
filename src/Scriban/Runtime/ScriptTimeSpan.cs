// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using Scriban.Model;

namespace Scriban.Runtime
{
    /// <summary>
    /// Represents a timespan equivalent to <see cref="TimeSpan"/>.
    /// </summary>
    /// <seealso cref="Scriban.Runtime.IScriptCustomType" />
    /// <seealso cref="System.IComparable" />
    public struct ScriptTimeSpan : IScriptCustomType, IComparable
    {
        private TimeSpan _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTimeSpan"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScriptTimeSpan(TimeSpan value)
        {
            this._value = value;
        }

        public int Days => _value.Days;

        public int Hours => _value.Hours;

        public int Minutes => _value.Minutes;

        public int Seconds => _value.Seconds;

        public int Milliseconds => _value.Milliseconds;

        public double TotalDays => _value.TotalDays;

        public double TotalHours => _value.TotalHours;

        public double TotalMinutes => _value.TotalMinutes;

        public double TotalSeconds => _value.TotalSeconds;

        public double TotalMilliseconds => _value.TotalMilliseconds;

        [ScriptMemberIgnore]
        public static implicit operator ScriptTimeSpan(TimeSpan timeSpan)
        {
            return new ScriptTimeSpan(timeSpan);
        }

        [ScriptMemberIgnore]
        public static implicit operator TimeSpan(ScriptTimeSpan timeSpan)
        {
            return timeSpan._value;
        }

        bool IScriptCustomType.TryConvertTo(Type destinationType, out object outValue)
        {
            outValue = null;
            if (destinationType == typeof (bool))
            {
                outValue = _value != TimeSpan.Zero;
                return true;
            }

            if (destinationType == typeof (double))
            {
                outValue = _value.TotalDays;
                return true;
            }

            return false;
        }

        object IScriptCustomType.EvaluateUnaryExpression(ScriptUnaryExpression expression)
        {
            switch (expression.Operator)
            {
                case ScriptUnaryOperator.Negate:
                    return (ScriptTimeSpan)_value.Negate();
                case ScriptUnaryOperator.Not:
                    return _value == TimeSpan.Zero;
                default:
                    throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for timespan");
            }
        }

        object IScriptCustomType.EvaluateBinaryExpression(ScriptBinaryExpression expression, object left, object right)
        {
            if (left is ScriptTimeSpan && right is ScriptTimeSpan)
            {
                return EvaluateBinaryExpression(expression, (ScriptTimeSpan) left, (ScriptTimeSpan) right);
            }

            if (left is ScriptTimeSpan && right is ScriptDate)
            {
                return EvaluateBinaryExpression(expression, (ScriptTimeSpan)left, (ScriptDate)right);
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for between [{left?.GetType()}] and [{right?.GetType()}]");
        }

        private object EvaluateBinaryExpression(ScriptBinaryExpression expression, ScriptTimeSpan left,
            ScriptTimeSpan right)
        {
            switch (expression.Operator)
            {
                case ScriptBinaryOperator.Add:
                    return new ScriptTimeSpan(left._value + right._value);
                case ScriptBinaryOperator.Substract:
                    return new ScriptTimeSpan(left._value - right._value);
                case ScriptBinaryOperator.CompareEqual:
                    return left._value == right._value;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left._value != right._value;
                case ScriptBinaryOperator.CompareLess:
                    return left._value < right._value;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left._value <= right._value;
                case ScriptBinaryOperator.CompareGreater:
                    return left._value > right._value;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left._value >= right._value;
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for timespan");
        }

        private object EvaluateBinaryExpression(ScriptBinaryExpression expression, ScriptTimeSpan left,
            ScriptDate right)
        {
            switch (expression.Operator)
            {
                case ScriptBinaryOperator.Add:
                    return new ScriptDate(right.Global, right.Value + left._value);
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for between <timespan> and <date>");
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            if (!(obj is ScriptTimeSpan))
            {
                throw new ArgumentException($"Object [{obj.GetType()}] cannot be compare to a date object");
            }
            var timeSpan = (ScriptTimeSpan)obj;

            return _value.CompareTo(timeSpan._value);
        }

        public override string ToString()
        {
            // TODO: standardize
            return _value.ToString();
        }
    }
}