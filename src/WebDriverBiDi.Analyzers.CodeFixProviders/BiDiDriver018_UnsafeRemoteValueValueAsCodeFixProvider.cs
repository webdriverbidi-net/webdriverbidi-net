// <copyright file="BiDiDriver018_UnsafeRemoteValueValueAsCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI018 that replaces ValueAs&lt;Dictionary&lt;string, object&gt;&gt; with
/// ValueAs&lt;RemoteValueDictionary&gt; and ValueAs&lt;List&lt;object&gt;&gt; with ValueAs&lt;RemoteValueList&gt;.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver018_UnsafeRemoteValueValueAsCodeFixProvider))]
[Shared]
public class BiDiDriver018_UnsafeRemoteValueValueAsCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BiDiDriver018_UnsafeRemoteValueValueAsAnalyzer.DiagnosticId);

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

        SyntaxNode? node = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<GenericNameSyntax>()
            .FirstOrDefault();

        if (node == null)
        {
            return;
        }

        GenericNameSyntax genericName = (GenericNameSyntax)node;
        if (genericName.Identifier.Text != "ValueAs" || genericName.TypeArgumentList.Arguments.Count != 1)
        {
            return;
        }

        SemanticModel? semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
            .ConfigureAwait(false);
        if (semanticModel == null)
        {
            return;
        }

        ITypeSymbol? typeArgument = semanticModel.GetTypeInfo(genericName.TypeArgumentList.Arguments[0], context.CancellationToken).Type;
        if (typeArgument == null)
        {
            return;
        }

        string replacementType;
        if (BiDiDriver018_UnsafeRemoteValueValueAsAnalyzerHelper.IsDictionaryStringObject(typeArgument))
        {
            replacementType = "RemoteValueDictionary";
        }
        else if (BiDiDriver018_UnsafeRemoteValueValueAsAnalyzerHelper.IsListObject(typeArgument))
        {
            replacementType = "RemoteValueList";
        }
        else
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Use ValueAs<{replacementType}>()",
                createChangedDocument: c => ReplaceTypeArgumentAsync(
                    context.Document,
                    genericName,
                    replacementType,
                    c),
                equivalenceKey: $"UseValueAs{replacementType}"),
            diagnostic);
    }

    private static async Task<Document> ReplaceTypeArgumentAsync(
        Document document,
        GenericNameSyntax genericName,
        string replacementType,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        TypeSyntax newTypeArgument = SyntaxFactory.IdentifierName(replacementType);
        TypeArgumentListSyntax newTypeArgumentList = genericName.TypeArgumentList
            .WithArguments(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(newTypeArgument));

        GenericNameSyntax newGenericName = genericName.WithTypeArgumentList(newTypeArgumentList);

        SyntaxNode newRoot = root.ReplaceNode(genericName, newGenericName);
        return document.WithSyntaxRoot(newRoot);
    }
}
