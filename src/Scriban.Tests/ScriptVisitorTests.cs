// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Text;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    [TestFixture]
    public class ScriptVisitorTests
    {
        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void ScriptVisitor_Returns_Original_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var visitor = new ScriptVisitor();
            var result = visitor.Visit(template.Page);

            // The base ScriptVisitor never changes any node, so we should end up with the same instance
            Assert.AreSame(template.Page, result);
        }

        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void LeafCopyScriptVisitor_Returns_Identical_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var visitor = new LeafCopyScriptVisitor();
            var result = visitor.Visit(template.Page);

            // This visitor makes copies of leaf nodes instead of returning the original nodes,
            // so we should end up with another instance identical to the original.
            Assert.AreNotSame(template.Page, result);
            Assert.AreEqual(ToText(template.Page), ToText(result));
        }

        private string ToText(ScriptNode node)
        {
            var output = new StringBuilder();
            var context = new TemplateRewriterContext(new StringBuilderOutput(output));
            context.Write(node);
            return output.ToString();
        }

        private Template LoadTemplate(string inputName)
        {
            var templateSource = TestFilesHelper.LoadTestFile(inputName);
            var parser =
                inputName.Contains("500-liquid")
                    ? (Func<string, string, ParserOptions?, LexerOptions?, Template>) Template.ParseLiquid
                    : Template.Parse;

            var template = parser(templateSource, inputName, default, default);
            if (template.HasErrors || template.Page == null)
                Assert.Ignore("Template didn't parse correctly");

            return template;
        }

        private class LeafCopyScriptVisitor : ScriptVisitor
        {
            protected override ScriptNode VisitVariableGlobal(ScriptVariableGlobal node)
            {
                return new ScriptVariableGlobal(node.Name).WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitVariableLocal(ScriptVariableLocal node)
            {
                return new ScriptVariableLocal(node.Name).WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitVariableLoop(ScriptVariableLoop node)
            {
                return new ScriptVariableLoop(node.Name).WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitRawStatement(ScriptRawStatement node)
            {
                return new ScriptRawStatement
                {
                    Text = node.Text,
                    EscapeCount = node.EscapeCount
                }.WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitThisExpression(ScriptThisExpression node)
            {
                return new ScriptThisExpression().WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitBreakStatement(ScriptBreakStatement node)
            {
                return new ScriptBreakStatement().WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitContinueStatement(ScriptContinueStatement node)
            {
                return new ScriptContinueStatement().WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitLiteral(ScriptLiteral node)
            {
                return new ScriptLiteral
                {
                    Value = node.Value,
                    StringQuoteType = node.StringQuoteType
                }.WithTriviaAndSpanFrom(node);
            }

            protected override ScriptNode VisitNopStatement(ScriptNopStatement node)
            {
                return new ScriptNopStatement().WithTriviaAndSpanFrom(node);
            }
        }
    }
}
