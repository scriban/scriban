// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;

namespace Scriban.Syntax
{
    public enum ScriptBinaryOperator
    {
        None,

        /// <summary>
        /// The empty coalescing operator ??
        /// </summary>
        EmptyCoalescing,

        Or,

        And,

        BinaryOr,

        BinaryAnd,

        CompareEqual,

        CompareNotEqual,

        CompareLessOrEqual,

        CompareGreaterOrEqual,

        CompareLess,

        CompareGreater,

        LiquidContains,
        LiquidStartsWith,
        LiquidEndsWith,
        LiquidHasKey,
        LiquidHasValue,

        Add,
        Substract,

        Multiply,
        Divide,
        DivideRound,
        Modulus,
        ShiftLeft,
        ShiftRight,

        Power,
        
        RangeInclude,
        RangeExclude,

        Custom,
    }

    public static class ScriptBinaryOperatorExtensions
    {
        public static ScriptToken ToToken(this ScriptBinaryOperator op)
        {
            return new ScriptToken(op.ToText());
        }

        public static string ToText(this ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return "+";
                case ScriptBinaryOperator.Substract:
                    // The substract operator requires to be separated by space
                    return "-";
                case ScriptBinaryOperator.Divide:
                    return "/";
                case ScriptBinaryOperator.DivideRound:
                    return "//";
                case ScriptBinaryOperator.Multiply:
                    return "*";
                case ScriptBinaryOperator.Modulus:
                    return "%";
                case ScriptBinaryOperator.RangeInclude:
                    return "..";
                case ScriptBinaryOperator.RangeExclude:
                    return "..<";
                case ScriptBinaryOperator.CompareEqual:
                    return "==";
                case ScriptBinaryOperator.CompareNotEqual:
                    return "!=";
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return "<=";
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return ">=";
                case ScriptBinaryOperator.CompareLess:
                    return "<";
                case ScriptBinaryOperator.CompareGreater:
                    return ">";
                case ScriptBinaryOperator.And:
                    return "&&";
                case ScriptBinaryOperator.Or:
                    return "||";
                case ScriptBinaryOperator.EmptyCoalescing:
                    return "??";
                case ScriptBinaryOperator.ShiftLeft:
                    return "<<";
                case ScriptBinaryOperator.ShiftRight:
                    return ">>";

                case ScriptBinaryOperator.LiquidContains:
                    return "| string.contains ";
                case ScriptBinaryOperator.LiquidStartsWith:
                    return "| string.starts_with ";
                case ScriptBinaryOperator.LiquidEndsWith:
                    return "| string.ends_with ";
                case ScriptBinaryOperator.LiquidHasKey:
                    return "| object.has_key ";
                case ScriptBinaryOperator.LiquidHasValue:
                    return "| object.has_value ";
                case ScriptBinaryOperator.Power:
                    return "^";
                case ScriptBinaryOperator.BinaryAnd:
                    return "&";
                case ScriptBinaryOperator.BinaryOr:
                    return "|";
                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }
        }
    }
}