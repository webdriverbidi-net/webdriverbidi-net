// <copyright file="BiDiDriver022_AdditionalDataMutationAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
        description: Description);

    // Methods on Dictionary<TKey, TValue> that add new values (and therefore introduce
    // potentially non-AOT-safe objects that will be serialized later).
    private static readonly ImmutableHashSet<string> ValueAddingMethodNames = ImmutableHashSet.Create(
        "Add",
        "TryAdd");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

        // The property must return Dictionary<string, object?> (or a type that is or derives from it).
        if (property.Type is not INamedTypeSymbol returnType)
        {
            return false;
        }

        return IsDictionaryStringObject(returnType);
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
            ITypeSymbol? receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (receiverType != null)
            {
                return receiverType.Name;
            }
        }

        // Fallback: use the declaring type of the property symbol.
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(additionalDataExpr).Symbol;
        return symbol?.ContainingType?.Name ?? string.Empty;
    }

    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        AssignmentExpressionSyntax assignment = (AssignmentExpressionSyntax)context.Node;

        // We are looking for: someExpr[key] = value
        if (assignment.Left is not ElementAccessExpressionSyntax elementAccess)
        {
            return;
        }

        if (!IsAdditionalDataProperty(context, elementAccess.Expression))
        {
            return;
        }

        string typeName = ReceiverTypeName(context, elementAccess.Expression);
        context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation(), typeName));
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        string methodName = memberAccess.Name.Identifier.Text;
        if (!ValueAddingMethodNames.Contains(methodName))
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
