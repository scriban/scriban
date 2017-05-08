// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Model
{
    [ScriptSyntax("function call expression", "<target_expression> <arguemnt[0]> ... <arguement[n]>")]
    public class ScriptFunctionCall : ScriptExpression
    {
        public ScriptFunctionCall()
        {
            Arguments = new List<ScriptExpression>();
        }

        public ScriptExpression Target { get; set; }

        public List<ScriptExpression> Arguments { get; private set; }

        public override void Evaluate(TemplateContext context)
        {
            // Call evaluate on the target, but don't automatically call the function as if it was a parameterless call.
            var targetFunction = context.Evaluate(Target, true);

            // Throw an exception if the target function is null
            if (targetFunction == null)
            {
                throw new ScriptRuntimeException(Target.Span, $"The target function [{Target}] is null");
            }

            context.Result = Call(context, this, targetFunction, Arguments);
        }

        public override string ToString()
        {
            var args = StringHelper.Join(" ", Arguments);
            return $"{Target} {args}";
        }

        public static bool IsFunction(object target)
        {
            return target is ScriptFunction || target is IScriptCustomFunction;
        }

        public static object Call(TemplateContext context, ScriptNode callerContext, object functionObject, List<ScriptExpression> arguments = null)
        {
            if (callerContext == null) throw new ArgumentNullException(nameof(callerContext));
            if (functionObject == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"The target function [{callerContext}] is null");
            }
            var function = functionObject as ScriptFunction;
            var externFunction = functionObject as IScriptCustomFunction;

            if (function == null && externFunction == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Invalid object function [{functionObject?.GetType()}]");
            }

            ScriptBlockStatement blockDelegate = null;
            if (context.BlockDelegates.Count > 0)
            {
                blockDelegate = context.BlockDelegates.Pop();
            }

            var argumentValues = new ScriptArray();
            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    var value = context.Evaluate(argument);

                    // Handle parameters expansion for a function call when the operator ^ is used
                    var unaryExpression = argument as ScriptUnaryExpression;
                    if (unaryExpression != null && unaryExpression.ExpandParameters(value, argumentValues))
                    {
                        continue;
                    }

                    argumentValues.Add(value);
                }
            }

            // Handle pipe arguments here
            if (context.PipeArguments.Count > 0)
            {
                var additionalArgument = context.PipeArguments.Pop();

                var value = context.Evaluate(additionalArgument);

                // Handle parameters expansion for a function call when the operator ~ is used
                var unaryExpression = additionalArgument as ScriptUnaryExpression;
                if (unaryExpression == null || !unaryExpression.ExpandParameters(value, argumentValues))
                {
                    argumentValues.Add(value);
                }
            }

            object result = null;
            context.EnterFunction(callerContext);
            try
            {
                if (externFunction != null)
                {
                    result = externFunction.Evaluate(context, callerContext, argumentValues, blockDelegate);
                }
                else
                {
                    context.SetValue(ScriptVariable.Arguments, argumentValues, true);

                    // Set the block delegate
                    if (blockDelegate != null)
                    {
                        context.SetValue(ScriptVariable.BlockDelegate, blockDelegate, true);
                    }
                    result = context.Evaluate(function.Body);
                }
            }
            finally
            {
                context.ExitFunction();
            }

            // Restore the flow state to none
            context.FlowState = ScriptFlowState.None;
            return result;
        }
    }
}