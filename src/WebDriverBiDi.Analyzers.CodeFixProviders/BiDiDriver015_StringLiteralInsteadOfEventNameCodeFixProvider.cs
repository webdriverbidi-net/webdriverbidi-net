// <copyright file="BiDiDriver015_StringLiteralInsteadOfEventNameCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI015 analyzer.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver015_StringLiteralInsteadOfEventNameCodeFixProvider))]
[Shared]
public class BiDiDriver015_StringLiteralInsteadOfEventNameCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver015_StringLiteralInsteadOfEventNameAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the string literal expression
        LiteralExpressionSyntax? literalExpression = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<LiteralExpressionSyntax>().First();
        if (literalExpression == null)
        {
            return;
        }

        // Get the event path from diagnostic properties
        if (!diagnostic.Properties.TryGetValue("EventPath", out string? eventPath) || eventPath == null)
        {
            return;
        }

        // Register a code action
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Replace with {eventPath}",
                createChangedDocument: c => ReplaceStringLiteralWithEventName(context.Document, root, literalExpression, eventPath, c),
                equivalenceKey: nameof(BiDiDriver015_StringLiteralInsteadOfEventNameCodeFixProvider)),
            diagnostic);
    }

    private static Task<Document> ReplaceStringLiteralWithEventName(
        Document document,
        SyntaxNode root,
        LiteralExpressionSyntax literalExpression,
        string eventPath,
        CancellationToken cancellationToken)
    {
        // Create the member access expression: driver.Module.OnEvent.EventName
        ExpressionSyntax replacementExpression = SyntaxFactory.ParseExpression(eventPath)
            .WithTriviaFrom(literalExpression);

        // Replace the string literal with the member access expression
        SyntaxNode newRoot = root.ReplaceNode(literalExpression, replacementExpression);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}
