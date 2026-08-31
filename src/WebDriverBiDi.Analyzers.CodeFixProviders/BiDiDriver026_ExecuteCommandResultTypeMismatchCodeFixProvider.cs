// <copyright file="BiDiDriver026_ExecuteCommandResultTypeMismatchCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI026 that corrects an <c>ExecuteCommandAsync&lt;T&gt;</c> type argument
/// that disagrees with the command's result type, either by changing it to the correct type or by
/// removing it so the type is inferred.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver026_ExecuteCommandResultTypeMismatchCodeFixProvider))]
[Shared]
public class BiDiDriver026_ExecuteCommandResultTypeMismatchCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver026_ExecuteCommandResultTypeMismatchAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // The diagnostic is reported on the type argument, which the analyzer guarantees is inside a
        // GenericNameSyntax member name of an invocation (ExecuteCommandAsync<T>(...)).
        InvocationExpressionSyntax invocation = root!.FindToken(diagnosticSpan.Start).Parent!
            .AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();
        GenericNameSyntax genericName = (GenericNameSyntax)((MemberAccessExpressionSyntax)invocation.Expression).Name;

        // The analyzer records the correct result type (already minimally qualified for this position).
        string resultTypeName = diagnostic.Properties["ResultType"]!;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Change type argument to '{resultTypeName}'",
                createChangedDocument: c => ChangeTypeArgumentAsync(context.Document, genericName, resultTypeName, c),
                equivalenceKey: "ChangeExecuteCommandTypeArgument"),
            diagnostic);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Remove the explicit type argument (infer it)",
                createChangedDocument: c => RemoveTypeArgumentAsync(context.Document, genericName, c),
                equivalenceKey: "RemoveExecuteCommandTypeArgument"),
            diagnostic);
    }

    private static async Task<Document> ChangeTypeArgumentAsync(
        Document document,
        GenericNameSyntax genericName,
        string resultTypeName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        TypeSyntax oldTypeArgument = genericName.TypeArgumentList.Arguments[0];
        TypeSyntax newTypeArgument = SyntaxFactory.ParseTypeName(resultTypeName).WithTriviaFrom(oldTypeArgument);

        SyntaxNode newRoot = root.ReplaceNode(oldTypeArgument, newTypeArgument);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> RemoveTypeArgumentAsync(
        Document document,
        GenericNameSyntax genericName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Replace ExecuteCommandAsync<T> with ExecuteCommandAsync so the type argument is inferred from
        // the parameters object, which binds the call to the generic CommandParameters<T> overload.
        IdentifierNameSyntax inferredName = SyntaxFactory.IdentifierName(genericName.Identifier).WithTriviaFrom(genericName);
        SyntaxNode newRoot = root.ReplaceNode(genericName, inferredName);
        return document.WithSyntaxRoot(newRoot);
    }
}
