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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        if (diagnostic.Properties.TryGetValue(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.FormPropertyName, out string? form)
            && form == BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.AwaitUsingFormValue)
        {
            // The diagnostic is on the identifier of an `await using` declaration or statement.
            SyntaxToken identifierToken = root!.FindToken(diagnosticSpan.Start);
            string awaitUsingDriverName = identifierToken.ValueText;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Insert StopAsync before the end of the await using scope",
                    createChangedDocument: c => InsertStopAsyncBeforeImplicitDisposeAsync(context.Document, identifierToken, awaitUsingDriverName, c),
                    equivalenceKey: "InsertStopAsyncBeforeImplicitDisposeAsync"),
                diagnostic);
            return;
        }

        InvocationExpressionSyntax invocation = root!.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // Extract the driver variable name directly from the invocation expression.
        // The analyzer only fires when the receiver is a simple identifier, so this cast is safe.
        string driverVariableName = ((IdentifierNameSyntax)((MemberAccessExpressionSyntax)invocation.Expression).Expression).Identifier.Text;

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
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        StatementSyntax disposeStatement = disposeAsyncInvocation.FirstAncestorOrSelf<StatementSyntax>()!;
        StatementSyntax stopAsyncStatement = CreateStopAsyncStatement(driverVariableName);

        SyntaxNode newRoot = root.InsertNodesBefore(disposeStatement, new[] { stopAsyncStatement });
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> InsertStopAsyncBeforeImplicitDisposeAsync(
        Document document,
        SyntaxToken identifierToken,
        string driverVariableName,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        StatementSyntax stopAsyncStatement = CreateStopAsyncStatement(driverVariableName);

        // An `await using (...) statement` form: the disposal happens when the embedded statement
        // finishes, so StopAsync goes at the end of that statement (wrapping it in a block if needed).
        UsingStatementSyntax? usingStatement = identifierToken.Parent!.FirstAncestorOrSelf<UsingStatementSyntax>();
        if (usingStatement is not null)
        {
            StatementSyntax newBody = usingStatement.Statement is BlockSyntax body
                ? body.WithStatements(body.Statements.Add(stopAsyncStatement))
                : SyntaxFactory.Block(usingStatement.Statement, stopAsyncStatement);
            return document.WithSyntaxRoot(root.ReplaceNode(usingStatement.Statement, newBody));
        }

        // An `await using var driver = ...;` declaration: the disposal happens at the end of the
        // enclosing block. Append StopAsync as its last statement, or just before a final
        // return/throw so that the inserted statement is reachable.
        LocalDeclarationStatementSyntax declaration = identifierToken.Parent!.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>()!;
        BlockSyntax enclosingBlock = (BlockSyntax)declaration.Parent!;
        StatementSyntax lastStatement = enclosingBlock.Statements.Last();
        SyntaxNode newRoot = lastStatement is ReturnStatementSyntax or ThrowStatementSyntax
            ? root.InsertNodesBefore(lastStatement, new[] { stopAsyncStatement })
            : root.ReplaceNode(enclosingBlock, enclosingBlock.WithStatements(enclosingBlock.Statements.Add(stopAsyncStatement)));
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

}
