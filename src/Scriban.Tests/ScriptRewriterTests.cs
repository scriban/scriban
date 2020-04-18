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
    public class ScriptRewriterTests
    {
        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void ScriptRewriter_Returns_Original_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var rewriter = new NopScriptRewriter();
            var result = rewriter.Visit(template.Page);

            // The base ScriptRewriter never changes any node, so we should end up with the same instance
            Assert.AreSame(template.Page, result);
        }

        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void LeafCopyScriptRewriter_Returns_Identical_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var rewriter = new LeafCopyScriptRewriter();
            var result = rewriter.Visit(template.Page);

            // This rewriter makes copies of leaf nodes instead of returning the original nodes,
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

        private class NopScriptRewriter : ScriptRewriter
        {
        }

        private class LeafCopyScriptRewriter : ScriptRewriter
        {
            public override ScriptNode Visit(ScriptVariableGlobal node)
            {
                return new ScriptVariableGlobal(node.Name).WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptVariableLocal node)
            {
                return new ScriptVariableLocal(node.Name).WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptVariableLoop node)
            {
                return new ScriptVariableLoop(node.Name).WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptRawStatement node)
            {
                return new ScriptRawStatement
                {
                    Text = node.Text,
                    EscapeCount = node.EscapeCount
                }.WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptThisExpression node)
            {
                return new ScriptThisExpression().WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptBreakStatement node)
            {
                return new ScriptBreakStatement().WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptContinueStatement node)
            {
                return new ScriptContinueStatement().WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptLiteral node)
            {
                return new ScriptLiteral
                {
                    Value = node.Value,
                    StringQuoteType = node.StringQuoteType
                }.WithTriviaAndSpanFrom(node);
            }

            public override ScriptNode Visit(ScriptNopStatement node)
            {
                return new ScriptNopStatement().WithTriviaAndSpanFrom(node);
            }
        }
    }
}
