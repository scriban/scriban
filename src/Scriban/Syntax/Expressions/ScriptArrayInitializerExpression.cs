// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("array initializer", "[item1, item2,...]")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptArrayInitializerExpression : ScriptExpression
    {
        private ScriptList<ScriptExpression> _values = new ScriptList<ScriptExpression>();
        private ScriptToken _openBracketToken = ScriptToken.OpenBracket();
        private ScriptToken _closeBracketToken = ScriptToken.CloseBracket();
        public ScriptArrayInitializerExpression()
        {
            _openBracketToken.Parent = this;
            _values.Parent = this;
            _closeBracketToken.Parent = this;
        }

        public ScriptToken OpenBracketToken
        {
            get => _openBracketToken;
            set => ParentToThis(ref _openBracketToken, value);
        }

        public ScriptList<ScriptExpression> Values
        {
            get => _values;
            set => ParentToThis(ref _values, value);
        }

        public ScriptToken CloseBracketToken
        {
            get => _closeBracketToken;
            set => ParentToThis(ref _closeBracketToken, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            var scriptArray = new ScriptArray();
            foreach (var value in Values)
            {
                var valueEval = context.Evaluate(value);
                scriptArray.Add(valueEval);
            }
            return scriptArray;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(OpenBracketToken);
            printer.WriteListWithCommas(Values);
            printer.Write(CloseBracketToken);
        }
    }
}
