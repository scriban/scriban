using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NuDoq;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.DocGen
{
    /// <summary>
    /// Program generating the documentation for all builtin functions by extracting the code comments from xml files
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

            var writer = new StreamWriter("../../../../../doc/builtins.md");

            writer.WriteLine(@"# Builtins

This document describes the various built-in functions available in scriban.

");

            var visitor = new MarkdownVisitor(builtinClassNames);
            members.Accept(visitor);

            writer.WriteLine(visitor.Toc);

            foreach (var classWriter in visitor.ClassWriters.OrderBy(c => c.Key).Select(c => c.Value))
            {
                writer.Write(classWriter.Head);
                writer.Write(classWriter.Body);
            }

            writer.WriteLine();
            writer.WriteLine("> Note: This document was automatically generated from the sourcecode using `Scriban.DocGen` program");
            writer.Flush();
            writer.Close();
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
                else if (methodInfo != null && IsBuiltinType(methodInfo.DeclaringType, out shortName))
                {
                    var methodShortName = StandardMemberRenamer.Default(methodInfo);

                    var classWriter = _classWriters[shortName];

                    // Write the toc
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
                content = content.Replace("```html", "> **output**\r\n```html");

                _writer.Write(content);
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
