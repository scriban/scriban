// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Scriban;
using Scriban.Parsing;

namespace Scriban.Tests
{
    [TestFixture]
    public class TestLexer
    {
        // TOOD: Add parse invalid character
        // TODO: Add parse invalid number
        // TODO: Add parse invalid escape in string
        // TODO: Add parse unexpected eof in string
        // TODO: Add tests for FrontMatterMarker

        [Test]
        public void ParseRaw()
        {
            var text = "text";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.Raw, new TextPosition(0,0, 0), new TextPosition(3,0, 3)),
                Token.Eof
            }, tokens);
            Assert.AreEqual(text, tokens[0].GetText(text));
        }


        [Test]
        public void ParseComment()
        {
            var text = "# This is a comment {{ and some interpolated}} and another text";
            var lexer = new Lexer(text, options: new LexerOptions() { Lang = ScriptLang.Default, Mode = ScriptMode.ScriptOnly });

            var tokens = lexer.ToList<Token>();
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.Comment, new TextPosition(0,0, 0), new TextPosition(62,0, 62)),
                Token.Eof
            }, tokens);
            Assert.AreEqual(text, tokens[0].GetText(text));
        }

        [Test]
        public void ParseLiquidComment()
        {
            string text;
            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345
            text = "{% comment %}This is a comment{% endcomment %}";
            var tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(12, 0, 12)),
                new Token(TokenType.CommentMulti, new TextPosition(13, 0, 13), new TextPosition(29, 0, 29)),
                new Token(TokenType.CodeExit, new TextPosition(30, 0, 30), new TextPosition(45, 0, 45)),
                Token.Eof
            }, tokens);

            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345
            text = "{%comment%}This is a comment{%endcomment%}";
            tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(10, 0, 10)),
                new Token(TokenType.CommentMulti, new TextPosition(11, 0, 11), new TextPosition(27, 0, 27)),
                new Token(TokenType.CodeExit, new TextPosition(28, 0, 28), new TextPosition(41, 0, 41)),
                Token.Eof
            }, tokens);

            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345678
            text = "{%    comment%}This is a comment{%   endcomment%}";
            tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(14, 0, 14)),
                new Token(TokenType.CommentMulti, new TextPosition(15, 0, 15), new TextPosition(31, 0, 31)),
                new Token(TokenType.CodeExit, new TextPosition(32, 0, 32), new TextPosition(48, 0, 48)),
                Token.Eof
            }, tokens);

            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345678
            text = "{%comment   %}This is a comment{%endcomment   %}";
            tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(13, 0, 13)),
                new Token(TokenType.CommentMulti, new TextPosition(14, 0, 14), new TextPosition(30, 0, 30)),
                new Token(TokenType.CodeExit, new TextPosition(31, 0, 31), new TextPosition(47, 0, 47)),
                Token.Eof
            }, tokens);
        }

        [Test]
        public void ParseLiquidRaw()
        {
            string text;
            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345
            text = "{% raw %}This is a raw{% endraw %}";
            var tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(8, 0, 8)),
                new Token(TokenType.Escape, new TextPosition(9, 0, 9), new TextPosition(21, 0, 21)),
                new Token(TokenType.EscapeExit, new TextPosition(22, 0, 22), new TextPosition(33, 0, 33)),
                Token.Eof
            }, tokens);

            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345
            text = "{%raw%}This is a raw{%endraw%}";
            tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(6, 0, 6)),
                new Token(TokenType.Escape, new TextPosition(7, 0, 7), new TextPosition(19, 0, 19)),
                new Token(TokenType.EscapeExit, new TextPosition(20, 0, 20), new TextPosition(29, 0, 29)),
                Token.Eof
            }, tokens);

            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345678
            text = "{%    raw%}This is a raw{%   endraw%}";
            tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(10, 0, 10)),
                new Token(TokenType.Escape, new TextPosition(11, 0, 11), new TextPosition(23, 0, 23)),
                new Token(TokenType.EscapeExit, new TextPosition(24, 0, 24), new TextPosition(36, 0, 36)),
                Token.Eof
            }, tokens);

            //      0         1         2         3         4
            //      0123456789012345678901234567890123456789012345678
            text = "{%raw   %}This is a raw{%endraw   %}";
            tokens = ParseTokens(text, isLiquid: true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(9, 0, 9)),
                new Token(TokenType.Escape, new TextPosition(10, 0, 10), new TextPosition(22, 0, 22)),
                new Token(TokenType.EscapeExit, new TextPosition(23, 0, 23), new TextPosition(35, 0, 35)),
                Token.Eof
            }, tokens);
        }

        private void CheckLiquidRawOrComment(bool isComment, string text, string inner, bool withLeft = false, bool withRight = false)
        {
            if (withLeft)
            {
                text = "abc " + text;
            }
            if (withRight)
            {
                text = text + " abc";
            }
            var startInner = text.IndexOf(inner, StringComparison.Ordinal);
            var endInner = startInner + inner.Length - 1;

            var tokens = ParseTokens(text, true);

            var expectedTokens = new List<Token>();
            int innerIndex = 0;
            if (isComment)
            {
                var startCodeEnter = text.IndexOf("{%");
                expectedTokens.Add(new Token(TokenType.CodeEnter, new TextPosition(startCodeEnter, 0, startCodeEnter), new TextPosition(startCodeEnter+1, 0, startCodeEnter+1)));
                innerIndex = 1;
            }

            expectedTokens.Add(new Token(isComment ? TokenType.CommentMulti : TokenType.Escape, new TextPosition(startInner, 0, startInner), new TextPosition(endInner, 0, endInner)));

            if (isComment)
            {
                var startCodeExit = text.LastIndexOf("%}");
                expectedTokens.Add(new Token(TokenType.CodeExit, new TextPosition(startCodeExit, 0, startCodeExit), new TextPosition(startCodeExit + 1, 0, startCodeExit + 1)));
            }
            else
            {
                expectedTokens.Add(new Token(TokenType.EscapeExit, new TextPosition(startInner, 0, startInner), new TextPosition(endInner, 0, endInner)));
            }
            expectedTokens.Add(Token.Eof);

            var tokensIt = tokens.GetEnumerator();
            foreach (var expectedToken in expectedTokens)
            {
                if (tokensIt.MoveNext())
                {
                    var token = tokensIt.Current;
                    if (expectedToken.Type == TokenType.EscapeExit)
                    {
                        Assert.AreEqual(expectedToken.Type, token.Type);
                    }
                    else
                    {
                        Assert.AreEqual(expectedToken, token);
                    }
                }
            }

            Assert.AreEqual(expectedTokens.Count, tokens.Count);
            Assert.AreEqual(inner, tokens[innerIndex].GetText(text));
        }

        [Test]
        public void ParseJekyllLiquidInclude()
        {
            //          0         1         2
            //          012345678901234567890123456
            var text = "{% include toto/tata.htm %}";
            var tokens = ParseTokens(text, true, true, true);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.LiquidTagEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                new Token(TokenType.Whitespace, new TextPosition(2, 0, 2), new TextPosition(2, 0, 2)),
                new Token(TokenType.Identifier, new TextPosition(3, 0, 3), new TextPosition(9, 0, 9)),
                new Token(TokenType.Whitespace, new TextPosition(10, 0, 10), new TextPosition(10, 0, 10)),
                new Token(TokenType.ImplicitString, new TextPosition(11, 0, 11), new TextPosition(23, 0, 23)),
                new Token(TokenType.Whitespace, new TextPosition(24, 0, 24), new TextPosition(24, 0, 24)),
                new Token(TokenType.LiquidTagExit, new TextPosition(25, 0, 25), new TextPosition(26, 0, 26)),
                Token.Eof
            }, tokens);
        }

        [Test]
        public void ParseRawWithNewLines()
        {
            var text = "te\r\n\r\n\r\n";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.Raw, new TextPosition(0,0, 0), new TextPosition(7,2, 1)),
                Token.Eof
            }, tokens);
            Assert.AreEqual(text, tokens[0].GetText(text));
        }

        [Test]
        public void ParseRawWithCodeExit()
        {
            var text = "te}}";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.Raw, new TextPosition(0,0, 0), new TextPosition(3,0, 3)),
                Token.Eof
            }, tokens);
            Assert.AreEqual(text, tokens[0].GetText(text));
        }

        [Test]
        public void ParseCodeEnterAndCodeExit()
        {
            var text = "{{}}";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0,0, 0), new TextPosition(1,0, 1)),
                new Token(TokenType.CodeExit, new TextPosition(2,0, 2), new TextPosition(3,0, 3)),
                Token.Eof
            }, tokens);

            VerifyTokenGetText(tokens, text);
        }

        [Test]
        public void ParseCodeEnterAndCodeExitWithNewLineAndTextInRaw()
        {
            // In this case a raw token is generated
            //          01234 5 6
            var text = "{{}}\r\na";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0,0, 0), new TextPosition(1,0, 1)),
                new Token(TokenType.CodeExit, new TextPosition(2,0, 2), new TextPosition(3,0, 3)),
                new Token(TokenType.Raw, new TextPosition(4,0, 4), new TextPosition(6,1, 0)),  // skip \r\n
                Token.Eof
            }, tokens);

            VerifyTokenGetText(tokens, text);
        }

        [Test]
        public void ParseCodeEnterAndCodeExitWithSpaces()
        {
            // whitespace don't generate tokens inside a code block
            var text = @"{{    }}";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0,0, 0), new TextPosition(1,0, 1)),
                new Token(TokenType.CodeExit, new TextPosition(6,0, 6), new TextPosition(7,0, 7)),
                Token.Eof
            }, tokens);

            VerifyTokenGetText(tokens, text);
        }

        [Test]
        public void ParseCodeEnterAndCodeExitWithNewLines()
        {
            // A New line is always generated only for a first statement (even empty), but subsequent empty white-space/new-lines are skipped
            var text = "{{ \r\n\r\n\r\n}}";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0,0, 0), new TextPosition(1,0, 1)),
                new Token(TokenType.NewLine, new TextPosition(4,0, 4), new TextPosition(8,2, 1)), // Whe whole whitespaces with newlines
                new Token(TokenType.CodeExit, new TextPosition(9,3, 0), new TextPosition(10,3, 1)),
                Token.Eof
            }, tokens);

            VerifyTokenGetText(tokens, text);
        }

        [Test]
        public void ParseLogicalOperators()
        {
            VerifyCodeBlock("{{ ! && || }}",
                new Token(TokenType.Exclamation, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                new Token(TokenType.DoubleAmp, new TextPosition(5, 0, 5), new TextPosition(6, 0, 6)),
                new Token(TokenType.DoubleVerticalBar, new TextPosition(8, 0, 8), new TextPosition(9, 0, 9))
                );
        }


        [Test]
        public void ParseEscapeRaw()
        {
            {
                var text = "{%{}%}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    // Empty escape
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)),
                    new Token(TokenType.EscapeExit, new TextPosition(3, 0, 3), new TextPosition(5, 0, 5)),
                    Token.Eof,
                }, tokens);
            }

            {
                var text = "{%{ }%}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)),
                    new Token(TokenType.Escape, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.EscapeExit, new TextPosition(4, 0, 4), new TextPosition(6, 0, 6)),
                    Token.Eof,
                }, tokens);
            }

            {
                var text = "{%{{{}}}%}"; // The raw should be {{}}
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)),
                    new Token(TokenType.Escape, new TextPosition(3, 0, 3), new TextPosition(6, 0, 6)),
                    new Token(TokenType.EscapeExit, new TextPosition(7, 0, 7), new TextPosition(9, 0, 9)),
                    Token.Eof,
                }, tokens);
            }
            {
                var text = "{%%{}%}}%%}"; // The raw should be }%}
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(3, 0, 3)),
                    new Token(TokenType.Escape, new TextPosition(4, 0, 4), new TextPosition(6, 0, 6)),
                    new Token(TokenType.EscapeExit, new TextPosition(7, 0, 7), new TextPosition(10, 0, 10)),
                    Token.Eof,
                }, tokens);
            }
        }

        [Test]
        public void ParseEscapeAndSpaces()
        {
            {
                //                     1
                //          01234567 8901234567
                var text = "{%{ }%}\n    {{~ }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)),
                    new Token(TokenType.Escape, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.EscapeExit, new TextPosition(4, 0, 4), new TextPosition(6, 0, 6)),
                    new Token(TokenType.Raw, new TextPosition(7, 0, 7), new TextPosition(7, 0, 7)),
                    new Token(TokenType.Whitespace, new TextPosition(8, 1, 0), new TextPosition(11, 1, 3)),
                    new Token(TokenType.CodeEnter, new TextPosition(12, 1, 4), new TextPosition(14, 1, 6)),
                    new Token(TokenType.CodeExit, new TextPosition(16, 1, 8), new TextPosition(17, 1, 9)),
                    Token.Eof,
                }, tokens);
            }
        }

        [Test]
        public void ParseWithoutSpaces()
        {
            {
                //                       1
                //          0 12 3 4567890
                var text = "\n \r\n {{~ }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(0, 0, 0), new TextPosition(3, 1, 2)),
                    new Token(TokenType.Whitespace, new TextPosition(4, 2, 0), new TextPosition(4, 2, 0)),
                    new Token(TokenType.CodeEnter, new TextPosition(5, 2, 1), new TextPosition(7, 2, 3)),
                    new Token(TokenType.CodeExit, new TextPosition(9, 2, 5), new TextPosition(10, 2, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                //                       1
                //          0 12 3 4567890
                var text = "\n \r\n {{- }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.WhitespaceFull, new TextPosition(0, 0, 0), new TextPosition(4, 2, 0)),
                    new Token(TokenType.CodeEnter, new TextPosition(5, 2, 1), new TextPosition(7, 2, 3)),
                    new Token(TokenType.CodeExit, new TextPosition(9, 2, 5), new TextPosition(10, 2, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          01234567
                var text = "a {{~ }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)),
                    new Token(TokenType.Whitespace, new TextPosition(1, 0, 1), new TextPosition(1, 0, 1)),
                    new Token(TokenType.CodeEnter, new TextPosition(2, 0, 2), new TextPosition(4, 0, 4)),
                    new Token(TokenType.CodeExit, new TextPosition(6, 0, 6), new TextPosition(7, 0, 7)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          0          1          2
                //          01234567 89012345 6789012
                var text = "{{ ~}} \n      \r\n      \n";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                    new Token(TokenType.CodeExit, new TextPosition(3, 0, 3), new TextPosition(5, 0, 5)),
                    new Token(TokenType.Whitespace, new TextPosition(6, 0, 6), new TextPosition(7, 0, 7)),
                    new Token(TokenType.Raw, new TextPosition(8, 1, 0), new TextPosition(22, 2, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          0          1          2
                //          01234567 89012345 6789012
                var text = "{{ -}} \n      \r\n      \n";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                    new Token(TokenType.CodeExit, new TextPosition(3, 0, 3), new TextPosition(5, 0, 5)),
                    new Token(TokenType.WhitespaceFull, new TextPosition(6, 0, 6), new TextPosition(22, 2, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          012345678
                var text = " {%{~ }%}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Whitespace, new TextPosition(0, 0, 0), new TextPosition(0, 0, 0)),
                    new Token(TokenType.EscapeEnter, new TextPosition(1, 0, 1), new TextPosition(4, 0, 4)),
                    new Token(TokenType.Escape, new TextPosition(5, 0, 5), new TextPosition(5, 0, 5)),
                    new Token(TokenType.EscapeExit, new TextPosition(6, 0, 6), new TextPosition(8, 0, 8)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          0123456789 01234567 8901234
                var text = "{%{ ~}%} \n       \n      \n";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)),
                    new Token(TokenType.Escape, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.EscapeExit, new TextPosition(4, 0, 4), new TextPosition(7, 0, 7)),
                    new Token(TokenType.Whitespace, new TextPosition(8, 0, 8), new TextPosition(9, 0, 9)),
                    new Token(TokenType.Raw, new TextPosition(10, 1, 0), new TextPosition(24, 2, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          0123456789 01234567 8901234
                var text = "{%{ -}%} \n       \n      \n";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.EscapeEnter, new TextPosition(0, 0, 0), new TextPosition(2, 0, 2)),
                    new Token(TokenType.Escape, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.EscapeExit, new TextPosition(4, 0, 4), new TextPosition(7, 0, 7)),
                    new Token(TokenType.WhitespaceFull, new TextPosition(8, 0, 8), new TextPosition(24, 2, 6)),
                    Token.Eof,
                }, tokens);
            }
        }

        [Test]
        public void ParseSimpleTokens()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"\x01", TokenType.Invalid},
                {"@", TokenType.Arroba},
                {"^", TokenType.Caret},
                {"^^", TokenType.DoubleCaret},
                {"*", TokenType.Asterisk},
                {"+", TokenType.Plus},
                {"-", TokenType.Minus},
                {"/", TokenType.Divide},
                {"//", TokenType.DoubleDivide},
                {"%", TokenType.Percent},
                {"=", TokenType.Equal},
                {"!", TokenType.Exclamation},
                {"|", TokenType.VerticalBar},
                {"|>", TokenType.PipeGreater},
                {",", TokenType.Comma},
                {".", TokenType.Dot},
                {"(", TokenType.OpenParen},
                {")", TokenType.CloseParen},
                {"[", TokenType.OpenBracket},
                {"]", TokenType.CloseBracket},
                {"<", TokenType.Less},
                {">", TokenType.Greater},
                {"==", TokenType.DoubleEqual},
                {">=", TokenType.GreaterEqual},
                {"<=", TokenType.LessEqual},
                {"&", TokenType.Amp},
                {"&&", TokenType.DoubleAmp},
                {"??", TokenType.DoubleQuestion},
                {"?!", TokenType.QuestionExclamation},
                {"||", TokenType.DoubleVerticalBar},
                {"..", TokenType.DoubleDot},
                {"..<", TokenType.DoubleDotLess},
            });
            //{ "{", TokenType.OpenBrace}, // We cannot test them individually here as they are used in the lexer to match braces and better handle closing code }}
            //{ "}", TokenType.CloseBrace},
        }

        [Test]
        public void ParseLiquidTokens()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"\x01", TokenType.Invalid},
                {"@", TokenType.Invalid},
                {"^", TokenType.Invalid},
                {"*", TokenType.Invalid},
                {"+", TokenType.Invalid},
                {"-", TokenType.Minus},
                {"/", TokenType.Invalid},
                {"%", TokenType.Invalid},
                {"=", TokenType.Equal},
                {"!", TokenType.Invalid},
                {"|", TokenType.VerticalBar},
                {",", TokenType.Comma},
                {".", TokenType.Dot},
                {"(", TokenType.OpenParen},
                {")", TokenType.CloseParen},
                {"[", TokenType.OpenBracket},
                {"]", TokenType.CloseBracket},
                {"<", TokenType.Less},
                {">", TokenType.Greater},
                {"==", TokenType.DoubleEqual},
                {"!=", TokenType.ExclamationEqual},
                {">=", TokenType.GreaterEqual},
                {"<=", TokenType.LessEqual},
                {"?", TokenType.Question},
                {"&", TokenType.Invalid},
                {"..", TokenType.DoubleDot}
            }, true);
            //{ "{", TokenType.OpenBrace}, // We cannot test them individualy here as they are used in the lexer to match braces and better handle closing code }}
            //{ "}", TokenType.CloseBrace},
        }

        [Test]
        public void ParseIdentifier()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"_", TokenType.Identifier},
                {"t_st", TokenType.Identifier},
                {"test", TokenType.Identifier},
                {"t999", TokenType.Identifier},
                {"_est", TokenType.Identifier},
                {"_999", TokenType.Identifier},
                {"$", TokenType.IdentifierSpecial},
                {"$$", TokenType.IdentifierSpecial},
                {"$0", TokenType.IdentifierSpecial},
                {"$test", TokenType.IdentifierSpecial},
                {"$t999", TokenType.IdentifierSpecial},
                {"$_est", TokenType.IdentifierSpecial},
                {"$_999", TokenType.IdentifierSpecial},
            });
        }

        [Test]
        public void ParseNumbers()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"1", TokenType.Integer},
                {"10", TokenType.Integer},
                {"100000", TokenType.Integer},
                {"1e1", TokenType.Integer},
                {"1.", TokenType.Float},
                {"1.e1", TokenType.Float},
                {"1.0", TokenType.Float},
                {"10.0", TokenType.Float},
                {"10.01235", TokenType.Float},
                {"10.01235e1", TokenType.Float},
                {"10.01235e-1", TokenType.Float},
                {"10.01235e+1", TokenType.Float},
            });
        }

        [Test]
        public void ParseNumberInvalid()
        {
            var lexer = new Lexer("{{ 1e }}");
            var tokens = lexer.ToList();
            Assert.True(lexer.HasErrors);
            StringAssert.Contains("Expecting at least one digit after the exponent", lexer.Errors.First().Message);
        }

        [Test]
        public void ParseCommentSingleLine()
        {
            var comment = "{{# This is a comment}}";
            VerifyCodeBlock(comment, new Token(TokenType.Comment, new TextPosition(2, 0, 2), new TextPosition(comment.Length-3, 0, comment.Length-3)) );
        }

        [Test]
        public void ParseCommentSingleLineEof()
        {
            var text = "{{# This is a comment";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>()
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                new Token(TokenType.Comment, new TextPosition(2, 0, 2), new TextPosition(20, 0, 20)),
                Token.Eof,
            }, tokens);
        }

        [Test]
        public void ParseCommentMultiLineEof()
        {
            var text = "{{## This is a comment";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>()
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                new Token(TokenType.CommentMulti, new TextPosition(2, 0, 2), new TextPosition(21, 0, 21)),
                Token.Eof,
            }, tokens);
        }

        [Test]
        public void ParseCommentMultiLineOnSingleLine()
        {
            {
                var comment = @"{{## This a multiline comment on a single line ##}}";
                VerifyCodeBlock(comment, new Token(TokenType.CommentMulti, new TextPosition(2, 0, 2), new TextPosition(comment.Length - 3, 0, comment.Length - 3)));
            }

            {
                var comment = @"{{## This a multiline comment on a single line without a ending}}";
                VerifyCodeBlock(comment, new Token(TokenType.CommentMulti, new TextPosition(2, 0, 2), new TextPosition(comment.Length - 3, 0, comment.Length - 3)));
            }
        }

        [Test]
        public void ParseCommentMultiLine()
        {
            var text = @"{{## This a multiline
comment on a
single line
##}}";
            VerifyCodeBlock(text,
                new Token(TokenType.CommentMulti, new TextPosition(2, 0, 2), new TextPosition(text.Length - 3, 3, 1)),
                // Handle eplicit code exit matching when we have multiline
                new Token(TokenType.CodeExit, new TextPosition(text.Length-2, 3, 2), new TextPosition(text.Length-1, 3, 3))
                );
        }

        [Test]
        public void ParseStringSingleLine()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {@"""This a string on a single line""", TokenType.String},
                {@"""This a string with an escape \"" and escape \\ """, TokenType.String},
                {@"'This a string with an escape \' and inlined "" '", TokenType.String},
                {@"'This a string with \0 \b \n \u0000 \uFFFF \x00 \xff'", TokenType.String},
//                {@"'This a single string spanning over multilines with \\
//This is the continuation'", TokenType.String},
            });
        }

        [TestCase('"')]
        [TestCase('\'')]
        public void ParseInterpolatedStringTokens(char quoteType)
        {
            string strWithInterpolatedExpressions = $@"{{{{ ${quoteType}Begin {{2}} middle {{5}} end{quoteType} }}}}";
            var tokens = ParseTokens(strWithInterpolatedExpressions);
            Assert.AreEqual(new List<Token>
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                new Token(TokenType.BeginInterpolatedString, new TextPosition(3, 0, 3), new TextPosition(11, 0, 11)),
                new Token(TokenType.OpenInterpolatedBrace, new TextPosition(11, 0, 11), new TextPosition(11, 0, 11)),
                new Token(TokenType.Integer, new TextPosition(12, 0, 12), new TextPosition(12, 0, 12)),
                new Token(TokenType.CloseInterpolatedBrace, new TextPosition(13, 0, 13), new TextPosition(13, 0, 13)),
                new Token(TokenType.ContinuationInterpolatedString, new TextPosition(13, 0, 13), new TextPosition(22, 0, 22)),
                new Token(TokenType.OpenInterpolatedBrace, new TextPosition(22, 0, 22), new TextPosition(22, 0, 22)),
                new Token(TokenType.Integer, new TextPosition(23, 0, 23), new TextPosition(23, 0, 23)),
                new Token(TokenType.CloseInterpolatedBrace, new TextPosition(24, 0, 24), new TextPosition(24, 0, 24)),
                new Token(TokenType.EndingInterpolatedString, new TextPosition(24, 0, 24), new TextPosition(29, 0, 29)),
                new Token(TokenType.CodeExit, new TextPosition(31, 0, 31), new TextPosition(32, 0, 32)),
                Token.Eof
            }, tokens);
        }

        [Test]
        public void ParseUnbalancedCloseBrace()
        {
            {
                var lexer = new Lexer("{{ } }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected } while no matching", lexer.Errors.First().Message);
            }
        }

        [Test]
        public void ParseStringInvalid()
        {
            {
                var lexer = new Lexer("{{ '\\u' }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected hex number", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ '\\u1' }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected hex number", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ '\\u12' }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected hex number", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ '\\u123' }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected hex number", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ '\\x' }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected hex number", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ '\\x1' }}");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected hex number", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ '");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected end of file while parsing a string not terminated", lexer.Errors.First().Message);
            }
            {
                var lexer = new Lexer("{{ `");
                var tokens = lexer.ToList();
                Assert.True(lexer.HasErrors);
                StringAssert.Contains("Unexpected end of file while parsing a verbatim string not terminated by", lexer.Errors.First().Message);
            }
        }

        [Test]
        public void ParseTestNewLine()
        {
            {
                //          0
                //          01234 567
                var text = "{{ a\r }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                    new Token(TokenType.Identifier, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.NewLine, new TextPosition(4, 0, 4), new TextPosition(5, 1, 0)),
                    new Token(TokenType.CodeExit, new TextPosition(6, 1, 1), new TextPosition(7, 1, 2)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          0
                //          01234 567
                var text = "{{ a\n }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                    new Token(TokenType.Identifier, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.NewLine, new TextPosition(4, 0, 4), new TextPosition(5, 1, 0)),
                    new Token(TokenType.CodeExit, new TextPosition(6, 1, 1), new TextPosition(7, 1, 2)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          0
                //          01234 5 678
                var text = "{{ a\r\n }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                    new Token(TokenType.Identifier, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    new Token(TokenType.NewLine, new TextPosition(4, 0, 4), new TextPosition(6, 1, 0)),
                    new Token(TokenType.CodeExit, new TextPosition(7, 1, 1), new TextPosition(8, 1, 2)),
                    Token.Eof,
                }, tokens);
            }
        }

        [Test]
        public void ParseStringEscapeEol()
        {
            //          0           1
            //          012345678 9 0123
            var text = "{{ 'text\\\n' }}";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>()
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                new Token(TokenType.String, new TextPosition(3, 0, 3), new TextPosition(10, 1, 0)),
                new Token(TokenType.CodeExit, new TextPosition(12, 1, 2), new TextPosition(13, 1, 3)),
                Token.Eof,
            }, tokens);
        }

        [Test]
        public void ParseStringEscapeEol2()
        {
            //          0           1
            //          012345678 9 0 1234
            var text = "{{ 'text\\\r\n' }}";
            var tokens = ParseTokens(text);
            Assert.AreEqual(new List<Token>()
            {
                new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                new Token(TokenType.String, new TextPosition(3, 0, 3), new TextPosition(11, 1, 0)),
                new Token(TokenType.CodeExit, new TextPosition(13, 1, 2), new TextPosition(14, 1, 3)),
                Token.Eof,
            }, tokens);
        }

        [Test]
        public void ParseStringMultiLine()
        {
            var text = @"{{""This a string on
a single line
""}}";
            VerifyCodeBlock(text,
                new Token(TokenType.String, new TextPosition(2, 0, 2), new TextPosition(text.Length - 3, 2, 0)),
                // Handle eplicit code exit matching when we have multiline
                new Token(TokenType.CodeExit, new TextPosition(text.Length - 2, 2, 1), new TextPosition(text.Length - 1, 2, 2))
                );
        }


        [Test]
        public void ParseIdentifiers()
        {
            var text = @"{{
with this
    xxx = 5
end}}This is a test";
            var lexer = new Lexer(text);
            Assert.False((bool)lexer.HasErrors);
            var tokens = Enumerable.ToList<Token>(lexer);

            // TODO Add testd
        }

        private void VerifySimpleTokens(Dictionary<string, TokenType> simpleTokens, bool isLiquid = false)
        {
            foreach (var token in simpleTokens)
            {
                var text = "{{ " + token.Key + " }}";
                VerifyCodeBlock(text, isLiquid, new Token(token.Value, new TextPosition(3, 0, 3), new TextPosition(3 + token.Key.Length - 1, 0, 3 + token.Key.Length - 1)) );
            }
        }

        private List<Token> ParseTokens(string text, bool isLiquid = false, bool keepTrivia = false, bool isJekyll = false)
        {
            var lexer = new Lexer(text, options: new LexerOptions() { Lang = isLiquid ? ScriptLang.Liquid : ScriptLang.Default, KeepTrivia = keepTrivia, EnableIncludeImplicitString = isJekyll});
            foreach (var error in lexer.Errors)
            {
                Console.WriteLine(error);
            }
            Assert.False((bool)lexer.HasErrors);
            var tokens = Enumerable.ToList<Token>(lexer);
            return tokens;
        }

        private void VerifyCodeBlock(string text, params Token[] expectedTokens)
        {
            VerifyCodeBlock(text, false, expectedTokens);
        }

        private void VerifyCodeBlock(string text, bool isLiquid, params Token[] expectedTokens)
        {
            var expectedTokenList = new List<Token>();
            expectedTokenList.Add(new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)));
            expectedTokenList.AddRange(expectedTokens);
            if (expectedTokenList[expectedTokenList.Count - 1].Type != TokenType.CodeExit)
            {
                // Add last token automatically if not already here
                expectedTokenList.Add(new Token(TokenType.CodeExit,
                    new TextPosition(text.Length - 2, 0, text.Length - 2),
                    new TextPosition(text.Length - 1, 0, text.Length - 1)));

            }
            expectedTokenList.Add(Token.Eof);

            var tokens = ParseTokens(text, isLiquid);
            Assert.AreEqual(expectedTokenList, tokens, $"Unexpected error while parsing: {text}");

            VerifyTokenGetText(tokens, text);
        }

        private static void VerifyTokenGetText(List<Token> tokens, string text)
        {
            foreach (var token in tokens)
            {
                var tokenText = token.GetText(text);
                if (token.Type.HasText())
                {
                    Assert.AreEqual(token.Type.ToText(), tokenText, $"Invalid captured text found for standard token `{token.Type}` while parsing: {text}");
                }
            }
        }

    }
}