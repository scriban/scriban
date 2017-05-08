// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Text;
using Scriban.Model;
using Scriban.Runtime;

namespace Scriban.Parsing
{
    public partial class Parser
    {
        private ScriptExpression ParseVariableOrLiteral()
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
                    return Close(functionExp);
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

                        Target = new ScriptVariable(ScriptVariable.Arguments.Name, ScriptVariableScope.Local)
                        {
                            Span = currentSpan
                        },

                        Index = new ScriptLiteral() {Span = currentSpan, Value = index}
                    };
                    return indexerExpression;
                }
            }

            if (text == "for" || text == "while")
            {
                if (Current.Type == TokenType.Dot)
                {
                    NextToken();
                    if (Current.Type == TokenType.Identifier)
                    {
                        endSpan = CurrentSpan;
                        var loopVariableText = GetAsText(Current);
                        scope = ScriptVariableScope.Loop;
                        switch (loopVariableText)
                        {
                            case "first":
                                text = "for.first";
                                break;
                            case "last":
                                if (text == "while")
                                {
                                    // unit test: 108-variable-loop-error2.txt
                                    LogError(currentToken, "The loop variable <while.last> is invalid");
                                }
                                text = "for.last";
                                break;
                            case "even":
                                text = "for.even";
                                break;
                            case "odd":
                                text = "for.odd";
                                break;
                            case "index":
                                text = "for.index";
                                break;
                            default:
                                text = text + "." + loopVariableText;
                                // unit test: 108-variable-loop-error1.txt
                                LogError(currentToken, $"The loop variable <{text}> is not supported");
                                break;
                        }

                        // We no longer checks at parse time usage of loop variables, as they can be used in a wrap context
                        //if (!IsInLoop())
                        //{
                        //    LogError(currentToken, $"Unexpected variable <{text}> outside of a loop");
                        //}

                        NextToken();
                    }
                    else
                    {
                        LogError(currentToken, $"Invalid token [{Current.Type}]. The loop variable <{text}> dot must be followed by an identifier");
                    }
                }
                else
                {
                    LogError(currentToken, $"The reserved keyword <{text}> cannot be used as a variable");
                }
            }
            else if (IsKeyword(text))
            {
                // unit test: 108-variable-error1.txt
                LogError(currentToken, $"The reserved keyword <{text}> cannot be used as a variable");
            }

            var result = new ScriptVariable(text, scope)
            {
                Span = {FileName =currentSpan.FileName, Start = currentSpan.Start, End = endSpan.End}
            };
            return result;
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
                literal.Value = (int)result;
            }
            else
            {
                literal.Value = result;
            }

            NextToken(); // Skip the literal
            return Close(literal);
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
                LogError($"Unable to parse double value [{text}]");
            }

            NextToken(); // Skip the float
            return Close(literal);
        }

        private ScriptLiteral ParseString()
        {
            var literal = Open<ScriptLiteral>();
            var text = lexer.Text;
            var builder = new StringBuilder(Current.End.Offset - Current.Start.Offset - 1);

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
                            builder.Append((char)0);
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
                            LogError($"Unexpected escape character [{text[i]}] in string");
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

        private ScriptLiteral ParseVerbatimString()
        {
            var literal = Open<ScriptLiteral>();
            var text = lexer.Text;

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
    }
}