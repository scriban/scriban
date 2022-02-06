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
using System.Threading.Tasks;
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
            _globalContexts.Push(GetOrCreateGlobalContext(scriptObject));
        }

        private VariableContext GetOrCreateGlobalContext(IScriptObject globalObject)
        {
            if (_availableGlobalContexts.Count == 0) return new VariableContext(globalObject);
            var globalContext = _availableGlobalContexts.Pop();
            globalContext.LocalObject = globalObject;
            return globalContext;
        }

        internal IScriptObject PopGlobalOnly()
        {
            if (_globalContexts.Count == 1)
            {
                throw new InvalidOperationException("Unexpected PopGlobal() not matching a PushGlobal");
            }
            var context = _globalContexts.Pop();
            var store = context.LocalObject;
            context.LocalObject = null;
            _availableGlobalContexts.Push(context);
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
            PushVariableScope(VariableScope.Local);
        }

        public void PopLocal()
        {
            PopVariableScope(VariableScope.Local);
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

            var context = variable.Scope == ScriptVariableScope.Global ? _globalContexts.Peek() : _currentLocalContext;

            if (context.Loops.Count == 0)
            {
                throw new InvalidOperationException("Cannot set a loop global variable without a loop variable store.");
            }

            var store = context.Loops.Peek();
            // Try to set the variable
            if (!store.TrySetValue(this, variable.Span, variable.Name, value, false))
            {
                throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the variable `{variable}`");
            }
        }

        private void PushLocalContext(ScriptObject locals = null)
        {
            var localContext = _availableLocalContexts.Count > 0 ? _availableLocalContexts.Pop() : new VariableContext(null);
            localContext.LocalObject = locals;
            _localContexts.Push(localContext);
            _currentLocalContext = localContext;
        }

        private ScriptObject PopLocalContext()
        {
            var oldLocalContext = _localContexts.Pop();
            var oldLocals = (ScriptObject)oldLocalContext.LocalObject;
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

            {
                var count = _globalContexts.Count;
                var items = _globalContexts.Items;
                var isInLoop = IsInLoop;
                for (int i = count - 1; i >= 0; i--)
                {
                    var context = items[i];
                    // Check loop variable first
                    if (isInLoop)
                    {
                        var loopCount = context.Loops.Count;
                        if (loopCount > 0)
                        {
                            var loopItems = context.Loops.Items;
                            for (int j = loopCount - 1; j >= 0; j--)
                            {
                                if (loopItems[j].TryGetValue(this, variable.Span, variable.Name, out value))
                                {
                                    return value;
                                }
                            }
                        }
                    }

                    if (items[i].LocalObject.TryGetValue(this, variable.Span, variable.Name, out value))
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

#if !SCRIBAN_NO_ASYNC
        public ValueTask<object> GetValueAsync(ScriptVariableGlobal variable)
        {
            return new ValueTask<object>(GetValue(variable));
        }

        public ValueTask<object> GetValueAsync(ScriptVariable variable)
        {
            return new ValueTask<object>(GetValue(variable));
        }

        public ValueTask SetValueAsync(ScriptVariable variable, object value, bool asReadOnly = false)
        {
            SetValue(variable, value, asReadOnly);
            return new ValueTask();
        }
#endif

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
                    int lastStoreIndex = _globalContexts.Count - 1;
                    var items = _globalContexts.Items;
                    for (int i = lastStoreIndex; i >= 0; i--)
                    {
                        var store = items[i].LocalObject;
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
                    finalStore = storeWithVariable ?? items[lastStoreIndex].LocalObject;
                    break;
                case ScriptVariableScope.Local:
                    if (_currentLocalContext.LocalObject != null)
                    {
                        finalStore = _currentLocalContext.LocalObject;
                    }
                    else if (_globalContexts.Count > 0)
                    {
                        finalStore = _globalContexts.Peek().LocalObject;
                    }
                    else
                    {
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
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

            switch (scope)
            {
                case ScriptVariableScope.Global:
                {
                    var isInLoop = IsInLoop;
                    for (int i = _globalContexts.Count - 1; i >= 0; i--)
                    {
                        var context = _globalContexts.Items[i];

                        // Return loop variable first
                        if (isInLoop)
                        {
                            var loopCount = context.Loops.Count;
                            if (loopCount > 0)
                            {
                                var loopItems = context.Loops.Items;
                                for (int j = loopCount - 1; j >= 0; j--)
                                {
                                    yield return loopItems[j];
                                }
                            }
                        }

                        yield return context.LocalObject;
                    }

                    break;
                }
                case ScriptVariableScope.Local:
                {
                    var loopItems = _currentLocalContext.Loops.Items;
                    for (int i = _currentLocalContext.Loops.Count - 1; i >= 0; i--)
                    {
                        yield return loopItems[i];
                    }

                    if (_currentLocalContext.LocalObject != null)
                    {
                        yield return _currentLocalContext.LocalObject;
                    }
                    else if (_globalContexts.Count > 0)
                    {
                        yield return _globalContexts.Peek().LocalObject;
                    }
                    else
                    {
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    }

                    break;
                }

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
        private void PushVariableScope(VariableScope scope)
        {
            if (scope == VariableScope.Local)
            {
                var store = _availableStores.Count > 0 ? _availableStores.Pop() : new ScriptObject();
                PushLocalContext(store);
            }
            else
            {
                var localStore = _availableStores.Count > 0 ? _availableStores.Pop() : new ScriptObject();
                var globalStore = _availableStores.Count > 0 ? _availableStores.Pop() : new ScriptObject();
                _globalContexts.Peek().Loops.Push(globalStore);
                _currentLocalContext.Loops.Push(localStore);
            }
        }

        /// <summary>
        /// Pops a previous <see cref="ScriptVariableScope"/>.
        /// </summary>
        /// <param name="scope"></param>
        private void PopVariableScope(VariableScope scope)
        {
            if (scope == VariableScope.Local)
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

                var globalStore = _globalContexts.Peek().Loops.Pop();
                // The store is cleanup once it is pushed back
                globalStore.Clear();
                _availableStores.Push(globalStore);

                var localStore = _currentLocalContext.Loops.Pop();
                // The store is cleanup once it is pushed back
                localStore.Clear();
                _availableStores.Push(localStore);
            }
        }

        private class VariableContext
        {
            public VariableContext(IScriptObject localObject)
            {
                LocalObject = localObject;
                Loops = new FastStack<ScriptObject>(4);
            }

            public IScriptObject LocalObject;

            public FastStack<ScriptObject> Loops;
        }

        private enum VariableScope
        {
            Local,
            Loop,
        }
    }
}