// <copyright file="CodeFixHelpers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Shared helpers for code fix providers.
/// </summary>
internal static class CodeFixHelpers
{
    /// <summary>
    /// Adds or replaces the <c>ObservableEventHandlerOptions.RunHandlerAsynchronously</c> argument
    /// on the given <paramref name="invocation"/> and returns the modified document.
    /// </summary>
    /// <param name="document">The document to modify.</param>
    /// <param name="invocation">The AddObserver invocation to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The updated document.</returns>
    internal static async Task<Document> AddRunHandlerAsynchronouslyOptionAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        SemanticModel? semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (semanticModel == null)
        {
            return document;
        }

        ArgumentSyntax optionsArgument = SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("ObservableEventHandlerOptions"),
                SyntaxFactory.IdentifierName("RunHandlerAsynchronously")));

        bool hasOptionsParameter = invocation.ArgumentList.Arguments.Any(arg =>
        {
            ITypeSymbol? argType = semanticModel.GetTypeInfo(arg.Expression, cancellationToken).Type;
            return argType?.Name == "ObservableEventHandlerOptions";
        });

        ArgumentListSyntax newArgumentList;
        if (hasOptionsParameter)
        {
            ArgumentSyntax existingOptionsArg = invocation.ArgumentList.Arguments.First(arg =>
            {
                ITypeSymbol? argType = semanticModel.GetTypeInfo(arg.Expression, cancellationToken).Type;
                return argType?.Name == "ObservableEventHandlerOptions";
            });

            newArgumentList = invocation.ArgumentList.WithArguments(
                invocation.ArgumentList.Arguments.Replace(existingOptionsArg, optionsArgument));
        }
        else
        {
            newArgumentList = invocation.ArgumentList.AddArguments(optionsArgument);
        }

        InvocationExpressionSyntax newInvocation = invocation.WithArgumentList(newArgumentList);
        SyntaxNode newRoot = root.ReplaceNode(invocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
