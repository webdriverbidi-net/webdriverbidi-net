// <copyright file="BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Code fix provider for BIDI007 that adds RunHandlerAsynchronously option to AddObserver calls.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider))]
[Shared]
public class BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver007_BlockingOperationsInEventHandlersAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        SyntaxNode blockingOperation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .First();

        InvocationExpressionSyntax addObserverInvocation = blockingOperation.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "AddObserver");

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add RunHandlerAsynchronously option",
                createChangedDocument: c => CodeFixHelpers.AddRunHandlerAsynchronouslyOptionAsync(
                    context.Document, addObserverInvocation, c),
                equivalenceKey: "AddRunHandlerAsynchronously"),
            diagnostic);
    }
}
