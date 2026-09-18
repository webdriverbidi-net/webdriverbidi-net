// <copyright file="BiDiDriver033_ExtensionDataShadowsPropertyAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that detects an entry with a constant name, written to one of the library's extension-data dictionaries
/// (<c>AdditionalData</c>, <c>AdditionalCapabilities</c>, <c>Command.AdditionalCommandProperties</c>), that reuses
/// the name of a property the same object already serializes. The object would carry that name twice, and sending
/// it throws a <c>WebDriverBiDiSerializationException</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver033_ExtensionDataShadowsPropertyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI033";

    private const string Category = "Usage";

    // JsonIgnoreCondition.Always, which is also what [JsonIgnore] means when no condition is named.
    private const int JsonIgnoreConditionAlways = 1;

    private static readonly LocalizableString Title = "Extension data entry reuses a serialized property name";

    private static readonly LocalizableString MessageFormat = "'{0}' is a property name that {1} already serializes, so sending it throws a WebDriverBiDiSerializationException instead of writing the name twice. Set the typed property instead, or give the entry a different name.";

    private static readonly LocalizableString Description = "The library's extension-data dictionaries add properties to the JSON object their owner writes. An entry named for a property the object already writes would put that name in the object twice, which has no defined meaning, so the library throws a WebDriverBiDiSerializationException when the object is sent. Set the typed property to send that value; use the dictionary only for names the type does not already write.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi033");

    private static readonly string[] EntryAddingMethodNames = ["Add", "TryAdd"];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationStart =>
        {
            // A symbol that is absent from the compilation is null here, and then simply matches no attribute.
            JsonAttributes attributes = new(
                compilationStart.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonExtensionDataAttribute"),
                compilationStart.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonIgnoreAttribute"),
                compilationStart.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonIncludeAttribute"),
                compilationStart.Compilation.GetTypeByMetadataName("System.Text.Json.Serialization.JsonPropertyNameAttribute"));

            compilationStart.RegisterSyntaxNodeAction(nodeContext => AnalyzeAssignment(nodeContext, attributes), SyntaxKind.SimpleAssignmentExpression);
            compilationStart.RegisterSyntaxNodeAction(nodeContext => AnalyzeInvocation(nodeContext, attributes), SyntaxKind.InvocationExpression);
        });
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context, JsonAttributes attributes)
    {
        AssignmentExpressionSyntax assignment = (AssignmentExpressionSyntax)context.Node;

        // The statement form: owner.AdditionalData["name"] = value;
        if (assignment.Left is ElementAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax dictionaryAccess, ArgumentList.Arguments: { Count: > 0 } indexArguments })
        {
            if (IsLibraryExtensionData(context.SemanticModel, dictionaryAccess, attributes))
            {
                ReportIfShadowing(context, indexArguments[0].Expression, context.SemanticModel.GetTypeInfo(dictionaryAccess.Expression).Type!, attributes);
            }

            return;
        }

        // The nested-initializer forms, which write into the get-only dictionary rather than assigning the property:
        //   new Owner { AdditionalData = { ["name"] = value } }
        //   new Owner { AdditionalData = { { "name", value } } }
        // Such an assignment binds to the property only inside an object initializer, whose parent is the construction,
        // the assignment of a nested member, or (in code that does not compile) a with expression; each is an
        // expression whose type is the owner's.
        if (assignment.Left is not IdentifierNameSyntax
            || assignment.Right is not InitializerExpressionSyntax entries
            || !IsLibraryExtensionData(context.SemanticModel, assignment.Left, attributes))
        {
            return;
        }

        SyntaxNode ownerInitializerParent = assignment.Parent!.Parent!;
        SyntaxNode owner = ownerInitializerParent is AssignmentExpressionSyntax memberAssignment ? memberAssignment.Left : ownerInitializerParent;
        ITypeSymbol ownerType = context.SemanticModel.GetTypeInfo(owner).Type!;
        foreach (ExpressionSyntax entry in entries.Expressions)
        {
            ExpressionSyntax? name = entry switch
            {
                AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: { Count: > 0 } entryIndex } } => entryIndex[0].Expression,
                InitializerExpressionSyntax { Expressions: { Count: > 0 } pair } => pair[0],
                _ => null,
            };

            if (name is not null)
            {
                ReportIfShadowing(context, name, ownerType, attributes);
            }
        }
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, JsonAttributes attributes)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        // owner.AdditionalData.Add("name", value) or owner.AdditionalData.TryAdd("name", value)
        if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax dictionaryAccess } call
            || !EntryAddingMethodNames.Contains(call.Name.Identifier.ValueText)
            || invocation.ArgumentList.Arguments.Count == 0
            || !IsLibraryExtensionData(context.SemanticModel, dictionaryAccess, attributes))
        {
            return;
        }

        ReportIfShadowing(context, invocation.ArgumentList.Arguments[0].Expression, context.SemanticModel.GetTypeInfo(dictionaryAccess.Expression).Type!, attributes);
    }

    /// <summary>
    /// Determines whether an expression reads one of the library's extension-data dictionaries.
    /// </summary>
    /// <param name="semanticModel">The semantic model.</param>
    /// <param name="expression">The expression.</param>
    /// <param name="attributes">The serialization attributes of the compilation.</param>
    /// <returns><see langword="true"/> if the expression is a library property marked with <c>[JsonExtensionData]</c>; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// Every such dictionary the library lets a caller write to is checked for this collision when its owner is sent.
    /// The receiver is rejected by its written name before it is bound, as BIDI022 rejects it: every
    /// <c>x.y[k] = v</c> and <c>x.y.Add(...)</c> in a compilation reaches this method, and nearly all of them name
    /// something other than one of the library's extension-data properties.
    /// </remarks>
    private static bool IsLibraryExtensionData(SemanticModel semanticModel, ExpressionSyntax expression, JsonAttributes attributes)
    {
        return AnalyzerSymbolHelpers.ExtensionDataPropertyNames.Contains(expression.GetLastToken().ValueText)
            && semanticModel.GetSymbolInfo(expression).Symbol is IPropertySymbol property
            && AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(property.ContainingType)
            && GetAttribute(property, attributes.ExtensionData) is not null;
    }

    private static void ReportIfShadowing(SyntaxNodeAnalysisContext context, ExpressionSyntax nameExpression, ITypeSymbol ownerType, JsonAttributes attributes)
    {
        if (context.SemanticModel.GetConstantValue(nameExpression) is not { HasValue: true, Value: string name }
            || !GetSerializedPropertyNames(ownerType, attributes).Contains(name))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, nameExpression.GetLocation(), name, ownerType.Name));
    }

    /// <summary>
    /// Gets the names a type's own properties are written under.
    /// </summary>
    /// <param name="type">The static type of the object that owns the dictionary.</param>
    /// <param name="attributes">The serialization attributes of the compilation.</param>
    /// <returns>The serialized property names.</returns>
    /// <remarks>
    /// This mirrors the metadata the serializer builds: an instance property with a getter that is public or marked
    /// <c>[JsonInclude]</c>, and is not always ignored, is written under its <c>[JsonPropertyName]</c> or, failing that,
    /// its own name. The library sets no naming policy. The static type is used, so a name only a derived type writes
    /// is not seen; nor is a name the library writes through a member internal to it, such as the shim that omits an
    /// empty optional list, because a referenced assembly's internal members are not imported into the compilation.
    /// Either can miss a collision, but neither reports one that is not there.
    /// </remarks>
    private static HashSet<string> GetSerializedPropertyNames(ITypeSymbol type, JsonAttributes attributes)
    {
        HashSet<string> names = new(StringComparer.Ordinal);
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (IPropertySymbol property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic
                    || property.IsIndexer
                    || property.GetMethod is null
                    || (property.GetMethod.DeclaredAccessibility != Accessibility.Public && GetAttribute(property, attributes.Include) is null)
                    || GetAttribute(property, attributes.ExtensionData) is not null
                    || IsAlwaysIgnored(GetAttribute(property, attributes.Ignore)))
                {
                    continue;
                }

                // A name attribute written without its argument does not compile, and leaves the property's own name.
                string? declaredName = GetAttribute(property, attributes.PropertyName)?.ConstructorArguments.FirstOrDefault().Value as string;
                names.Add(declaredName ?? property.Name);
            }
        }

        return names;
    }

    private static bool IsAlwaysIgnored(AttributeData? ignore)
    {
        if (ignore is null)
        {
            return false;
        }

        // No condition named means JsonIgnoreCondition.Always; a condition such as WhenWritingNull still writes the name.
        object? condition = ignore.NamedArguments.FirstOrDefault(argument => argument.Key == "Condition").Value.Value;
        return condition is null || (int)condition == JsonIgnoreConditionAlways;
    }

    private static AttributeData? GetAttribute(IPropertySymbol property, INamedTypeSymbol? attributeType)
    {
        return property.GetAttributes().FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));
    }

    /// <summary>
    /// The System.Text.Json attribute types that decide which properties a type serializes, and under what names.
    /// </summary>
    private sealed class JsonAttributes
    {
        public JsonAttributes(INamedTypeSymbol? extensionData, INamedTypeSymbol? ignore, INamedTypeSymbol? include, INamedTypeSymbol? propertyName)
        {
            this.ExtensionData = extensionData;
            this.Ignore = ignore;
            this.Include = include;
            this.PropertyName = propertyName;
        }

        public INamedTypeSymbol? ExtensionData { get; }

        public INamedTypeSymbol? Ignore { get; }

        public INamedTypeSymbol? Include { get; }

        public INamedTypeSymbol? PropertyName { get; }
    }
}
