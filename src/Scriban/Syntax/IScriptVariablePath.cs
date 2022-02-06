// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#if !SCRIBAN_NO_ASYNC
using System.Threading.Tasks;
#endif

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptVariablePath
    {
        object GetValue(TemplateContext context);

        void SetValue(TemplateContext context, object valueToSet);

        string GetFirstPath();

#if !SCRIBAN_NO_ASYNC
        ValueTask<object> GetValueAsync(TemplateContext context);

        ValueTask SetValueAsync(TemplateContext context, object valueToSet);
#endif
    }
}