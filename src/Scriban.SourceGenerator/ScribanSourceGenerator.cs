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
        public void Execute(GeneratorExecutionContext context)
        {
            var files = context.AdditionalFiles.Where(f => StringComparer.OrdinalIgnoreCase.Equals(".stt", Path.GetExtension(f.Path)));
            foreach(var file in files)
            {
                var templateText = File.ReadAllText(file.Path);
                var src = Template.Parse(templateText).Render();                
                context.AddSource(Path.GetFileName(file.Path) + ".cs", SourceText.From(src, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
