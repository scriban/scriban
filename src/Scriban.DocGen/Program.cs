using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NuDoq;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.DocGen
{
    /// <summary>
    /// Program generating the documentation for all builtin functions by extracting the code comments from xml files.
    /// Generates per-group files under site/docs/builtins/.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            var options = new ReaderOptions
            {
                KeepNewLinesInText = true
            };

            var members = DocReader.Read(typeof(Template).Assembly, options);

            var builtinClassNames = new Dictionary<string, string>
            {
                [nameof(ArrayFunctions)] = "array",
                [nameof(DateTimeFunctions)] = "date",
                [nameof(HtmlFunctions)] = "html",
                [nameof(MathFunctions)] = "math",
                [nameof(ObjectFunctions)] = "object",
                [nameof(RegexFunctions)] = "regex",
                [nameof(StringFunctions)] = "string",
                [nameof(TimeSpanFunctions)] = "timespan"
            };

            var visitor = new MarkdownVisitor(builtinClassNames);
            members.Accept(visitor);

            // --- Write per-group files under site/docs/builtins/ ---
            WriteSiteBuiltins(visitor, builtinClassNames);
        }

        /// <summary>
        /// Escapes Scriban delimiters <c>{{ "{{" }}</c> and <c>{{ "}}" }}</c> so that they are not
        /// interpreted by the Lunet template engine when rendering the site.
        /// </summary>
        static string EscapeForSite(string text)
        {
            // Two-pass replacement via placeholders to avoid recursive substitution.
            text = text.Replace("{{", "___OPEN___");
            text = text.Replace("}}", "___CLOSE___");
            text = text.Replace("___OPEN___", "{{ \"{{\" }}");
            text = text.Replace("___CLOSE___", "{{ \"}}\" }}");
            return text;
        }

        /// <summary>
        /// Writes per-group Markdown files with front matter under site/docs/builtins/.
        /// Also writes a readme.md index and a menu.yml for navigation.
        /// </summary>
        static void WriteSiteBuiltins(MarkdownVisitor visitor, Dictionary<string, string> builtinClassNames)
        {
            var baseDir = Path.Combine(AppContext.BaseDirectory, "../../../../../site/docs/builtins");
            var siteDir = Path.GetFullPath(baseDir);
            Directory.CreateDirectory(siteDir);

            var orderedGroups = visitor.ClassWriters.OrderBy(c => c.Key).ToList();

            // --- Write the index page: readme.md ---
            using (var indexWriter = new StreamWriter(Path.Combine(siteDir, "readme.md")))
            {
                indexWriter.WriteLine("---");
                indexWriter.WriteLine("title: \"Built-in functions\"");
                indexWriter.WriteLine("---");
                indexWriter.WriteLine();
                indexWriter.WriteLine("# Built-in functions");
                indexWriter.WriteLine();
                indexWriter.WriteLine("Scriban provides a rich set of built-in functions organized into groups. Click on a group below to see all available functions.");
                indexWriter.WriteLine();

                foreach (var kvp in orderedGroups)
                {
                    var shortName = kvp.Key;
                    var titleCase = char.ToUpperInvariant(shortName[0]) + shortName.Substring(1);
                    indexWriter.WriteLine($"- [`{shortName}` functions]({shortName}.md) — {titleCase} manipulation functions");
                }

                indexWriter.WriteLine();
                indexWriter.WriteLine("> Note: This document was automatically generated from the source code using `Scriban.DocGen`.");
            }

            // --- Write menu.yml ---
            using (var menuWriter = new StreamWriter(Path.Combine(siteDir, "menu.yml")))
            {
                menuWriter.WriteLine("doc:");
                menuWriter.WriteLine("  - {path: readme.md, title: \"<i class='bi bi-gear' aria-hidden='true'></i> Overview\"}");
                foreach (var kvp in orderedGroups)
                {
                    var shortName = kvp.Key;
                    var titleCase = char.ToUpperInvariant(shortName[0]) + shortName.Substring(1);
                    menuWriter.WriteLine($"  - {{path: {shortName}.md, title: \"{titleCase}\"}}");
                }
            }

            // --- Write per-group files ---
            foreach (var kvp in orderedGroups)
            {
                var shortName = kvp.Key;
                var classWriter = kvp.Value;
                var titleCase = char.ToUpperInvariant(shortName[0]) + shortName.Substring(1);
                var filePath = Path.Combine(siteDir, $"{shortName}.md");

                // Get the raw content and adjust heading levels for standalone pages:
                // - "## `x` functions" -> "# `x` functions" (h2 -> h1)
                // - "### `x.method`" -> "## `x.method`" (h3 -> h2)
                // - Remove [:top:](#builtins) links
                var headContent = classWriter.Head.ToString();
                var bodyContent = classWriter.Body.ToString();

                headContent = headContent.Replace("[:top:](#builtins)\r\n\r\n", "");
                headContent = headContent.Replace("[:top:](#builtins)\n\n", "");
                headContent = Regex.Replace(headContent, @"^## ", "# ", RegexOptions.Multiline);

                bodyContent = bodyContent.Replace("[:top:](#builtins)\r\n", "");
                bodyContent = bodyContent.Replace("[:top:](#builtins)\n", "");
                bodyContent = Regex.Replace(bodyContent, @"^### ", "## ", RegexOptions.Multiline);

                // Escape {{ and }} so they are not interpreted by the Lunet Scriban engine
                headContent = EscapeForSite(headContent);
                bodyContent = EscapeForSite(bodyContent);

                using var fileWriter = new StreamWriter(filePath);
                fileWriter.WriteLine("---");
                fileWriter.WriteLine($"title: \"{titleCase} functions\"");
                fileWriter.WriteLine("---");
                fileWriter.WriteLine();
                fileWriter.Write(headContent);
                fileWriter.Write(bodyContent);
                fileWriter.WriteLine();
                fileWriter.WriteLine("> Note: This document was automatically generated from the source code using `Scriban.DocGen`.");
            }
        }


        public class MarkdownVisitor : Visitor
        {
            private readonly Dictionary<string, string> _builtinClassNames;
            private readonly Dictionary<string, ClassWriter> _classWriters;
            private readonly StringWriter _writerToc;

            private StringWriter _writer;
            private StringWriter _writerParameters;
            private StringWriter _writerReturns;
            private StringWriter _writerSummary;
            private StringWriter _writerRemarks;


            public class ClassWriter
            {
                public ClassWriter()
                {
                    Head = new StringWriter();
                    Body = new StringWriter();
                }

                public readonly StringWriter Head;

                public readonly StringWriter Body;
            }


            public MarkdownVisitor(Dictionary<string, string> builtinClassNames)
            {
                _classWriters = new Dictionary<string, ClassWriter>();
                _writerToc = new StringWriter();
                _writerParameters = new StringWriter();
                _writerReturns = new StringWriter();
                _writerSummary = new StringWriter();
                _writerRemarks = new StringWriter();
                _builtinClassNames = builtinClassNames;
            }

            public StringWriter Toc => _writerToc;

            public Dictionary<string, ClassWriter> ClassWriters => _classWriters;

            private bool IsBuiltinType(Type type, out string shortName)
            {
                shortName = null;
                return type.Namespace == "Scriban.Functions" && _builtinClassNames.TryGetValue(type.Name, out shortName);
            }

            public override void VisitMember(Member member)
            {
                var type = member.Info as Type;
                var methodInfo = member.Info as MethodInfo;


                if (type != null && IsBuiltinType(type, out string shortName))
                {
                    var classWriter = new ClassWriter();
                    _classWriters[shortName] = classWriter;

                    _writer = classWriter.Head;

                    _writer.WriteLine("[:top:](#builtins)");
                    _writer.WriteLine();
                    _writer.WriteLine($"## `{shortName}` functions");
                    _writer.WriteLine();

                    base.VisitMember(member);

                    _writer = classWriter.Head;

                    _writer.WriteLine(_writerSummary);
                    _writer.WriteLine();

                    // Write the toc
                    _writerToc.WriteLine($"- [`{shortName}` functions](#{shortName}-functions)");
                }
                else if (methodInfo != null && IsBuiltinType(methodInfo.DeclaringType, out shortName)  && methodInfo.IsPublic)
                {
                    var methodShortName = StandardMemberRenamer.Default(methodInfo);

                    var classWriter = _classWriters[shortName];

                    // Write the toc — GitHub-style same-page anchor (dots stripped from heading)
                    classWriter.Head.WriteLine($"- [`{shortName}.{methodShortName}`](#{shortName}{methodShortName})");

                    _writer = classWriter.Body;
                    _writer.WriteLine();
                    _writer.WriteLine("[:top:](#builtins)");
                    _writer.WriteLine($"### `{shortName}.{methodShortName}`");
                    _writer.WriteLine();
                    _writer.WriteLine("```");
                    _writer.Write($"{shortName}.{methodShortName}");
                    foreach (var parameter in methodInfo.GetParameters())
                    {
                        if (parameter.ParameterType == typeof(TemplateContext) || parameter.ParameterType == typeof(SourceSpan))
                        {
                            continue;
                        }
                        _writer.Write(" ");

                        _writer.Write($"<{parameter.Name}");
                        if (parameter.IsOptional)
                        {
                            var defaultValue = parameter.DefaultValue;
                            if (defaultValue is string)
                            {
                                defaultValue = "\"" + defaultValue + "\"";
                            }

                            if (defaultValue != null)
                            {
                                defaultValue = ": " + defaultValue;
                            }
                            _writer.Write($"{defaultValue}>?");
                        }
                        else
                        {
                            _writer.Write(">");
                        }
                    }
                    _writer.WriteLine();
                    _writer.WriteLine("```");
                    _writer.WriteLine();

                    base.VisitMember(member);

                    _writer = classWriter.Body;

                    // Write parameters after the signature
                    _writer.WriteLine("#### Description");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerSummary);
                    _writer.WriteLine();
                    _writer.WriteLine("#### Arguments");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerParameters);
                    _writer.WriteLine("#### Returns");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerReturns);
                    _writer.WriteLine();
                    _writer.WriteLine("#### Examples");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerRemarks);
                }

                _writerSummary = new StringWriter();
                _writerParameters = new StringWriter();
                _writerReturns = new StringWriter();
                _writerRemarks = new StringWriter();
            }

            public override void VisitSummary(Summary summary)
            {
                _writer = _writerSummary;
                base.VisitSummary(summary);
            }

            public override void VisitRemarks(Remarks remarks)
            {
                _writer = _writerRemarks;
                base.VisitRemarks(remarks);
            }

            public override void VisitExample(Example example)
            {
                //base.VisitExample(example);
            }

            public override void VisitC(C code)
            {
                //// Wrap inline code in ` according to Markdown syntax.
                //Console.Write(" `");
                //Console.Write(code.Content);
                //Console.Write("` ");

                base.VisitC(code);
            }

            public override void VisitParam(Param param)
            {
                if (param.Name == "context" || param.Name == "span")
                {
                    return;
                }

                _writer = _writerParameters;
                _writer.Write($"- `{param.Name}`: ");
                base.VisitParam(param);
                _writer.WriteLine();
            }

            public override void VisitReturns(Returns returns)
            {
                _writer = _writerReturns;
                base.VisitReturns(returns);
            }

            public override void VisitCode(Code code)
            {
                //base.VisitCode(code);
            }

            public override void VisitText(Text text)
            {
                var content = text.Content;

                content = content.Replace("```scriban-html", "> **input**\r\n```scriban-html");
                content = AddTryOutLink(content);
                content = content.Replace("```html", "> **output**\r\n```html");

                _writer.Write(content);
            }

            private static string AddTryOutLink(string content)
            {
                const string inputText = "> **input**";
                if (!content.Contains(inputText)) return content;

                var regex = new Regex(@"```scriban\-html(?:\r\n|\r|\n)([\S\s]*?)(?:\r\n|\r|\n)```", RegexOptions.Multiline);
                var matches = regex.Matches(content);
                foreach (Match match in matches)
                {
                    var template = match.Groups[1].Value.ReplaceLineEndings("\n");
                    var link = $"/?template={Uri.EscapeDataString(template)}&model=%7B%7D";
                    content = content.Replace($"{inputText}\r\n{match.Value}", $"{inputText} [Try out]({link})\r\n{match.Value}");
                }

                return content;
            }

            public override void VisitPara(Para para)
            {
                //base.VisitPara(para);
            }

            public override void VisitSee(See see)
            {
                //var cref = NormalizeLink(see.Cref);
                //Console.Write(" [{0}]({1}) ", cref.Substring(2), cref);
            }

            public override void VisitSeeAlso(SeeAlso seeAlso)
            {
                //if (seeAlso.Cref != null)
                //{
                //    var cref = NormalizeLink(seeAlso.Cref);
                //    Console.WriteLine("[{0}]({1})", cref.Substring(2), cref);
                //}
            }
        }
    }
}
