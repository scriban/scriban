// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class LiquidBuiltinsFunctions : ScriptObject
    {
        /// <summary>
        /// This object is readonly, should not be modified by any other objects internally.
        /// </summary>
        private static readonly DefaultBuiltins Default = new DefaultBuiltins();

        public LiquidBuiltinsFunctions() : base(50, false)
        {
            // We copy the default scriban objects
            ((ScriptObject)BuiltinFunctions.Default.Clone(true)).CopyTo(this);
            // And we create the liquid alias
            ((ScriptObject)Default.Clone(false)).CopyTo(this);
        }

        public static bool TryLiquidToScriban(string liquidBuiltin, out string target, out string member)
        {
            if (liquidBuiltin == null) throw new ArgumentNullException(nameof(liquidBuiltin));

            target = null;
            member = null;

            switch (liquidBuiltin)
            {
                case "abs": target = "math"; member = "abs"; return true;
                case "append": target = "string"; member = "append"; return true;
                case "capitalize": target = "string"; member = "capitalize"; return true;
                case "ceil": target = "math"; member = "ceil"; return true;
                case "compact": target = "array"; member = "compact"; return true;
                case "concat": target = "array"; member = "concat"; return true;
                case "cycle": target = "array"; member = "cycle"; return true;
                case "date": target = "date"; member = "parse"; return true;
                case "default": target = "object"; member = "default"; return true;
                case "divided_by": target = "math"; member = "divided_by"; return true;
                case "downcase": target = "string"; member = "downcase"; return true;
                case "escape": target = "html"; member = "escape"; return true;
                case "escape_once": target = "html"; member = "escape_once"; return true;
                case "first": target = "array"; member = "first"; return true;
                case "floor": target = "math"; member = "floor"; return true;
                case "join": target = "array"; member = "join"; return true;
                case "last": target = "array"; member = "last"; return true;
                case "lstrip": target = "string"; member = "lstrip"; return true;
                case "map": target = "array"; member = "map"; return true;
                case "minus": target = "math"; member = "minus"; return true;
                case "modulo": target = "math"; member = "modulo"; return true;
                case "newline_to_br": target = "html"; member = "newline_to_br"; return true;
                case "plus": target = "math"; member = "plus"; return true;
                case "prepend": target = "string"; member = "prepend"; return true;
                case "remove": target = "string"; member = "remove"; return true;
                case "remove_first": target = "string"; member = "remove_first"; return true;
                case "replace": target = "string"; member = "replace"; return true;
                case "replace_first": target = "string"; member = "replace_first"; return true;
                case "reverse": target = "array"; member = "reverse"; return true;
                case "round": target = "math"; member = "round"; return true;
                case "rstrip": target = "string"; member = "rstrip"; return true;
                case "size": target = "object"; member = "size"; return true;
                case "slice": target = "string"; member = "slice1"; return true;
                case "sort": target = "array"; member = "sort"; return true;
                case "split": target = "string"; member = "split"; return true;
                case "strip": target = "string"; member = "strip"; return true;
                case "strip_html": target = "html"; member = "strip"; return true;
                case "strip_newlines": target = "string"; member = "strip_newlines"; return true;
                case "times": target = "math"; member = "times"; return true;
                case "truncate": target = "string"; member = "truncate"; return true;
                case "truncatewords": target = "string"; member = "truncatewords"; return true;
                case "uniq": target = "array"; member = "uniq"; return true;
                case "upcase": target = "string"; member = "upcase"; return true;
                case "contains": target = "array"; member = "contains"; return true;
            }

            return false;
        }

        /// <summary>
        /// Use an internal object to create all default builtins just once to avoid allocations of delegates/IScriptCustomFunction
        /// </summary>
        private class DefaultBuiltins : ScriptObject
        {
            public DefaultBuiltins() : base(50, false)
            {
                // ReSharper disable CollectionNeverUpdated.Local
                var math = (ScriptObject)BuiltinFunctions.Default["math"];
                var str = (ScriptObject)BuiltinFunctions.Default["string"];
                var array = (ScriptObject)BuiltinFunctions.Default["array"];
                var date = (ScriptObject)BuiltinFunctions.Default[DateTimeFunctions.DateVariable.Name];
                var html = (ScriptObject)BuiltinFunctions.Default["html"];
                var objs = (ScriptObject)BuiltinFunctions.Default["object"];

                // NOTE: must be in sync with TryLiquidToScriban

                // ReSharper restore CollectionNeverUpdated.Local
                SetValue("abs", math["abs"], true);
                SetValue("append", str["append"], true);
                SetValue("capitalize", str["capitalize"], true);
                SetValue("ceil", math["ceil"], true);
                SetValue("compact", array["compact"], true);
                SetValue("concat", array["concat"], true);
                SetValue("cycle", array["cycle"], true);
                SetValue("date", date, true);
                SetValue("default", objs["default"], true);
                SetValue("divided_by", math["divided_by"], true);
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
                SetValue("size", objs["size"], true);
                SetValue("slice", str["slice1"], true); // Special liquid compatible function
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
                SetValue("upcase", str["upcase"], true);
                SetValue("contains", array["contains"], true);

                this.Import(typeof(LiquidBuiltinsFunctions), ScriptMemberImportFlags.All);
            }
        }
    }
}