// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using Scriban.Runtime;

namespace Scriban.Model
{
    /// <summary>
    /// A script variable
    /// </summary>
    /// <remarks>This class is immutable as all variable object are being shared across all templates</remarks>
    [ScriptSyntax("variable", "<variable_name>")]
    public sealed class ScriptVariable : ScriptVariablePath, IEquatable<ScriptVariable>
    {
        private readonly int _hashCode;

        public static readonly ScriptVariable Arguments = new ScriptVariable(string.Empty, ScriptVariableScope.Local);

        public static readonly ScriptVariable BlockDelegate = new ScriptVariable("$", ScriptVariableScope.Local);

        public static readonly ScriptVariable LoopFirst = new ScriptVariable("for.first", ScriptVariableScope.Loop);

        public static readonly ScriptVariable LoopLast = new ScriptVariable("for.last", ScriptVariableScope.Loop);

        public static readonly ScriptVariable LoopEven = new ScriptVariable("for.even", ScriptVariableScope.Loop);

        public static readonly ScriptVariable LoopOdd = new ScriptVariable("for.odd", ScriptVariableScope.Loop);

        public static readonly ScriptVariable LoopIndex = new ScriptVariable("for.index", ScriptVariableScope.Loop);

        public ScriptVariable(string name, ScriptVariableScope scope)
        {
            Name = name;
            Scope = scope;
            unchecked
            {
                _hashCode = (Name.GetHashCode() * 397) ^ (int)Scope;
            }
        }

        /// <summary>
        /// Gets or sets the name of the variable (without the $ sign for local variable)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a boolean indicating whether this variable is a local variable (starting with $ in the template ) or global.
        /// </summary>
        public ScriptVariableScope Scope { get; }


        public bool Equals(ScriptVariable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Scope == other.Scope;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ScriptVariable && Equals((ScriptVariable) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return Scope == ScriptVariableScope.Local ? $"${Name}" : Name;
        }

        public static bool operator ==(ScriptVariable left, ScriptVariable right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScriptVariable left, ScriptVariable right)
        {
            return !Equals(left, right);
        }

        public override void Evaluate(TemplateContext context)
        {
            context.Result = context.GetValue(this);
        }
    }
}