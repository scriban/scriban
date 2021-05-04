// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("function call expression", "<target_expression> <arguemnt[0]> ... <arguement[n]>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptFunctionCall : ScriptExpression
    {
        private ScriptExpression _target;
        private ScriptToken _openParent;
        private ScriptList<ScriptExpression> _arguments;
        private ScriptToken _closeParen;

        // Maximum number of parameters is 64
        // it equals the argMask we are using for matching arguments passed
        // as it is a long, it can only store 64 bits, so we are limiting
        // the number of parameter to simplify the implementation.
        public const int MaximumParameterCount = 64;

        public ScriptFunctionCall()
        {
            Arguments = new ScriptList<ScriptExpression>();
        }

        public ScriptExpression Target
        {
            get => _target;
            set => ParentToThis(ref _target, value);
        }

        public ScriptToken OpenParent
        {
            get => _openParent;
            set => ParentToThis(ref _openParent, value);
        }

        public ScriptList<ScriptExpression> Arguments
        {
            get => _arguments;
            set => ParentToThis(ref _arguments, value);
        }

        public ScriptToken CloseParen
        {
            get => _closeParen;
            set => ParentToThis(ref _closeParen, value);
        }

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


            var parameters = new ScriptList<ScriptParameter>();

            function = new ScriptFunction
            {
                NameOrDoToken = (ScriptVariable)name.Clone(),
                OpenParen = (ScriptToken)OpenParent.Clone(),
                CloseParen = (ScriptToken)CloseParen.Clone(),
                Span = Span
            };

            foreach (var arg in Arguments)
            {
                var variableName = (ScriptVariableGlobal) arg.Clone();
                parameters.Add(new ScriptParameter() {Name = variableName});
            }
            function.Parameters = parameters;

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

        [Obsolete("This method has no effect and will be deleted in a future version")]
        public ScriptExpression GetScientificExpression(TemplateContext context)
        {
            return this;
        }

        public override object Evaluate(TemplateContext context)
        {
            // Invoke evaluate on the target, but don't automatically call the function as if it was a parameterless call.
            var targetFunction = context.Evaluate(Target, true);

            // Throw an exception if the target function is null
            if (targetFunction == null)
            {
                if (context.EnableRelaxedFunctionAccess)
                {
                    return null;
                }
                throw new ScriptRuntimeException(Target.Span, $"The function `{Target}` was not found");
            }

            return Call(context, this, targetFunction, context.AllowPipeArguments, Arguments);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Target);
            if (OpenParent != null)
            {
                printer.Write(OpenParent);
                printer.WriteListWithCommas(Arguments);
            }
            else
            {
                for (var i = 0; i < Arguments.Count; i++)
                {
                    var scriptExpression = Arguments[i];
                    printer.ExpectSpace();
                    printer.Write(scriptExpression);
                }
            }

            if (CloseParen != null) printer.Write(CloseParen);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }
        public static bool IsFunction(object target)
        {
            return target is IScriptCustomFunction;
        }

        public static object Call(TemplateContext context, ScriptNode callerContext, object functionObject, bool processPipeArguments, IReadOnlyList<ScriptExpression> arguments)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Pop immediately the block
            ScriptBlockStatement blockDelegate = null;
            if (context.BlockDelegates.Count > 0)
            {
                blockDelegate = context.BlockDelegates.Pop();
            }

            if (callerContext == null) throw new ArgumentNullException(nameof(callerContext));
            if (functionObject == null)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"The target function `{callerContext}` is null");
            }
            var scriptFunction = functionObject as ScriptFunction;
            var function = functionObject as IScriptCustomFunction;

            var isPipeCall = processPipeArguments && context.CurrentPipeArguments != null && context.CurrentPipeArguments.Count > 0;

            if (function == null)
            {
                if ((isPipeCall) && (callerContext is ScriptFunctionCall funcCall))
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Pipe expression destination `{funcCall.Target}` is not a valid function ");
                }
                else
                {
                    throw new ScriptRuntimeException(callerContext.Span, $"Invalid target function `{functionObject}` ({context.GetTypeName(functionObject)})");
                }
            }

            if (function.ParameterCount >= MaximumParameterCount)
            {
                throw new ScriptRuntimeException(callerContext.Span, $"Out of range number of parameters {function.ParameterCount} for target function `{functionObject}`. The maximum number of parameters for a function is: {MaximumParameterCount}.");
            }

            // Generates an error only if the context is configured for it
            if (context.ErrorForStatementFunctionAsExpression && function.ReturnType == typeof(void) && callerContext.Parent is ScriptExpression)
            {
                var firstToken = callerContext.FindFirstTerminal();
                throw new ScriptRuntimeException(callerContext.Span, $"The function `{firstToken}` is a statement and cannot be used within an expression.");
            }

            // We can't cache this array because it might be collect by the function
            // So we absolutely need to generate a new array everytime we call a function
            ScriptArray argumentValues;
            List<ScriptExpression> allArgumentsWithPipe = null;

            // Handle pipe arguments here
            if (isPipeCall)
            {
                var argCount = Math.Max(function.RequiredParameterCount, 1 + (arguments?.Count ?? 0));
                allArgumentsWithPipe = context.GetOrCreateListOfScriptExpressions(argCount);
                var pipeFrom = context.CurrentPipeArguments.Pop();
                argumentValues =  new ScriptArray(argCount);
                allArgumentsWithPipe.Add(pipeFrom);

                if (arguments != null)
                {
                    allArgumentsWithPipe.AddRange(arguments);
                }
                arguments = allArgumentsWithPipe;
            }
            else
            {
                argumentValues = new ScriptArray(arguments?.Count ?? 0);
            }

            var needLocal = !(function is ScriptFunction func && func.HasParameters);
            object result = null;
            try
            {
                // Process direct arguments
                ulong argMask = 0;
                if (arguments != null)
                {
                    argMask = ProcessArguments(context, callerContext, arguments, function, scriptFunction, argumentValues);
                }

                // Fill remaining argument default values
                var hasVariableParams = function.VarParamKind != ScriptVarParamKind.None;
                var requiredParameterCount = function.RequiredParameterCount;
                var parameterCount = function.ParameterCount;

                if (function.VarParamKind != ScriptVarParamKind.Direct)
                {
                    FillRemainingOptionalArguments(ref argMask, argumentValues.Count, parameterCount - 1 , function, argumentValues);
                }

                // Check the required number of arguments
                var requiredMask = (1U << requiredParameterCount) - 1;
                argMask = argMask & requiredMask;

                // Create a span after the caller for missing arguments
                var afterCallerSpan = callerContext.Span;
                afterCallerSpan.Start = afterCallerSpan.End.NextColumn();
                afterCallerSpan.End = afterCallerSpan.End.NextColumn();

                if (argMask != requiredMask)
                {
                    int argCount = 0;
                    while (argMask != 0)
                    {
                        if ((argMask & 1) != 0) argCount++;
                        argMask = argMask >> 1;
                    }
                    throw new ScriptRuntimeException(afterCallerSpan, $"Invalid number of arguments `{argCount}` passed to `{callerContext}` while expecting `{requiredParameterCount}` arguments");
                }

                if (!hasVariableParams && argumentValues.Count > parameterCount)
                {
                    if (argumentValues.Count > 0 && arguments != null && argumentValues.Count <= arguments.Count)
                    {
                        throw new ScriptRuntimeException(arguments[argumentValues.Count - 1].Span, $"Invalid number of arguments `{argumentValues.Count}` passed to `{callerContext}` while expecting `{parameterCount}` arguments");
                    }
                    throw new ScriptRuntimeException(afterCallerSpan, $"Invalid number of arguments `{argumentValues.Count}` passed to `{callerContext}` while expecting `{parameterCount}` arguments");
                }

                context.EnterFunction(callerContext);
                try
                {
                    result = function.Invoke(context, callerContext, argumentValues, blockDelegate);
                }
                catch (ArgumentException ex)
                {
                    // Slow path to detect the argument index from the name if we can
                    var index = GetParameterIndexByName(function, ex.ParamName);
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
                finally
                {
                    context.ExitFunction(callerContext);
                }
            }
            finally
            {
                if (allArgumentsWithPipe != null)
                {
                    context.ReleaseListOfScriptExpressions(allArgumentsWithPipe);
                }
            }

            // Restore the flow state to none
            context.FlowState = ScriptFlowState.None;
            return result;
        }

        /// <summary>
        /// Call a custom function with the already resolved parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callerContext"></param>
        /// <param name="function"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static object Call(TemplateContext context, ScriptNode callerContext, IScriptCustomFunction function, ScriptArray arguments)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (callerContext == null) throw new ArgumentNullException(nameof(callerContext));
            if (function == null) throw new ArgumentNullException(nameof(function));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            var parameterCount = function.ParameterCount;

            var argumentValues = new ScriptArray();
            var span = callerContext.Span;
            // Fast path if we don't have complicated parameters to handle (direct call, same amount of arguments than expected parameters)
            if (function.VarParamKind == ScriptVarParamKind.None && parameterCount == arguments.Count)
            {
                for (int i = 0; i < parameterCount; i++)
                {
                    var arg = arguments[i];
                    var paramType = function.GetParameterInfo(i).ParameterType;
                    var value = context.ToObject(span, arg, paramType);
                    argumentValues.Add(value);
                }
            }
            else
            {
                // Otherwise we need to do a slow path
                ulong argMask = 0;
                foreach (var arg in arguments)
                {
                    int index = argumentValues.Count;
                    {
                        var paramType = function.GetParameterInfo(index).ParameterType;
                        var value = context.ToObject(span, arg, paramType);
                        SetArgumentValue(index, value, function, ref argMask, argumentValues, parameterCount);
                    }
                }

                FillRemainingOptionalArguments(ref argMask, argumentValues.Count, parameterCount - 1, function, argumentValues);

                int requiredParameterCount = function.RequiredParameterCount;

                // Check the required number of arguments
                var requiredMask = (1U << requiredParameterCount) - 1;
                argMask = argMask & requiredMask;

                // Create a span after the caller for missing arguments
                var afterCallerSpan = callerContext.Span;
                afterCallerSpan.Start = afterCallerSpan.End.NextColumn();
                afterCallerSpan.End = afterCallerSpan.End.NextColumn();

                if (argMask != requiredMask)
                {
                    int argCount = 0;
                    while (argMask != 0)
                    {
                        if ((argMask & 1) != 0) argCount++;
                        argMask = argMask >> 1;
                    }

                    throw new ScriptRuntimeException(afterCallerSpan, $"Invalid number of arguments `{argCount}` passed to `{callerContext}` while expecting `{requiredParameterCount}` arguments");
                }
            }

            object result = null;
            context.EnterFunction(callerContext);
            try
            {
                result = function.Invoke(context, callerContext, argumentValues, null);
            }
            catch (ArgumentException ex)
            {
                // Slow path to detect the argument index from the name if we can
                var index = GetParameterIndexByName(function, ex.ParamName);
                if (index >= 0 && arguments != null && index < arguments.Count)
                {
                    throw new ScriptRuntimeException(span, ex.Message);
                }

                throw;
            }
            catch (ScriptArgumentException ex)
            {
                var index = ex.ArgumentIndex;
                if (index >= 0 && arguments != null && index < arguments.Count)
                {
                    throw new ScriptRuntimeException(span, ex.Message);
                }

                throw;
            }
            finally
            {
                context.ExitFunction(callerContext);
            }

            // Restore the flow state to none
            context.FlowState = ScriptFlowState.None;
            return result;
        }

        private static ulong ProcessArguments(TemplateContext context, ScriptNode callerContext, IReadOnlyList<ScriptExpression> arguments, IScriptCustomFunction function, ScriptFunction scriptFunction, ScriptArray argumentValues)
        {
            ulong argMask = 0;
            var parameterCount = function.ParameterCount;

            for (var argIndex = 0; argIndex < arguments.Count; argIndex++)
            {
                var argument = arguments[argIndex];

                int index = argumentValues.Count;
                object value;

                // Handle named arguments
                var namedArg = argument as ScriptNamedArgument;
                if (namedArg != null)
                {
                    var argName = namedArg.Name?.Name;
                    if (argName == null)
                    {
                        throw new ScriptRuntimeException(argument.Span, "Invalid null argument name");
                    }

                    index = GetParameterIndexByName(function, argName);
                    // In case of a ScriptFunction, we write the named argument into the ScriptArray directly
                    if (function.VarParamKind != ScriptVarParamKind.None)
                    {
                        if (index >= 0)
                        {
                        }
                        // We can't add an argument that is "size" for array
                        else if (argumentValues.CanWrite(argName))
                        {
                            argumentValues.TrySetValue(context, callerContext.Span, argName, context.Evaluate(namedArg), false);
                            continue;
                        }
                        else
                        {
                            throw new ScriptRuntimeException(argument.Span, $"Cannot pass argument {argName} to function. This name is not supported by this function.");
                        }
                    }

                    if (index < 0)
                    {
                        index = argumentValues.Count;
                    }
                }

                if (function.IsParameterType<ScriptExpression>(index))
                {
                    value = namedArg != null ? namedArg.Value : argument;
                }
                else
                {
                    value = context.Evaluate(argument);
                }

                // Handle parameters expansion for a function call when the operator ^ is used
                if (argument is ScriptUnaryExpression unaryExpression && unaryExpression.Operator == ScriptUnaryOperator.FunctionParametersExpand && !(value is ScriptExpression))
                {
                    var valueEnumerator = value as IEnumerable;
                    if (valueEnumerator != null)
                    {
                        foreach (var subValue in valueEnumerator)
                        {
                            var paramType = function.GetParameterInfo(argumentValues.Count).ParameterType;
                            var newValue = context.ToObject(callerContext.Span, subValue, paramType);

                            SetArgumentValue(index, newValue, function, ref argMask, argumentValues, parameterCount);
                            index++;
                        }
                        continue;
                    }
                }

                {
                    var paramType = function.GetParameterInfo(index).ParameterType;
                    value = context.ToObject(argument.Span, value, paramType);
                }

                SetArgumentValue(index, value, function, ref argMask, argumentValues, parameterCount);
            }

            return argMask;
        }

        private static void SetArgumentValue(int index, object value, IScriptCustomFunction function, ref ulong argMask, ScriptArray argumentValues, int parameterCount)
        {
            // NamedArguments can index further, so we need to fill any intermediate argument values
            // with their default values.
            if (index > argumentValues.Count)
            {
                FillRemainingOptionalArguments(ref argMask, argumentValues.Count, index, function, argumentValues);
            }

            if (index < parameterCount) argMask |= 1U << index;

            if (function.VarParamKind == ScriptVarParamKind.LastParameter && index >= parameterCount - 1)
            {
                var varArgs = (ScriptArray) argumentValues[parameterCount - 1];
                if (varArgs == null)
                {
                    argumentValues[index] = varArgs = new ScriptArray();
                }
                varArgs.Add(value);
            }
            else
            {
                argumentValues[index] = value;
            }
        }

        private static void FillRemainingOptionalArguments(ref ulong argMask, int startIndex, int endIndex, IScriptCustomFunction function, ScriptArray argumentValues)
        {
            var maxIndex = function.ParameterCount - 1;
            if (endIndex > maxIndex) endIndex = maxIndex;

            int paramsVarIndex = function.VarParamKind == ScriptVarParamKind.LastParameter ? maxIndex : -1;

            for (int i = startIndex; i <= endIndex; i++)
            {
                var parameterInfo = function.GetParameterInfo(i);
                if (parameterInfo.HasDefaultValue)
                {
                    argumentValues[i] = parameterInfo.DefaultValue;
                    argMask |= 1U << i;
                }
                else
                {
                    argumentValues[i] = i == paramsVarIndex ? new ScriptArray() : null;
                }
            }
        }

        private static int GetParameterIndexByName(IScriptFunctionInfo functionInfo, string name)
        {
            var paramCount = functionInfo.ParameterCount;
            for (var i = 0; i < paramCount; i++)
            {
                var p = functionInfo.GetParameterInfo(i);
                if (p.Name == name) return i;
            }

            return -1;
        }
    }
}