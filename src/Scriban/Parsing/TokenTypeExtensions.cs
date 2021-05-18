// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

namespace Scriban.Parsing
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    static class TokenTypeExtensions
    {
        public static bool HasText(this TokenType type)
        {
            return ToText(type) != null;
        }

        public static string ToText(this TokenType type)
        {
            return type switch
            {
                TokenType.CodeEnter => "{{",
                TokenType.LiquidTagEnter => "{%",
                TokenType.CodeExit => "}}",
                TokenType.LiquidTagExit => "%}",
                TokenType.SemiColon => ";",
                TokenType.Arroba => "@",
                TokenType.Caret => "^",
                TokenType.DoubleCaret => "^^",
                TokenType.Colon => ":",
                TokenType.Equal => "=",
                TokenType.VerticalBar => "|",
                TokenType.PipeGreater => "|>",
                TokenType.Exclamation => "!",
                TokenType.DoubleAmp => "&&",
                TokenType.DoubleVerticalBar => "||",
                TokenType.Amp => "&",
                TokenType.Question => "?",
                TokenType.QuestionDot => "?.",
                TokenType.DoubleQuestion => "??",
                TokenType.QuestionExclamation => "?!",
                TokenType.DoubleEqual => "==",
                TokenType.ExclamationEqual => "!=",
                TokenType.Less => "<",
                TokenType.Greater => ">",
                TokenType.LessEqual => "<=",
                TokenType.GreaterEqual => ">=",
                TokenType.Divide => "/",
                TokenType.DivideEqual => "/=",
                TokenType.DoubleDivide => "//",
                TokenType.DoubleDivideEqual => "//=",
                TokenType.Asterisk => "*",
                TokenType.AsteriskEqual => "*=",
                TokenType.Plus => "+",
                TokenType.PlusEqual => "+=",
                TokenType.Minus => "-",
                TokenType.MinusEqual => "-=",
                TokenType.DoublePlus => "++",
                TokenType.DoubleMinus => "--",
                TokenType.Percent => "%",
                TokenType.PercentEqual => "%=",
                TokenType.DoubleLessThan => "<<",
                TokenType.DoubleGreaterThan => ">>",
                TokenType.Comma => ",",
                TokenType.Dot => ".",
                TokenType.DoubleDot => "..",
                TokenType.TripleDot => "...",
                TokenType.DoubleDotLess => "..<",
                TokenType.OpenParen => "(",
                TokenType.CloseParen => ")",
                TokenType.OpenBrace => "{",
                TokenType.CloseBrace => "}",
                TokenType.OpenBracket => "[",
                TokenType.CloseBracket => "]",
                _ => null
            };
        }
    }
}