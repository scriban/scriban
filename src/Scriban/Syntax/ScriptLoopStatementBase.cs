// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;

#if SCRIBAN_ASYNC
using System.Threading.Tasks;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;

#endif

namespace Scriban.Syntax
{
    /// <summary>
    /// Base class for a loop statement
    /// </summary>
    public abstract partial class ScriptLoopStatementBase : ScriptStatement
    {
        public ScriptBlockStatement Body { get; set; }


        protected virtual void BeforeLoop(TemplateContext context)
        {
        }

        /// <summary>
        /// Base implementation for a loop single iteration
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="index">The index in the loop</param>
        /// <param name="localIndex"></param>
        /// <param name="isLast"></param>
        /// <returns></returns>
        protected abstract object LoopItem(TemplateContext context, LoopState state);

        protected virtual LoopState CreateLoopState() {  return new LoopState(); }

        protected bool ContinueLoop(TemplateContext context)
        {
            // Return must bubble up to call site
            if (context.FlowState == ScriptFlowState.Return)
            {
                return false;
            }

            // If we need to break, restore to none state
            var result = context.FlowState != ScriptFlowState.Break;
            context.FlowState = ScriptFlowState.None;
            return result;
        }

        protected virtual void AfterLoop(TemplateContext context)
        {
        }

        public override object Evaluate(TemplateContext context)
        {
            // Notify the context that we enter a loop block (used for variable with scope Loop)
            object result = null;
            context.EnterLoop(this);
            try
            {
                result = EvaluateImpl(context);
            }
            finally
            {
                // Level scope block
                context.ExitLoop(this);

                if (context.FlowState != ScriptFlowState.Return)
                {
                    // Revert to flow state to none unless we have a return that must be handled at a higher level
                    context.FlowState = ScriptFlowState.None;
                }
            }
            return result;
        }

        protected abstract object EvaluateImpl(TemplateContext context);

#if SCRIBAN_ASYNC
        protected abstract ValueTask<object> EvaluateImplAsync(TemplateContext context);

        protected abstract ValueTask<object> LoopItemAsync(TemplateContext context, LoopState state);

        protected virtual ValueTask BeforeLoopAsync(TemplateContext context)
        {
            return new ValueTask();
        }

        protected virtual ValueTask AfterLoopAsync(TemplateContext context)
        {
            return new ValueTask();
        }
#endif

        /// <summary>
        /// Store the loop state
        /// </summary>
        protected class LoopState : IScriptObject
        {
            private int _length;
            private object _lengthObject;

            public int Index { get; set; }

            public int LocalIndex { get; set; }

            public bool IsFirst => Index == 0;

            public bool IsEven => (Index & 1) == 0;

            public bool IsOdd => !IsEven;

            public bool ValueChanged { get; set; }

            public bool IsLast { get; set; }

            public int Length
            {
                get => _length;
                set
                {
                    _length = value;
                    _lengthObject = value;
                }
            }

            public int Count { get; set; }

            public IEnumerable<string> GetMembers()
            {
                return Enumerable.Empty<string>();
            }

            public virtual bool Contains(string member)
            {
                switch (member)
                {
                    case "index":
                    case "index0":
                    case "first":
                    case "even":
                    case "odd":
                    case "last":
                        return true;
                    case "length":
                        return _lengthObject != null;
                    case "rindex":
                    case "rindex0":
                        return _lengthObject != null;
                    case "changed":
                        return true;
                }
                return false;
            }

            public bool IsReadOnly { get; set; }

            public virtual  bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
            {
                value = null;
                var isLiquid = context.IsLiquid;
                switch (member)
                {
                    case "index":
                        value = isLiquid ? Index + 1 : Index;
                        return true;
                    case "length":
                        value = _lengthObject;
                        return _lengthObject != null;
                    case "first":
                        value = IsFirst ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                        return true;
                    case "even":
                        value = IsEven ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                        return true;
                    case "odd":
                        value = IsOdd ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                        return true;
                    case "last":
                        value = IsLast ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                        return true;
                    case "changed":
                        value = ValueChanged ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                        return true;
                    case "rindex":
                        if (_lengthObject != null)
                        {
                            value = isLiquid ? _length - Index : _length - Index - 1;
                        }
                        return _lengthObject != null;
                    default:
                        if (isLiquid)
                        {
                            if (member == "index0")
                            {
                                value = Index;
                                return true;
                            }
                            if (member == "rindex0")
                            {
                                value = _length - Index - 1;
                                return true;
                            }
                        }
                        return false;
                }
            }

            public bool CanWrite(string member)
            {
                throw new System.NotImplementedException();
            }

            public void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
            {
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
                return (IScriptObject)MemberwiseClone();
            }
        }
    }
}