using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NuDoq;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.DocGen
{
    /// <summary>
    /// Program generating the documentation for all builtin functions by extracting the code comments from xml files
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var options = new ReaderOptions()
            {
                KeepNewLinesInText = true
            };

            var members = DocReader.Read(typeof(Template).Assembly, options);

            var builtinClassNames = new Dictionary<string, string>()
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

            var writer = new StreamWriter("../../../../doc/builtins.md");

            writer.WriteLine(@"# Builtins

This document describes the various built-in functions available in scriban.

");

            var visitor = new MarkdownVisitor(builtinClassNames);
            members.Accept(visitor);

            writer.WriteLine(visitor.Toc);

            writer.Write(visitor.Body);
            writer.Flush();
            writer.Close();
        }


        public class MarkdownVisitor : Visitor
        {
            private readonly Dictionary<string, string> _builtinClassNames;
            private readonly HashSet<string> _classVisited;
            private readonly StringWriter _writerBody;
            private readonly StringWriter _writerToc;

            private TextWriter _writer;
            private StringWriter _writerParameters;
            private StringWriter _writerReturns;
            private StringWriter _writerSummary;
            private StringWriter _writerRemarks;

            public MarkdownVisitor(Dictionary<string, string> builtinClassNames)
            {
                _classVisited = new HashSet<string>();
                _writerToc = new StringWriter();
                _writerBody = new StringWriter();
                _writerParameters = new StringWriter();
                _writerReturns = new StringWriter();
                _writerSummary = new StringWriter();
                _writerRemarks = new StringWriter();
                _builtinClassNames = builtinClassNames;
            }

            public StringWriter Toc => _writerToc;

            public StringWriter Body => _writerBody;


            public override void VisitMember(Member member)
            {
                var methodInfo = member.Info as MethodInfo;
                string shortName;
                if (methodInfo != null && methodInfo.DeclaringType.Namespace == "Scriban.Functions" && _builtinClassNames.TryGetValue(methodInfo.DeclaringType.Name, out shortName))
                {
                    var methodShortName = StandardMemberRenamer.Default(methodInfo.Name);

                    _writer = _writerBody;

                    if (_classVisited.Add(methodInfo.DeclaringType.Name))
                    {
                        _writer.WriteLine("[:top:](#builtins)");
                        _writer.WriteLine();
                        _writer.WriteLine($"## `{shortName}` functions");

                        // TODO: Write the doc of the class
                        _writer.WriteLine();

                        // Write the toc
                        _writerToc.WriteLine($"- [`{shortName}` functions](#{shortName}-functions)");
                    }

                    _writer.WriteLine();
                    _writer.WriteLine("[:top:](#builtins)");
                    _writer.WriteLine($"### `{shortName}.{methodShortName}`");
                    // Write the toc
                    _writerToc.WriteLine($"  - [`{shortName}.{methodShortName}`](#{shortName}{methodShortName})");

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

                        _writer.Write($"<{parameter.Name}>");
                        if (parameter.IsOptional)
                        {
                            _writer.Write($"?={parameter.DefaultValue}");
                        }
                    }
                    _writer.WriteLine();
                    _writer.WriteLine("```");
                    _writer.WriteLine();

                    base.VisitMember(member);

                    // Write parameters after the signature
                    _writer.WriteLine("#### Description");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerSummary);
                    _writerSummary = new StringWriter();
                    _writer.WriteLine();
                    _writer.WriteLine("#### Arguments");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerParameters);
                    _writerParameters = new StringWriter();
                    _writer.WriteLine("#### Returns");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerReturns);
                    _writerReturns = new StringWriter();
                    _writer.WriteLine();
                    _writer.WriteLine("#### Examples");
                    _writer.WriteLine();
                    _writer.WriteLine(_writerRemarks);
                    _writerRemarks = new StringWriter();
                }
            }

            public override void VisitSummary(Summary summary)
            {
                _writer = _writerSummary;
                base.VisitSummary(summary);
                _writer = _writerBody;
            }

            public override void VisitRemarks(Remarks remarks)
            {
                _writer = _writerRemarks;
                base.VisitRemarks(remarks);
                _writer = _writerBody;
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
                _writer = _writerBody;
            }

            public override void VisitReturns(Returns returns)
            {
                _writer = _writerReturns;
                base.VisitReturns(returns);
                _writer = _writerBody;
            }

            public override void VisitCode(Code code)
            {
                //base.VisitCode(code);
            }

            public override void VisitText(Text text)
            {
                _writer.Write(text.Content);
                //base.VisitText(text);
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

            private string NormalizeLink(string cref)
            {
                if (cref == null)
                {
                    return string.Empty;
                }
                return cref.Replace(":", "-").Replace("(", "-").Replace(")", "");
            }
        }
    }
}
