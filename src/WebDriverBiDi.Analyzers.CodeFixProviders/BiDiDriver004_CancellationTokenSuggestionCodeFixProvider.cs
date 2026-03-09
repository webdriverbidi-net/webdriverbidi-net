// <copyright file="BiDiDriver004_CancellationTokenSuggestionCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI004 that adds CancellationToken parameters to method calls.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver004_CancellationTokenSuggestionCodeFixProvider))]
[Shared]
public class BiDiDriver004_CancellationTokenSuggestionCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BiDiDriver004_CancellationTokenSuggestionAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() =>
        WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var invocation = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        if (invocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add CancellationToken.None parameter",
                createChangedDocument: c => AddCancellationTokenNoneAsync(
                    context.Document, invocation, c),
                equivalenceKey: "AddCancellationTokenNone"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add cancellationToken parameter",
                createChangedDocument: c => AddCancellationTokenParameterAsync(
                    context.Document, invocation, c),
                equivalenceKey: "AddCancellationTokenParameter"),
            diagnostic);
    }

    private static async Task<Document> AddCancellationTokenNoneAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Add CancellationToken.None as last argument
        var tokenArgument = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("CancellationToken"),
                SyntaxFactory.IdentifierName("None")));

        var newArgumentList = invocation.ArgumentList.AddArguments(tokenArgument);
        var newInvocation = invocation.WithArgumentList(newArgumentList);

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> AddCancellationTokenParameterAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Add a cancellationToken variable reference
        var tokenArgument = SyntaxFactory.Argument(
            SyntaxFactory.IdentifierName("cancellationToken"));

        var newArgumentList = invocation.ArgumentList.AddArguments(tokenArgument);
        var newInvocation = invocation.WithArgumentList(newArgumentList);

        var newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
