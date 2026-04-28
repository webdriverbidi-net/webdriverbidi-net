// <copyright file="BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI014 that replaces parameterless constructor with Reset property.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider))]
[Shared]
public class BiDiDriver014_ParameterlessConstructorWithResetPropertyCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver014_ParameterlessConstructorWithResetPropertyAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the object creation expression that triggered the diagnostic
        ObjectCreationExpressionSyntax? objectCreation = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<ObjectCreationExpressionSyntax>()
            .First();

        if (objectCreation == null)
        {
            return;
        }

        // Get the type name and reset property name from the diagnostic
        string typeName = diagnostic.Properties["TypeName"] ?? string.Empty;
        string resetPropertyName = diagnostic.Properties["ResetPropertyName"] ?? string.Empty;

        if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(resetPropertyName))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use '{typeName}.{resetPropertyName}' instead",
                createChangedDocument: c => ReplaceWithResetPropertyAsync(
                    context.Document, objectCreation, typeName, resetPropertyName, c),
                equivalenceKey: "UseResetProperty"),
            diagnostic);
    }

    private static async Task<Document> ReplaceWithResetPropertyAsync(
        Document document,
        ObjectCreationExpressionSyntax objectCreation,
        string typeName,
        string resetPropertyName,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Create the replacement: TypeName.ResetPropertyName
        MemberAccessExpressionSyntax resetPropertyAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(typeName),
            SyntaxFactory.IdentifierName(resetPropertyName));

        // Preserve the trivia from the original expression
        resetPropertyAccess = resetPropertyAccess
            .WithLeadingTrivia(objectCreation.GetLeadingTrivia())
            .WithTrailingTrivia(objectCreation.GetTrailingTrivia());

        // Replace the object creation with the reset property access
        SyntaxNode newRoot = root.ReplaceNode(objectCreation, resetPropertyAccess);

        return document.WithSyntaxRoot(newRoot);
    }
}
