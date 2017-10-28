// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// Builtins functions for a Liquid TemplateContext
    /// </summary>
    public class LiquidBuiltinsFunctions : ScriptObject
    {
        public LiquidBuiltinsFunctions()
        {
            // ReSharper disable CollectionNeverUpdated.Local
            var math = new MathFunctions();
            var str = new StringFunctions();
            var array = new ArrayFunctions();
            var date = new DateTimeFunctions();
            var html = new HtmlFunctions();
            // ReSharper restore CollectionNeverUpdated.Local
            SetValue("abs", math["abs"], true);
            SetValue("append", str["append"], true);
            SetValue("capitalize", str["capitalize"], true);
            SetValue("ceil", math["ceil"], true);
            SetValue("compact", array["compact"], true);
            SetValue("concat", array["concat"], true);
            SetValue("date", date["parse"], true);
            SetValue("downcase", str["downcase"], true);
            SetValue("escape", html["escape"], true);
            //SetValue("escape_once", html["escape_once"], true);
            SetValue("first", array["first"], true);
            SetValue("floor", math["floor"], true);
            SetValue("join", array["join"], true);
            SetValue("last", array["last"], true);
            SetValue("lstrip", str["lstrip"], true);
            SetValue("map", array["map"], true);
            SetValue("minus", math["minus"], true);
            SetValue("modulo", math["modulo"], true);
            //SetValue("newline_to_br", html["newline_to_br"], true);
            SetValue("plus", math["plus"], true);
            SetValue("prepend", str["prepend"], true);
            SetValue("remove", str["remove"], true);
            SetValue("remove_first", str["remove_first"], true);
            SetValue("replace", str["replace"], true);
            SetValue("replace_first", str["replace_first"], true);
            SetValue("reverse", array["reverse"], true);
            SetValue("round", math["round"], true);
            SetValue("rstrip", str["rstrip"], true);
            //size: provided by this function
            SetValue("slice", new DelegateCustomFunction(Slice), true); // Special liquid compatible function
            SetValue("sort", array["sort"], true);
            // sort_natural: not supported
            SetValue("split", str["split"], true);
            SetValue("strip", str["strip"], true);
            SetValue("strip_html", html["strip"], true);
            SetValue("strip_newlines", str["strip_newlines"], true);
            SetValue("times", math["times"], true);
            SetValue("truncate", str["truncate"], true);
            SetValue("truncatewords", str["truncatewords"], true);
            SetValue("uniq", array["uniq"], true);
        }

        public static object Default(object defaultValue, object value)
        {
            return value ?? defaultValue;
        }

        public static int Size(TemplateContext context, SourceSpan span, object value)
        {
            if (value is string)
            {
                return StringFunctions.Size((string) value);
            }

            if (value is IEnumerable)
            {
                return ArrayFunctions.Size((IEnumerable) value);
            }

            // Should we throw an exception?
            return 0;
        }

        public static object DividedBy(TemplateContext context, SourceSpan span, object right, double left)
        {
            var leftType = typeof(double);
            var rightType = right?.GetType();

            var result = ScriptBinaryExpression.Calculate(context, span, ScriptBinaryOperator.Divide, left, leftType, right, rightType);

            // If the divisor is an integer, return a an integer
            if (right is int)
            {
                if (result is double)
                {
                    return (int)Math.Floor((double) result);
                }
                if (result is float)
                {
                    return (int)Math.Floor((float) result);
                }
            }
            return result;
        }

        // On Liquid: Slice will return 1 character by default, unlike in scriban that returns the rest of the string
        [ScriptMemberIgnore]
        public static string Slice(string text, int start, int length = 1)
        {
            if (text == null || start > text.Length)
            {
                return string.Empty;
            }

            if (length < 0)
            {
                length = text.Length - start;
            }

            if (start < 0)
            {
                start = Math.Max(start + text.Length, 0);
            }
            var end = start + length;
            if (end <= start)
            {
                return string.Empty;
            }
            if (end > text.Length)
            {
                length = text.Length - start;
            }

            return text.Substring(start, length);
        }

        private static object Slice(TemplateContext context, ScriptNode callerContext, ScriptArray parameters)
        {
            if (parameters.Count < 2 || parameters.Count > 3)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Unexpected number of arguments [{parameters.Count}] for slice. Expecting at least 2 parameters <start> <length>? <text>");
            }

            var text = context.ToString(callerContext.Span, parameters[parameters.Count - 1]);
            var start = context.ToInt(callerContext.Span, parameters[0]);
            var length = 1;
            if (parameters.Count == 3)
            {
                length = context.ToInt(callerContext.Span, parameters[1]);
            }

            return Slice(text, start, length);
        }
    }
}