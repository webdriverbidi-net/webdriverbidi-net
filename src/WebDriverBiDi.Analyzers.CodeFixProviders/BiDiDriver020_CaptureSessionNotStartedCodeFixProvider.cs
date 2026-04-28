// <copyright file="BiDiDriver020_CaptureSessionNotStartedCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI020 that inserts a <c>StartCapturingTasks()</c> call before the
/// offending <c>WaitForAsync</c> or <c>WaitForCapturedTasksAsync</c> invocation.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver020_CaptureSessionNotStartedCodeFixProvider))]
[Shared]
public class BiDiDriver020_CaptureSessionNotStartedCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BiDiDriver020_CaptureSessionNotStartedAnalyzer.DiagnosticId);

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
            .First();

        if (invocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add StartCapturingTasks() before this call",
                createChangedDocument: c => AddStartCapturingAsync(context.Document, invocation, c),
                equivalenceKey: "AddStartCapturingTasks"),
            diagnostic);
    }

    private static async Task<Document> AddStartCapturingAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        StatementSyntax? targetStatement = invocation.FirstAncestorOrSelf<StatementSyntax>();
        if (targetStatement == null)
        {
            return document;
        }

        // Derive the receiver name from the invocation (e.g. "observer" from "observer.WaitForAsync(...)").
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Expression is not IdentifierNameSyntax receiverIdentifier)
        {
            return document;
        }

        string receiverName = receiverIdentifier.Identifier.Text;

        // Build: observer.StartCapturingTasks();
        ExpressionStatementSyntax startCapturingStatement =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(receiverName),
                        SyntaxFactory.IdentifierName("StartCapturingTasks"))))
            .WithTrailingTrivia(SyntaxFactory.ElasticLineFeed)
            .WithLeadingTrivia(targetStatement.GetLeadingTrivia());

        SyntaxNode newRoot = root.InsertNodesBefore(targetStatement, new[] { startCapturingStatement });
        return document.WithSyntaxRoot(newRoot);
    }
}
