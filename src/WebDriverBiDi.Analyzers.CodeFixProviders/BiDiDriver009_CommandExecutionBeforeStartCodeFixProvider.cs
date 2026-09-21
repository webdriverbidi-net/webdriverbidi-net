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
    // Every diagnostic in a member moves its call to the same place, so the batch fixer would keep
    // one edit and drop the rest; the fixes are applied one after another instead.
    public sealed override FixAllProvider GetFixAllProvider() => SequentialDocumentFixAllProvider.Create(this);

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

        // The fix relocates the command after an existing StartAsync call on the same driver
        // in the same block-bodied method. The analyzer also fires in constructors and
        // top-level programs, where there is no such method; no fix is possible there, so none
        // is offered.
        MethodDeclarationSyntax? method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (method?.Body is null)
        {
            return;
        }

        // The analyzer fires on far more shapes than the fix can rewrite safely. Unless a StartAsync
        // call exists that the command can be moved after without changing whether or how often it
        // runs, and without stranding a local it declares, no fix is offered.
        if (FindMoveTargetStatement(method, invocation) is null)
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
        SyntaxNode root = (await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false))!;

        // Find the statement containing the command
        StatementSyntax commandStatement = invocation.FirstAncestorOrSelf<StatementSyntax>()!;

        // Find the method containing this statement
        MethodDeclarationSyntax method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()!;

        // Find the StartAsync statement on the same driver variable as the command. Registration has
        // already established that such a statement exists and that moving the command into its block
        // is safe, so the search cannot come back empty here.
        StatementSyntax startAsyncStatement = FindMoveTargetStatement(method, invocation)!;

        // Track both statements through the transformation
        SyntaxNode trackedMethod = method.TrackNodes(commandStatement, startAsyncStatement);

        // Get the current tracked command statement and remove it
        StatementSyntax trackedCommandStatement = trackedMethod.GetCurrentNode(commandStatement)!;
        SyntaxNode methodWithoutCommand = trackedMethod.RemoveNode(trackedCommandStatement, SyntaxRemoveOptions.KeepNoTrivia)!;

        // Get the current tracked StartAsync statement
        StatementSyntax updatedStartAsyncStatement = methodWithoutCommand.GetCurrentNode(startAsyncStatement)!;

        // Insert the command into StartAsync's own enclosing block so that, when StartAsync is nested
        // (for example inside a try block), the command lands immediately after it rather than after
        // the whole nested statement.
        BlockSyntax block = (BlockSyntax)updatedStartAsyncStatement.Parent!;
        int startAsyncIndex = block.Statements.IndexOf(updatedStartAsyncStatement);

        // The moved command keeps its own comments. Its leading trivia is re-indented to match the
        // StartAsync statement it now follows, which matters when StartAsync is nested more deeply than
        // the command's original position (for example inside a try block); copying StartAsync's
        // leading trivia instead would duplicate any comment above StartAsync. A comment on the same
        // line as the command moves with it as well.
        string indentation = CodeFixHelpers.GetIndentation(updatedStartAsyncStatement);
        StatementSyntax commandStatementCopy = trackedCommandStatement
            .WithLeadingTrivia(trackedCommandStatement.GetLeadingTrivia().Select(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) ? SyntaxFactory.Whitespace(indentation) : trivia))
            .WithTrailingTrivia(CodeFixHelpers.GetTrailingTriviaForMove(trackedCommandStatement));
        SyntaxNode newMethod;

        if (startAsyncIndex < block.Statements.Count - 1)
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

    /// <summary>
    /// Finds the statement holding the StartAsync call that the command should be moved after, or
    /// <see langword="null"/> when there is no such statement or the move would not be safe.
    /// </summary>
    /// <param name="method">The block-bodied method containing the command.</param>
    /// <param name="invocation">The invocation the diagnostic was reported on.</param>
    /// <returns>The StartAsync statement to move the command after, or <see langword="null"/>.</returns>
    private static StatementSyntax? FindMoveTargetStatement(MethodDeclarationSyntax method, InvocationExpressionSyntax invocation)
    {
        StatementSyntax commandStatement = invocation.FirstAncestorOrSelf<StatementSyntax>()!;
        string? driverVariableName = CodeFixHelpers.GetRootIdentifierName(invocation.Expression);
        StatementSyntax? startAsyncStatement = method.Body!.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => CodeFixHelpers.IsStartAsyncOn(inv, driverVariableName))
            .Select(inv => inv.FirstAncestorOrSelf<StatementSyntax>()!)
            .FirstOrDefault();

        // The driver may never be started at all, which is the very thing the diagnostic reports.
        // There is then nothing to move the command after.
        if (startAsyncStatement is null)
        {
            return null;
        }

        // The command becomes a statement of the block that holds StartAsync, so that block has to
        // exist. An unbraced embedded statement, as in "if (start) await driver.StartAsync(url);",
        // has a statement rather than a block for its parent.
        if (startAsyncStatement.Parent is not BlockSyntax destinationBlock)
        {
            return null;
        }

        if (!IsUnconditionalDescendantOf(destinationBlock, commandStatement.Parent))
        {
            return null;
        }

        return DeclaredLocalsRemainUsable(method, commandStatement, startAsyncStatement, destinationBlock)
            ? startAsyncStatement
            : null;
    }

    /// <summary>
    /// Gets a value indicating whether a destination block is reached from the block holding the
    /// command exactly once, and without any condition.
    /// </summary>
    /// <param name="destinationBlock">The block the command would be moved into.</param>
    /// <param name="commandBlock">The block that currently holds the command.</param>
    /// <returns><see langword="true"/> if the move is a pure reordering; otherwise, <see langword="false"/>.</returns>
    private static bool IsUnconditionalDescendantOf(BlockSyntax destinationBlock, SyntaxNode? commandBlock)
    {
        // Reordering within one block always preserves execution; so does moving into a nested block
        // that is entered unconditionally and exactly once, such as the body of a try or a using.
        // Descending through an if, a loop, a switch section, or a catch or finally clause would
        // change whether or how often the command runs. Moving the command outward, or sideways into a
        // lambda or a local function, is no safer, and it is rejected here as well: the walk stops
        // when it leaves the ancestors of the command's own block without having found it, and
        // running off the top of the tree ends it too, since null matches none of the shapes below.
        SyntaxNode? current = destinationBlock;
        while (!ReferenceEquals(current, commandBlock))
        {
            if (current is not (BlockSyntax or TryStatementSyntax or UsingStatementSyntax))
            {
                return false;
            }

            current = current.Parent;
        }

        return true;
    }

    /// <summary>
    /// Gets a value indicating whether every local the command statement declares is still usable
    /// after the statement has moved.
    /// </summary>
    /// <param name="method">The block-bodied method containing the command.</param>
    /// <param name="commandStatement">The statement that would be moved.</param>
    /// <param name="startAsyncStatement">The statement the command would be moved after.</param>
    /// <param name="destinationBlock">The block the command would be moved into.</param>
    /// <returns><see langword="true"/> if no use of a declared local is broken; otherwise, <see langword="false"/>.</returns>
    private static bool DeclaredLocalsRemainUsable(
        MethodDeclarationSyntax method,
        StatementSyntax commandStatement,
        StatementSyntax startAsyncStatement,
        BlockSyntax destinationBlock)
    {
        // Moving a statement that declares locals moves the declarations, and with them the scope of
        // the names they introduce. A use that would end up ahead of the new declaration, or outside
        // the block the declaration lands in, no longer compiles, so no fix is offered for it.
        if (commandStatement is not LocalDeclarationStatementSyntax declaration)
        {
            return true;
        }

        foreach (VariableDeclaratorSyntax declarator in declaration.Declaration.Variables)
        {
            string declaredName = declarator.Identifier.ValueText;
            foreach (IdentifierNameSyntax reference in method.Body!.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (reference.Identifier.ValueText != declaredName)
                {
                    continue;
                }

                if (reference.SpanStart < startAsyncStatement.Span.End || !destinationBlock.Span.Contains(reference.Span))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
