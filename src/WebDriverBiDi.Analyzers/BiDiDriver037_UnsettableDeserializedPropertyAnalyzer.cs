// <copyright file="BiDiDriver037_UnsettableDeserializedPropertyAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a property of a <c>CommandResult</c> or <c>WebDriverBiDiEventArgs</c> type that is
/// mapped to a JSON name but that the deserializer cannot set, because the property or its setter is not
/// public and the property is not marked <c>[JsonInclude]</c>. The remote end's value is discarded in
/// silence and the property keeps its default.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver037_UnsettableDeserializedPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI037";

    private const string Category = "Usage";

    // JsonIgnoreCondition.Always, which is also what [JsonIgnore] means when no condition is named.
    private const int JsonIgnoreConditionAlways = 1;

    private static readonly LocalizableString Title = "Deserialized property the serializer cannot set";

    private static readonly LocalizableString MessageFormat = "'{0}' is mapped to the JSON name '{1}', but {2}, so deserializing {3} leaves it at its default value. Mark it with [JsonInclude].";

    private static readonly LocalizableString Description = "System.Text.Json reads and writes a non-public property, or a public property with a non-public setter, only when it is marked with [JsonInclude]. A command result or event args type is built by deserializing the remote end's response, so such a property without the attribute is never assigned: the value arrives, is matched to nothing, and is silently discarded, leaving the property at its default. The library's own result types carry [JsonPropertyName], [JsonInclude] and an internal setter together for this reason.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi037");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationStart =>
        {
            DeserializedTypeSymbols symbols = new(compilationStart.Compilation);

            // A compilation that references neither base type declares no type this rule judges.
            if (symbols.CommandResult is null && symbols.EventArgs is null)
            {
                return;
            }

            compilationStart.RegisterSymbolAction(symbolContext => AnalyzeProperty(symbolContext, symbols), SymbolKind.Property);
        });
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context, DeserializedTypeSymbols symbols)
    {
        IPropertySymbol property = (IPropertySymbol)context.Symbol;
        if (property.IsStatic || property.IsIndexer)
        {
            return;
        }

        // The name attribute is what marks the property as one the remote end's payload fills. A
        // property without it may be anything, including state the type computes for itself.
        string? jsonName = GetAttribute(property, symbols.PropertyName)?.ConstructorArguments.FirstOrDefault().Value as string;
        if (jsonName is null
            || GetAttribute(property, symbols.Include) is not null
            || IsAlwaysIgnored(GetAttribute(property, symbols.Ignore))
            || !IsDeserializedType(property.ContainingType, symbols))
        {
            return;
        }

        // Only a property the serializer would otherwise assign is reported: [JsonInclude] gives the
        // serializer an existing setter to use, and cannot conjure one.
        if (property.SetMethod is not { } setter)
        {
            return;
        }

        string reason = property.DeclaredAccessibility != Accessibility.Public
            ? "the property is not public"
            : setter.DeclaredAccessibility != Accessibility.Public
                ? "its setter is not public"
                : string.Empty;
        if (reason.Length == 0 || IsSetByConstructor(property, symbols))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, property.Locations[0], property.Name, jsonName, reason, property.ContainingType.Name));
    }

    /// <summary>
    /// Determines whether a type is one the library builds by deserializing a payload from the remote end.
    /// </summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="symbols">The types this rule matches against.</param>
    /// <returns><see langword="true"/> if the type is a command result or event args type.</returns>
    /// <remarks>
    /// A type a result merely holds -- a DTO named by one of its properties -- is deserialized just as
    /// surely, but is reached only through the property's type and is not judged here. A type-level
    /// <c>[JsonConverter]</c> takes the type's construction away from the serializer's property metadata
    /// altogether, so a type that declares one is not judged either.
    /// </remarks>
    private static bool IsDeserializedType(INamedTypeSymbol type, DeserializedTypeSymbols symbols)
    {
        if (type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, symbols.Converter)))
        {
            return false;
        }

        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, symbols.CommandResult)
                || SymbolEqualityComparer.Default.Equals(current, symbols.EventArgs))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the constructor the serializer would use assigns the property itself.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="symbols">The types this rule matches against.</param>
    /// <returns><see langword="true"/> if a constructor parameter carries the property's value.</returns>
    /// <remarks>
    /// A parameter whose name matches the property, case insensitively, is bound to it. The serializer
    /// takes the constructor marked <c>[JsonConstructor]</c>; failing that a public parameterless one,
    /// which assigns nothing; failing that the type's only public constructor, which is a record's
    /// primary constructor in the shape this rule meets most often.
    /// </remarks>
    private static bool IsSetByConstructor(IPropertySymbol property, DeserializedTypeSymbols symbols)
    {
        ImmutableArray<IMethodSymbol> constructors = property.ContainingType.InstanceConstructors;
        ImmutableArray<IMethodSymbol> publicConstructors = constructors.Where(constructor => constructor.DeclaredAccessibility == Accessibility.Public).ToImmutableArray();
        IMethodSymbol? chosen = constructors.FirstOrDefault(constructor => GetAttribute(constructor, symbols.Constructor) is not null)
            ?? publicConstructors.FirstOrDefault(constructor => constructor.Parameters.Length == 0)
            ?? (publicConstructors.Length == 1 ? publicConstructors[0] : null);

        return chosen is not null
            && chosen.Parameters.Any(parameter => string.Equals(parameter.Name, property.Name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAlwaysIgnored(AttributeData? ignore)
    {
        if (ignore is null)
        {
            return false;
        }

        // No condition named means JsonIgnoreCondition.Always; a condition such as WhenWritingNull still maps the name.
        object? condition = ignore.NamedArguments.FirstOrDefault(argument => argument.Key == "Condition").Value.Value;
        return condition is null || (int)condition == JsonIgnoreConditionAlways;
    }

    private static AttributeData? GetAttribute(ISymbol symbol, INamedTypeSymbol? attributeType)
    {
        return symbol.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));
    }

    /// <summary>
    /// The types that decide whether a property is deserialized, and how.
    /// </summary>
    private sealed class DeserializedTypeSymbols
    {
        public DeserializedTypeSymbols(Compilation compilation)
        {
            // A symbol that is absent from the compilation is null here, and then simply matches nothing.
            this.CommandResult = compilation.GetTypeByMetadataName("WebDriverBiDi.CommandResult");
            this.EventArgs = compilation.GetTypeByMetadataName("WebDriverBiDi.WebDriverBiDiEventArgs");
            this.Constructor = compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonConstructorAttribute");
            this.Converter = compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonConverterAttribute");
            this.Ignore = compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonIgnoreAttribute");
            this.Include = compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonIncludeAttribute");
            this.PropertyName = compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonPropertyNameAttribute");
        }

        public INamedTypeSymbol? CommandResult { get; }

        public INamedTypeSymbol? EventArgs { get; }

        public INamedTypeSymbol? Constructor { get; }

        public INamedTypeSymbol? Converter { get; }

        public INamedTypeSymbol? Ignore { get; }

        public INamedTypeSymbol? Include { get; }

        public INamedTypeSymbol? PropertyName { get; }
    }
}
