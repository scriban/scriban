// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
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
            if (double.TryParse(text, out floatResult))
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
            if (!long.TryParse(text, out result))
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
                            var value = (text[i++].HexToInt() << 12) +
                                        (text[i++].HexToInt() << 8) +
                                        (text[i++].HexToInt() << 4) +
                                        text[i].HexToInt();
                            // Is it correct?
                            builder.Append(ConvertFromUtf32(value));
                            break;
                        }
                        case 'x':
                        {
                            i++;
                            var value = (text[i++].HexToInt() << 4) +
                                        text[i++].HexToInt();
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
                if (int.TryParse(text, out index))
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
                    NextToken();
                    if (Current.Type == TokenType.Identifier)
                    {
                        endSpan = CurrentSpan;
                        var loopVariableText = GetAsText(Current);
                        NextToken();

                        scope = ScriptVariableScope.Loop;
                        if (_isLiquid)
                        {
                            switch (loopVariableText)
                            {
                                case "first":
                                    text = ScriptVariable.LoopFirst.Name;
                                    break;
                                case "last":
                                    text = ScriptVariable.LoopLast.Name;
                                    break;
                                case "index0":
                                    text = ScriptVariable.LoopIndex.Name;
                                    break;
                                case "rindex0":
                                    text = ScriptVariable.LoopRIndex.Name;
                                    break;
                                case "rindex":
                                case "index":
                                    // Because forloop.index is 1 based index, we need to create a binary expression
                                    // to support it here
                                    bool isrindex = loopVariableText == "rindex";

                                    var nested = new ScriptNestedExpression()
                                    {
                                        Expression = new ScriptBinaryExpression()
                                        {
                                            Operator = ScriptBinaryOperator.Add,
                                            Left = new ScriptVariableLoop(isrindex ? ScriptVariable.LoopRIndex.Name : ScriptVariable.LoopIndex.Name)
                                            {
                                                Span = currentSpan
                                            },
                                            Right = new ScriptLiteral(1)
                                            {
                                                Span = currentSpan
                                            },
                                            Span = currentSpan
                                        },
                                        Span = currentSpan
                                    };

                                    if (_isKeepTrivia)
                                    {
                                        if (triviasBefore != null)
                                        {
                                            nested.AddTrivias(triviasBefore, true);
                                        }
                                        FlushTrivias(nested, false);
                                    }
                                    return nested;
                                case "length":
                                    text = ScriptVariable.LoopLength.Name;
                                    break;
                                case "col":
                                    if (text != "tablerowloop")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, $"The loop variable <{text}.col> is invalid");
                                    }
                                    text = ScriptVariable.TableRowCol.Name;
                                    break;

                                default:
                                    text = text + "." + loopVariableText;
                                    LogError(currentToken, $"The liquid loop variable <{text}> is not supported");
                                    break;
                            }
                        }
                        else
                        {
                            switch (loopVariableText)
                            {
                                case "first":
                                    text = ScriptVariable.LoopFirst.Name;
                                    break;
                                case "last":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, "The loop variable <while.last> is invalid");
                                    }
                                    text = ScriptVariable.LoopLast.Name;
                                    break;
                                case "changed":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, "The loop variable <while.changed> is invalid");
                                    }
                                    text = ScriptVariable.LoopChanged.Name;
                                    break;
                                case "length":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, "The loop variable <while.length> is invalid");
                                    }
                                    text = ScriptVariable.LoopLength.Name;
                                    break;
                                case "even":
                                    text = ScriptVariable.LoopEven.Name;
                                    break;
                                case "odd":
                                    text = ScriptVariable.LoopOdd.Name;
                                    break;
                                case "index":
                                    text = ScriptVariable.LoopIndex.Name;
                                    break;
                                case "rindex":
                                    if (text == "while")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, "The loop variable <while.rindex> is invalid");
                                    }
                                    text = ScriptVariable.LoopRIndex.Name;
                                    break;
                                case "col":
                                    if (text != "tablerow")
                                    {
                                        // unit test: 108-variable-loop-error2.txt
                                        LogError(currentToken, $"The loop variable <{text}.col> is invalid");
                                    }
                                    text = ScriptVariable.TableRowCol.Name;
                                    break;
                                default:
                                    text = text + "." + loopVariableText;
                                    // unit test: 108-variable-loop-error1.txt
                                    LogError(currentToken, $"The loop variable <{text}> is not supported");
                                    break;
                            }
                        }

                        // We no longer checks at parse time usage of loop variables, as they can be used in a wrap context
                        //if (!IsInLoop())
                        //{
                        //    LogError(currentToken, $"Unexpected variable <{text}> outside of a loop");
                        //}
                    }
                    else
                    {
                        LogError(currentToken, $"Invalid token `{Current.Type}`. The loop variable <{text}> dot must be followed by an identifier");
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

            // Flush any trivias after
            if (_isKeepTrivia)
            {
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