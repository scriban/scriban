// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("binary expression", "<expression> operator <expression>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptBinaryExpression : ScriptExpression
    {
        private ScriptExpression _left;
        private ScriptToken _operatorToken;
        private ScriptExpression _right;

        public ScriptExpression Left
        {
            get => _left;
            set => ParentToThis(ref _left, value);
        }

        public ScriptBinaryOperator Operator { get; set; }

        public ScriptToken OperatorToken
        {
            get => _operatorToken;
            set => ParentToThis(ref _operatorToken, value);
        }

        public string OperatorAsText => OperatorToken?.Value ?? Operator.ToText();

        public ScriptExpression Right
        {
            get => _right;
            set => ParentToThis(ref _right, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // If we are in scientific mode and we have a function which takes arguments, and is not an explicit call (e.g sin(x) rather then sin * x)
            // Then we need to rewrite the call to a proper expression.
            if (context.UseScientific)
            {
                var newExpression = ScientificFunctionCallRewriter.Rewrite(context, this);
                if (!ReferenceEquals(newExpression, this))
                {
                    return context.Evaluate(newExpression);
                }
            }

            var leftValue = context.Evaluate(Left);

            switch (Operator)
            {
                case ScriptBinaryOperator.And:
                {
                    var leftBoolValue = context.ToBool(Left.Span, leftValue);
                    if (!leftBoolValue) return false;
                    var rightValue = context.Evaluate(Right);
                    var rightBoolValue = context.ToBool(Right.Span, rightValue);
                    return leftBoolValue && rightBoolValue;
                }

                case ScriptBinaryOperator.Or:
                {
                    var leftBoolValue = context.ToBool(Left.Span, leftValue);
                    if (leftBoolValue) return true;
                    var rightValue = context.Evaluate(Right);
                    return context.ToBool(Right.Span, rightValue);
                }

                default:
                {
                    var rightValue = context.Evaluate(Right);
                    return Evaluate(context, OperatorToken?.Span ?? Span , Operator, Left.Span, leftValue, Right.Span, rightValue);
                }
            }
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Left);
            // Because a-b is a variable name, we need to transform binary op a-b to a - b
            if (Operator == ScriptBinaryOperator.Subtract && !printer.PreviousHasSpace)
            {
                printer.ExpectSpace();
            }

            if (OperatorToken != null)
            {
                printer.Write(OperatorToken);
            }
            else
            {
                printer.ExpectSpace();
            }

            if (Operator == ScriptBinaryOperator.Subtract)
            {
                printer.ExpectSpace();
            }
            printer.Write(Right);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }


        public static object Evaluate(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, object leftValue, object rightValue)
        {
            return Evaluate(context, span, op, span, leftValue, span, rightValue);
        }

        public static object Evaluate(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object leftValue, SourceSpan rightSpan, object rightValue)
        {
            if (op == ScriptBinaryOperator.EmptyCoalescing)
            {
                return leftValue ?? rightValue;
            }

            if (op == ScriptBinaryOperator.NotEmptyCoalescing)
            {
                return leftValue != null ? rightValue : leftValue;
            }

            switch (op)
            {
                case ScriptBinaryOperator.LiquidHasKey:
                {
                    var leftDict = leftValue as IDictionary<string, object>;
                    if (leftDict != null)
                    {
                        return ObjectFunctions.HasKey(leftDict, context.ObjectToString(rightValue));
                    }
                }
                    break;

                case ScriptBinaryOperator.LiquidHasValue:
                {
                    var leftDict = leftValue as IDictionary<string, object>;
                    if (leftDict != null)
                    {
                        return ObjectFunctions.HasValue(leftDict, context.ObjectToString(rightValue));
                    }
                }
                    break;

                case ScriptBinaryOperator.CompareEqual:
                case ScriptBinaryOperator.CompareNotEqual:
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.CompareLess:
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Subtract:
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.Power:
                case ScriptBinaryOperator.BinaryAnd:
                case ScriptBinaryOperator.BinaryOr:
                case ScriptBinaryOperator.ShiftLeft:
                case ScriptBinaryOperator.ShiftRight:
                case ScriptBinaryOperator.RangeInclude:
                case ScriptBinaryOperator.RangeExclude:
                case ScriptBinaryOperator.LiquidContains:
                case ScriptBinaryOperator.LiquidStartsWith:
                case ScriptBinaryOperator.LiquidEndsWith:
                    try
                    {
                        if (leftValue is string || rightValue is string || leftValue is char || rightValue is char)
                        {
                            if (leftValue is char leftChar) leftValue = leftChar.ToString(context.CurrentCulture);
                            if (rightValue is char rightChar) rightValue = rightChar.ToString(CultureInfo.InvariantCulture);

                            return CalculateToString(context, span, op, leftSpan, leftValue, rightSpan, rightValue);
                        }
                        else if (leftValue == EmptyScriptObject.Default || rightValue == EmptyScriptObject.Default)
                        {
                            return CalculateEmpty(context, span, op, leftSpan, leftValue, rightSpan, rightValue);
                        }
                        // Allow custom binary operation
                        else if (leftValue is IScriptCustomBinaryOperation leftBinaryOp)
                        {
                            if (leftBinaryOp.TryEvaluate(context, span, op, leftSpan, leftValue, rightSpan, rightValue, out var result))
                            {
                                return result;
                            }
                            break;
                        }
                        else if (rightValue is IScriptCustomBinaryOperation rightBinaryOp)
                        {
                            if (rightBinaryOp.TryEvaluate(context, span, op, leftSpan, leftValue, rightSpan, rightValue, out var result))
                            {
                                return result;
                            }
                            break;
                        }
                        else
                        {
                            return CalculateOthers(context, span, op, leftSpan, leftValue, rightSpan, rightValue);
                        }
                    }
                    catch (Exception ex) when(!(ex is ScriptRuntimeException))
                    {
                        throw new ScriptRuntimeException(span, ex.Message);
                    }
            }

            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not supported between `{leftValue}` and `{rightValue}`");
        }

        private static object CalculateEmpty(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object leftValue, SourceSpan rightSpan, object rightValue)
        {

            var leftIsEmptyObject = leftValue == EmptyScriptObject.Default;
            var rightIsEmptyObject = rightValue == EmptyScriptObject.Default;

            // If both are empty, we return false or empty
            if (leftIsEmptyObject && rightIsEmptyObject)
            {
                switch (op)
                {
                    case ScriptBinaryOperator.CompareEqual:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                        return true;
                    case ScriptBinaryOperator.CompareNotEqual:
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        return false;
                }
                return EmptyScriptObject.Default;
            }

            var against = leftIsEmptyObject ? rightValue : leftValue;
            var againstEmpty = context.IsEmpty(span, against);

            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                    return againstEmpty;
                case ScriptBinaryOperator.CompareNotEqual:
                    return againstEmpty is bool ? !(bool)againstEmpty : againstEmpty;
                case ScriptBinaryOperator.CompareGreater:
                case ScriptBinaryOperator.CompareLess:
                    return false;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return againstEmpty;
                case ScriptBinaryOperator.Add:
                case ScriptBinaryOperator.Subtract:
                case ScriptBinaryOperator.Multiply:
                case ScriptBinaryOperator.Power:
                case ScriptBinaryOperator.BinaryOr:
                case ScriptBinaryOperator.BinaryAnd:
                case ScriptBinaryOperator.Divide:
                case ScriptBinaryOperator.DivideRound:
                case ScriptBinaryOperator.Modulus:
                case ScriptBinaryOperator.RangeInclude:
                case ScriptBinaryOperator.RangeExclude:
                    return EmptyScriptObject.Default;

                case ScriptBinaryOperator.LiquidContains:
                case ScriptBinaryOperator.LiquidStartsWith:
                case ScriptBinaryOperator.LiquidEndsWith:
                    return false;
            }

            throw new ScriptRuntimeException(span, $"Operator `{op.ToText()}` is not implemented for `{(leftIsEmptyObject ? "empty" : leftValue)}` / `{(rightIsEmptyObject ? "empty" : rightValue)}`");
        }

        private static object CalculateToString(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object left, SourceSpan rightSpan, object right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return context.ObjectToString(left) + context.ObjectToString(right);
                case ScriptBinaryOperator.Multiply:

                    var spanMultiplier = rightSpan;
                    if (right is string)
                    {
                        var temp = left;
                        left = right;
                        right = temp;
                        spanMultiplier = leftSpan;
                    }

                    // Don't fail when converting
                    int value;
                    try
                    {
                        value = context.ToInt(span, right);
                    }
                    catch
                    {
                        throw new ScriptRuntimeException(spanMultiplier, $"Expecting an integer. The operator `{op.ToText()}` is not supported for the expression. Only working on string x int or int x string"); // unit test: 112-binary-string-error1
                    }
                    var leftText = context.ObjectToString(left);
                    var builder = new StringBuilder();
                    for (int i = 0; i < value; i++)
                    {
                        builder.Append(leftText);
                    }
                    return builder.ToString();

                case ScriptBinaryOperator.CompareEqual:
                    return context.ObjectToString(left) == context.ObjectToString(right);
                case ScriptBinaryOperator.CompareNotEqual:
                    return context.ObjectToString(left) != context.ObjectToString(right);
                case ScriptBinaryOperator.CompareGreater:
                    return context.ObjectToString(left).CompareTo(context.ObjectToString(right)) > 0;
                case ScriptBinaryOperator.CompareLess:
                    return context.ObjectToString(left).CompareTo(context.ObjectToString(right)) < 0;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return context.ObjectToString(left).CompareTo(context.ObjectToString(right)) >= 0;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return context.ObjectToString(left).CompareTo(context.ObjectToString(right)) <= 0;

                case ScriptBinaryOperator.LiquidContains:
                    return context.ObjectToString(left).Contains(context.ObjectToString(right));
                case ScriptBinaryOperator.LiquidStartsWith:
                    return context.ObjectToString(left).StartsWith(context.ObjectToString(right));
                case ScriptBinaryOperator.LiquidEndsWith:
                    return context.ObjectToString(left).EndsWith(context.ObjectToString(right));
                default:
                    break;
            }

            // unit test: 150-range-expression-error1.out.txt
            throw new ScriptRuntimeException(span, $"Operator `{op.ToText()}` is not supported on string objects"); // unit test: 112-binary-string-error2.txt
        }

        private static IEnumerable<object> RangeInclude(long left, long right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (var i = left; i <= right; i++)
                {
                    yield return FitToBestInteger(i);
                }
            }
            else
            {
                for (var i = left; i >= right; i--)
                {
                    yield return FitToBestInteger(i);
                }
            }
        }

        private static IEnumerable<object> RangeExclude(long left, long right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (var i = left; i < right; i++)
                {
                    yield return FitToBestInteger(i);
                }
            }
            else
            {
                for (var i = left; i > right; i--)
                {
                    yield return FitToBestInteger(i);
                }
            }
        }

        private static IEnumerable<object> RangeInclude(BigInteger left, BigInteger right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (var i = left; i <= right; i++)
                {
                    yield return FitToBestInteger(i);
                }
            }
            else
            {
                for (var i = left; i >= right; i--)
                {
                    yield return FitToBestInteger(i);
                }
            }
        }

        private static IEnumerable<object> RangeExclude(BigInteger left, BigInteger right)
        {
            // unit test: 150-range-expression.txt
            if (left < right)
            {
                for (var i = left; i < right; i++)
                {
                    yield return FitToBestInteger(i);
                }
            }
            else
            {
                for (var i = left; i > right; i--)
                {
                    yield return FitToBestInteger(i);
                }
            }
        }

        private static object CalculateOthers(TemplateContext context, SourceSpan span, ScriptBinaryOperator op, SourceSpan leftSpan, object leftValue, SourceSpan rightSpan, object rightValue)
        {
            // Both values are null, applies the relevant binary ops
            if (leftValue == null && rightValue == null)
            {
                switch (op)
                {
                    case ScriptBinaryOperator.CompareEqual:
                        return true;
                    case ScriptBinaryOperator.CompareNotEqual:
                        return false;
                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                        if (context.UseScientific) throw new ScriptRuntimeException(span, $"Both left and right expressions are null. Cannot perform this operation on null values.");
                        return false;
                    case ScriptBinaryOperator.Add:
                    case ScriptBinaryOperator.Subtract:
                    case ScriptBinaryOperator.Multiply:
                    case ScriptBinaryOperator.Divide:
                    case ScriptBinaryOperator.Power:
                    case ScriptBinaryOperator.BinaryOr:
                    case ScriptBinaryOperator.BinaryAnd:
                    case ScriptBinaryOperator.ShiftLeft:
                    case ScriptBinaryOperator.ShiftRight:
                    case ScriptBinaryOperator.DivideRound:
                    case ScriptBinaryOperator.Modulus:
                    case ScriptBinaryOperator.RangeInclude:
                    case ScriptBinaryOperator.RangeExclude:
                        if (context.UseScientific) throw new ScriptRuntimeException(span, $"Both left and right expressions are null. Cannot perform this operation on null values.");
                        return null;
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        return false;
                }
                return null;
            }

            // One value is null
            if (leftValue == null || rightValue == null)
            {
                switch (op)
                {
                    case ScriptBinaryOperator.CompareEqual:
                    	return false;
                    case ScriptBinaryOperator.CompareNotEqual:
                        return true;

                    case ScriptBinaryOperator.CompareGreater:
                    case ScriptBinaryOperator.CompareLess:
                    case ScriptBinaryOperator.CompareGreaterOrEqual:
                    case ScriptBinaryOperator.CompareLessOrEqual:
                    case ScriptBinaryOperator.LiquidContains:
                    case ScriptBinaryOperator.LiquidStartsWith:
                    case ScriptBinaryOperator.LiquidEndsWith:
                        if (context.UseScientific) throw new ScriptRuntimeException(span, $"The {(leftValue == null ? "left" : "right")} expression is null. Cannot perform this operation on a null value.");
                        return false;
                    case ScriptBinaryOperator.Add:
                    case ScriptBinaryOperator.Subtract:
                    case ScriptBinaryOperator.Multiply:
                    case ScriptBinaryOperator.Divide:
                    case ScriptBinaryOperator.DivideRound:
                    case ScriptBinaryOperator.Power:
                    case ScriptBinaryOperator.BinaryOr:
                    case ScriptBinaryOperator.BinaryAnd:
                    case ScriptBinaryOperator.ShiftLeft:
                    case ScriptBinaryOperator.ShiftRight:
                    case ScriptBinaryOperator.Modulus:
                    case ScriptBinaryOperator.RangeInclude:
                    case ScriptBinaryOperator.RangeExclude:
                        if (context.UseScientific) throw new ScriptRuntimeException(span, $"The {(leftValue == null ? "left" : "right")} expression is null. Cannot perform this operation on a null value.");

                        return null;
                }
                return null;
            }

            var leftType = leftValue.GetType();
            var rightType = rightValue.GetType();

            // The order matters: decimal, double, float, long, int
            if (leftType == typeof(decimal))
            {
                var rightDecimal = (decimal)context.ToObject(span, rightValue, typeof(decimal));
                return CalculateDecimal(op, span, (decimal)leftValue, rightDecimal);
            }

            if (rightType == typeof(decimal))
            {
                var leftDecimal = (decimal)context.ToObject(span, leftValue, typeof(decimal));
                return CalculateDecimal(op, span, leftDecimal, (decimal)rightValue);
            }

            if (leftType == typeof(double))
            {
                var rightDouble = (double)context.ToObject(span, rightValue, typeof(double));
                return CalculateDouble(op, span, (double)leftValue, rightDouble);
            }

            if (rightType == typeof(double))
            {
                var leftDouble = (double)context.ToObject(span, leftValue, typeof(double));
                return CalculateDouble(op, span, leftDouble, (double)rightValue);
            }

            if (leftType == typeof(float))
            {
                var rightFloat = (float)context.ToObject(span, rightValue, typeof(float));
                return CalculateFloat(op, span, (float)leftValue, rightFloat);
            }

            if (rightType == typeof(float))
            {
                var leftFloat = (float)context.ToObject(span, leftValue, typeof(float));
                return CalculateFloat(op, span, leftFloat, (float)rightValue);
            }

            if (leftType == typeof(BigInteger))
            {
                var rightBig = (BigInteger)context.ToObject(span, rightValue, typeof(BigInteger));
                return CalculateBigInteger(op, span, (BigInteger)leftValue, rightBig);
            }

            if (rightType == typeof(BigInteger))
            {
                var leftBig = (BigInteger)context.ToObject(span, leftValue, typeof(BigInteger));
                return CalculateBigInteger(op, span, leftBig, (BigInteger)rightValue);
            }

            if (leftType == typeof(long))
            {
                var rightLong = (long)context.ToObject(span, rightValue, typeof(long));
                return CalculateLong(op, span, (long)leftValue, rightLong);
            }

            if (rightType == typeof(long))
            {
                var leftLong = (long)context.ToObject(span, leftValue, typeof(long));
                return CalculateLong(op, span, leftLong, (long)rightValue);
            }

            if (leftType == typeof(ulong))
            {
                var rightLong = (ulong)context.ToObject(span, rightValue, typeof(ulong));
                return CalculateLong(op, span, (ulong)leftValue, rightLong);
            }

            if (rightType == typeof(ulong))
            {
                var leftLong = (ulong)context.ToObject(span, leftValue, typeof(ulong));
                return CalculateLong(op, span, leftLong, (ulong)rightValue);
            }

            if (leftType == typeof(int) || (leftType != null && leftType.IsEnum))
            {
                var rightInt = (int)context.ToObject(span, rightValue, typeof(int));
                return CalculateInt(op, span, (int)leftValue, rightInt);
            }

            if (rightType == typeof(int) || (rightType != null && rightType.IsEnum))
            {
                var leftInt = (int)context.ToObject(span, leftValue, typeof(int));
                return CalculateInt(op, span, leftInt, (int)rightValue);
            }

            if (leftType == typeof(bool))
            {
                var rightBool = (bool)context.ToObject(span, rightValue, typeof(bool));
                return CalculateBool(op, span, (bool)leftValue, rightBool);
            }

            if (rightType == typeof(bool))
            {
                var leftBool = (bool)context.ToObject(span, leftValue, typeof(bool));
                return CalculateBool(op, span, leftBool, (bool)rightValue);
            }

            if (leftType == typeof(DateTime) && rightType == typeof(DateTime))
            {
                return CalculateDateTime(op, span, (DateTime)leftValue, (DateTime)rightValue);
            }

            if (leftType == typeof(DateTime) && rightType == typeof(TimeSpan))
            {
                return CalculateDateTime(op, span, (DateTime)leftValue, (TimeSpan)rightValue);
            }

            //allows to check equality for objects with not only primitive types
            if (op == ScriptBinaryOperator.CompareEqual)
            {
                return leftValue.Equals(rightValue);
            }

            throw new ScriptRuntimeException(span, $"Unsupported types `{leftValue}/{context.GetTypeName(leftValue)}` {op.ToText()} `{rightValue}/{context.GetTypeName(rightValue)}` for binary operation");
        }

        private static object CalculateInt(ScriptBinaryOperator op, SourceSpan span, int left, int right)
        {
            return FitToBestInteger(CalculateLongWithInt(op, span, left, right));
        }

        private static object FitToBestInteger(object value)
        {
            if (value is int) return value;

            if (value is long longValue)
            {
                return FitToBestInteger(longValue);
            }

            if (value is BigInteger bigInt)
            {
                return FitToBestInteger(bigInt);
            }
            return value;
        }

        private static object FitToBestInteger(long longValue)
        {
            if (longValue >= int.MinValue && longValue <= int.MaxValue) return (int)longValue;
            return longValue;
        }

        private static object FitToBestInteger(BigInteger bigInt)
        {
            if (bigInt >= int.MinValue && bigInt <= int.MaxValue) return (int)bigInt;
            if (bigInt >= long.MinValue && bigInt <= long.MaxValue) return (long)bigInt;
            return bigInt;
        }

        /// <summary>
        /// Use this value as a maximum integer
        /// </summary>
        private static readonly BigInteger MaxBigInteger = BigInteger.One << 1024 * 1024;

        private static object CalculateLongWithInt(ScriptBinaryOperator op, SourceSpan span, int leftInt, int rightInt)
        {
            long left = leftInt;
            long right = rightInt;

            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Subtract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (double)left / (double)right;
                case ScriptBinaryOperator.DivideRound:
                    return left / right;

                case ScriptBinaryOperator.ShiftLeft:
                    return (BigInteger)left << (int)right;
                case ScriptBinaryOperator.ShiftRight:
                    return (BigInteger)left >> (int)right;

                case ScriptBinaryOperator.Power:
                    if (right < 0)
                    {
                        return Math.Pow(left, right);
                    }
                    else
                    {
                        return BigInteger.ModPow(left, right, MaxBigInteger);
                    }

                case ScriptBinaryOperator.BinaryOr:
                    return left | right;
                case ScriptBinaryOperator.BinaryAnd:
                    return left & right;

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
                    return new ScriptRange(RangeInclude(left, right));
                case ScriptBinaryOperator.RangeExclude:
                    return new ScriptRange(RangeExclude(left, right));
            }
            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not implemented for long<->long");
        }

        private static object CalculateLong(ScriptBinaryOperator op, SourceSpan span, long left, long right)
        {
            return CalculateBigInteger(op, span, new BigInteger(left), new BigInteger(right));
        }

        private static object CalculateLong(ScriptBinaryOperator op, SourceSpan span, ulong left, ulong right)
        {
            return CalculateBigInteger(op, span, new BigInteger(left), new BigInteger(right));
        }

        private static object CalculateBigInteger(ScriptBinaryOperator op, SourceSpan span, BigInteger left, BigInteger right)
        {
            return FitToBestInteger(CalculateBigIntegerNoFit(op, span, left, right));
        }

        private static object CalculateBigIntegerNoFit(ScriptBinaryOperator op, SourceSpan span, BigInteger left, BigInteger right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Subtract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (double)left / (double)right;
                case ScriptBinaryOperator.DivideRound:
                    return left / right;

                case ScriptBinaryOperator.ShiftLeft:
                    return left << (int)right;
                case ScriptBinaryOperator.ShiftRight:
                    return left >> (int)right;

                case ScriptBinaryOperator.Power:
                    if (right < 0)
                    {
                        return Math.Pow((double)left, (double)right);
                    }
                    else
                    {
                        return BigInteger.ModPow(left, right, MaxBigInteger);
                    }

                case ScriptBinaryOperator.BinaryOr:
                    return left | right;
                case ScriptBinaryOperator.BinaryAnd:
                    return left & right;

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
                    return new ScriptRange(RangeInclude(left, right));
                case ScriptBinaryOperator.RangeExclude:
                    return new ScriptRange(RangeExclude(left, right));
            }
            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not implemented for long<->long");
        }

        private static object CalculateDouble(ScriptBinaryOperator op, SourceSpan span, double left, double right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Subtract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return left / right;
                case ScriptBinaryOperator.DivideRound:
                    return Math.Round(left / right);

                case ScriptBinaryOperator.ShiftLeft:
                    return left * Math.Pow(2, right);

                case ScriptBinaryOperator.ShiftRight:
                    return left / Math.Pow(2, right);

                case ScriptBinaryOperator.Power:
                    return Math.Pow(left, right);

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
            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not implemented for double<->double");
        }

        private static object CalculateDecimal(ScriptBinaryOperator op, SourceSpan span, decimal left, decimal right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Subtract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return left / right;
                case ScriptBinaryOperator.DivideRound:
                    return Math.Round(left / right);

                case ScriptBinaryOperator.ShiftLeft:
                    return left * (decimal) Math.Pow(2, (double) right);
                case ScriptBinaryOperator.ShiftRight:
                    return left / (decimal) Math.Pow(2, (double) right);

                case ScriptBinaryOperator.Power:
                    return (decimal)Math.Pow((double)left, (double)right);

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
            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not implemented for decimal<->decimal");
        }

        private static object CalculateFloat(ScriptBinaryOperator op, SourceSpan span, float left, float right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
                case ScriptBinaryOperator.Subtract:
                    return left - right;
                case ScriptBinaryOperator.Multiply:
                    return left * right;
                case ScriptBinaryOperator.Divide:
                    return (float)left / right;
                case ScriptBinaryOperator.DivideRound:
                    return (float)(int)(left / right);
#if NETSTANDARD2_1
                case ScriptBinaryOperator.Power:
                    return MathF.Pow(left, right);
#else
                case ScriptBinaryOperator.Power:
                    return (float)Math.Pow(left, right);
#endif

#if NETSTANDARD2_1
                case ScriptBinaryOperator.ShiftLeft:
                    return left * (float)MathF.Pow(2.0f, right);

                case ScriptBinaryOperator.ShiftRight:
                    return left / (float)MathF.Pow(2.0f, right);
#else
                case ScriptBinaryOperator.ShiftLeft:
                    return left * (float)Math.Pow(2, right);

                case ScriptBinaryOperator.ShiftRight:
                    return left / (float)Math.Pow(2, right);
#endif

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
            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not implemented for float<->float");
        }

        private static object CalculateDateTime(ScriptBinaryOperator op, SourceSpan span, DateTime left, DateTime right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Subtract:
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

            throw new ScriptRuntimeException(span, $"The operator `{op}` is not supported for DateTime");
        }

        private static object CalculateDateTime(ScriptBinaryOperator op, SourceSpan span, DateTime left, TimeSpan right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return left + right;
            }

            throw new ScriptRuntimeException(span, $"The operator `{op}` is not supported for between <DateTime> and <TimeSpan>");
        }

        private static object CalculateBool(ScriptBinaryOperator op, SourceSpan span, bool left, bool right)
        {
            switch (op)
            {
                case ScriptBinaryOperator.CompareEqual:
                    return left == right;
                case ScriptBinaryOperator.CompareNotEqual:
                    return left != right;
            }
            throw new ScriptRuntimeException(span, $"The operator `{op.ToText()}` is not valid for bool<->bool");
        }
    }
}