// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("pipe expression", "<expression> | <expression>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptPipeCall : ScriptExpression
    {
        private ScriptExpression _from;
        private ScriptToken _pipeToken;
        private ScriptExpression _to;

        public ScriptExpression From
        {
            get => _from;
            set => ParentToThis(ref _from, value);
        }

        public ScriptToken PipeToken
        {
            get => _pipeToken;
            set => ParentToThis(ref _pipeToken, value);
        }

        public ScriptExpression To
        {
            get => _to;
            set => ParentToThis(ref _to, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            bool newPipe = context.CurrentPipeArguments == null;
            try
            {
                // Push a new pipe arguments
                if (newPipe) context.PushPipeArguments();

                context.CurrentPipeArguments.Push(From);

                var result = context.Evaluate(To);

                // If the result returns by the evaluation is a function and we haven't yet consumed the pipe argument
                // that means that we need to evaluate this function with the actual pipe arguments.
                if (result is IScriptCustomFunction && context.CurrentPipeArguments.Count > 0 && context.CurrentPipeArguments.Peek() == From)
                {
                    result = ScriptFunctionCall.Call(context, To, result, true, null);
                }

                // If we have still remaining arguments, it is likely that the destination expression is not a function
                // so pipe arguments were not picked up and this is an error
                if (context.CurrentPipeArguments.Count > 0 && context.CurrentPipeArguments.Peek() == From)
                {
                    throw new ScriptRuntimeException(To.Span, $"Pipe expression destination `{To}` is not a valid function ");
                }

                return result;
            }
            catch
            {
                // If we have an exception clear all the pipe froms
                newPipe = false; // Don't try to clear the pipe
                context.ClearPipeArguments();
                throw;
            }
            finally
            {
                if (newPipe)
                {
                    context.PopPipeArguments();
                }
            }
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(From);
            printer.Write(PipeToken);
            printer.Write(To);
        }
    }
}