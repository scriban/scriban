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
                {"DefineConstants", "SCRIBAN_NO_ASYNC" }
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
                        InheritFrom(callingMethodSymbol.ContainingType, "Syntax", "ScriptVisitor"))
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
using System.Globalization;
using System.Reflection;
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
            var cuNodes = (CompilationUnitSyntax)ParseSyntaxTree(@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
using System.Numerics;
").GetRoot();

            var scriptSyntaxNs = NamespaceDeclaration(
                QualifiedName(
                    IdentifierName("Scriban"),
                    IdentifierName("Syntax")));

            var allScriptNodeTypes = await SymbolFinder.FindDerivedClassesAsync(_scriptNodeType, solution, new[] {project}.ToImmutableHashSet());
            var listOfScriptNodes = allScriptNodeTypes.OrderBy(x => x.Name).ToList();
            listOfScriptNodes = listOfScriptNodes.Where(x => (x.DeclaredAccessibility & Accessibility.Public) != 0 && !x.IsAbstract && x.Name != "ScriptList").ToList();

            foreach (var scriptNodeDerivedType in listOfScriptNodes)
            {
                var typeDecl = GenerateChildrenCountAndGetChildrenImplMethods(scriptNodeDerivedType);
                scriptSyntaxNs = scriptSyntaxNs.AddMembers(typeDecl);
            }

            // Generate visitors and rewriters
            var newMembers = GenerateVisitors(listOfScriptNodes);
            newMembers.AddRange(GenerateRewriters(listOfScriptNodes));
            scriptSyntaxNs = scriptSyntaxNs.AddMembers(newMembers.ToArray());

            scriptSyntaxNs = ReorderNsMembers(scriptSyntaxNs);

            cuNodes = cuNodes.AddMembers(scriptSyntaxNs);
            cuNodes = (CompilationUnitSyntax)Formatter.Format(cuNodes, workspace);

            // ----------------------------------------------------------------------------------
            // 5) Output codegen
            // ----------------------------------------------------------------------------------

            //var document = project.AddDocument("ScribanAsync.generated.cs", sourceText, null, "ScribanAsync.generated.cs");
            var projectPath = Path.GetDirectoryName(project.FilePath);

            WriteCu(cu, Path.Combine(projectPath, "ScribanAsync.generated.cs"));
            WriteCu(cuNodes, Path.Combine(projectPath, "ScribanVisitors.generated.cs"));

            // Applies changes for partial classes
            workspace.TryApplyChanges(solution);
        }

        private static void WriteCu(CompilationUnitSyntax cu, string path)
        {
            var text = $@"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Date:{DateTime.Now.ToString(CultureInfo.InvariantCulture.DateTimeFormat)}
//     Runtime Version:{Environment.Version}
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

        private static TypeDeclarationSyntax GenerateChildrenCountAndGetChildrenImplMethods(INamedTypeSymbol typeSymbol)
        {
            var properties = GetScriptProperties(typeSymbol, false);

            // Generate implementation for ScriptNode.GetChildrenImpl
            // protected override ScriptNode GetChildrenImpl(int index) => null;
            var getChildrenImplDecl = MethodDeclaration(
                    IdentifierName("ScriptNode"),
                    Identifier("GetChildrenImpl"))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.ProtectedKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        }))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("index"))
                                .WithType(
                                    PredefinedType(
                                        Token(SyntaxKind.IntKeyword))))));

            if (properties.Count == 0)
            {
                // protected override ScriptNode GetChildrenImpl(int index) => null;
                getChildrenImplDecl = getChildrenImplDecl.WithExpressionBody(
                    ArrowExpressionClause(
                        LiteralExpression(
                            SyntaxKind.NullLiteralExpression))).WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));
            }
            else if (properties.Count == 1)
            {
                // protected override ScriptNode GetChildrenImpl(int index) => Statements;
                getChildrenImplDecl = getChildrenImplDecl.WithExpressionBody(
                        ArrowExpressionClause(
                            IdentifierName(properties[0].Name)))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                // protected override ScriptNode GetChildrenImpl(int index)
                // {
                //     return index switch
                //     {
                //         0 => Left,
                //         1 => OperatorToken,
                //         2 => Right,
                //         _ => null
                //     };
                // }
                var listTokens = new List<SyntaxNodeOrToken>();
                for (var i = 0; i < properties.Count; i++)
                {
                    var prop = properties[i];
                    listTokens.Add(
                        SwitchExpressionArm(
                            ConstantPattern(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(i))),
                            IdentifierName(prop.Name)));
                    listTokens.Add(Token(SyntaxKind.CommaToken));
                }

                listTokens.Add(SwitchExpressionArm(
                    DiscardPattern(),
                    LiteralExpression(
                        SyntaxKind.NullLiteralExpression)));

                getChildrenImplDecl = getChildrenImplDecl.WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(
                                SwitchExpression(
                                        IdentifierName("index"))
                                    .WithArms(
                                        SeparatedList<SwitchExpressionArmSyntax>(listTokens))))));
            }

            var childrenCountDecl = PropertyDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.IntKeyword)),
                    Identifier("ChildrenCount"))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        }))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(properties.Count))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));


            //public override void Accept(ScriptVisitor visitor) => visitor.Visit(this);
            var acceptMethod = MethodDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.VoidKeyword)),
                    Identifier("Accept"))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        }))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("visitor"))
                                .WithType(
                                    IdentifierName("ScriptVisitor")))))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("visitor"),
                                    IdentifierName("Visit")))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            ThisExpression()))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));


            //public override TResult Accept<TResult>(ScriptVisitor<TResult> visitor) => visitor.Visit(this);
            var acceptGenericMethod = MethodDeclaration(
                    IdentifierName("TResult"),
                    Identifier("Accept"))
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.OverrideKeyword)
                        }))
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList<TypeParameterSyntax>(
                            TypeParameter(
                                Identifier("TResult")))))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("visitor"))
                                .WithType(
                                    GenericName(
                                            Identifier("ScriptVisitor"))
                                        .WithTypeArgumentList(
                                            TypeArgumentList(
                                                SingletonSeparatedList<TypeSyntax>(
                                                    IdentifierName("TResult"))))))))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("visitor"),
                                    IdentifierName("Visit")))
                            .WithArgumentList(
                                ArgumentList(
                                    SingletonSeparatedList<ArgumentSyntax>(
                                        Argument(
                                            ThisExpression()))))))
                .WithSemicolonToken(
                    Token(SyntaxKind.SemicolonToken));

            return ClassDeclaration(typeSymbol.Name)
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.PartialKeyword)
                        })).WithMembers(
                    List(new MemberDeclarationSyntax[]
                    {
                        childrenCountDecl,
                        getChildrenImplDecl,
                        acceptMethod,
                        acceptGenericMethod
                    }));
        }


        private static List<MemberDeclarationSyntax> GenerateVisitors(List<INamedTypeSymbol> scriptNodeTypes)
        {
            var members = new List<MemberDeclarationSyntax>();
            foreach (var scriptNodeType in scriptNodeTypes)
            {
                // public virtual void Visit(ScriptTableRowStatement node) => DefaultVisit(node);
                var method = MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier("Visit"))
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.VirtualKeyword)
                            }))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList<ParameterSyntax>(
                                Parameter(
                                        Identifier("node"))
                                    .WithType(
                                        IdentifierName(scriptNodeType.Name)))))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            InvocationExpression(
                                    IdentifierName("DefaultVisit"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                IdentifierName("node")))))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));
                members.Add(method);
            }


            var scriptVisitor = ClassDeclaration("ScriptVisitor")
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.AbstractKeyword),
                            Token(SyntaxKind.PartialKeyword)
                        })).WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));


            members.Clear();
            foreach (var scriptNodeType in scriptNodeTypes)
            {
                // public virtual void Visit(ScriptTableRowStatement node) => DefaultVisit(node);
                var method = MethodDeclaration(
                        IdentifierName("TResult"),
                        Identifier("Visit"))
                    .WithModifiers(
                        TokenList(
                            new[]
                            {
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.VirtualKeyword)
                            }))
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList<ParameterSyntax>(
                                Parameter(
                                        Identifier("node"))
                                    .WithType(
                                        IdentifierName(scriptNodeType.Name)))))
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            InvocationExpression(
                                    IdentifierName("DefaultVisit"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(
                                                IdentifierName("node")))))))
                    .WithSemicolonToken(
                        Token(SyntaxKind.SemicolonToken));
                members.Add(method);
            }

            var scriptVisitorTResult = ClassDeclaration("ScriptVisitor")
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.AbstractKeyword),
                            Token(SyntaxKind.PartialKeyword)
                        }))
                .WithTypeParameterList(
                    TypeParameterList(
                        SingletonSeparatedList<TypeParameterSyntax>(
                            TypeParameter(
                                Identifier("TResult"))))).WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));


            members.Clear();
            members.Add(scriptVisitor);
            members.Add(scriptVisitorTResult);

            return members;
        }


        private static List<IPropertySymbol> GetScriptProperties(INamedTypeSymbol typeSymbol, bool includeNonScriptNode)
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
                    (IsScriptNode(prop.Type)
                    || includeNonScriptNode)
                    ).ToList();
                properties.AddRange(localProperties);
            }

            return properties;
        }

        private static List<MemberDeclarationSyntax> GenerateRewriters(List<INamedTypeSymbol> scriptNodeTypes)
        {
            var members = new List<MemberDeclarationSyntax>();
            foreach (var scriptNodeType in scriptNodeTypes)
            {
                var properties = GetScriptProperties(scriptNodeType, true);
                var statements = new List<StatementSyntax>();
                var assignExpressions = new List<SyntaxNodeOrToken>();

                switch (scriptNodeType.Name)
                {
                    case "ScriptVariableGlobal":
                    case "ScriptVariableLocal":
                    case "ScriptVariableLoop":
                        continue;
                }

                foreach (var prop in properties)
                {
                    if (assignExpressions.Count > 0)
                    {
                        assignExpressions.Add(Token(SyntaxKind.CommaToken));
                    }

                    if (!IsScriptNode(prop.Type))
                    {
                        // XXX = node.XXX
                        assignExpressions.Add(AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(prop.Name),
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("node"),
                                IdentifierName(prop.Name)))
                        );
                        continue;
                    }

                    LocalDeclarationStatementSyntax localVar;

                    // XXX = newXXX
                    assignExpressions.Add(AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(prop.Name),
                        IdentifierName($"new{prop.Name}"))
                    );

                    if (IsScriptList(prop.Type))
                    {
                        // var newXXX = VisitAll(node.XXX);
                        localVar = LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        VariableDeclarator(
                                                Identifier($"new{prop.Name}"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    InvocationExpression(
                                                            IdentifierName("VisitAll"))
                                                        .WithArgumentList(
                                                            ArgumentList(
                                                                SingletonSeparatedList<ArgumentSyntax>(
                                                                    Argument(
                                                                        MemberAccessExpression(
                                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                                            IdentifierName("node"),
                                                                            IdentifierName(prop.Name)))))))))));
                    }
                    else
                    {
                        // var newXXX = (XXXType) Visit((ScriptNode)node.XXX);
                        localVar = LocalDeclarationStatement(
                            VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                                        VariableDeclarator(
                                                Identifier($"new{prop.Name}"))
                                            .WithInitializer(
                                                EqualsValueClause(
                                                    CastExpression(
                                                        IdentifierName(prop.Type.Name),
                                                        InvocationExpression(
                                                                IdentifierName("Visit"))
                                                            .WithArgumentList(
                                                                ArgumentList(
                                                                    SingletonSeparatedList<ArgumentSyntax>(
                                                                        Argument(
                                                                            CastExpression(
                                                                                IdentifierName(_scriptNodeType.Name),
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                                    IdentifierName("node"),
                                                                                    IdentifierName(prop.Name)))))))))))));

                    }
                    statements.Add(localVar);
                }

                statements.Add(ReturnStatement(ObjectCreationExpression(
                        IdentifierName(scriptNodeType.Name))
                    .WithArgumentList(
                        ArgumentList())
                    .WithInitializer(
                        InitializerExpression(
                            SyntaxKind.ObjectInitializerExpression,
                            SeparatedList<ExpressionSyntax>(assignExpressions))))
                );

                members.Add(
                    MethodDeclaration(
                            IdentifierName("ScriptNode"),
                            Identifier("Visit"))
                        .WithModifiers(
                            TokenList(
                                new[]
                                {
                                    Token(SyntaxKind.PublicKeyword),
                                    Token(SyntaxKind.OverrideKeyword)
                                }))
                        .WithParameterList(
                            ParameterList(
                                SingletonSeparatedList<ParameterSyntax>(
                                    Parameter(
                                            Identifier("node"))
                                        .WithType(
                                            IdentifierName(scriptNodeType.Name)))))
                        .WithBody(
                            Block(statements))
                );
            }

            var scriptRewriterType = ClassDeclaration("ScriptRewriter")
                .WithModifiers(
                    TokenList(
                        new[]
                        {
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.AbstractKeyword),
                            Token(SyntaxKind.PartialKeyword)
                        }))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(members));

            members.Clear();
            members.Add(scriptRewriterType);
            return members;
        }

        private static SyntaxNode GetDeclaredSyntax(ISymbol symbol, Solution solution)
        {
            var typeRefDeclList = symbol.DeclaringSyntaxReferences.Where(x => !solution.GetDocument(x.SyntaxTree).FilePath.Contains(".generated")).ToList();
            if (typeRefDeclList.Count != 1) throw new InvalidOperationException($"Invalid number {typeRefDeclList.Count} of syntax references for {symbol.MetadataName}. Expecting only 1.");
            return typeRefDeclList[0].GetSyntax();
        }

        private static Solution ModifyProperties(INamedTypeSymbol typeSymbol, Solution solution)
        {
            var typeSyntax = (TypeDeclarationSyntax)GetDeclaredSyntax(typeSymbol, solution);


            var properties = typeSymbol.GetMembers().OfType<IPropertySymbol>().Where(x => IsScriptNode(x.Type)).ToList();

            Console.WriteLine($"{typeSymbol.MetadataName} {typeSyntax.Identifier.Text}");
            foreach (var prop in properties)
            {
                var propertyDecls = (PropertyDeclarationSyntax)GetDeclaredSyntax(prop, solution);
                Console.WriteLine($"  => {propertyDecls.Type} {propertyDecls.Identifier.Text}");
            }

            //var typeDecl = (TypeDeclarationSyntax)typeSymbol.DeclaringSyntaxReferences[0].GetSyntax();

            //// TODO: Doesn't support nested classes
            //if (typeDecl.Modifiers.All(x => x.Text != "partial"))
            //{
            //    var rootSyntax = typeDecl.SyntaxTree.GetRoot();
            //    var originalDoc = solution.GetDocument(rootSyntax.SyntaxTree);

            //    var previousDecl = typeDecl;
            //    typeDecl = typeDecl.WithModifiers(typeDecl.Modifiers.Add(Token(SyntaxKind.PartialKeyword).WithTrailingTrivia(Space)));

            //    rootSyntax = rootSyntax.ReplaceNode(previousDecl, typeDecl);

            //    originalDoc = originalDoc.WithSyntaxRoot(rootSyntax);
            //    solution = originalDoc.Project.Solution;
            //}

            return solution;
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
