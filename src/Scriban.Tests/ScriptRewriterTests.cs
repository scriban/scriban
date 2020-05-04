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

            var rewriter = new TestCloneScriptRewriter();
            var result = rewriter.Visit(template.Page);

            // The base ScriptRewriter never changes any node, so we should end up with the same instance
            Assert.AreNotSame(template.Page, result);
        }

        [TestCaseSource(typeof(TestFilesHelper), nameof(TestFilesHelper.ListAllTestFiles))]
        public void LeafCopyScriptRewriter_Returns_Identical_Script(string inputFileName)
        {
            var template = LoadTemplate(inputFileName);

            var rewriter = new TestCloneScriptRewriter();
            var result = rewriter.Visit(template.Page);

            // This rewriter makes copies of leaf nodes instead of returning the original nodes,
            // so we should end up with another instance identical to the original.
            Assert.AreNotSame(template.Page, result);
            Assert.AreEqual(ToText(template.Page), ToText(result));
        }

        private string ToText(ScriptNode node)
        {
            var output = new StringBuilder();
            var context = new ScriptPrinter(new StringBuilderOutput(output));
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

            var options = new LexerOptions();
            if (inputName.Contains("liquid"))
            {
                options.Lang = ScriptLang.Liquid;
            }
            else if (inputName.Contains("scientific"))
            {
                options.Lang = ScriptLang.Scientific;
            }

            var template = parser(templateSource, inputName, default, options);
            if (template.HasErrors || template.Page == null)
            {
                if (inputName.Contains("error"))
                {
                    Assert.Ignore("Template has errors and didn't parse correctly. This is expected for an `error` test.");
                }
                else
                {
                    Console.WriteLine(template.Messages);
                    Assert.Fail("Template has errors and didn't parse correctly. This is not expected.");
                }
            }

            return template;
        }

        private class TestCloneScriptRewriter : ScriptRewriter
        {
        }
    }
}
