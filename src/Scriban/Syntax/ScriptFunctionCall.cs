// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        public ScriptToken OpenParent { get; set; }

        public List<ScriptExpression> Arguments { get; private set; }

        public ScriptToken CloseParen { get; set; }

        public bool ExplicitCall { get; set; }

        public bool TryGetFunctionDeclaration(out ScriptFunction function)
        {
            function = null;
            if (!ExplicitCall) return false;
            if (!(Target is ScriptVariableGlobal name)) return false;
            if (OpenParent == null || CloseParen == null) return false;

            foreach(var arg in Arguments)
            {
                if (!(arg is ScriptVariableGlobal)) return false;
            }

            function = new ScriptFunction
            {
                Name = name,
                OpenParen = OpenParent,
                Parameters = new List<ScriptVariableGlobal>(),
                CloseParen = CloseParen,
                Span = Span
            };

            foreach (var arg in Arguments)
            {
                var parameter = (ScriptVariableGlobal) arg;
                function.Parameters.Add(parameter);
            }

            return true;
        }
        
        public void AddArgument(ScriptExpression argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument));
            Arguments.Add(argument);
            if (CloseParen == null && !argument.Span.IsEmpty)
            {
                Span.End = argument.Span.End;
            }
        }

        public ScriptExpression GetScientificExpression(TemplateContext context)
        {
            // If we are in scientific mode and we have a function which takes arguments, and is not an explicit call (e.g sin(x) rather then sin x)
            // Then we need to rewrite the call to a proper expression.
            if (context.UseScientific && !ExplicitCall && Arguments.Count > 0)
            {
                var rewrite = new ScientificFunctionCallRewriter(1 + Arguments.Count);
                rewrite.Add(Target);
                rewrite.AddRange(Arguments);
                return rewrite.Rewrite(context);
            }
            return this;
        }

        public override object Evaluate(TemplateContext context)
        {
            // Double check if the expression can be rewritten
            var newExpression = GetScientificExpression(context);
            if (newExpression != this)
            {
                return context.Evaluate(newExpression);
            }

            // Invoke evaluate on the target, but don't automatically call the function as if it was a parameterless call.
            var targetFunction = context.Evaluate(Target, true);

            // Throw an exception if the target function is null
            if (targetFunction == null)
            {
                throw new ScriptRuntimeException(Target.Span, $"The function `{Target}` was not found");
            }

            return Call(context, Target, targetFunction, context.AllowPipeArguments, Arguments);
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write(Target);
            if (OpenParent != null) context.Write(OpenParent);
            for (var i = 0; i < Arguments.Count; i++)
            {
                var scriptExpression = Arguments[i];
                if (OpenParent == null || i > 0)
                {
                    context.ExpectSpace();
                }
                context.Write(scriptExpression);
            }

            if (CloseParen != null) context.Write(CloseParen);
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

        public override void Accept(ScriptVisitor visitor) => visitor.Visit(this);

        public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => visitor.Visit(this);

        protected override IEnumerable<ScriptNode> GetChildren()
        {
            yield return Target;
            if (OpenParent != null) yield return OpenParent;
            foreach (var argument in Arguments)
            {
                yield return argument;
            }
            if (CloseParen != null) yield return CloseParen;
        }

        public static bool IsFunction(object target)
        {
            return target is IScriptCustomFunction;
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

            if (externFunction == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Invalid target function `{callerContext}` ({functionObject?.GetType().ScriptFriendlyName()})");
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
                for (var argIndex = 0; argIndex < arguments.Count; argIndex++)
                {
                    var argument = arguments[argIndex];
                    object value;

                    // Handle named arguments
                    var namedArg = argument as ScriptNamedArgument;
                    if (namedArg != null)
                    {
                        // In case of a ScriptFunction, we write the named argument into the ScriptArray directly
                        if (function != null)
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
                        if (externFunction.IsExpressionParameter(argIndex))
                        {
                            value = argument;
                        }
                        else
                        {
                            value = context.Evaluate(argument);
                        }
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
            var needLocal = !(externFunction is ScriptFunction func && func.HasParameters);
            context.EnterFunction(callerContext, needLocal);
            try
            {
                try
                {
                    result = externFunction.Invoke(context, callerContext, argumentValues, blockDelegate);
                }
                catch (ArgumentException ex)
                {
                    var index = externFunction.GetParameterIndex(ex.ParamName);
                    if (index >= 0 && arguments != null && index < arguments.Count)
                    {
                        throw new ScriptRuntimeException(arguments[index].Span, ex.Message);
                    }

                    throw;
                }
                catch (ScriptArgumentException ex)
                {
                    var index = ex.ArgumentIndex;
                    if (index >= 0 && arguments != null && index < arguments.Count)
                    {
                        throw new ScriptRuntimeException(arguments[index].Span, ex.Message);
                    }
                    throw;
                }
            }
            finally
            {
                context.ExitFunction(needLocal);
            }

            // Restore the flow state to none
            context.FlowState = ScriptFlowState.None;
            return result;
        }
    }
}