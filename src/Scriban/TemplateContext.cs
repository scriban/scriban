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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Scriban.Functions;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Runtime.Accessors;
using Scriban.Syntax;

#if !SCRIBAN_SIGNED
[assembly: InternalsVisibleTo("Scriban.Tests")]
#endif

namespace Scriban
{
    /// <summary>
    /// The template context contains the state of the page, the model.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class TemplateContext
    {
        private FastStack<ScriptObject> _availableStores;
        internal FastStack<ScriptBlockStatement> BlockDelegates;
        private FastStack<VariableContext> _globalContexts;
        private FastStack<CultureInfo> _cultures;
        private readonly Dictionary<Type, IListAccessor> _listAccessors;
        private FastStack<ScriptLoopStatementBase> _loops;
        private readonly Dictionary<Type, IObjectAccessor> _memberAccessors;
        private FastStack<IScriptOutput> _outputs;
        private FastStack<VariableContext> _localContexts;
        private VariableContext _currentLocalContext;
        private IScriptOutput _output;
        private FastStack<string> _sourceFiles;
        private FastStack<object> _caseValues;
        private int _callDepth;
        private bool _isFunctionCallDisabled;
        private int _loopStep;
        private int _getOrSetValueLevel;
        private FastStack<VariableContext> _availableGlobalContexts;
        private FastStack<VariableContext> _availableLocalContexts;
        private FastStack<ScriptPipeArguments> _availablePipeArguments;
        private FastStack<ScriptPipeArguments> _pipeArguments;
        private FastStack<List<ScriptExpression>> _availableScriptExpressionLists;
        private object[][] _availableReflectionArguments;
        private FastStack<Dictionary<object, object>> _availableTags;
        private ScriptPipeArguments _currentPipeArguments;
        private bool _previousTextWasNewLine;

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
            EnableRelaxedTargetAccess = false;
            EnableRelaxedMemberAccess = true;
            EnableRelaxedFunctionAccess = false;
            EnableRelaxedIndexerAccess = true;
            AutoIndent = true;
            LoopLimit = 1000;
            RecursiveLimit = 100;
            LimitToString = 0;
            ObjectRecursionLimit=0;
            MemberRenamer = StandardMemberRenamer.Default;

            RegexTimeOut = TimeSpan.FromSeconds(10);

            TemplateLoaderParserOptions = new ParserOptions();
            TemplateLoaderLexerOptions = LexerOptions.Default;

            NewLine = Environment.NewLine;

            Language = ScriptLang.Default;

            _outputs = new FastStack<IScriptOutput>(4);
            _output = new StringBuilderOutput();
            _outputs.Push(_output);

            _globalContexts = new FastStack<VariableContext>(4);
            _availableGlobalContexts = new FastStack<VariableContext>(4);
            _availableLocalContexts = new FastStack<VariableContext>(4);
            _localContexts = new FastStack<VariableContext>(4);
            _availableStores = new FastStack<ScriptObject>(4);
            _cultures = new FastStack<CultureInfo>(4);
            _caseValues = new FastStack<object>(4);

            _availableTags = new FastStack<Dictionary<object, object>>(4);

            _sourceFiles = new FastStack<string>(4);

            _memberAccessors = new Dictionary<Type, IObjectAccessor>();
            _listAccessors = new Dictionary<Type, IListAccessor>();
            _loops = new FastStack<ScriptLoopStatementBase>(4);

            BlockDelegates = new FastStack<ScriptBlockStatement>(4);

            _availablePipeArguments = new FastStack<ScriptPipeArguments>(4);
            _pipeArguments = new FastStack<ScriptPipeArguments>(4);
            _availableScriptExpressionLists = new FastStack<List<ScriptExpression>>(4);
            _availableReflectionArguments = new object[ScriptFunctionCall.MaximumParameterCount + 1][];
            for (int i = 0; i < _availableReflectionArguments.Length; i++)
            {
                _availableReflectionArguments[i] = new object[i];
            }
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
        /// If sets to <c>true</c>, the include statement will maintain the indent.
        /// </summary>
        public bool AutoIndent { get; set; }

        /// <summary>
        /// If sets to <c>true</c>, the include statement will maintain the indent.
        /// </summary>
        [Obsolete("Use AutoIndent instead. Note that AutoIndent is true by default.")]
        public bool IndentWithInclude
        {
            get => AutoIndent;
            set => AutoIndent = value;
        }

        /// <summary>
        /// Gets or sets the buffer limit in characters for a ToString in a list/string. Default is 0, no limit.
        /// </summary>
        public int LimitToString { get; set; }

        /// <summary>
        /// Gets or sets the maximum recursion depth while traversing an object graph during the ToString operation.  Default is 0, no limit
        /// </summary>
        public int ObjectRecursionLimit { get; set; }

        /// <summary>
        /// String used for new-line.
        /// </summary>
        public string NewLine { get; set; }

        /// <summary>
        /// Gets or sets the default scripting language - used for example by <see cref="ObjectFunctions.Eval"/>.
        /// </summary>
        public ScriptLang Language { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token used for async evaluation
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

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
        /// Set to 0 to disable checking loop limit.
        /// </summary>
        public int LoopLimit { get; set; }


        /// <summary>
        /// A loop limit that can be used at runtime to limit the number of loops over a IQueryable object. Defaults to LoopLimit property.
        /// Set to 0 to disable checking IQueryable loop limit.
        /// </summary>
        public int? LoopLimitQueryable { get; set; }

        /// <summary>
        /// A function recursive limit count used at runtime to limit the number of recursive calls. Default is 100
        /// Set to 0 to disable checking recursive call limit.
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
        /// Gets or sets a boolean indicating if the runtime is running in scientific mode.
        /// </summary>
        public bool UseScientific { get; set; }

        /// <summary>
        /// Generates an explicit error for function without a return value that are used in an expression.
        /// </summary>
        public bool ErrorForStatementFunctionAsExpression { get; set; }

        /// <summary>
        /// Gets the builtin objects (that can be setup via the constructor). Default is retrieved via <see cref="GetDefaultBuiltinObject"/>.
        /// </summary>
        public ScriptObject BuiltinObject { get; }

        /// <summary>
        /// Gets the current global <see cref="ScriptObject"/>.
        /// </summary>
        public IScriptObject CurrentGlobal => _globalContexts.Peek()?.LocalObject;

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
        /// Store the current stack of pipe arguments used by <see cref="ScriptPipeCall"/> and <see cref="ScriptFunctionCall"/>
        /// </summary>
        internal ScriptPipeArguments CurrentPipeArguments => _currentPipeArguments;

        /// <summary>
        /// Gets the number of <see cref="PushGlobal"/> that are pushed to this context.
        /// </summary>
        public int GlobalCount => _globalContexts.Count;

        /// <summary>
        /// Gets the number of <see cref="PushOutput()"/> that are pushed to this context.
        /// </summary>
        public int OutputCount => _outputs.Count;
        
        /// <summary>
        /// Gets the number of <see cref="PushCulture"/> that are pushed to this context.
        /// </summary>
        public int CultureCount => _cultures.Count;

        /// <summary>
        /// Gets the number of <see cref="PushSourceFile"/> that are pushed to this context.
        /// </summary>
        public int SourceFileCount => _sourceFiles.Count;

        /// <summary>
        /// Gets or sets the internal state of control flow.
        /// </summary>
        internal ScriptFlowState FlowState { get; set; }

        /// <summary>
        /// Timeout used for any regexp that might be used by a builtin function. Default is 10s.
        /// Set to <see cref="System.Text.RegularExpressions.Regex.InfiniteMatchTimeout"/> to disable regexp timeouts
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
        /// Enables the (e.g x) target of a member access (e.g x.y) to be null by returning null instead of an exception. Default is <c>false</c>
        /// </summary>
        public bool EnableRelaxedTargetAccess { get; set; }

        /// <summary>
        /// Enables a (e.g x.y) member/indexer access that does not exist to return null instead of an exception. Default is <c>true</c>
        /// </summary>
        public bool EnableRelaxedMemberAccess { get; set; }

        /// <summary>
        /// Enables a function call access that does not exist to return null instead of an exception. Default is <c>false</c>
        /// </summary>
        public bool EnableRelaxedFunctionAccess { get; set; }

        /// <summary>
        /// Enables an indexer access to go out of bounds. Default is <c>true</c>
        /// </summary>
        public bool EnableRelaxedIndexerAccess { get; set; }

        /// <summary>
        /// Enables the index of an indexer access to be null and return null instead of an exception. Default is <c>false</c>
        /// </summary>
        public bool EnableNullIndexer { get; set; }

        /// <summary>
        /// Gets the current node being evaluated.
        /// </summary>
        public ScriptNode CurrentNode { get; private set; }

        public SourceSpan CurrentSpan => CurrentNode?.Span ?? new SourceSpan();

        /// <summary>
        /// Returns the current indent used for prefixing output lines.
        /// </summary>
        public string CurrentIndent { get; set; }

        /// <summary>
        /// Indicates if we are in a loop
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in loop]; otherwise, <c>false</c>.
        /// </value>
        internal bool IsInLoop => _loops.Count > 0;

        internal bool IgnoreExceptionsWhileRewritingScientific { get; set; }

        /// <summary>
        /// Throws a <see cref="ScriptAbortException"/> is a cancellation was issued on the <see cref="CancellationToken"/>.
        /// </summary>
        public void CheckAbort()
        {
            RuntimeHelpers.EnsureSufficientExecutionStack();
            var token = this.CancellationToken;
            // Throw if cancellation is requested
            if (token.IsCancellationRequested)
            {
                throw new ScriptAbortException(CurrentNode?.Span ?? new SourceSpan(), token);
            }
        }

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
            var arguments = _availablePipeArguments.Count > 0 ? _availablePipeArguments.Pop() : new ScriptPipeArguments(1);
            _pipeArguments.Push(arguments);
            _currentPipeArguments = arguments;
        }

        internal void ClearPipeArguments()
        {
            while (_pipeArguments.Count > 0)
            {
                PopPipeArguments();
            }
        }

        internal List<ScriptExpression> GetOrCreateListOfScriptExpressions(int capacity)
        {
            var list  = _availableScriptExpressionLists.Count > 0 ? _availableScriptExpressionLists.Pop() : new List<ScriptExpression>();
            if (capacity > list.Capacity) list.Capacity = capacity;
            return list;
        }

        internal void ReleaseListOfScriptExpressions(List<ScriptExpression> list)
        {
            _availableScriptExpressionLists.Push(list);
            list.Clear();
        }

        internal object[] GetOrCreateReflectionArguments(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            // Don't try to allocate more than we can allocate
            if (length >= _availableReflectionArguments.Length) return new object[length];

           var reflectionArguments =  _availableReflectionArguments[length] ?? new object[length];
           if (length > 0)
           {
               _availableReflectionArguments[length] = (object[])reflectionArguments[0];
               reflectionArguments[0] = null;
           }
           return reflectionArguments;
        }

        internal void ReleaseReflectionArguments(object[] reflectionArguments)
        {
            // Nothing to release
            if (reflectionArguments == null) return;

            if (reflectionArguments.Length >= _availableReflectionArguments.Length) return;
            Array.Clear(reflectionArguments, 0, reflectionArguments.Length);

            var previousArg = _availableReflectionArguments[reflectionArguments.Length];
            _availableReflectionArguments[reflectionArguments.Length] = reflectionArguments;

            if (reflectionArguments.Length > 0)
            {
                reflectionArguments[0] = previousArg;
            }
        }

        internal void PopPipeArguments()
        {
            if (_pipeArguments.Count == 0)
            {
                throw new InvalidOperationException("Cannot PopPipeArguments more than PushPipeArguments");
            }

            var pipeFrom = _pipeArguments.Pop();
            pipeFrom.Clear();
            _currentPipeArguments = _pipeArguments.Count > 0 ? _pipeArguments.Peek() : null;
            _availablePipeArguments.Push(pipeFrom);
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

            var previousNode = CurrentNode;
            _getOrSetValueLevel++;
            try
            {
                CurrentNode = target;
                return GetOrSetValue(target, null, false);
            }
            finally
            {
                CurrentNode = previousNode;
                _getOrSetValueLevel--;
            }
        }


        private static readonly object TrueObject = true;
        private static readonly object FalseObject = false;

        public void SetValue(ScriptVariable variable, bool value)
        {
            SetValue(variable, value ? TrueObject : FalseObject);
        }

        public virtual void Import(SourceSpan span, object objectToImport)
        {
            var scriptObject = objectToImport as ScriptObject;
            if (scriptObject == null)
            {
                throw new ScriptRuntimeException(span, $"Unexpected value `{GetTypeName(objectToImport)}` for import. Expecting an plain script object.");
            }
            CurrentGlobal.Import(objectToImport);
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
        public virtual TemplateContext Write(SourceSpan span, object textAsObject)
        {
            if (textAsObject != null)
            {
                var text = ObjectToString(textAsObject);
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
                Write(text, 0, text.Length);
            }
            return this;
        }

        /// <summary>
        /// Writes the a new line to the current <see cref="Output"/>
        /// </summary>
        public TemplateContext WriteLine()
        {
            Write(NewLine);
            return this;
        }

        /// <summary>
        /// Writes the text to the current <see cref="Output"/>
        /// </summary>
        /// <param name="slice">The text.</param>
        public TemplateContext Write(ScriptStringSlice slice)
        {
            Write(slice.FullText, slice.Index, slice.Length);
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
                // Indented text
                if (CurrentIndent != null)
                {
                    var index = startIndex;
                    var indexEnd = startIndex + count;

                    while (index < indexEnd)
                    {
                        // Write indents if necessary
                        if (_previousTextWasNewLine)
                        {
                            Output.Write(CurrentIndent, 0, CurrentIndent.Length);
                            _previousTextWasNewLine = false;
                        }

                        var newLineIndex = text.IndexOf('\n', index);
                        if (newLineIndex < 0 || newLineIndex >= indexEnd)
                        {
                            Output.Write(text, index, indexEnd - index);
                            break;
                        }

                        // We output the new line
                        Output.Write(text, index, newLineIndex - index + 1);
                        index = newLineIndex + 1;
                        _previousTextWasNewLine = true;
                    }
                }
                else
                {
                    Output.Write(text, startIndex, count);
                }
            }

            return this;
        }

        /// <summary>
        /// Evaluates the specified script node.
        /// </summary>
        /// <param name="scriptNode">The script node.</param>
        /// <returns>The result of the evaluation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        public virtual object Evaluate(ScriptNode scriptNode, bool aliasReturnedFunction)
        {
            if (scriptNode == null) return null;

            var previousFunctionCallState = _isFunctionCallDisabled;
            var previousLevel = _getOrSetValueLevel;
            var previousNode = CurrentNode;
            try
            {
                CurrentNode = scriptNode;
                _getOrSetValueLevel = 0;
                _isFunctionCallDisabled = aliasReturnedFunction;
                var result = scriptNode.Evaluate(this);
                return result;
            }
            catch (ScriptRuntimeException ex) when (this.RenderRuntimeException != null)
            {
                return this.RenderRuntimeException(ex);
            }
            catch (Exception ex) when (!(ex is ScriptRuntimeException))
            {
                var toThrow = new ScriptRuntimeException(scriptNode.Span, ex.Message, ex);
                if (RenderRuntimeException != null)
                {
                    return RenderRuntimeException(toThrow);
                }
                throw toThrow;
            }
            finally
            {
                CurrentNode = previousNode;
                _getOrSetValueLevel = previousLevel;
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
            if (!_memberAccessors.TryGetValue(type, out var accessor))
            {
                accessor = GetMemberAccessorImpl(target) ?? NullAccessor.Default;
                _memberAccessors.Add(type, accessor);
            }
            return accessor;
        }

        /// <summary>
        /// This method is reset-ing correctly this instance so that
        /// it can be reused safely on the same thread for another run.
        /// </summary>
        /// <remarks>
        /// - Removes any pending output expect the top level one.
        /// - This methods clears the top level output (which is a guaranteed to be <see cref="StringBuilderOutput"/>).
        /// - Remove all global stored pushed via <see cref="PushGlobal"/> expect the builtin top level one.
        /// - Remove all cultures pushed via <see cref="PushCulture"/>.
        /// - Remove all source files pushed via <see cref="PushSourceFile"/>.
        /// </remarks>
        public virtual void Reset()
        {
            // We need to leave one output
            while (OutputCount > 1)
            {
                PopOutput();
            }
            // Clear the output
            ((StringBuilderOutput)Output).Builder.Length = 0;

            // We need to keep the builtins
            while (GlobalCount > 1)
            {
                PopGlobal();
            }

            while (CultureCount > 0)
            {
                PopCulture();
            }

            while (SourceFileCount > 0)
            {
                PopSourceFile();
            }
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
            else if (target is string)
            {
                accessor = StringAccessor.Default;
            }
            else if (type.IsPrimitiveOrDecimal())
            {
                accessor = PrimitiveAccessor.Default;
            }
            else if (DictionaryAccessor.TryGet(target, out accessor))
            {
            }
            else if (type.IsArray)
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
            try
            {
                RuntimeHelpers.EnsureSufficientExecutionStack();
            }
            catch (InsufficientExecutionStackException)
            {
                throw new ScriptRuntimeException(node.Span, $"Exceeding recursive depth limit, near to stack overflow");
            }

            _callDepth++;
            if (RecursiveLimit != 0 && _callDepth > RecursiveLimit)
            {
                throw new ScriptRuntimeException(node.Span, $"Exceeding number of recursive depth limit `{RecursiveLimit}` for node: `{node}`"); // unit test: 305-func-error2.txt
            }
        }

        public void ExitRecursive(ScriptNode node)
        {
            _callDepth--;
            if (_callDepth < 0)
            {
                throw new ScriptRuntimeException(node.Span, $"unexpected ExitRecursive not matching EnterRecursive for `{node}`");
            }
        }

        /// <summary>
        /// Called when entering a function.
        /// </summary>
        /// <param name="caller"></param>
        internal void EnterFunction(ScriptNode caller)
        {
            EnterRecursive(caller);
        }

        /// <summary>
        /// Called when exiting a function.
        /// </summary>
        internal void ExitFunction(ScriptNode caller)
        {
            ExitRecursive(caller);
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
            PushVariableScope(VariableScope.Loop);
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
            try
            {
                OnExitLoop(loop);
            }
            finally
            {
                PopVariableScope(VariableScope.Loop);
                _loops.Pop();
                _loopStep = 0;
            }
        }

        /// <summary>
        /// Called when exiting a loop.
        /// </summary>
        /// <param name="loop">The loop expression object</param>
        protected virtual void OnExitLoop(ScriptLoopStatementBase loop)
        {
        }

        internal enum LoopType
        {
            Default,
            Queryable
        }

        internal bool StepLoop(ScriptLoopStatementBase loop, LoopType loopType = LoopType.Default )
        {
            Debug.Assert(_loops.Count > 0);

            _loopStep++;

            int loopLimit;
            switch (loopType)
            {
                case LoopType.Queryable:
                    {
                        loopLimit = LoopLimitQueryable.GetValueOrDefault(LoopLimit);
                        break;
                    }
                default:
                    {
                        loopLimit = LoopLimit;
                        break;
                    }
            }

            if (loopLimit != 0 && _loopStep > loopLimit)
            {
                var currentLoopStatement = _loops.Peek();
                throw new ScriptRuntimeException(currentLoopStatement.Span, $"Exceeding number of iteration limit `{loopLimit}` for loop statement."); // unit test: 215-for-statement-error1.txt
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
                    throw new ScriptRuntimeException(targetExpression.Span, $"Unsupported target expression for assignment."); // unit test: 105-assign-error1.txt
                }
            }
            catch (Exception readonlyException) when(_getOrSetValueLevel == 1 && !(readonlyException is ScriptRuntimeException))
            {
                throw new ScriptRuntimeException(targetExpression.Span, $"Unexpected exception while accessing target expression: {readonlyException.Message}", readonlyException);
            }

            // If the variable being returned is a function, we need to evaluate it
            // If function call is disabled, it will be only when returning the final object (level 0 of recursion)
            var allowFunctionCall = (_isFunctionCallDisabled && _getOrSetValueLevel > 1) || !_isFunctionCallDisabled;
            if (allowFunctionCall && ScriptFunctionCall.IsFunction(value))
            {
                // Allow to pipe arguments only for top level returned function
                value = ScriptFunctionCall.Call(this, targetExpression, value, _getOrSetValueLevel == 1, null);
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
            if (type.IsArray)
            {
                return ArrayAccessor.Default;
            }

            if (type == typeof(string))
            {
                return StringAccessor.Default;
            }

            if (type.IsPrimitiveOrDecimal())
            {
                return PrimitiveAccessor.Default;
            }

            if (target is IList)
            {
                return ListAccessor.Default;
            }
            return null;
        }

        internal void ResetPreviousNewLine()
        {
            _previousTextWasNewLine = false;
        }
    }

    /// <summary>
    /// A Liquid based <see cref="TemplateContext"/> providing the builtin functions usually available for a liquid template.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    class LiquidTemplateContext : TemplateContext
    {
        public LiquidTemplateContext() : base(new LiquidBuiltinsFunctions())
        {
            Language = ScriptLang.Liquid;

            // In liquid, if we have a break/continue outside a loop, we return from the current script
            EnableBreakAndContinueAsReturnOutsideLoop = true;
            EnableRelaxedTargetAccess = true;

            TemplateLoaderLexerOptions = new LexerOptions() {Lang = ScriptLang.Liquid};
            TemplateLoaderParserOptions = new ParserOptions() {LiquidFunctionsToScriban = true};
            IsLiquid = true;
        }
    }
}