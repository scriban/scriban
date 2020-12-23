// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Scriban.Syntax
{
    [ScriptSyntax("literal", "<value>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptLiteral : ScriptExpression, IScriptTerminal
    {
        public ScriptLiteral()
        {
        }

        public ScriptLiteral(object value)
        {
            Value = value;
        }

        public ScriptTrivias Trivias { get; set; }

        public object Value { get; set; }

        public ScriptLiteralStringQuoteType StringQuoteType { get; set; }

        public override object Evaluate(TemplateContext context)
        {
            return Value;
        }

        public bool IsPositiveInteger()
        {
            if (Value == null)
            {
                return false;
            }
            var type = Value.GetType();
            if (type == typeof(int))
            {
                return ((int)Value) >= 0;
            }
            else if (type == typeof(byte))
            {
                return true;
            }
            else if (type == typeof(sbyte))
            {
                return ((sbyte)Value) >= 0;
            }
            else if (type == typeof(short))
            {
                return ((short)Value) >= 0;
            }
            else if (type == typeof(ushort))
            {
                return true;
            }
            else if (type == typeof(uint))
            {
                return true;
            }
            else if (type == typeof(long))
            {
                return (long)Value > 0;
            }
            else if (type == typeof(ulong))
            {
                return true;
            }
            return false;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Value == null)
            {
                printer.Write("null");
                return;
            }

            var type = Value.GetType();
            if (type == typeof(string))
            {
                printer.Write(ToLiteral(StringQuoteType, (string) Value));
            }
            else if (type == typeof(bool))
            {
                printer.Write(((bool) Value) ? "true" : "false");
            }
            else if (type == typeof(int))
            {
                printer.Write(((int) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(double))
            {
                printer.Write(AppendDecimalPoint(((double)Value).ToString("R", CultureInfo.InvariantCulture), true));
            }
            else if (type == typeof(float))
            {
                printer.Write(AppendDecimalPoint(((float)Value).ToString("R", CultureInfo.InvariantCulture), true));
                printer.Write("f");
            }
            else if (type == typeof(decimal))
            {
                printer.Write(AppendDecimalPoint(((decimal)Value).ToString(CultureInfo.InvariantCulture), true));
                printer.Write("m");
            }
            else if (type == typeof(byte))
            {
                printer.Write(((byte) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(sbyte))
            {
                printer.Write(((sbyte) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(short))
            {
                printer.Write(((short) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(ushort))
            {
                printer.Write(((ushort) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(uint))
            {
                printer.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(long))
            {
                printer.Write(((long) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(ulong))
            {
                printer.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(char))
            {
                printer.Write(ToLiteral(ScriptLiteralStringQuoteType.SimpleQuote, Value.ToString()));
            }
            else
            {
                printer.Write(Value.ToString());
            }
        }

        private static string ToLiteral(ScriptLiteralStringQuoteType quoteType, string input)
        {
            char quote;
            switch (quoteType)
            {
                case ScriptLiteralStringQuoteType.DoubleQuote:
                    quote = '"';
                    break;
                case ScriptLiteralStringQuoteType.SimpleQuote:
                    quote = '\'';
                    break;
                case ScriptLiteralStringQuoteType.Verbatim:
                    quote = '`';
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(quoteType));
            }

            var literal = new StringBuilder(input.Length + 2);
            literal.Append(quote);

            if (quoteType == ScriptLiteralStringQuoteType.Verbatim)
            {
                literal.Append(input.Replace("`", "``"));
            }
            else
            {
                foreach (var c in input)
                {
                    switch (c)
                    {
                        case '\\': literal.Append(@"\\"); break;
                        case '\0': literal.Append(@"\0"); break;
                        case '\a': literal.Append(@"\a"); break;
                        case '\b': literal.Append(@"\b"); break;
                        case '\f': literal.Append(@"\f"); break;
                        case '\n': literal.Append(@"\n"); break;
                        case '\r': literal.Append(@"\r"); break;
                        case '\t': literal.Append(@"\t"); break;
                        case '\v': literal.Append(@"\v"); break;
                        default:
                            if (c == quote)
                            {
                                literal.Append('\\').Append(c);
                            }
                            else if (char.IsControl(c))
                            {
                                literal.Append(@"\u");
                                literal.Append(((ushort)c).ToString("x4"));
                            }
                            else
                            {
                                literal.Append(c);
                            }
                            break;
                    }
                }
            }
            literal.Append(quote);
            return literal.ToString();
        }

        // Code from SharpYaml
        private static string AppendDecimalPoint(string text, bool hasNaN)
        {
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                // Do not append a decimal point if floating point type value
                // - is in exponential form, or
                // - already has a decimal point
                if (c == 'e' || c == 'E' || c == '.')
                {
                    return text;
                }
            }
            // Special cases for floating point type supporting NaN and Infinity
            if (hasNaN && (string.Equals(text, "NaN") || text.Contains("Infinity")))
                return text;

            return text + ".0";
        }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptLiteralStringQuoteType
    {
        DoubleQuote,

        SimpleQuote,

        Verbatim
    }
}