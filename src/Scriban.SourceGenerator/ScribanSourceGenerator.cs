using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        static int Count;
        static int CtorCount;
        static TimeSpan Duration;
        static int CacheHit;
        static int CacheMiss;
        static int MaxLength;

        public ScribanSourceGenerator()
        {
            CtorCount++;
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
            Count++;
            var stopwatch = Stopwatch.StartNew();
            
            var files = context.AdditionalFiles.Where(f => StringComparer.OrdinalIgnoreCase.Equals(".stt", Path.GetExtension(f.Path)));

            foreach (var file in files)
            {
                var path = file.Path;
                var updateTime = File.GetLastWriteTimeUtc(path);
                var genSourceName = Path.GetFileName(file.Path) + ".cs";
                CachedSource cs;
                if (cache.TryGetValue(path, out cs)) {
                    if(updateTime <= cs.Timestamp)
                    {
                        CacheHit++;
                        context.AddSource(genSourceName, cs.Source);
                        continue;
                    } else
                    {
                        CacheMiss++;
                    }
                } else
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
                
                MaxLength = Math.Max(MaxLength, (int) ms.Length);
                var buf = ms.GetBuffer();
                var source = SourceText.From(buf, (int)ms.Length, Encoding.UTF8, canBeEmbedded: true);
                cs.Timestamp = updateTime;
                cs.Source = source;
                var name = Path.GetFileName(file.Path) + ".cs";

                context.AddSource(name, source);
            }

            stopwatch.Stop();
            Duration += stopwatch.Elapsed;

            //EmitDebugInfo();
        }

        // hack to allow inspecting some metrics.
        void EmitDebugInfo()
        {
            try
            {
                using var sw = File.CreateText("debugInfo.txt");
                sw.WriteLine("Duration = " + Duration.ToString() + "");
                sw.WriteLine("GenCount = " + Count);
                sw.WriteLine("Ctor = " + CtorCount);
                sw.WriteLine("Hits = " + CacheHit);
                sw.WriteLine("Misses = " + CacheMiss);
                sw.WriteLine("MaxLen = " + MaxLength);
            }
            catch (Exception) { }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
