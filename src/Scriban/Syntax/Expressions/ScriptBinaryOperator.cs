// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using Scriban.Parsing;

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptBinaryOperator
    {
        None,

        /// <summary>
        /// The empty coalescing operator ??
        /// </summary>
        EmptyCoalescing,

        /// <summary>
        /// The not empty coalescing operator ?!
        /// </summary>
        NotEmptyCoalescing,

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
        Subtract,

        [Obsolete]
        Substract = Subtract,

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

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class ScriptBinaryOperatorExtensions
    {
        public static ScriptToken ToToken(this ScriptBinaryOperator op)
        {
            return new ScriptToken(op.ToTokenType());
        }

      public static TokenType ToTokenType(this ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return TokenType.Plus;
                case ScriptBinaryOperator.Subtract:
                    return TokenType.Minus;
                case ScriptBinaryOperator.Divide:
                    return TokenType.Divide;
                case ScriptBinaryOperator.DivideRound:
                    return TokenType.DoubleDivide;
                case ScriptBinaryOperator.Multiply:
                    return TokenType.Asterisk;
                case ScriptBinaryOperator.Modulus:
                    return TokenType.Percent;
                case ScriptBinaryOperator.RangeInclude:
                    return TokenType.DoubleDot;
                case ScriptBinaryOperator.RangeExclude:
                    return TokenType.DoubleDotLess;
                case ScriptBinaryOperator.CompareEqual:
                    return TokenType.DoubleEqual;
                case ScriptBinaryOperator.CompareNotEqual:
                    return TokenType.ExclamationEqual;
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return TokenType.LessEqual;
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return TokenType.GreaterEqual;
                case ScriptBinaryOperator.CompareLess:
                    return TokenType.Less;
                case ScriptBinaryOperator.CompareGreater:
                    return TokenType.Greater;
                case ScriptBinaryOperator.And:
                    return TokenType.DoubleAmp;
                case ScriptBinaryOperator.Or:
                    return TokenType.DoubleVerticalBar;
                case ScriptBinaryOperator.EmptyCoalescing:
                    return TokenType.DoubleQuestion;
                case ScriptBinaryOperator.NotEmptyCoalescing:
                    return TokenType.QuestionExclamation;
                case ScriptBinaryOperator.ShiftLeft:
                    return TokenType.DoubleLessThan;
                case ScriptBinaryOperator.ShiftRight:
                    return TokenType.DoubleGreaterThan;
                case ScriptBinaryOperator.Power:
                    return TokenType.Caret;
                case ScriptBinaryOperator.BinaryAnd:
                    return TokenType.Amp;
                case ScriptBinaryOperator.BinaryOr:
                    return TokenType.VerticalBar;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op));
            }
        }


        public static string ToText(this ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return "+";
                case ScriptBinaryOperator.Subtract:
                    // The subtract operator requires to be separated by space
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
                case ScriptBinaryOperator.NotEmptyCoalescing:
                    return "?!";
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