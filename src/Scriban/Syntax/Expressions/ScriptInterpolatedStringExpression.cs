// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable
using System;
using System.Threading.Tasks;

namespace Scriban.Syntax
{
    [ScriptSyntax("interpolated expression", "{<expression>}")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptInterpolatedStringExpression : ScriptExpression
    {
        private ScriptList<ScriptExpression> _stringParts;

        public ScriptInterpolatedStringExpression()
        {
            Parts = new ScriptList<ScriptExpression>();
        }

        public ScriptList<ScriptExpression> Parts
        {
            get => _stringParts;
            set => ParentToThis(ref _stringParts, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            // A nested expression will reset the pipe arguments for the group
            context.PushPipeArguments();
            try
            {
                var builder = new System.Text.StringBuilder(); // TODO: use thread local
                foreach (var scriptExpression in Parts)
                {
                    var value = context.Evaluate(scriptExpression);
                    if (value != null)
                    {
                        builder.Append(value);
                    }
                }
                return builder.ToString();
            }
            finally
            {
                if (context.CurrentPipeArguments != null)
                {
                    context.PopPipeArguments();
                }
            }
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            foreach (var scriptExpression in Parts)
            {
                printer.Write(scriptExpression);
            }
        }
    }
}