using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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


        static readonly DiagnosticDescriptor TemplateError =
            new DiagnosticDescriptor("SG0001", "Template Error", "{0}", "Template", DiagnosticSeverity.Error, true);

        public void Execute(GeneratorExecutionContext context)
        {
            var files = context.AdditionalFiles.Where(f => StringComparer.OrdinalIgnoreCase.Equals(".stt", Path.GetExtension(f.Path)));

            foreach(var file in files)
            {
                var path = file.Path;
                var templateText = File.ReadAllText(path);
                string source;

                var template = Template.Parse(templateText);
                if (template.HasErrors)
                {
                    foreach(var err in template.Messages)
                    {
                        var location = err.Span.ToLocation(path);
                        var diag = Diagnostic.Create(TemplateError, location, err.Message);
                        context.ReportDiagnostic(diag);
                    }
                }

                source = template.Render();

                context.AddSource(Path.GetFileName(file.Path) + ".cs", SourceText.From(source, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
