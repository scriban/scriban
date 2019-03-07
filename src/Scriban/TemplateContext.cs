// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Runtime.Accessors;
using Scriban.Syntax;

namespace Scriban
{
    /// <summary>
    /// The template context contains the state of the page, the model.
    /// </summary>
    public partial class TemplateContext
    {
        private FastStack<ScriptObject> _availableStores;
        internal FastStack<ScriptBlockStatement> BlockDelegates;
        private FastStack<IScriptObject> _globalStores;
        private FastStack<CultureInfo> _cultures;
        private readonly Dictionary<Type, IListAccessor> _listAccessors;
        private FastStack<ScriptObject> _localStores;
        private FastStack<ScriptLoopStatementBase> _loops;
        private FastStack<ScriptObject> _loopStores;
        private readonly Dictionary<Type, IObjectAccessor> _memberAccessors;
        private FastStack<IScriptOutput> _outputs;
        private IScriptOutput _output;
        private FastStack<string> _sourceFiles;
        private FastStack<object> _caseValues;
        private int _callDepth;
        private bool _isFunctionCallDisabled;
        private int _loopStep;
        private int _getOrSetValueLevel;
        private FastStack<ScriptPipeArguments> _availablePipeArguments;
        private FastStack<ScriptPipeArguments> _pipeArguments;
        private FastStack<Dictionary<object, object>> _localTagsStack;
        private FastStack<Dictionary<object, object>> _loopTagsStack;
        private FastStack<Dictionary<object, object>> _availableTags;
        private ScriptPipeArguments _currentPipeArguments;

        internal bool AllowPipeArguments => _getOrSetValueLevel <= 1;

        /// <summary>
        /// A delegate used to late binding <see cref="TryGetMember"/>
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The current sourcespan</param>
        /// <param name="target">The target.</param>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the member on the target , <c>false</c> otherwise.</returns>
        public delegate bool TryGetMemberDelegate(TemplateContext context, SourceSpan span, object target, string member, out object value);

        /// <summary>
        /// A delegate used to late binding <see cref="TryGetVariable"/>
        /// </summary>
        /// <param name="context">The template context</param>
        /// <param name="span">The current sourcespan</param>
        /// <param name="variable">The the variable to look for.</param>
        /// <param name="value">The value if the result is true.</param>
        /// <returns><c>true</c> if the variable was found, <c>false</c> otherwise.</returns>
        public delegate bool TryGetVariableDelegate(TemplateContext context, SourceSpan span, ScriptVariable variable, out object value);

        /// <summary>
        /// A delegate used to format <see cref="ScriptRuntimeException"/>s while rendering the template.
        /// The result from the delegate call will be rendered into the output.
        /// </summary>
        /// <param name="exception">The exception which occoured while rendering the template.</param>
        /// <returns>The string which will be written to the output at the position where the exception occoured.</returns>
        public delegate string RenderRuntimeExceptionDelegate(ScriptRuntimeException exception);

        /// <summary>
        /// This <see cref="RenderRuntimeExceptionDelegate"/> implementation provides a default template for rendering <see cref="ScriptRuntimeException"/>s.
        /// Assign it to the <see cref="RenderRuntimeException"/> property to get exceptions formatted as [exception message] in the output.
        /// </summary>
        public static RenderRuntimeExceptionDelegate RenderRuntimeExceptionDefault = ex => string.Format("[{0}]", ex.OriginalMessage);

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Scriban.TemplateContext" /> class.
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
            EnableBreakAndContinueAsReturnOutsideLoop = false;
            LoopLimit = 1000;
            RecursiveLimit = 100;
            MemberRenamer = StandardMemberRenamer.Default;

            RegexTimeOut = TimeSpan.FromSeconds(10);

            TemplateLoaderParserOptions = new ParserOptions();
            TemplateLoaderLexerOptions = LexerOptions.Default;

            NewLine = Environment.NewLine;

            _outputs = new FastStack<IScriptOutput>(4);
            _output = new StringBuilderOutput();
            _outputs.Push(_output);

            _globalStores = new FastStack<IScriptObject>(4);
            _localStores = new FastStack<ScriptObject>(4);
            _loopStores = new FastStack<ScriptObject>(4);
            _availableStores = new FastStack<ScriptObject>(4);
            _cultures = new FastStack<CultureInfo>(4);
            _caseValues = new FastStack<object>(4);

            _localTagsStack = new FastStack<Dictionary<object, object>>(1);
            _loopTagsStack = new FastStack<Dictionary<object, object>>(1);
            _availableTags = new FastStack<Dictionary<object, object>>(4);

            _sourceFiles = new FastStack<string>(4);

            _memberAccessors = new Dictionary<Type, IObjectAccessor>();
            _listAccessors = new Dictionary<Type, IListAccessor>();
            _loops = new FastStack<ScriptLoopStatementBase>(4);

            BlockDelegates = new FastStack<ScriptBlockStatement>(4);

            _availablePipeArguments = new FastStack<ScriptPipeArguments>(4);
            _pipeArguments = new FastStack<ScriptPipeArguments>(4);

            _isFunctionCallDisabled = false;

            CachedTemplates = new Dictionary<string, Template>();

            Tags = new Dictionary<object, object>();

            // Ensure that builtin is registered first
            PushGlobal(BuiltinObject);
        }

        /// <summary>
        /// Gets the current culture set. Default is <c>CultureInfo.InvariantCulture</c>. Can be modified via <see cref="PushCulture"/>, and <see cref="PopCulture"/>.
        /// </summary>
        public CultureInfo CurrentCulture => _cultures.Count == 0 ? CultureInfo.InvariantCulture : _cultures.Peek();

        /// <summary>
        /// Gets or sets the <see cref="ITemplateLoader"/> used by the include directive. Must be set in order for the include directive to work.
        /// </summary>
        public ITemplateLoader TemplateLoader { get; set; }

        /// <summary>
        /// Gets a boolean if the context is being used  with liquid
        /// </summary>
        public bool IsLiquid { get; protected set; }

        /// <summary>
        /// String used for new-line.
        /// </summary>
        public string NewLine { get; set; }

#if SCRIBAN_ASYNC
        /// <summary>
        /// Gets or sets the cancellation token used for async evaluation
        /// </summary>
        public CancellationToken CancellationToken { get; set; }
#endif

        /// <summary>
        /// The <see cref="ParserOptions"/> used by the <see cref="TemplateLoader"/> via the include directive.
        /// </summary>
        public ParserOptions TemplateLoaderParserOptions { get; set; }

        /// <summary>
        /// The <see cref="LexerOptions"/> used by the <see cref="TemplateLoader"/> via the include directive.
        /// </summary>
        public LexerOptions TemplateLoaderLexerOptions { get; set; }

        /// <summary>
        /// A global settings used to rename property names of exposed .NET objects.
        /// </summary>
        public MemberRenamerDelegate MemberRenamer { get; set; }

        /// <summary>
        /// A global settings used to filter field/property names of exposed .NET objects.
        /// </summary>
        public MemberFilterDelegate MemberFilter { get; set; }

        /// <summary>
        /// A loop limit that can be used at runtime to limit the number of loops. Default is 1000.
        /// </summary>
        public int LoopLimit { get; set; }

        /// <summary>
        /// A function recursive limit count used at runtime to limit the number of recursive calls. Default is 100
        /// </summary>
        public int RecursiveLimit { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating whether to enable text output via <see cref="Output"/>.
        /// </summary>
        public bool EnableOutput { get; set; }

        /// <summary>
        /// Gets the current output of the template being rendered (via <see cref="Template.Render(Scriban.TemplateContext)"/>)/>.
        /// </summary>
        public IScriptOutput Output => _output;

        /// <summary>
        /// Gets the builtin objects (that can be setup via the constructor). Default is retrieved via <see cref="GetDefaultBuiltinObject"/>.
        /// </summary>
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
        public TryGetVariableDelegate TryGetVariable { get; set; }

        /// <summary>
        /// Gets ot sets a callback function which formats a <see cref="ScriptRuntimeException"/> to a string.
        /// The result from the delegate call will be rendered into the output where the exception occoured.
        /// You can assign <see cref="TemplateContext.RenderRuntimeExceptionDefault"/> to this property to easy get default exception rendering behavior.
        /// </summary>
        public RenderRuntimeExceptionDelegate RenderRuntimeException { get; set; }

        /// <summary>
        /// Gets or sets the fallback accessor when accessing a member of an object and the member was not found, this accessor will be called.
        /// </summary>
        public TryGetMemberDelegate TryGetMember { get; set; }

        /// <summary>
        /// Allows to store data within this context.
        /// </summary>
        public Dictionary<object, object> Tags { get; }

        /// <summary>
        /// Gets the tags currently available only inside the current local scope
        /// </summary>
        public Dictionary<object, object> TagsCurrentLocal => _localTagsStack.Count == 0 ? null : _localTagsStack.Peek();

        /// <summary>
        /// Gets the tags currently available only inside the current loop
        /// </summary>
        public Dictionary<object, object> TagsCurrentLoop => _loopTagsStack.Count == 0 ? null : _loopTagsStack.Peek();

        /// <summary>
        /// Store the current stack of pipe arguments used by <see cref="ScriptPipeCall"/> and <see cref="ScriptFunctionCall"/>
        /// </summary>
        internal ScriptPipeArguments PipeArguments => _currentPipeArguments;

        /// <summary>
        /// Gets or sets the internal state of control flow.
        /// </summary>
        internal ScriptFlowState FlowState { get; set; }

        /// <summary>
        /// Timeout used for any regexp that might be used by a builtin function. Default is 10s.
        /// </summary>
        public TimeSpan RegexTimeOut { get; set; }

        /// <summary>
        /// Gets or sets a boolean indicating if the template should throw an exception if it doesn't find a variable. Default is <c>false</c>
        /// </summary>
        public bool StrictVariables { get; set; }

        /// <summary>
        /// Enables break and continue to act as a return outside of a loop, used by liquid templates. Default is <c>false</c>.
        /// </summary>
        public bool EnableBreakAndContinueAsReturnOutsideLoop { get; set; }

        /// <summary>
        /// Enables a member access on a null by returning null instead of an exception. Default is <c>false</c>
        /// </summary>
        public bool EnableRelaxedMemberAccess { get; set; }

        /// <summary>
        /// Indicates if we are in a looop
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in loop]; otherwise, <c>false</c>.
        /// </value>
        internal bool IsInLoop => _loops.Count > 0;

        /// <summary>
        /// Push a new <see cref="CultureInfo"/> to be used when rendering/parsing numbers.
        /// </summary>
        /// <param name="culture">The new culture to use when rendering/parsing numbers</param>
        public void PushCulture(CultureInfo culture)
        {
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            // Create a stack for cultures if they are actually used
            _cultures.Push(culture);
        }

        /// <summary>
        /// Pops the current culture used on the stack.
        /// </summary>
        /// <returns></returns>
        public CultureInfo PopCulture()
        {
            if (_cultures.Count == 0)
            {
                throw new InvalidOperationException("Cannot PopCulture more than PushCulture");
            }
            return _cultures.Pop();
        }

        internal void PushPipeArguments()
        {
            var pipeArguments = _availablePipeArguments.Count > 0 ? _availablePipeArguments.Pop() : new ScriptPipeArguments(4);
            _pipeArguments.Push(pipeArguments);
            _currentPipeArguments = pipeArguments;
        }

        internal void PopPipeArguments()
        {
            if (_pipeArguments.Count == 0)
            {
                throw new InvalidOperationException("Cannot PopPipeArguments more than PushPipeArguments");
            }

            var pipeArguments = _pipeArguments.Pop();
            // Might be not null in case of an exception
            pipeArguments.Clear();
            _availablePipeArguments.Push(pipeArguments);
            _currentPipeArguments = _pipeArguments.Count > 0 ? _pipeArguments.Peek() : null;
        }

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
            _getOrSetValueLevel++;
            try
            {
                return GetOrSetValue(target, null, false);
            }
            finally
            {
                _getOrSetValueLevel--;
            }
        }


        private static readonly object TrueObject = true;
        private static readonly object FalseObject = false;

        public void SetValue(ScriptVariableLoop variable, bool value)
        {
            SetValue(variable, value ? TrueObject : FalseObject);
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

            if (_loopStores.Count > 0)
            {
                // Try to set the variable
                var store = _loopStores.Peek();
                if (!store.TrySetValue(variable.Name, value, false))
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


            var scope = variable.Scope;
            IScriptObject firstStore = null;

            switch (scope)
            {
                case ScriptVariableScope.Global:
                    for (int i = _globalStores.Count - 1; i >= 0; i--)
                    {
                        var store = _globalStores.Items[i];
                        if (firstStore == null)
                        {
                            firstStore = store;
                        }

                        // We check that for upper store, we actually can write a variable with this name
                        // otherwise we don't allow to create a variable with the same name as a readonly variable
                        if (!store.CanWrite(variable.Name))
                        {
                            var variableType = store == BuiltinObject ? "builtin " : string.Empty;
                            throw new ScriptRuntimeException(variable.Span, $"Cannot set the {variableType}readonly variable `{variable}`");
                        }
                    }
                    break;
                case ScriptVariableScope.Local:
                    if (_localStores.Count > 0)
                    {
                        firstStore = _localStores.Peek();
                    }
                    else
                    {
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    }
                    break;
                case ScriptVariableScope.Loop:
                    if (_loopStores.Count > 0)
                    {
                        firstStore = _loopStores.Peek();
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

            // Try to set the variable
            if (!firstStore.TrySetValue(variable.Name, value, asReadOnly))
            {
                throw new ScriptRuntimeException(variable.Span, $"Cannot set value on the readonly variable `{variable}`"); // unit test: 105-assign-error2.txt
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
            _getOrSetValueLevel++;
            try
            {
                GetOrSetValue(target, value, true);
            }
            finally
            {
                _getOrSetValueLevel--;
            }
        }

        /// <summary>
        /// Pushes a new object context accessible to the template.
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
        /// Pops the previous object context.
        /// </summary>
        /// <returns>The previous object context</returns>
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
            PushOutput(new StringBuilderOutput());
        }

        /// <summary>
        /// Pushes a new output used for rendering the current template while keeping the previous output.
        /// </summary>
        public void PushOutput(IScriptOutput output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _outputs.Push(_output);
        }

        /// <summary>
        /// Pops a previous output.
        /// </summary>
        public IScriptOutput PopOutput()
        {
            if (_outputs.Count == 1)
            {
                throw new InvalidOperationException("Unexpected PopOutput for top level writer");
            }

            var previous = _outputs.Pop();
            _output = _outputs.Peek();
            return previous;
        }

        /// <summary>
        /// Writes an object value to the current <see cref="Output"/>.
        /// </summary>
        /// <param name="span">The span of the object to render.</param>
        /// <param name="textAsObject">The text as object.</param>
        public TemplateContext Write(SourceSpan span, object textAsObject)
        {
            if (textAsObject != null)
            {
                var text = ToString(span, textAsObject);
                Write(text);
            }
            return this;
        }

        /// <summary>
        /// Writes the text to the current <see cref="Output"/>
        /// </summary>
        /// <param name="text">The text.</param>
        public TemplateContext Write(string text)
        {
            if (text != null)
            {
                Output.Write(text);
            }
            return this;
        }

        /// <summary>
        /// Writes the a new line to the current <see cref="Output"/>
        /// </summary>
        public TemplateContext WriteLine()
        {
            Output.Write(NewLine);
            return this;
        }

        /// <summary>
        /// Writes the text to the current <see cref="Output"/>
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="startIndex">The zero-based position of the substring of text</param>
        /// <param name="count">The number of characters to output starting at <paramref name="startIndex"/> position from the text</param>
        public TemplateContext Write(string text, int startIndex, int count)
        {
            if (text != null)
            {
                Output.Write(text, startIndex, count);
            }

            return this;
        }

        /// <summary>
        /// Evaluates the specified script node.
        /// </summary>
        /// <param name="scriptNode">The script node.</param>
        /// <returns>The result of the evaluation.</returns>
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
        public object Evaluate(ScriptNode scriptNode, bool aliasReturnedFunction)
        {
            var previousFunctionCallState = _isFunctionCallDisabled;
            var previousLevel = _getOrSetValueLevel;
            try
            {
                _getOrSetValueLevel = 0;
                _isFunctionCallDisabled = aliasReturnedFunction;
                return EvaluateImpl(scriptNode);
            }
            finally
            {
                _getOrSetValueLevel = previousLevel;
                _isFunctionCallDisabled = previousFunctionCallState;
            }
        }

        /// <summary>
        /// Evaluates the specified script node by calling <see cref="ScriptNode.Evaluate"/>
        /// </summary>
        /// <param name="scriptNode">The script node (might be null but should not throw an error)</param>
        /// <returns>The result of the evaluation</returns>
        /// <remarks>The purpose of this method is to allow to hook during the evaluation of all ScriptNode. By default calls <see cref="ScriptNode.Evaluate"/></remarks>
        protected virtual object EvaluateImpl(ScriptNode scriptNode)
        {
            try
            {
                return scriptNode != null ? scriptNode.Evaluate(this) : null;
            }
            catch (ScriptRuntimeException ex) when (this.RenderRuntimeException != null)
            {
                return this.RenderRuntimeException(ex);
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
            if (!_memberAccessors.TryGetValue(type, out var accessor))
            {
                accessor = GetMemberAccessorImpl(target) ?? NullAccessor.Default;
                _memberAccessors.Add(type, accessor);
            }
            return accessor;
        }

        /// <summary>
        /// Gets the member accessor for the specified object if not already cached. This method can have an override.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual IObjectAccessor GetMemberAccessorImpl(object target)
        {
            var type = target.GetType();
            IObjectAccessor accessor;
            if (target is IScriptObject)
            {
                accessor = ScriptObjectAccessor.Default;
            }
            else if (DictionaryAccessor.TryGet(target, out accessor))
            {
            }
            else if (type.GetTypeInfo().IsArray)
            {
                accessor = ArrayAccessor.Default;
            }
            else if (target is IList)
            {
                accessor = ListAccessor.Default;
            }
            else
            {
                accessor = new TypedObjectAccessor(type, MemberFilter, MemberRenamer);
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

        public void EnterRecursive(ScriptNode node)
        {
            _callDepth++;
            if (_callDepth > RecursiveLimit)
            {
                throw new ScriptRuntimeException(node.Span, $"Exceeding number of recursive depth limit `{RecursiveLimit}` for node: `{node}`"); // unit test: 305-func-error2.txt
            }
        }

        public void ExitRecursive(ScriptNode node)
        {
            _callDepth--;
            if (_callDepth < 0)
            {
                throw new InvalidOperationException($"unexpected ExitRecursive not matching EnterRecursive for `{node}`");
            }
        }

        /// <summary>
        /// Called when entering a function.
        /// </summary>
        /// <param name="caller"></param>
        internal void EnterFunction(ScriptNode caller)
        {
            EnterRecursive(caller);
            PushVariableScope(ScriptVariableScope.Local);
        }

        /// <summary>
        /// Called when exiting a function.
        /// </summary>
        internal void ExitFunction()
        {
            PopVariableScope(ScriptVariableScope.Local);
            _callDepth--;
        }

        /// <summary>
        /// Push a new <see cref="ScriptVariableScope"/> for variables
        /// </summary>
        /// <param name="scope"></param>
        internal void PushVariableScope(ScriptVariableScope scope)
        {
            var store = _availableStores.Count > 0 ? _availableStores.Pop() : new ScriptObject();
            var tags = _availableTags.Count > 0 ? _availableTags.Pop() : new Dictionary<object, object>();
            if (scope == ScriptVariableScope.Local)
            {
                _localStores.Push(store);
                _localTagsStack.Push(tags);
            }
            else
            {
                _loopStores.Push(store);
                _loopTagsStack.Push(tags);
            }
        }

        /// <summary>
        /// Pops a previous <see cref="ScriptVariableScope"/>.
        /// </summary>
        /// <param name="scope"></param>
        internal void PopVariableScope(ScriptVariableScope scope)
        {
            Dictionary<object, object> tags;
            if (scope == ScriptVariableScope.Local)
            {
                PopVariableScope(ref _localStores);
                tags = _localTagsStack.Pop();
            }
            else
            {
                PopVariableScope(ref _loopStores);
                tags = _loopTagsStack.Pop();
            }
            // Make sure that tags are clear
            tags.Clear();
            _availableTags.Push(tags);
        }

        /// <summary>
        /// Pops a previous <see cref="ScriptVariableScope"/>.
        /// </summary>
        internal void PopVariableScope(ref FastStack<ScriptObject> stores)
        {
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

        /// <summary>
        /// Notifies this context when entering a loop.
        /// </summary>
        /// <param name="loop"></param>
        internal void EnterLoop(ScriptLoopStatementBase loop)
        {
            if (loop == null) throw new ArgumentNullException(nameof(loop));
            _loops.Push(loop);
            _loopStep = 0;
            PushVariableScope(ScriptVariableScope.Loop);
            OnEnterLoop(loop);
        }

        /// <summary>
        /// Called when entering a loop.
        /// </summary>
        /// <param name="loop">The loop expression object</param>
        protected virtual void OnEnterLoop(ScriptLoopStatementBase loop)
        {
        }

        /// <summary>
        /// Notifies this context when exiting a loop.
        /// </summary>
        internal void ExitLoop(ScriptLoopStatementBase loop)
        {
            OnExitLoop(loop);
            PopVariableScope(ScriptVariableScope.Loop);
            _loops.Pop();
            _loopStep = 0;
        }

        /// <summary>
        /// Called when exiting a loop.
        /// </summary>
        /// <param name="loop">The loop expression object</param>
        protected virtual void OnExitLoop(ScriptLoopStatementBase loop)
        {
        }

        internal bool StepLoop(ScriptLoopStatementBase loop)
        {
            Debug.Assert(_loops.Count > 0);

            _loopStep++;
            if (_loopStep > LoopLimit)
            {
                var currentLoopStatement = _loops.Peek();

                throw new ScriptRuntimeException(currentLoopStatement.Span, $"Exceeding number of iteration limit `{LoopLimit}` for statement: {currentLoopStatement}"); // unit test: 215-for-statement-error1.txt
            }
            return OnStepLoop(loop);
        }

        /// <summary>
        /// Called when stepping into a loop.
        /// </summary>
        /// <param name="loop">The loop expression object</param>
        /// <returns><c>true</c> to continue loop; <c>false</c> to break the loop. Default is <c>true</c></returns>
        protected virtual bool OnStepLoop(ScriptLoopStatementBase loop)
        {
            return true;
        }

        internal void PushCase(object caseValue)
        {
            _caseValues.Push(caseValue);
        }

        internal object PeekCase()
        {
            return _caseValues.Peek();
        }

        internal object PopCase()
        {
            if (_caseValues.Count == 0)
            {
                throw new InvalidOperationException("Cannot PopCase more than PushCase");
            }
            return _caseValues.Pop();
        }

        /// <summary>
        /// Gets the value for the specified variable from the current object context/scope.
        /// </summary>
        /// <param name="variable">The variable to retrieve the value</param>
        /// <returns>Value of the variable</returns>
        public object GetValue(ScriptVariable variable)
        {
            if (variable == null) throw new ArgumentNullException(nameof(variable));
            var stores = GetStoreForSet(variable);
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

            if (StrictVariables && !found)
            {
                throw new ScriptRuntimeException(variable.Span, $"The variable `{variable}` was not found");
            }
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
            var count = _globalStores.Count;
            var items = _globalStores.Items;
            for (int i = count - 1; i >= 0; i--)
            {
                if (items[i].TryGetValue(this, variable.Span, variable.Name, out value))
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

            if (StrictVariables && !found)
            {
                throw new ScriptRuntimeException(variable.Span, $"The variable `{variable}` was not found");
            }
            return value;
        }

        /// <summary>
        /// Evaluates the specified expression
        /// </summary>
        /// <param name="targetExpression">The expression to evaluate</param>
        /// <param name="valueToSet">A value to set in case of a setter</param>
        /// <param name="setter">true if this a setter</param>
        /// <returns>The value of the targetExpression</returns>
        private object GetOrSetValue(ScriptExpression targetExpression, object valueToSet, bool setter)
        {
            object value = null;

            try
            {
                if (targetExpression is IScriptVariablePath nextPath)
                {
                    if (setter)
                    {
                        nextPath.SetValue(this, valueToSet);
                    }
                    else
                    {
                        value = nextPath.GetValue(this);
                    }
                }
                else if (!setter)
                {
                    value = Evaluate(targetExpression);
                }
                else
                {
                    throw new ScriptRuntimeException(targetExpression.Span, $"Unsupported expression for target for assignment: {targetExpression} = ..."); // unit test: 105-assign-error1.txt
                }
            }
            catch (Exception readonlyException) when(_getOrSetValueLevel == 1 && !(readonlyException is ScriptRuntimeException))
            {
                throw new ScriptRuntimeException(targetExpression.Span, $"Unexpected exception while accessing `{targetExpression}`", readonlyException);
            }

            // If the variable being returned is a function, we need to evaluate it
            // If function call is disabled, it will be only when returning the final object (level 0 of recursion)
            var allowFunctionCall = (_isFunctionCallDisabled && _getOrSetValueLevel > 1) || !_isFunctionCallDisabled;
            if (allowFunctionCall && ScriptFunctionCall.IsFunction(value))
            {
                // Allow to pipe arguments only for top level returned function
                value = ScriptFunctionCall.Call(this, targetExpression, value, _getOrSetValueLevel == 1);
            }

            return value;
        }

        /// <summary>
        /// Gets the list accessor or a previous cached one.
        /// </summary>
        /// <param name="target">The expected object to be a list</param>
        /// <returns>A list accessor for the specified type of target</returns>
        public IListAccessor GetListAccessor(object target)
        {
            var type = target.GetType();
            if (!_listAccessors.TryGetValue(type, out var accessor))
            {
                accessor = GetListAccessorImpl(target, type);
                _listAccessors.Add(type, accessor);
            }
            return accessor;
        }

        /// <summary>
        /// Gets the list accessor for the specified target and type, if it hasn't been found yet.
        /// </summary>
        /// <param name="target">The expected object to be a list</param>
        /// <param name="type">Type of the target object</param>
        /// <returns>A list accessor for the specified type of target</returns>
        protected virtual IListAccessor GetListAccessorImpl(object target, Type type)
        {
            if (type.GetTypeInfo().IsArray)
            {
                return ArrayAccessor.Default;
            }

            if (target is IList)
            {
                return ListAccessor.Default;
            }
            return null;
        }

        /// <summary>
        /// Returns the list of <see cref="ScriptObject"/> depending on the scope of the variable.
        /// </summary>
        /// <param name="variable"></param>
        /// <exception cref="NotImplementedException"></exception>
        /// <returns>The list of script objects valid for the specified variable scope</returns>
        private IEnumerable<IScriptObject> GetStoreForSet(ScriptVariable variable)
        {
            var scope = variable.Scope;
            switch (scope)
            {
                case ScriptVariableScope.Global:
                    for (int i = _globalStores.Count - 1; i >= 0; i--)
                    {
                            yield return _globalStores.Items[i];
                    }
                    break;
                case ScriptVariableScope.Local:
                    if (_localStores.Count > 0)
                    {
                        yield return _localStores.Peek();
                    }
                    else
                    {
                        throw new ScriptRuntimeException(variable.Span, $"Invalid usage of the local variable `{variable}` in the current context");
                    }
                    break;
                case ScriptVariableScope.Loop:
                    if (_loopStores.Count > 0)
                    {
                        yield return _loopStores.Peek();
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
    }

    /// <summary>
    /// A Liquid based <see cref="TemplateContext"/> providing the builtin functions usually available for a liquid template.
    /// </summary>
    public class LiquidTemplateContext : TemplateContext
    {
        public LiquidTemplateContext() : base(new LiquidBuiltinsFunctions())
        {
            // In liquid, if we have a break/continue outside a loop, we return from the current script
            EnableBreakAndContinueAsReturnOutsideLoop = true;
            EnableRelaxedMemberAccess = true;

            TemplateLoaderLexerOptions = new LexerOptions() {Mode = ScriptMode.Liquid};
            TemplateLoaderParserOptions = new ParserOptions() {LiquidFunctionsToScriban = true};
            IsLiquid = true;
        }
    }
}