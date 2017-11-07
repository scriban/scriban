// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Scriban.Benchmarks
{
    public abstract class RazorTemplatePage
    {
        public dynamic Model;

        public TextWriter Output;
        
        public abstract void Execute();

        protected void WriteLiteral(string text)
        {
            Output.Write(text);
        }

        protected void Write(object obj)
        {
            Output.Write(obj);
        }
    }

    /// <summary>
    /// The code required to compiler an in-memory Razor template with a custom TemplatePage
    /// </summary>
    public class RazorBuilder
    {
        public static RazorTemplatePage Compile(string content)
        {
            var engine = Microsoft.AspNetCore.Razor.Language.RazorEngine.Create(builder => builder.Features.Add(new RazorLightTemplateDocumentClassifierPass()));
            var source = Microsoft.AspNetCore.Razor.Language.RazorSourceDocument.Create(content, "input");
            var razorDoc = Microsoft.AspNetCore.Razor.Language.RazorCodeDocument.Create(source);
            engine.Process(razorDoc);

            var sourceCode = razorDoc.GetCSharpDocument().GeneratedCode;

            SourceText sourceText = SourceText.From(sourceCode, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText);

            // Force dynamic assembly to be linked in
            dynamic assemblyObj = typeof(RazorBuilder).Assembly;
            var filePath = assemblyObj.Location;
            var references = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)).Select(x => x.Location).ToList();

            var metadataReferences = new List<MetadataReference>();
            foreach (var reference in references)
            {
                try
                {
                    var moduleMetadata = ModuleMetadata.CreateFromFile(reference);
                    var assemblyMetadata = AssemblyMetadata.Create(moduleMetadata);
                    var metaDataReference = assemblyMetadata.GetReference(filePath: filePath);
                    metadataReferences.Add(metaDataReference);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Expecting while loading metadata from assembly: {reference}", ex);
                }
            }

            var compilation = CSharpCompilation.Create("InMemory", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary), references: metadataReferences).AddSyntaxTrees(syntaxTree);

            var assemblyStream = new MemoryStream();
            var result = compilation.Emit(assemblyStream);

            if (!result.Success)
            {
                var errorsDiagnostics = result.Diagnostics.Where(d => d.IsWarningAsError || d.Severity == DiagnosticSeverity.Error).ToList();

                var builder = new StringBuilder();

                foreach (Diagnostic diagnostic in errorsDiagnostics)
                {
                    FileLinePositionSpan lineSpan = diagnostic.Location.SourceTree.GetMappedLineSpan(diagnostic.Location.SourceSpan);
                    string errorMessage = diagnostic.GetMessage();
                    string formattedMessage = $"- ({lineSpan.StartLinePosition.Line}:{lineSpan.StartLinePosition.Character}) {errorMessage}";
                    builder.AppendLine(formattedMessage);
                }

                throw new InvalidOperationException(builder.ToString());
            }
            assemblyStream.Position = 0;
            var buffer = assemblyStream.ToArray();
            var assembly = Assembly.Load(buffer);
            var templateType = assembly.GetType("Scriban.Benchmarks.GeneratedTemplate");
            return (RazorTemplatePage)Activator.CreateInstance(templateType);
        }

        public class RazorLightTemplateDocumentClassifierPass : DocumentClassifierPassBase
        {
            protected override string DocumentKind => "razor.scriban.template";

            protected override bool IsMatch(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode) => true;

            protected override void OnDocumentStructureCreated(
                RazorCodeDocument codeDocument,
                NamespaceDeclarationIntermediateNode @namespace,
                ClassDeclarationIntermediateNode @class,
                MethodDeclarationIntermediateNode method)
            {
                string templateKey = codeDocument.Source.FilePath ?? codeDocument.Source.FilePath;

                base.OnDocumentStructureCreated(codeDocument, @namespace, @class, method);

                @namespace.Content = "Scriban.Benchmarks";

                @class.ClassName = "GeneratedTemplate"; //CSharpIdentifier.GetClassNameFromPath(templateKey);
                @class.BaseType = "global::Scriban.Benchmarks.RazorTemplatePage";
                @class.Modifiers.Clear();
                @class.Modifiers.Add("public");

                method.MethodName = "Execute";
                method.Modifiers.Clear();
                method.Modifiers.Add("public");
                method.Modifiers.Add("override");
                method.ReturnType = $"void";
            }
        }
    }
}