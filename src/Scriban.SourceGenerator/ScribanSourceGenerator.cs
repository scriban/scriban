using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Scriban.SourceGenerator
{
    [Generator]
    public class ScribanSourceGenerator : ISourceGenerator
    {
        static readonly DiagnosticDescriptor TemplateError =
            new DiagnosticDescriptor("SG0001", "Template Error", "{0}", "Template", DiagnosticSeverity.Error, true);

        public void Execute(GeneratorExecutionContext context)
        {
            var files = context.AdditionalFiles.Where(f => StringComparer.OrdinalIgnoreCase.Equals(".stt", Path.GetExtension(f.Path)));

            foreach (var file in files)
            {
                var path = file.Path;
                var templateText = File.ReadAllText(path);

                var template = Template.Parse(templateText, Path.GetFullPath(path));
                if (template.HasErrors)
                {
                    foreach (var err in template.Messages)
                    {
                        var location = err.Span.ToLocation();
                        var diag = Diagnostic.Create(TemplateError, location, err.Message);
                        context.ReportDiagnostic(diag);
                    }
                    continue;
                }

                // TODO: Is there a render option that can render to a TextWriter?
                // then I could render directly to a memory stream that I could pass to SourceText.
                var sourceText = template.Render();
                var ms = new MemoryStream();
                var sw = new StreamWriter(ms, Encoding.UTF8);
                sw.Write(sourceText);
                sw.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                var source = SourceText.From(ms, Encoding.UTF8, canBeEmbedded: true);

                context.AddSource(Path.GetFileName(file.Path) + ".cs", source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
