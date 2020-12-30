using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using Roslynator;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using INamedTypeSymbol = Microsoft.CodeAnalysis.INamedTypeSymbol;

namespace Scriban.AsyncCodeGen
{
    /// <summary>
    /// Add support for async/await code for Scriban automatically from existing synchronous code
    /// </summary>
    class Program
    {
        private static INamedTypeSymbol _scriptNodeType;
        private static INamedTypeSymbol _scriptListType;

        static async Task Main(string[] args)
        {
            Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>()
            {
                {"TargetFramework", "netstandard2.0"},
                {"DefineConstants", "SCRIBAN_NO_ASYNC;SCRIBAN_PUBLIC" }
            });

            var solution = await workspace.OpenSolutionAsync(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../../Scriban.sln")), new ConsoleProgressReporter());
            var project = solution.Projects.First(x => x.Name == "Scriban");
            var compilation = await project.GetCompilationAsync();

            var errors = compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

            if (errors.Count > 0)
            {
                Console.WriteLine("Compilation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                Console.WriteLine("Error, Exiting.");
                Environment.Exit(1);
                return;
            }

            _scriptNodeType = compilation.GetTypeByMetadataName("Scriban.Syntax.ScriptNode");
            _scriptListType = compilation.GetTypeByMetadataName("Scriban.Syntax.ScriptList");

            var models = compilation.SyntaxTrees.Select(tree => compilation.GetSemanticModel(tree)).ToList();

            var methods = new Stack<IMethodSymbol>();
            var visited = new HashSet<IMethodSymbol>();

            // ----------------------------------------------------------------------------------
            // 1) Collect origin methods from IScriptOutput.Write and all ScriptNode.Evaluate methods
            // ----------------------------------------------------------------------------------
            foreach (var model in models)
            {
                foreach (var methodDeclaration in model.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (methodDeclaration.Parent is InterfaceDeclarationSyntax)
                    {
                        var interfaceDecl = (InterfaceDeclarationSyntax) methodDeclaration.Parent;

                        var interfaceType = model.GetDeclaredSymbol(interfaceDecl);
                        if (interfaceType != null && interfaceType.ContainingNamespace.Name == "Runtime" && (interfaceType.Name == "IScriptOutput" || interfaceType.Name == "IScriptCustomFunction" || (interfaceType.Name == "ITemplateLoader" && methodDeclaration.Identifier.Text == "Load")))
                        {
                            var method = model.GetDeclaredSymbol(methodDeclaration);

                            // Convert only IScriptCustomFunction.Invoke
                            if (interfaceType.Name == "IScriptCustomFunction" && method.Name != "Invoke") continue;

                            if (visited.Add(method))
                            {
                                methods.Push(method);
                            }
                        }
                    }
                    else
                    {
                        var methodModel = model.GetDeclaredSymbol(methodDeclaration);
                        if (!methodModel.IsStatic && (methodModel.Name == "Evaluate" || methodModel.Name == "EvaluateImpl") && methodModel.Parameters.Length == 1 && methodModel.Parameters[0].Type.Name == "TemplateContext" && InheritFrom(methodModel.ReceiverType, "Syntax", "ScriptNode"))
                        {
                            while (methodModel != null)
                            {
                                if (visited.Add(methodModel))
                                {
                                    methods.Push(methodModel);
                                }
                                methodModel = methodModel.OverriddenMethod;
                            }
                        }
                    }
                }
            }

            var originalMethods = new List<IMethodSymbol>(methods);

            // ----------------------------------------------------------------------------------
            // 2) Collect method graph calls
            // ----------------------------------------------------------------------------------
            var methodGraph = new Dictionary<IMethodSymbol, HashSet<ITypeSymbol>>();
            var classGraph = new Dictionary<ITypeSymbol, ClassToTransform>();

            visited.Clear();
            while (methods.Count > 0)
            {
                var method = methods.Pop();
                if (!visited.Add(method))
                {
                    continue;
                }

                HashSet<ITypeSymbol> callerTypes;
                if (!methodGraph.TryGetValue(method, out callerTypes))
                {
                    callerTypes = new HashSet<ITypeSymbol>();
                    methodGraph.Add(method, callerTypes);
                }

                var finds = await SymbolFinder.FindCallersAsync(method, solution);
                foreach (var referencer in finds.Where(f => f.IsDirect))
                {
                    var doc  =solution.GetDocument(referencer.Locations.First().SourceTree);
                    if (doc.Project != project)
                    {
                        continue;
                    }

                    var callingMethodSymbol = (IMethodSymbol)referencer.CallingSymbol;

                    if (callingMethodSymbol.MethodKind == MethodKind.StaticConstructor || callingMethodSymbol.MethodKind == MethodKind.Constructor)
                    {
                        continue;
                    }

                    // Skip methods over than Evaluate for ScriptNode
                    // Skip also entirely any methods related to ScriptVisitor
                    if (callingMethodSymbol.Name == "ToString" ||
                            (callingMethodSymbol.OverriddenMethod != null && callingMethodSymbol.OverriddenMethod.ContainingType.Name == "ScriptNode" && callingMethodSymbol.Name != "Evaluate") ||
                        InheritFrom(callingMethodSymbol.ContainingType, "Syntax", "ScriptVisitor") ||
                        InheritFrom(callingMethodSymbol.ContainingType, "Runtime", "ScriptObject"))
                    {
                        continue;
                    }

                    methods.Push(callingMethodSymbol);

                    // Push the method overriden
                    var methodOverride = callingMethodSymbol;
                    while (methodOverride != null && methodOverride.IsOverride && methodOverride.OverriddenMethod != null)
                    {
                        methods.Push(methodOverride.OverriddenMethod);
                        methodOverride = methodOverride.OverriddenMethod;
                    }

                    var callingSyntax = referencer.CallingSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                    var callingMethod = (MethodDeclarationSyntax)callingSyntax;

                    foreach (var invokeLocation in referencer.Locations)
                    {
                        var invoke = callingMethod.FindNode(invokeLocation.SourceSpan);
                        while (invoke != null && !(invoke is InvocationExpressionSyntax))
                        {
                            invoke = invoke.Parent;
                        }
                        Debug.Assert(invoke is InvocationExpressionSyntax);

                        var declaredSymbol = callingMethodSymbol.ReceiverType;


                        if (declaredSymbol.Name != "ScriptPrinter" && callingMethodSymbol.Parameters.All(x => x.Type.Name != "ScriptPrinter" && x.Type.Name != "ScriptPrinterOptions")
                            && (declaredSymbol.BaseType.Name != "DynamicCustomFunction" || declaredSymbol.Name == "GenericFunctionWrapper"))
                        {
                            ClassToTransform classToTransform;
                            if (!classGraph.TryGetValue(callingMethodSymbol.ReceiverType, out classToTransform))
                            {
                                classToTransform = new ClassToTransform(callingMethodSymbol.ReceiverType);
                                classGraph.Add(callingMethodSymbol.ReceiverType, classToTransform);
                                callerTypes.Add(callingMethodSymbol.ReceiverType);
                            }

                            // Find an existing method to transform
                            var methodToTransform = classToTransform.MethodCalls.FirstOrDefault(x => x.MethodSymbol.Equals(callingMethodSymbol));
                            if (methodToTransform == null)
                            {
                                methodToTransform = new MethodCallToTransform(callingMethodSymbol, callingMethod);
                                classToTransform.MethodCalls.Add(methodToTransform);
                            }

                            // Add a call site
                            methodToTransform.CallSites.Add((InvocationExpressionSyntax)invoke);
                        }
                    }
                }
            }

            // ----------------------------------------------------------------------------------
            // 3) Generate Async Methods
            // ----------------------------------------------------------------------------------

            methods.Clear();
            methods = new Stack<IMethodSymbol>(originalMethods);
            visited.Clear();


            var cu = (CompilationUnitSyntax)ParseSyntaxTree(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
using System.Numerics;
").GetRoot();

            solution = project.Solution;

            // Create namespaces
            var namespaces = new Dictionary<string, NamespaceDeclarationSyntax>();
            foreach (var classToTransform in classGraph.Values)
            {
                NamespaceDeclarationSyntax nsDecl;
                if (!namespaces.TryGetValue(classToTransform.Namespace, out nsDecl))
                {
                    nsDecl = NamespaceDeclaration(ParseName(classToTransform.Namespace)).NormalizeWhitespace()
                        .WithLeadingTrivia(LineFeed)
                        .WithTrailingTrivia(LineFeed);
                    namespaces.Add(classToTransform.Namespace, nsDecl);
                }
            }

            var typeTransformed = new HashSet<ITypeSymbol>();

            while (methods.Count > 0)
            {
                var methodSymbol = methods.Pop();
                if (!visited.Add(methodSymbol))
                {
                    continue;
                }

                var asyncTypes = methodGraph[methodSymbol];

                foreach (var asyncTypeSymbol in asyncTypes)
                {
                    // The type has been already transformed, don't try to transform it
                    if (!typeTransformed.Add(asyncTypeSymbol))
                    {
                        continue;
                    }

                    var callingClass = classGraph[asyncTypeSymbol];

                    var typeDecl = (TypeDeclarationSyntax)asyncTypeSymbol.DeclaringSyntaxReferences[0].GetSyntax();

                    // TODO: Doesn't support nested classes
                    if (typeDecl.Modifiers.All(x => x.Text != "partial"))
                    {
                        var rootSyntax = typeDecl.SyntaxTree.GetRoot();
                        var originalDoc = solution.GetDocument(rootSyntax.SyntaxTree);
                        if (originalDoc != null)
                        {
                            var previousDecl = typeDecl;
                            typeDecl = typeDecl.WithModifiers(typeDecl.Modifiers.Add(Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(Space)));

                            rootSyntax = rootSyntax.ReplaceNode(previousDecl, typeDecl);

                            originalDoc = originalDoc.WithSyntaxRoot(rootSyntax);
                            solution = originalDoc.Project.Solution;
                        }
                    }

                    typeDecl = typeDecl.RemoveNodes(typeDecl.ChildNodes().ToList(), SyntaxRemoveOptions.KeepNoTrivia);

                    // Remove if/endif directive
                    typeDecl = typeDecl.WithCloseBraceToken(Token(RemoveIfDef(typeDecl.CloseBraceToken.LeadingTrivia), SyntaxKind.CloseBraceToken, typeDecl.CloseBraceToken.TrailingTrivia));

                    foreach (var callingMethod in callingClass.MethodCalls)
                    {
                        var methodModel = callingMethod.MethodSymbol;
                        var method = callingMethod.CallerMethod;

                        //Console.WriteLine(method.ToFullString());
                        //Console.Out.Flush();
                        //method = method.TrackNodes(callingMethod.CallSites);
                        //var originalMethod = method;

                        bool addCancellationToken = false;

                        method = method.ReplaceNodes(callingMethod.CallSites, (callSite, r) =>
                        {
                            var leadingTrivia = callSite.GetLeadingTrivia();
                            var newCallSite = callSite.WithLeadingTrivia(Space);

                            switch (newCallSite.Expression)
                            {
                                case MemberBindingExpressionSyntax m:
                                {
                                    var newExpression = m.WithName(IdentifierName(m.Name.ToString() + "Async"));
                                    newCallSite = newCallSite.WithExpression(newExpression);
                                    break;
                                }
                                case IdentifierNameSyntax m:
                                {
                                    var newExpression = m.WithIdentifier(Identifier(m.Identifier.Text.ToString() + "Async"));
                                    newCallSite = newCallSite.WithExpression(newExpression);
                                    break;
                                }
                                case MemberAccessExpressionSyntax m:
                                {
                                    var newExpression = m.WithName(IdentifierName(m.Name.ToString() + "Async"));
                                    newCallSite = newCallSite.WithExpression(newExpression);

                                    var sm = compilation.GetSemanticModel(callSite.SyntaxTree.GetRoot().SyntaxTree);
                                    var originalMember = ((MemberAccessExpressionSyntax) callSite.Expression).Expression;
                                    var symbol = sm.GetSymbolInfo(originalMember).Symbol;

                                    if (symbol != null)
                                    {
                                        if (symbol is IPropertySymbol)
                                        {
                                            var prop = (IPropertySymbol) symbol;
                                            if (prop.Type.Name == "IScriptOutput")
                                            {
                                                newCallSite = newCallSite.WithArgumentList(newCallSite.ArgumentList.AddArguments(Argument(IdentifierName("CancellationToken").WithLeadingTrivia(Space))));
                                            }
                                        }
                                        else if (symbol is IParameterSymbol)
                                        {
                                            var param = (IParameterSymbol) symbol;
                                            if (param.Type.Name == "IScriptOutput")
                                            {
                                                addCancellationToken = true;
                                                newCallSite = newCallSite.WithArgumentList(newCallSite.ArgumentList.AddArguments(Argument(IdentifierName("cancellationToken").WithLeadingTrivia(Space))));
                                            }
                                        }
                                    }

                                    //if (.ReceiverType.Name == "IScriptOutput" || methodModel.ReceiverType.Name == "ScriptOutputExtensions")
                                        //{
                                        //    var existingArguments = newCallSite.ArgumentList;
                                        //    existingArguments = existingArguments.AddArguments(Argument(IdentifierName("CancellationToken")));
                                        //    newCallSite = newCallSite.WithArgumentList(existingArguments);
                                        //}

                                    break;
                                }
                                default:
                                    throw new NotSupportedException($"Expression not supported: {newCallSite.Expression}");
                            }

                            var awaitCall = AwaitExpression(InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            newCallSite,
                                            IdentifierName("ConfigureAwait")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.FalseLiteralExpression))))))
                                .WithAwaitKeyword(Token(leadingTrivia, SyntaxKind.AwaitKeyword, TriviaList(Space)));
                            return awaitCall;
                        });

                        if (addCancellationToken)
                        {
                            method = method.WithParameterList(method.ParameterList.AddParameters(
                                Parameter(Identifier("cancellationToken")).WithType(IdentifierName("CancellationToken")).NormalizeWhitespace()
                            ));
                        }

                        TypeSyntax asyncReturnType;
                        if (methodModel.ReturnsVoid)
                        {
                            asyncReturnType = IdentifierName("ValueTask").WithTrailingTrivia(Space);
                        }
                        else
                        {
                            var trailingTrivia = method.ReturnType.GetTrailingTrivia();

                            asyncReturnType = GenericName(
                                    Identifier("ValueTask"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList(method.ReturnType.WithoutTrailingTrivia()))).WithTrailingTrivia(trailingTrivia);
                        }

                        method = method.WithReturnType(asyncReturnType);

                        // Rename method with `Async` postfix
                        method = method.WithIdentifier(Identifier(method.Identifier.Text + "Async"));

                        // Add async keyword to the method
                        method = method.WithModifiers(method.Modifiers.Add(Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(Space)));

                        // Remove any if/def
                        method = method.WithLeadingTrivia(RemoveIfDef(method.GetLeadingTrivia()));

                        typeDecl = typeDecl.AddMembers(method);

                        methods.Push(callingMethod.MethodSymbol);
                    }

                    //Debug.Assert(typeDecl.Members.All(x => x is MethodDeclarationSyntax));

                    // Order members
                    var orderedMembers = typeDecl.Members.OfType<MemberDeclarationSyntax>().OrderBy(m =>
                    {
                        if (m is PropertyDeclarationSyntax prop) return prop.Identifier.Text;
                        else
                        {
                            return ((MethodDeclarationSyntax) m).Identifier.Text;
                        }
                    }).ToArray();
                    typeDecl = typeDecl.WithMembers(new SyntaxList<MemberDeclarationSyntax>(orderedMembers));

                    // Update namespace
                    namespaces[callingClass.Namespace] = namespaces[callingClass.Namespace].AddMembers(typeDecl);

                    // work on method transforms
                }

                //methodSymbol.ContainingType.
            }

            // Reorder members
            var nsList = namespaces.Values.OrderBy(ns => ns.Name.ToString()).ToList();
            for (var i = 0; i < nsList.Count; i++)
            {
                nsList[i] = ReorderNsMembers(nsList[i]);
            }

            // Add #endif at the end
            var lastns = nsList[nsList.Count - 1];
            nsList[nsList.Count - 1] = lastns.WithCloseBraceToken(lastns.CloseBraceToken.WithTrailingTrivia(Trivia(EndIfDirectiveTrivia(true)))).NormalizeWhitespace();


            var triviaList = cu.GetLeadingTrivia();
            triviaList = triviaList.Insert(0, Trivia(
                IfDirectiveTrivia(
                    IdentifierName("!SCRIBAN_NO_ASYNC"),
                    true,
                    false,
                    false)));
            cu = cu.WithLeadingTrivia(triviaList).NormalizeWhitespace();

            cu = cu.AddMembers(nsList.ToArray());

            cu = (CompilationUnitSyntax)Formatter.Format(cu, workspace);

            // ----------------------------------------------------------------------------------
            // 4) Generate ScriptNodes (visitor, accept methods, children, rewriter...)
            // ----------------------------------------------------------------------------------
            var allScriptNodeTypes = await SymbolFinder.FindDerivedClassesAsync(_scriptNodeType, solution, true, new[] {project}.ToImmutableHashSet());
            var listOfScriptNodes = allScriptNodeTypes.OrderBy(x => x.Name).ToList();
            listOfScriptNodes = listOfScriptNodes.Where(x => (x.DeclaredAccessibility & Accessibility.Public) != 0 && !x.IsAbstract && x.Name != "ScriptList").ToList();

            var scriptNodeViewModels = new List<ScriptNodeViewModel>();
            foreach (var scriptNodeDerivedType in listOfScriptNodes)
            {
                var nodeViewModel = GenerateScriptNodeViewModel(scriptNodeDerivedType);
                scriptNodeViewModels.Add(nodeViewModel);
            }

            var text = GenerateScribanVisitors(scriptNodeViewModels);
            var projectPath = Path.GetDirectoryName(project.FilePath);
            File.WriteAllText(Path.Combine(projectPath, "ScribanVisitors.generated.cs"), text, new UTF8Encoding(false));

            // ----------------------------------------------------------------------------------
            // 5) Output codegen
            // ----------------------------------------------------------------------------------
            //var document = project.AddDocument("ScribanAsync.generated.cs", sourceText, null, "ScribanAsync.generated.cs");
            WriteCu(cu, Path.Combine(projectPath, "ScribanAsync.generated.cs"));

            // Applies changes for partial classes
            workspace.TryApplyChanges(solution);
        }

        private static void WriteCu(CompilationUnitSyntax cu, string path)
        {
            var text = $@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
{cu.ToFullString()}";

            // Normalize NewLine
            text = text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

            File.WriteAllText(path, text, Encoding.UTF8);
        }

        private static bool IsScriptNode(ITypeSymbol typeSymbol)
        {
            return ReferenceEquals(typeSymbol, _scriptNodeType) || typeSymbol.InheritsFrom(_scriptNodeType);
        }

        private static bool IsScriptList(ITypeSymbol typeSymbol)
        {
            return typeSymbol.InheritsFrom(_scriptListType);
        }

        private static NamespaceDeclarationSyntax ReorderNsMembers(NamespaceDeclarationSyntax ns)
        {
            Debug.Assert(ns.Members.All(m => m is TypeDeclarationSyntax));

            var types = ns.Members.OfType<TypeDeclarationSyntax>().OrderBy(t => t.Identifier.Text).ToList();
            ns = ns.WithMembers(new SyntaxList<MemberDeclarationSyntax>(types));
            return ns;
        }

        private class ScriptNodeViewModel
        {
            public ScriptNodeViewModel()
            {
                Properties = new List<ScriptNodePropertyViewModel>();
                CanRewrite = true;
            }

            public string Name { get; set; }

            public bool CanRewrite { get; set; }

            public int ScriptNodePropertyCount { get; set; }

            public List<ScriptNodePropertyViewModel> Properties { get; }
        }

        private class ScriptNodePropertyViewModel
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public bool IsScriptNode { get; set; }

            public bool IsList { get; set; }
        }

        private static ScriptNodeViewModel GenerateScriptNodeViewModel(INamedTypeSymbol typeSymbol)
        {

            var viewModel = new ScriptNodeViewModel();
            viewModel.Name = typeSymbol.Name;

            switch (typeSymbol.Name)
            {
                case "ScriptVariableGlobal":
                case "ScriptVariableLocal":
                case "ScriptVariableLoop":
                    viewModel.CanRewrite = false;
                    break;
            }

            var properties = GetScriptProperties(typeSymbol);
            // Add first all script nodes types
            foreach (var prop in properties.Where(x => IsScriptNode(x.Type)))
            {
                var propViewModel = new ScriptNodePropertyViewModel {Name = prop.Name, Type = prop.Type.Name, IsScriptNode = true, IsList = IsScriptList(prop.Type)};
                viewModel.Properties.Add(propViewModel);
                viewModel.ScriptNodePropertyCount++;
            }
            // Add non script node types
            foreach (var prop in properties.Where(x => !IsScriptNode(x.Type)))
            {
                var propViewModel = new ScriptNodePropertyViewModel {Name = prop.Name, Type = prop.Type.Name};
                viewModel.Properties.Add(propViewModel);
            }

            return viewModel;
        }

        private static string GenerateScribanVisitors(List<ScriptNodeViewModel> nodes)
        {
            nodes.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.Ordinal));
            var sourceFilePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ScribanVisitors.generated.sbncs"));
            var text = File.ReadAllText(sourceFilePath);
            var template = Template.Parse(text, sourceFilePath);

            var result = template.Render(new {nodes = nodes}, memberRenamer: member => member.Name);
            return result;
        }

        private static List<IPropertySymbol> GetScriptProperties(INamedTypeSymbol typeSymbol)
        {
            var types = new Stack<ITypeSymbol>();

            var typeToFetch = typeSymbol;
            while (typeToFetch != null)
            {
                types.Push(typeToFetch);
                typeToFetch = typeToFetch.BaseType;
                if (typeToFetch.Name == "ScriptNode")
                {
                    break;
                }
            }

            var properties = new List<IPropertySymbol>();

            foreach (var type in types)
            {
                var localProperties = type.GetMembers().OfType<IPropertySymbol>().Where(prop =>
                    prop.Name != "Trivias" &&
                    !prop.IsReadOnly &&
                    (prop.SetMethod != null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
                ).ToList();
                properties.AddRange(localProperties);
            }

            return properties;
        }

        private static SyntaxNode GetDeclaredSyntax(ISymbol symbol, Solution solution)
        {
            var typeRefDeclList = symbol.DeclaringSyntaxReferences.Where(x => !solution.GetDocument(x.SyntaxTree).FilePath.Contains(".generated")).ToList();
            if (typeRefDeclList.Count != 1) throw new InvalidOperationException($"Invalid number {typeRefDeclList.Count} of syntax references for {symbol.MetadataName}. Expecting only 1.");
            return typeRefDeclList[0].GetSyntax();
        }

        public static string GetNamespace(ISymbol symbol)
        {
            if (string.IsNullOrEmpty(symbol.ContainingNamespace?.Name))
            {
                return null;
            }

            var restOfResult = GetNamespace(symbol.ContainingNamespace);
            var result = symbol.ContainingNamespace.Name;

            if (restOfResult != null)
                result = restOfResult + '.' + result;

            return result;
        }

        public static SyntaxTriviaList RemoveIfDef(SyntaxTriviaList trivia)
        {
            int inDirective = 0;
            for (int i = 0; i < trivia.Count; i++)
            {
                if (trivia[i].Kind() == SyntaxKind.IfDirectiveTrivia)
                {
                    trivia = trivia.RemoveAt(i);
                    i--;
                    inDirective++;
                }
                else if (trivia[i].Kind() == SyntaxKind.EndIfDirectiveTrivia)
                {
                    trivia = trivia.RemoveAt(i);
                    i--;
                    inDirective--;
                }
                else if (inDirective > 0)
                {
                    trivia = trivia.RemoveAt(i);
                    i--;
                }
            }
            return trivia;
        }

        public static bool InheritFrom(ITypeSymbol type, string nameSpace, string typeName)
        {
            while (type != null)
            {
                if (type.Name == typeName && type.ContainingNamespace.Name == nameSpace)
                {
                    return true;
                }
                else
                {
                    type = type.BaseType;
                }
            }

            return false;
        }

        [DebuggerDisplay("{TypeSymbol}")]
        class ClassToTransform
        {
            public ClassToTransform(ITypeSymbol typeSymbol)
            {
                TypeSymbol = typeSymbol;
                MethodCalls = new List<MethodCallToTransform>();
                Namespace = GetNamespace(typeSymbol);
            }

            public ITypeSymbol TypeSymbol { get; }

            public string Namespace { get; }

            public TypeDeclarationSyntax AsyncDeclarationTypeSyntax { get; set; }

            public List<MethodCallToTransform> MethodCalls { get; }
        }

        [DebuggerDisplay("{MethodSymbol} CallSites: {CallSites.Count}")]
        class MethodCallToTransform
        {
            public MethodCallToTransform(IMethodSymbol methodSymbol, MethodDeclarationSyntax callerMethod)
            {
                MethodSymbol = methodSymbol;
                CallerMethod = callerMethod;
                CallSites = new List<InvocationExpressionSyntax>();
            }

            public IMethodSymbol MethodSymbol { get; }

            public MethodDeclarationSyntax CallerMethod { get; }

            public MethodDeclarationSyntax AsyncCalleeMethod { get; set; }

            public List<InvocationExpressionSyntax> CallSites { get; }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
