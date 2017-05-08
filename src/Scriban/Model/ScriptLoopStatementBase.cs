// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Model
{
    /// <summary>
    /// Base class for a loop statement
    /// </summary>
    public abstract class ScriptLoopStatementBase : ScriptStatement
    {
        public ScriptStatement Body { get; set; }

        /// <summary>
        /// Base implementation for a loop single iteration
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="index">The index in the loop</param>
        /// <returns></returns>
        protected bool Loop(TemplateContext context, int index)
        {
            // Setup variable
            context.SetValue(ScriptVariable.LoopFirst, index == 0);
            var even = (index & 1) == 0;
            context.SetValue(ScriptVariable.LoopEven, even);
            context.SetValue(ScriptVariable.LoopOdd, !even);
            context.SetValue(ScriptVariable.LoopIndex, index);

            Body?.Evaluate(context);

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

        public override void Evaluate(TemplateContext context)
        {
            // Notify the context that we enter a loop block (used for variable with scope Loop)
            context.EnterLoop(this);
            try
            {
                EvaluateImpl(context);
            }
            finally
            {
                // Level scope block
                context.ExitLoop();

                // Revert to flow state to none unless we have a return that must be handled at a higher level
                if (context.FlowState != ScriptFlowState.Return)
                {
                    context.FlowState = ScriptFlowState.None;
                }
            }
        }
        protected abstract void EvaluateImpl(TemplateContext context);
    }
}