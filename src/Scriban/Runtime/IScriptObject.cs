// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Base interface for a scriptable object.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    interface IScriptObject
    {
        /// <summary>
        /// Gets the number of members
        /// </summary>
        int Count { get; }

        IEnumerable<string> GetMembers();

        /// <summary>
        /// Determines whether this object contains the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if this object contains the specified member; <c>false</c> otherwise</returns>
        /// <exception cref="System.ArgumentNullException">If member is null</exception>
        bool Contains(string member);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Tries the get the value of the specified member.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="span"></param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the value was retrieved</returns>
        bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value);

        /// <summary>
        /// Determines whether the specified member is read-only.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if the specified member is read-only</returns>
        bool CanWrite(string member);

        /// <summary>
        /// Sets the value and readonly state of the specified member. This method overrides previous readonly state.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="span"></param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly);

        /// <summary>
        /// Removes the specified member from this object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns><c>true</c> if it was removed</returns>
        bool Remove(string member);

        /// <summary>
        /// Sets to read only the specified member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="readOnly">if set to <c>true</c> the value will be read only.</param>
        void SetReadOnly(string member, bool readOnly);

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <param name="deep">Clones this instance deeply</param>
        /// <returns>A clone of this instance</returns>
        IScriptObject Clone(bool deep);
    }
}