// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("binary expression", "<expression> operator <expression>")]
    public class ScriptBinaryExpression : ScriptExpression
    {
        public ScriptExpression Left { get; set; }

        public ScriptBinaryOperator Operator { get; set; }

        public ScriptExpression Right { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            var leftValueOriginal = context.Evaluate(Left);
            var leftValue = leftValueOriginal;
            var rightValueOriginal = context.Evaluate(Right);
            object rightValue = rightValueOriginal;

            if (Operator == ScriptBinaryOperator.EmptyCoalescing)
            {
                return leftValue ?? rightValue;
            }
            else if (Operator == ScriptBinaryOperator.And || Operator == ScriptBinaryOperator.Or)
            {
                var leftBoolValue = context.ToBool(leftValue);
                var rightBoolValue = context.ToBool(rightValue);
                if (Operator == ScriptBinaryOperator.And)
                {
                    return leftBoolValue && rightBoolValue;
                }
                else
                {
                    return leftBoolValue || rightBoolValue;
                }
            }
            else
            {
                switch (Operator)
                {
                    case ScriptBinaryOperator.ShiftLeft:
                        var leftList = leftValue as IList;
                        if (leftList != null)
                        {
                            var newList = new ScriptArray(leftList) { rightValue };
                            return newList;
                        }
                        break;
                    case ScriptBinaryOperator.ShiftRight:
                        var rightList = rightValue as IList;
                        if (rightList != null)
                        {
                            var newList = new ScriptArray(rightList);
                            newList.Insert(0, leftValue);
                            return newList;
                        }
                        break;
                    case ScriptBinaryOperator.CompareEqual:
                    case ScriptBinaryOperator.CompareNotEqual:
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                    case ScriptBinaryOperator.Add:
                    case ScriptBinaryOperator.Substract:
                    case ScriptBinaryOperator.Multiply:
                    case ScriptBinaryOperator.Divide:
                    case ScriptBinaryOperator.DivideRound:
                    case ScriptBinaryOperator.Modulus:
                    case ScriptBinaryOperator.RangeInclude:
                    case ScriptBinaryOperator.RangeExclude:
                        var leftType = leftValue?.GetType();
                        var rightType = rightValue?.GetType();

                        if (leftValue is string || rightValue is string)
                        {
                            return CalculateToString(context, leftValue, rightValue);
                        }
                        else
                        {
                            return Calculate(context, leftValue, leftType, rightValue, rightType);
                        }
                }
            }

            throw new ScriptRuntimeException(Span, $"Operator [{Operator.ToText()}] is not implemented for the left [{Left}] / right [{Right}]");
        }

        public override string ToString()
        {
            return $"{Left} {Operator.ToText()} {Right}";
        }

        private object CalculateToString(TemplateContext context, object left, object right)
        {
            switch (Operator)
            {
                case ScriptBinaryOperator.Add:
                    return context.ToString(Span, left) + context.ToString(Span, right);
                case ScriptBinaryOperator.Multiply:
                    if (right is int)
                    {
                        var temp = left;
                        left = right;
                        right = temp;
                    }

                    if (left is int)
                    {
                        var rightText = context.ToString(Span, right);
                        var builder = new StringBuilder();
                        for (int i = 0; i < (int) left; i++)
                        {
                            builder.Append(rightText);
                        }
                        return builder.ToString();
                    }
                    throw new ScriptRuntimeException(Span, $"Operator [{Operator.ToText()}] is not supported for the expression [{this}]. Only working on string x int or int x string"); // unit test: 112-binary-string-error1.txt
                case ScriptBinaryOperator.CompareEqual:
                    return context.ToString(Span, left) == context.ToString(Span, right);
                case ScriptBinaryOperator.CompareNotEqual:
                    return context.ToString(Span, left) != context.ToString(Span, right);
                case ScriptBinaryOperator.CompareGreater:
                    return context.ToString(Span, left).CompareTo(context.ToString(Span, right)) > 0;
                case ScriptBinaryOperator.CompareLess:
                    return context.ToString(Span, left).CompareTo(context.ToString(Span, right)) < 0;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return context.ToString(Span, left).CompareTo(context.ToString(Span, right)) >= 0;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return context.ToString(Span, left).CompareTo(context.ToString(Span, right)) <= 0;
            }

            // unit test: 150-range-expression-error1.out.txt
            throw new ScriptRuntimeException(Span, $"Operator [{Operator.ToText()}] is not supported on string objects"); // unit test: 112-binary-string-error2.txt
        }

        private object Calculate(ScriptBinaryOperator op, int left, int right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left*right;
                case ScriptBinaryOperator.Divide:
                    return (float)left/right;
                case ScriptBinaryOperator.DivideRound:
                    return left/right;
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
                case ScriptBinaryOperator.RangeInclude:
                    return RangeInclude(left, right);
                case ScriptBinaryOperator.RangeExclude:
                    return RangeExclude(left, right);
            }
            throw new ScriptRuntimeException(Span, $"The operator [{op.ToText()}] is not implemented for int<->int");
        }


        private static IEnumerable<int> RangeInclude(int left, int right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (int i = left; i <= right; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = left; i >= right; i--)
                {
                    yield return i;
                }
            }
        }

        private static IEnumerable<int> RangeExclude(int left, int right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (int i = left; i < right; i++)
                {
                    yield return i;
                }
            }
            else
            {
                for (int i = left; i > right; i--)
                {
                    yield return i;
                }
            }
        }

        private object Calculate(ScriptBinaryOperator op, long left, long right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (float)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return left / right;
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
            }
            throw new ScriptRuntimeException(Span, $"The operator [{op.ToText()}] is not implemented for long<->long");
        }


        private object Calculate(ScriptBinaryOperator op, double left, double right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (float)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return (double)(int)(left / right);
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
            }
            throw new ScriptRuntimeException(Span, $"The operator [{op.ToText()}] is not implemented for double<->double");
        }

        private object Calculate(ScriptBinaryOperator op, float left, float right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (float)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return (double)(int)(left / right);
                case ScriptBinaryOperator.Modulus:
                    return left % right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
            }
            throw new ScriptRuntimeException(Span, $"The operator [{op.ToText()}] is not implemented for float<->float");
        }

        private object EvaluateBinaryExpression(TemplateContext context, DateTime left, DateTime right)
        {
            switch (Operator)
            {
                case ScriptBinaryOperator.Substract:
                    return left - right;
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
                case ScriptBinaryOperator.CompareLess:
                    return left < right;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return left <= right;
                case ScriptBinaryOperator.CompareGreater:
                    return left > right;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return left >= right;
            }

            throw new ScriptRuntimeException(Span, $"The operator [{Operator}] is not supported for DateTime");
        }

        private object EvaluateBinaryExpression(TemplateContext context, DateTime left, TimeSpan right)
        {
            switch (Operator)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
            }

            throw new ScriptRuntimeException(Span, $"The operator [{Operator}] is not supported for between <DateTime> and <TimeSpan>");
        }

        private object Calculate(ScriptBinaryOperator op, bool left, bool right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
            }
            throw new ScriptRuntimeException(Span, $"The operator [{op.ToText()}] is not valid for bool<->bool");
        }

        private object Calculate(TemplateContext context, object leftValue, Type leftType, object rightValue, Type rightType)
        {
            var op = Operator;
            // The order matters: double, float, long, int
            if (leftType == typeof(double))
            {
                var rightDouble = (double)context.ToObject(Span, rightValue, typeof(double));
                return Calculate(op, (double)leftValue, rightDouble);
            }

            if (rightType == typeof(double))
            {
                var leftDouble = (double)context.ToObject(Span, leftValue, typeof(double));
                return Calculate(op, leftDouble, (double)rightValue);
            }

            if (leftType == typeof(float))
            {
                var rightFloat = (float)context.ToObject(Span, rightValue, typeof(float));
                return Calculate(op, (float)leftValue, rightFloat);
            }

            if (rightType == typeof(float))
            {
                var leftFloat = (float)context.ToObject(Span, leftValue, typeof(float));
                return Calculate(op, leftFloat, (float)rightValue);
            }

            if (leftType == typeof(long))
            {
                var rightLong = (long)context.ToObject(Span, rightValue, typeof(long));
                return Calculate(op, (long)leftValue, rightLong);
            }

            if (rightType == typeof(long))
            {
                var leftLong = (long)context.ToObject(Span, leftValue, typeof(long));
                return Calculate(op, leftLong, (long)rightValue);
            }

            if (leftType == typeof (int) || (leftType != null && leftType.GetTypeInfo().IsEnum))
            {
                var rightInt = (int) context.ToObject(Span, rightValue, typeof (int));
                return Calculate(op, (int) leftValue, rightInt);
            }

            if (rightType == typeof (int) || (rightType != null && rightType.GetTypeInfo().IsEnum))
            {
                var leftInt = (int) context.ToObject(Span, leftValue, typeof (int));
                return Calculate(op, leftInt, (int) rightValue);
            }

            if (leftType == typeof(bool))
            {
                var rightBool = (bool)context.ToObject(Span, rightValue, typeof(bool));
                return Calculate(op, (bool)leftValue, rightBool);
            }

            if (rightType == typeof(bool))
            {
                var leftBool = (bool)context.ToObject(Span, leftValue, typeof(bool));
                return Calculate(op, leftBool, (bool)rightValue);
            }

            if (leftType == typeof(DateTime) && rightType == typeof(DateTime))
            {
                return EvaluateBinaryExpression(context, (DateTime)leftValue, (DateTime)rightValue);
            }

            if (leftType == typeof(DateTime) && rightType == typeof(TimeSpan))
            {
                return EvaluateBinaryExpression(context, (DateTime)leftValue, (TimeSpan)rightValue);
            }

            throw new ScriptRuntimeException(Span, $"Unsupported types [{leftValue ?? "null"}/{leftType?.ToString() ?? "null"}] {op.ToText()} [{rightValue ?? "null"}/{rightType?.ToString() ?? "null"}] for binary operation");
        }
    }
}