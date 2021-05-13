// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
#nullable disable

using System;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Generic interface used to access an indexer, used by <see cref="TemplateContext"/> via <see cref="TemplateContext.GetListAccessor"/>
    /// This is _not_ used for IDictionary, IList, etc.
    /// </summary>
    public interface IItemAccessor
    {
        /// <summary>
        /// Tries to get the member value for the specified target object.
        /// </summary>
        /// <param name="context">The originated tempate context</param>
        /// <param name="span">The originated span</param>
        /// <param name="target">The object target</param>
        /// <param name="index">The index value</param>
        /// <param name="value">The value of the specified member if successful.</param>
        /// <returns><c>true</c> if the member value was retrieved from the target object; <c>false</c> otherwise</returns>

        bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value);

        /// <summary>
        /// Tries to set the member value for the specified target object.
        /// </summary>
        /// <param name="context">The originated tempate context</param>
        /// <param name="span">The originated span</param>
        /// <param name="target">The object target</param>
        /// <param name="index">The index value</param>
        /// <param name="value">The value of the specified member to set.</param>
        /// <returns><c>true</c> if the member value was set onto the target object; <c>false</c> otherwise</returns>
        bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value);

        /// <summary>
        /// Gets or sets the type of the indexer
        /// </summary>
        Type ItemType { get; }
    }
}