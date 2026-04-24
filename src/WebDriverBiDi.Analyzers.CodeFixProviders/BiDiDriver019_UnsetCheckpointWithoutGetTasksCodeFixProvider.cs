// <copyright file="BiDiDriver019_UnsetCheckpointWithoutGetTasksCodeFixProvider.cs" company="WebDriverBiDi.NET Committers">
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
using Microsoft.CodeAnalysis.Formatting;

/// <summary>
/// Code fix provider for BIDI019 analyzer.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BiDiDriver019_UnsetCheckpointWithoutGetTasksCodeFixProvider))]
[Shared]
public class BiDiDriver019_UnsetCheckpointWithoutGetTasksCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(BiDiDriver019_UnsetCheckpointWithoutGetTasksAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics.First();
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the UnsetCheckpoint invocation
        InvocationExpressionSyntax? unsetCheckpointCall = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault();

        if (unsetCheckpointCall == null)
        {
            return;
        }

        // Get the observer variable name
        string? observerName = GetObserverVariableName(unsetCheckpointCall);
        if (observerName == null)
        {
            return;
        }

        // Register a code action
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Call GetCheckpointTasks() before UnsetCheckpoint",
                createChangedDocument: c => InsertGetCheckpointTasks(context.Document, root, unsetCheckpointCall, observerName, c),
                equivalenceKey: nameof(BiDiDriver019_UnsetCheckpointWithoutGetTasksCodeFixProvider)),
            diagnostic);
    }

    private static Task<Document> InsertGetCheckpointTasks(
        Document document,
        SyntaxNode root,
        InvocationExpressionSyntax unsetCheckpointCall,
        string observerName,
        CancellationToken cancellationToken)
    {
        // Find the statement containing the UnsetCheckpoint call
        StatementSyntax? unsetStatement = unsetCheckpointCall.FirstAncestorOrSelf<StatementSyntax>();
        if (unsetStatement == null)
        {
            return Task.FromResult(document);
        }

        // Create the GetCheckpointTasks call statement
        // var tasks = observer.GetCheckpointTasks();
        ExpressionSyntax observerIdentifier = SyntaxFactory.IdentifierName(observerName);
        MemberAccessExpressionSyntax getTasksMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            observerIdentifier,
            SyntaxFactory.IdentifierName("GetCheckpointTasks"));
        InvocationExpressionSyntax getTasksInvocation = SyntaxFactory.InvocationExpression(getTasksMemberAccess);

        // Create: var tasks = observer.GetCheckpointTasks();
        VariableDeclaratorSyntax variableDeclarator = SyntaxFactory.VariableDeclarator(
            SyntaxFactory.Identifier("tasks"))
            .WithInitializer(SyntaxFactory.EqualsValueClause(getTasksInvocation));

        VariableDeclarationSyntax variableDeclaration = SyntaxFactory.VariableDeclaration(
            SyntaxFactory.IdentifierName("var"))
            .WithVariables(SyntaxFactory.SingletonSeparatedList(variableDeclarator));

        LocalDeclarationStatementSyntax getTasksStatement = SyntaxFactory.LocalDeclarationStatement(variableDeclaration)
            .WithAdditionalAnnotations(Formatter.Annotation);

        // Find the block containing the statement
        BlockSyntax? containingBlock = unsetStatement.Parent as BlockSyntax;
        if (containingBlock == null)
        {
            return Task.FromResult(document);
        }

        // Find the index of the unset statement in the block
        int statementIndex = containingBlock.Statements.IndexOf(unsetStatement);
        if (statementIndex < 0)
        {
            return Task.FromResult(document);
        }

        // Insert the GetCheckpointTasks call before the UnsetCheckpoint statement
        SyntaxList<StatementSyntax> newStatements = containingBlock.Statements.Insert(statementIndex, getTasksStatement);
        BlockSyntax newBlock = containingBlock.WithStatements(newStatements);

        // Replace the old block with the new one
        SyntaxNode newRoot = root.ReplaceNode(containingBlock, newBlock);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private static string? GetObserverVariableName(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            // Handle simple identifier: observer.UnsetCheckpoint()
            if (memberAccess.Expression is IdentifierNameSyntax identifier)
            {
                return identifier.Identifier.Text;
            }

            // Handle this-qualified member access: this.observer.UnsetCheckpoint()
            if (memberAccess.Expression is MemberAccessExpressionSyntax nestedMemberAccess &&
                nestedMemberAccess.Expression is ThisExpressionSyntax)
            {
                return nestedMemberAccess.Name.Identifier.Text;
            }
        }

        return null;
    }
}
