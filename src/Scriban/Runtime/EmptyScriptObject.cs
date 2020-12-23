// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Scriban.Runtime
{
    /// <summary>
    /// The empty object (unique singleton, cannot be modified, does not contain any properties)
    /// </summary>
    [DebuggerDisplay("<empty object>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    sealed class EmptyScriptObject : IScriptObject
    {
        public static readonly EmptyScriptObject Default = new EmptyScriptObject();

        private EmptyScriptObject()
        {
        }

        public int Count => 0;

        public IEnumerable<string> GetMembers()
        {
            yield break;
        }

        public bool Contains(string member)
        {
            return false;
        }

        public bool IsReadOnly
        {
            get => true;
            set { }
        }

        public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            value = null;
            return false;
        }

        public bool CanWrite(string member)
        {
            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            throw new ScriptRuntimeException(span, "Cannot set a property on the empty object");
        }

        public bool Remove(string member)
        {
            return false;
        }

        public void SetReadOnly(string member, bool readOnly)
        {
        }

        public IScriptObject Clone(bool deep)
        {
            return this;
        }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}