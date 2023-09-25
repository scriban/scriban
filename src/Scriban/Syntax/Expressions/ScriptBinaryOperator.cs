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

        InterpBegin,
        InterpEnd,

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
            return op switch
            {
                ScriptBinaryOperator.Add => TokenType.Plus,
                ScriptBinaryOperator.Subtract => TokenType.Minus,
                ScriptBinaryOperator.Divide => TokenType.Divide,
                ScriptBinaryOperator.DivideRound => TokenType.DoubleDivide,
                ScriptBinaryOperator.Multiply => TokenType.Asterisk,
                ScriptBinaryOperator.Modulus => TokenType.Percent,
                ScriptBinaryOperator.RangeInclude => TokenType.DoubleDot,
                ScriptBinaryOperator.RangeExclude => TokenType.DoubleDotLess,
                ScriptBinaryOperator.CompareEqual => TokenType.DoubleEqual,
                ScriptBinaryOperator.CompareNotEqual => TokenType.ExclamationEqual,
                ScriptBinaryOperator.CompareLessOrEqual => TokenType.LessEqual,
                ScriptBinaryOperator.CompareGreaterOrEqual => TokenType.GreaterEqual,
                ScriptBinaryOperator.CompareLess => TokenType.Less,
                ScriptBinaryOperator.CompareGreater => TokenType.Greater,
                ScriptBinaryOperator.And => TokenType.DoubleAmp,
                ScriptBinaryOperator.Or => TokenType.DoubleVerticalBar,
                ScriptBinaryOperator.EmptyCoalescing => TokenType.DoubleQuestion,
                ScriptBinaryOperator.NotEmptyCoalescing => TokenType.QuestionExclamation,
                ScriptBinaryOperator.ShiftLeft => TokenType.DoubleLessThan,
                ScriptBinaryOperator.ShiftRight => TokenType.DoubleGreaterThan,
                ScriptBinaryOperator.Power => TokenType.Caret,
                ScriptBinaryOperator.BinaryAnd => TokenType.Amp,
                ScriptBinaryOperator.BinaryOr => TokenType.VerticalBar,
                ScriptBinaryOperator.InterpBegin => TokenType.OpenInterpBrace,
                ScriptBinaryOperator.InterpEnd => TokenType.CloseInterpBrace,
                _ => throw new ArgumentOutOfRangeException(nameof(op)),
            };
        }


        public static string ToText(this ScriptBinaryOperator op)
        {
            return op switch
            {
                ScriptBinaryOperator.Add => "+",
                ScriptBinaryOperator.Subtract => "-",// The subtract operator requires to be separated by space
                ScriptBinaryOperator.Divide => "/",
                ScriptBinaryOperator.DivideRound => "//",
                ScriptBinaryOperator.Multiply => "*",
                ScriptBinaryOperator.Modulus => "%",
                ScriptBinaryOperator.RangeInclude => "..",
                ScriptBinaryOperator.RangeExclude => "..<",
                ScriptBinaryOperator.CompareEqual => "==",
                ScriptBinaryOperator.CompareNotEqual => "!=",
                ScriptBinaryOperator.CompareLessOrEqual => "<=",
                ScriptBinaryOperator.CompareGreaterOrEqual => ">=",
                ScriptBinaryOperator.CompareLess => "<",
                ScriptBinaryOperator.CompareGreater => ">",
                ScriptBinaryOperator.And => "&&",
                ScriptBinaryOperator.Or => "||",
                ScriptBinaryOperator.EmptyCoalescing => "??",
                ScriptBinaryOperator.NotEmptyCoalescing => "?!",
                ScriptBinaryOperator.ShiftLeft => "<<",
                ScriptBinaryOperator.ShiftRight => ">>",
                ScriptBinaryOperator.InterpBegin => "{",
                ScriptBinaryOperator.InterpEnd => "}",
                ScriptBinaryOperator.LiquidContains => "| string.contains ",
                ScriptBinaryOperator.LiquidStartsWith => "| string.starts_with ",
                ScriptBinaryOperator.LiquidEndsWith => "| string.ends_with ",
                ScriptBinaryOperator.LiquidHasKey => "| object.has_key ",
                ScriptBinaryOperator.LiquidHasValue => "| object.has_value ",
                ScriptBinaryOperator.Power => "^",
                ScriptBinaryOperator.BinaryAnd => "&",
                ScriptBinaryOperator.BinaryOr => "|",
                _ => throw new ArgumentOutOfRangeException(nameof(op)),
            };
        }
    }
}