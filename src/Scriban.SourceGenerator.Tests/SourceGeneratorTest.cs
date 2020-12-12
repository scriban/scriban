using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Scriban.SourceGenerator.Tests
{
    class TestAdditionalText : AdditionalText
    {
        string path;

        public TestAdditionalText(string path)
        {
            this.path = path;
        }
        public override string Path => path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(File.ReadAllText(this.path), Encoding.UTF8);
        }
    }

    public abstract class SourceGeneratorTest
    {
        static readonly MetadataReference CoreLibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
        static readonly MetadataReference NetStdReference = GetMetadataReference("netstandard");
        static readonly MetadataReference RuntimeReference = GetMetadataReference("System.Runtime");
        static readonly MetadataReference ConsoleReference = GetMetadataReference("System.Console");
        static readonly MetadataReference FileSystemReference = GetMetadataReference("System.IO.FileSystem");


        static MetadataReference GetMetadataReference(string name)
        {
            var path = Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), name + ".dll");
            return MetadataReference.CreateFromFile(path);
        }

        public SourceGeneratorResult RunSourceGeneratorTest<T>([CallerMemberName] string testName = null)
            where T : ISourceGenerator, new()
        {
            return RunSourceGeneratorTest(testName, new T());
        }

        public SourceGeneratorResult RunSourceGeneratorTest(string testName, ISourceGenerator sg)
        {
            var co =
                new CSharpCompilationOptions(
                    OutputKind.ConsoleApplication,
                    platform: Platform.AnyCpu,
                    optimizationLevel: OptimizationLevel.Debug
                );

            var testDir = Path.Combine("TestFiles", testName);

            var sourceFiles =
                Directory
                .EnumerateFiles(testDir, "*.cs", SearchOption.AllDirectories)
                .Select(f => Path.GetFullPath(f))
                .ToArray();

            var ets = new List<EmbeddedText>();

            var syntaxes =
                sourceFiles
                .Select(f => {
                    using var r = File.OpenText(f);
                    var text = r.ReadToEnd();
                    var enc = r.CurrentEncoding;
                    var et = EmbeddedText.FromStream(f, File.OpenRead(f));
                    ets.Add(et);
                    return CSharpSyntaxTree.ParseText(text, path: f, encoding: enc);
                });

            

            var additionalFiles =
                Directory
                .EnumerateFiles(testDir)
                .Where(f => Path.GetExtension(f) != ".cs")
                .Select(f => new TestAdditionalText(f))
                .Cast<AdditionalText>()
                .ToImmutableArray();

            var asmName = "test_" + this.GetType().Name + "_" + testName;

            var c = CSharpCompilation
                .Create(asmName)
                .AddSyntaxTrees(syntaxes.ToArray())
                .AddReferences(CoreLibReference)
                .AddReferences(RuntimeReference)
                .AddReferences(ConsoleReference)
                .AddReferences(FileSystemReference)
                .WithOptions(co);

            var opts = CSharpParseOptions.Default;
            var gd =
                CSharpGeneratorDriver
                .Create(sg)
                .AddAdditionalTexts(additionalFiles);

            Compilation fullCompilation;
            ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;

            gd = gd.RunGeneratorsAndUpdateCompilation(c, out fullCompilation, out diagnostics);
            var runResult = gd.GetRunResult();

            foreach (var srcTxt in runResult.GeneratedTrees)
            {
                var et = EmbeddedText.FromSource(srcTxt.FilePath, srcTxt.GetText());
                ets.Add(et);
            }

            var assemblyStream = new MemoryStream();
            var symbolsStream = new MemoryStream();

            var eo = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
            var result = fullCompilation.Emit(assemblyStream, symbolsStream, embeddedTexts: ets, options: eo);

            if (result.Success == false)
            {
                return new SourceGeneratorResult(result, null, diagnostics);
            }

            var assemblyBytes = assemblyStream.GetBuffer();
            var symbolsBytes = symbolsStream.GetBuffer();
            var asm = Assembly.Load(assemblyBytes, symbolsBytes);

            return new SourceGeneratorResult(result, asm, diagnostics);
        }
    }

    public sealed class SourceGeneratorResult
    {
        Assembly asm;

        public ImmutableArray<Diagnostic> SourceGeneratorDiagnostics { get; }

        public SourceGeneratorResult(EmitResult emitResult, Assembly asm, ImmutableArray<Diagnostic> diagnostics)
        {
            this.CompileResult = emitResult;
            this.SourceGeneratorDiagnostics = diagnostics;
            this.asm = asm;
        }

        public EmitResult CompileResult { get; }

        public void AssertSuccess()
        {
            if (this.SourceGeneratorDiagnostics.Any())
            {
                var sw = new StringWriter();
                sw.WriteLine("Source generation failed:");
                foreach (var diag in SourceGeneratorDiagnostics)
                {
                    sw.WriteLine(diag.ToString());
                }
                throw new Exception(sw.ToString());
            }

            if (!CompileResult.Success)
            {
                var sw = new StringWriter();
                sw.WriteLine("Compilation failed:");
                foreach (var diag in CompileResult.Diagnostics)
                {
                    sw.WriteLine(diag.ToString());
                }
                throw new Exception(sw.ToString());
            }
        }

        public InvocationResult Invoke()
        {
            return Invoke(Array.Empty<object>());
        }

        public InvocationResult Invoke(params string[] args)
        {
            return Invoke(new object[] { args });
        }

        InvocationResult Invoke(object[] args)
        {
            var sw = new TestTextWriter();
            Exception ex = null;
            int? result = null;
            bool success = false;
            try
            {
                var returnValue = asm.EntryPoint.Invoke(null, args);
                result = returnValue is int i ? i : (int?)null;
                success = true;
            }
            catch (TargetInvocationException e)
            {
                ex = e.InnerException;
            }
            var output = sw.ToString();
            return new InvocationResult(success, result, output, ex);
        }
    }

    public class InvocationResult
    {
        public bool Success { get; }
        public int? Result { get; }
        public string Output { get; }
        public Exception Exception { get; }

        public InvocationResult(bool success, int? result, string output, Exception ex = null)
        {
            this.Success = success;
            this.Result = result;
            this.Output = output;
            this.Exception = ex;
        }
    }
}

