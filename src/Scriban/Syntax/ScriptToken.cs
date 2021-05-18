// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    /// <summary>
    /// A verbatim node (use for custom parsing).
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptToken : ScriptVerbatim
    {
        public static ScriptToken SemiColon() => new ScriptToken(TokenType.SemiColon);
        public static ScriptToken Arroba() => new ScriptToken(TokenType.Arroba);
        public static ScriptToken Caret() => new ScriptToken(TokenType.Caret);
        public static ScriptToken DoubleCaret() => new ScriptToken(TokenType.DoubleCaret);
        public static ScriptToken Colon() => new ScriptToken(TokenType.Colon);
        public static ScriptToken Equal() => new ScriptToken(TokenType.Equal);
        public static ScriptToken Pipe() => new ScriptToken(TokenType.VerticalBar);
        public static ScriptToken PipeGreater() => new ScriptToken(TokenType.PipeGreater);
        public static ScriptToken Exclamation() => new ScriptToken(TokenType.Exclamation);
        public static ScriptToken DoubleAmp() => new ScriptToken(TokenType.DoubleAmp);
        public static ScriptToken DoublePipe() => new ScriptToken(TokenType.DoubleVerticalBar);
        public static ScriptToken Amp() => new ScriptToken(TokenType.Amp);
        public static ScriptToken Question() => new ScriptToken(TokenType.Question);
        public static ScriptToken DoubleQuestion() => new ScriptToken(TokenType.DoubleQuestion);
        public static ScriptToken QuestionExclamation() => new ScriptToken(TokenType.QuestionExclamation);
        public static ScriptToken CompareEqual() => new ScriptToken(TokenType.DoubleEqual);
        public static ScriptToken CompareNotEqual() => new ScriptToken(TokenType.ExclamationEqual);
        public static ScriptToken CompareLess() => new ScriptToken(TokenType.Less);
        public static ScriptToken CompareGreater() => new ScriptToken(TokenType.Greater);
        public static ScriptToken CompareLessOrEqual() => new ScriptToken(TokenType.LessEqual);
        public static ScriptToken CompareGreaterOrEqual() => new ScriptToken(TokenType.GreaterEqual);
        public static ScriptToken Divide() => new ScriptToken(TokenType.Divide);
        public static ScriptToken DivideEqual() => new ScriptToken(TokenType.DivideEqual);
        public static ScriptToken DoubleDivide() => new ScriptToken(TokenType.DoubleDivide);
        public static ScriptToken DoubleDivideEqual() => new ScriptToken(TokenType.DoubleDivideEqual);
        public static ScriptToken Star() => new ScriptToken(TokenType.Asterisk);
        public static ScriptToken StarEqual() => new ScriptToken(TokenType.AsteriskEqual);
        public static ScriptToken Plus() => new ScriptToken(TokenType.Plus);
        public static ScriptToken PlusEqual() => new ScriptToken(TokenType.PlusEqual);
        public static ScriptToken Minus() => new ScriptToken(TokenType.Minus);
        public static ScriptToken MinusEqual() => new ScriptToken(TokenType.MinusEqual);
        public static ScriptToken Modulus() => new ScriptToken(TokenType.Percent);
        public static ScriptToken ModulusEqual() => new ScriptToken(TokenType.PercentEqual);
        public static ScriptToken DoubleLess() => new ScriptToken(TokenType.DoubleLessThan);
        public static ScriptToken DoubleGreater() => new ScriptToken(TokenType.DoubleGreaterThan);
        public static ScriptToken Comma() => new ScriptToken(TokenType.Comma);
        public static ScriptToken Dot() => new ScriptToken(TokenType.Dot);
        public static ScriptToken DoubleDot() => new ScriptToken(TokenType.DoubleDot);
        public static ScriptToken TripleDot() => new ScriptToken(TokenType.TripleDot);
        public static ScriptToken DoubleDotLess() => new ScriptToken(TokenType.DoubleDotLess);
        public static ScriptToken OpenParen() => new ScriptToken(TokenType.OpenParen);
        public static ScriptToken CloseParen() => new ScriptToken(TokenType.CloseParen);
        public static ScriptToken OpenBrace() => new ScriptToken(TokenType.OpenBrace);
        public static ScriptToken CloseBrace() => new ScriptToken(TokenType.CloseBrace);
        public static ScriptToken OpenBracket() => new ScriptToken(TokenType.OpenBracket);
        public static ScriptToken CloseBracket() => new ScriptToken(TokenType.CloseBracket);

        public ScriptToken()
        {
        }

        public ScriptToken(TokenType type)
        {
            TokenType = type;
            Value = type.ToText();
        }

        public TokenType TokenType { get; set; }
    }
}