// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#if SCRIBAN_ASYNC
using System.Threading.Tasks;
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
        protected virtual object LoopItem(TemplateContext context, int index, int localIndex, bool isLast)
        {
            // Setup variable
            context.SetValue(ScriptVariable.LoopFirst, index == 0);
            var even = (index & 1) == 0;
            context.SetValue(ScriptVariable.LoopEven, even);
            context.SetValue(ScriptVariable.LoopOdd, !even);
            context.SetValue(ScriptVariable.LoopIndex, index);

            // bug: temp workaround to correct a bug with ret. Should be handled differently
            return context.Evaluate(Body);
        }

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

        protected virtual ValueTask BeforeLoopAsync(TemplateContext context)
        {
            return new ValueTask();
        }

        protected virtual ValueTask AfterLoopAsync(TemplateContext context)
        {
            return new ValueTask();
        }
#endif
    }
}