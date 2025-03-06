// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#if NET7_0_OR_GREATER
#nullable enable
using System;
using System.Text.Json;


namespace Scriban.Runtime {
    /// <summary>
    /// Extensions attached to an <see cref="JsonElement"/>.
    /// </summary>
    internal static class JsonElementExtensions {
        internal static object? ToScriban(this JsonElement model)
        {
            return model.ValueKind switch {
                JsonValueKind.Array => model.CopyToScriptArray(new ScriptArray()),
                JsonValueKind.Object => model.CopyToScriptObject(new ScriptObject()),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Null => null,
                JsonValueKind.Number => model.GetDecimal(),
                JsonValueKind.String => model.GetString(),
                JsonValueKind.Undefined => null,
                _ => throw new ArgumentOutOfRangeException(nameof(model), model.ValueKind, null)
            };
        }

        internal static IScriptObject CopyToScriptObject(this JsonElement json, IScriptObject obj)
        {
            foreach (var property in json.EnumerateObject()) {
                obj.SetValue(property.Name, property.Value.ToScriban(), false);
            }

            return obj;
        }

        internal static ScriptArray CopyToScriptArray(this JsonElement json, ScriptArray array)
        {
            foreach (var value in json.EnumerateArray()) {
                array.Add(value.ToScriban());
            }

            return array;
        }
    }
}

#endif
