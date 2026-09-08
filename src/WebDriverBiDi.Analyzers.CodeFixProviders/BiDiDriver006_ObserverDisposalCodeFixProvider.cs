// <copyright file="BiDiDriver006_ObserverDisposalCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI006 that adds using statement for EventObserver disposal.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver006_ObserverDisposalCodeFixProvider))]
[Shared]
public class BiDiDriver006_ObserverDisposalCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver006_ObserverDisposalAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        // The fix converts the declaration into a using declaration, which is C# 8. Unlike the other
        // version-sensitive fixes in this package, there is no equivalent one-line spelling to fall back
        // to: a classic using statement would have to take ownership of the rest of the enclosing block
        // and re-indent it, which is a different and far more invasive edit than the one offered here.
        // A project on an older language version keeps the diagnostic, which still says what is wrong,
        // and is left to dispose the observer in whichever way suits its code.
        if (CodeFixHelpers.GetLanguageVersion(context.Document) < LanguageVersion.CSharp8)
        {
            return;
        }

        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        LocalDeclarationStatementSyntax declaration = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<LocalDeclarationStatementSyntax>()
            .First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add 'using' declaration",
                createChangedDocument: c => AddUsingDeclarationAsync(
                    context.Document, declaration, c),
                equivalenceKey: "AddUsingDeclaration"),
            diagnostic);
    }

    private static async Task<Document> AddUsingDeclarationAsync(
        Document document,
        LocalDeclarationStatementSyntax declaration,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Create a new using declaration statement (C# 8.0 style: using var observer = ...)
        // The using keyword should have only a space after it
        SyntaxToken usingKeyword = SyntaxFactory.Token(
            SyntaxTriviaList.Empty,
            SyntaxKind.UsingKeyword,
            SyntaxFactory.TriviaList(SyntaxFactory.Space));

        // Remove any leading trivia from the declaration since we'll add it to the statement
        VariableDeclarationSyntax cleanedDeclaration = declaration.Declaration.WithoutLeadingTrivia();

        LocalDeclarationStatementSyntax usingDeclaration = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.TokenList(usingKeyword),
            cleanedDeclaration,
            declaration.SemicolonToken)
            .WithLeadingTrivia(declaration.GetLeadingTrivia())
            .WithTrailingTrivia(declaration.GetTrailingTrivia());

        SyntaxNode newRoot = root.ReplaceNode(declaration, usingDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
