// <copyright file="SequentialDocumentFixAllProvider.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Analyzers;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

/// <summary>
/// A "Fix all" provider that applies one fix at a time, each to the document the previous fix produced.
/// </summary>
/// <remarks>
/// <see cref="WellKnownFixAllProviders.BatchFixer"/> computes every fix against the original document and
/// merges the results, dropping changes whose spans intersect. Fixes that rewrite one shared node -- the
/// events argument every BIDI005 diagnostic in a member extends, or the statement BIDI001, BIDI002,
/// BIDI003 and BIDI009 each insert before -- all touch the same span, so batching keeps one and silently
/// discards the rest. Re-running the analyzer after each fix also lets a fix see the code the last one
/// produced, which is what makes adding several events to one subscription come out right.
/// </remarks>
internal sealed class SequentialDocumentFixAllProvider : FixAllProvider
{
    private readonly CodeFixProvider codeFixProvider;

    private SequentialDocumentFixAllProvider(CodeFixProvider codeFixProvider)
    {
        this.codeFixProvider = codeFixProvider;
    }

    /// <summary>
    /// Creates a provider that applies the given code fix's actions one after another.
    /// </summary>
    /// <param name="codeFixProvider">The code fix whose actions to apply.</param>
    /// <returns>The fix-all provider.</returns>
    public static FixAllProvider Create(CodeFixProvider codeFixProvider)
    {
        return new SequentialDocumentFixAllProvider(codeFixProvider);
    }

    /// <inheritdoc/>
    public override Task<CodeAction?> GetFixAsync(FixAllContext fixAllContext)
    {
        // Document scope carries the document; the wider scopes enumerate the projects they cover. The
        // host never asks for a scope this provider did not declare support for.
        List<Document> documents = fixAllContext.Scope == FixAllScope.Document
            ? new List<Document> { fixAllContext.Document! }
            : fixAllContext.Solution.Projects
                .Where(project => fixAllContext.Scope == FixAllScope.Solution || project.Id == fixAllContext.Project.Id)
                .SelectMany(project => project.Documents)
                .ToList();

        CodeAction fixAll = CodeAction.Create(
            "Fix all occurrences",
            cancellationToken => this.FixDocumentsAsync(fixAllContext, documents, cancellationToken));
        return Task.FromResult<CodeAction?>(fixAll);
    }

    private async Task<Solution> FixDocumentsAsync(FixAllContext fixAllContext, List<Document> documents, CancellationToken cancellationToken)
    {
        Solution solution = fixAllContext.Solution;
        foreach (Document document in documents)
        {
            Document current = solution.GetDocument(document.Id)!;
            foreach (Diagnostic diagnostic in await fixAllContext.GetDocumentDiagnosticsAsync(document).ConfigureAwait(false))
            {
                // Each fix is computed against the document the previous one produced, so two fixes
                // that touch the same node compose instead of colliding. Each of the providers this
                // serves registers one action per diagnostic, and a provider that declines to fix a
                // diagnostic registers none, which leaves the document as it was.
                List<CodeAction> actions = new();
                CodeFixContext context = new(current, diagnostic, (action, _) => actions.Add(action), cancellationToken);
                await this.codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

                foreach (CodeAction action in actions.Take(1))
                {
                    ImmutableArray<CodeActionOperation> operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
                    foreach (ApplyChangesOperation operation in operations.OfType<ApplyChangesOperation>().Take(1))
                    {
                        solution = operation.ChangedSolution;
                        current = solution.GetDocument(document.Id)!;
                    }
                }
            }
        }

        return solution;
    }
}
