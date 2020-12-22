// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Generic interface used to access a list/array, used by <see cref="TemplateContext"/> via <see cref="TemplateContext.GetListAccessor"/>
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IListAccessor
    {
        /// <summary>
        /// Gets the length of the specified target object
        /// </summary>
        /// <param name="context">The template context originating this call</param>
        /// <param name="span">The source span originating</param>
        /// <param name="target">The target list object</param>
        /// <returns>The length</returns>
        int GetLength(TemplateContext context, SourceSpan span, object target);

        /// <summary>
        /// Gets the element value at the specified index.
        /// </summary>
        /// <param name="context">The template context originating this call</param>
        /// <param name="span">The source span originating</param>
        /// <param name="target">The target list object</param>
        /// <param name="index">The index to retrieve a value</param>
        /// <returns>The value retrieved at the specified index for the target object</returns>
        object GetValue(TemplateContext context, SourceSpan span, object target, int index);

        /// <summary>
        /// Sets the element value at the specified index.
        /// </summary>
        /// <param name="context">The template context originating this call</param>
        /// <param name="span">The source span originating</param>
        /// <param name="target">The target list object</param>
        /// <param name="index">The index to set the value</param>
        /// <param name="value">The value to set at the specified index</param>
        void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value);
    }
}