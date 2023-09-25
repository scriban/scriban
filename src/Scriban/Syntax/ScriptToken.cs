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
        public static ScriptToken SemiColon() => new(TokenType.SemiColon);
        public static ScriptToken Arroba() => new(TokenType.Arroba);
        public static ScriptToken Caret() => new(TokenType.Caret);
        public static ScriptToken DoubleCaret() => new(TokenType.DoubleCaret);
        public static ScriptToken Colon() => new(TokenType.Colon);
        public static ScriptToken Equal() => new(TokenType.Equal);
        public static ScriptToken Pipe() => new(TokenType.VerticalBar);
        public static ScriptToken PipeGreater() => new(TokenType.PipeGreater);
        public static ScriptToken Exclamation() => new(TokenType.Exclamation);
        public static ScriptToken DoubleAmp() => new(TokenType.DoubleAmp);
        public static ScriptToken DoublePipe() => new(TokenType.DoubleVerticalBar);
        public static ScriptToken Amp() => new(TokenType.Amp);
        public static ScriptToken Question() => new(TokenType.Question);
        public static ScriptToken DoubleQuestion() => new(TokenType.DoubleQuestion);
        public static ScriptToken QuestionExclamation() => new(TokenType.QuestionExclamation);
        public static ScriptToken CompareEqual() => new(TokenType.DoubleEqual);
        public static ScriptToken CompareNotEqual() => new(TokenType.ExclamationEqual);
        public static ScriptToken CompareLess() => new(TokenType.Less);
        public static ScriptToken CompareGreater() => new(TokenType.Greater);
        public static ScriptToken CompareLessOrEqual() => new(TokenType.LessEqual);
        public static ScriptToken CompareGreaterOrEqual() => new(TokenType.GreaterEqual);
        public static ScriptToken Divide() => new(TokenType.Divide);
        public static ScriptToken DivideEqual() => new(TokenType.DivideEqual);
        public static ScriptToken DoubleDivide() => new(TokenType.DoubleDivide);
        public static ScriptToken DoubleDivideEqual() => new(TokenType.DoubleDivideEqual);
        public static ScriptToken Star() => new(TokenType.Asterisk);
        public static ScriptToken StarEqual() => new(TokenType.AsteriskEqual);
        public static ScriptToken Plus() => new(TokenType.Plus);
        public static ScriptToken PlusEqual() => new(TokenType.PlusEqual);
        public static ScriptToken Minus() => new(TokenType.Minus);
        public static ScriptToken MinusEqual() => new(TokenType.MinusEqual);
        public static ScriptToken Modulus() => new(TokenType.Percent);
        public static ScriptToken ModulusEqual() => new(TokenType.PercentEqual);
        public static ScriptToken DoubleLess() => new(TokenType.DoubleLessThan);
        public static ScriptToken DoubleGreater() => new(TokenType.DoubleGreaterThan);
        public static ScriptToken Comma() => new(TokenType.Comma);
        public static ScriptToken Dot() => new(TokenType.Dot);
        public static ScriptToken DoubleDot() => new(TokenType.DoubleDot);
        public static ScriptToken TripleDot() => new(TokenType.TripleDot);
        public static ScriptToken DoubleDotLess() => new(TokenType.DoubleDotLess);
        public static ScriptToken OpenParen() => new(TokenType.OpenParen);
        public static ScriptToken CloseParen() => new(TokenType.CloseParen);
        public static ScriptToken OpenBrace() => new(TokenType.OpenBrace);
        public static ScriptToken CloseBrace() => new(TokenType.CloseBrace);
        public static ScriptToken OpenBracket() => new(TokenType.OpenBracket);
        public static ScriptToken CloseBracket() => new(TokenType.CloseBracket);
        public static ScriptToken OpenInterpBrace() => new(TokenType.OpenInterpBrace);
        public static ScriptToken CloseInterpBrace() => new(TokenType.CloseInterpBrace);

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