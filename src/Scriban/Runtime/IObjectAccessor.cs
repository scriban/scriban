// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Runtime
{
    /// <summary>
    /// Generic interface used to access an object (either <see cref="ScriptObject"/> or .NET object), used by <see cref="TemplateContext"/> via <see cref="TemplateContext.GetMemberAccessor"/>
    /// </summary>
    public interface IObjectAccessor
    {
        /// <summary>
        /// Returns true if the object has any members, <c>false</c> otherwise.
        /// </summary>
        int GetMemberCount(TemplateContext context, SourceSpan span, object target);

        /// <summary>
        /// Returns the member names of an object.
        /// </summary>
        /// <param name="context">The originated tempate context</param>
        /// <param name="span">The originated span</param>
        /// <param name="target">The object target</param>
        IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target);

        /// <summary>
        /// Returns true if the specified target object has the specified member.
        /// </summary>
        /// <param name="context">The originated tempate context</param>
        /// <param name="span">The originated span</param>
        /// <param name="target">The object target</param>
        /// <param name="member">The member name</param>
        /// <returns><c>true</c> if the target object has the specified member; <c>false</c> otherwise</returns>
        bool HasMember(TemplateContext context, SourceSpan span, object target, string member);

        /// <summary>
        /// Tries to get the member value for the specified target object.
        /// </summary>
        /// <param name="context">The originated tempate context</param>
        /// <param name="span">The originated span</param>
        /// <param name="target">The object target</param>
        /// <param name="member">The member name</param>
        /// <param name="value">The value of the specified member if successful.</param>
        /// <returns><c>true</c> if the member value was retrieved from the target object; <c>false</c> otherwise</returns>
        bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value);

        /// <summary>
        /// Tries to set the member value for the specified target object.
        /// </summary>
        /// <param name="context">The originated tempate context</param>
        /// <param name="span">The originated span</param>
        /// <param name="target">The object target</param>
        /// <param name="member">The member name</param>
        /// <param name="value">The value of the specified member to set.</param>
        /// <returns><c>true</c> if the member value was set onto the target object; <c>false</c> otherwise</returns>
        bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value);
    }
}