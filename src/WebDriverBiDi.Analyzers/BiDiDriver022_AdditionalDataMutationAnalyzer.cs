// <copyright file="BiDiDriver022_AdditionalDataMutationAnalyzer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

/// <summary>
/// Analyzer that warns when code writes values into an <c>AdditionalData</c> dictionary
/// on a <see cref="WebDriverBiDi.CommandParameters"/> or other BiDi outbound object,
/// because <c>Dictionary&lt;string, object?&gt;</c> values are serialized via reflection
/// and are not compatible with native AOT or IL trimming.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver022_AdditionalDataMutationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI022";

    private const string Category = "Reliability";

    private static readonly LocalizableString Title = "AdditionalData mutation is not AOT-safe";

    private static readonly LocalizableString MessageFormat = "Writing to '{0}.AdditionalData' uses reflection-based JSON serialization that is not compatible with native AOT or IL trimming. Ensure every value's runtime type is registered via BiDiDriver.RegisterTypeInfoResolverAsync, or avoid publishing with PublishAot=true.";

    private static readonly LocalizableString Description = "Dictionary<string, object?> values stored in AdditionalData are serialized using reflection-based JsonSerializer overloads that are not compatible with native AOT or trimmed assemblies. If you are targeting AOT, register a JsonTypeInfoResolver for every value type you add via BiDiDriver.RegisterTypeInfoResolverAsync before sending the command.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi022");

    // Methods on Dictionary<TKey, TValue> that add new values (and therefore introduce
    // potentially non-AOT-safe objects that will be serialized later).
    private static readonly HashSet<string> ValueAddingMethodNames = new(StringComparer.Ordinal)
    {
        "Add",
        "TryAdd",
    };

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Covers dict[key] = value.
        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);

        // Covers dict.Add(...) / dict.TryAdd(...) etc.
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static bool IsAdditionalDataProperty(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;
        if (symbol is not IPropertySymbol property)
        {
            return false;
        }

        if (property.Name != "AdditionalData")
        {
            return false;
        }

        // Require the declaring type to be a WebDriverBiDi type so a user's own type with an
        // AdditionalData property of the same shape is not matched.
        if (!AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(property.ContainingType))
        {
            return false;
        }

        return property.Type is INamedTypeSymbol returnType && IsDictionaryStringObject(returnType);
    }

    private static bool IsDictionaryStringObject(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current != null; current = current.BaseType as INamedTypeSymbol)
        {
            if (current.OriginalDefinition.SpecialType == SpecialType.None
                && current.OriginalDefinition.Name == "Dictionary"
                && current.TypeArguments.Length == 2
                && current.TypeArguments[0].SpecialType == SpecialType.System_String
                && IsNullableObject(current.TypeArguments[1]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNullableObject(ITypeSymbol typeArg)
    {
        // object? is represented as System.Object with NullableAnnotation.Annotated,
        // but in contexts where NullableAnnotation is not tracked it may be Oblivious.
        // Accept both; reject non-object types.
        return typeArg.SpecialType == SpecialType.System_Object;
    }

    // Returns the static type name of the receiver object (i.e. the thing before `.AdditionalData`).
    // `additionalDataExpr` is expected to be a MemberAccessExpressionSyntax whose Name is "AdditionalData".
    private static string ReceiverTypeName(SyntaxNodeAnalysisContext context, ExpressionSyntax additionalDataExpr)
    {
        if (additionalDataExpr is MemberAccessExpressionSyntax memberAccess)
        {
            return context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type!.Name;
        }

        // Fallback for unqualified or parenthesized access (e.g. `AdditionalData[key]` inside the
        // declaring type, or `(cmd.AdditionalData)[key]`). Every caller has already run
        // IsAdditionalDataProperty on this expression, so it binds to a property symbol here.
        IPropertySymbol property = (IPropertySymbol)context.SemanticModel.GetSymbolInfo(additionalDataExpr).Symbol!;
        return property.ContainingType.Name;
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        AssignmentExpressionSyntax assignment = (AssignmentExpressionSyntax)context.Node;

        // The statement form: someExpr.AdditionalData[key] = value
        if (assignment.Left is ElementAccessExpressionSyntax elementAccess)
        {
            if (IsAdditionalDataProperty(context, elementAccess.Expression))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation(), ReceiverTypeName(context, elementAccess.Expression)));
            }

            return;
        }

        // The two nested-initializer spellings inside an object initializer, which write into the
        // get-only property's dictionary rather than assigning the property:
        //   new P { AdditionalData = { ["key"] = value } }   (an indexer element)
        //   new P { AdditionalData = { { "key", value } } }  (a collection element)
        // Both arrive here as the outer `AdditionalData = { ... }` assignment. Each element is
        // reported on its own, as the equivalent statement-form writes would be; the indexer
        // elements are themselves assignments, but their left side is an implicit element access
        // rather than an element access, so they fall through the test above without a report.
        if (assignment.Left is IdentifierNameSyntax
            && assignment.Right is InitializerExpressionSyntax elements
            && IsAdditionalDataProperty(context, assignment.Left))
        {
            // An assignment whose value is an initializer only parses inside an object initializer.
            string typeName = InitializedTypeName(context, (InitializerExpressionSyntax)assignment.Parent!);
            foreach (ExpressionSyntax element in elements.Expressions)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, element.GetLocation(), typeName));
            }
        }
    }

    // Returns the name of the type an object initializer initializes: the created type for
    // `new P { ... }`, or the member's type for a nested `Member = { ... }` initializer. An object
    // initializer has one of those two parents, and both bind to a type here: the AdditionalData
    // property inside it has already resolved to a library property, so the initialized type is a
    // library type.
    private static string InitializedTypeName(SyntaxNodeAnalysisContext context, InitializerExpressionSyntax objectInitializer)
    {
        ExpressionSyntax initialized = objectInitializer.Parent is BaseObjectCreationExpressionSyntax creation
            ? creation
            : ((AssignmentExpressionSyntax)objectInitializer.Parent!).Left;
        return context.SemanticModel.GetTypeInfo(initialized).Type!.Name;
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || !ValueAddingMethodNames.Contains(memberAccess.Name.Identifier.ValueText))
        {
            return;
        }

        if (!IsAdditionalDataProperty(context, memberAccess.Expression))
        {
            return;
        }

        string typeName = ReceiverTypeName(context, memberAccess.Expression);
        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), typeName));
    }
}
