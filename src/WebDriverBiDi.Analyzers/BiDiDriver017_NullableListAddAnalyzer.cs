// <copyright file="BiDiDriver017_NullableListAddAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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
/// Analyzer that suggests using the null-coalescing assignment operator (??=) when adding
/// to nullable list properties, to avoid NullReferenceException when the property is null.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BiDiDriver017_NullableListAddAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The diagnostic ID for this analyzer.
    /// </summary>
    public const string DiagnosticId = "BIDI017";

    private const string Category = "Usage";

    private static readonly LocalizableString Title = "Use ??= when adding to nullable list property";

    private static readonly LocalizableString MessageFormat = "Use '??= new List<{0}>()' before adding to nullable list property '{1}' to avoid NullReferenceException when the property is null";

    private static readonly LocalizableString Description = "Use the null-coalescing assignment operator (??=) to initialize nullable list properties before adding items.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly HashSet<string> AddMethodNames = new(StringComparer.Ordinal)
    {
        "Add",
        "AddRange",
        "Insert",
        "InsertRange",
    };

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        InvocationExpressionSyntax invocation = (InvocationExpressionSyntax)context.Node;
        SemanticModel semanticModel = context.SemanticModel;

        // Must be a method call like expr.Add(...) or expr.AddRange(...)
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!AddMethodNames.Contains(memberAccess.Name.Identifier.Text))
        {
            return;
        }

        // Don't flag if already using null-conditional: expr?.Add(...)
        if (memberAccess.Expression is ConditionalAccessExpressionSyntax)
        {
            return;
        }

        // Don't flag if receiver is already wrapped in ??=: (expr ??= new List<T>()).Add(...)
        if (IsInsideNullCoalescingAssignment(memberAccess))
        {
            return;
        }

        // Get the type of the receiver (e.g., params.Contexts)
        ITypeSymbol? receiverType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (receiverType == null)
        {
            return;
        }

        // Check if this is a nullable list/collection type
        (bool isNullableList, ITypeSymbol? elementType) = GetNullableListElementType(receiverType);
        if (!isNullableList || elementType == null)
        {
            return;
        }

        // Only flag when the receiver is a property access (not a local variable that we can't easily fix)
        // We need the property to be on a type - memberAccess.Expression could be IdentifierName (local)
        // or another MemberAccess. For params.Contexts.Add(), the receiver is params.Contexts - the
        // expression is params (IdentifierName) and the member is Contexts. So memberAccess is
        // the full "params.Contexts" - the expression part is "params". We need to verify the member
        // (Contexts) is a property. GetSymbolInfo on memberAccess gives us the property symbol.
        ISymbol? receiverSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
        if (receiverSymbol is not IPropertySymbol propertySymbol)
        {
            return;
        }

        string propertyName = propertySymbol.Name;
        string elementTypeName = elementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        ImmutableDictionary<string, string?> properties = ImmutableDictionary.CreateRange(
            new KeyValuePair<string, string?>[]
            {
                new("PropertyName", propertyName),
                new("ElementTypeName", elementTypeName),
            });

        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Expression.GetLocation(),
            properties,
            elementTypeName,
            propertyName);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInsideNullCoalescingAssignment(MemberAccessExpressionSyntax memberAccess)
    {
        // Check if we're in (expr ??= new List<T>()).Add(...)
        SyntaxNode? parent = memberAccess.Parent;
        while (parent != null)
        {
            if (parent is AssignmentExpressionSyntax assignment && assignment.Kind() == SyntaxKind.CoalesceAssignmentExpression)
            {
                return true;
            }

            parent = parent.Parent;
        }

        return false;
    }

    private static (bool isNullableList, ITypeSymbol? elementType) GetNullableListElementType(ITypeSymbol type)
    {
        // Handle nullable reference types: List<T>?, IList<T>?, ICollection<T>?
        ITypeSymbol effectiveType = type;
        if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            // Value type nullable (int?, etc.) - not a list
            return (false, null);
        }

        // For nullable reference types, the type might have NullableAnnotation
        // Get the underlying type if it's a nullable value type
        if (type.NullableAnnotation == NullableAnnotation.Annotated || type.NullableAnnotation == NullableAnnotation.NotAnnotated)
        {
            effectiveType = type;
        }

        // Check for List<T>, IList<T>, ICollection<T>
        if (effectiveType is INamedTypeSymbol namedTypeSymbol)
        {
            string typeName = namedTypeSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            if (typeName == "List<T>" || typeName == "System.Collections.Generic.List<T>")
            {
                if (namedTypeSymbol.TypeArguments.Length == 1)
                {
                    return (IsNullableType(type), namedTypeSymbol.TypeArguments[0]);
                }
            }

            if (typeName == "IList<T>" || typeName == "System.Collections.Generic.IList<T>" ||
                typeName == "ICollection<T>" || typeName == "System.Collections.Generic.ICollection<T>")
            {
                if (namedTypeSymbol.TypeArguments.Length == 1)
                {
                    return (IsNullableType(type), namedTypeSymbol.TypeArguments[0]);
                }
            }
        }

        return (false, null);
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        // Nullable value type
        if (type is INamedTypeSymbol namedType && namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            return true;
        }

        // Nullable reference type
        return type.NullableAnnotation == NullableAnnotation.Annotated;
    }
}
