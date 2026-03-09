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
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(BiDiDriver009_CommandExecutionBeforeStartAnalyzer.DiagnosticId);

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
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return document;
        }

        // Find the statement containing the command
        StatementSyntax? commandStatement = invocation.FirstAncestorOrSelf<StatementSyntax>();
        if (commandStatement == null)
        {
            return document;
        }

        // Find the method containing this statement
        MethodDeclarationSyntax? method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body == null)
        {
            return document;
        }

        // Find the StartAsync call for the same driver
        StatementSyntax? startAsyncStatement = FindStartAsyncStatement(method, invocation);
        if (startAsyncStatement == null)
        {
            return document;
        }

        // Track both statements through the transformation
        SyntaxNode trackedMethod = method.TrackNodes(commandStatement, startAsyncStatement);

        // Get the current tracked command statement and remove it
        StatementSyntax? trackedCommandStatement = trackedMethod.GetCurrentNode(commandStatement);
        if (trackedCommandStatement == null)
        {
            return document;
        }

        SyntaxNode? methodWithoutCommand = trackedMethod.RemoveNode(trackedCommandStatement, SyntaxRemoveOptions.KeepNoTrivia);
        if (methodWithoutCommand == null)
        {
            return document;
        }

        // Get the current tracked StartAsync statement
        StatementSyntax? updatedStartAsyncStatement = methodWithoutCommand.GetCurrentNode(startAsyncStatement);
        if (updatedStartAsyncStatement == null)
        {
            return document;
        }

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

    private static StatementSyntax? FindStartAsyncStatement(MethodDeclarationSyntax method, InvocationExpressionSyntax commandInvocation)
    {
        if (method.Body == null)
        {
            return null;
        }

        // Get the driver variable name from the command invocation
        string? driverVariableName = GetDriverVariableName(commandInvocation);
        if (driverVariableName == null)
        {
            return null;
        }

        // Find the first StartAsync call on the same driver variable
        foreach (StatementSyntax statement in method.Body.Statements)
        {
            InvocationExpressionSyntax? invocation = statement.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault();

            if (invocation == null)
            {
                continue;
            }

            if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                continue;
            }

            if (memberAccess.Name.Identifier.Text != "StartAsync")
            {
                continue;
            }

            string? invocationDriverName = GetDriverVariableName(invocation);
            if (invocationDriverName == driverVariableName)
            {
                return statement;
            }
        }

        return null;
    }

    private static string? GetDriverVariableName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        ExpressionSyntax current = memberAccess.Expression;

        // Walk through the member access chain to find the base identifier
        while (current is MemberAccessExpressionSyntax nestedMemberAccess)
        {
            current = nestedMemberAccess.Expression;
        }

        if (current is IdentifierNameSyntax identifier)
        {
            return identifier.Identifier.Text;
        }

        return null;
    }
}
