// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections.Generic;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("wrap statement", "wrap <function_call> ... end")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptWrapStatement : ScriptStatement
    {
        private ScriptKeyword _wrapKeyword = ScriptKeyword.Wrap();
        private ScriptExpression? _target;
        private ScriptBlockStatement? _body;
        public ScriptWrapStatement()
        {
            _wrapKeyword.Parent = this;
        }

        public ScriptKeyword WrapKeyword
        {
            get => _wrapKeyword;
            set => ParentToThis(ref _wrapKeyword, value);
        }

        public ScriptExpression? Target
        {
            get => _target;
            set => ParentToThisNullable(ref _target, value);
        }

        public ScriptBlockStatement? Body
        {
            get => _body;
            set => ParentToThisNullable(ref _body, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            var target = Target;
            var body = Body;
            if (target is null || body is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid wrap statement. Target and body are required.");
            }

            // Check that the Target is actually a function
            var functionCall = target as ScriptFunctionCall;
            if (functionCall is null)
            {
                var parameterLessFunction = context.Evaluate(target, true);
                if (!(parameterLessFunction is IScriptCustomFunction))
                {
                    var targetPrettyName = ScriptSyntaxAttribute.Get(target);
                    throw new ScriptRuntimeException(target.Span, $"Expecting a direct function instead of the expression `{target}/{targetPrettyName?.TypeName ?? "unknown"}`");
                }

                context.BlockDelegates.Push(body);
                return ScriptFunctionCall.Call(context, this, parameterLessFunction, false, null);
            }

            context.BlockDelegates.Push(body);
            return context.Evaluate(functionCall);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(WrapKeyword).ExpectSpace();
            if (Target is not null)
            {
                printer.Write(Target);
            }
            printer.ExpectEos();
            if (Body is not null)
            {
                printer.Write(Body).ExpectEos();
            }
        }
    }
}
