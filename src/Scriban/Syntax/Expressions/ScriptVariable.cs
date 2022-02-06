// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Scriban.Syntax
{
    /// <summary>
    /// A script variable
    /// </summary>
    /// <remarks>This class is immutable as all variable object are being shared across all templates</remarks>
    [ScriptSyntax("variable", "<variable_name>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    abstract partial class ScriptVariable : ScriptExpression, IScriptVariablePath, IEquatable<ScriptVariable>, IScriptTerminal
    {
        private int _hashCode;

        public static readonly ScriptVariableLocal Arguments = new ScriptVariableLocal(string.Empty);
        public static readonly ScriptVariableLocal BlockDelegate = new ScriptVariableLocal("$");
        public static readonly ScriptVariableLocal Continue = new ScriptVariableLocal("continue"); // Used by liquid offset:continue
        public static readonly ScriptVariableGlobal ForObject = new ScriptVariableGlobal("for");
        public static readonly ScriptVariableGlobal TablerowObject = new ScriptVariableGlobal("tablerow");
        public static readonly ScriptVariableGlobal WhileObject = new ScriptVariableGlobal("while");

        protected ScriptVariable(string name, ScriptVariableScope scope)
        {
            BaseName = name;
            Scope = scope;
            switch (scope)
            {
                case ScriptVariableScope.Global:
                    Name = name;
                    break;
                case ScriptVariableScope.Local:
                    Name = $"${name}";
                    break;
            }
            unchecked
            {
                _hashCode = (BaseName.GetHashCode() * 397) ^ (int)Scope;
            }
        }

        public ScriptTrivias Trivias { get; set; }

        /// <summary>
        /// Creates a <see cref="ScriptVariable"/> according to the specified name and <see cref="ScriptVariableScope"/>
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="scope">Scope of the variable</param>
        /// <returns>The script variable</returns>
        public static ScriptVariable Create(string name, ScriptVariableScope scope)
        {
            switch (scope)
            {
                case ScriptVariableScope.Global:
                    return new ScriptVariableGlobal(name);
                case ScriptVariableScope.Local:
                    return new ScriptVariableLocal(name);
                default:
                    throw new InvalidOperationException($"Scope `{scope}` is not supported");
            }
        }

        public string BaseName { get; }

        /// <summary>
        /// Gets or sets the name of the variable (without the $ sign for local variable)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a boolean indicating whether this variable is a local variable (starting with $ in the template ) or global.
        /// </summary>
        public ScriptVariableScope Scope { get; }

        public string GetFirstPath()
        {
            return Name;
        }

#if !SCRIBAN_NO_ASYNC
        public ValueTask SetValueAsync(TemplateContext context, object valueToSet)
        {
            return context.SetValueAsync(this, valueToSet);
        }
#endif

        public virtual bool Equals(ScriptVariable other)
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

        public static bool operator ==(ScriptVariable left, ScriptVariable right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScriptVariable left, ScriptVariable right)
        {
            return !Equals(left, right);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue((ScriptExpression)this);
        }

        public virtual object GetValue(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            context.SetValue(this, valueToSet);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Name);
        }
    }

#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptVariableGlobal : ScriptVariable
    {
        public ScriptVariableGlobal(string name) : base(name, ScriptVariableScope.Global)
        {
        }

        public override object GetValue(TemplateContext context)
        {
            // Used a specialized overrides on contxet for ScriptVariableGlobal
            return context.GetValue(this);
        }
    }


#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptVariableLocal : ScriptVariable
    {
        public ScriptVariableLocal(string name) : base(name, ScriptVariableScope.Local)
        {
        }
    }
}