// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Globalization;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Syntax
{
    /// <summary>
    /// Statement handling the `tablerow`
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptTableRowStatement : ScriptForStatement
    {
        private int _columnsCount;

        public ScriptTableRowStatement()
        {
            _columnsCount = 1;
        }

        protected override ScriptVariable GetLoopVariable(TemplateContext context)
        {
            return ScriptVariable.TablerowObject;
        }

        protected override void ProcessArgument(TemplateContext context, ScriptNamedArgument argument)
        {
            _columnsCount = 1;
            if (argument.Name?.Name == "cols")
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

        protected override object LoopItem(TemplateContext context, LoopState state)
        {
            var localIndex = state.Index;

            var columnIndex = localIndex % _columnsCount;

            var tableRowLoopState = (TableRowLoopState) state;
            tableRowLoopState.Col = columnIndex;
            tableRowLoopState.ColFirst = columnIndex == 0;
            tableRowLoopState.ColLast = ((localIndex + 1) % _columnsCount) == 0;

            if (columnIndex == 0 && localIndex > 0)
            {
                context.Write("</tr>").Write(context.NewLine);
                var rowIndex = (localIndex / _columnsCount) + 1;
                context.Write("<tr class=\"row").Write(rowIndex.ToString(CultureInfo.InvariantCulture)).Write("\">");
            }
            context.Write("<td class=\"col").Write((columnIndex + 1).ToString(CultureInfo.InvariantCulture)).Write("\">");

            var result = base.LoopItem(context, state);

            context.Write("</td>");

            return result;
        }

        protected override LoopState CreateLoopState()
        {
            return new TableRowLoopState();
        }

        /// <summary>
        /// State for a tablerow
        /// </summary>
        protected class TableRowLoopState : LoopState
        {
            public int Col { get; set; }

            public bool ColFirst { get; set; }

            public bool ColLast { get; set; }

            public override bool Contains(string member)
            {
                if (!base.Contains(member))
                {
                    switch (member)
                    {
                        case "col":
                        case "col0":
                        case "col_first":
                        case "col_last":
                            return true;
                    }
                    return false;
                }
                return true;
            }


            public override bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
            {
                if (!base.TryGetValue(context, span, member, out value))
                {
                    switch (member)
                    {
                        case "col":
                            value = context.IsLiquid ? Col + 1 : Col;
                            return true;
                        case "col0":
                            value = Col;
                            return true;
                        case "col_first":
                            value = ColFirst ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                            return true;
                        case "col_last":
                            value = ColLast ? BoxHelper.TrueObject : BoxHelper.FalseObject;
                            return true;
                    }
                    return false;
                }

                return true;
            }
        }
    }
}