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
        SyntaxNode root = (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false))!;

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        if (diagnostic.Properties.TryGetValue(BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.FormPropertyName, out string? form)
            && form == BiDiDriver012_StopAsyncBeforeDisposeAsyncAnalyzer.AwaitUsingFormValue)
        {
            // The diagnostic is on the declarator of an `await using` declaration, or on the receiver
            // expression of an `await using (driver)` statement. Both sit inside an `await using`, so
            // the inserted `await` is always legal.
            SyntaxNode reportedNode = root.FindNode(diagnosticSpan);
            ExpressionSyntax receiver = reportedNode is VariableDeclaratorSyntax declarator
                ? SyntaxFactory.IdentifierName(declarator.Identifier.ValueText)
                : (ExpressionSyntax)reportedNode;
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Insert StopAsync before the end of the await using scope",
                    createChangedDocument: c => InsertStopAsyncBeforeImplicitDisposeAsync(context.Document, reportedNode, receiver, c),
                    equivalenceKey: "InsertStopAsyncBeforeImplicitDisposeAsync"),
                diagnostic);
            return;
        }

        InvocationExpressionSyntax invocation = root.FindToken(diagnosticSpan.Start)
            .Parent!.AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .First();

        // The inserted statement awaits StopAsync, so it is offered only where `await` is legal. The
        // analyzer also reports a blocking DisposeAsync().AsTask().GetAwaiter().GetResult() inside a
        // synchronous Dispose(), where an inserted await would not compile (CS4033).
        if (!CodeFixHelpers.IsInAsyncContext(invocation))
        {
            return;
        }

        // The analyzer reports only a receiver that is a simple identifier or a `this.` member, so
        // the receiver expression is reused as written: `this.driver` stays `this.driver`.
        ExpressionSyntax disposedReceiver = ((MemberAccessExpressionSyntax)invocation.Expression).Expression;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Insert StopAsync before DisposeAsync",
                createChangedDocument: c => InsertStopAsyncBeforeDisposeAsync(context.Document, invocation, disposedReceiver, c),
                equivalenceKey: "InsertStopAsyncBeforeDisposeAsync"),
            diagnostic);
    }

    private static async Task<Document> InsertStopAsyncBeforeDisposeAsync(
        Document document,
        InvocationExpressionSyntax disposeAsyncInvocation,
        ExpressionSyntax receiver,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        StatementSyntax disposeStatement = disposeAsyncInvocation.FirstAncestorOrSelf<StatementSyntax>()!;
        StatementSyntax stopAsyncStatement = CreateStopAsyncStatement(receiver);

        // A DisposeAsync in a top-level statement has a global statement, not a block, as the
        // statement's parent; the new statement must be wrapped the same way to sit beside it.
        SyntaxNode newRoot = disposeStatement.Parent is GlobalStatementSyntax globalStatement
            ? root.InsertNodesBefore(globalStatement, new[] { CreateGlobalStatement(root, stopAsyncStatement) })
            : CodeFixHelpers.InsertStatementBefore(root, disposeStatement, stopAsyncStatement);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> InsertStopAsyncBeforeImplicitDisposeAsync(
        Document document,
        SyntaxNode reportedNode,
        ExpressionSyntax receiver,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;
        StatementSyntax stopAsyncStatement = CreateStopAsyncStatement(receiver);

        // An `await using (...) statement` form, spelled with either a declaration or an expression:
        // the disposal happens when the embedded statement finishes, so StopAsync goes at the end of
        // that statement (wrapping it in a block if needed). The relationship is tested directly
        // rather than by searching the ancestors, so that a declaration nested inside some other
        // using statement's block is not mistaken for this form.
        UsingStatementSyntax? usingStatement = reportedNode switch
        {
            VariableDeclaratorSyntax { Parent.Parent: UsingStatementSyntax statement } => statement,
            ExpressionSyntax { Parent: UsingStatementSyntax statement } => statement,
            _ => null,
        };

        if (usingStatement is not null)
        {
            StatementSyntax newBody = usingStatement.Statement is BlockSyntax body
                ? body.WithStatements(body.Statements.Add(stopAsyncStatement))
                : SyntaxFactory.Block(usingStatement.Statement, stopAsyncStatement);
            return document.WithSyntaxRoot(root.ReplaceNode(usingStatement.Statement, newBody));
        }

        // An `await using var driver = ...;` declaration: the disposal happens at the end of the
        // enclosing block, or of the top-level program. Append StopAsync as its last statement, or
        // just before a final return/throw so that the inserted statement is reachable.
        LocalDeclarationStatementSyntax declaration = (LocalDeclarationStatementSyntax)reportedNode.Parent!.Parent!;
        if (declaration.Parent is BlockSyntax enclosingBlock)
        {
            StatementSyntax lastStatement = enclosingBlock.Statements.Last();
            SyntaxNode newBlockRoot = ExitsScope(lastStatement)
                ? root.InsertNodesBefore(lastStatement, new[] { stopAsyncStatement })
                : root.ReplaceNode(enclosingBlock, enclosingBlock.WithStatements(enclosingBlock.Statements.Add(stopAsyncStatement)));
            return document.WithSyntaxRoot(newBlockRoot);
        }

        // The analyzer reports a declaration only when its parent is a block or a global statement.
        // Appending after the last global statement, which may end the file without a line break,
        // gives that statement the line break and the new one whatever the old one had.
        CompilationUnitSyntax compilationUnit = (CompilationUnitSyntax)declaration.Parent!.Parent!;
        GlobalStatementSyntax lastGlobalStatement = compilationUnit.Members.OfType<GlobalStatementSyntax>().Last();
        GlobalStatementSyntax newGlobalStatement = CreateGlobalStatement(root, stopAsyncStatement);
        SyntaxNode newRoot = ExitsScope(lastGlobalStatement.Statement)
            ? root.InsertNodesBefore(lastGlobalStatement, new[] { newGlobalStatement })
            : root.ReplaceNode(
                lastGlobalStatement,
                new[]
                {
                    lastGlobalStatement.WithTrailingTrivia(GetEndOfLine(root)),
                    newGlobalStatement.WithTrailingTrivia(lastGlobalStatement.GetTrailingTrivia()),
                });
        return document.WithSyntaxRoot(newRoot);
    }

    private static GlobalStatementSyntax CreateGlobalStatement(SyntaxNode root, StatementSyntax statement)
    {
        // Global statements are members of the compilation unit, and the formatter separates members
        // that carry elastic trivia with a blank line; stripping the factory's elastic markers and
        // using an explicit line ending keeps the inserted statement on the line after its neighbour.
        return SyntaxFactory.GlobalStatement(statement.WithoutLeadingTrivia().WithTrailingTrivia(GetEndOfLine(root)));
    }

    private static SyntaxTrivia GetEndOfLine(SyntaxNode root)
    {
        // Every compilable file has at least one line break (the usings, if nothing else); reuse
        // the file's own line ending so the fix never mixes styles.
        return root.DescendantTrivia().First(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    private static bool ExitsScope(StatementSyntax statement)
    {
        return statement is ReturnStatementSyntax or ThrowStatementSyntax;
    }

    private static StatementSyntax CreateStopAsyncStatement(ExpressionSyntax receiver)
    {
        InvocationExpressionSyntax stopAsyncInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                receiver.WithoutTrivia(),
                SyntaxFactory.IdentifierName("StopAsync")));

        ExpressionStatementSyntax statement = SyntaxFactory.ExpressionStatement(
            SyntaxFactory.AwaitExpression(stopAsyncInvocation));

        return statement.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);
    }
}
