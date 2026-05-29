// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
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
using System.Xml.Linq;

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
            if (scriptObject is null) throw new ArgumentNullException(nameof(scriptObject));
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
            if (store is null)
            {
                throw new InvalidOperationException("Unexpected null global store.");
            }

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

        /// <summary>
        /// Creates a new context for local variables.
        /// </summary>
        public void PushLocal()
        {
            PushVariableScope(VariableScope.Local);
        }

        public void PopLocal()
        {
            PopVariableScope(VariableScope.Local);
        }

        internal void PushFunction(ScriptObject? functionVariables, bool hideParentFunctionScopes)
        {
            var functionContext = _availableFunctionContexts.Count > 0 ? _availableFunctionContexts.Pop() : new VariableContext(null);
            functionContext.LocalObject = functionVariables;
            functionContext.HideParentFunctionScopes = hideParentFunctionScopes;
            _functionContexts.Push(functionContext);
            PushLocal();
        }

        internal void PopFunction()
        {
            var functionContext = _functionContexts.Pop();
            PopLocal();

            if (functionContext.LocalObject is ScriptObject functionVariables)
            {
                functionVariables.Clear();
            }
            functionContext.LocalObject = null;
            functionContext.HideParentFunctionScopes = false;
            _availableFunctionContexts.Push(functionContext);
        }

        /// <summary>
        /// Sets the variable with the specified value.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="value">The value.</param>
        /// <param name="asReadOnly">if set to <c>true</c> the variable set will be read-only.</param>
        /// <exception cref="System.ArgumentNullException">If variable is null</exception>
        /// <exception cref="ScriptRuntimeException">If an existing variable is already read-only</exception>
        public void SetValue(ScriptVariable variable, object? value, bool asReadOnly = false)
        {
            if (variable is null) throw new ArgumentNullException(nameof(variable));
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
        public void SetValue(ScriptVariable variable, object? value, bool asReadOnly, bool force)
        {
            if (variable is null) throw new ArgumentNullException(nameof(variable));
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
            if (variable is null) throw new ArgumentNullException(nameof(variable));
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
            if (variable is null) throw new ArgumentNullException(nameof(variable));
            var store = GetStoreForWrite(variable);
            store.SetReadOnly(variable.Name, isReadOnly);
        }

        /// <summary>
        /// Sets the loop variable with the specified value.
        /// </summary>
        /// <param name="variable">The loop variable to set.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">If target is null</exception>
        public virtual void SetLoopVariable(ScriptVariable variable, object? value)
        {
            if (variable is null) throw new ArgumentNullException(nameof(variable));

            var context = variable.Scope == ScriptVariableScope.Global
                ? GetCurrentGlobalVariableContext()
                : _currentLocalContext ?? throw new InvalidOperationException("No current local context is available.");

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

        private void PushLocalContext(ScriptObject? locals = null)
        {
            var localContext = _availableLocalContexts.Count > 0 ? _availableLocalContexts.Pop() : new VariableContext(null);
            localContext.LocalObject = locals;
            _localContexts.Push(localContext);
            _currentLocalContext = localContext;
        }

        private ScriptObject? PopLocalContext()
        {
            var oldLocalContext = _localContexts.Pop();
            var oldLocals = oldLocalContext.LocalObject as ScriptObject;
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
        /// <exception cref="ScriptRuntimeException">If <see cref="StrictVariables"/> is enabled and the specified variable is undefined.</exception>
        public object? GetValue(ScriptVariable variable)
        {
            if (variable is null) throw new ArgumentNullException(nameof(variable));

            var stores = GetStoreForRead(variable);
            object? value = null;
            foreach (var store in stores)
            {
                if (store.TryGetValue(this, variable.Span, variable.Name, out value))
                {
                    return value;
                }
            }

            bool found = false;
            if (TryGetVariable is not null)
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
        /// <exception cref="ScriptRuntimeException">If <see cref="StrictVariables"/> is enabled and the specified variable is undefined.</exception>
        public object? GetValue(ScriptVariableGlobal variable)
        {
            if (variable is null) throw new ArgumentNullException(nameof(variable));
            object? value = null;

            foreach (var functionContext in GetVisibleFunctionContexts())
            {
                if (TryGetValue(functionContext, variable, out value))
                {
                    return value;
                }
            }

            {
                var count = _globalContexts.Count;
                var items = _globalContexts.Items;
                for (int i = count - 1; i >= 0; i--)
                {
                    var context = items[i];
                    if (TryGetValue(context, variable, out value))
                    {
                        return value;
                    }
                }
            }

            bool found = false;
            if (TryGetVariable is not null)
            {
                if (TryGetVariable(this, variable.Span, variable, out value))
                {
                    found = true;
                }
            }

            CheckVariableFound(variable, found);
            return value;
        }

        internal object? AwaitIfNeeded(object? value)
        {
            return value;
        }

#if !SCRIBAN_NO_ASYNC
        internal async ValueTask<object?> AwaitIfNeededAsync(object? value)
        {
            if (value is null)
            {
                return null;
            }

            if (value is Task task)
            {
                await task.ConfigureAwait(false);
                return task.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(task);
            }

            if (value is ValueTask valueTask)
            {
                await valueTask.ConfigureAwait(false);
                return null;
            }

            var type = value.GetType();
            if (type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                var asTaskMethod = type.GetMethod(nameof(ValueTask<object>.AsTask), Type.EmptyTypes);
                if (asTaskMethod is null)
                {
                    return value;
                }

                var awaitedTask = asTaskMethod.Invoke(value, null) as Task;
                if (awaitedTask is null)
                {
                    return value;
                }

                await awaitedTask.ConfigureAwait(false);
                return awaitedTask.GetType().GetProperty(nameof(Task<object>.Result))?.GetValue(awaitedTask);
            }

            return value;
        }

        public ValueTask<object?> GetValueAsync(ScriptVariableGlobal variable)
        {
            return new ValueTask<object?>(GetValue(variable));
        }

        public ValueTask<object?> GetValueAsync(ScriptVariable variable)
        {
            return new ValueTask<object?>(GetValue(variable));
        }

        public ValueTask SetValueAsync(ScriptVariable variable, object? value, bool asReadOnly = false)
        {
            SetValue(variable, value, asReadOnly);
            return new ValueTask();
        }
#endif

        private IScriptObject GetStoreForWrite(ScriptVariable variable)
        {
            var scope = variable.Scope;
            IScriptObject? finalStore = null;

            switch (scope)
            {
                case ScriptVariableScope.Global:
                    finalStore = GetGlobalStoreForWrite(variable);
                    break;
                case ScriptVariableScope.Local:
                    var currentLocalContext = _currentLocalContext ?? throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    if (currentLocalContext.LocalObject is not null)
                    {
                        finalStore = currentLocalContext.LocalObject;
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

            if (finalStore is null)
            {
                throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the variable `{variable}` in the current context");
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
                    foreach (var functionContext in GetVisibleFunctionContexts())
                    {
                        foreach (var store in GetStoresForRead(functionContext))
                        {
                            yield return store;
                        }
                    }

                    for (int i = _globalContexts.Count - 1; i >= 0; i--)
                    {
                        var context = _globalContexts.Items[i];
                        foreach (var store in GetStoresForRead(context))
                        {
                            yield return store;
                        }
                    }

                    break;
                }
                case ScriptVariableScope.Local:
                {
                    var currentLocalContext = _currentLocalContext ?? throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    var loopItems = currentLocalContext.Loops.Items;
                    for (int i = currentLocalContext.Loops.Count - 1; i >= 0; i--)
                    {
                        yield return loopItems[i];
                    }

                    if (currentLocalContext.LocalObject is not null)
                    {
                        yield return currentLocalContext.LocalObject;
                    }
                    else if (_globalContexts.Count > 0)
                    {
                        var globalObject = _globalContexts.Peek().LocalObject;
                        if (globalObject is null)
                        {
                            throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                        }

                        yield return globalObject;
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

        private IEnumerable<VariableContext> GetVisibleFunctionContexts()
        {
            for (int i = _functionContexts.Count - 1; i >= 0; i--)
            {
                var functionContext = _functionContexts.Items[i];
                yield return functionContext;

                if (functionContext.HideParentFunctionScopes)
                {
                    yield break;
                }
            }
        }

        private VariableContext GetCurrentGlobalVariableContext()
        {
            foreach (var currentFunctionContext in GetVisibleFunctionContexts())
            {
                if (currentFunctionContext.LocalObject is not null)
                {
                    return currentFunctionContext;
                }
            }

            return _globalContexts.Peek();
        }

        private IScriptObject GetGlobalStoreForWrite(ScriptVariable variable)
        {
            var name = variable.Name;
            var currentGlobal = _globalContexts.Peek().LocalObject;
            if (currentGlobal is null)
            {
                throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the global variable `{variable}` in the current context");
            }

            IScriptObject? currentFunctionStore = null;
            foreach (var functionContext in GetVisibleFunctionContexts())
            {
                var functionStore = functionContext.LocalObject;
                if (functionStore is null)
                {
                    continue;
                }

                currentFunctionStore ??= functionStore;
                if (functionStore.Contains(name))
                {
                    CheckStoreCanWrite(variable, functionStore);
                    return functionStore;
                }
            }

            if (currentFunctionStore is null)
            {
                CheckStoreCanWrite(variable, currentGlobal);
                return currentGlobal;
            }

            if (ContainsInGlobalContexts(name))
            {
                CheckStoreCanWrite(variable, currentGlobal);
                return currentGlobal;
            }

            CheckStoreCanWrite(variable, currentFunctionStore);
            return currentFunctionStore;
        }

        private bool ContainsInGlobalContexts(string name)
        {
            for (int i = _globalContexts.Count - 1; i >= 0; i--)
            {
                if (_globalContexts.Items[i].LocalObject?.Contains(name) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<IScriptObject> GetStoresForRead(VariableContext context)
        {
            var loopItems = context.Loops.Items;
            for (int i = context.Loops.Count - 1; i >= 0; i--)
            {
                yield return loopItems[i];
            }

            if (context.LocalObject is not null)
            {
                yield return context.LocalObject;
            }
        }

        private bool TryGetValue(VariableContext context, ScriptVariable variable, out object? value)
        {
            foreach (var store in GetStoresForRead(context))
            {
                if (store.TryGetValue(this, variable.Span, variable.Name, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }

        private void CheckStoreCanWrite(ScriptVariable variable, IScriptObject store)
        {
            if (!store.CanWrite(variable.Name))
            {
                var variableType = store == BuiltinObject ? "builtin " : string.Empty;
                throw new ScriptRuntimeException(variable.Span, $"Cannot set the {variableType}readonly variable `{variable}`");
            }
        }

        private void CheckVariableFound(ScriptVariable variable, bool found)
        {
            if (StrictVariables && !found)
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
                GetCurrentGlobalVariableContext().Loops.Push(globalStore);
                var currentLocalContext = _currentLocalContext ?? throw new InvalidOperationException("No current local context is available.");
                currentLocalContext.Loops.Push(localStore);
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
                if (local is null)
                {
                    throw new InvalidOperationException("Invalid number of matching push/pop VariableScope.");
                }

                local.Clear();
                _availableStores.Push(local);
            }
            else
            {
                var currentLocalContext = _currentLocalContext ?? throw new InvalidOperationException("No current local context is available.");
                if (currentLocalContext.Loops.Count == 0)
                {
                    // Should not happen at runtime
                    throw new InvalidOperationException("Invalid number of matching push/pop VariableScope.");
                }

                var globalStore = GetCurrentGlobalVariableContext().Loops.Pop();
                // The store is cleanup once it is pushed back
                globalStore.Clear();
                _availableStores.Push(globalStore);

                var localStore = currentLocalContext.Loops.Pop();
                // The store is cleanup once it is pushed back
                localStore.Clear();
                _availableStores.Push(localStore);
            }
        }

        private class VariableContext
        {
            public VariableContext(IScriptObject? localObject)
            {
                LocalObject = localObject;
                Loops = new FastStack<ScriptObject>(4);
            }

            public IScriptObject? LocalObject;

            public FastStack<ScriptObject> Loops;

            public bool HideParentFunctionScopes;
        }

        private enum VariableScope
        {
            Local,
            Loop,
        }
    }
}
