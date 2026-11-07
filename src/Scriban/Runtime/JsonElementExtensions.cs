// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#if !SCRIBAN_NO_SYSTEM_TEXT_JSON
#nullable enable
using System;
using System.Text.Json;


namespace Scriban.Runtime {
    /// <summary>
    /// Extensions attached to an <see cref="JsonElement"/>.
    /// </summary>
    internal static class JsonElementExtensions {
        internal static object? ToScriban(this JsonElement model, TemplateContext? context = null)
        {
            context?.CheckAbort();
            return model.ValueKind switch {
                JsonValueKind.Array => model.CopyToScriptArray(new ScriptArray(), context),
                JsonValueKind.Object => model.CopyToScriptObject(new ScriptObject(), context),
                JsonValueKind.False => false,
                JsonValueKind.True => true,
                JsonValueKind.Null => null,
                JsonValueKind.Number => model.GetDecimal(),
                JsonValueKind.String => model.GetString(),
                JsonValueKind.Undefined => null,
                _ => throw new ArgumentOutOfRangeException(nameof(model), model.ValueKind, null)
            };
        }

        internal static IScriptObject CopyToScriptObject(this JsonElement json, IScriptObject obj, TemplateContext? context = null)
        {
            using var loopScope = context?.EnterLoopScope();
            foreach (var property in json.EnumerateObject()) {
                context?.StepLoop(context.CurrentSpan);
                obj.SetValue(property.Name, property.Value.ToScriban(context), false);
            }

            return obj;
        }

        internal static ScriptArray CopyToScriptArray(this JsonElement json, ScriptArray array, TemplateContext? context = null)
        {
            using var loopScope = context?.EnterLoopScope();
            foreach (var value in json.EnumerateArray()) {
                context?.StepLoop(context.CurrentSpan);
                array.Add(value.ToScriban(context));
            }

            return array;
        }
    }
}
#endif
