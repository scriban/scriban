// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("function call expression", "<target_expression> <arguemnt[0]> ... <arguement[n]>")]
    public partial class ScriptFunctionCall : ScriptExpression
    {
        public ScriptFunctionCall()
        {
            Arguments = new List<ScriptExpression>();
        }

        public ScriptExpression Target { get; set; }

        public List<ScriptExpression> Arguments { get; private set; }

        public override object Evaluate(TemplateContext context)
        {
            // Invoke evaluate on the target, but don't automatically call the function as if it was a parameterless call.
            var targetFunction = context.Evaluate(Target, true);

            // Throw an exception if the target function is null
            if (targetFunction == null)
            {
                throw new ScriptRuntimeException(Target.Span, $"The function `{Target}` was not found");
            }

            return Call(context, this, targetFunction, context.AllowPipeArguments, Arguments);
        }


        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            foreach (var scriptExpression in Arguments)
            {
                context.ExpectSpace();
                context.Write(scriptExpression);
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
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

        public static object Call(TemplateContext context, ScriptNode callerContext, object functionObject, bool processPipeArguments, List<ScriptExpression> arguments = null)
        {
            if (callerContext == null) throw new ArgumentNullException(nameof(callerContext));
            if (functionObject == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"The target function `{callerContext}` is null");
            }
            var function = functionObject as ScriptFunction;
            var externFunction = functionObject as IScriptCustomFunction;

            if (function == null && externFunction == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Invalid target function `{callerContext}`( as `{functionObject?.GetType()}`)");
            }

            ScriptBlockStatement blockDelegate = null;
            if (context.BlockDelegates.Count > 0)
            {
                blockDelegate = context.BlockDelegates.Pop();
            }

            // We can't cache this array because it might be collect by the function
            // So we absolutely need to generate a new array everytime we call a function
            ScriptArray argumentValues;

            // Handle pipe arguments here
            if (processPipeArguments && context.PipeArguments != null && context.PipeArguments.Count > 0)
            {
                var args = context.PipeArguments;
                argumentValues = new ScriptArray(args.Count);
                for (int i = 0; i < args.Count; i++)
                {
                    argumentValues.Add(args[i]);
                }
                args.Clear();
            }
            else
            {
                argumentValues = new ScriptArray(arguments?.Count ?? 0);
            }

            // Process direct arguments
            if (arguments != null)
            {
                foreach (var argument in arguments)
                {
                    object value;

                    // Handle named arguments
                    var namedArg = argument as ScriptNamedArgument;
                    if (namedArg != null)
                    {
                        // In case of a ScriptFunction, we write the named argument into the ScriptArray directly
                        if (externFunction == null)
                        {
                            // We can't add an argument that is "size" for array
                            if (argumentValues.CanWrite(namedArg.Name))
                            {
                                argumentValues.SetValue(context, callerContext.Span, namedArg.Name, context.Evaluate(namedArg), false);
                                continue;
                            }

                            // Otherwise pass as a regular argument
                            value = context.Evaluate(namedArg);
                        }
                        else
                        {
                            // Named argument are passed as is to the IScriptCustomFunction
                            value = argument;
                        }
                    }
                    else
                    {
                        value = context.Evaluate(argument);
                    }

                    // Handle parameters expansion for a function call when the operator ^ is used
                    var unaryExpression = argument as ScriptUnaryExpression;
                    if (unaryExpression != null && unaryExpression.Operator == ScriptUnaryOperator.FunctionParametersExpand)
                    {
                        var valueEnumerator = value as IEnumerable;
                        if (valueEnumerator != null)
                        {
                            foreach (var subValue in valueEnumerator)
                            {
                                argumentValues.Add(subValue);
                            }
                            continue;
                        }
                    }

                    argumentValues.Add(value);
                }
            }

            object result = null;
            context.EnterFunction(callerContext);
            try
            {
                if (externFunction != null)
                {
                    result = externFunction.Invoke(context, callerContext, argumentValues, blockDelegate);
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