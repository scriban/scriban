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
        protected virtual bool Loop(TemplateContext context, int index, int localIndex, bool isLast)
        {
            // Setup variable
            context.SetValue(ScriptVariable.LoopFirst, index == 0);
            var even = (index & 1) == 0;
            context.SetValue(ScriptVariable.LoopEven, even);
            context.SetValue(ScriptVariable.LoopOdd, !even);
            context.SetValue(ScriptVariable.LoopIndex, index);

            // bug: temp workaround to correct a bug with ret. Should be handled differently
            context.TempLoopResult = context.Evaluate(Body);

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
                EvaluateImpl(context);
            }
            finally
            {
                // Level scope block
                context.ExitLoop(this);

                if (context.FlowState == ScriptFlowState.Return)
                {
                    result = context.TempLoopResult;
                }
                else
                {
                    // Revert to flow state to none unless we have a return that must be handled at a higher level
                    context.FlowState = ScriptFlowState.None;
                }

                context.TempLoopResult = null;
            }
            return result;
        }

        protected abstract void EvaluateImpl(TemplateContext context);

#if SCRIBAN_ASYNC
        protected abstract ValueTask EvaluateImplAsync(TemplateContext context);

        protected virtual async ValueTask BeforeLoopAsync(TemplateContext context)
        {
        }

        protected virtual async ValueTask AfterLoopAsync(TemplateContext context)
        {
        }
#endif
    }
}