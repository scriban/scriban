using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Scriban.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scriban.SourceGenerator
{
    [Generator]
    public class ScribanSourceGenerator : ISourceGenerator
    {

#pragma warning disable RS2008 // analyzer release tracking

        static readonly DiagnosticDescriptor TemplateError =
            new DiagnosticDescriptor("ScribanGen0001", "Template Error", "{0}", "Template", DiagnosticSeverity.Error, true);

        static readonly DiagnosticDescriptor TemplateWarning =
            new DiagnosticDescriptor("ScribanGen0002", "Template Warning", "{0}", "Template", DiagnosticSeverity.Warning, true);

        static readonly DiagnosticDescriptor ScriptError =
            new DiagnosticDescriptor("ScribanGen1001", "Script Error", "{0}", "Script", DiagnosticSeverity.Error, true);

#pragma warning restore RS2008


        public ScribanSourceGenerator()
        {
            cache = new Dictionary<string, CachedSource>();
        }

        Dictionary<string, CachedSource> cache;

        class CachedSource
        {
            public SourceText Source { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var files = context.AdditionalFiles.Where(f => StringComparer.OrdinalIgnoreCase.Equals(".stt", Path.GetExtension(f.Path)));

            foreach (var file in files)
            {
                var path = file.Path;
                var updateTime = File.GetLastWriteTimeUtc(path);
                var genSourceName = Path.GetFileName(file.Path) + ".cs";
                CachedSource cs;
                if (cache.TryGetValue(path, out cs))
                {
                    if (updateTime <= cs.Timestamp)
                    {
                        context.AddSource(genSourceName, cs.Source);
                        continue;
                    }
                }
                else
                {
                    cache.Add(path, cs = new CachedSource());
                }

                var templateText = File.ReadAllText(path);

                var template = Template.Parse(templateText, Path.GetFullPath(path));
                if (template.HasErrors)
                {
                    foreach (var err in template.Messages)
                    {
                        var location = err.Span.ToLocation();
                        var dd = err.Type == Parsing.ParserMessageType.Error
                            ? TemplateError
                            : TemplateWarning;

                        var diag = Diagnostic.Create(dd, location, err.Message);
                        context.ReportDiagnostic(diag);
                    }
                    continue;
                }

                // TODO: Is there a render option that can render to a TextWriter?
                // then I could render directly to a memory stream that I could pass to SourceText.
                string sourceText;
                try
                {
                    sourceText = template.Render();
                }
                catch (ScriptRuntimeException sre)
                {
                    var location = sre.Span.ToLocation();
                    var diag = Diagnostic.Create(ScriptError, location, sre.OriginalMessage);
                    context.ReportDiagnostic(diag);
                    continue;
                }
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms, Encoding.UTF8);
                sw.Write(sourceText);
                sw.Flush();

                var buf = ms.GetBuffer();
                var source = SourceText.From(buf, (int)ms.Length, Encoding.UTF8, canBeEmbedded: true);
                cs.Timestamp = updateTime;
                cs.Source = source;
                var name = Path.GetFileName(file.Path) + ".cs";

                context.AddSource(name, source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
