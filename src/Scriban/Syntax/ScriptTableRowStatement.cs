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
            context.Output.Append("<tr class=\"row1\">").Append(Environment.NewLine);
        }

        protected override void AfterLoop(TemplateContext context)
        {
            context.Output.Append("</tr>").Append(Environment.NewLine);
        }

        protected override bool Loop(TemplateContext context, int index, int localIndex, bool isLast)
        {
            var output = context.Output;

            var columnIndex = localIndex % _columnsCount;

            context.SetValue(ScriptVariable.TableRowCol, columnIndex + 1);

            if (columnIndex == 0 && localIndex > 0)
            {
                output.Append("</tr>").Append(Environment.NewLine);
                output.Append("<tr class=\"row").Append((localIndex / _columnsCount) + 1).Append("\">");
            }
            output.Append("<td class=\"col").Append(columnIndex + 1).Append("\">");

            var result = base.Loop(context, index, localIndex, isLast);

            output.Append("</td>");

            return result;
        }

        public override void Write(RenderContext context)
        {
            context.Write("tablerow").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            NamedParameters?.Write(context);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
    }
}