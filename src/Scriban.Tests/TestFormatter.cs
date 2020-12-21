// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Scriban.Tests
{
    public class TestFormatter
    {
        [Test]
        public void TestCompressSpaces()
        {
            {
                var result = Format(ParseScriptOnly("  x  "), ScriptFormatterFlags.CompressSpaces);
                Assert.AreEqual("x", result);
            }

            {
                var result = Format(ParseScriptOnly("  1  "), ScriptFormatterFlags.CompressSpaces);
                Assert.AreEqual("1", result);
            }

            {
                var result = Format(ParseScriptOnly("  x   +2  ;  "), ScriptFormatterFlags.CompressSpaces);
                Assert.AreEqual("x +2;", result);
            }

            {
                var result = Format(ParseScriptOnly("  x +   2 \n\n  "), ScriptFormatterFlags.CompressSpaces);
                Assert.AreEqual("x + 2", result);
            }

            {
                var result = Format(ParseScriptOnly("  x +   2 \n\n; y\n x;"), ScriptFormatterFlags.CompressSpaces);
                // Note that we don't remove consecutive \n
                Assert.AreEqual("x + 2\n\n; y\nx;", result);
            }
            {
                var result = Format(ParseScriptOnly("  x +   2     \n      y\n x    \n     "), ScriptFormatterFlags.CompressSpaces);
                // Note that we don't remove consecutive \n
                Assert.AreEqual("x + 2\ny\nx", result);
            }
        }

        [Test]
        public void TestSimpleBinary()
        {
            var script = ParseScriptOnly("  x   +2  ; ");
            {
                var result = Format(script, ScriptFormatterFlags.RemoveExistingTrivias);
                Assert.AreEqual("x+2", result);
            }

            {
                var result = Format(script, ScriptFormatterFlags.CompressSpaces);
                Assert.AreEqual("x +2;", result);
            }

            {
                var result = Format(script, ScriptFormatterFlags.CompressSpaces | ScriptFormatterFlags.AddSpaceBetweenOperators);
                Assert.AreEqual("x + 2;", result);
            }
        }

        [Test]
        public void TestSimple()
        {
            var script = ParseScriptOnly("x+   1 * 2 /5; y+2\n z= z+ 3; ");
            {
                var newScript = Format(script, ScriptFormatterFlags.RemoveExistingTrivias);
                var result = newScript.ToString();
                Assert.AreEqual("x+1*2/5; y+2; z=z+3", result);
            }

            {
                var result = Format(script, ScriptFormatterFlags.CompressSpaces);
                Assert.AreEqual("x+ 1 * 2 /5; y+2\nz= z+ 3;", result);
            }

            {
                var result = Format(script,ScriptFormatterFlags.CompressSpaces | ScriptFormatterFlags.AddSpaceBetweenOperators);
                Assert.AreEqual("x + 1 * 2 / 5; y + 2\nz = z + 3;", result);
            }

            {
                var result = Format(script,ScriptFormatterFlags.CompressSpaces | ScriptFormatterFlags.AddSpaceBetweenOperators | ScriptFormatterFlags.ExplicitParenthesis);
                Assert.AreEqual("x + ((1 * 2) / 5); y + 2\nz = z + 3;", result);
            }
        }

        [Test]
        public void TestEosWithEnd()
        {
            var templateText = @"
{{-
func foo
   if b
-}}
bar
{{-
    end
end
-}}{{foo}}";
            var template = Template.Parse(templateText);
            var templateText2 = template.ToText();
            var template2 = Template.Parse(templateText2);
            var templateText3 = template2.ToText();
            TextAssert.AreEqual(templateText2, templateText3);
            TextAssert.AreEqual(template.Render(), template2.Render());
        }

        /// <summary>
        /// Case with anonymous functions
        /// </summary>
        [Test]
        public void TestEosWithEnd2()
        {
            var templateText = @"
{{-
foo = (do; ret 'fo'; end) + (do; ret 'o'; end)
-}}
{{foo}}";
            var template = Template.Parse(templateText);
            var templateText2 = template.ToText();
            var template2 = Template.Parse(templateText2);
            var templateText3 = template2.ToText();
            Console.WriteLine(templateText3);
            TextAssert.AreEqual(templateText2, templateText3);
            TextAssert.AreEqual(template.Render(), template2.Render());
        }

        private static Template ParseScriptOnly(string text)
        {
            var script = Template.Parse(text, lexerOptions: new LexerOptions() {KeepTrivia = true, Mode = ScriptMode.ScriptOnly});
            Assert.False(script.HasErrors, $"Invalid errors while parsing `{text}`: {string.Join("\n", script.Messages)}");
            return script;
        }

        private static string Format(Template script, ScriptFormatterFlags flags)
        {
            var newScript = script.Page.Format(new ScriptFormatterOptions(flags));
            var result = newScript.ToString();
            return result;
        }
   }
}