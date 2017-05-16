// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Model;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Runtime.Accessors;

namespace Scriban
{
    /// <summary>
    /// The template context contains the state of the page, the model.
    /// </summary>
    public class TemplateContext
    {
        private readonly Stack<ScriptObject> _availableStores;
        internal readonly Stack<ScriptBlockStatement> BlockDelegates;
        private readonly Stack<IScriptObject> _globalStores;
        private readonly Dictionary<Type, IListAccessor> _listAccessors;
        private readonly Stack<ScriptObject> _localStores;
        private readonly Stack<ScriptLoopStatementBase> _loops;
        private readonly Stack<ScriptObject> _loopStores;
        private readonly Dictionary<Type, IObjectAccessor> _memberAccessors;
        private readonly Stack<StringBuilder> _outputs;
        private readonly Stack<string> _sourceFiles;
        private int _functionDepth;
        private bool _isFunctionCallDisabled;
        private int _loopStep;

        /// <summary>
        /// A delegate used to late binding <see cref="TryGetMember"/>
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the member on the target , <c>false</c> otherwise.</returns>
        public delegate bool TryGetMemberDelegate(object target, string member, out object value);

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateContext"/> class.
        /// </summary>
        public TemplateContext() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateContext" /> class.
        /// </summary>
        /// <param name="builtin">The builtin object used to expose builtin functions, default is <see cref="GetDefaultBuiltinObject"/>.</param>
        public TemplateContext(ScriptObject builtin)
        {
            BuiltinObject = builtin ?? GetDefaultBuiltinObject();
            EnableOutput = true;
            LoopLimit = 1000;
            RecursiveLimit = 100;
            MemberRenamer = StandardMemberRenamer.Default;

            TemplateLoaderParserOptions = new ParserOptions();

            _outputs = new Stack<StringBuilder>();
            _outputs.Push(new StringBuilder());

            _globalStores = new Stack<IScriptObject>();
            _localStores = new Stack<ScriptObject>();
            _loopStores = new Stack<ScriptObject>();
            _availableStores = new Stack<ScriptObject>();

            _sourceFiles = new Stack<string>();

            _memberAccessors = new Dictionary<Type, IObjectAccessor>();
            _listAccessors = new Dictionary<Type, IListAccessor>();
            _loops = new Stack<ScriptLoopStatementBase>();
            PipeArguments = new Stack<ScriptExpression>();

            BlockDelegates = new Stack<ScriptBlockStatement>();

            _isFunctionCallDisabled = false;

            CachedTemplates = new Dictionary<string, Template>();

            Tags = new Dictionary<object, object>();

            // Ensure that builtin is registered first
            PushGlobal(BuiltinObject);
        }

        public ITemplateLoader TemplateLoader { get; set; }

        public ParserOptions TemplateLoaderParserOptions { get; set; }

        public LexerOptions TemplateLoaderLexerOptions { get; set; }

        public IMemberRenamer MemberRenamer { get; set; }

        public int LoopLimit { get; set; }

        public int RecursiveLimit { get; set; }

        public bool EnableOutput { get; set; }

        /// <summary>
        /// Gets the current output of the template being rendered (via <see cref="Template.Render(Scriban.TemplateContext)")/>.
        /// </summary>
        public StringBuilder Output => _outputs.Peek();

        /// <summary>
        /// Gets the result of the last expression.
        /// </summary>
        public object Result { get; set; }

        public ScriptObject BuiltinObject { get; }

        /// <summary>
        /// Gets the current global <see cref="ScriptObject"/>.
        /// </summary>
        public IScriptObject CurrentGlobal => _globalStores.Peek();

        /// <summary>
        /// Gets the cached templates, used by the include function.
        /// </summary>
        public Dictionary<string, Template> CachedTemplates { get; }

        /// <summary>
        /// Gets the current source file.
        /// </summary>
        public string CurrentSourceFile => _sourceFiles.Peek();

        /// <summary>
        /// Gets or sets a callback function that is called when a variable is being resolved and was not found from any scopes.
        /// </summary>
        public Func<ScriptVariable, object> TryGetVariable { get; set; }

        /// <summary>
        /// Gets or sets the fallback accessor when accessing a member of an object and the member was not found, this accessor will be called.
        /// </summary>
        public TryGetMemberDelegate TryGetMember { get; set; }

        /// <summary>
        /// Allows to store data within this context.
        /// </summary>
        public Dictionary<object, object> Tags { get; }

        internal Stack<ScriptExpression> PipeArguments { get; }

        internal ScriptFlowState FlowState { get; set; }

        /// <summary>
        /// Indicates if we are in a looop
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in loop]; otherwise, <c>false</c>.
        /// </value>
        internal bool IsInLoop => _loops.Count > 0;

        /// <summary>
        /// Pushes the source file path being executed. This should have enough information so that template loading/include can work correctly.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        public void PushSourceFile(string sourceFile)
        {
            if (sourceFile == null) throw new ArgumentNullException(nameof(sourceFile));
            _sourceFiles.Push(sourceFile);
        }

        /// <summary>
        /// Pops the source file being executed.
        /// </summary>
        /// <returns>The source file that was executed</returns>
        /// <exception cref="System.InvalidOperationException">Cannot PopSourceFile more than PushSourceFile</exception>
        public string PopSourceFile()
        {
            if (_sourceFiles.Count == 0)
            {
                throw new InvalidOperationException("Cannot PopSourceFile more than PushSourceFile");
            }
            return _sourceFiles.Pop();
        }

        /// <summary>
        /// Gets the value from the specified expression using the current <see cref="ScriptObject"/> bound to the model context.
        /// </summary>
        /// <param name="target">The expression</param>
        /// <returns>The value of the expression</returns>
        public object GetValue(ScriptExpression target)
        {
            return GetOrSetValue(target, null, false, 0);
        }

        /// <summary>
        /// Sets the variable with the specified value.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="value">The value.</param>
        /// <param name="asReadOnly">if set to <c>true</c> the variable set will be read-only.</param>
        /// <exception cref="System.ArgumentNullException">If variable is null</exception>
        /// <exception cref="ScriptRuntimeException">If an existing variable is already read-only</exception>
        public void SetValue(ScriptVariable variable, object value, bool asReadOnly)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));

            var store = GetStoreForSet(variable).First();

            // Try to set the variable
            if (!store.TrySetValue(variable.Name, value, asReadOnly))
            {
                throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the readonly variable [{variable}]"); // unit test: 105-assign-error2.txt
            }
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
            var store = GetStoreForSet(variable).First();
            store.SetReadOnly(variable.Name, isReadOnly);
        }

        /// <summary>
        /// Sets the target expression with the specified value.
        /// </summary>
        /// <param name="target">The target expression.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">If target is null</exception>
        public void SetValue(ScriptExpression target, object value)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            GetOrSetValue(target, value, true, 0);
        }

        /// <summary>
        /// Pushes a new model context accessible to the template.
        /// </summary>
        /// <param name="scriptObject">The script object.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void PushGlobal(IScriptObject scriptObject)
        {
            if (scriptObject == null) throw new ArgumentNullException(nameof(scriptObject));
            _globalStores.Push(scriptObject);
            PushVariableScope(ScriptVariableScope.Local);
        }

        /// <summary>
        /// Pops the previous model context.
        /// </summary>
        /// <returns>The previous model context</returns>
        /// <exception cref="System.InvalidOperationException">Unexpected PopGlobal() not matching a PushGlobal</exception>
        public IScriptObject PopGlobal()
        {
            if (_globalStores.Count == 1)
            {
                throw new InvalidOperationException("Unexpected PopGlobal() not matching a PushGlobal");
            }
            var store = _globalStores.Pop();
            PopVariableScope(ScriptVariableScope.Local);
            return store;
        }

        /// <summary>
        /// Pushes a new output used for rendering the current template while keeping the previous output.
        /// </summary>
        public void PushOutput()
        {
            _outputs.Push(new StringBuilder());
        }

        /// <summary>
        /// Pops a previous output.
        /// </summary>
        public string PopOutput()
        {
            if (_outputs.Count == 1)
            {
                throw new InvalidOperationException("Unexpected PopOutput for top level writer");
            }

            return _outputs.Pop().ToString();
        }

        /// <summary>
        /// Writes an object value to the current <see cref="Output"/>.
        /// </summary>
        /// <param name="span">The span of the object to render.</param>
        /// <param name="textAsObject">The text as object.</param>
        public void Write(SourceSpan span, object textAsObject)
        {
            if (textAsObject == null)
            {
                return;
            }
            var text = ScriptValueConverter.ToString(span, textAsObject);
            Write(text);
        }

        /// <summary>
        /// Writes the text to the current <see cref="Output"/>
        /// </summary>
        /// <param name="text">The text.</param>
        public void Write(string text)
        {
            if (text == null)
            {
                return;
            }

            Output.Append(text);
        }

        /// <summary>
        /// Evaluates the specified script node.
        /// </summary>
        /// <param name="scriptNode">The script node.</param>
        /// <returns>The result of the evaluation.</returns>
        /// <remarks>
        /// <see cref="Result"/> is set to null when calling directly this method.
        /// </remarks>
        public object Evaluate(ScriptNode scriptNode)
        {
            return Evaluate(scriptNode, false);
        }

        /// <summary>
        /// Evaluates the specified script node.
        /// </summary>
        /// <param name="scriptNode">The script node.</param>
        /// <param name="aliasReturnedFunction">if set to <c>true</c> and a function would be evaluated as part of this node, return the object function without evaluating it.</param>
        /// <returns>The result of the evaluation.</returns>
        /// <remarks>
        /// <see cref="Result"/> is set to null when calling directly this method.
        /// </remarks>
        public object Evaluate(ScriptNode scriptNode, bool aliasReturnedFunction)
        {
            var previousFunctionCallState = _isFunctionCallDisabled;
            try
            {
                _isFunctionCallDisabled = aliasReturnedFunction;
                scriptNode?.Evaluate(this);
                var result = Result;
                Result = null;
                return result;
            }
            finally
            {
                _isFunctionCallDisabled = previousFunctionCallState;
            }
        }

        /// <summary>
        /// Gets the member accessor for the specified object.
        /// </summary>
        /// <param name="target">The target object to get a member accessor.</param>
        /// <returns>A member accessor</returns>
        public IObjectAccessor GetMemberAccessor(object target)
        {
            if (target == null)
            {
                return NullAccessor.Default;
            }

            var type = target.GetType();
            IObjectAccessor accessor;
            if (!_memberAccessors.TryGetValue(type, out accessor))
            {
                accessor = GetMemberAccessorImpl(target) ?? NullAccessor.Default;
                _memberAccessors.Add(type, accessor);
            }
            return accessor;
        }

        protected virtual IObjectAccessor GetMemberAccessorImpl(object target)
        {
            var type = target.GetType();
            IObjectAccessor accessor;
            if (target is IScriptObject)
            {
                accessor = ScriptObjectAccessor.Default;
            }
            else if (!DictionaryAccessor.TryGet(type, out accessor))
            {
                accessor = new TypedObjectAccessor(type, MemberRenamer);
            }
            return accessor;
        }

        /// <summary>
        /// Gets a <see cref="ScriptObject"/> with all default builtins registered.
        /// </summary>
        /// <returns>A <see cref="ScriptObject"/> with all default builtins registered</returns>
        public static ScriptObject GetDefaultBuiltinObject()
        {
            var builtinObject = new BuiltinFunctions();
            return builtinObject;
        }

        internal void EnterFunction(ScriptNode caller)
        {
            _functionDepth++;
            if (_functionDepth > RecursiveLimit)
            {
                throw new ScriptRuntimeException(caller.Span, $"Exceeding number of recursive depth limit [{RecursiveLimit}] for function call: [{caller}]"); // unit test: 305-func-error2.txt
            }

            PushVariableScope(ScriptVariableScope.Local);
        }

        internal void ExitFunction()
        {
            PopVariableScope(ScriptVariableScope.Local);
            _functionDepth--;
        }

        internal void PushVariableScope(ScriptVariableScope scope)
        {
            var store = _availableStores.Count > 0 ? _availableStores.Pop() : new ScriptObject();
            (scope == ScriptVariableScope.Local ? _localStores : _loopStores).Push(store);
        }

        internal void PopVariableScope(ScriptVariableScope scope)
        {
            var stores = (scope == ScriptVariableScope.Local ? _localStores : _loopStores);
            if (stores.Count == 0)
            {
                // Should not happen at runtime
                throw new InvalidOperationException("Invalid number of matching push/pop VariableScope.");
            }

            var store = stores.Pop();
            // The store is cleanup once it is pushed back
            store.Clear();

            _availableStores.Push(store);
        }

        internal void EnterLoop(ScriptLoopStatementBase loop)
        {
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            _loops.Push(loop);
            PushVariableScope(ScriptVariableScope.Loop);
        }

        internal void ExitLoop()
        {
            PopVariableScope(ScriptVariableScope.Loop);
            _loops.Pop();
        }

        internal bool StepLoop()
        {
            Debug.Assert(_loops.Count > 0);

            _loopStep++;
            if (_loopStep > LoopLimit)
            {
                var currentLoopStatement = _loops.Peek();

                throw new ScriptRuntimeException(currentLoopStatement.Span, $"Exceeding number of iteration limit [{LoopLimit}] for statement: {currentLoopStatement}"); // unit test: 215-for-statement-error1.txt
            }
            return true;
        }

        private object GetValueInternal(ScriptVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var stores = GetStoreForSet(variable);
            object value = null;
            foreach (var store in stores)
            {
                if (store.TryGetValue(variable.Name, out value))
                {
                    return value;
                }
            }

            if (TryGetVariable != null)
            {
                value = TryGetVariable(variable);
            }

            return value;
        }

        private object GetOrSetValue(ScriptExpression targetExpression, object valueToSet, bool setter, int level)
        {
            object value = null;

            try
            {

                var nextVariable = targetExpression as ScriptVariable;
                if (nextVariable != null)
                {
                    if (setter)
                    {
                        SetValue(nextVariable, valueToSet, false);
                    }
                    else
                    {
                        value = GetValueInternal(nextVariable);
                    }
                }
                else
                {
                    var nextDot = targetExpression as ScriptMemberExpression;
                    if (nextDot != null)
                    {
                        var targetObject = GetOrSetValue(nextDot.Target, valueToSet, false, level + 1);

                        if (targetObject == null)
                        {
                            throw new ScriptRuntimeException(nextDot.Span,
                                $"Object [{nextDot.Target}] is null. Cannot access member: {nextDot}"); // unit test: 131-member-accessor-error1.txt
                        }

                        if (targetObject is string || targetObject.GetType().GetTypeInfo().IsPrimitive)
                        {
                            throw new ScriptRuntimeException(nextDot.Span,
                                $"Cannot get or set a member on the primitive [{targetObject}/{targetObject.GetType()}] when accessing member: {nextDot}"); // unit test: 132-member-accessor-error2.txt
                        }

                        var accessor = GetMemberAccessor(targetObject);

                        var memberName = nextDot.Member.Name;

                        if (setter)
                        {
                            if (!accessor.TrySetValue(targetObject, memberName, valueToSet))
                            {
                                throw new ScriptRuntimeException(nextDot.Member.Span,
                                    $"Cannot set a value for the readonly member: {nextDot}"); // unit test: 132-member-accessor-error3.txt
                            }
                        }
                        else
                        {
                            if (!accessor.TryGetValue(targetObject, memberName, out value))
                            {
                                TryGetMember?.Invoke(targetObject, memberName, out value);
                            }
                        }
                    }
                    else
                    {
                        var nextIndexer = targetExpression as ScriptIndexerExpression;
                        if (nextIndexer != null)
                        {
                            var targetObject = GetOrSetValue(nextIndexer.Target, valueToSet, false, level + 1);
                            if (targetObject == null)
                            {
                                throw new ScriptRuntimeException(nextIndexer.Target.Span,
                                    $"Object [{nextIndexer.Target}] is null. Cannot access indexer: {nextIndexer}"); // unit test: 130-indexer-accessor-error1.txt
                            }
                            else
                            {
                                var index = this.Evaluate(nextIndexer.Index);
                                if (index == null)
                                {
                                    throw new ScriptRuntimeException(nextIndexer.Index.Span,
                                        $"Cannot access target [{nextIndexer.Target}] with a null indexer: {nextIndexer}"); // unit test: 130-indexer-accessor-error2.txt
                                }
                                else
                                {
                                    if (targetObject is IDictionary || targetObject is ScriptObject)
                                    {
                                        var accessor = GetMemberAccessor(targetObject);
                                        var indexAsString =
                                            ScriptValueConverter.ToString(nextIndexer.Index.Span, index);

                                        if (setter)
                                        {
                                            if (!accessor.TrySetValue(targetObject, indexAsString, valueToSet))
                                            {
                                                throw new ScriptRuntimeException(nextIndexer.Index.Span,
                                                    $"Cannot set a value for the readonly member [{indexAsString}] in the indexer: {nextIndexer.Target}['{indexAsString}']"); // unit test: 130-indexer-accessor-error3.txt
                                            }
                                        }
                                        else
                                        {
                                            if (!accessor.TryGetValue(targetObject, indexAsString, out value))
                                            {
                                                TryGetMember?.Invoke(targetObject, indexAsString, out value);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var accessor = GetListAccessor(targetObject);
                                        if (accessor == null)
                                        {
                                            throw new ScriptRuntimeException(nextIndexer.Target.Span,
                                                $"Expecting a list. Invalid value [{targetObject}/{targetObject?.GetType().Name}] for the target [{nextIndexer.Target}] for the indexer: {nextIndexer}"); // unit test: 130-indexer-accessor-error4.txt
                                        }
                                        else
                                        {
                                            int i = ScriptValueConverter.ToInt(nextIndexer.Index.Span, index);

                                            // Allow negative index from the end of the array
                                            if (i < 0)
                                            {
                                                i = accessor.GetLength(targetObject) + i;
                                            }

                                            if (i >= 0)
                                            {
                                                if (setter)
                                                {
                                                    accessor.SetValue(targetObject, i, valueToSet);
                                                }
                                                else
                                                {
                                                    value = accessor.GetValue(targetObject, i);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (!setter)
                        {
                            targetExpression.Evaluate(this);
                            value = this.Result;
                            this.Result = null;
                        }
                        else
                        {
                            throw new ScriptRuntimeException(targetExpression.Span,
                                $"Unsupported expression for target for assignment: {targetExpression} = ..."); // unit test: 105-assign-error1.txt
                        }
                    }
                }
            }
            catch (Exception readonlyException) when(level == 0 && !(readonlyException is ScriptRuntimeException))
            {
                throw new ScriptRuntimeException(targetExpression.Span, $"Unexpected exception while accessing `{targetExpression}`", readonlyException);
            }

            // If the variable being returned is a function, we need to evaluate it
            // If function call is disabled, it will be only when returning the final object (level 0 of recursion)
            if ((!_isFunctionCallDisabled || level > 0) && ScriptFunctionCall.IsFunction(value))
            {
                value = ScriptFunctionCall.Call(this, targetExpression, value);
            }

            return value;
        }

        private IListAccessor GetListAccessor(object target)
        {
            var type = target.GetType();
            IListAccessor accessor;
            if (!_listAccessors.TryGetValue(type, out accessor))
            {
                if (type.GetTypeInfo().IsArray)
                {
                    accessor = ArrayAccessor.Default;
                }
                else if (target is IList)
                {
                    accessor = ListAccessor.Default;
                }
                _listAccessors.Add(type, accessor);
            }
            return accessor;
        }

        private IEnumerable<IScriptObject> GetStoreForSet(ScriptVariable variable)
        {
            var scope = variable.Scope; 
            if (scope == ScriptVariableScope.Global)
            {
                foreach (var store in _globalStores)
                {
                    yield return store;
                }
            }
            else if (scope == ScriptVariableScope.Local)
            {
                if (_localStores.Count > 0)
                {
                    yield return _localStores.Peek();
                }
                else
                {
                    throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable [{variable}] in the current context");
                }
            }
            else if (scope == ScriptVariableScope.Loop)
            {
                if (_loopStores.Count > 0)
                {
                    yield return _loopStores.Peek();
                }
                else
                {
                    // unit test: 215-for-special-var-error1.txt
                    throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the loop variable [{variable}] in the current context");
                }
            }
            else
            {
                throw new NotImplementedException($"Variable scope [{scope}] is not implemented");
            }
        }
    }
}