// <copyright file="BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI023 that adds RunHandlerAsynchronously option to AddObserver calls
/// that contain module command invocations.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider))]
[Shared]
public class BiDiDriver023_ModuleCommandInEventHandlerCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver023_ModuleCommandInEventHandlerAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        SyntaxNode? moduleCommandNode = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .FirstOrDefault();

        if (moduleCommandNode == null)
        {
            return;
        }

        InvocationExpressionSyntax? addObserverInvocation = moduleCommandNode.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(inv =>
                inv.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "AddObserver");

        if (addObserverInvocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add RunHandlerAsynchronously option",
                createChangedDocument: c => CodeFixHelpers.AddRunHandlerAsynchronouslyOptionAsync(
                    context.Document, addObserverInvocation, c),
                equivalenceKey: "AddRunHandlerAsynchronously"),
            diagnostic);
    }
}
