// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#if NET
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
                JsonValueKind.Array => new ScriptArray().AddJsonArray(model),
                JsonValueKind.Object => new ScriptObject().AddJsonObject(model),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Null => null,
                JsonValueKind.Number => model.GetDecimal(),
                JsonValueKind.String => model.GetString(),
                JsonValueKind.Undefined => null,
                _ => throw new ArgumentOutOfRangeException(nameof(model), model.ValueKind, null)
            };
        }
    }
}

#endif