// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
        private ScriptKeyword _wrapKeyword;
        private ScriptExpression _target;
        private ScriptBlockStatement _body;

        public ScriptWrapStatement()
        {
            WrapKeyword = ScriptKeyword.Wrap();
        }

        public ScriptKeyword WrapKeyword
        {
            get => _wrapKeyword;
            set => ParentToThis(ref _wrapKeyword, value);
        }

        public ScriptExpression Target
        {
            get => _target;
            set => ParentToThis(ref _target, value);
        }

        public ScriptBlockStatement Body
        {
            get => _body;
            set => ParentToThis(ref _body, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // Check that the Target is actually a function
            var functionCall = Target as ScriptFunctionCall;
            if (functionCall == null)
            {
                var parameterLessFunction = context.Evaluate(Target, true);
                if (!(parameterLessFunction is IScriptCustomFunction))
                {
                    var targetPrettyName = ScriptSyntaxAttribute.Get(Target);
                    throw new ScriptRuntimeException(Target.Span, $"Expecting a direct function instead of the expression `{Target}/{targetPrettyName.TypeName}`");
                }

                context.BlockDelegates.Push(Body);
                return ScriptFunctionCall.Call(context, this, parameterLessFunction, false, null);
            }

            context.BlockDelegates.Push(Body);
            return context.Evaluate(functionCall);
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(WrapKeyword).ExpectSpace();
            printer.Write(Target);
            printer.ExpectEos();
            printer.Write(Body).ExpectEos();
        }
    }
}