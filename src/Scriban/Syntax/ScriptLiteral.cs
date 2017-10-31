// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Globalization;
using System.Text;

namespace Scriban.Syntax
{
    [ScriptSyntax("literal", "<value>")]
    public class ScriptLiteral : ScriptExpression
    {
        public object Value { get; set; }

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

        public override void Write(RenderContext context)
        {
            if (Value == null)
            {
                context.Write("null");
                return;
            }

            var type = Value.GetType();
            if (type == typeof(string))
            {
                context.Write(ToLiteral((string) Value));
            }
            else if (type == typeof(bool))
            {
                context.Write(((bool) Value) ? "true" : "false");
            }
            else if (type == typeof(int))
            {
                context.Write(((int) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(double))
            {
                context.Write(((double) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(float))
            {
                context.Write(((float) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(byte))
            {
                context.Write(((byte) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(sbyte))
            {
                context.Write(((sbyte) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(short))
            {
                context.Write(((short) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(ushort))
            {
                context.Write(((ushort) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(uint))
            {
                context.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(long))
            {
                context.Write(((long) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(ulong))
            {
                context.Write(((uint) Value).ToString(CultureInfo.InvariantCulture));
            }
            else if (type == typeof(char))
            {
                context.Write(ToLiteral(Value.ToString()));
            }
            else
            {
                context.Write(Value.ToString());
            }
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        private static string ToLiteral(string input)
        {
            var literal = new StringBuilder(input.Length + 2);
            literal.Append("\"");
            foreach (var c in input)
            {
                switch (c)
                {
                    case '\'': literal.Append(@"\'"); break;
                    case '\"': literal.Append("\\\""); break;
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
                        if (char.IsControl(c))
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
            literal.Append("\"");
            return literal.ToString();
        }
    }
}