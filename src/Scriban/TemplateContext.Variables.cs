// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection; // Leave this as it is required by some .NET targets
using System.Text;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class TemplateContext
    {
        /// <summary>
        /// Pushes a new object context accessible to the template. This method creates also a new context for local variables.
        /// </summary>
        /// <param name="scriptObject">The script object.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void PushGlobal(IScriptObject scriptObject)
        {
            PushGlobalOnly(scriptObject);
            PushLocal();
        }

        internal void PushGlobalOnly(IScriptObject scriptObject)
        {
            if (scriptObject == null) throw new ArgumentNullException(nameof(scriptObject));
            _globalStores.Push(scriptObject);
        }

        internal IScriptObject PopGlobalOnly()
        {
            if (_globalStores.Count == 1)
            {
                throw new InvalidOperationException("Unexpected PopGlobal() not matching a PushGlobal");
            }
            var store = _globalStores.Pop();
            return store;
        }

        /// <summary>
        /// Pops the previous object context. This method pops also a local variable context.
        /// </summary>
        /// <returns>The previous object context</returns>
        /// <exception cref="System.InvalidOperationException">Unexpected PopGlobal() not matching a PushGlobal</exception>
        public IScriptObject PopGlobal()
        {
            var store = PopGlobalOnly();
            PopLocal();
            return store;
        }

        public void PushLocal()
        {
            PushVariableScope(ScriptVariableScope.Local);
        }

        public void PopLocal()
        {
            PopVariableScope(ScriptVariableScope.Local);
        }

        /// <summary>
        /// Sets the variable with the specified value.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">If variable is null</exception>
        /// <exception cref="ScriptRuntimeException">If an existing variable is already read-only</exception>
        public void SetValue(ScriptVariableLoop variable, object value)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));

            if (_currentLocalContext.Loops.Count > 0)
            {
                // Try to set the variable
                var store = _currentLocalContext.Loops.Peek();
                if (!store.TrySetValue(this, variable.Span, variable.Name, value, false))
                {
                    throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the readonly variable `{variable}`"); // unit test: 105-assign-error2.txt
                }
            }
            else
            {
                // unit test: 215-for-special-var-error1.txt
                throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the loop variable `{variable}` not inside a loop");
            }
        }

        /// <summary>
        /// Sets the variable with the specified value.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="value">The value.</param>
        /// <param name="asReadOnly">if set to <c>true</c> the variable set will be read-only.</param>
        /// <exception cref="System.ArgumentNullException">If variable is null</exception>
        /// <exception cref="ScriptRuntimeException">If an existing variable is already read-only</exception>
        public void SetValue(ScriptVariable variable, object value, bool asReadOnly = false)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var finalStore = GetStoreForWrite(variable);

            // Try to set the variable
            if (!finalStore.TrySetValue(this, variable.Span, variable.Name, value, asReadOnly))
            {
                throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the readonly variable `{variable}`"); // unit test: 105-assign-error2.txt
            }
        }


        /// <summary>
        /// Sets the variable with the specified value.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="value">The value.</param>
        /// <param name="asReadOnly">if set to <c>true</c> the variable set will be read-only.</param>
        /// <param name="force">force setting the value even if it is already readonly</param>
        /// <exception cref="System.ArgumentNullException">If variable is null</exception>
        /// <exception cref="ScriptRuntimeException">If an existing variable is already read-only</exception>
        public void SetValue(ScriptVariable variable, object value, bool asReadOnly, bool force)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var finalStore = GetStoreForWrite(variable);

            // Try to set the variable
            if (force)
            {
                finalStore.Remove(variable.Name);
                finalStore.TrySetValue(this, variable.Span, variable.Name, value, asReadOnly);
            }
            else if (!finalStore.TrySetValue(this, variable.Span, variable.Name, value, asReadOnly))
            {
                throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the readonly variable `{variable}`"); // unit test: 105-assign-error2.txt
            }
        }

        /// <summary>
        /// Deletes the variable from the current store.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void DeleteValue(ScriptVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var finalStore = GetStoreForWrite(variable);
            finalStore.Remove(variable.Name);
        }

        /// <summary>
        /// Sets the variable to read only.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="isReadOnly">if set to <c>true</c> the variable will be set to readonly.</param>
        /// <exception cref="System.ArgumentNullException">If variable is null</exception>
        /// <remarks>
        /// This will not throw an exception if a previous variable was readonly.
        /// </remarks>
        public void SetReadOnly(ScriptVariable variable, bool isReadOnly = true)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var store = GetStoreForWrite(variable);
            store.SetReadOnly(variable.Name, isReadOnly);
        }

        /// <summary>
        /// Sets the loop variable with the specified value.
        /// </summary>
        /// <param name="variable">The loop variable to set.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">If target is null</exception>
        public virtual void SetLoopVariable(ScriptVariable variable, object value)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            if (_currentLocalContext.Loops.Count == 0)
            {
                throw new InvalidOperationException("Cannot set a loop variable without a loop variable store.");

            }

            var store = _currentLocalContext.Loops.Peek();
            // Try to set the variable
            if (!store.TrySetValue(this, variable.Span, variable.Name, value, false))
            {
                throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the variable `{variable}`");
            }
        }

        private void PushLocalContext(ScriptObject locals = null)
        {
            var localContext = _availableLocalContexts.Count > 0 ? _availableLocalContexts.Pop() : new LocalContext(null);
            localContext.LocalObject = locals;
            _localContexts.Push(localContext);
            _currentLocalContext = localContext;
        }

        private ScriptObject PopLocalContext()
        {
            var oldLocalContext = _localContexts.Pop();
            var oldLocals = oldLocalContext.LocalObject;
            oldLocalContext.LocalObject = null;
            _availableLocalContexts.Push(oldLocalContext);

            // There is always an available local context because there is always a global context
            _currentLocalContext = _localContexts.Peek();
            return oldLocals;
        }


        /// <summary>
        /// Gets the value for the specified variable from the current object context/scope.
        /// </summary>
        /// <param name="variable">The variable to retrieve the value</param>
        /// <returns>Value of the variable</returns>
        public object GetValue(ScriptVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));

            var stores = GetStoreForRead(variable);
            object value = null;
            foreach (var store in stores)
            {
                if (store.TryGetValue(this, variable.Span, variable.Name, out value))
                {
                    return value;
                }
            }

            bool found = false;
            if (TryGetVariable != null)
            {
                if (TryGetVariable(this, variable.Span, variable, out value))
                {
                    found = true;
                }
            }

            CheckVariableFound(variable, found);
            return value;
        }

        /// <summary>
        /// Gets the value for the specified global variable from the current object context/scope.
        /// </summary>
        /// <param name="variable">The variable to retrieve the value</param>
        /// <returns>Value of the variable</returns>
        public object GetValue(ScriptVariableGlobal variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            object value = null;

            if (IsInLoop)
            {
                var count = _currentLocalContext.Loops.Count;	               
                var items = _currentLocalContext.Loops.Items;
                for (int i = count - 1; i >= 0; i--)
                {
                    if (items[i].TryGetValue(this, variable.Span, variable.Name, out value))
                    {
                        return value;
                    }
                }
            }

            {
                var count = _globalStores.Count;
                var items = _globalStores.Items;
                for (int i = count - 1; i >= 0; i--)
                {
                    if (items[i].TryGetValue(this, variable.Span, variable.Name, out value))
                    {
                        return value;
                    }
                }
            }

            bool found = false;
            if (TryGetVariable != null)
            {
                if (TryGetVariable(this, variable.Span, variable, out value))
                {
                    found = true;
                }
            }

            CheckVariableFound(variable, found);
            return value;
        }

        private IScriptObject GetStoreForWrite(ScriptVariable variable)
        {
            var scope = variable.Scope;
            IScriptObject finalStore = null;

            switch (scope)
            {
                case ScriptVariableScope.Global:
                    // In scientific we always resolve to local storage first
                    IScriptObject storeWithVariable = null;

                    var name = variable.Name;
                    int lastStoreIndex = _globalStores.Count - 1;
                    for (int i = lastStoreIndex; i >= 0; i--)
                    {
                        var store = _globalStores.Items[i];
                        if (storeWithVariable == null && store.Contains(name))
                        {
                            storeWithVariable = store;
                        }

                        // We check that for upper store, we actually can write a variable with this name
                        // otherwise we don't allow to create a variable with the same name as a readonly variable
                        if (!store.CanWrite(name))
                        {
                            var variableType = store == BuiltinObject ? "builtin " : string.Empty;
                            throw new ScriptRuntimeException(variable.Span, $"Cannot set the {variableType}readonly variable `{variable}`");
                        }
                    }

                    // If we have a store for this variable name use it, otherwise use the first store available.
                    finalStore = storeWithVariable ?? _globalStores.Items[lastStoreIndex];
                    break;
                case ScriptVariableScope.Local:
                    if (_currentLocalContext.LocalObject != null)
                    {
                        finalStore = _currentLocalContext.LocalObject;
                    }
                    else if (_globalStores.Count > 0)
                    {
                        finalStore = _globalStores.Peek();
                    }
                    else
                    {
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    }

                    break;
                case ScriptVariableScope.Loop:
                    if (_currentLocalContext.Loops.Count > 0)
                    {
                        finalStore = _currentLocalContext.Loops.Peek();
                    }
                    else
                    {
                        // unit test: 215-for-special-var-error1.txt
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the loop variable `{variable}` not inside a loop");
                    }

                    break;
                default:
                    Debug.Assert(false, $"Variable scope `{scope}` is not implemented");
                    break;
            }

            return finalStore;
        }

        /// <summary>
        /// Returns the list of <see cref="ScriptObject"/> depending on the scope of the variable.
        /// </summary>
        /// <param name="variable"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns>The list of script objects valid for the specified variable scope</returns>
        private IEnumerable<IScriptObject> GetStoreForRead(ScriptVariable variable)
        {
            var scope = variable.Scope;

            var loopItems = _currentLocalContext.Loops.Items;

            switch (scope)
            {
                case ScriptVariableScope.Global:
                    for (int i = _currentLocalContext.Loops.Count - 1; i >= 0; i--)
                    {
                        yield return loopItems[i];
                    }

                    for (int i = _globalStores.Count - 1; i >= 0; i--)
                    {
                            yield return _globalStores.Items[i];
                    }
                    break;
                case ScriptVariableScope.Local:
                    for (int i = _currentLocalContext.Loops.Count - 1; i >= 0; i--)
                    {
                        yield return loopItems[i];
                    }

                    if (_currentLocalContext.LocalObject != null)
                    {
                        yield return _currentLocalContext.LocalObject;
                    }
                    else if (_globalStores.Count > 0)
                    {
                        yield return _globalStores.Peek();
                    }
                    else
                    {
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    }
                    break;
                case ScriptVariableScope.Loop:
                    if (_currentLocalContext.Loops.Count > 0)
                    {
                        yield return _currentLocalContext.Loops.Peek();
                    }
                    else
                    {
                        // unit test: 215-for-special-var-error1.txt
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the loop variable `{variable}` not inside a loop");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Variable scope `{scope}` is not implemented");
            }
        }

        private void CheckVariableFound(ScriptVariable variable, bool found)
        {
            //ScriptVariable.Arguments is a special "magic" variable which is not always present so ignore this
            if (StrictVariables && !found && variable != ScriptVariable.Arguments)
            {
                throw new ScriptRuntimeException(variable.Span, $"The variable or function `{variable}` was not found");
            }
        }

        /// <summary>
        /// Push a new <see cref="ScriptVariableScope"/> for variables
        /// </summary>
        /// <param name="scope"></param>
        private void PushVariableScope(ScriptVariableScope scope)
        {
            Debug.Assert(scope != ScriptVariableScope.Global);
            var store = _availableStores.Count > 0 ? _availableStores.Pop() : new ScriptObject();
            if (scope == ScriptVariableScope.Local)
            {
                PushLocalContext(store);
            }
            else
            {
                _currentLocalContext.Loops.Push(store);
            }
        }

        /// <summary>
        /// Pops a previous <see cref="ScriptVariableScope"/>.
        /// </summary>
        /// <param name="scope"></param>
        private void PopVariableScope(ScriptVariableScope scope)
        {
            Debug.Assert(scope != ScriptVariableScope.Global);
            if (scope == ScriptVariableScope.Local)
            {
                var local = PopLocalContext();
                local.Clear();
                _availableStores.Push(local);
            }
            else
            {
                if (_currentLocalContext.Loops.Count == 0)
                {
                    // Should not happen at runtime
                    throw new InvalidOperationException("Invalid number of matching push/pop VariableScope.");
                }

                var store = _currentLocalContext.Loops.Pop();
                // The store is cleanup once it is pushed back
                store.Clear();

                _availableStores.Push(store);
            }
        }

        private class LocalContext
        {
            public LocalContext(ScriptObject localObject)
            {
                LocalObject = localObject;
                Loops = new FastStack<ScriptObject>(4);
            }

            public ScriptObject LocalObject;

            public FastStack<ScriptObject> Loops;
        }
    }
}