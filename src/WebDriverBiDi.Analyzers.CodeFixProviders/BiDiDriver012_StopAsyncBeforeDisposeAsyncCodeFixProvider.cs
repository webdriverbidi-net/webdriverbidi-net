// <copyright file="BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI012 that inserts StopAsync before DisposeAsync.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider))]
[Shared]
public class BiDiDriver012_StopAsyncBeforeDisposeAsyncCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId);

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

        InvocationExpressionSyntax? invocation = root?.FindToken(diagnosticSpan.Start)
            .Parent?.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        if (invocation == null)
        {
            return;
        }

        string? driverVariableName = GetDriverVariableName(invocation);
        if (driverVariableName == null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Insert StopAsync before DisposeAsync",
                createChangedDocument: c => InsertStopAsyncBeforeDisposeAsync(
                    context.Document, invocation, driverVariableName, c),
                equivalenceKey: "InsertStopAsyncBeforeDisposeAsync"),
            diagnostic);
    }

    private static async Task<Document> InsertStopAsyncBeforeDisposeAsync(
        Document document,
        InvocationExpressionSyntax disposeAsyncInvocation,
        string driverVariableName,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        StatementSyntax? disposeStatement = disposeAsyncInvocation.FirstAncestorOrSelf<StatementSyntax>();
        if (disposeStatement == null)
        {
            return document;
        }

        StatementSyntax stopAsyncStatement = CreateStopAsyncStatement(driverVariableName);

        SyntaxNode newRoot = root.InsertNodesBefore(disposeStatement, new[] { stopAsyncStatement });
        return document.WithSyntaxRoot(newRoot);
    }

    private static StatementSyntax CreateStopAsyncStatement(string driverVariableName)
    {
        InvocationExpressionSyntax stopAsyncInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(driverVariableName),
                SyntaxFactory.IdentifierName("StopAsync")));

        ExpressionStatementSyntax statement = SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AwaitExpression(stopAsyncInvocation));

        return statement.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);
    }

    private static string? GetDriverVariableName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Expression is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        return null;
    }
}
