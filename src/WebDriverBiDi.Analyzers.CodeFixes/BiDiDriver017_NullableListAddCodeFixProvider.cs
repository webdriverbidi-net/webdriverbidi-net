// <copyright file="BiDiDriver017_NullableListAddCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI017 that wraps the receiver in a null-coalescing assignment (??=)
/// when adding to a nullable list property.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver017_NullableListAddCodeFixProvider))]
[Shared]
public class BiDiDriver017_NullableListAddCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BiDiDriver017_NullableListAddAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        InvocationExpressionSyntax? invocation = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (invocation == null || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (!diagnostic.Properties.TryGetValue("ElementTypeName", out string? elementTypeName) || string.IsNullOrEmpty(elementTypeName))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use ??= to initialize list before adding",
                createChangedDocument: c => ApplyFixAsync(context.Document, invocation, memberAccess, elementTypeName!, c),
                equivalenceKey: "UseNullCoalescingAssignment"),
            diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax memberAccess,
        string elementTypeName,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Create: (receiver ??= new List<ElementType>()).Add(...)
        ExpressionSyntax receiver = memberAccess.Expression;

        // Create new List<ElementType>()
        TypeSyntax elementTypeSyntax = SyntaxFactory.ParseTypeName(elementTypeName);
        ObjectCreationExpressionSyntax listCreation = SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.GenericName(
                SyntaxFactory.Identifier("List"),
                SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementTypeSyntax))),
            SyntaxFactory.ArgumentList(),
            null);

        // Create receiver ??= new List<ElementType>()
        AssignmentExpressionSyntax coalescingAssignment = SyntaxFactory.AssignmentExpression(
            SyntaxKind.CoalesceAssignmentExpression,
            receiver,
            listCreation);

        // Wrap in parentheses: (receiver ??= new List<ElementType>())
        ParenthesizedExpressionSyntax parenthesized = SyntaxFactory.ParenthesizedExpression(coalescingAssignment);

        // Create new member access: (receiver ??= new List<ElementType>()).Add
        MemberAccessExpressionSyntax newMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            parenthesized,
            memberAccess.Name);

        // Create new invocation: (receiver ??= new List<ElementType>()).Add(...)
        InvocationExpressionSyntax newInvocation = invocation.WithExpression(newMemberAccess);

        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);

        return document.WithSyntaxRoot(newRoot);
    }
}
