// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

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
        private ScriptExpression? _from;
        private ScriptToken? _pipeToken;
        private ScriptExpression? _to;
        public ScriptExpression? From
        {
            get => _from;
            set => ParentToThisNullable(ref _from, value);
        }

        public ScriptToken? PipeToken
        {
            get => _pipeToken;
            set => ParentToThisNullable(ref _pipeToken, value);
        }

        public ScriptExpression? To
        {
            get => _to;
            set => ParentToThisNullable(ref _to, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            var from = From;
            var to = To;
            if (from is null || to is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid pipe expression. Source and destination are required.");
            }

            bool newPipe = context.CurrentPipeArguments is null;
            try
            {
                // Push a new pipe arguments
                if (newPipe) context.PushPipeArguments();

                var pipeArguments = context.CurrentPipeArguments ?? throw new ScriptRuntimeException(Span, "Pipe arguments were not initialized.");
                pipeArguments.Push(from);

                var result = context.Evaluate(to);

                // If the result returns by the evaluation is a function and we haven't yet consumed the pipe argument
                // that means that we need to evaluate this function with the actual pipe arguments.
                if (result is IScriptCustomFunction && pipeArguments.Count > 0 && pipeArguments.Peek() == from)
                {
                    result = ScriptFunctionCall.Call(context, to, result, true, null);
                }

                // If we have still remaining arguments, it is likely that the destination expression is not a function
                // so pipe arguments were not picked up and this is an error
                if (pipeArguments.Count > 0 && pipeArguments.Peek() == from)
                {
                    throw new ScriptRuntimeException(to.Span, $"Pipe expression destination `{to}` is not a valid function ");
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
            if (From is not null)
            {
                printer.Write(From);
            }
            if (PipeToken is not null)
            {
                printer.Write(PipeToken);
            }
            if (To is not null)
            {
                printer.Write(To);
            }
        }
    }
}
