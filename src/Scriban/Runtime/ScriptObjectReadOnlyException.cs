// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;

namespace Scriban.Runtime
{
    /// <summary>
    /// An exception thrown when accessing a readonly <see cref="IScriptObject"/>
    /// </summary>
    public class ScriptObjectReadOnlyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptObjectReadOnlyException"/> class.
        /// </summary>
        /// <param name="scriptObject">The script object.</param>
        /// <exception cref="System.ArgumentNullException">scriptObject</exception>
        public ScriptObjectReadOnlyException(IScriptObject scriptObject) : base("This instance is readonly")
        {
            ScriptObject = scriptObject ?? throw new ArgumentNullException(nameof(scriptObject));
        }

        /// <summary>
        /// Gets the script object that is readonly.
        /// </summary>
        public IScriptObject ScriptObject { get; }
    }
}