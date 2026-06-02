// <copyright file="BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI003 that moves RegisterTypeInfoResolver before StartAsync.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider))]
[Shared]
public class BiDiDriver003_TypeInfoResolverRegistrationAfterStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver003_TypeInfoResolverRegistrationAfterStartAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        InvocationExpressionSyntax invocation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Move RegisterTypeInfoResolver before StartAsync",
                createChangedDocument: c => MoveRegisterTypeInfoResolverBeforeStartAsync(
                    context.Document, invocation, c),
                equivalenceKey: "MoveRegisterTypeInfoResolverBeforeStartAsync"),
            diagnostic);
    }

    private static async Task<Document> MoveRegisterTypeInfoResolverBeforeStartAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Find the statement containing the RegisterTypeInfoResolver call
        StatementSyntax registerStatement = invocation.FirstAncestorOrSelf<StatementSyntax>()!;

        // Find the method containing this statement
        MethodDeclarationSyntax method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()!;

        // Find the StartAsync call on the same driver variable as the resolver registration.
        string driverVariableName = GetRootIdentifierName(invocation.Expression);
        StatementSyntax startAsyncStatement = method.Body!.Statements
            .First(s => s.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Any(inv => inv.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "StartAsync"
                    && GetRootIdentifierName(ma) == driverVariableName));

        // Track both statements through the transformation
        SyntaxNode trackedMethod = method.TrackNodes(registerStatement, startAsyncStatement);

        // Get the current tracked register statement and remove it
        StatementSyntax trackedRegisterStatement = trackedMethod.GetCurrentNode(registerStatement)!;
        SyntaxNode methodWithoutRegister = trackedMethod.RemoveNode(trackedRegisterStatement, SyntaxRemoveOptions.KeepNoTrivia)!;

        // Get the current tracked StartAsync statement
        StatementSyntax updatedStartAsyncStatement = methodWithoutRegister.GetCurrentNode(startAsyncStatement)!;

        // Insert the register statement before StartAsync
        StatementSyntax registerStatementCopy = trackedRegisterStatement.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);
        SyntaxNode newMethod = methodWithoutRegister.InsertNodesBefore(updatedStartAsyncStatement, new[] { registerStatementCopy });

        SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }

    private static string GetRootIdentifierName(ExpressionSyntax expression)
    {
        // BIDI003 fires only on direct driver calls (driver.RegisterTypeInfoResolverAsync),
        // so the expression is always a single-level MemberAccess with an identifier base.
        return ((IdentifierNameSyntax)((MemberAccessExpressionSyntax)expression).Expression).Identifier.Text;
    }
}
