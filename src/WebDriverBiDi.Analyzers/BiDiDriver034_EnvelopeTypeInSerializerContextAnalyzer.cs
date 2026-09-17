// <copyright file="BiDiDriver034_EnvelopeTypeInSerializerContextAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects a <c>[JsonSerializable]</c> attribute naming one of the library's protocol envelope types
/// (<c>CommandResponseMessage&lt;T&gt;</c>, <c>EventMessage&lt;T&gt;</c>, or another type deriving from
/// <c>Message</c>) on a serializer context. Their members are internal to the library, so a context in another
/// assembly cannot generate working metadata for them, and the transport never asks for it: it reads the envelopes
/// itself and asks the serializer only for the result and event args types.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver034_EnvelopeTypeInSerializerContextAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI034";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Library envelope type registered in a serializer context";

    private static readonly LocalizableString MessageFormat = "'{0}' is a protocol envelope type whose members are internal to WebDriverBiDi, so this serializer context cannot generate working metadata for it. Register only your own result and event args types; the transport reads the envelopes itself.";

    private static readonly LocalizableString Description = "The library's protocol envelopes (CommandResponseMessage<T>, EventMessage<T>, and the other types deriving from Message) have internal members, so a source-generated JsonSerializerContext in another assembly cannot generate working metadata for them. The transport does not need it: it reads the envelopes itself and asks a registered resolver only for your CommandResult and event args types. Remove the [JsonSerializable] attribute for the envelope and register the type it wraps instead.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi034");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        AttributeSyntax attribute = (AttributeSyntax)context.Node;

        // The type is the attribute's first argument, and only a typeof names one; a cheap filter before any bind.
        if (attribute.ArgumentList is not { Arguments.Count: > 0 } argumentList
            || argumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpression)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeConstructor
            || attributeConstructor.ContainingType.ToDisplayString() != "System.Text.Json.Serialization.JsonSerializableAttribute")
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(typeOfExpression.Type).Type is not INamedTypeSymbol registeredType
            || !IsLibraryEnvelopeType(registeredType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            Rule,
            typeOfExpression.GetLocation(),
            registeredType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }

    /// <summary>
    /// Determines whether a type is <c>WebDriverBiDi.Protocol.Message</c> or one of the library's types deriving from
    /// it.
    /// </summary>
    /// <param name="type">The type named by the attribute.</param>
    /// <returns><see langword="true"/> if the type is a library envelope; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// A user's own type deriving from <c>Message</c> is not matched: its members are the user's, and the rule is about
    /// members internal to the library.
    /// </remarks>
    private static bool IsLibraryEnvelopeType(INamedTypeSymbol type)
    {
        if (!AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(type))
        {
            return false;
        }

        // An unbound generic type (typeof(CommandResponseMessage<>)) has no base type of its own; its definition does.
        for (INamedTypeSymbol? current = type.OriginalDefinition; current is not null; current = current.BaseType)
        {
            if (current.Name == "Message" && current.ContainingNamespace.ToDisplayString() == "WebDriverBiDi.Protocol")
            {
                return true;
            }
        }

        return false;
    }
}
