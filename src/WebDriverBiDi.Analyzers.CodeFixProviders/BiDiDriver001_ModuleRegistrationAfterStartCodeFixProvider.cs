// <copyright file="BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
using Microsoft.CodeAnalysis.Text;

/// <summary>
/// Code fix provider for BIDI001 that moves RegisterModule() calls before StartAsync().
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider))]
[Shared]
public class BiDiDriver001_ModuleRegistrationAfterStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver001_ModuleRegistrationAfterStartAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        InvocationExpressionSyntax invocation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Move RegisterModule() before StartAsync()",
                createChangedDocument: c => MoveRegisterModuleBeforeStartAsync(context.Document, invocation, c),
                equivalenceKey: "MoveRegisterModule"),
            diagnostic);
    }

    private static async Task<Document> MoveRegisterModuleBeforeStartAsync(
        Document document,
        InvocationExpressionSyntax registerModuleInvocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        MethodDeclarationSyntax method = registerModuleInvocation.Ancestors()
            .OfType<MethodDeclarationSyntax>()
            .First();

        // Find the statement containing RegisterModule
        StatementSyntax registerStatement = registerModuleInvocation.Ancestors()
            .OfType<StatementSyntax>()
            .First();

        // Find the StartAsync call
        InvocationExpressionSyntax startAsyncInvocation = method.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .First(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "StartAsync");

        StatementSyntax startAsyncStatement = startAsyncInvocation.Ancestors()
            .OfType<StatementSyntax>()
            .First();

        // Track nodes through transformations
        MethodDeclarationSyntax trackedMethod = method.TrackNodes(registerStatement, startAsyncStatement);

        // Remove RegisterModule from its current location
        StatementSyntax trackedRegisterStatement = trackedMethod.GetCurrentNode(registerStatement)!;
        MethodDeclarationSyntax methodWithoutRegister = trackedMethod.RemoveNode(trackedRegisterStatement, SyntaxRemoveOptions.KeepNoTrivia)!;

        // Find the tracked StartAsync statement in the updated tree
        StatementSyntax updatedStartAsyncStatement = methodWithoutRegister.GetCurrentNode(startAsyncStatement)!;

        // Create a copy of the register statement to insert
        StatementSyntax? registerStatementCopy = trackedRegisterStatement.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);

        // Insert RegisterModule before StartAsync
        MethodDeclarationSyntax? newMethod = methodWithoutRegister.InsertNodesBefore(updatedStartAsyncStatement, new[] { registerStatementCopy });

        SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }
}
