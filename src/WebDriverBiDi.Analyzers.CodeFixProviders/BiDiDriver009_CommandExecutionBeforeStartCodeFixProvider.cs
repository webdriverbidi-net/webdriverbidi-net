// <copyright file="BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
/// Code fix provider for BIDI009 that moves command execution after StartAsync.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider))]
[Shared]
public class BiDiDriver009_CommandExecutionBeforeStartCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId);

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
                title: "Move command execution after StartAsync",
                createChangedDocument: c => MoveCommandAfterStartAsync(
                    context.Document, invocation, c),
                equivalenceKey: "MoveCommandAfterStartAsync"),
            diagnostic);
    }

    private static async Task<Document> MoveCommandAfterStartAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        CancellationToken cancellationToken)
    {
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Find the statement containing the command
        StatementSyntax commandStatement = invocation.FirstAncestorOrSelf<StatementSyntax>()!;

        // Find the method containing this statement
        MethodDeclarationSyntax method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()!;

        // Find the StartAsync call on the same driver variable as the command.
        string driverVariableName = GetRootIdentifierName(invocation.Expression)!;
        StatementSyntax startAsyncStatement = method.Body!.Statements
            .First(s => s.DescendantNodes().OfType<InvocationExpressionSyntax>()
                .Any(inv => inv.Expression is MemberAccessExpressionSyntax ma
                    && ma.Name.Identifier.Text == "StartAsync"
                    && GetRootIdentifierName(ma) == driverVariableName));

        // Track both statements through the transformation
        SyntaxNode trackedMethod = method.TrackNodes(commandStatement, startAsyncStatement);

        // Get the current tracked command statement and remove it
        StatementSyntax trackedCommandStatement = trackedMethod.GetCurrentNode(commandStatement)!;
        SyntaxNode methodWithoutCommand = trackedMethod.RemoveNode(trackedCommandStatement, SyntaxRemoveOptions.KeepNoTrivia)!;

        // Get the current tracked StartAsync statement
        StatementSyntax updatedStartAsyncStatement = methodWithoutCommand.GetCurrentNode(startAsyncStatement)!;

        BlockSyntax block = ((MethodDeclarationSyntax)methodWithoutCommand).Body!;
        int startAsyncIndex = block.Statements.IndexOf(updatedStartAsyncStatement);

        StatementSyntax commandStatementCopy = trackedCommandStatement.WithTrailingTrivia(SyntaxFactory.ElasticLineFeed);
        SyntaxNode newMethod;

        if (startAsyncIndex >= 0 && startAsyncIndex < block.Statements.Count - 1)
        {
            // Insert command after StartAsync (before the next statement)
            StatementSyntax nextStatement = block.Statements[startAsyncIndex + 1];
            newMethod = methodWithoutCommand.InsertNodesBefore(nextStatement, new[] { commandStatementCopy });
        }
        else
        {
            // StartAsync is the last statement; add command at the end
            BlockSyntax newBlock = block.AddStatements(commandStatementCopy);
            newMethod = methodWithoutCommand.ReplaceNode(block, newBlock);
        }

        SyntaxNode newRoot = root.ReplaceNode(method, newMethod);
        return document.WithSyntaxRoot(newRoot);
    }

    private static string? GetRootIdentifierName(ExpressionSyntax expression)
    {
        // expression is always a MemberAccessExpressionSyntax when called from this provider.
        ExpressionSyntax current = ((MemberAccessExpressionSyntax)expression).Expression;
        while (current is MemberAccessExpressionSyntax nestedAccess)
        {
            current = nestedAccess.Expression;
        }

        return ((IdentifierNameSyntax)current).Identifier.Text;
    }
}
