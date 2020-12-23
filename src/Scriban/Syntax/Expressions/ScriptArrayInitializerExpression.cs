// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

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
        private ScriptList<ScriptExpression> _values;
        private ScriptToken _openBracketToken;
        private ScriptToken _closeBracketToken;

        public ScriptArrayInitializerExpression()
        {
            OpenBracketToken = ScriptToken.OpenBracket();
            Values = new ScriptList<ScriptExpression>();
            CloseBracketToken = ScriptToken.CloseBracket();
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

        public override object Evaluate(TemplateContext context)
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