// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    /// <summary>
    /// Represents fully qualified metadata name of a symbol.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct MetadataName : IEquatable<MetadataName>
    {
        internal static int PlusHashCode = GetHashCode("+");

        internal static int DotHashCode = GetHashCode(".");

        /// <summary>
        /// Initializes a new instance of <see cref="MetadataName"/>.
        /// </summary>
        /// <param name="containingNamespaces"></param>
        /// <param name="name"></param>
        public MetadataName(IEnumerable<string> containingNamespaces, string name)
            : this(containingNamespaces, Array.Empty<string>(), name)
        {
            if (containingNamespaces == null)
                throw new ArgumentNullException(nameof(containingNamespaces));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            ContainingTypes = ImmutableArray<string>.Empty;
            ContainingNamespaces = containingNamespaces.ToImmutableArray();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MetadataName"/>.
        /// </summary>
        /// <param name="containingNamespaces"></param>
        /// <param name="containingTypes"></param>
        /// <param name="name"></param>
        public MetadataName(IEnumerable<string> containingNamespaces, IEnumerable<string> containingTypes, string name)
        {
            if (containingNamespaces == null)
                throw new ArgumentNullException(nameof(containingNamespaces));

            if (containingTypes == null)
                throw new ArgumentNullException(nameof(containingTypes));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            ContainingTypes = containingTypes.ToImmutableArray();
            ContainingNamespaces = containingNamespaces.ToImmutableArray();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MetadataName"/>.
        /// </summary>
        /// <param name="containingNamespaces"></param>
        /// <param name="name"></param>
        public MetadataName(ImmutableArray<string> containingNamespaces, string name)
            : this(containingNamespaces, ImmutableArray<string>.Empty, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MetadataName"/>.
        /// </summary>
        /// <param name="containingNamespaces"></param>
        /// <param name="containingTypes"></param>
        /// <param name="name"></param>
        public MetadataName(ImmutableArray<string> containingNamespaces, ImmutableArray<string> containingTypes, string name)
        {
            if (containingNamespaces.IsDefault)
                throw new ArgumentException("Containing namespaces are not initialized.", nameof(containingNamespaces));

            if (containingTypes.IsDefault)
                throw new ArgumentException("Containing types are not initialized.", nameof(containingTypes));

            Name = name ?? throw new ArgumentNullException(nameof(name));
            ContainingTypes = containingTypes;
            ContainingNamespaces = containingNamespaces;
        }

        /// <summary>
        /// Gets metadata names of containing namespaces
        /// </summary>
        public ImmutableArray<string> ContainingNamespaces { get; }

        /// <summary>
        /// Get metadata names of containing types.
        /// </summary>
        public ImmutableArray<string> ContainingTypes { get; }

        /// <summary>
        /// Get metadata name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Determines whether this struct was initialized with an actual names.
        /// </summary>
        public bool IsDefault
        {
            get { return Name == null; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Returns the fully qualified metadata name.
        /// </summary>
        public override string ToString()
        {
            return ToString(SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        }

        internal string ToString(SymbolDisplayTypeQualificationStyle typeQualificationStyle)
        {
            if (IsDefault)
                return "";

            switch (typeQualificationStyle)
            {
                case SymbolDisplayTypeQualificationStyle.NameOnly:
                {
                    return Name;
                }
                case SymbolDisplayTypeQualificationStyle.NameAndContainingTypes:
                {
                    if (ContainingTypes.Any())
                        return string.Join("+", ContainingTypes) + "+" + Name;

                    return Name;
                }
                case SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces:
                {
                    if (ContainingNamespaces.Any())
                    {
                        string @namespace = string.Join(".", ContainingNamespaces);

                        if (ContainingTypes.Any())
                        {
                            return @namespace + "." + string.Join("+", ContainingTypes) + "+" + Name;
                        }
                        else
                        {
                            return @namespace + "." + Name;
                        }
                    }
                    else if (ContainingTypes.Any())
                    {
                        return string.Join("+", ContainingTypes) + "+" + Name;
                    }

                    return Name;
                }
            }

            throw new ArgumentException($"Unknown enum value '{typeQualificationStyle}'.", nameof(typeQualificationStyle));
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false. </returns>
        public override bool Equals(object obj)
        {
            return obj is MetadataName other
                && Equals(other);
        }

        internal bool Equals(ISymbol symbol)
        {
            if (symbol == null)
                return false;

            if (!string.Equals(Name, symbol.MetadataName, StringComparison.Ordinal))
                return false;

            INamedTypeSymbol containingType = symbol.ContainingType;

            for (int i = ContainingTypes.Length - 1; i >= 0; i--)
            {
                if (containingType == null)
                    return false;

                if (!string.Equals(containingType.MetadataName, ContainingTypes[i], StringComparison.Ordinal))
                    return false;

                containingType = containingType.ContainingType;
            }

            if (containingType != null)
                return false;

            INamespaceSymbol containingNamespace = symbol.ContainingNamespace;

            for (int i = ContainingNamespaces.Length - 1; i >= 0; i--)
            {
                if (containingNamespace?.IsGlobalNamespace != false)
                    return false;

                if (!string.Equals(containingNamespace.Name, ContainingNamespaces[i], StringComparison.Ordinal))
                    return false;

                containingNamespace = containingNamespace.ContainingNamespace;
            }

            return containingNamespace?.IsGlobalNamespace == true;
        }

        /// <summary>
        /// Indicates whether this instance and a specified <see cref="MetadataName"/> are equal.
        /// </summary>
        /// <param name="other"></param>
        public bool Equals(MetadataName other)
        {
            if (IsDefault)
                return other.IsDefault;

            if (other.IsDefault)
                return false;

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
                return false;

            if (!ContainingTypes.SequenceEqual(other.ContainingTypes, StringComparer.Ordinal))
                return false;

            if (!ContainingNamespaces.SequenceEqual(other.ContainingNamespaces, StringComparer.Ordinal))
                return false;

            return true;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            if (IsDefault)
                return 0;

            int hashCode = GetHashCode(Name);

            ImmutableArray<string>.Enumerator en = ContainingTypes.GetEnumerator();

            if (en.MoveNext())
            {
                while (true)
                {
                    hashCode = Combine(en.Current);

                    if (en.MoveNext())
                    {
                        hashCode = HashCode.Combine(PlusHashCode, hashCode);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            en = ContainingNamespaces.GetEnumerator();

            if (en.MoveNext())
            {
                while (true)
                {
                    hashCode = Combine(en.Current);

                    if (en.MoveNext())
                    {
                        hashCode = HashCode.Combine(DotHashCode, hashCode);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return hashCode;

            int Combine(string name)
            {
                return HashCode.Combine(GetHashCode(name), hashCode);
            }
        }

        internal static int GetHashCode(string name)
        {
            return StringComparer.Ordinal.GetHashCode(name);
        }

        /// <summary>
        /// Converts the string representation of a fully qualified metadata name to its <see cref="MetadataName"/> equivalent.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or invalid.</exception>
        public static MetadataName Parse(string name)
        {
            return Parse(name, shouldThrow: true);
        }

        /// <summary>
        /// Converts the string representation of a fully qualified metadata name to its <see cref="MetadataName"/> equivalent.
        /// A return value indicates whether the parsing succeeded.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="metadataName"></param>
        public static bool TryParse(string name, out MetadataName metadataName)
        {
            metadataName = Parse(name, shouldThrow: false);

            return !metadataName.IsDefault;
        }

        private static MetadataName Parse(string name, bool shouldThrow)
        {
            if (name == null)
            {
                if (shouldThrow)
                    throw new ArgumentNullException(nameof(name));

                return default;
            }

            int length = name.Length;

            if (length == 0)
            {
                if (shouldThrow)
                    throw new ArgumentException("Name cannot be empty.", nameof(name));

                return default;
            }

            string containingType = null;

            int prevIndex = 0;

            int containingNamespaceCount = 0;
            int containingTypeCount = 0;

            for (int i = 0; i < length; i++)
            {
                if (name[i] == '.')
                {
                    if (containingTypeCount > 0
                        || i == prevIndex
                        || i == length - 1)
                    {
                        if (shouldThrow)
                            throw new ArgumentException("Name is invalid.", nameof(name));

                        return default;
                    }

                    containingNamespaceCount++;

                    prevIndex = i + 1;
                }

                if (name[i] == '+')
                {
                    if (i == prevIndex
                        || i == length - 1)
                    {
                        if (shouldThrow)
                            throw new ArgumentException("Name is invalid.", nameof(name));

                        return default;
                    }

                    containingTypeCount++;

                    prevIndex = i + 1;
                }
            }

            ImmutableArray<string>.Builder containingNamespaces = (containingNamespaceCount > 0)
                ? ImmutableArray.CreateBuilder<string>(containingNamespaceCount)
                : null;

            ImmutableArray<string>.Builder containingTypes = (containingTypeCount > 1)
                ? ImmutableArray.CreateBuilder<string>(containingTypeCount)
                : null;

            prevIndex = 0;

            for (int i = 0; i < length; i++)
            {
                if (name[i] == '.')
                {
                    string n = name.Substring(prevIndex, i - prevIndex);

                    containingNamespaces.Add(n);

                    prevIndex = i + 1;
                }
                else if (name[i] == '+')
                {
                    string n = name.Substring(prevIndex, i - prevIndex);

                    if (containingTypes != null)
                    {
                        containingTypes.Add(n);
                    }
                    else
                    {
                        containingType = n;
                    }

                    prevIndex = i + 1;
                }
            }

            return new MetadataName(
                containingNamespaces?.MoveToImmutable() ?? ImmutableArray<string>.Empty,
                (containingType != null)
                    ? ImmutableArray.Create(containingType)
                    : containingTypes?.MoveToImmutable() ?? ImmutableArray<string>.Empty,
                name.Substring(prevIndex, length - prevIndex));
        }

        public static bool operator ==(in MetadataName metadataName1, in MetadataName metadataName2)
        {
            return metadataName1.Equals(metadataName2);
        }

        public static bool operator !=(in MetadataName metadataName1, in MetadataName metadataName2)
        {
            return !(metadataName1 == metadataName2);
        }
    }
}
