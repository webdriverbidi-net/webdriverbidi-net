// <copyright file="BiDiDriver037_UnsettableDeserializedPropertyCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Code fix provider for BIDI037 that marks the property with <c>[JsonInclude]</c>, which is what lets
/// the deserializer use its non-public accessor.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver037_UnsettableDeserializedPropertyCodeFixProvider))]
[Shared]
public class BiDiDriver037_UnsettableDeserializedPropertyCodeFixProvider : CodeFixProvider
{
    private const string JsonIncludeAttributeName = "JsonInclude";

    private const string QualifiedJsonIncludeAttributeName = "System.Text.Json.Serialization.JsonInclude";

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver037_UnsettableDeserializedPropertyAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();

        // The diagnostic is reported on the property's own name.
        PropertyDeclarationSyntax property = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Mark the property with [JsonInclude]",
                createChangedDocument: cancellationToken => AddJsonIncludeAsync(context.Document, property, cancellationToken),
                equivalenceKey: "AddJsonInclude"),
            diagnostic);
    }

    private static async Task<Document> AddJsonIncludeAsync(Document document, PropertyDeclarationSyntax property, CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // The property carries [JsonPropertyName], which is what the rule reports on. Written as a
        // simple name, it proves the attribute namespace is in scope here and the new attribute can be
        // written the same way; written any other way, the fix qualifies it rather than add a using.
        bool namespaceInScope = property.AttributeLists
            .SelectMany(attributeList => attributeList.Attributes)
            .Any(attribute => attribute.Name is IdentifierNameSyntax { Identifier.ValueText: "JsonPropertyName" or "JsonPropertyNameAttribute" });

        AttributeListSyntax includeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.ParseName(namespaceInScope ? JsonIncludeAttributeName : QualifiedJsonIncludeAttributeName))));

        // Whatever separates the last attribute from the declaration -- a newline and the property's
        // indentation, or a single space -- separates the new attribute from it in the same way. The
        // last attribute's own leading trivia cannot be reused: for the first one it holds the
        // documentation comment.
        AttributeListSyntax lastList = property.AttributeLists[property.AttributeLists.Count - 1];
        SyntaxTriviaList separator = lastList.GetLastToken().GetNextToken().LeadingTrivia;
        PropertyDeclarationSyntax fixedProperty = property.WithAttributeLists(
            property.AttributeLists.Add(includeList.WithLeadingTrivia(separator)));

        return document.WithSyntaxRoot(root.ReplaceNode(property, fixedProperty));
    }
}
