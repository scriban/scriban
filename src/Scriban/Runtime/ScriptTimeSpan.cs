// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;

namespace Scriban.Runtime
{
    /// <summary>
    /// Represents a timespan equivalent to <see cref="TimeSpan"/>.
    /// </summary>
    /// <seealso cref="Scriban.Runtime.IScriptCustomType" />
    /// <seealso cref="System.IComparable" />
    public struct ScriptTimeSpan : IScriptCustomType, IComparable
    {
        private TimeSpan value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTimeSpan"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public ScriptTimeSpan(TimeSpan value)
        {
            this.value = value;
        }

        public int Days => value.Days;

        public int Hours => value.Hours;

        public int Minutes => value.Minutes;

        public int Seconds => value.Seconds;

        public int Milliseconds => value.Milliseconds;

        public double TotalDays => value.TotalDays;

        public double TotalHours => value.TotalHours;

        public double TotalMinutes => value.TotalMinutes;

        public double TotalSeconds => value.TotalSeconds;

        public double TotalMilliseconds => value.TotalMilliseconds;

        [ScriptMemberIgnore]
        public static implicit operator ScriptTimeSpan(TimeSpan timeSpan)
        {
            return new ScriptTimeSpan(timeSpan);
        }

        [ScriptMemberIgnore]
        public static implicit operator TimeSpan(ScriptTimeSpan timeSpan)
        {
            return timeSpan.value;
        }

        bool IScriptCustomType.TryConvertTo(Type destinationType, out object outValue)
        {
            outValue = null;
            if (destinationType == typeof (bool))
            {
                outValue = value != TimeSpan.Zero;
                return true;
            }

            if (destinationType == typeof (double))
            {
                outValue = value.TotalDays;
                return true;
            }

            return false;
        }

        object IScriptCustomType.EvaluateUnaryExpression(ScriptUnaryExpression expression)
        {
            switch (expression.Operator)
            {
                case ScriptUnaryOperator.Negate:
                    return (ScriptTimeSpan)value.Negate();
                case ScriptUnaryOperator.Not:
                    return value == TimeSpan.Zero;
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
                    return new ScriptTimeSpan(left.value + right.value);
                case ScriptBinaryOperator.Substract:
                    return new ScriptTimeSpan(left.value - right.value);
                case ScriptBinaryOperator.CompareEqual:
                    return left.value == right.value;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left.value != right.value;
                case ScriptBinaryOperator.CompareLess:
                    return left.value < right.value;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left.value <= right.value;
                case ScriptBinaryOperator.CompareGreater:
                    return left.value > right.value;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left.value >= right.value;
            }

            throw new ScriptRuntimeException(expression.Span, $"Operator [{expression.Operator}] is not supported for timespan");
        }

        private object EvaluateBinaryExpression(ScriptBinaryExpression expression, ScriptTimeSpan left,
            ScriptDate right)
        {
            switch (expression.Operator)
            {
                case ScriptBinaryOperator.Add:
                    return new ScriptDate(right.Global, right.Value + left.value);
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

            return value.CompareTo(timeSpan.value);
        }

        public override string ToString()
        {
            // TODO: standardize
            return value.ToString();
        }
    }
}