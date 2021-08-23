// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace Scriban.Parsing
{
    /// <summary>
    /// An enumeration to categorize tokens.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum TokenType
    {
        Invalid,

        FrontMatterMarker,

        /// <summary>Token "{{"</summary>
        CodeEnter,

        /// <summary>Token "{%"</summary>
        LiquidTagEnter,

        /// <summary>Token "}}"</summary>
        CodeExit,

        /// <summary>Token "%}"</summary>
        LiquidTagExit,

        Raw,
        Escape,

        EscapeEnter,
        EscapeExit,

        NewLine,

        Whitespace,

        WhitespaceFull,

        Comment,

        CommentMulti,

        /// <summary>
        /// An identifier starting by a $
        /// </summary>
        IdentifierSpecial,

        /// <summary>
        /// An identifier
        /// </summary>
        Identifier,

        /// <summary>
        /// An integer (int, long...)
        /// </summary>
        Integer,

        /// <summary>
        /// A Hexadecimal integer (int, long...)
        /// </summary>
        HexaInteger,

        /// <summary>
        /// A binary integer (int, long...)
        /// </summary>
        BinaryInteger,

        /// <summary>
        /// A floating point number
        /// </summary>
        Float,

        /// <summary>
        /// A string
        /// </summary>
        String,

        /// <summary>
        /// An implicit string with quotes
        /// </summary>
        ImplicitString,

        /// <summary>
        /// A verbatim string
        /// </summary>
        VerbatimString,

        /// <summary>Token ";"</summary>
        SemiColon,

        /// <summary>Token "@"</summary>
        Arroba,

        /// <summary>Token "^"</summary>
        Caret,

        /// <summary>Token "^^"</summary>
        DoubleCaret,

        /// <summary>Token ":"</summary>
        Colon,

        /// <summary>Token "="</summary>
        Equal,

        /// <summary>Token "|"</summary>
        VerticalBar, // |

        /// <summary>Token "|>"</summary>
        PipeGreater, // |>

        /// <summary>Token "!"</summary>
        Exclamation, // !

        /// <summary>Token "&amp;&amp;"</summary>
        DoubleAmp, // &&

        /// <summary>Token "||"</summary>
        DoubleVerticalBar, // ||

        /// <summary>Token "&amp;"</summary>
        Amp, // &

        /// <summary>Token "?"</summary>
        Question,

        /// <summary>Token "??"</summary>
        DoubleQuestion,

        /// <summary>Token "?."</summary>
        QuestionDot,

        /// <summary>Token "?!"</summary>
        QuestionExclamation,

        /// <summary>Token "=="</summary>
        DoubleEqual,

        /// <summary>Token "!="</summary>
        ExclamationEqual,

        /// <summary>Token "&lt;"</summary>
        Less,

        /// <summary>Token ">"</summary>
        Greater,

        /// <summary>Token "&lt;="</summary>
        LessEqual,

        /// <summary>Token ">="</summary>
        GreaterEqual,

        /// <summary>Token "/"</summary>
        Divide,

        /// <summary>Token "/="</summary>
        DivideEqual,

        /// <summary>Token "//"</summary>
        DoubleDivide,

        /// <summary>Token "//="</summary>
        DoubleDivideEqual,

        /// <summary>Token "*"</summary>
        Asterisk,

        /// <summary>Token "*="</summary>
        AsteriskEqual,

        /// <summary>Token "+"</summary>
        Plus,

        /// <summary>Token "+="</summary>
        PlusEqual,

        /// <summary>Token "++"</summary>
        DoublePlus,

        /// <summary>Token "-"</summary>
        Minus,

        /// <summary>Token "-="</summary>
        MinusEqual,

        /// <summary>Token "--"</summary>
        DoubleMinus,

        /// <summary>Token "%"</summary>
        Percent,

        /// <summary>Token "%="</summary>
        PercentEqual,

        /// <summary>Token "&lt;&lt;"</summary>
        DoubleLessThan,

        /// <summary>Token ">>"</summary>
        DoubleGreaterThan,

        /// <summary>Token ","</summary>
        Comma,

        /// <summary>Token "."</summary>
        Dot,

        /// <summary>Token ".."</summary>
        DoubleDot,

        /// <summary>Token "..."</summary>
        TripleDot,

        /// <summary>Token "..&lt;"</summary>
        DoubleDotLess,

        /// <summary>Token "("</summary>
        OpenParen,

        /// <summary>Token ")"</summary>
        CloseParen,

        /// <summary>Token "{"</summary>
        OpenBrace,

        /// <summary>Token "}"</summary>
        CloseBrace,

        /// <summary>Token "["</summary>
        OpenBracket,

        /// <summary>Token "]"</summary>
        CloseBracket,

        /// <summary>
        /// Custom token
        /// </summary>
        Custom,
        Custom1,
        Custom2,
        Custom3,
        Custom4,
        Custom5,
        Custom6,
        Custom7,
        Custom8,
        Custom9,

        Eof,
    }
}
