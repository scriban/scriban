// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Runtime.Accessors;
using Scriban.Syntax;

namespace Scriban.Functions
{
    /// <summary>
    /// Object functions available through the builtin object 'object'.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class ObjectFunctions : ScriptObject
    {
        /// <summary>
        /// The `default` value is returned if the input `value` is null or an empty string "". A string containing whitespace characters will not resolve to the default value.
        /// </summary>
        /// <param name="value">The input value to check if it is null or an empty string.</param>
        /// <param name="default">The default value to return if the input `value` is null or an empty string.</param>
        /// <returns>The `default` value is returned if the input `value` is null or an empty string "", otherwise it returns `value`</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ undefined_var | object.default "Yo" }}
        /// ```
        /// ```html
        /// Yo
        /// ```
        /// </remarks>
        public static object Default(object value, object @default)
        {
            return value == null || (value is string && string.IsNullOrEmpty((string)value)) ? @default : value;
        }

        /// <summary>
        /// The evaluates a string as a scriban expression or evaluate the passed function or return the passed value.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="value">The input value, either a scriban template in a string, or an alias function or directly a value.</param>
        /// <returns>The evaluation of the input value.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "1 + 2" | object.eval }}
        /// ```
        /// ```html
        /// 3
        /// ```
        /// </remarks>
        public static object Eval(TemplateContext context, SourceSpan span, object value)
        {
            if (value == null) return null;

            if (value is string templateStr)
            {
                try
                {
                    var template = Template.Parse(templateStr, lexerOptions: new LexerOptions() { Lang = context.Language, Mode = ScriptMode.ScriptOnly });
                    return context.Evaluate(template.Page);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message, nameof(value));
                }
            }

            if (value is IScriptCustomFunction function)
            {
                return ScriptFunctionCall.Call(context, context.CurrentNode, function, false, null);
            }

            return value;
        }

        /// <summary>
        /// The evaluates a string as a scriban template or evaluate the passed function or return the passed value.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="value">The input value, either a scriban template in a string, or an alias function or directly a value.</param>
        /// <returns>The evaluation of the input value.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ "This is a template text {{ 1 + 2 }}" | object.eval_template }}
        /// ```
        /// ```html
        /// This is a template text 3
        /// ```
        /// </remarks>
        public static object EvalTemplate(TemplateContext context, SourceSpan span, object value)
        {
            if (value == null) return null;

            if (value is string templateStr)
            {
                try
                {
                    var template = Template.Parse(templateStr, lexerOptions: new LexerOptions() { Lang = context.Language, Mode = ScriptMode.Default });
                    var output = new StringBuilderOutput();
                    context.PushOutput(output);
                    try
                    {
                        context.Evaluate(template.Page);
                    }
                    finally
                    {
                        context.PopOutput();
                    }
                    return output.ToString();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message, nameof(value));
                }
            }

            if (value is IScriptCustomFunction function)
            {
                return ScriptFunctionCall.Call(context, context.CurrentNode, function, false, null);
            }

            return value;
        }

        /// <summary>
        /// Formats an object using specified format.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The source span</param>
        /// <param name="value">The input value</param>
        /// <param name="format">The format string.</param>
        /// <param name="culture">The culture as a string (e.g `en-US`). By default the culture from <see cref="TemplateContext.CurrentCulture"/> is used</param>
        /// <remarks>
        /// ```scriban-html
        /// {{ 255 | object.format "X4" }}
        /// {{ 1523 | object.format "N2" "en-US" }}
        /// ```
        /// ```html
        /// 00FF
        /// 1,523.00
        /// ```
        /// </remarks>
        public static string Format(TemplateContext context, SourceSpan span, object value, string format, string culture = null)
        {
            if (value == null)
            {
                return string.Empty;
            }
            format = format ?? string.Empty;
            if (!(value is IFormattable formattable))
            {
                throw new ScriptRuntimeException(span, $"Unexpected `{value}`. Must be a formattable object");
            }

            return formattable.ToString(format, culture != null ? new CultureInfo(culture) : context.CurrentCulture);
        }

        /// <summary>
        /// Checks if the specified object as the member `key`
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <param name="key">The member name to check its existence.</param>
        /// <returns>**true** if the input object contains the member `key`; otherwise **false**</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ product | object.has_key "title" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool HasKey(IDictionary<string, object> value, string key)
        {
            if (value == null || key == null)
            {
                return false;
            }

            return value.ContainsKey(key);
        }

        /// <summary>
        /// Checks if the specified object as a value for the member `key`
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <param name="key">The member name to check the existence of its value.</param>
        /// <returns>**true** if the input object contains the member `key` and has a value; otherwise **false**</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ product | object.has_value "title" }}
        /// ```
        /// ```html
        /// true
        /// ```
        /// </remarks>
        public static bool HasValue(IDictionary<string, object> value, string key)
        {
            if (value == null || key == null)
            {
                return false;
            }
            return value.ContainsKey(key) && value[key] != null;
        }

        /// <summary>
        /// Gets the members/keys of the specified value object.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="value">The input object.</param>
        /// <returns>A list with the member names/key of the input object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ product | object.keys | array.sort }}
        /// ```
        /// ```html
        /// ["title", "type"]
        /// ```
        /// </remarks>
#pragma warning disable CS0108
        public static ScriptArray Keys(TemplateContext context, object value)
#pragma warning restore CS0108
        {
            if (value == null) return new ScriptArray();
            if (value is IDictionary dict) return new ScriptArray(dict.Keys);
            if (value is IDictionary<string, object> dictStringObject) return new ScriptArray(dictStringObject.Keys);
            if (value is IScriptObject scriptObj) return new ScriptArray(scriptObj.GetMembers());
            // Don't try to return members of a custom function
            if (value is IScriptCustomFunction) return new ScriptArray();

            var accessor = context.GetMemberAccessor(value);
            return new ScriptArray(accessor.GetMembers(context, context.CurrentSpan, value));
        }

        /// <summary>
        /// Returns the size of the input object.
        /// - If the input object is a string, it will return the length
        /// - If the input is a list, it will return the number of elements
        /// - If the input is an object, it will return the number of members
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <returns>The size of the input object.</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ [1, 2, 3] | object.size }}
        /// ```
        /// ```html
        /// 3
        /// ```
        /// </remarks>
        public static int Size(object value)
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

        /// <summary>
        /// Returns string representing the type of the input object. The type can be `string`, `boolean`, `number`, `array`, `iterator` and `object`
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <remarks>
        /// ```scriban-html
        /// {{ null | object.typeof }}
        /// {{ true | object.typeof }}
        /// {{ 1 | object.typeof }}
        /// {{ 1.0 | object.typeof }}
        /// {{ "text" | object.typeof }}
        /// {{ 1..5 | object.typeof }}
        /// {{ [1,2,3,4,5] | object.typeof }}
        /// {{ {} | object.typeof }}
        /// {{ object | object.typeof }}
        /// ```
        /// ```html
        ///
        /// boolean
        /// number
        /// number
        /// string
        /// iterator
        /// array
        /// object
        /// object
        /// ```
        /// </remarks>
        public static string Typeof(object value)
        {
            // TODO: rewrite this in a major version with TemplateContext.GetTypeName
            if (value == null)
            {
                return null;
            }
            var type = value.GetType();
            var typeInfo = type;
            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(bool))
            {
                return "boolean";
            }

            // We assume that we are only using int/double/long for integers and shortcut to IsPrimitive
            if (type.IsPrimitiveOrDecimal())
            {
                return "number";
            }

            if (typeof(ScriptRange).IsAssignableFrom(typeInfo))
            {
                return "iterator";
            }

            // Test first IList, then IEnumerable
            if (typeof(IList).IsAssignableFrom(typeInfo))
            {
                return "array";
            }

            if ((!typeof(ScriptObject).IsAssignableFrom(typeInfo) && !typeof(IDictionary).IsAssignableFrom(typeInfo)) &&
                typeof(IEnumerable).IsAssignableFrom(typeInfo))
            {
                return "iterator";
            }

            return "object";
        }

        /// <summary>
        /// Returns string representing the type of the input object. The type can be `string`, `bool`, `byte`, `sbyte`, `ushort`, `short`, `uint`, `int`,
        /// `ulong`, `long`, `float`, `double`, `decimal`, `bigint`, `enum`, `range`, `array`, `function` and `object`
        /// </summary>
        /// <param name="value">The input object.</param>
        /// <remarks>
        /// This function is newer than object.typeof and returns more detailed results about the types (e.g instead of `number`, returns `int` or `double`)
        ///
        /// ```scriban-html
        /// {{ null | object.kind }}
        /// {{ true | object.kind }}
        /// {{ 1 | object.kind }}
        /// {{ 1.0 | object.kind }}
        /// {{ "text" | object.kind }}
        /// {{ 1..5 | object.kind }}
        /// {{ [1,2,3,4,5] | object.kind }}
        /// {{ {} | object.kind }}
        /// {{ object | object.kind }}
        /// ```
        /// ```html
        ///
        /// bool
        /// int
        /// double
        /// string
        /// range
        /// array
        /// object
        /// object
        /// ```
        /// </remarks>
#pragma warning disable 1573
        public static string Kind(TemplateContext context, object value)
#pragma warning restore 1573
        {
            if (value == null)
            {
                return null;
            }
            return context.GetTypeName(value);
        }

        /// <summary>
        /// Gets the member's values of the specified value object.
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="value">The input object.</param>
        /// <returns>A list with the member values of the input object</returns>
        /// <remarks>
        /// ```scriban-html
        /// {{ product | object.values | array.sort }}
        /// ```
        /// ```html
        /// ["fruit", "Orange"]
        /// ```
        /// </remarks>
#pragma warning disable CS0108
        public static ScriptArray Values(TemplateContext context, object value)
#pragma warning restore CS0108
        {
            if (value == null) return new ScriptArray();
            if (value is IDictionary<string, object> dictStringObject) return new ScriptArray(dictStringObject.Values);
            // Don't try to return values of a custom function
            if (value is IScriptCustomFunction) return new ScriptArray();

            var accessor = context.GetMemberAccessor(value);
            var scriptArray = new ScriptArray();
            foreach(var member in accessor.GetMembers(context, context.CurrentSpan, value))
            {
                _ = accessor.TryGetValue(context, context.CurrentSpan, value, member, out var memberValue);
                scriptArray.Add(memberValue);
            }
            return scriptArray;
        }
    }
}