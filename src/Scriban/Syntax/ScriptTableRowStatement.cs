// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Globalization;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    /// <summary>
    /// Statement handling the `tablerow`
    /// </summary>
    public partial class ScriptTableRowStatement : ScriptForStatement
    {
        private int _columnsCount;

        public ScriptTableRowStatement()
        {
            _columnsCount = 1;
        }

        protected override void ProcessArgument(TemplateContext context, ScriptNamedArgument argument)
        {
            _columnsCount = 1;
            if (argument.Name == "cols")
            {
                _columnsCount = context.ToInt(argument.Value.Span, context.Evaluate(argument.Value));
                if (_columnsCount <= 0)
                {
                    _columnsCount = 1;
                }
                return;
            }
            base.ProcessArgument(context, argument);
        }

        protected override void BeforeLoop(TemplateContext context)
        {
            context.Write("<tr class=\"row1\">");
        }

        protected override void AfterLoop(TemplateContext context)
        {
            context.Write("</tr>").WriteLine();
        }

        protected override object LoopItem(TemplateContext context, int index, int localIndex, bool isLast)
        {
            var columnIndex = localIndex % _columnsCount;

            context.SetValue(ScriptVariable.TableRowCol, columnIndex + 1);

            if (columnIndex == 0 && localIndex > 0)
            {
                context.Write("</tr>").Write(context.NewLine);
                var rowIndex = (localIndex / _columnsCount) + 1;
                context.Write("<tr class=\"row").Write(rowIndex.ToString(CultureInfo.InvariantCulture)).Write("\">");
            }
            context.Write("<td class=\"col").Write((columnIndex + 1).ToString(CultureInfo.InvariantCulture)).Write("\">");

            var result = base.LoopItem(context, index, localIndex, isLast);

            context.Write("</td>");

            return result;
        }

        public override void Write(TemplateRewriterContext context)
        {
            context.Write("tablerow").ExpectSpace();
            context.Write(Variable).ExpectSpace();
            context.Write("in").ExpectSpace();
            context.Write(Iterator);
            context.Write(NamedArguments);
            context.ExpectEos();
            context.Write(Body);
            context.ExpectEnd();
        }
    }
}