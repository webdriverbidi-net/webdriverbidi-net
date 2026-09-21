// <copyright file="BiDiDriver017_NullableListAddAnalyzer.cs" company="WebDriverBiDi.NET Committers">
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

    /// <summary>
    /// The key of the diagnostic property that carries the fully qualified name of the list's element
    /// type, for the code fix to insert.
    /// </summary>
    public const string ElementTypeFullNamePropertyName = "ElementTypeFullName";

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
        description: Description,
        helpLinkUri: "https://webdriverbidi-net.github.io/webdriverbidi-net/articles/advanced/analyzers.html#bidi017");

    private static readonly HashSet<string> AddMethodNames = new(StringComparer.Ordinal)
    {
        "Add",
        "AddRange",
        "Insert",
        "InsertRange",
    };

    // Minimally-qualified display names of the collection types whose nullable form this rule
    // flags. Both the short and namespace-qualified spellings are accepted so the lookup is
    // independent of how the symbol display format renders the type.
    // Compared against INamedTypeSymbol.Name, which carries no type arguments and no namespace, so
    // that matching costs no formatted display string.
    private static readonly HashSet<string> CollectionTypeNames = new(StringComparer.Ordinal)
    {
        "List",
        "IList",
        "ICollection",
    };

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        if (!AddMethodNames.Contains(memberAccess.Name.Identifier.ValueText))
        {
            return;
        }

        // The fix emits ??=, which is C# 8. Reporting below that would offer a fix that does not
        // compile; until this rule read the declared type, the missing annotations hid the case. The
        // tree is one this C# analyzer was handed, so its options are always CSharpParseOptions.
        if (((CSharpParseOptions)memberAccess.SyntaxTree.Options).LanguageVersion < LanguageVersion.CSharp8)
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

        // The property's own declared type decides this, not the type of the expression here: the
        // expression's annotation comes from the calling code's nullable context, which is off in a
        // nullable-oblivious project -- the one place the compiler warns about none of this itself.
        // The declaration's annotation is read from the library's metadata either way.
        (bool isNullableList, ITypeSymbol? elementType) = GetNullableListElementType(propertySymbol.Type);
        if (!isNullableList || elementType == null)
        {
            return;
        }

        // The suggested fix assigns the property (Items ??= new List<T>()), so only a property the calling
        // code can assign is reported. A get-only property, such as the read-only list views on received
        // types (BrowsingContextInfo.Children), and one whose setter is init-only or not accessible here,
        // cannot take that assignment, and suggesting it would offer a fix that does not compile.
        if (propertySymbol.SetMethod is not { IsInitOnly: false } setter
            || !semanticModel.IsAccessible(memberAccess.Expression.SpanStart, setter))
        {
            return;
        }

        // Require the property's containing type to be a WebDriverBiDi type so an unrelated user
        // type with a nullable list property is not flagged with this BiDi-branded warning.
        if (!AnalyzerSymbolHelpers.IsInWebDriverBiDiNamespace(propertySymbol.ContainingType))
        {
            return;
        }

        string propertyName = propertySymbol.Name;
        string elementTypeName = elementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        // The message names the element type minimally (Header); the code fix needs a name that
        // resolves wherever the fix is applied, so it is also recorded fully qualified
        // (global::WebDriverBiDi.Network.Header) for the fix to insert and let the host simplify.
        ImmutableDictionary<string, string?> properties = ImmutableDictionary.CreateRange(
            new KeyValuePair<string, string?>[]
            {
                new("PropertyName", propertyName),
                new("ElementTypeName", elementTypeName),
                new(ElementTypeFullNamePropertyName, elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
            });

        Diagnostic diagnostic = Diagnostic.Create(
            Rule,
            memberAccess.Expression.GetLocation(),
            properties,
            elementTypeName,
            propertyName);

        context.ReportDiagnostic(diagnostic);
    }

    private static (bool isNullableList, ITypeSymbol? elementType) GetNullableListElementType(ITypeSymbol? type)
    {
        // Handle nullable reference types: List<T>?, IList<T>?, ICollection<T>?
        ITypeSymbol? effectiveType = type;

        // Check for List<T>, IList<T>, ICollection<T>
        if (effectiveType is INamedTypeSymbol namedTypeSymbol)
        {
            if (CollectionTypeNames.Contains(namedTypeSymbol.OriginalDefinition.Name) && namedTypeSymbol.TypeArguments.Length == 1)
            {
                return (IsNullableType(namedTypeSymbol), namedTypeSymbol.TypeArguments[0]);
            }
        }

        return (false, null);
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        return type.NullableAnnotation == NullableAnnotation.Annotated;
    }
}
