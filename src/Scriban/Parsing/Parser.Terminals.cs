// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Parsing
{
    public partial class Parser
    {
        private ScriptExpression ParseVariableOrLiteral()
        {
            ScriptExpression literal = null;
            switch (Current.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                    literal = ParseVariable();
                    break;
                case TokenType.Integer:
                    literal = ParseInteger();
                    break;
                case TokenType.Float:
                    literal = ParseFloat();
                    break;
                case TokenType.String:
                    literal = ParseString();
                    break;
                case TokenType.ImplicitString:
                    literal = ParseImplicitString();
                    break;
                case TokenType.VerbatimString:
                    literal = ParseVerbatimString();
                    break;
                default:
                    LogError(Current, "Unexpected token found `{GetAsText(Current)}` while parsing a variable or literal");
                    break;
            }
            return literal;
        }

        private ScriptLiteral ParseFloat()
        {
            var literal = Open<ScriptLiteral>();

            var text = GetAsText(Current);
            double floatResult;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out floatResult))
            {
                literal.Value = floatResult;
            }
            else
            {
                LogError($"Unable to parse double value `{text}`");
            }

            NextToken(); // Skip the float
            return Close(literal);
        }

        private ScriptLiteral ParseImplicitString()
        {
            var literal = Open<ScriptLiteral>();
            literal.Value = GetAsText(Current);
            Close(literal);
            NextToken();
            return literal;
        }

        private ScriptLiteral ParseInteger()
        {
            var literal = Open<ScriptLiteral>();

            var text = GetAsText(Current);
            long result;
            if (!long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                LogError($"Unable to parse the integer {text}");
            }

            if (result >= int.MinValue && result <= int.MaxValue)
            {
                literal.Value = (int) result;
            }
            else
            {
                literal.Value = result;
            }

            NextToken(); // Skip the literal
            return Close(literal);
        }

        private ScriptLiteral ParseString()
        {
            var literal = Open<ScriptLiteral>();
            var text = _lexer.Text;
            var builder = new StringBuilder(Current.End.Offset - Current.Start.Offset - 1);

            literal.StringQuoteType =
                _lexer.Text[Current.Start.Offset] == '\''
                    ? ScriptLiteralStringQuoteType.SimpleQuote
                    : ScriptLiteralStringQuoteType.DoubleQuote;

            var end = Current.End.Offset;
            for (int i = Current.Start.Offset + 1; i < end; i++)
            {
                var c = text[i];
                // Handle escape characters
                if (text[i] == '\\')
                {
                    i++;
                    switch (text[i])
                    {
                        case '0':
                            builder.Append((char) 0);
                            break;
                        case '\n':
                            break;
                        case '\r':
                            i++; // skip next \n that was validated by the lexer
                            break;
                        case '\'':
                            builder.Append('\'');
                            break;
                        case '"':
                            builder.Append('"');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'v':
                            builder.Append('\v');
                            break;
                        case 'u':
                        {
                            i++;
                            int value = 0;
                            if (i < text.Length) value = text[i++].HexToInt();
                            if (i < text.Length) value = (value << 4) | text[i++].HexToInt();
                            if (i < text.Length) value = (value << 4) | text[i++].HexToInt();
                            if (i < text.Length) value = (value << 4) | text[i].HexToInt();

                            // Is it correct?
                            builder.Append(ConvertFromUtf32(value));
                            break;
                        }
                        case 'x':
                        {
                            i++;
                            int value = 0;
                            if (i < text.Length) value = text[i++].HexToInt();
                            if (i < text.Length) value = (value << 4) | text[i].HexToInt();
                            builder.Append((char) value);
                            break;
                        }

                        default:
                            // This should not happen as the lexer is supposed to prevent this
                            LogError($"Unexpected escape character `{text[i]}` in string");
                            break;
                    }
                }
                else
                {
                    builder.Append(c);
                }
            }
            literal.Value = builder.ToString();

            NextToken();
            return Close(literal);
        }

        private ScriptExpression ParseVariable()
        {
            var currentToken = Current;
            var currentSpan = CurrentSpan;
            var endSpan = currentSpan;
            var text = GetAsText(currentToken);

            // Return ScriptLiteral for null, true, false
            // Return ScriptAnonymousFunction 
            switch (text)
            {
                case "null":
                    var nullValue = Open<ScriptLiteral>();
                    NextToken();
                    return Close(nullValue);
                case "true":
                    var trueValue = Open<ScriptLiteral>();
                    trueValue.Value = true;
                    NextToken();
                    return Close(trueValue);
                case "false":
                    var falseValue = Open<ScriptLiteral>();
                    falseValue.Value = false;
                    NextToken();
                    return Close(falseValue);
                case "do":
                    var functionExp = Open<ScriptAnonymousFunction>();
                    functionExp.Function = ParseFunctionStatement(true);
                    var func = Close(functionExp);
                    return func;
                case "this":
                    if (!_isLiquid)
                    {
                        var thisExp = Open<ScriptThisExpression>();
                        NextToken();
                        return Close(thisExp);
                    }
                    break;
            }

            // Keeps trivia before this token
            List<ScriptTrivia> triviasBefore = null;
            if (_isKeepTrivia && _trivias.Count > 0)
            {
                triviasBefore = new List<ScriptTrivia>();
                triviasBefore.AddRange(_trivias);
                _trivias.Clear();
            }

            NextToken();
            var scope = ScriptVariableScope.Global;
            if (text.StartsWith("$"))
            {
                scope = ScriptVariableScope.Local;
                text = text.Substring(1);

                // Convert $0, $1... $n variable into $[0] $[1]...$[n] variables
                int index;
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    var indexerExpression = new ScriptIndexerExpression
                    {
                        Span = currentSpan,

                        Target = new ScriptVariableLocal(ScriptVariable.Arguments.Name)
                        {
                            Span = currentSpan
                        },

                        Index = new ScriptLiteral() {Span = currentSpan, Value = index}
                    };

                    if (_isKeepTrivia)
                    {
                        if (triviasBefore != null)
                        {
                            indexerExpression.Target.AddTrivias(triviasBefore, true);
                        }
                        FlushTrivias(indexerExpression.Index, false);
                    }

                    return indexerExpression;
                }
            }
            else if (text == "for" || text == "while" || text == "tablerow" || (_isLiquid && (text == "forloop" || text == "tablerowloop")))
            {
                if (Current.Type == TokenType.Dot)
                {
                    scope = ScriptVariableScope.Loop;
                    var token = PeekToken();
                    if (token.Type == TokenType.Identifier)
                    {
                        //endSpan = GetSpanForToken(token);
                        var loopVariableText = GetAsText(token);
                        if (_isLiquid)
                        {
                            switch (loopVariableText)
                            {
                                case "first":
                                case "last":
                                case "index0":
                                case "rindex0":
                                case "index":
                                case "rindex":
                                case "length":
                                    break;
                                case "col":
                                    if (text != "tablerowloop")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, $"The loop variable <{text}.col> is invalid");
                                    }
                                    break;

                                default:
                                    LogError(currentToken, $"The liquid loop variable <{text}.{loopVariableText}> is not supported");
                                    break;
                            }

                            if (text == "forloop") text = "for";
                            else if (text == "tablerowloop") text = "tablerow";
                        }
                        else
                        {
                            switch (loopVariableText)
                            {
                                // supported by both for and while
                                case "first":
                                case "even":
                                case "odd":
                                case "index":
                                    break;
                                case "last":
                                case "changed":
                                case "length":
                                case "rindex":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, $"The loop variable <while.{loopVariableText}> is invalid");
                                    }
                                    break;
                                case "col":
                                    if (text != "tablerow")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, $"The loop variable <{text}.col> is invalid");
                                    }
                                    break;
                                default:
                                    // unit test: 108-variable-loop-error1.txt
                                    LogError(currentToken, $"The loop variable <{text}.{loopVariableText}> is not supported");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        LogError(currentToken, $"Invalid token `{GetAsText(Current)}`. The loop variable <{text}> dot must be followed by an identifier");
                    }
                }
            }
            else if (_isLiquid && text == "continue")
            {
                scope = ScriptVariableScope.Local;
            }

            var result = ScriptVariable.Create(text, scope);
            result.Span = new SourceSpan
            {
                FileName = currentSpan.FileName,
                Start = currentSpan.Start,
                End = endSpan.End
            };

            // A liquid variable can have `-` in its identifier
            // If this is the case, we need to translate it to `this["this"]` instead
            if (_isLiquid && text.IndexOf('-') >= 0)
            {
                var newExp = new ScriptIndexerExpression
                {
                    Target = new ScriptThisExpression()
                    {
                        Span = result.Span
                    },
                    Index = new ScriptLiteral(text)
                    {
                        Span = result.Span
                    },
                    Span = result.Span
                };

                // Flush any trivias after
                if (_isKeepTrivia)
                {
                    if (triviasBefore != null)
                    {
                        newExp.Target.AddTrivias(triviasBefore, true);
                    }
                    FlushTrivias(newExp, false);
                }
                // Return the expression
                return newExp;
            }

            if (_isKeepTrivia)
            {
                // Flush any trivias after
                if (triviasBefore != null)
                {
                    result.AddTrivias(triviasBefore, true);
                }
                FlushTrivias(result, false);
            }
            return result;
        }

        private ScriptLiteral ParseVerbatimString()
        {
            var literal = Open<ScriptLiteral>();
            var text = _lexer.Text;

            literal.StringQuoteType = ScriptLiteralStringQuoteType.Verbatim;

            StringBuilder builder = null;

            // startOffset start at the first character (`a` in the string `abc`)
            var startOffset = Current.Start.Offset + 1;
            // endOffset is at the last character (`c` in the string `abc`)
            var endOffset = Current.End.Offset - 1;

            int offset = startOffset;
            while (true)
            {
                // Go to the next escape (the lexer verified that there was a following `)
                var nextOffset = text.IndexOf("`", offset, endOffset - offset + 1, StringComparison.OrdinalIgnoreCase);
                if (nextOffset < 0)
                {
                    break;
                }
                if (builder == null)
                {
                    builder = new StringBuilder(endOffset - startOffset + 1);
                }
                builder.Append(text.Substring(offset, nextOffset - offset + 1));
                // Skip the escape ``
                offset = nextOffset + 2;
            }
            if (builder != null)
            {
                var count = endOffset - offset + 1;
                if (count > 0)
                {
                    builder.Append(text.Substring(offset, count));
                }
                literal.Value = builder.ToString();
            }
            else
            {
                literal.Value = text.Substring(offset, endOffset - offset + 1);
            }

            NextToken();
            return Close(literal);
        }

        private static string ConvertFromUtf32(int utf32)
        {
            if (utf32 < 65536)
                return ((char)utf32).ToString();
            utf32 -= 65536;
            return new string(new char[2]
            {
                (char) (utf32 / 1024 + 55296),
                (char) (utf32 % 1024 + 56320)
            });
        }

        private static bool IsVariableOrLiteral(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Identifier:
                case TokenType.IdentifierSpecial:
                case TokenType.Integer:
                case TokenType.Float:
                case TokenType.String:
                case TokenType.ImplicitString:
                case TokenType.VerbatimString:
                    return true;
            }
            return false;
        }
    }
}