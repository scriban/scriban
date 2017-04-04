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
                new Token(TokenType.NewLine, new TextPosition(4,0, 4), new TextPosition(4,0, 4)), // Only first line is kept
                new Token(TokenType.CodeExit, new TextPosition(9,3, 0), new TextPosition(10,3, 1)),
                Token.Eof
            }, tokens);

            VerifyTokenGetText(tokens, text);
        }

        [Test]
        public void ParseLogicalOperators()
        {
            VerifyCodeBlock("{{ ! && || }}",
                new Token(TokenType.Not, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                new Token(TokenType.And, new TextPosition(5, 0, 5), new TextPosition(6, 0, 6)),
                new Token(TokenType.Or, new TextPosition(8, 0, 8), new TextPosition(9, 0, 9))
                );
        }


        [Test]
        public void ParseEscapeRaw()
        {
            {
                var text = "{%{}%}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>() { Token.Eof, }, tokens);
            }

            {
                var text = "{%{ }%}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    Token.Eof,
                }, tokens);
            }

            {
                var text = "{%{{{}}}%}"; // The raw should be {{}}
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(3, 0, 3), new TextPosition(6, 0, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                var text = "{%%{}%}}%%}"; // The raw should be }%}
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(4, 0, 4), new TextPosition(6, 0, 6)),
                    Token.Eof,
                }, tokens);
            }
        }

        [Test]
        public void ParseWithoutSpaces()
        {
            {
                //          0123456
                var text = " {{~ }}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(1, 0, 1), new TextPosition(3, 0, 3)),
                    new Token(TokenType.CodeExit, new TextPosition(5, 0, 5), new TextPosition(6, 0, 6)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          012345
                var text = "{{ ~}} \n       \n      \n";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.CodeEnter, new TextPosition(0, 0, 0), new TextPosition(1, 0, 1)),
                    new Token(TokenType.CodeExit, new TextPosition(3, 0, 3), new TextPosition(5, 0, 5)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          012345678
                var text = " {%{~ }%}";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(5, 0, 5), new TextPosition(5, 0, 5)),
                    Token.Eof,
                }, tokens);
            }
            {
                //          01234567
                var text = "{%{ ~}%} \n       \n      \n";
                var tokens = ParseTokens(text);
                Assert.AreEqual(new List<Token>()
                {
                    new Token(TokenType.Raw, new TextPosition(3, 0, 3), new TextPosition(3, 0, 3)),
                    Token.Eof,
                }, tokens);
            }
        }

        [Test]
        public void ParseSimpleTokens()
        {
            VerifySimpleTokens(new Dictionary<string, TokenType>()
            {
                {"@", TokenType.Arroba},
                {"^", TokenType.Caret},
                {"*", TokenType.Multiply},
                {"+", TokenType.Plus},
                {"-", TokenType.Minus},
                {"/", TokenType.Divide},
                {"//", TokenType.DoubleDivide},
                {"%", TokenType.Modulus},
                {"=", TokenType.Equal},
                {"!", TokenType.Not},
                {"|", TokenType.Pipe},
                {",", TokenType.Comma},
                {".", TokenType.Dot},
                {"(", TokenType.OpenParent},
                {")", TokenType.CloseParent},
                {"[", TokenType.OpenBracket},
                {"]", TokenType.CloseBracket},
                {"<", TokenType.CompareLess},
                {">", TokenType.CompareGreater},
                {"==", TokenType.CompareEqual},
                {">=", TokenType.CompareGreaterOrEqual},
                {"<=", TokenType.CompareLessOrEqual},
                {"&&", TokenType.And},
                {"??", TokenType.EmptyCoalescing},
                {"||", TokenType.Or},
                {"..", TokenType.DoubleDot},
                {"..<", TokenType.DoubleDotLess},
            });
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
        public void ParseCommentSingleLine()
        {
            var comment = "{{# This is a comment}}";
            VerifyCodeBlock(comment, new Token(TokenType.Comment, new TextPosition(2, 0, 2), new TextPosition(comment.Length-3, 0, comment.Length-3)) );
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

        private void VerifySimpleTokens(Dictionary<string, TokenType> simpleTokens)
        {
            foreach (var token in simpleTokens)
            {
                var text = "{{ " + token.Key + " }}";
                VerifyCodeBlock(text, new Token(token.Value, new TextPosition(3, 0, 3), new TextPosition(3 + token.Key.Length - 1, 0, 3 + token.Key.Length - 1)) );
            }
        }

        private List<Token> ParseTokens(string text)
        {
            var lexer = new Lexer(text);
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

            var tokens = ParseTokens(text);
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
                    Assert.AreEqual(token.Type.ToText(), tokenText, $"Invalid captured text found for standard token [{token.Type}] while parsing: {text}");
                }
            }
        }

    }
}