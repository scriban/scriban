// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    /// <summary>
    /// A set of extension methods for <see cref="ISymbol"/> and its derived types.
    /// </summary>
    public static class SymbolExtensions
    {
        #region ISymbol
        internal static IEnumerable<ISymbol> FindImplementedInterfaceMembers(this ISymbol symbol, bool allInterfaces = false)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            return Iterator();

            IEnumerable<ISymbol> Iterator()
            {
                INamedTypeSymbol containingType = symbol.ContainingType;

                if (containingType != null)
                {
                    ImmutableArray<INamedTypeSymbol> interfaces = containingType.GetInterfaces(allInterfaces);

                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        ImmutableArray<ISymbol> members = interfaces[i].GetMembers();

                        for (int j = 0; j < members.Length; j++)
                        {
                            if (symbol.Equals(containingType.FindImplementationForInterfaceMember(members[j])))
                            {
                                yield return members[j];
                            }
                        }
                    }
                }
            }
        }

        internal static ISymbol FindFirstImplementedInterfaceMember(this ISymbol symbol, bool allInterfaces = false)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            return FindFirstImplementedInterfaceMemberImpl(symbol, null, allInterfaces);
        }

        internal static ISymbol FindImplementedInterfaceMember(this ISymbol symbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces = false)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (interfaceSymbol == null)
                throw new ArgumentNullException(nameof(interfaceSymbol));

            return FindFirstImplementedInterfaceMemberImpl(symbol, interfaceSymbol, allInterfaces);
        }

        private static ISymbol FindFirstImplementedInterfaceMemberImpl(this ISymbol symbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces)
        {
            INamedTypeSymbol containingType = symbol.ContainingType;

            if (containingType != null)
            {
                ImmutableArray<INamedTypeSymbol> interfaces = containingType.GetInterfaces(allInterfaces);

                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaceSymbol == null
                        || interfaces[i].Equals(interfaceSymbol))
                    {
                        ImmutableArray<ISymbol> members = interfaces[i].GetMembers();

                        for (int j = 0; j < members.Length; j++)
                        {
                            if (symbol.Equals(containingType.FindImplementationForInterfaceMember(members[j])))
                                return members[j];
                        }
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Returns true if the symbol implements any interface member.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool ImplementsInterfaceMember(this ISymbol symbol, bool allInterfaces = false)
        {
            return FindFirstImplementedInterfaceMember(symbol, allInterfaces) != null;
        }

        /// <summary>
        /// Returns true if the symbol implements any member of the specified interface.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="interfaceSymbol"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool ImplementsInterfaceMember(this ISymbol symbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces = false)
        {
            return FindImplementedInterfaceMember(symbol, interfaceSymbol, allInterfaces) != null;
        }

        internal static TSymbol FindFirstImplementedInterfaceMember<TSymbol>(this ISymbol symbol, bool allInterfaces = false) where TSymbol : ISymbol
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            return FindFirstImplementedInterfaceMemberImpl<TSymbol>(symbol, null, allInterfaces);
        }

        internal static TSymbol FindFirstImplementedInterfaceMember<TSymbol>(this ISymbol symbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces = false) where TSymbol : ISymbol
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (interfaceSymbol == null)
                throw new ArgumentNullException(nameof(interfaceSymbol));

            return FindFirstImplementedInterfaceMemberImpl<TSymbol>(symbol, interfaceSymbol, allInterfaces);
        }

        private static TSymbol FindFirstImplementedInterfaceMemberImpl<TSymbol>(this ISymbol symbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces = false) where TSymbol : ISymbol
        {
            INamedTypeSymbol containingType = symbol.ContainingType;

            if (containingType != null)
            {
                ImmutableArray<INamedTypeSymbol> interfaces = containingType.GetInterfaces(allInterfaces);

                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaceSymbol == null
                        || interfaces[i].Equals(interfaceSymbol))
                    {
                        ImmutableArray<ISymbol> members = interfaces[i].GetMembers();

                        for (int j = 0; j < members.Length; j++)
                        {
                            if ((members[j] is TSymbol tmember)
                                && symbol.Equals(containingType.FindImplementationForInterfaceMember(tmember)))
                            {
                                return tmember;
                            }
                        }
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// Returns true if the symbol implements any interface member.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="symbol"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool ImplementsInterfaceMember<TSymbol>(this ISymbol symbol, bool allInterfaces = false) where TSymbol : ISymbol
        {
            return !EqualityComparer<TSymbol>.Default.Equals(
                FindFirstImplementedInterfaceMember<TSymbol>(symbol, allInterfaces),
                default(TSymbol));
        }

        /// <summary>
        /// Returns true if the symbol implements any member of the specified interface.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="symbol"></param>
        /// <param name="interfaceSymbol"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool ImplementsInterfaceMember<TSymbol>(this ISymbol symbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces = false) where TSymbol : ISymbol
        {
            return !EqualityComparer<TSymbol>.Default.Equals(
                FindFirstImplementedInterfaceMember<TSymbol>(symbol, interfaceSymbol, allInterfaces),
                default(TSymbol));
        }

        /// <summary>
        /// Returns true if the symbol is the specified kind.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="kind"></param>
        public static bool IsKind(this ISymbol symbol, SymbolKind kind)
        {
            return symbol?.Kind == kind;
        }

        /// <summary>
        /// Returns true if the symbol is one of the specified kinds.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="kind1"></param>
        /// <param name="kind2"></param>
        public static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2)
        {
            if (symbol == null)
                return false;

            SymbolKind kind = symbol.Kind;

            return kind == kind1
                || kind == kind2;
        }

        /// <summary>
        /// Returns true if the symbol is one of the specified kinds.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="kind1"></param>
        /// <param name="kind2"></param>
        /// <param name="kind3"></param>
        public static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2, SymbolKind kind3)
        {
            if (symbol == null)
                return false;

            SymbolKind kind = symbol.Kind;

            return kind == kind1
                || kind == kind2
                || kind == kind3;
        }

        /// <summary>
        /// Returns true if the symbol is one of the specified kinds.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="kind1"></param>
        /// <param name="kind2"></param>
        /// <param name="kind3"></param>
        /// <param name="kind4"></param>
        public static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2, SymbolKind kind3, SymbolKind kind4)
        {
            if (symbol == null)
                return false;

            SymbolKind kind = symbol.Kind;

            return kind == kind1
                || kind == kind2
                || kind == kind3
                || kind == kind4;
        }

        /// <summary>
        /// Returns true if the symbol is one of the specified kinds.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="kind1"></param>
        /// <param name="kind2"></param>
        /// <param name="kind3"></param>
        /// <param name="kind4"></param>
        /// <param name="kind5"></param>
        public static bool IsKind(this ISymbol symbol, SymbolKind kind1, SymbolKind kind2, SymbolKind kind3, SymbolKind kind4, SymbolKind kind5)
        {
            if (symbol == null)
                return false;

            SymbolKind kind = symbol.Kind;

            return kind == kind1
                || kind == kind2
                || kind == kind3
                || kind == kind4
                || kind == kind5;
        }

        /// <summary>
        /// Returns true if the symbol represents an error.
        /// </summary>
        /// <param name="symbol"></param>
        public static bool IsErrorType(this ISymbol symbol)
        {
            return symbol?.Kind == SymbolKind.ErrorType;
        }

        /// <summary>
        /// Returns true if the symbol is an async method.
        /// </summary>
        /// <param name="symbol"></param>
        public static bool IsAsyncMethod(this ISymbol symbol)
        {
            return symbol?.Kind == SymbolKind.Method
                && ((IMethodSymbol)symbol).IsAsync;
        }

        internal static bool IsPropertyOfAnonymousType(this ISymbol symbol)
        {
            return symbol?.Kind == SymbolKind.Property
                && symbol.ContainingType.IsAnonymousType;
        }

        internal static SyntaxNode GetSyntax(this ISymbol symbol, CancellationToken cancellationToken = default)
        {
            return symbol
                .DeclaringSyntaxReferences[0]
                .GetSyntax(cancellationToken);
        }

        internal static Task<SyntaxNode> GetSyntaxAsync(this ISymbol symbol, CancellationToken cancellationToken = default)
        {
            return symbol
                .DeclaringSyntaxReferences[0]
                .GetSyntaxAsync(cancellationToken);
        }

        internal static SyntaxNode GetSyntaxOrDefault(this ISymbol symbol, CancellationToken cancellationToken = default)
        {
            return symbol
                .DeclaringSyntaxReferences
                .FirstOrDefault()?
                .GetSyntax(cancellationToken);
        }

        /// <summary>
        /// Returns the attribute for the symbol that matches the specified attribute class, or null if the symbol does not have the specified attribute.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="attributeClass"></param>
        public static AttributeData GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeClass)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (attributeClass != null)
            {
                ImmutableArray<AttributeData> attributes = symbol.GetAttributes();

                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i].AttributeClass.Equals(attributeClass))
                        return attributes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the attribute for the symbol that matches the specified name, or null if the symbol does not have the specified attribute.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="attributeName"></param>
        public static AttributeData GetAttribute(this ISymbol symbol, in MetadataName attributeName)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            foreach (AttributeData attributeData in symbol.GetAttributes())
            {
                if (attributeData.AttributeClass.HasMetadataName(attributeName))
                    return attributeData;
            }

            return null;
        }

        /// <summary>
        /// Returns true if the symbol has the specified attribute.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="attributeClass"></param>
        public static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeClass)
        {
            return GetAttribute(symbol, attributeClass) != null;
        }

        /// <summary>
        /// Returns true if the type symbol has the specified attribute.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="attributeClass"></param>
        /// <param name="includeBaseTypes"></param>
        public static bool HasAttribute(this ITypeSymbol typeSymbol, INamedTypeSymbol attributeClass, bool includeBaseTypes)
        {
            if (!includeBaseTypes)
                return HasAttribute(typeSymbol, attributeClass);

            ITypeSymbol t = typeSymbol;

            do
            {
                if (t.HasAttribute(attributeClass))
                    return true;

                t = t.BaseType;

            } while (t != null
                && t.SpecialType != SpecialType.System_Object);

            return false;
        }

        /// <summary>
        /// Returns true if the symbol has attribute with the specified name.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="attributeName"></param>
        public static bool HasAttribute(this ISymbol symbol, in MetadataName attributeName)
        {
            return GetAttribute(symbol, attributeName) != null;
        }

        /// <summary>
        /// Returns true if the type symbol has attribute with the specified name.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="attributeName"></param>
        /// <param name="includeBaseTypes"></param>
        public static bool HasAttribute(this ITypeSymbol typeSymbol, in MetadataName attributeName, bool includeBaseTypes)
        {
            if (!includeBaseTypes)
                return HasAttribute(typeSymbol, attributeName);

            ITypeSymbol t = typeSymbol;

            do
            {
                if (t.HasAttribute(attributeName))
                    return true;

                t = t.BaseType;

            } while (t != null
                && t.SpecialType != SpecialType.System_Object);

            return false;
        }

        internal static ImmutableArray<IParameterSymbol> ParametersOrDefault(this ISymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).Parameters;
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).Parameters;
                default:
                    return default;
            }
        }

        internal static ISymbol BaseOverriddenSymbol(this ISymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).BaseOverriddenMethod();
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).BaseOverriddenProperty();
                case SymbolKind.Event:
                    return ((IEventSymbol)symbol).BaseOverriddenEvent();
            }

            return null;
        }

        internal static bool IsName(this ISymbol symbol, string name)
        {
            return StringUtility.Equals(symbol.Name, name);
        }

        internal static bool IsName(this ISymbol symbol, string name1, string name2)
        {
            return StringUtility.Equals(symbol.Name, name1, name2);
        }

        internal static bool IsContainingType(this ISymbol symbol, SpecialType specialType)
        {
            return symbol?.ContainingType?.SpecialType == specialType;
        }

        /// <summary>
        /// Return true if the specified symbol is publicly visible.
        /// </summary>
        /// <param name="symbol"></param>
        public static bool IsPubliclyVisible(this ISymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            do
            {
                if (!symbol.DeclaredAccessibility.Is(
                    Accessibility.Public,
                    Accessibility.Protected,
                    Accessibility.ProtectedOrInternal))
                {
                    return false;
                }

                symbol = symbol.ContainingType;

            } while (symbol != null);

            return true;
        }

        internal static bool IsPubliclyOrInternallyVisible(this ISymbol symbol)
        {
            do
            {
                if (symbol.DeclaredAccessibility.Is(
                    Accessibility.NotApplicable,
                    Accessibility.Private))
                {
                    return false;
                }

                symbol = symbol.ContainingType;

            } while (symbol != null);

            return true;
        }

        internal static Visibility GetVisibility(this ISymbol symbol)
        {
            var visibility = Visibility.Public;

            do
            {
                switch (symbol.DeclaredAccessibility)
                {
                    case Accessibility.Public:
                    case Accessibility.Protected:
                    case Accessibility.ProtectedOrInternal:
                    {
                        break;
                    }
                    case Accessibility.Internal:
                    case Accessibility.ProtectedAndInternal:
                    {
                        if (visibility == Visibility.Public)
                            visibility = Visibility.Internal;

                        break;
                    }
                    case Accessibility.Private:
                    {
                        visibility = Visibility.Private;
                        break;
                    }
                    case Accessibility.NotApplicable:
                    {
                        return Visibility.NotApplicable;
                    }
                    default:
                    {
                        throw new InvalidOperationException($"Unknown accessibility '{symbol.DeclaredAccessibility}'.");
                    }
                }

                symbol = symbol.ContainingType;

            } while (symbol != null);

            return visibility;
        }

        internal static bool IsVisible(this ISymbol symbol, Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Public:
                    return IsPubliclyVisible(symbol);
                case Visibility.Internal:
                    return IsPubliclyOrInternallyVisible(symbol);
                case Visibility.Private:
                    return true;
                default:
                    throw new ArgumentException($"Unknown value '{visibility}'.", nameof(visibility));
            }
        }

        internal static bool IsVisible(this ISymbol symbol, VisibilityFilter visibilityFilter)
        {
            switch (symbol.GetVisibility())
            {
                case Visibility.NotApplicable:
                    break;
                case Visibility.Private:
                    return (visibilityFilter & VisibilityFilter.Private) != 0;
                case Visibility.Internal:
                    return (visibilityFilter & VisibilityFilter.Internal) != 0;
                case Visibility.Public:
                    return (visibilityFilter & VisibilityFilter.Public) != 0;
            }

            //Debug.Fail(symbol.ToDisplayString(SymbolDisplayFormats.Test));

            return false;
        }

        /// <summary>
        /// Returns true if a symbol has the specified <see cref="MetadataName"/>.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="metadataName"></param>
        public static bool HasMetadataName(this ISymbol symbol, in MetadataName metadataName)
        {
            return metadataName.Equals(symbol);
        }

        internal static ImmutableArray<IParameterSymbol> GetParameters(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    return ((IMethodSymbol)symbol).Parameters;
                case SymbolKind.NamedType:
                    return ((INamedTypeSymbol)symbol).DelegateInvokeMethod?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;
                case SymbolKind.Property:
                    return ((IPropertySymbol)symbol).Parameters;
            }

            return ImmutableArray<IParameterSymbol>.Empty;
        }

        internal static INamespaceSymbol GetRootNamespace(this ISymbol symbol)
        {
            INamespaceSymbol n = symbol.ContainingNamespace;

            if (n?.IsGlobalNamespace == false)
            {
                while (true)
                {
                    INamespaceSymbol n2 = n.ContainingNamespace;

                    if (n2.IsGlobalNamespace)
                        return n;

                    n = n2;
                }
            }

            return null;
        }
        #endregion ISymbol

        #region IAssemblySymbol
        internal static ImmutableArray<INamedTypeSymbol> GetTypes(this IAssemblySymbol assemblySymbol, Func<INamedTypeSymbol, bool> predicate = null)
        {
            ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

            GetTypes(assemblySymbol.GlobalNamespace);

            return builder.ToImmutableArray();

            void GetTypes(INamespaceOrTypeSymbol namespaceOrTypeSymbol)
            {
                if (namespaceOrTypeSymbol is INamedTypeSymbol namedTypeSymbol
                    && (predicate == null || predicate(namedTypeSymbol)))
                {
                    builder.Add(namedTypeSymbol);
                }

                foreach (ISymbol memberSymbol in namespaceOrTypeSymbol.GetMembers())
                {
                    if (memberSymbol is INamespaceOrTypeSymbol namespaceOrTypeSymbol2)
                    {
                        GetTypes(namespaceOrTypeSymbol2);
                    }
                }
            }
        }

        internal static ImmutableArray<INamespaceSymbol> GetNamespaces(this IAssemblySymbol assemblySymbol, Func<INamespaceSymbol, bool> predicate = null)
        {
            ImmutableArray<INamespaceSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamespaceSymbol>();

            GetNamespaces(assemblySymbol.GlobalNamespace);

            return builder.ToImmutableArray();

            void GetNamespaces(INamespaceSymbol namespaceSymbol)
            {
                if (predicate == null || predicate(namespaceSymbol))
                    builder.Add(namespaceSymbol);

                foreach (INamespaceSymbol namespaceSymbol2 in namespaceSymbol.GetNamespaceMembers())
                    GetNamespaces(namespaceSymbol2);
            }
        }
        #endregion IAssemblySymbol

        #region IEventSymbol
        internal static IEventSymbol BaseOverriddenEvent(this IEventSymbol eventSymbol)
        {
            if (eventSymbol == null)
                throw new ArgumentNullException(nameof(eventSymbol));

            IEventSymbol overriddenEvent = eventSymbol.OverriddenEvent;

            if (overriddenEvent == null)
                return null;

            while (true)
            {
                IEventSymbol symbol = overriddenEvent.OverriddenEvent;

                if (symbol == null)
                    return overriddenEvent;

                overriddenEvent = symbol;
            }
        }
        #endregion IEventSymbol

        #region IFieldSymbol
        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, bool value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is bool value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, char value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is char value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, sbyte value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is sbyte value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, byte value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is byte value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, short value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is short value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, ushort value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is ushort value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, int value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is int value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, uint value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is uint value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, long value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is long value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, ulong value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is ulong value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, decimal value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is decimal value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, float value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is float value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, double value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is double value2
                    && value == value2;
            }

            return false;
        }

        /// <summary>
        /// Get a value indicating whether the field symbol has specified constant value.
        /// </summary>
        /// <param name="fieldSymbol"></param>
        /// <param name="value"></param>
        public static bool HasConstantValue(this IFieldSymbol fieldSymbol, string value)
        {
            if (fieldSymbol == null)
                throw new ArgumentNullException(nameof(fieldSymbol));

            if (fieldSymbol.HasConstantValue)
            {
                object constantValue = fieldSymbol.ConstantValue;

                return constantValue is string value2
                    && value == value2;
            }

            return false;
        }
        #endregion IFieldSymbol

        #region IMethodSymbol
        internal static IMethodSymbol BaseOverriddenMethod(this IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null)
                throw new ArgumentNullException(nameof(methodSymbol));

            IMethodSymbol overriddenMethod = methodSymbol.OverriddenMethod;

            if (overriddenMethod == null)
                return null;

            while (true)
            {
                IMethodSymbol symbol = overriddenMethod.OverriddenMethod;

                if (symbol == null)
                    return overriddenMethod;

                overriddenMethod = symbol;
            }
        }

        /// <summary>
        /// If this method is a reduced extension method, returns the definition of extension method from which this was reduced. Otherwise, returns this symbol.
        /// </summary>
        /// <param name="methodSymbol"></param>
        public static IMethodSymbol ReducedFromOrSelf(this IMethodSymbol methodSymbol)
        {
            return methodSymbol?.ReducedFrom ?? methodSymbol;
        }

        /// <summary>
        /// Returns true if this method is a reduced extension method.
        /// </summary>
        /// <param name="methodSymbol"></param>
        public static bool IsReducedExtensionMethod(this IMethodSymbol methodSymbol)
        {
            return methodSymbol?.MethodKind == MethodKind.ReducedExtension;
        }

        /// <summary>
        /// Returns true if this method is an ordinary extension method (i.e. "this" parameter has not been removed).
        /// </summary>
        /// <param name="methodSymbol"></param>
        public static bool IsOrdinaryExtensionMethod(this IMethodSymbol methodSymbol)
        {
            return methodSymbol?.IsExtensionMethod == true
                && methodSymbol.MethodKind == MethodKind.Ordinary;
        }

        internal static bool IsReturnType(this IMethodSymbol methodSymbol, SpecialType specialType)
        {
            return methodSymbol?.ReturnType.SpecialType == specialType;
        }

        internal static bool HasSingleParameter(this IMethodSymbol methodSymbol, SpecialType parameterType)
        {
            return methodSymbol.Parameters.SingleOrDefault()?.Type.SpecialType == parameterType;
        }

        internal static bool HasTwoParameters(this IMethodSymbol methodSymbol, SpecialType firstParameterType, SpecialType secondParameterType)
        {
            ImmutableArray<IParameterSymbol> parameters = methodSymbol.Parameters;

            return parameters.Length == 2
                && parameters[0].Type.SpecialType == firstParameterType
                && parameters[1].Type.SpecialType == secondParameterType;
        }
        #endregion IMethodSymbol

        #region INamespaceSymbol
        internal static bool IsSystemNamespace(this INamespaceSymbol namespaceSymbol)
        {
            return string.Equals(namespaceSymbol.Name, "System", StringComparison.Ordinal)
                && namespaceSymbol.ContainingNamespace.IsGlobalNamespace;
        }
        #endregion INamespaceSymbol

        #region IParameterSymbol
        /// <summary>
        /// Returns true if the parameter was declared as a parameter array that has a specified element type.
        /// </summary>
        /// <param name="parameterSymbol"></param>
        /// <param name="elementType"></param>
        public static bool IsParameterArrayOf(this IParameterSymbol parameterSymbol, SpecialType elementType)
        {
            return parameterSymbol?.IsParams == true
                && (parameterSymbol.Type as IArrayTypeSymbol)?.ElementType.SpecialType == elementType;
        }

        /// <summary>
        /// Returns true if the parameter was declared as a parameter array that has one of specified element types.
        /// </summary>
        /// <param name="parameterSymbol"></param>
        /// <param name="elementType1"></param>
        /// <param name="elementType2"></param>
        public static bool IsParameterArrayOf(
            this IParameterSymbol parameterSymbol,
            SpecialType elementType1,
            SpecialType elementType2)
        {
            return parameterSymbol?.IsParams == true
                && (parameterSymbol.Type as IArrayTypeSymbol)?
                    .ElementType
                    .SpecialType
                    .Is(elementType1, elementType2) == true;
        }

        /// <summary>
        /// Returns true if the parameter was declared as a parameter array that has one of specified element types.
        /// </summary>
        /// <param name="parameterSymbol"></param>
        /// <param name="elementType1"></param>
        /// <param name="elementType2"></param>
        /// <param name="elementType3"></param>
        public static bool IsParameterArrayOf(
            this IParameterSymbol parameterSymbol,
            SpecialType elementType1,
            SpecialType elementType2,
            SpecialType elementType3)
        {
            return parameterSymbol?.IsParams == true
                && (parameterSymbol.Type as IArrayTypeSymbol)?
                    .ElementType
                    .SpecialType
                    .Is(elementType1, elementType2, elementType3) == true;
        }

        /// <summary>
        /// Returns true if the parameter was declared as "ref" or "out" parameter.
        /// </summary>
        /// <param name="parameterSymbol"></param>
        public static bool IsRefOrOut(this IParameterSymbol parameterSymbol)
        {
            if (parameterSymbol == null)
                throw new ArgumentNullException(nameof(parameterSymbol));

            RefKind refKind = parameterSymbol.RefKind;

            return refKind == RefKind.Ref
                || refKind == RefKind.Out;
        }
        #endregion IParameterSymbol

        #region IPropertySymbol
        internal static IPropertySymbol BaseOverriddenProperty(this IPropertySymbol propertySymbol)
        {
            if (propertySymbol == null)
                throw new ArgumentNullException(nameof(propertySymbol));

            IPropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;

            if (overriddenProperty == null)
                return null;

            while (true)
            {
                IPropertySymbol symbol = overriddenProperty.OverriddenProperty;

                if (symbol == null)
                    return overriddenProperty;

                overriddenProperty = symbol;
            }
        }
        #endregion IPropertySymbol

        #region INamedTypeSymbol
        /// <summary>
        /// Returns true if the type is <see cref="Nullable{T}"/> and it has specified type argument.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <param name="specialType"></param>
        public static bool IsNullableOf(this INamedTypeSymbol namedTypeSymbol, SpecialType specialType)
        {
            return namedTypeSymbol.IsNullableType()
                && namedTypeSymbol.TypeArguments[0].SpecialType == specialType;
        }

        /// <summary>
        /// Returns true if the type is <see cref="Nullable{T}"/> and it has specified type argument.
        /// </summary>
        /// <param name="namedTypeSymbol"></param>
        /// <param name="typeArgument"></param>
        public static bool IsNullableOf(this INamedTypeSymbol namedTypeSymbol, ITypeSymbol typeArgument)
        {
            return namedTypeSymbol.IsNullableType()
                && namedTypeSymbol.TypeArguments[0] == typeArgument;
        }

        /// <summary>
        /// Searches for a member that matches the conditions defined by the specified predicate and returns the first occurrence within the type's members.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="typeSymbol"></param>
        /// <param name="predicate"></param>
        /// <param name="includeBaseTypes"></param>
        public static TSymbol FindMember<TSymbol>(
            this INamedTypeSymbol typeSymbol,
            Func<TSymbol, bool> predicate,
            bool includeBaseTypes = false) where TSymbol : ISymbol
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return FindMemberImpl(typeSymbol, name: null, predicate, includeBaseTypes);
        }

        /// <summary>
        /// Searches for a member that has the specified name and matches the conditions defined by the specified predicate, if any, and returns the first occurrence within the type's members.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="typeSymbol"></param>
        /// <param name="name"></param>
        /// <param name="predicate"></param>
        /// <param name="includeBaseTypes"></param>
        public static TSymbol FindMember<TSymbol>(
            this INamedTypeSymbol typeSymbol,
            string name,
            Func<TSymbol, bool> predicate = null,
            bool includeBaseTypes = false) where TSymbol : ISymbol
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return FindMemberImpl(typeSymbol, name, predicate, includeBaseTypes);
        }

        private static TSymbol FindMemberImpl<TSymbol>(
            this INamedTypeSymbol typeSymbol,
            string name,
            Func<TSymbol, bool> predicate = null,
            bool includeBaseTypes = false) where TSymbol : ISymbol
        {
            ImmutableArray<ISymbol> members;

            do
            {
                members = (name != null)
                    ? typeSymbol.GetMembers(name)
                    : typeSymbol.GetMembers();

                TSymbol symbol = FindMemberImpl(members, predicate);

                if (symbol != null)
                    return symbol;

                if (!includeBaseTypes)
                    break;

                typeSymbol = typeSymbol.BaseType;

            } while (typeSymbol != null);

            return default;
        }

        /// <summary>
        /// Searches for a type member that matches the conditions defined by the specified predicate and returns the first occurrence within the type's members.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="predicate"></param>
        /// <param name="includeBaseTypes"></param>
        public static INamedTypeSymbol FindTypeMember(
            this INamedTypeSymbol typeSymbol,
            Func<INamedTypeSymbol, bool> predicate,
            bool includeBaseTypes = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return FindTypeMemberImpl(typeSymbol, name: null, arity: null, predicate, includeBaseTypes);
        }

        /// <summary>
        /// Searches for a type member that has the specified name and matches the conditions defined by the specified predicate, if any, and returns the first occurrence within the type's members.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="name"></param>
        /// <param name="predicate"></param>
        /// <param name="includeBaseTypes"></param>
        public static INamedTypeSymbol FindTypeMember(
            this INamedTypeSymbol typeSymbol,
            string name,
            Func<INamedTypeSymbol, bool> predicate = null,
            bool includeBaseTypes = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return FindTypeMemberImpl(typeSymbol, name, arity: null, predicate, includeBaseTypes);
        }

        /// <summary>
        /// Searches for a type member that has the specified name, arity and matches the conditions defined by the specified predicate, if any, and returns the first occurrence within the type's members.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="name"></param>
        /// <param name="arity"></param>
        /// <param name="predicate"></param>
        /// <param name="includeBaseTypes"></param>
        public static INamedTypeSymbol FindTypeMember(
            this INamedTypeSymbol typeSymbol,
            string name,
            int arity,
            Func<INamedTypeSymbol, bool> predicate = null,
            bool includeBaseTypes = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return FindTypeMemberImpl(typeSymbol, name, arity, predicate, includeBaseTypes);
        }

        private static INamedTypeSymbol FindTypeMemberImpl(
            this INamedTypeSymbol typeSymbol,
            string name,
            int? arity,
            Func<INamedTypeSymbol, bool> predicate = null,
            bool includeBaseTypes = false)
        {
            ImmutableArray<INamedTypeSymbol> members;

            do
            {
                if (name != null)
                {
                    if (arity != null)
                    {
                        members = typeSymbol.GetTypeMembers(name, arity.Value);
                    }
                    else
                    {
                        members = typeSymbol.GetTypeMembers(name);
                    }
                }
                else
                {
                    members = typeSymbol.GetTypeMembers();
                }

                INamedTypeSymbol symbol = FindMemberImpl(members, predicate);

                if (symbol != null)
                    return symbol;

                if (!includeBaseTypes)
                    break;

                typeSymbol = typeSymbol.BaseType;

            } while (typeSymbol != null);

            return null;
        }
        #endregion INamedTypeSymbol

        #region ITypeSymbol
        /// <summary>
        /// Returns true if the type is <see cref="Nullable{T}"/> and it has specified type argument.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="specialType"></param>
        public static bool IsNullableOf(this ITypeSymbol typeSymbol, SpecialType specialType)
        {
            return (typeSymbol as INamedTypeSymbol)?.IsNullableOf(specialType) == true;
        }

        /// <summary>
        /// Returns true if the type is <see cref="Nullable{T}"/> and it has specified type argument.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="typeArgument"></param>
        public static bool IsNullableOf(this ITypeSymbol typeSymbol, ITypeSymbol typeArgument)
        {
            return (typeSymbol as INamedTypeSymbol)?.IsNullableOf(typeArgument) == true;
        }

        /// <summary>
        /// Returns true if the type is <see cref="Void"/>.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsVoid(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.SpecialType == SpecialType.System_Void;
        }

        /// <summary>
        /// Returns true if the type is <see cref="string"/>.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsString(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.SpecialType == SpecialType.System_String;
        }

        /// <summary>
        /// Returns true if the type is <see cref="object"/>.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsObject(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.SpecialType == SpecialType.System_Object;
        }

        /// <summary>
        /// Gets a list of base types of this type.
        /// </summary>
        /// <param name="type"></param>
        public static IEnumerable<INamedTypeSymbol> BaseTypes(this ITypeSymbol type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return BaseTypesIterator();

            IEnumerable<INamedTypeSymbol> BaseTypesIterator()
            {
                INamedTypeSymbol baseType = type.BaseType;

                while (baseType != null)
                {
                    yield return baseType;
                    baseType = baseType.BaseType;
                }
            }
        }

        /// <summary>
        /// Gets a list of base types of this type (including this type).
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static IEnumerable<ITypeSymbol> BaseTypesAndSelf(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return BaseTypesAndSelfIterator();

            IEnumerable<ITypeSymbol> BaseTypesAndSelfIterator()
            {
                ITypeSymbol current = typeSymbol;

                while (current != null)
                {
                    yield return current;
                    current = current.BaseType;
                }
            }
        }

        /// <summary>
        /// Returns true if the type implements specified interface.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="interfaceType"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool Implements(this ITypeSymbol typeSymbol, SpecialType interfaceType, bool allInterfaces = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            ImmutableArray<INamedTypeSymbol> interfaces = typeSymbol.GetInterfaces(allInterfaces);

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].ConstructedFrom.SpecialType == interfaceType)
                    return true;
            }

            return false;
        }

        internal static bool IsOrImplements(this ITypeSymbol typeSymbol, SpecialType interfaceType, bool allInterfaces = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return typeSymbol.SpecialType == interfaceType
                || typeSymbol.Implements(interfaceType, allInterfaces: allInterfaces);
        }

        /// <summary>
        /// Returns true if the type implements any of specified interfaces.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="interfaceType1"></param>
        /// <param name="interfaceType2"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool ImplementsAny(this ITypeSymbol typeSymbol, SpecialType interfaceType1, SpecialType interfaceType2, bool allInterfaces = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            ImmutableArray<INamedTypeSymbol> interfaces = typeSymbol.GetInterfaces(allInterfaces);

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].ConstructedFrom.SpecialType.Is(interfaceType1, interfaceType2))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type implements any of specified interfaces.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="interfaceType1"></param>
        /// <param name="interfaceType2"></param>
        /// <param name="interfaceType3"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool ImplementsAny(this ITypeSymbol typeSymbol, SpecialType interfaceType1, SpecialType interfaceType2, SpecialType interfaceType3, bool allInterfaces = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            ImmutableArray<INamedTypeSymbol> interfaces = typeSymbol.GetInterfaces(allInterfaces);

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].ConstructedFrom.SpecialType.Is(interfaceType1, interfaceType2, interfaceType3))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type implements specified interface.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="interfaceSymbol"></param>
        /// <param name="allInterfaces">If true, use <see cref="ITypeSymbol.AllInterfaces"/>, otherwise use <see cref="ITypeSymbol.Interfaces"/>.</param>
        public static bool Implements(this ITypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol, bool allInterfaces = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (interfaceSymbol != null)
            {
                ImmutableArray<INamedTypeSymbol> interfaces = typeSymbol.GetInterfaces(allInterfaces);

                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaces[i].Equals(interfaceSymbol))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type implements specified interface name.
        /// </summary>
        /// <param name="typeSymbol"></param>
        /// <param name="interfaceName"></param>
        /// <param name="allInterfaces"></param>
        public static bool Implements(this ITypeSymbol typeSymbol, in MetadataName interfaceName, bool allInterfaces = false)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            foreach (INamedTypeSymbol interfaceSymbol in typeSymbol.GetInterfaces(allInterfaces))
            {
                if (interfaceSymbol.HasMetadataName(interfaceName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type can be declared explicitly in a source code.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool SupportsExplicitDeclaration(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            if (typeSymbol.IsAnonymousType)
                return false;

            switch (typeSymbol.Kind)
            {
                case SymbolKind.TypeParameter:
                case SymbolKind.DynamicType:
                {
                    return true;
                }
                case SymbolKind.ArrayType:
                {
                    return SupportsExplicitDeclaration(((IArrayTypeSymbol)typeSymbol).ElementType);
                }
                case SymbolKind.NamedType:
                {
                    var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;

                    if (typeSymbol.IsTupleType)
                    {
                        foreach (IFieldSymbol tupleElement in namedTypeSymbol.TupleElements)
                        {
                            if (!SupportsExplicitDeclaration(tupleElement.Type))
                                return false;
                        }

                        return true;
                    }

                    return SupportsExplicitDeclaration2(namedTypeSymbol.TypeArguments);
                }
            }

            return false;

            static bool SupportsExplicitDeclaration2(ImmutableArray<ITypeSymbol> typeSymbols)
            {
                foreach (ITypeSymbol symbol in typeSymbols)
                {
                    if (symbol.IsAnonymousType)
                        return false;

                    switch (symbol.Kind)
                    {
                        case SymbolKind.NamedType:
                        {
                            var namedTypeSymbol = (INamedTypeSymbol)symbol;

                            if (!SupportsExplicitDeclaration2(namedTypeSymbol.TypeArguments))
                                return false;

                            break;
                        }
                        case SymbolKind.ErrorType:
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Returns true if the type inherits from a specified base type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <param name="includeInterfaces"></param>
        public static bool InheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (baseType == null)
                return false;

            INamedTypeSymbol t = type.BaseType;

            while (t != null)
            {
                Debug.Assert(t.TypeKind.Is(TypeKind.Class, TypeKind.Error), t.TypeKind.ToString());

                if (t.OriginalDefinition.Equals(baseType))
                    return true;

                t = t.BaseType;
            }

            if (includeInterfaces
                && baseType.TypeKind == TypeKind.Interface)
            {
                foreach (INamedTypeSymbol interfaceType in type.AllInterfaces)
                {
                    if (interfaceType.OriginalDefinition.Equals(baseType))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type inherits from a type with the specified name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseTypeName"></param>
        /// <param name="includeInterfaces"></param>
        public static bool InheritsFrom(this ITypeSymbol type, in MetadataName baseTypeName, bool includeInterfaces = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            INamedTypeSymbol baseType = type.BaseType;

            while (baseType != null)
            {
                if (baseType.HasMetadataName(baseTypeName))
                    return true;

                baseType = baseType.BaseType;
            }

            if (includeInterfaces)
            {
                foreach (INamedTypeSymbol interfaceType in type.AllInterfaces)
                {
                    if (interfaceType.HasMetadataName(baseTypeName))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type is equal or inherits from a specified base type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseType"></param>
        /// <param name="includeInterfaces"></param>
        public static bool EqualsOrInheritsFrom(this ITypeSymbol type, ITypeSymbol baseType, bool includeInterfaces = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.Equals(baseType)
                || InheritsFrom(type, baseType, includeInterfaces);
        }

        /// <summary>
        /// Returns true if the type is equal or inherits from a type wit the specified name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="baseTypeName"></param>
        /// <param name="includeInterfaces"></param>
        public static bool EqualsOrInheritsFrom(this ITypeSymbol type, in MetadataName baseTypeName, bool includeInterfaces = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.HasMetadataName(baseTypeName)
                || InheritsFrom(type, baseTypeName, includeInterfaces);
        }

        /// <summary>
        /// Searches for a member that matches the conditions defined by the specified predicate, if any, and returns the first occurrence within the type's members.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="typeSymbol"></param>
        /// <param name="predicate"></param>
        public static TSymbol FindMember<TSymbol>(this ITypeSymbol typeSymbol, Func<TSymbol, bool> predicate = null) where TSymbol : ISymbol
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return FindMemberImpl(typeSymbol.GetMembers(), predicate);
        }

        /// <summary>
        /// Searches for a member that has the specified name and matches the conditions defined by the specified predicate, if any, and returns the first occurrence within the type's members.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="typeSymbol"></param>
        /// <param name="name"></param>
        /// <param name="predicate"></param>
        public static TSymbol FindMember<TSymbol>(this ITypeSymbol typeSymbol, string name, Func<TSymbol, bool> predicate = null) where TSymbol : ISymbol
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return FindMemberImpl(typeSymbol.GetMembers(name), predicate);
        }

        private static TSymbol FindMemberImpl<TSymbol, TMemberSymbol>(ImmutableArray<TMemberSymbol> members, Func<TSymbol, bool> predicate)
            where TSymbol : ISymbol
            where TMemberSymbol : ISymbol
        {
            if (predicate != null)
            {
                foreach (TMemberSymbol symbol in members)
                {
                    if (symbol is TSymbol tsymbol
                        && predicate(tsymbol))
                    {
                        return tsymbol;
                    }
                }
            }
            else
            {
                foreach (TMemberSymbol symbol in members)
                {
                    if (symbol is TSymbol tsymbol)
                        return tsymbol;
                }
            }

            return default;
        }

        /// <summary>
        /// Returns true if the type contains member that matches the conditions defined by the specified predicate, if any.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="typeSymbol"></param>
        /// <param name="predicate"></param>
        internal static bool ContainsMember<TSymbol>(this ITypeSymbol typeSymbol, Func<TSymbol, bool> predicate = null) where TSymbol : ISymbol
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return FindMember(typeSymbol, predicate) != null;
        }

        /// <summary>
        /// Returns true if the type contains member that has the specified name and matches the conditions defined by the specified predicate, if any.
        /// </summary>
        /// <typeparam name="TSymbol"></typeparam>
        /// <param name="typeSymbol"></param>
        /// <param name="name"></param>
        /// <param name="predicate"></param>
        internal static bool ContainsMember<TSymbol>(this ITypeSymbol typeSymbol, string name, Func<TSymbol, bool> predicate = null) where TSymbol : ISymbol
        {
            if (typeSymbol == null)
                throw new ArgumentNullException(nameof(typeSymbol));

            return FindMember(typeSymbol, name, predicate) != null;
        }

        /// <summary>
        /// Returns true if the type is <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsIEnumerableOfT(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T;
        }

        /// <summary>
        /// Returns true if the type is <see cref="IEnumerable"/> or <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsIEnumerableOrIEnumerableOfT(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?
                .SpecialType
                .Is(SpecialType.System_Collections_IEnumerable, SpecialType.System_Collections_Generic_IEnumerable_T) == true;
        }

        /// <summary>
        /// Returns true if the type is a reference type or a nullable type.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsReferenceTypeOrNullableType(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.IsReferenceType == true
                || typeSymbol.IsNullableType();
        }

        /// <summary>
        /// Returns true if the type is a nullable type.
        /// </summary>
        /// <param name="typeSymbol"></param>
        public static bool IsNullableType(this ITypeSymbol typeSymbol)
        {
            return typeSymbol?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        private static ImmutableArray<INamedTypeSymbol> GetInterfaces(this ITypeSymbol typeSymbol, bool allInterfaces)
        {
            return (allInterfaces) ? typeSymbol.AllInterfaces : typeSymbol.Interfaces;
        }
        #endregion ITypeSymbol
    }
}
