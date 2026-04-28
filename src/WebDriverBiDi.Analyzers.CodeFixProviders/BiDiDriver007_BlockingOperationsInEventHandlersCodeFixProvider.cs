// <copyright file="BiDiDriver007_BlockingOperationsInEventHandlersCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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

        // Find the AddObserver invocation
        SyntaxNode? blockingOperation = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .FirstOrDefault();

        if (blockingOperation == null)
        {
            return;
        }

        // Find the AddObserver invocation that contains this blocking operation
        InvocationExpressionSyntax? addObserverInvocation = blockingOperation.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.Text == "AddObserver");

        if (addObserverInvocation == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add RunHandlerAsynchronously option",
                createChangedDocument: c => AddRunHandlerAsynchronouslyOptionAsync(
                    context.Document, addObserverInvocation, c),
                equivalenceKey: "AddRunHandlerAsynchronously"),
            diagnostic);
    }

    private static async Task<Document> AddRunHandlerAsynchronouslyOptionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Create the ObservableEventHandlerOptions.RunHandlerAsynchronously argument
        ArgumentSyntax optionsArgument = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("ObservableEventHandlerOptions"),
                SyntaxFactory.IdentifierName("RunHandlerAsynchronously")));

        // Check if there's already an options argument (it would be the second or third parameter)
        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        IMethodSymbol? methodSymbol = semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol as IMethodSymbol;
        if (methodSymbol == null)
        {
            return document;
        }

        // Check if an options parameter already exists
        bool hasOptionsParameter = invocation.ArgumentList.Arguments.Any(arg =>
        {
            ITypeSymbol? argType = semanticModel.GetTypeInfo(arg.Expression, cancellationToken).Type;
            return argType?.Name == "ObservableEventHandlerOptions";
        });

        ArgumentListSyntax newArgumentList;
        if (hasOptionsParameter)
        {
            // Replace the existing options argument
            ArgumentSyntax existingOptionsArg = invocation.ArgumentList.Arguments.First(arg =>
            {
                ITypeSymbol? argType = semanticModel.GetTypeInfo(arg.Expression, cancellationToken).Type;
                return argType?.Name == "ObservableEventHandlerOptions";
            });

            int existingIndex = invocation.ArgumentList.Arguments.IndexOf(existingOptionsArg);
            newArgumentList = invocation.ArgumentList.WithArguments(invocation.ArgumentList.Arguments.Replace(existingOptionsArg, optionsArgument));
        }
        else
        {
            // Add the options argument as a new parameter
            newArgumentList = invocation.ArgumentList.AddArguments(optionsArgument);
        }

        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);
        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
