// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Syntax
{
    /// <summary>
    /// Statement handling the `tablerow`
    /// </summary>
    public class ScriptTableRowStatement : ScriptForStatement
    {
        private int _columnsCount;

        protected override void ProcessOption(TemplateContext context, ScriptNamedParameter option)
        {
            _columnsCount = 1;
            if (option.Name == "cols")
            {
                _columnsCount = context.ToInt(option.Value.Span, context.Evaluate(option.Value));
                if (_columnsCount <= 0)
                {
                    _columnsCount = 1;
                }
                return;
            }
            base.ProcessOption(context, option);
        }

        protected override void BeforeLoop(TemplateContext context)
        {
            context.Write("<tr class=\"row1\">").WriteLine();
        }

        protected override void AfterLoop(TemplateContext context)
        {
            context.Write("</tr>").WriteLine();
        }

        protected override bool Loop(TemplateContext context, int index, int localIndex, bool isLast)
        {
            var output = context.Output;

            var columnIndex = localIndex % _columnsCount;

            context.SetValue(ScriptVariable.TableRowCol, columnIndex + 1);

            if (columnIndex == 0 && localIndex > 0)
            {
                output.Write("</tr>").Write(Environment.NewLine);
                output.Write("<tr class=\"row").Write((localIndex / _columnsCount) + 1).Write("\">");
            }
            output.Write("<td class=\"col").Write(columnIndex + 1).Write("\">");

            var result = base.Loop(context, index, localIndex, isLast);

            output.Write("</td>");

            return result;
        }

        public override void Write(RenderContext context)
        {
            context.Write("tablerow").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            context.Write(NamedParameters);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
    }
}