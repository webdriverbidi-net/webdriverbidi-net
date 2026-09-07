// <copyright file="BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI010 that awaits a fire-and-forget module command.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider))]
[Shared]
public class BiDiDriver010_FireAndForgetAsyncModuleCommandCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver010_FireAndForgetAsyncModuleCommandAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        InvocationExpressionSyntax invocation = root.FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent!
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // Only offer the fix where `await` is legal. Adding it inside a synchronous method would
        // replace one compile error's worth of trouble with another (CS4033), so a caller that has to
        // become async first is left to make that decision.
        if (!CodeFixHelpers.IsInAsyncContext(invocation))
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Await the command",
                createChangedDocument: c => AddAwaitAsync(context.Document, root, invocation, c),
                equivalenceKey: "AwaitModuleCommand"),
            diagnostic);
    }

    private static Task<Document> AddAwaitAsync(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        // The diagnostic fires only where the call stands alone as an expression statement, so the
        // invocation carries the statement's leading trivia; move it to the await so the indentation
        // and any preceding comment stay put.
        AwaitExpressionSyntax awaitExpression = SyntaxFactory.AwaitExpression(invocation.WithoutLeadingTrivia())
            .WithLeadingTrivia(invocation.GetLeadingTrivia());

        return Task.FromResult(document.WithSyntaxRoot(root.ReplaceNode(invocation, awaitExpression)));
    }
}
