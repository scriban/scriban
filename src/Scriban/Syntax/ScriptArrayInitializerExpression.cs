// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using Scriban.Helpers;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    [ScriptSyntax("array initializer", "[item1, item2,...]")]
    public class ScriptArrayInitializerExpression : ScriptExpression
    {
        public ScriptArrayInitializerExpression()
        {
            Values = new List<ScriptExpression>();
        }

        public List<ScriptExpression> Values { get; private set; }

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

        public override void Write(RenderContext context)
        {
            context.Write("[");
            for (var i = 0; i < Values.Count; i++)
            {
                var value = Values[i];
                context.Write(value);

                // If the value didn't have any Comma Trivia, we can emit it
                if (i + 1 < Values.Count && !value.HasTrivia(ScriptTriviaType.Comma, false))
                {
                    context.Write(",");
                }
            }
            context.Write("]");
        }

        public override string ToString()
        {
            return $"[{StringHelper.Join(", ", Values)}]";
        }
    }
}